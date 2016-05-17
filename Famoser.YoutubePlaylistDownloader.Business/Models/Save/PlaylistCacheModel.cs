using System.Collections.Generic;

namespace Famoser.YoutubePlaylistDownloader.Business.Models.Save
{
    public class PlaylistCacheModel : YoutubeCacheModel
    {
        public bool Refresh;
        public List<VideoCacheModel> VideoCacheModels;
    }
}
