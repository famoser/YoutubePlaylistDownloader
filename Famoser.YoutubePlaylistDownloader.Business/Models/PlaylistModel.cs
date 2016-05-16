using System.Collections.Generic;
using System.Collections.ObjectModel;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;

namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    public class PlaylistModel : BaseModel
    {
        public PlaylistModel()
        {
            NewFiles = new ObservableCollection<Mp3Model>();
        }

        public string Id { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        private bool _download;
        public bool Download
        {
            get { return _download; }
            set { Set(ref _download, value); }
        }

        private int _totalVideos;
        public int TotalVideos
        {
            get { return _totalVideos; }
            set { Set(ref _totalVideos, value); }
        }

        private ObservableCollection<Mp3Model> _newFiles;
        public ObservableCollection<Mp3Model> NewFiles
        {
            get { return _newFiles; }
            set { Set(ref _newFiles, value); }
        }

        private List<VideoModel> _downloadedVideos;
        public List<VideoModel> DownloadedVideos
        {
            get { return _downloadedVideos; }
            set { Set(ref _downloadedVideos, value); }
        }

        private List<VideoModel> _failedVideos;
        public List<VideoModel> FailedVideos
        {
            get { return _failedVideos; }
            set { Set(ref _failedVideos, value); }
        }
    }
}
