using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Famoser.YoutubePlaylistDownloader.Business.Attributes;
using Famoser.YoutubePlaylistDownloader.Business.Enums;

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Converters
{
    public class SaveStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (SaveStatus) value;
            var memInfo = typeof(SaveStatus).GetMember(val.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute),
                false).ToList();
            if (attributes.Any())
                return ((DescriptionAttribute)attributes[0]).Description;

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
