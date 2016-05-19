using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
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
    }
}
