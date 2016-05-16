using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Repositories;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using GalaSoft.MvvmLight.Ioc;

namespace Famoser.YoutubePlaylistDownloader.View.ViewModels
{
    public class ViewModelLocatorBase
    {
        public ViewModelLocatorBase()
        {
            SimpleIoc.Default.Register<IProgressService, ProgressService>();
            SimpleIoc.Default.Register<IMp3Respository, Mp3Repository>();
            SimpleIoc.Default.Register<ISettingsRepository, SettingsRepository>();
            SimpleIoc.Default.Register<ISmartRepository, SmartRepository>();
            SimpleIoc.Default.Register<IYoutubeRepository, YoutubeRepository>();
                
            SimpleIoc.Default.Register<MainPageViewModel>();
        }

        public MainPageViewModel MainPageViewModel { get; set; }
    }
}
