using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Famoser.FrameworkEssentials.Singleton;
using GalaSoft.MvvmLight.Views;

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Platform
{

    public class CustomNavigationService : SingletonBase<CustomNavigationService>, INavigationService
    {
        private int _backStack;
        private readonly NavigationService _realNavigationService;

        public CustomNavigationService()
        {
            _realNavigationService = new NavigationService();
            _backStack = 0;
        }

        public NavigationService Implementation => _realNavigationService;

        public void GoBack()
        {
            _backStack--;
            ConfigureButtons();

            _realNavigationService.GoBack();
        }

        public void NavigateTo(string pageKey)
        {
            _backStack++;
            ConfigureButtons();

            _realNavigationService.NavigateTo(pageKey);
        }

        public void NavigateTo(string pageKey, object parameter)
        {
            _backStack++;
            ConfigureButtons();

            _realNavigationService.NavigateTo(pageKey, parameter);
        }

        public string CurrentPageKey
        {
            get { return _realNavigationService.CurrentPageKey; }
        }

        private void ConfigureButtons()
        {
            if (_backStack == 0)
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            else
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
        }
    }
}
