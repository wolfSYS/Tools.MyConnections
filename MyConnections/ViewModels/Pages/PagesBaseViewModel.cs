using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using MyConnections.Views.Dialogs;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace MyConnections.ViewModels
{
	/// <summary>
	/// All ViewModels that need dialog / snackbar / progress handling inherit from this class.
	/// </summary>
	public abstract partial class PagesBaseViewModel : ObservableObject, INavigationAware
	{
		#region Services (injected once)

		protected readonly IContentDialogService _dialogService;
		protected readonly Interfaces.ILoggerService _logger;
		protected readonly ISnackbarService _snackbarService;

		#endregion

		#region Common UI‑state

		/// <summary>Flag that the UI can bind to – “show a progress ring or not”.</summary>
		[ObservableProperty]  // <-- the code‑gen will create the *public* ShowProgress{get;set;}
		private bool _showProgress = true;

		#endregion

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

		#endregion

		#region Helper methods (protected – called from derived classes)

		/// <summary>Shows a snackbar “info” message.</summary>
		protected void ShowInfo(string title, string message)
		{
			_snackbarService.Show(
				title,
				message,
				ControlAppearance.Info,
				new SymbolIcon(SymbolRegular.Info24),
				TimeSpan.FromSeconds(8));
		}

		/// <summary>Shows a snackbar “error” message.</summary>
		protected void ShowError(Exception ex)
		{
			_snackbarService.Show(
				"Error occured.",
				ex.Message,
				ControlAppearance.Caution,
				new SymbolIcon(SymbolRegular.ErrorCircle24),
				TimeSpan.FromSeconds(8));
		}

		/// <summary>Shows a modal input dialog and returns the typed string (or empty if cancelled).</summary>
		protected async Task<string> ShowInputDialog(string title, string message, string defaultValue = "")
		{
			var dlgHost = _dialogService.GetDialogHost();           // you return the host from the dialog‑service
			var dlg = new InputDialog(dlgHost, title, message, defaultValue);
			var result = await dlg.ShowAsync();

			return result == ContentDialogResult.Primary
				   ? dlg.InputText
				   : string.Empty;
		}

		/// <summary>Shows a modal “yes / no” dialog (the same layout that the old code used).</summary>
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
		/// Updates the <see cref="ShowProgress"/> flag from any thread.
		/// The UI will be updated automatically because the flag is an
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

		#endregion

		#region Navigation hooks (virtual so that derived VMs can override)

		public abstract Task OnNavigatedFromAsync();
		public abstract Task OnNavigatedToAsync();

		public bool Equals(PagesBaseViewModel other)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}