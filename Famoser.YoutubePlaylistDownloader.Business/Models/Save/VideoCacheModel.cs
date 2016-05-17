using Famoser.YoutubePlaylistDownloader.Business.Enums;

namespace Famoser.YoutubePlaylistDownloader.Business.Models.Save
{
    public class VideoCacheModel : YoutubeCacheModel
    {
        public Mp3CacheModel Mp3Model;
        public SaveStatus SaveStatus;
    }
}
