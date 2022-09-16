using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.pi_player.Scheduling.Jobs;
using adworks.pi_player.Services;
using Quartz;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Scheduling
{
    public class PlaylistScheduler: IPlaylistScheduler
    {
        private readonly ILogger _logger;
        private readonly ISchedulerService _schedulerService;


        public PlaylistScheduler(ILogger logger, ISchedulerService schedulerService)
        {
            _logger = logger;
            _schedulerService = schedulerService;
        }

        public async Task SchedulePlaylist(PlaylistDto playlistDto)
        {
            _logger.Information($"Schedule playlist into Quarz, {playlistDto.Name}");
            try
            {
                await _schedulerService.EnsureStarted();

                var jobName = playlistDto.Id.ToString();
                var job = await _schedulerService.GetJobDetail(jobName, Constants.PlaylistGroupName);
                var trigger = await _schedulerService.GetTrigger(jobName, Constants.PlaylistGroupName);

                //clean up existing jobs and triggers and rebuild
                if (job != null || trigger != null)
                {
                    await _schedulerService.CleanupJobs(jobName, Constants.PlaylistGroupName);
                }

                IDictionary dict = new Dictionary<string, object> {{jobName, playlistDto}};

                // define the job and tie it to our HelloJob class
                job = JobBuilder.Create<ExecutePlaylistJob>()
                    .WithIdentity(jobName, Constants.PlaylistGroupName)
                    .UsingJobData(new JobDataMap(dict))
                    .StoreDurably(false)
                    .Build();

                // Trigger the job to run now, and then repeat every 60 seconds
                trigger = TriggerBuilder.Create()
                    .WithIdentity(jobName, Constants.PlaylistGroupName)
                    .StartAt(playlistDto.StartDate)
                    .EndAt(playlistDto.EndDate)
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(1)
                        .RepeatForever())
                    .ForJob(job)
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await _schedulerService.ScheduleJob(job, trigger);

                // some sleep to show what's happening
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            catch (SchedulerException se)
            {
                _logger.Error(se, "Schedule playlist failed");
            }
        }

        public async Task UnschedulePlaylist(PlaylistDto playlistDto)
        {
            try
            {
                await _schedulerService.EnsureStarted();

                var jobName = playlistDto.Id.ToString();
                var job = await _schedulerService.GetJobDetail(jobName, Constants.PlaylistGroupName);
                var trigger = await _schedulerService.GetTrigger(jobName, Constants.PlaylistGroupName);

                //clean up existing jobs and triggers and rebuild
                if (job != null && trigger != null)
                {
                    await _schedulerService.CleanupJobs(jobName, Constants.PlaylistGroupName);
                }
            }
            catch (SchedulerException se)
            {
                _logger.Error(se, "Unschedule playlist failed");
            }
        }

        public async Task Pause()
        {
            await _schedulerService.Pause(Constants.PlaylistGroupName);
        }

        public async Task Resume()
        {
            await _schedulerService.Resume(Constants.PlaylistGroupName);
        }
    }
}
