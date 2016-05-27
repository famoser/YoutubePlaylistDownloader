using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Famoser.FrameworkEssentials.Logging;
using Famoser.YoutubePlaylistDownloader.Business.Helpers;
using Famoser.YoutubePlaylistDownloader.View.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChooseImagePage : Page
    {
        public ChooseImagePage()
        {
            this.InitializeComponent();
        }

        public VideoViewModel VideoViewModel => DataContext as VideoViewModel;

        private async void SetFromUri_OnClick(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(UriTextBox.Text);
            var bytes = await DownloadHelper.DownloadBytes(uri);
            if (VideoViewModel.AddNewPictureCommand.CanExecute(bytes))
                VideoViewModel.AddNewPictureCommand.Execute(bytes);
        }

        private async void ChooseFile_OnClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker { ViewMode = PickerViewMode.Thumbnail };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            var file = await openPicker.PickSingleFileAsync(Guid.NewGuid().ToString());
            await FromFile(file);
        }

        private async Task<bool> FromFile(StorageFile file)
        {
            try
            {
                var stream = await file.OpenStreamForReadAsync();
                return FromStream(stream);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        private bool FromStream(Stream stream)
        {
            try
            {
                var bytes = StreamHelper.StreamToByte(stream);
                if (VideoViewModel.AddNewPictureCommand.CanExecute(bytes))
                {
                    VideoViewModel.AddNewPictureCommand.Execute(bytes);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        private async void SetFromClipboard_OnClick(object sender, RoutedEventArgs e)
        {
            var content = Clipboard.GetContent();
            var fileTypes = new List<string>() { "png", "jpeg", "jpg" };
            if (content.Contains("FileDrop"))
            {
                var files = await content.GetStorageItemsAsync();
                foreach (var storageItem in files)
                {
                    var item = storageItem as StorageFile;
                    if (item != null && fileTypes.Any(f => f == item.FileType))
                    {
                        if (await FromFile(item))
                            return;
                    }
                }
            }
            else if (content.Contains("Bitmap"))
            {
                var bitmap = await content.GetBitmapAsync();
                var read = await bitmap.OpenReadAsync();
                FromStream(read.AsStreamForRead());
            }
        }
    }
}
