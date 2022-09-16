using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;

namespace adworks.data_services
{
    public interface IPlaylistService
    {
        Task<Guid> AddPlaylist(PlaylistDto playlist);

        /// <summary>
        /// this call will cascade and all associated playlist items will be removed as well.        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeletePlaylist(Guid id);

        Task<Playlist> FindPlaylist(Guid id);
        Task<PlaylistItem> FindPlaylistItem(Guid id);

        Task<IEnumerable<Playlist>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn", bool isDescending = true);

        Task<Playlist> UpdatePlaylist(PlaylistDto playlistDto);

        Task<Guid> AddPlaylistItem(PlaylistItemDto item);
        MediaAsset FindAsset(Guid assetId, string assetType);
        
        Task<Playlist> RemovePlaylistItem(RemovePlaylistItemDto dto);
        Task<PlaylistItem> UpdatePlaylistItem(PlaylistItemDto item);

        Task<Playlist> Validate(Guid id);
        Task<Guid> AddSubPlaylist(SubPlaylistDto subDto);
    }
}