﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.FrameworkEssentials.View.Commands;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.View.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Famoser.YoutubePlaylistDownloader.View.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IProgressService _progressService;

        private readonly IHistoryNavigationService _historyNavigationService;

        public MainPageViewModel(IPlaylistRepository playlistRepository, IProgressService progressService, IHistoryNavigationService historyNavigationService, ISettingsRepository settingsRepository)
        {
            _playlistRepository = playlistRepository;
            _progressService = progressService;
            _historyNavigationService = historyNavigationService;
            _settingsRepository = settingsRepository;

            _refreshPlaylists = new RelayCommand(RefreshPlaylist, () => CanExecuteRefreshPlaylistsCommand);
            _startDownload = new RelayCommand(StartDownload, () => CanExecuteStartDownloadCommand);
            _addToPlaylistsCommand = new RelayCommand(AddToPlaylist, () => CanExecuteAddToPlaylistCommand);
            _saveCache = new RelayCommand(SaveCache, () => CanExecuteSaveCacheCommand);
            _selectPlaylist = new RelayCommand<PlaylistModel>(SelectPlaylist);

            Playlists = _playlistRepository.GetPlaylists();

            if (!IsInDesignMode)
                RefreshPlaylist();
        }

        public ProgressService ProgressService => _progressService as ProgressService;

        private readonly RelayCommand _refreshPlaylists;
        public ICommand RefreshPlaylistsCommand => _refreshPlaylists;

        private bool CanExecuteRefreshPlaylistsCommand => !IsRefreshing;

        public async void RefreshPlaylist()
        {
            using (new IndeterminateProgressDisposable<IndeterminateProgressKeys, object>(_refreshPlaylists, b => IsRefreshing = b, IndeterminateProgressKeys.RefreshingPlaylists, _progressService))
            {
                _startDownload.RaiseCanExecuteChanged();

                await _playlistRepository.RefreshAllPlaylists();
            }
            _startDownload.RaiseCanExecuteChanged();
        }

        private readonly RelayCommand _startDownload;
        public ICommand StartDownloadCommand => _startDownload;

        private bool CanExecuteStartDownloadCommand => Playlists != null && Playlists.Any() && !IsRefreshing;

        private bool IsRefreshing { get; set; }
        public async void StartDownload()
        {
            using (new IndeterminateProgressDisposable<IndeterminateProgressKeys, object>(_startDownload, b => IsRefreshing = b, IndeterminateProgressKeys.StartingDownload, _progressService))
            {
                _refreshPlaylists.RaiseCanExecuteChanged();

                await _playlistRepository.DownloadVideosForAllPlaylists();
            }
            _refreshPlaylists.RaiseCanExecuteChanged();
        }

        private readonly RelayCommand _saveCache;
        public ICommand SaveCacheCommand => _saveCache;

        private bool CanExecuteSaveCacheCommand => !IsSavingCache;

        private bool IsSavingCache { get; set; }
        public async void SaveCache()
        {
            using (new IndeterminateProgressDisposable<IndeterminateProgressKeys, object>(_saveCache, b => IsSavingCache = b, IndeterminateProgressKeys.SavingCache, _progressService))
            {
                await _settingsRepository.SaveCache();
            }
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
        public ICommand AddToPlaylistCommand => _addToPlaylistsCommand;

        private bool CanExecuteAddToPlaylistCommand => !string.IsNullOrEmpty(PlaylistLink) && !IsAddingPlaylist;

        private bool IsAddingPlaylist { get; set; }
        public async void AddToPlaylist()
        {
            using (new IndeterminateProgressDisposable<IndeterminateProgressKeys, object>(_addToPlaylistsCommand, b => IsAddingPlaylist = b, IndeterminateProgressKeys.AddingToPlaylist, _progressService))
            {
                await _playlistRepository.AddNewPlaylistByLink(PlaylistLink);
                PlaylistLink = null;
            }
        }

        private readonly RelayCommand<PlaylistModel> _selectPlaylist;
        public ICommand SelectPlaylistCommand => _selectPlaylist;

        public void SelectPlaylist(PlaylistModel model)
        {
            _historyNavigationService.NavigateTo(PageKeys.Playlist.ToString());
            Messenger.Default.Send(model, Messages.Select);
        }

        public ObservableCollection<PlaylistModel> Playlists { get; }
    }
}
