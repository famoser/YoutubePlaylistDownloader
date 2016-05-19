using Windows.UI.Xaml.Controls;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.View.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public MainPageViewModel MainPageViewModel { get { return DataContext as MainPageViewModel; } }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var playlist = e.ClickedItem as PlaylistModel;
            if (playlist != null)
                if (MainPageViewModel.SelectPlaylistCommand.CanExecute(playlist))
                    MainPageViewModel.SelectPlaylistCommand.Execute(playlist);
        }
    }
}
