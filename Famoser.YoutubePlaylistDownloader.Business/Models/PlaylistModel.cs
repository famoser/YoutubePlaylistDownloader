using System.Collections.ObjectModel;

namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    
    public class PlaylistModel : YoutubeModel
    {
        public PlaylistModel()
        {
            Videos = new ObservableCollection<VideoModel>();
        }
        public override string Link => "https://www.youtube.com/playlist?list=" + Id;

        private bool _refresh;
        public bool Refresh
        {
            get { return _refresh; }
            set { Set(ref _refresh, value); }
        }

        private ObservableCollection<VideoModel> _videos;
        public ObservableCollection<VideoModel> Videos
        {
            get { return _videos; }
            set { Set(ref _videos, value); }
        }
    }
}
