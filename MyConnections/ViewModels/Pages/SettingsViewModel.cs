using System.Diagnostics;
using System.IO;
using MyConnections.Properties;
using Windows.Storage;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace MyConnections.ViewModels.Pages
{
    public partial class FirewallViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        [ObservableProperty]
        private bool _logLevelDebug = Settings.Default.LogLevelDebug;

		public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            _isInitialized = true;
		}
    }
}
