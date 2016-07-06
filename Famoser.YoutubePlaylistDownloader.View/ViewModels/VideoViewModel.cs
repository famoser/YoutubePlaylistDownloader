using System.Windows.Input;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.FrameworkEssentials.View.Commands;
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
        private readonly IProgressService _progressService;

        public VideoViewModel(IVideoRespository videoRespository, IProgressService progressService)
        {
            _videoRespository = videoRespository;
            _progressService = progressService;

            _saveFile = new RelayCommand(SaveFile, () => CanExecuteSaveFileCommand);
            _addNewPicture = new RelayCommand<byte[]>(AddNewPicture, CanExecuteAddNewPictureCommand);

            Messenger.Default.Register<VideoModel>(this, Messages.Select, EvaluateSelectMessage);

            if (IsInDesignMode)
            {
                SelectedVideo = SimpleIoc.Default.GetInstance<IPlaylistRepository>().GetPlaylists()[0].Videos[0];
            }
        }

        private async void EvaluateSelectMessage(VideoModel obj)
        {
            SelectedVideo = obj;
            await _videoRespository.LoadFromMusicLibrary(SelectedVideo);
        }

        private readonly RelayCommand _saveFile;
        public ICommand SaveFileCommand => _saveFile;
        private bool CanExecuteSaveFileCommand => SelectedVideo != null && !_saveFileActive;

        private bool _saveFileActive;
        public async void SaveFile()
        {
            using (new IndeterminateProgressDisposable<IndeterminateProgressKey>(_saveFile, b => _saveFileActive = b, IndeterminateProgressKey.SavingFile, _progressService))
            {
                _addNewPicture.RaiseCanExecuteChanged();

                SelectedVideo.ProgressService = new ProgressService();
                await _videoRespository.SaveToMusicLibrary(SelectedVideo);
            }
            _addNewPicture.RaiseCanExecuteChanged();
        }

        private readonly RelayCommand<byte[]> _addNewPicture;
        public ICommand AddNewPictureCommand => _addNewPicture;

        private bool CanExecuteAddNewPictureCommand(byte[] bytes)
        {
            return !_saveFileActive && bytes != null && bytes.Length > 0;
        }

        public void AddNewPicture(byte[] bytes)
        {
            using (new IndeterminateProgressDisposable<IndeterminateProgressKey>(_saveFile, b => _saveFileActive = b, IndeterminateProgressKey.AddingNewPicture, _progressService))
            {
                _addNewPicture.RaiseCanExecuteChanged();
                SelectedVideo.Mp3Model.AlbumCover = bytes;

            }
            _addNewPicture.RaiseCanExecuteChanged();
        }

        private VideoModel _selectedVideo;
        public VideoModel SelectedVideo
        {
            get { return _selectedVideo; }
            set { Set(ref _selectedVideo, value); }
        }
    }
}
