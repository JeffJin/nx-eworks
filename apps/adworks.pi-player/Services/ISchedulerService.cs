using System;
using System.Threading.Tasks;
using adworks.media_common;
using Quartz;

namespace adworks.pi_player.Services
{
    public interface ISchedulerService
    {
        Task<IJobDetail> GetJobDetail(string name, string group);

        Task<ITrigger> GetTrigger(string name, string group);

        Task<bool> CleanupJobs(string name, string group);

        Task SetPlaylistCache(PlaylistDto newPlaylist);
        
        Task UpdatePlaylistCache(PlaylistDto oldPlaylist, PlaylistDto newPlaylist);

        Task Resume(string group);

        Task Pause(string group);
        
        Task EnsureStarted();
        
        Task<DateTimeOffset> ScheduleJob(IJobDetail job, ITrigger trigger);
        void ScheduleLiceseCheck();
    }
}