using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.View.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;

namespace Famoser.YoutubePlaylistDownloader.View.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IHistoryNavigationService _historyNavigationService;

        public PlaylistViewModel(IPlaylistRepository playlistRepository, IHistoryNavigationService historyNavigationService)
        {
            _playlistRepository = playlistRepository;
            _historyNavigationService = historyNavigationService;

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
            _refreshPlaylistActive = true;
            _refreshPlaylist.RaiseCanExecuteChanged();

            //todo: check if already actualizing playlist
            SelectedPlaylist.ProgressService = new ProgressService();
            await _playlistRepository.RefreshPlaylist(SelectedPlaylist, SelectedPlaylist.ProgressService);

            _refreshPlaylistActive = false;
            _refreshPlaylist.RaiseCanExecuteChanged();
        }

        private readonly RelayCommand _startDownload;
        public ICommand StartDownloadCommand => _startDownload;

        public bool CanExecuteStartDownloadCommand => SelectedPlaylist != null && !_startDownloadActive;

        private bool _startDownloadActive;
        public async void StartDownload()
        {
            _startDownloadActive = true;
            _startDownload.RaiseCanExecuteChanged();

            SelectedPlaylist.ProgressService = new ProgressService();
            await _playlistRepository.DownloadVideosForPlaylist(SelectedPlaylist, SelectedPlaylist.ProgressService);

            _startDownloadActive = false;
            _startDownload.RaiseCanExecuteChanged();
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
            set { Set(ref _selectedPlaylist, value); }
        }
    }
}
