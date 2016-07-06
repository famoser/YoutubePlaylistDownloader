using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Repositories;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace Famoser.YoutubePlaylistDownloader.View.ViewModels
{
    public class ViewModelLocatorBase
    {
        public ViewModelLocatorBase()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IProgressService, ProgressService>();
            SimpleIoc.Default.Register<IVideoRespository, VideoRepository>();
            SimpleIoc.Default.Register<ISettingsRepository, SettingsRepository>();
            SimpleIoc.Default.Register<ISmartRepository, SmartRepository>();
            SimpleIoc.Default.Register<IPlaylistRepository, PlaylistRepository>();

            SimpleIoc.Default.Register<MainPageViewModel>();
            SimpleIoc.Default.Register<PlaylistViewModel>();
            SimpleIoc.Default.Register<VideoViewModel>();
        }

        public MainPageViewModel MainPageViewModel => SimpleIoc.Default.GetInstance<MainPageViewModel>();
        public PlaylistViewModel PlaylistViewModel => SimpleIoc.Default.GetInstance<PlaylistViewModel>();
        public VideoViewModel VideoViewModel => SimpleIoc.Default.GetInstance<VideoViewModel>();
        public ProgressService ProgressService => SimpleIoc.Default.GetInstance<IProgressService>() as ProgressService;
    }
}
