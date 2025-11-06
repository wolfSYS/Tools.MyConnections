using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using MyConnections.Helpers;
using MyConnections.Models;
using MyConnections.Properties;
using WindowsFirewallHelper;
using Wpf.Ui;

namespace MyConnections.ViewModels.Pages
{
	public partial class FirewallViewModel : PagesBaseViewModel
	{
		private bool _isInitialized = false;

		[ObservableProperty]
		private ObservableCollection<IFirewallRule> _rules  =
			new ObservableCollection<IFirewallRule>();



		public FirewallViewModel(
					Interfaces.ILoggerService logger,
					IContentDialogService dialogService,
					ISnackbarService snackbarService)
					: base(logger, dialogService, snackbarService)
		{
			// BaseVM will care about DY
		}

		public override Task OnNavigatedFromAsync()
		{
			Rules.Clear();
			_isInitialized = false;

			return Task.CompletedTask;
		}

		public override Task OnNavigatedToAsync()
		{
			if (!_isInitialized)
				InitializeViewModel();

			return Task.CompletedTask;
		}

		private void InitializeViewModel()
		{
			RefreshRulesAsync();
			_isInitialized = true;
		}

		private async Task RefreshRulesAsync()
		{
			try
			{
				await SetProgressAsync(true);
				Rules.Clear();

				foreach (var r in FirewallManager.Instance.Rules)
					Rules.Add(r);
			}
			catch (Exception ex) 
			{
				_logger.Error(ex, "FirewallVM::RefreshRules");
				ShowError(ex);
			}
			finally
			{
				await SetProgressAsync(false);
			}
		}

		[RelayCommand]
		private async Task RefreshConnection()
		{
			await RefreshRulesAsync();
		}
	}
}