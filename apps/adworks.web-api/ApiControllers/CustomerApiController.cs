using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/organizations")]
    public class OrganizationApiController : BaseApiController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationApiController(IOrganizationService organizationService, IConfiguration configuration, IUserService userService,
            ILogger logger): base(configuration, userService, logger)
        {
            _organizationService = organizationService;
        }

        // GET list
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<OrganizationDto>> Get()
        {
            IEnumerable<Organization> models = await _organizationService.FindAll();
            return models.Select(m => DtoHelper.Convert(m));
        }

        // GET details/id
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<OrganizationDto> GetDetails(Guid id)
        {
            var model = await _organizationService.FindOrganization(id);

            return DtoHelper.Convert(model);
        }

        // POST create
        [HttpPost]
        public async Task<Guid> Create([FromBody] OrganizationDto dto)
        {
            dto.CreatedBy = GetCurrentEmail();
            return await _organizationService.AddOrganization(dto);
        }

        // PUT update/id
        [HttpPut("{id}")]
        public async Task<OrganizationDto> Put([FromBody] OrganizationDto dto)
        {
            try
            {
                dto.UpdatedBy = GetCurrentEmail();
                var result = await _organizationService.UpdateOrganization(dto);
                return DtoHelper.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update organization", ex);
                throw;
            }
        }

        // DELETE delete/id
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                bool result = _organizationService.DeleteOrganization(id);
                if (!result)
                {
                    return NotFound(null);
                }
                else
                {
                    return Ok(new DummyResult());
                }
            }
            catch (Exception e)
            {
                _logger.Error("Failed to delete organization with id " + id, e);
                throw;
            }
        }
    }
}
