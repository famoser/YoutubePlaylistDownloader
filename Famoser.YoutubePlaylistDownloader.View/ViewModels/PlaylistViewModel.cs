using System.Windows.Input;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.FrameworkEssentials.View.Commands;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.View.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Famoser.YoutubePlaylistDownloader.View.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IHistoryNavigationService _historyNavigationService;
        private readonly IProgressService _progressService;

        public PlaylistViewModel(IPlaylistRepository playlistRepository, IHistoryNavigationService historyNavigationService, IProgressService progressService)
        {
            _playlistRepository = playlistRepository;
            _historyNavigationService = historyNavigationService;
            _progressService = progressService;

            _startDownload = new RelayCommand(StartDownload, () => CanExecuteStartDownloadCommand);
            _refreshPlaylist = new RelayCommand(RefreshPlaylist, () => CanExecuteRefreshPlaylistCommand);
            _selectVideo = new RelayCommand<VideoModel>(SelectVideo);

            Messenger.Default.Register<PlaylistModel>(this, Messages.Select, EvaluateSelectMessage);

            if (IsInDesignMode)
            {
                SelectedPlaylist = _playlistRepository.GetPlaylists()[0];
            }
        }

        private void EvaluateSelectMessage(PlaylistModel obj)
        {
            SelectedPlaylist = obj;
            RaisePropertyChanged(() => ProgressService);
        }

        public ProgressService ProgressService => SelectedPlaylist.ProgressService;

        private readonly RelayCommand _refreshPlaylist;
        public ICommand RefreshPlaylistCommand => _refreshPlaylist;

        public bool CanExecuteRefreshPlaylistCommand => !_refreshPlaylistActive;

        private bool _refreshPlaylistActive;
        public async void RefreshPlaylist()
        {
            using (new IndeterminateProgressDisposable<IndeterminateProgressKey>(_refreshPlaylist, b => _refreshPlaylistActive = b, IndeterminateProgressKey.RefreshingPlaylists, _progressService))
            {
                SelectedPlaylist.ProgressService = new ProgressService();
                await _playlistRepository.RefreshPlaylist(SelectedPlaylist, SelectedPlaylist.ProgressService);
            }
        }

        private readonly RelayCommand _startDownload;
        public ICommand StartDownloadCommand => _startDownload;

        public bool CanExecuteStartDownloadCommand => SelectedPlaylist != null && !_startDownloadActive;

        private bool _startDownloadActive;
        public async void StartDownload()
        {
            using (new IndeterminateProgressDisposable<IndeterminateProgressKey>(_startDownload, b => _startDownloadActive = b, IndeterminateProgressKey.StartingDownload, _progressService))
            {
                SelectedPlaylist.ProgressService = new ProgressService();
                await _playlistRepository.DownloadVideosForPlaylist(SelectedPlaylist, SelectedPlaylist.ProgressService);
            }
        }

        private readonly RelayCommand<VideoModel> _selectVideo;
        public ICommand SelectVideoCommand => _selectVideo;

        public void SelectVideo(VideoModel model)
        {
            _historyNavigationService.NavigateTo(PageKeys.Video.ToString());
            Messenger.Default.Send(model, Messages.Select);
        }

        private PlaylistModel _selectedPlaylist;
        public PlaylistModel SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                if (Set(ref _selectedPlaylist, value))
                    _playlistRepository.DownloadVideosForPlaylist(_selectedPlaylist, _progressService);
            }
        }
    }
}
