using System.Diagnostics;
using System.IO;
using MyConnections.Properties;
using Windows.Storage;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace MyConnections.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
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
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"UiDesktopApp1 - {GetAssemblyVersion()}";

            _isInitialized = true;

			//Settings.Default.LogLevelDebug

		}

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

		[RelayCommand]
		private void SetLogLevel()
		{
			LogLevelDebug = !LogLevelDebug;
            Settings.Default.LogLevelDebug = LogLevelDebug;
            Settings.Default.Save();
		}

		[RelayCommand]
		public void OpenLogFile()
		{
		    var logFilePath = $@"{AppContext.BaseDirectory}\logfiles\log.txt";

			Process process = new Process();
			process.StartInfo.FileName = "explorer.exe";
			process.StartInfo.Arguments = Path.GetDirectoryName(logFilePath);
			process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			process.Start();
		}

		[RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;

                    Settings.Default.Theme = "theme_light";
                    Settings.Default.Save();

					break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

					Settings.Default.Theme = "theme_dark";
					Settings.Default.Save();

					break;
            }
        }
    }
}
