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
		/// <para>
		/// Returns the folder that the Common Language Runtime uses as the starting point for assembly
		/// resolution.  On Windows, the exact string returned can by AppContext.BaseDirectory vary between
		/// installations and deployment types: the value may or may not have a trailing backslash.
		/// </para>
		/// </summary>
		/// <remarks>
		/// <para>
		/// • **Framework‑dependent deployments** – The base directory is usually the folder that
		///   contains the host executable (e.g. <c>&lt;install‑folder&gt;\app.exe</c>).  Some installations
		///   record that path with a trailing backslash, while others do not.
		/// </para>
		/// <para>
		/// • **Self‑contained or single‑file deployments** – When the app runs as a single file, the
		///   runtime extracts the embedded assemblies into a temporary folder.  The path to that
		///   folder is returned by <c>AppContext.BaseDirectory</c>, and that temporary path ends with a
		///   trailing backslash.
		/// </para>
		/// <para>
		/// Because the presence of the trailing slash is not guaranteed, the .NET runtime documentation
		/// specifies that <c>AppContext.BaseDirectory</c> may or may not end with a backslash, and the
		/// behaviour can differ between installations.  For consistent path handling, always combine
		/// paths using <c>Path.Combine</c>, which takes care of any trailing slashes automatically.
		/// </para>
		/// </remarks>
		public static string GetLogFilesPath
		{
			get
			{
				return Path.Combine(AppContext.BaseDirectory, "logfiles", "log.txt");
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