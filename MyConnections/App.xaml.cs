using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ConnectionMgr.Properties;
using ConnectionMgr.Services;
using ConnectionMgr.ViewModels.Pages;
using ConnectionMgr.ViewModels.Windows;
using ConnectionMgr.Views.Pages;
using ConnectionMgr.Views.Windows;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.DependencyInjection;

namespace ConnectionMgr
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		// The.NET Generic Host provides dependency injection, configuration, logging, and other services.
		// https://docs.microsoft.com/dotnet/core/extensions/generic-host
		// https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
		// https://docs.microsoft.com/dotnet/core/extensions/configuration https://docs.microsoft.com/dotnet/core/extensions/logging
		private static readonly IHost _host = Host
			.CreateDefaultBuilder()
			.ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)); })
			.ConfigureServices((context, services) =>
			{
				services.AddNavigationViewPageProvider();

				services.AddHostedService<ApplicationHostService>();

				// Serilog
				services.AddSingleton<Interfaces.ILoggerService, LoggerService>();

				// Theme manipulation
				services.AddSingleton<IThemeService, ThemeService>();

				// TaskBar manipulation
				services.AddSingleton<ITaskBarService, TaskBarService>();

				// SnackBar
				services.AddSingleton<ISnackbarService, SnackbarService>();

				// Content Dialogs
				services.AddSingleton<IContentDialogService, ContentDialogService>();

				// Service containing navigation, same as INavigationWindow... but without window
				services.AddSingleton<INavigationService, NavigationService>();

				// Main window with navigation
				services.AddSingleton<INavigationWindow, MainWindow>();
				services.AddSingleton<MainWindowViewModel>();

				services.AddSingleton<ConnectionsPage>();
				services.AddSingleton<ConnectionsViewModel>();
				services.AddSingleton<FirewallPage>();
				services.AddSingleton<FirewallViewModel>();
				services.AddSingleton<HostsFileEditPage>();
				services.AddSingleton<HostsFileEditViewModel>();
				services.AddSingleton<SettingsPage>();
				services.AddSingleton<SettingsViewModel>();
			}).Build();

		/// <summary>
		/// Create logfiles in C:\Users\[UserName]\AppData\Local\wolfSYS\Tools\ConnectionMgr\logfiles
		/// </summary>
		public static string GetLogFilesPath
		{
			get
			{
				// Local user data folder (C:\Users\[UserName]\AppData\Local)
				string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

				// Folder hierarchy
				string logFolder = Path.Combine(appData, "wolfSYS", "Tools", "ConnectionMgr", "logfiles");

				// Ensure the folder exists
				if (!Directory.Exists(logFolder))
					Directory.CreateDirectory(logFolder);

				// Full path to the log file
				return Path.Combine(logFolder, "log.txt");
			}
		}

		/// <summary>
		/// Gets services.
		/// </summary>
		public static IServiceProvider Services
		{
			get { return _host.Services; }
		}

		/// <summary>
		/// Occurs when an exception is thrown by an application but not handled.
		/// </summary>
		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			// For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
			LoggerService log = new LoggerService();
			log.Fatal(e.Exception, "APP::UnhandledException");
			log.Dispose();
		}

		/// <summary>
		/// Occurs when the application is closing.
		/// </summary>
		private async void OnExit(object sender, ExitEventArgs e)
		{
			await _host.StopAsync();

			_host.Dispose();
		}

		/// <summary>
		/// Occurs when the application is loading.
		/// </summary>
		private async void OnStartup(object sender, StartupEventArgs e)
		{
			await _host.StartAsync();
		}
	}
}