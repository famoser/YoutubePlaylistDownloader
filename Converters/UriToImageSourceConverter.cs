using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace YoutubePlaylistDownloader.Converters
{
    public class UriToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var uri = value as Uri;
            if (uri != null)
            {
                Image img = new Image();
                BitmapImage img2 = new BitmapImage();
                img2.BeginInit();
                img2.UriSource = uri;
                img2.EndInit();

                img.Source = img2;
                return img;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
