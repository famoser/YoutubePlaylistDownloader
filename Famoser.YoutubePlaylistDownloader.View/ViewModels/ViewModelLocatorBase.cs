using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using GalaSoft.MvvmLight.Ioc;

namespace Famoser.YoutubePlaylistDownloader.View.ViewModels
{
    public class ViewModelLocatorBase
    {
        public ViewModelLocatorBase()
        {
            SimpleIoc.Default.Register<IProgressService, ProgressService>();
        }
    }
}
