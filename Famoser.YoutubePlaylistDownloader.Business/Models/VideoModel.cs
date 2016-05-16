namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    public class VideoModel : BaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }

        public string Link
        {
            get { return "https://www.youtube.com/watch?v=" + Id; }
        }
    }
}
