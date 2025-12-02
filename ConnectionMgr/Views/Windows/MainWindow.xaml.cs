using System.Windows.Media;
using AutoUpdaterDotNET;
using ConnectionMgr.Helpers;
using ConnectionMgr.Properties;
using ConnectionMgr.ViewModels.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ConnectionMgr.Views.Windows
{
	/// <summary>
	/// Provides the MainWindow of the application.
	/// </summary>
	/// <remarks>In the <see cref="MainWindow_Loaded(object, RoutedEventArgs)"/> event we check for automatically updates via AutoUpdaterDotNET.</remarks>
	public partial class MainWindow : INavigationWindow
	{
		public MainWindow(
			MainWindowViewModel viewModel,
			INavigationViewPageProvider navigationViewPageProvider,
			INavigationService navigationService
		)
		{
			ViewModel = viewModel;
			DataContext = this;

			SystemThemeWatcher.Watch(this);

			SettingsUpgradeManager.EnsureUpgraded();

			if (Settings.Default.Theme.Contains("_light"))
				ApplicationThemeManager.Apply(ApplicationTheme.Light);
			else
				ApplicationThemeManager.Apply(ApplicationTheme.Dark);

			HandleMainWindowSettings();
			InitializeComponent();

			// set content presenters for both Snackbar and ContentDialog (Lepo WPF UI)
			var snackbarService = (ISnackbarService)App.Services.GetRequiredService<ISnackbarService>();
			var contentDialogService = (IContentDialogService)App.Services.GetRequiredService<IContentDialogService>();
			snackbarService.SetSnackbarPresenter(SnackbarPresenter);
			contentDialogService.SetDialogHost(RootContentDialog);

			SetPageService(navigationViewPageProvider);
			navigationService.SetNavigationControl(RootNavigation);
		}

		public MainWindowViewModel ViewModel { get; }

		#region INavigationWindow methods

		public void CloseWindow() => Close();

		public INavigationView GetNavigation() => RootNavigation;

		public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

		public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

		public void ShowWindow() => Show();

		#endregion INavigationWindow methods

		INavigationView INavigationWindow.GetNavigation()
		{
			throw new NotImplementedException();
		}

		public void SetServiceProvider(IServiceProvider serviceProvider)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Raises the closed event and saves the main window poisition & size.
		/// </summary>
		protected override void OnClosed(EventArgs e)
		{
			// Returns true if the entire window is inside the screens boundaries.
			// The check is performed in *device pixels* so it works even on high‑DPI displays.
			bool IsWindowFullyVisible()
			{
				// 1. get the DPI scaling for this window.
				var dpi = VisualTreeHelper.GetDpi(this);

				// 2. convert Left/Top/Width/Height to device pixel values.
				var left = (int)Math.Round(this.Left * dpi.PixelsPerDip);
				var top = (int)Math.Round(this.Top * dpi.PixelsPerDip);
				var width = (int)Math.Round(this.Width * dpi.PixelsPerDip);
				var height = (int)Math.Round(this.Height * dpi.PixelsPerDip);

				var windowRect = new System.Drawing.Rectangle(left, top, width, height);

				// 3. find the monitor that contains the window’s center point
				var centerPoint = new System.Drawing.Point(left + width / 2, top + height / 2);
				var ownerScreen = System.Windows.Forms.Screen.FromPoint(centerPoint);

				// 4. the rectangle must be fully inside that monitor’s bounds
				return ownerScreen.Bounds.Contains(windowRect);
			}

			// persist window position and size
			if (this.WindowState == WindowState.Normal && IsWindowFullyVisible())
			{
				Settings.Default.MainWindowX = this.Left;
				Settings.Default.MainWindowY = this.Top;
				Settings.Default.MainWindowW = this.Width;
				Settings.Default.MainWindowH = this.Height;
				Settings.Default.Save();
			}

			base.OnClosed(e);

			// Make sure that closing this window will begin the process of closing the application.
			Application.Current.Shutdown();
		}

		/// <summary>
		/// Load last known window position and size
		/// </summary>
		private void HandleMainWindowSettings()
		{
			if (Settings.Default.MainWindowX > 0)
				this.Left = Settings.Default.MainWindowX;

			if (Settings.Default.MainWindowY > 0)
				this.Top = Settings.Default.MainWindowY;

			if (Settings.Default.MainWindowW > 0)
				this.Width = Settings.Default.MainWindowW;

			if (Settings.Default.MainWindowH > 0)
				this.Height = Settings.Default.MainWindowH;
		}

		/// <summary>
		/// Check for updates via AutoUpdaterNET
		/// </summary>
		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			AutoUpdater.Start("https://downloads.wolfsys.net/ConnectionMgr/update.xml");
		}
	}
}