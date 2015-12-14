using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using YoutubePlaylistDownloader.Business.Services;

namespace YoutubePlaylistDownloader.View.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
            _startDownload = new RelayCommand(StartDownload);
        }

        private RelayCommand _startDownload;

        public ICommand StartDownloadCommand
        {
            get { return _startDownload; }
        }

        public async void StartDownload()
        {
            var service = new YoutubeService();
            var res = await service.DownloadVideos();
            res.ToString();
        }
    }
}
