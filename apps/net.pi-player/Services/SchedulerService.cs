using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using adworks.media_common;
using Quartz;
using Quartz.Impl.Matchers;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Services
{
    public class SchedulerService: ISchedulerService
    {
        private readonly IScheduler _scheduler;
        private readonly IShellService _shellService;
        private readonly ILogger _logger;
        private readonly ICachingService _cachingService;

        public SchedulerService(IScheduler scheduler, IShellService shellService, ILogger logger, ICachingService cachingService)
        {
            _scheduler = scheduler;
            _shellService = shellService;
            _logger = logger;
            _cachingService = cachingService;
        }

        public async Task<IJobDetail> GetJobDetail(string name, string group)
        {
            JobKey key = JobKey.Create(name, group);
            if (await _scheduler.CheckExists(key))
            {
                return await _scheduler.GetJobDetail(key);
            }

            return null;
        }

        public async Task<ITrigger> GetTrigger(string name, string group)
        {
            var keys = await _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(group));
            if (keys.Count == 1)
            {
                return await _scheduler.GetTrigger(keys.Single());
            }

            return null;
        }

        public async Task<bool> CleanupJobs(string name, string group)
        {
            bool result = false;
            try
            {
                await Pause(group);
                var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(name));
                var jobResult = await _scheduler.DeleteJobs(jobKeys);

                var triggerKeys = await _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(group));
                var triggerResult = await _scheduler.UnscheduleJobs(triggerKeys);
                result = jobResult & triggerResult;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Clean up existing jobs failed, {name}, {group}", name, group);
            }
            finally
            {
                await Resume(group);
            }

            return result;
        }

        public async Task Pause(string group)
        {
            var keys = GroupMatcher<TriggerKey>.GroupEquals(group);
            await _scheduler.PauseTriggers(keys);
        }

        public async Task EnsureStarted()
        {
            if (!_scheduler.IsStarted)
            {
                await _scheduler.Start();
            }
        }

        public async Task<DateTimeOffset> ScheduleJob(IJobDetail job, ITrigger trigger)
        {
            var time = await _scheduler.ScheduleJob(job, trigger);

            return time;
        }

        public void ScheduleLiceseCheck()
        {
            //1. get license from database
            //2. get license from backend
            //3. create job to validate license
            //
        }

        public async Task Resume(string group)
        {
            var keys = GroupMatcher<TriggerKey>.GroupEquals(group);
            await _scheduler.ResumeTriggers(keys);
        }

        public async Task UpdatePlaylistCache(PlaylistDto oldPlaylist, PlaylistDto newPlaylist)
        {
            _logger.Information($"Update playlist cache {oldPlaylist.Name}");

            //stop omx player and delete cached files
            _shellService.StopVideoPlayer();

            var oldPlaylistItems = oldPlaylist.SubPlaylists.Select(s => s.PlaylistItems)
                .Aggregate(new List<PlaylistItemDto>(), (acc, x) => {
                    acc.AddRange(x);
                    return acc;
                });

            var newPlaylistItems = newPlaylist.SubPlaylists.Select(s => s.PlaylistItems)
                .Aggregate(new List<PlaylistItemDto>(), (acc, x) => {
                    acc.AddRange(x);
                    return acc;
                });
            foreach (var newItem in newPlaylistItems)
            {
                var oldItem = oldPlaylistItems.SingleOrDefault(pi => pi.Id == newItem.Id);
                if (oldItem != null)
                {
                    //update cache if the item is updated
                    await UpdatePlaylistItemCache(oldItem, newItem);
                }
                else
                {
                    newItem.CacheLocation = await _cachingService.DownloadAndSave(newItem);
                }
            }

            //remove old items that are not included in new playlist
            var oldItemsToBeDeleted =
                oldPlaylistItems.Where(opi => newPlaylistItems.Any(npi => npi.Id == opi.Id));
            foreach (var itemToDelete in oldItemsToBeDeleted)
            {
                File.Delete(itemToDelete.CacheLocation);
            }
        }

        public async Task SetPlaylistCache(PlaylistDto newPlaylist)
        {
            _logger.Information($"Add playlist cache {newPlaylist.Name}");

            //stop omx player and delete cached files
            _shellService.StopVideoPlayer();

            foreach (var sub in newPlaylist.SubPlaylists)
            {
                foreach (var newItem in sub.PlaylistItems)
                {
                    newItem.CacheLocation = await _cachingService.DownloadAndSave(newItem);
                }
            }
        }

        private async Task UpdatePlaylistItemCache(PlaylistItemDto oldPlaylistItem, PlaylistItemDto newPlaylistItem)
        {
            if (newPlaylistItem.UpdatedOn > oldPlaylistItem.UpdatedOn)
            {
                File.Delete(oldPlaylistItem.CacheLocation);
                var filePath = await _cachingService.DownloadAndSave(newPlaylistItem);
                newPlaylistItem.CacheLocation = filePath;
            }
            else
            {
                newPlaylistItem.CacheLocation = oldPlaylistItem.CacheLocation;
            }
        }
    }
}
