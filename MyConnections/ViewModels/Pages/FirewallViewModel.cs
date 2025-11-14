using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using ConnectionMgr.Helpers;
using ConnectionMgr.Models;
using ConnectionMgr.Properties;
using WindowsFirewallHelper;
using Wpf.Ui;

namespace ConnectionMgr.ViewModels.Pages
{
	public partial class FirewallViewModel : PagesBaseViewModel
	{
		private bool _isInitialized = false;

		[ObservableProperty]
		private ObservableCollection<IFirewallRule> _rules =
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
			GetFirewallRules();
			_isInitialized = true;
		}

		private async Task GetFirewallRules()
		{
			try
			{
				await SetProgressAsync(true);
				Rules.Clear();

				var filteresRules = FirewallManager.Instance.Rules.Where(x => x.Name.Contains("#ConnectionMgr")).OrderBy(x => x.Name);
				//var filteresRules = FirewallManager.Instance.Rules.OrderBy(x => x.Name);
				foreach (var rule in filteresRules)
					Rules.Add(rule);

				await SetProgressAsync(false);
			}
			catch (Exception ex)
			{
				await SetProgressAsync(false);
				_logger.Error(ex, "FirewallVM::GetFirewallRules");
				ShowError(ex);
			}
		}


		[RelayCommand]
		private async Task RefreshConnection()
		{
			//await RefreshRulesAsync();
		}

		[RelayCommand]
		private async Task OpenWindowsFirewall()
		{
			try
			{
				ProcessStartInfo psi = new()
				{
					FileName = "mmc.exe",
					Arguments = "wf.msc",
					UseShellExecute = true,
					Verb = "runas"
				};

				Process.Start(psi);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "FirewallVM::OpenWindowsFirewall");
				ShowError(ex);
			}
		}
	}
}