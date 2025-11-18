using System.Windows.Controls;
using System.Windows.Threading;
using ConnectionMgr.Views.Dialogs;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace ConnectionMgr.ViewModels
{
	/// <summary>
	/// All VMs that need INavigationAware and Wpf.UI stuff (dialogs, snackbar, progress) inherit from this class.
	/// </summary>
	public abstract partial class PagesBaseViewModel : ObservableObject, INavigationAware
	{
		#region Services (injected once)

		protected readonly IContentDialogService _dialogService;
		protected readonly Interfaces.ILoggerService _logger;
		protected readonly ISnackbarService _snackbarService;

		#endregion Services (injected once)

		#region Common UI‑state

		/// <summary>
		/// Flag that the UI can bind to – “show a progress ring or not”.
		/// </summary>
		[ObservableProperty]
		private bool _showProgress = true;

		#endregion Common UI‑state

		#region Constructor – everything is forwarded to the base

		protected PagesBaseViewModel(
			Interfaces.ILoggerService logger,
			IContentDialogService dialogService,
			ISnackbarService snackbarService)
		{
			_logger = logger;
			_dialogService = dialogService;
			_snackbarService = snackbarService;
		}

		#endregion Constructor – everything is forwarded to the base

		#region Helper methods (protected – called from derived classes)

		/// <summary>
		/// Updates the <see cref="ShowProgress"/> flag from any thread. The UI will be updated automatically because the flag is an
		/// ObservableProperty in the base class.
		/// </summary>
		protected async Task SetProgressAsync(bool doShowProgress)
		{
			if (!Application.Current.Dispatcher.CheckAccess())
			{
				await Application.Current.Dispatcher.InvokeAsync(
					() => SetProgressAsync(doShowProgress), DispatcherPriority.Background);
				return;
			}

			ShowProgress = doShowProgress;
			await Dispatcher.Yield(DispatcherPriority.Background);
		}

		/// <summary>
		/// Shows a modal “yes / no” dialog (the same layout that the old code used).
		/// </summary>
		/// <returns>
		/// TRUE = yes | FALSE = no
		/// </returns>
		protected async Task<bool> ShowDialogYesNo(string title, string message)
		{
			var cd = new ContentDialog();
			cd.SetCurrentValue(ContentDialog.TitleProperty, title);
			cd.SetCurrentValue(ContentControl.ContentProperty, message);
			cd.SetCurrentValue(ContentDialog.SecondaryButtonTextProperty, "YES");
			cd.SetCurrentValue(ContentDialog.SecondaryButtonIconProperty,
							   new SymbolIcon(SymbolRegular.Checkmark24));
			cd.SetCurrentValue(ContentDialog.CloseButtonTextProperty, "NO");
			cd.SetCurrentValue(ContentDialog.CloseButtonIconProperty,
							   new SymbolIcon(SymbolRegular.Dismiss24));

			var dlgResult = await _dialogService.ShowAsync(cd, CancellationToken.None);
			return dlgResult == ContentDialogResult.Secondary;
		}

		/// <summary>
		/// Shows a snackbar “error” message.
		/// </summary>
		protected void ShowError(Exception ex)
		{
			_snackbarService.Show(
				"Error occured.",
				ex.Message,
				ControlAppearance.Danger,
				new SymbolIcon(SymbolRegular.ErrorCircle24),
				TimeSpan.FromSeconds(8));
		}

		/// <summary>
		/// Shows a snackbar “info” message.
		/// </summary>
		protected void ShowInfo(string title, string message)
		{
			_snackbarService.Show(
				title,
				message,
				ControlAppearance.Info,
				new SymbolIcon(SymbolRegular.Info24),
				TimeSpan.FromSeconds(8));

			_logger.Debug(message);
		}

		/// <summary>
		/// Shows a snackbar “info” message.
		/// </summary>
		protected void ShowWarning(string title, string message)
		{
			_snackbarService.Show(
				title,
				message,
				ControlAppearance.Caution,
				new SymbolIcon(SymbolRegular.Info24),
				TimeSpan.FromSeconds(8));

			_logger.Debug(message);
		}

		/// <summary>
		/// Shows a modal input dialog and returns the typed string (or empty if cancelled).
		/// </summary>
		protected async Task<string> ShowInputDialog(string title, string message, string defaultValue = "")
		{
			var dlgHost = _dialogService.GetDialogHost();           // you return the host from the dialog‑service
			var dlg = new InputDialog(dlgHost, title, message, defaultValue);
			var result = await dlg.ShowAsync();

			return result == ContentDialogResult.Primary
				   ? dlg.InputText
				   : string.Empty;
		}

		//protected async Task<string> ShowAiOverview(string title, string message)
		//{
		//	var dlgHost = _dialogService.GetDialogHost();           // you return the host from the dialog‑service
		//	var dlg = new AiOverview(dlgHost, title, message);
		//	var result = await dlg.ShowAsync();

		//	return result == ContentDialogResult.Primary
		//		   ? dlg.InputText
		//		   : string.Empty;
		//}


		#endregion Helper methods (protected – called from derived classes)

		#region Navigation hooks (virtual so that derived VMs can override)

		public bool Equals(PagesBaseViewModel other)
		{
			throw new NotImplementedException();
		}

		public abstract Task OnNavigatedFromAsync();

		public abstract Task OnNavigatedToAsync();

		#endregion Navigation hooks (virtual so that derived VMs can override)
	}
}