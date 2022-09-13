using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;
using adworks.media_web_api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/groups")]
    public class GroupApiController : BaseApiController
    {
        private readonly IDeviceGroupService _grouprService;

        public GroupApiController(IDeviceGroupService groupService, IConfiguration configuration, IUserService userService,
            ILogger logger) : base(configuration, userService, logger)
        {
            _grouprService = groupService;
        }

        // GET
        [HttpGet]
        public async Task<IEnumerable<DeviceGroupDto>> Get()
        {
            IEnumerable<DeviceGroupDto> dtos = await _grouprService.FindAll();
            return dtos;
        }

        // GET id
        [HttpGet("{id}")]
        public async Task<DeviceGroupDto> GetDetails(Guid id)
        {
            var model = await _grouprService.FindGroup(id);

            return DtoHelper.Convert(model);
        }

        // GET groups with device details
        [HttpGet("devices/{num}")]
        public async Task<IEnumerable<DeviceGroupDto>> GetGroupsWithDevices(int num)
        {
            var dtos = await _grouprService.GetGroupsWithDevices(num);
            return dtos;
        }

        // POST
        [HttpPost]
        public async Task<Guid> Create([FromBody] DeviceGroupDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name.Trim()))
            {
                throw new InvalidRequstException("Group name cannot be null or empty");
            }

            dto.CreatedBy = GetCurrentEmail();
            dto.Id = Guid.NewGuid();
            dto.Name = dto.Name.Trim();

            return await _grouprService.AddGroup(dto);
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<DeviceGroupDto> Put([FromBody] DeviceGroupDto dto)
        {
            dto.UpdatedBy = GetCurrentEmail();
            var result = await _grouprService.UpdateDeviceGroup(dto);
            return DtoHelper.Convert(result);
        }

        // DELETE id
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            bool result = _grouprService.DeleteDeviceGroup(id);
            if (!result)
            {
                return NotFound(null);
            }
            else
            {
                return Ok(new DummyResult());
            }
        }
    }
}
