using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.media_common;

namespace adworks.pi_player
{
    public interface IDataService
    {
        Task Init();
        
        Task<IEnumerable<Record>> GetRecords();
        Task<Record> AddRecord(Record record);
        Task RemoveRecord(Guid id);
        
        //playlist
        PlaylistDto GetPlaylist(Guid id);
        void RemovePlaylist(Guid id);
        Task SavePlaylist(PlaylistDto newPlaylist);
        
        //device
        Task<DeviceDto> GetDevice();
        void RemoveDevice(Guid id);
        Task SaveDevice(DeviceDto deviceDto);
        Task RemoveAll();
    }
}