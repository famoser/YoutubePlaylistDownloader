using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
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
            SimpleIoc.Default.Register(GetNavigationService);
        }

        private INavigationService GetNavigationService()
        {
            var navigationService = new CustomNavigationService();
            navigationService.Implementation.Configure(PageKeys.Mainpage.ToString(), typeof(MainPage));
            return navigationService;
        }
    }
}
