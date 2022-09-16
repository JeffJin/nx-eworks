using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    [Route("api/locations")]
    public class LocationApiController : BaseApiController
    {
        private readonly ILocationService _locationService;

        public LocationApiController(ILocationService locationService, IConfiguration configuration, IUserService userService,
            ILogger logger): base(configuration, userService, logger)
        {
            _locationService = locationService;
        }

        // GET list
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<LocationDto>> Get()
        {
            IEnumerable<Location> models = await _locationService.FindAll();
            return models.Select(m => DtoHelper.Convert(m));
        }

        // GET details/id
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<LocationDto> GetDetails(Guid id)
        {
            var model = await _locationService.FindLocation(id);

            return DtoHelper.Convert(model);
        }

        // POST create
        [HttpPost]
        public async Task<Guid> Create([FromBody]LocationDto dto)
        {
            dto.CreatedBy = GetCurrentEmail();
            return await _locationService.AddLocation(dto);
        }

        // PUT update/id
        [HttpPut("{id}")]
        public async Task<LocationDto> Put([FromBody] LocationDto dto)
        {
            var claim= TryGetClaim(HttpContext.User , ClaimTypes.Email);
            string email = claim.Value;
            try
            {
                dto.UpdatedBy = (email);
                var result = await _locationService.UpdateLocation(dto);
                return DtoHelper.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update location", ex);
                throw;
            }
        }

        // DELETE delete/id
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                bool result = _locationService.DeleteLocation(id);
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
                _logger.Error("Failed to delete location with id " + id, e);
                throw;
            }
        }
    }
}
