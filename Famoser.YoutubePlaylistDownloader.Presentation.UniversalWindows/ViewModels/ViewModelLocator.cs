using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Platform;
using Famoser.YoutubePlaylistDownloader.View.ViewModels;
using GalaSoft.MvvmLight.Ioc;

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.ViewModels
{
    public class ViewModelLocator : ViewModelLocatorBase
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<IPlatformService, PlatformService>();
            SimpleIoc.Default.Register<IFolderStorageService, FolderStorageService>();
        }
    }
}
