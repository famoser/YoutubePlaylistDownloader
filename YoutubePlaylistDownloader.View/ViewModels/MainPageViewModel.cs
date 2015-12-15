using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using YoutubePlaylistDownloader.Business.Models;
using YoutubePlaylistDownloader.Business.Services;

namespace YoutubePlaylistDownloader.View.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
            _startDownload = new RelayCommand(StartDownload, () => CanExecuteStartDownloadCommand);
            _getPlaylistsCommand = new RelayCommand(GetPlaylists, () => CanExecuteGetPlaylistsCommand);
            _saveStateCommand = new RelayCommand(SaveState);

            _downloadAlbumArtCommand = new RelayCommand(DownloadAlbumArt, () => CanExecuteDownloadAlbumArtCommand);
            _saveMp3Command = new RelayCommand(SaveMp3, () => CanExecuteSaveMp3Command);
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

                Mp3Files = new ObservableCollection<Mp3Model>();
                for (int i = 0; i < 20; i++)
                {
                    var model = new Mp3Model()
                    {
                        OriginalTitle = "OriginTitle",
                        Album = "Album",
                        AlbumArtist = "Album Artist",
                        Artist = "Artist",
                        Comment = "Comment",
                        Genre = "Genre",
                        Title = "Title",
                        AlbumCover = new Uri("http://ecx.images-amazon.com/images/I/71HH7D7Z66L._SL1500_.jpg"),
                        Year = 2012
                    };
                    Mp3Files.Add(model);
                }
                SelectedMp3File = Mp3Files[0];
            }
            else
            {
                var state = SaveService.Instance.RetrieveState();
                Mp3Files = new ObservableCollection<Mp3Model>();
                Playlists = new ObservableCollection<PlaylistModel>();
                TargetFolder = state.TargetFolder;
                TempFolder = state.TempFolder;
            }
        }

        private bool _getPlaylistsActive;
        private RelayCommand _getPlaylistsCommand;
        public ICommand GetPlaylistsCommand
        {
            get { return _getPlaylistsCommand; }
        }

        public bool CanExecuteGetPlaylistsCommand
        {
            get
            {
                return !string.IsNullOrEmpty(TempFolder) && !string.IsNullOrEmpty(TargetFolder) && !_getPlaylistsActive;
            }
        }

        public async void GetPlaylists()
        {
            _getPlaylistsActive = true;
            _getPlaylistsCommand.RaiseCanExecuteChanged();

            Playlists = new ObservableCollection<PlaylistModel>(await YoutubeService.Instance.GetPlaylists());
            _startDownload.RaiseCanExecuteChanged();

            _getPlaylistsActive = false;
            _getPlaylistsCommand.RaiseCanExecuteChanged();
        }

        private RelayCommand _saveStateCommand;
        public ICommand SaveStateCommand
        {
            get { return _saveStateCommand; }
        }

        public void SaveState()
        {
            var state = new SaveModel()
            {
                TempFolder = TempFolder,
                TargetFolder = TargetFolder
            };
            SaveService.Instance.SaveState(state);
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

            foreach (var playlist in Playlists.Where(p => p.Download))
            {
                var newfiles = await WorkflowService.Instance.Execute(playlist, TempFolder, TargetFolder);
                foreach (var mp3Model in newfiles)
                {
                    mp3Model.AlbumCover = await Mp3Service.Instance.GetAlbumCoverUri(mp3Model);
                    await Mp3Service.Instance.SaveModel(mp3Model, false);
                    Mp3Files.Add(mp3Model);
                }
            }
            _startDownloadActive = false;
            _startDownload.RaiseCanExecuteChanged();
        }

        private RelayCommand _saveMp3Command;
        public ICommand SaveMp3Command
        {
            get { return _saveMp3Command; }
        }

        public bool CanExecuteSaveMp3Command
        {
            get { return !_saveMp3Active; }
        }

        private bool _saveMp3Active;
        public async void SaveMp3()
        {
            _saveMp3Active = true;
            _saveMp3Command.RaiseCanExecuteChanged();

            await Task.Run(() => Mp3Service.Instance.SaveModel(SelectedMp3File, true));
            Mp3Files.Remove(SelectedMp3File);
            if (Mp3Files.Any())
                SelectedMp3File = Mp3Files[0];

            _saveMp3Active = false;
            _saveMp3Command.RaiseCanExecuteChanged();
        }

        private RelayCommand _downloadAlbumArtCommand;
        public ICommand DownloadAlbumArtCommand
        {
            get { return _downloadAlbumArtCommand; }
        }

        public bool CanExecuteDownloadAlbumArtCommand
        {
            get { return !_downloadAlbumArtActive; }
        }

        private bool _downloadAlbumArtActive;
        public async void DownloadAlbumArt()
        {
            _downloadAlbumArtActive = true;
            _downloadAlbumArtCommand.RaiseCanExecuteChanged();

            SelectedMp3File.AlbumCover = await Mp3Service.Instance.GetAlbumCoverUri(SelectedMp3File);

            _downloadAlbumArtActive = false;
            _downloadAlbumArtCommand.RaiseCanExecuteChanged();
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
            var playlist = await YoutubeService.Instance.GetPlaylistByLink(PlaylistLink);
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

        private string _targetFolder;
        public string TargetFolder
        {
            get { return _targetFolder; }
            set
            {
                if (Set(ref _targetFolder, value))
                    _getPlaylistsCommand.RaiseCanExecuteChanged();
            }
        }

        private string _tempFolder;
        public string TempFolder
        {
            get { return _tempFolder; }
            set
            {
                if (Set(ref _tempFolder, value))
                    _getPlaylistsCommand.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<Mp3Model> _mp3Files;
        public ObservableCollection<Mp3Model> Mp3Files
        {
            get { return _mp3Files; }
            set { Set(ref _mp3Files, value); }
        }

        private Mp3Model _selectedMp3File;
        public Mp3Model SelectedMp3File
        {
            get { return _selectedMp3File; }
            set { Set(ref _selectedMp3File, value); }
        }
    }
}
