using System;
using System.Threading;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.message_bus;
using adworks.message_common;
using adworks.pi_player.Scheduling;
using adworks.pi_player.Services;
using Serilog;
using ILogger = Serilog.ILogger;
using ISubscription = adworks.message_bus.ISubscription;

namespace adworks.pi_player.Processors
{
    public class PlaylistProcessor : IPlaylistProcessor
    {
        private readonly IMessageClient _messageClient;
        private readonly ICachingService _cachingService;
        private readonly IDataService _dataService;
        private readonly ISystemService _systemService;
        private readonly IReportStatusScheduler _reportStatusScheduler;
        private readonly ISchedulerService _schedulerService;
        private readonly IPlaylistScheduler _playlistScheduler;
        private static ILogger _logger;

        public PlaylistProcessor(ILogger logger, IMessageClient messageClient, ICachingService cachingService,
            IDataService dataService,
            ISystemService systemService, IReportStatusScheduler reportStatusScheduler,
            ISchedulerService schedulerService,
            IPlaylistScheduler playlistScheduler)
        {
            _logger = logger;
            _messageClient = messageClient;
            _cachingService = cachingService;
            _dataService = dataService;
            _systemService = systemService;
            _reportStatusScheduler = reportStatusScheduler;
            _schedulerService = schedulerService;
            _playlistScheduler = playlistScheduler;
        }

        public async Task<ISubscription> Run()
        {
            _logger.Information("Playlist processor running on thread {threadId}",
                Thread.CurrentThread.ManagedThreadId);

            DeviceDto device = await InitConnection();

            while(device == null)
            {
                int num = new Random().Next(3, 10);
                _logger.Information("Device provision not successful, retreying in {num} seconds", num);
                Thread.Sleep(num * 1000);
                device = await InitConnection();
            }

            //schedule heartbeat
            _logger.Information("Start scheduling status report");
            await _reportStatusScheduler.ScheduleStatusReport(device.SerialNumber);

            _schedulerService.ScheduleLiceseCheck();

            var deviceRoutingKey = string.Format("{0}.{1}.{2}", device.OrganizationName, device.DeviceGroupName,
                device.SerialNumber);
            var groupRoutingKey = string.Format("{0}.{1}", device.OrganizationName, device.DeviceGroupName);
            string[] keys = {deviceRoutingKey, groupRoutingKey};
            var queueName = $"{device.OrganizationName}.{device.DeviceGroupName}.{device.SerialNumber}";

            _logger.Information("Playlist processor start linstening on DeviceExchange, {queueName}, {keys}", queueName, keys);

            return _messageClient.Subscribe(MessageExchanges.DeviceExchange, queueName, keys, ProcessPlaylist);
        }

        private async Task<DeviceDto> InitConnection()
        {
            DeviceDto device = await _dataService.GetDevice();
            // if the previous provision is valid, return the saved device
            // TODO Warning, security vulnerable here, check if license is expired
            if (device?.ActivatedOn != null)
            {
                return device;
            }

            string osType = _systemService.GetOsType();
            _logger.Information("Player OS Type is {osType}", osType);

            string serialNumber = _systemService.GetSerialNumber(osType);
            _logger.Information("Player CPU serial number is {serialNumber}", serialNumber);

            device = await _systemService.RequestProvision(serialNumber);
            if (device != null && device.ActivatedOn != null)
            {
                await _dataService.RemoveAll();
                await _dataService.SaveDevice(device);
            }

            return device;
        }

        private async Task RemovePlaylist(Message message)
        {
            var playlistToRemove = SerializeHelper.Deserialize<PlaylistDto>(message.Body);
            var savedPlaylist = _dataService.GetPlaylist(playlistToRemove.Id);

            // Delete from database
            _dataService.RemovePlaylist(playlistToRemove.Id);

            // Stop the playlist by unscheduling
            await _playlistScheduler.UnschedulePlaylist(playlistToRemove);

            // Remove caching
            _cachingService.RemoveCache(savedPlaylist);
            await Task.CompletedTask;
        }

        private async Task SchedulePlaylist(Message message)
        {
            var newPlaylist = SerializeHelper.Deserialize<PlaylistDto>(message.Body);

            //if new playlist is received, old one will be removed along with all related local cached files if size is limited
            _logger.Information("Received Playlist {playlistName}, Start processing", newPlaylist.Name);

            //Find playlist in database
            PlaylistDto savedPlaylist = _dataService.GetPlaylist(newPlaylist.Id);

            if (savedPlaylist != null)
            {
                //download or update cache
                await _schedulerService.UpdatePlaylistCache(savedPlaylist, newPlaylist);
            }
            else
            {
                await _schedulerService.SetPlaylistCache(newPlaylist);
            }

            //Update or save database record
            await _dataService.SavePlaylist(newPlaylist);

            //schedule playlist
            await _playlistScheduler.SchedulePlaylist(newPlaylist);
        }

        private async Task ProcessPlaylist(Message message)
        {
            switch (message.Topic)
            {
                    case MessageTopics.PiPlaylistPublished:
                        await SchedulePlaylist(message);
                        break;

                    case MessageTopics.PiPlaylistUnscheduled:
                        await RemovePlaylist(message);
                        break;

                    default:
                        await Task.CompletedTask;
                        break;
            }

        }
    }
}
