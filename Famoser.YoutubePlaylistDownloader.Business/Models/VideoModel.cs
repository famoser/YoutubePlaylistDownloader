namespace YoutubePlaylistDownloader.Business.Models
{
    public class VideoModel : BaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        private bool _isDownloaded;
        public bool IsDownloaded
        {
            get { return _isDownloaded; }
            set { Set(ref _isDownloaded, value); }
        }

        public string Link
        {
            get { return "https://www.youtube.com/watch?v=" + Id; }
        }
    }
}
