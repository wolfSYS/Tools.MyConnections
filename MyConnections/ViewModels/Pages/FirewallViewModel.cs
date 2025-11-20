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

		[ObservableProperty]
		private IFirewallRule _currentSelection;

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

				var filteredRules = FirewallManager.Instance.Rules.Where(x => x.Name.Contains("#ConnectionMgr")).OrderBy(x => x.Name);
				foreach (var rule in filteredRules)
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

		private bool CanEnableRule(IFirewallRule rule)
		{
			if ((rule != null) && !rule.IsEnable)
				return true;

			return false;
		}

		private bool CanDisableRule(IFirewallRule rule)
		{
			if ((rule != null) && rule.IsEnable)
				return true;

			return false;
		}

		private bool CanDeleteRule(IFirewallRule rule)
		{
			if (rule != null) 
				return true;

			return false;
		}

		[RelayCommand(CanExecute = nameof(CanEnableRule))]
		private async Task EnableRule(IFirewallRule rule)
		{
			try
			{
				string displayRuleName = rule.Name.Replace("#ConnectionMgr", "");

				if (await ShowDialogYesNo("Enable Rule",
					$"Do you really want to enable the Firewall Rule '{displayRuleName}'?"))
				{
					rule.IsEnable = true;
					FirewallManager.Instance.Reload();

					_logger.Information($"Rule {displayRuleName}' has been enabled.");
					await GetFirewallRules();
					ShowInfo("Success", $"The Firewall Rule '{displayRuleName}' has been enabled again.");
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "FirewallVM::EnableRule");
				ShowError(ex);
			}
		}

		[RelayCommand(CanExecute = nameof(CanDisableRule))]
		private async Task DisableRule(IFirewallRule rule)
		{
			try
			{
				string displayRuleName = rule.Name.Replace("#ConnectionMgr", "");

				if (await ShowDialogYesNo("Disable Rule",
					$"Do you really want to disable the Firewall Rule '{displayRuleName}'?"))
				{
					rule.IsEnable = false;
					FirewallManager.Instance.Reload();

					_logger.Information($"Rule {displayRuleName}' has been disabled.");
					await GetFirewallRules();
					ShowInfo("Success", $"The Firewall Rule '{displayRuleName}' has been disabled.");
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "FirewallVM::DisableRule");
				ShowError(ex);
			}
		}

		[RelayCommand(CanExecute = nameof(CanDeleteRule))]
		private async Task DeleteRule(IFirewallRule rule)
		{
			try
			{
				string displayRuleName = rule.Name.Replace("#ConnectionMgr", "");

				if (await ShowDialogYesNo("Delete Rule",
					$"Do you really want to remove the Firewall Rule '{displayRuleName}'?\nThis action can not be undone."))
				{
					FirewallManager.Instance.Rules.Remove(rule);
					_logger.Information($"Rule '{displayRuleName}' removed from MS Windows Firewall.");
					await GetFirewallRules();
					ShowInfo("Removed successfully", $"The Rule '{displayRuleName}' has been removed successfully from the Windows Firewall.");
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "FirewallVM::DeleteRule");
				ShowError(ex);
			}
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