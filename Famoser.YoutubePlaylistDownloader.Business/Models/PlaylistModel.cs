namespace YoutubePlaylistDownloader.Business.Models
{
    public class PlaylistModel : BaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        private bool _download;
        public bool Download
        {
            get { return _download; }
            set { Set(ref _download, value); }
        }


        public int TotalVideos { get; set; }
    }
}
