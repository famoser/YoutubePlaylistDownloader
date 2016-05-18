using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Famoser.YoutubePlaylistDownloader.Business.Enums;

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Converters
{
    public class SaveStatusVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (SaveStatus) value;
            if (val > SaveStatus.Discovered && val < SaveStatus.Finished)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
