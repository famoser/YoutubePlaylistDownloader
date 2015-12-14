namespace YoutubePlaylistDownloader.Business.Models
{
    public class PlaylistModel : BaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public bool Download { get; set; }

        public int TotalVideos { get; set; }
    }
}
