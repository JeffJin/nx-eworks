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
    [Route("api/devices")]
    public class DeviceApiController : BaseApiController
    {
        private readonly IDeviceService _deviceService;
        private readonly ILicenseService _licenseService;

        public DeviceApiController(IDeviceService deviceService, ILicenseService licenseService, IConfiguration configuration,
            IUserService userService, ILogger logger) : base(configuration, userService, logger)
        {
            _licenseService = licenseService;
            _deviceService = deviceService;
        }

        // GET list
        [HttpGet]
        public async Task<IEnumerable<DeviceDto>> Get()
        {
            IEnumerable<Device> models = await _deviceService.FindAll();
            return models.Select(m => DtoHelper.Convert(m));
        }

        // GET by serial number
        [HttpGet("{serialNumber}")]
        [AllowAnonymous]
        public async Task<DeviceDto> GetDetails(string serialNumber)
        {
            var model = await _deviceService.FindDeviceBySerialNumber(serialNumber);
            return DtoHelper.Convert(model);
        }

        // GET by serial number
        [HttpGet("request/{serialNumber}")]
        [AllowAnonymous]
        public async Task<DeviceDto> RequestProvision(string serialNumber)
        {
            var model = await _deviceService.FindDeviceBySerialNumber(serialNumber);
            if (model == null)
            {
                return null;
            }

            var dto = DtoHelper.Convert(model);
            foreach (var license in model.Licenses)
            {
                dto.Licenses.Add(DtoHelper.Convert(license));
            }

            //TODO validate licenses

            return dto;
        }

        // POST
        [HttpPost]
        public async Task<Guid> Create([FromBody] DeviceDto dto)
        {
            dto.CreatedBy = GetCurrentEmail();

            if (dto.Licenses.Count > 0)
            {
                IList<License> licenses = await _licenseService.FindLicenses(dto.Licenses.Select(l => l.Id));
                if (licenses == null || licenses.Count == 0 || licenses.All(l => l.ExpireOn < DateTimeOffset.UtcNow))
                {
                    dto.Licenses.Add(DtoHelper.Convert(_licenseService.CreateTrialLicense()));
                }
            }
            else
            {
                dto.Licenses.Add(DtoHelper.Convert(_licenseService.CreateTrialLicense()));
            }

            return await _deviceService.RegisterDevice(dto);
        }

        // PUT id
        [HttpPut("{id}")]
        public async Task<DeviceDto> Put([FromBody] DeviceDto dto)
        {
            try
            {
                dto.UpdatedBy = GetCurrentEmail();
                var result = await _deviceService.UpdateDevice(dto);
                return DtoHelper.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to update device");
                throw;
            }
        }

        // DELETE delete/id
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                bool result = _deviceService.DeleteDevice(id);
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
                _logger.Error(e, "Failed to delete device with id " + id);
                throw;
            }
        }


        // GET device status
        [HttpGet("status/{serialNumber}")]
        [AllowAnonymous]
        public async Task<DeviceStatusDto> GetDeviceStatus(string serialNumber)
        {
            var model = await _deviceService.GetDeviceStatus(serialNumber);
            return DtoHelper.Convert(model);
        }


        // GET  id
        [HttpPost("status/")]
        public async Task<DeviceStatusDto> SetDeviceStatus([FromBody] DeviceStatusDto dto)
        {
            var licenseResult = await _deviceService.CheckLicense(dto.SerialNumber);
            if (!licenseResult)
            {
                throw new InvalidLicenseException(dto.SerialNumber);
            }

            var model = await _deviceService.AddOrUpdateDeviceStatus(dto);
            return DtoHelper.Convert(model);
        }

    }
}
