using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Famoser.YoutubePlaylistDownloader.View.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private IMp3Respository _mp3Respository;
        private IYoutubeRepository _youtubeRepository;
        private ISmartRepository _smartRepository;
        private IProgressService _progressService;

        public MainPageViewModel(IMp3Respository mp3Respository, IYoutubeRepository youtubeRepository, ISmartRepository smartRepository, IProgressService progressService)
        {
            _mp3Respository = mp3Respository;
            _youtubeRepository = youtubeRepository;
            _smartRepository = smartRepository;
            _progressService = progressService;

            _startDownload = new RelayCommand(StartDownload, () => CanExecuteStartDownloadCommand);
            _addToPlaylistsCommand = new RelayCommand(AddToPlaylist, () => CanExecuteAddToPlaylistCommand);

            if (IsInDesignMode)
            {
                Playlists = new ObservableCollection<PlaylistModel>();
                for (int i = 0; i < 20; i++)
                {
                    var model = new PlaylistModel()
                    {
                        Download = Convert.ToBoolean(i % 2),
                        Name = "name " + i,
                        TotalVideos = 15 + i
                    };
                    Playlists.Add(model);
                }
            }
            else
            {
                Initialize();
            }
        }

        private async void Initialize()
        {
            Playlists = await _youtubeRepository.GetPlaylists();
        }

        private RelayCommand _startDownload;
        public ICommand StartDownloadCommand
        {
            get { return _startDownload; }
        }

        public bool CanExecuteStartDownloadCommand
        {
            get { return Playlists != null && Playlists.Any() && !_startDownloadActive; }
        }

        private bool _startDownloadActive;
        public async void StartDownload()
        {
            _startDownloadActive = true;
            _startDownload.RaiseCanExecuteChanged();

            await _youtubeRepository.DownloadVideos(_progressService);
            _smartRepository.AssignMetaTags(Playlists);
            await _mp3Respository.SavePlaylists(Playlists);
            
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

        private RelayCommand _addToPlaylistsCommand;
        public ICommand AddToPlaylistCommand
        {
            get { return _addToPlaylistsCommand; }
        }

        public bool CanExecuteAddToPlaylistCommand
        {
            get { return !string.IsNullOrEmpty(PlaylistLink); }
        }

        public async void AddToPlaylist()
        {
            var playlist = await _youtubeRepository.GetPlaylistByLink(PlaylistLink);
            PlaylistLink = null;
            if (playlist != null)
            {
                Playlists.Add(playlist);
                _startDownload.RaiseCanExecuteChanged();
            }
            _playListLink = null;
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
        
        private Mp3Model _selectedMp3File;
        public Mp3Model SelectedMp3File
        {
            get { return _selectedMp3File; }
            set { Set(ref _selectedMp3File, value); }
        }
    }
}
