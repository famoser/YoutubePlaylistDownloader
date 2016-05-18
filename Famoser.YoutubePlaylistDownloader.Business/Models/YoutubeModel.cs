namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    public abstract class YoutubeModel : ProgressAwareModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public abstract string Link { get; }
    }
}
