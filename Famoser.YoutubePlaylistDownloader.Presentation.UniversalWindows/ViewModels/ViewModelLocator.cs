using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Enum;
using Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Pages;
using Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Platform;
using Famoser.YoutubePlaylistDownloader.View.Enums;
using Famoser.YoutubePlaylistDownloader.View.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.ViewModels
{
    public class ViewModelLocator : ViewModelLocatorBase
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IPlatformService, PlatformService>();
            SimpleIoc.Default.Register<IFolderStorageService, FolderStorageService>();
            var ns = GetNavigationService();
            SimpleIoc.Default.Register(() => ns);
        }

        private INavigationService GetNavigationService()
        {
            var navigationService = CustomNavigationService.Instance;
            navigationService.Implementation.Configure(PageKeys.Mainpage.ToString(), typeof(MainPage));
            navigationService.Implementation.Configure(PageKeys.Video.ToString(), typeof(VideoPage));
            navigationService.Implementation.Configure(PageKeys.Playlist.ToString(), typeof(PlaylistPage));
            navigationService.Implementation.Configure(LocalPages.ChooseImagePage.ToString(), typeof(ChooseImagePage));
            return navigationService;
        }
    }
}
