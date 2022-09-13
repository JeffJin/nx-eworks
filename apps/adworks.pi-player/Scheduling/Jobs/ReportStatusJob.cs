using System;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.pi_player.Exceptions;
using adworks.pi_player.Services;
using Quartz;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Scheduling.Jobs
{
    [DisallowConcurrentExecution()]
    public class ReportStatusJob : IJob
    {
        private ILogger _logger;
        private readonly ISystemService _systemService;

        public ReportStatusJob(ILogger logger, ISystemService systemService)
        {
            _logger = logger;
            _systemService = systemService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;

            string serialNumber = key.Name;

            _logger.Information("Report status " + serialNumber);
            try
            {
                DeviceDto dto = await _systemService.GetDeviceInfo(serialNumber);
                await _systemService.ReportStatus(dto);
            }
            catch (StatusReportException ext)
            {
                _logger.Error(ext, "web service to report status failed, serial number: {serialNumber}", serialNumber);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to report status for device with serial number: {serialNumber}", serialNumber);
            }

        }
    }
}
