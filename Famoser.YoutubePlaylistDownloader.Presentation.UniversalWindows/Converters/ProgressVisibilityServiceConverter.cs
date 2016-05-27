using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Famoser.FrameworkEssentials.Services.Interfaces;

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Converters
{
    public class ProgressVisibilityServiceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var service = value as IProgressService;
            if (service != null && service.IsAnyProgressActive())
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
