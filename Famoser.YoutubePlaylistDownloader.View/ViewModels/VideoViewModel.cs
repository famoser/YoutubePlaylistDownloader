using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.View.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;

namespace Famoser.YoutubePlaylistDownloader.View.ViewModels
{
    public class VideoViewModel : ViewModelBase
    {
        private readonly IVideoRespository _videoRespository;

        public VideoViewModel(IVideoRespository videoRespository)
        {
            _videoRespository = videoRespository;

            _saveFile = new RelayCommand(SaveFile, () => CanExecuteSaveFileCommand);

            Messenger.Default.Register<VideoModel>(this, Messages.Select, EvaluateSelectMessage);

            if (IsInDesignMode)
            {
                SelectedVideo = SimpleIoc.Default.GetInstance<IPlaylistRepository>().GetDesignCollection()[0].Videos[0];
            }
        }

        private void EvaluateSelectMessage(VideoModel obj)
        {
            SelectedVideo = obj;
        }

        private readonly RelayCommand _saveFile;
        public ICommand SaveFileCommand => _saveFile;

        public bool CanExecuteSaveFileCommand => SelectedVideo != null && !_saveFileActive;

        private bool _saveFileActive;
        public async void SaveFile()
        {
            _saveFileActive = true;
            _saveFile.RaiseCanExecuteChanged();

            //todo: check status of file
            SelectedVideo.ProgressService = new ProgressService();
            await _videoRespository.SaveToMusicLibrary(SelectedVideo);

            _saveFileActive = false;
            _saveFile.RaiseCanExecuteChanged();
        }

        private VideoModel _selectedVideo;
        public VideoModel SelectedVideo
        {
            get { return _selectedVideo; }
            set { Set(ref _selectedVideo, value); }
        }
    }
}
