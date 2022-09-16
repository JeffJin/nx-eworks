using System;
using System.Collections.Generic;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.scheduler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/schedules")]
    public class ScheduleApiController : BaseApiController
    {
        private readonly IScheduleService _scheduleService;
        public ScheduleApiController(IScheduleService scheduleService, IConfiguration configuration,
            IUserService userService, ILogger logger): base(configuration, userService, logger)
        {
            _scheduleService = scheduleService;
        }

        // POST schedule
        [HttpGet("list")]
        public IEnumerable<Appointment> ListAppointment()
        {
            return _scheduleService.GetAppointments(DateTimeOffset.Now, DateTimeOffset.MaxValue);
        }
        // POST schedule/appointment
        [HttpPost("appointment")]
        public void MakeAppointment([FromBody] Appointment appointment)
        {
            _scheduleService.AddAppointment(appointment);
        }
        // PUT schedule/appointment
        [HttpPut("appointment")]
        public void UpdateAppointment(Appointment appointment)
        {
            this._scheduleService.UpdateAppointment(appointment);
        }
        // PUT schedule/appointment
        [HttpDelete("appointment")]
        public void DeleteAppointment(Guid id)
        {
            this._scheduleService.DeleteAppointment(id);
        }

    }
}
