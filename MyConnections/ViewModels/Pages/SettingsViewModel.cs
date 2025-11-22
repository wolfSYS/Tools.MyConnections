using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using ConnectionMgr.Properties;
using Windows.Storage;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace ConnectionMgr.ViewModels.Pages
{
	public partial class SettingsViewModel : PagesBaseViewModel
	{
		[ObservableProperty]
		private string _appVersion = String.Empty;

		[ObservableProperty]
		private bool _captureTCP4 = Settings.Default.CaptureTCP4;

		[ObservableProperty]
		private bool _captureTCP6 = Settings.Default.CaptureTCP4;

		[ObservableProperty]
		private bool _captureUDP4 = Settings.Default.CaptureUDP4;

		[ObservableProperty]
		private bool _captureUDP6 = Settings.Default.CaptureUDP6;

		[ObservableProperty]
		private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

		private bool _isInitialized = false;

		[ObservableProperty]
		private bool _logLevelDebug = Settings.Default.LogLevelDebug;

		[ObservableProperty]
		private string _openAiApiKey = Settings.Default.OpenAiApiKey;

		[ObservableProperty]
		private string _openAiModel = Settings.Default.OpenAiModel;

		[ObservableProperty]
		private string _openAiServerUrl = Settings.Default.OpenAiServerUrl;

		public SettingsViewModel(
			Interfaces.ILoggerService logger,
			IContentDialogService dialogService,
			ISnackbarService snackbarService)
			: base(logger, dialogService, snackbarService)
		{
			// BaseVM will care about DY
		}

		public override Task OnNavigatedFromAsync()
		{
			_isInitialized = false;

			return Task.CompletedTask;
		}

		public override Task OnNavigatedToAsync()
		{
			if (!_isInitialized)
				InitializeViewModel();

			return Task.CompletedTask;
		}

		[RelayCommand]
		public void OpenLogFile()
		{
			Process process = new Process();
			process.StartInfo.FileName = "explorer.exe";
			process.StartInfo.Arguments = Path.GetDirectoryName(App.GetLogFilesPath);
			process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			process.Start();
		}

		private string GetAssemblyVersion()
		{
			return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
				?? String.Empty;
		}

		private void InitializeViewModel()
		{
			CurrentTheme = ApplicationThemeManager.GetAppTheme();
			AppVersion = $"wolfSYS.Tools.ConnectionMgr - ver. {GetAssemblyVersion()}";
			_isInitialized = true;
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

		[RelayCommand]
		private void SetCaptureTCP4()
		{
			CaptureTCP4 = !CaptureTCP4;
			Settings.Default.CaptureTCP4 = CaptureTCP4;
			Settings.Default.Save();
		}

		[RelayCommand]
		private void SetCaptureTCP6()
		{
			CaptureTCP6 = !CaptureTCP6;
			Settings.Default.CaptureTCP6 = CaptureTCP6;
			Settings.Default.Save();
		}

		[RelayCommand]
		private void SetCaptureUDP4()
		{
			CaptureUDP4 = !CaptureUDP4;
			Settings.Default.CaptureUDP4 = CaptureUDP4;
			Settings.Default.Save();
		}

		[RelayCommand]
		private void SetCaptureUDP6()
		{
			CaptureUDP6 = !CaptureUDP6;
			Settings.Default.CaptureUDP6 = CaptureUDP6;
			Settings.Default.Save();
		}

		[RelayCommand]
		private void SetLogLevel()
		{
			LogLevelDebug = !LogLevelDebug;
			Settings.Default.LogLevelDebug = LogLevelDebug;
			Settings.Default.Save();
		}

		[RelayCommand]
		private void VisitWebsite()
		{
			try
			{
				Process.Start(new ProcessStartInfo("https://www.wolfsys.net") { UseShellExecute = true });
			}
			catch (System.ComponentModel.Win32Exception noBrowserEx)
			{
				_logger.Error(noBrowserEx, "SettingsVM::VisitWebsite");
				ShowError(noBrowserEx);
			}
			catch (System.Exception otherEx)
			{
				_logger.Error(otherEx, "SettingsVM::VisitWebsite");
				ShowError(otherEx);
			}
		}
	}
}