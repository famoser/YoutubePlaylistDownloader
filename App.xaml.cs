using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using YoutubePlaylistDownloader.View.ViewModels;

namespace YoutubePlaylistDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SimpleIoc.Default.GetInstance<MainPageViewModel>().SaveState();
        }
    }
}
