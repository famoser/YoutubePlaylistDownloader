using System.Collections.Generic;

namespace Famoser.YoutubePlaylistDownloader.Business.Models.Save
{
    public class CacheModel
    {
        public CacheModel()
        {
            CachedPlaylists = new List<PlaylistCacheModel>();
        }
        public List<PlaylistCacheModel> CachedPlaylists { get; set; } 
    }
}
