using System;
using System.Threading.Tasks;
using adworks.pi_player.Scheduling.Jobs;
using adworks.pi_player.Services;
using Microsoft.Extensions.Configuration;
using Quartz;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Scheduling
{
    public class ReportStatusScheduler: IReportStatusScheduler
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly ISchedulerService _schedulerService;

        public ReportStatusScheduler(ILogger logger, IConfiguration config, ISchedulerService schedulerService)
        {
            _logger = logger;
            _config = config;
            _schedulerService = schedulerService;
        }

        public async Task ScheduleStatusReport(string serialNumber)
        {
            try
            {
                int interval = Convert.ToInt32(_config["HeartBeatInterval"]);

                await _schedulerService.EnsureStarted();

                var job = await _schedulerService.GetJobDetail(serialNumber, Constants.StatusReportGroupName);
                var trigger = await _schedulerService.GetTrigger(serialNumber, Constants.StatusReportGroupName);

                if (job == null || trigger == null)
                {
                    _logger.Information("Scheduling heartbeat at interval {interval}", interval);

                    //clean up existing jobs and triggers and rebuild
                    await _schedulerService.CleanupJobs(serialNumber, Constants.StatusReportGroupName);

                    // define the job and tie it to our HelloJob class
                    job = JobBuilder.Create<ReportStatusJob>()
                        .WithIdentity(serialNumber, Constants.StatusReportGroupName)
                        .StoreDurably(false)
                        .Build();

                    // Trigger the job to run now, and then repeat every interval seconds
                    trigger = TriggerBuilder.Create()
                        .WithIdentity(serialNumber, Constants.StatusReportGroupName)
                        .WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(interval)
                            .RepeatForever())
                        .ForJob(job)
                        .Build();

                    // Tell quartz to schedule the job using our trigger
                    await _schedulerService.ScheduleJob(job, trigger);
                }

                // some sleep to show what's happening
                _logger.Information("Sleeping for 3 seconds to see what's happening");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            catch (SchedulerException se)
            {
                _logger.Error(se, "Schedule status report failed");
            }
        }

        public async Task Pause()
        {
            await _schedulerService.Pause(Constants.StatusReportGroupName);
        }

        public async Task Resume()
        {
            await _schedulerService.Resume(Constants.StatusReportGroupName);
        }
    }

}
