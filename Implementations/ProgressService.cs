using GalaSoft.MvvmLight.Ioc;
using YoutubePlaylistDownloader.Business.Models;
using YoutubePlaylistDownloader.Business.Services.Interfaces;
using YoutubePlaylistDownloader.View.ViewModels;

namespace YoutubePlaylistDownloader.Implementations
{
    class ProgressService : IProgressService
    {
        private ProgressViewModel _pvm;
        public ProgressService()
        {
            _pvm = SimpleIoc.Default.GetInstance<ProgressViewModel>();
        }

        public void SetProgress(ProgressModel pm, int progress)
        {
            _pvm.SetProgress(pm, progress);
        }

        public void RemoveProgress(ProgressModel pm)
        {
            _pvm.RemoveProgress(pm);
        }
    }
}
