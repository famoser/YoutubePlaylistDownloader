using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Famoser.YoutubePlaylistDownloader.View.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IVideoRespository _videoRespository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ISmartRepository _smartRepository;
        private readonly IProgressService _progressService;

        public MainPageViewModel(IVideoRespository videoRespository, IPlaylistRepository playlistRepository, ISmartRepository smartRepository, IProgressService progressService)
        {
            _videoRespository = videoRespository;
            _playlistRepository = playlistRepository;
            _smartRepository = smartRepository;
            _progressService = progressService;

            _refreshPlaylists = new RelayCommand(RefreshPlaylist, () => CanExecuteRefreshPlaylistsCommand);
            _startDownload = new RelayCommand(StartDownload, () => CanExecuteStartDownloadCommand);
            _addToPlaylistsCommand = new RelayCommand(AddToPlaylist, () => CanExecuteAddToPlaylistCommand);

            if (IsInDesignMode)
            {
                Playlists = _playlistRepository.GetDesignCollection();
            }
            else
            {
                Initialize();
            }
        }

        private async void Initialize()
        {
            Playlists = await _playlistRepository.GetPlaylists();
        }

        private readonly RelayCommand _refreshPlaylists;
        public ICommand RefreshPlaylistsCommand => _refreshPlaylists;

        public bool CanExecuteRefreshPlaylistsCommand => !_refreshPlaylistsActive;

        private bool _refreshPlaylistsActive;
        public async void RefreshPlaylist()
        {
            _refreshPlaylistsActive = true;
            _refreshPlaylists.RaiseCanExecuteChanged();

            await _playlistRepository.RefreshAllPlaylists(_progressService);

            _refreshPlaylistsActive = false;
            _refreshPlaylists.RaiseCanExecuteChanged();
        }

        private readonly RelayCommand _startDownload;
        public ICommand StartDownloadCommand => _startDownload;

        public bool CanExecuteStartDownloadCommand => Playlists != null && Playlists.Any() && !_startDownloadActive;

        private bool _startDownloadActive;
        public async void StartDownload()
        {
            _startDownloadActive = true;
            _startDownload.RaiseCanExecuteChanged();

            await _playlistRepository.DownloadVideosForAllPlaylists(_progressService);

            _startDownloadActive = false;
            _startDownload.RaiseCanExecuteChanged();
        }


        private string _playListLink;
        public string PlaylistLink
        {
            get { return _playListLink; }
            set
            {
                if (Set(ref _playListLink, value))
                    _addToPlaylistsCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly RelayCommand _addToPlaylistsCommand;
        public ICommand AddToPlaylistCommand
        {
            get { return _addToPlaylistsCommand; }
        }

        public bool CanExecuteAddToPlaylistCommand
        {
            get { return !string.IsNullOrEmpty(PlaylistLink) && !_isAddingPlaylist; }
        }

        private bool _isAddingPlaylist;
        public async void AddToPlaylist()
        {
            _isAddingPlaylist = true;
            _addToPlaylistsCommand.RaiseCanExecuteChanged();

            await _playlistRepository.AddNewPlaylistByLink(PlaylistLink);
            PlaylistLink = null;

            _isAddingPlaylist = false;
            _addToPlaylistsCommand.RaiseCanExecuteChanged();
        }

        private ObservableCollection<PlaylistModel> _playlists;
        public ObservableCollection<PlaylistModel> Playlists
        {
            get { return _playlists; }
            set
            {
                if (Set(ref _playlists, value))
                    _startDownload.RaiseCanExecuteChanged();
            }
        }
    }
}
