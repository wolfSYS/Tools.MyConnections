using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using WindowsFirewallHelper;
using Wpf.Ui;

namespace ConnectionMgr.ViewModels.Pages
{
	/// <summary> Provides the ViewModel logic for the <see cref="ConnectionMgr.Views.Pages.FirewallPage"> view. </summary> <remarks> <para>
	/// This ViewModel derives from <see cref="PagesBaseViewModel"/> and uses <see
	/// cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject"/> to raise <see cref="INotifyPropertyChanged.PropertyChanged"/>
	/// automatically. </para><para> The class is marked <c>partial</c> so that the CommunityToolkit.Mvvm source generators can add the
	/// backing fields for <see cref="[ObservableProperty]"/> attributes. </para> </remarks>
	public partial class FirewallViewModel : PagesBaseViewModel
	{
		[ObservableProperty]
		private IFirewallRule? _currentSelection;

		[ObservableProperty]
		private ObservableCollection<IFirewallRule> _rules =
			new ObservableCollection<IFirewallRule>();

		private bool _isInitialized = false;

		public FirewallViewModel(
					Interfaces.ILoggerService logger,
					IContentDialogService dialogService,
					ISnackbarService snackbarService)
					: base(logger, dialogService, snackbarService)
		{
			// BaseVM will care about DY
		}

		public event Action? RulesRefreshed;

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

		private bool CanDeleteRule(IFirewallRule rule)
		{
			if (rule != null)
				return true;

			return false;
		}

		private bool CanDisableRule(IFirewallRule rule)
		{
			if ((rule != null) && rule.IsEnable)
				return true;

			return false;
		}

		private bool CanEnableRule(IFirewallRule rule)
		{
			if ((rule != null) && !rule.IsEnable)
				return true;

			return false;
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
					var nameToRemove = rule.Name;
					var match = FirewallManager.Instance.Rules.FirstOrDefault(r => r.Name == nameToRemove);
					if (match != null)
						FirewallManager.Instance.Rules.Remove(match);

					await GetFirewallRules();
					App.Current.Dispatcher.Invoke(() => CurrentSelection = null);

					_logger.Information($"Rule '{displayRuleName}' removed from MS Windows Firewall.");
					ShowInfo("Removed successfully", $"The Rule '{displayRuleName}' has been removed successfully from the Windows Firewall.");
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "FirewallVM::DeleteRule");
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
					var ruleToDisable = rule.Name;
					var match = FirewallManager.Instance.Rules.FirstOrDefault(r => r.Name == ruleToDisable);
					if (match != null)
						match.IsEnable = false;

					await GetFirewallRules();
					App.Current.Dispatcher.Invoke(() => CurrentSelection = null);

					_logger.Information($"Rule {displayRuleName}' has been disabled.");
					ShowInfo("Success", $"The Firewall Rule '{displayRuleName}' has been disabled.");
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "FirewallVM::DisableRule");
				ShowError(ex);
			}
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
					var ruleToEnable = rule.Name;
					var match = FirewallManager.Instance.Rules.FirstOrDefault(r => r.Name == ruleToEnable);
					if (match != null)
						match.IsEnable = true;

					await GetFirewallRules();
					App.Current.Dispatcher.Invoke(() => CurrentSelection = null);

					_logger.Information($"Rule {displayRuleName}' has been enabled.");
					ShowInfo("Success", $"The Firewall Rule '{displayRuleName}' has been enabled again.");
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "FirewallVM::EnableRule");
				ShowError(ex);
			}
		}

		private async Task GetFirewallRules()
		{
			try
			{
				await SetProgressAsync(true);
				Rules.Clear();

				var filteredRules = FirewallManager.Instance.Rules.Where(x => x.Name.Contains("#ConnectionMgr")).OrderBy(x => x.Name).ToList();
				App.Current.Dispatcher.Invoke(() =>
				{
					Rules.Clear();
					foreach (var rule in filteredRules)
						Rules.Add(rule);

					CurrentSelection = null;
				});

				// Rules collection changed => tell commands to re-evaluate their CanExecute
				EnableRuleCommand.NotifyCanExecuteChanged();
				DisableRuleCommand.NotifyCanExecuteChanged();
				DeleteRuleCommand.NotifyCanExecuteChanged();

				await SetProgressAsync(false);
			}
			catch (Exception ex)
			{
				await SetProgressAsync(false);
				_logger.Error(ex, "FirewallVM::GetFirewallRules");
				ShowError(ex);
			}
		}

		private void InitializeViewModel()
		{
			GetFirewallRules();
			_isInitialized = true;
		}

		[RelayCommand]
		private Task OpenWindowsFirewall()
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

			return Task.CompletedTask;
		}

		partial void OnCurrentSelectionChanged(IFirewallRule? value)
		{
			EnableRuleCommand.NotifyCanExecuteChanged();
			DisableRuleCommand.NotifyCanExecuteChanged();
			DeleteRuleCommand.NotifyCanExecuteChanged();
		}
	}
}