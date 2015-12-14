using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
                var state = SaveService.Instance.RetrieveState();
                TargetFolder = state.TargetFolder;
                TempFolder = state.TempFolder;
            }
        }

        private RelayCommand _getPlaylistsCommand;
        public ICommand GetPlaylistsCommand
        {
            get { return _getPlaylistsCommand; }
        }

        public bool CanExecuteGetPlaylistsCommand
        {
            get
            {
                return !string.IsNullOrEmpty(TempFolder) && !string.IsNullOrEmpty(TargetFolder);
            }
        }

        public async void GetPlaylists()
        {
            Playlists = new ObservableCollection<PlaylistModel>(await YoutubeService.Instance.GetPlaylists());
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
            get { return Playlists != null && Playlists.Any(); }
        }

        public async void StartDownload()
        {
            foreach (var source in Playlists.Where(p => p.Download))
            {
                var vids = await YoutubeService.Instance.GetVideos(source.Id);
                var dic = Path.Combine(TempFolder, source.Name);

                /* remove already downloaded temp files */
                if (!Directory.Exists(dic))
                    Directory.CreateDirectory(dic);
                else
                {
                    string[] files = Directory.GetFiles(dic, "*.", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        var names = files.Select(f => f.Split(new string[] {"."}, StringSplitOptions.None)[0]);
                        foreach (var name in names)
                        {
                            var item = vids.FirstOrDefault(v => v.Id == name);
                            if (item != null)
                                vids.Remove(item);
                        }
                    }
                }

                foreach (var videoModel in vids)
                {
                    await DownloadService.Instance.DownloadYoutubeVideo(videoModel, dic);
                }
            }
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
    }
}
