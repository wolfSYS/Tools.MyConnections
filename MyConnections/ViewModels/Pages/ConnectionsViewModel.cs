using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using MyConnections.Helpers;
using MyConnections.Interfaces;
using MyConnections.Models;
using MyConnections.Properties;
using MyConnections.Services;
using MyConnections.Views.Dialogs;
using Serilog.Configuration;
using Windows.System;
using WindowsFirewallHelper;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace MyConnections.ViewModels.Pages
{
	public partial class ConnectionsViewModel : ObservableObject, INavigationAware
	{
		private readonly IContentDialogService _dialogService;
		private readonly Interfaces.ILoggerService _logger;
		private readonly ISnackbarService _snackbarService;

		[ObservableProperty]
		private ObservableCollection<NetworkConnectionInfo> _connections =
			new ObservableCollection<NetworkConnectionInfo>();

		[ObservableProperty]
		private NetworkConnectionInfo _currentSelection;

		private bool _isInitialized = false;

		[ObservableProperty]
		private bool _showProgress = true;

		public ConnectionsViewModel(Interfaces.ILoggerService logger, IContentDialogService dialogService, ISnackbarService snackbarService)
		{
			_logger = logger;
			_dialogService = dialogService;
			_snackbarService = snackbarService;
		}

		Task INavigationAware.OnNavigatedFromAsync()
		{
			return Task.CompletedTask;
		}

		public async Task<Task> OnNavigatedToAsync()
		{
			if (!_isInitialized)
				await InitializeViewModel();

			return Task.CompletedTask;
		}

		Task INavigationAware.OnNavigatedToAsync()
		{
			return OnNavigatedToAsync();
		}

		private bool CanKillProcess(NetworkConnectionInfo info)
		{
			if (info != null)
			{
				return !info.NormalizedProcessPath.StartsWith("PID:");
			}
			else
				return false;
		}

		private bool CanShowDetails(NetworkConnectionInfo info)
		{
			return info != null ? true : false;
		}

		private bool CanShowFirewall(NetworkConnectionInfo info)
		{
			return info != null ? true : false;
		}

		[RelayCommand(CanExecute = nameof(CanKillProcess))]
		private async Task FirewallBlockProcess(NetworkConnectionInfo info)
		{
			try
			{
				var exe = Path.GetFileName(info.ProcessPath);
				if (!string.IsNullOrEmpty(exe) && await FirewallConfirmWarning())
				{
					var ruleName = await ShowInputDialog("Rule Name",
						"Enter a unique name for the new rule that should be added to Windows Firewall.",
						$"BLOCK {exe}");
					if (!string.IsNullOrEmpty(ruleName))
					{
						if (!FirewallRuleAlreadyExists(ruleName))
						{
							var rule = FirewallManager.Instance.CreateApplicationRule(
								ruleName,
								FirewallAction.Block,
								info.ProcessPath
							);
							rule.Direction = FirewallDirection.Outbound;
							FirewallManager.Instance.Rules.Add(rule);

							ShowInfo("Sucess", $"New rule for blocking {exe} added to Windows Firewall.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "ConnectionVM::FirewallBlockProcess");
				ShowError(ex);
			}
		}

		[RelayCommand(CanExecute = nameof(CanShowFirewall))]
		private async Task FirewallBlockConnection(NetworkConnectionInfo info)
		{
			if (await FirewallConfirmWarning())
			{

			}
		}

		private async Task<bool> FirewallConfirmWarning()
		{
			int nrOfWarnings = Settings.Default.NrOfConfirmGeneralWarningsFW;
			if (nrOfWarnings < 3)
			{
				Settings.Default.NrOfConfirmGeneralWarningsFW = nrOfWarnings + 1;
				Settings.Default.Save();

				return await ShowDialogYesNo("Important",
					"Altering the settings for Windows Firewall could result in unwanted results\nunless you're absolutely shure what you are doing.\n\nDo you really want to contine and add a new firewall rule?");
			}
			return true;
		}

		/// <summary>
		/// Does a rule with this name already exist in windows firewall?
		/// </summary>
		private bool FirewallRuleAlreadyExists(string ruleName)
		{
			var myRule = FirewallManager.Instance.Rules.SingleOrDefault(r => r.Name == ruleName);
			if (myRule != null)
			{
				ShowError(new Exception($"There already exists a rule with the name {ruleName}, please choose another name."));
				return true;
			}
			return false;
		}


		[RelayCommand(CanExecute = nameof(CanShowFirewall))]
		private void FirewallDummy(NetworkConnectionInfo info)
		{
			// nothing to see|do here^^
		}

		private async Task InitializeViewModel()
		{
			await RefreshConnectionsAsync();
			_isInitialized = true;
		}

		private bool IsSameExecutable(Process proc, string exePath)
		{
			try
			{
				return string.Equals(proc.MainModule?.FileName,
									 exePath,
									 StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception ex)
			{
				_logger.Debug($"ConnectionsVM:IsSameExecutable => {ex.Message}");
				return false; // treat it as "not a match"
			}
		}

		[RelayCommand(CanExecute = nameof(CanKillProcess))]
		private async Task KillProcess(NetworkConnectionInfo info)
		{
			if (info != null)
			{
				var exe = info.ProcessPath;
				var exe2 = Path.GetFileName(exe);
				var ok = false;

				try
				{
					if (string.IsNullOrWhiteSpace(exe))
						return;

					await SetProgressAsync(true);

					var procs = Process.GetProcesses()
						.Where(p => IsSameExecutable(p, exe))
						.ToArray();

					if (procs.Length > 0)
					{
						CurrentSelection = null;
						OnPropertyChanged(nameof(CurrentSelection));
						Connections.Clear();

						foreach (var proc in procs)
						{
							try
							{
								proc.CloseMainWindow();
								proc.Kill();
								await SetProgressAsync(true);
								proc.WaitForExit();
							}
							catch (Exception ex)
							{
								_logger.Warning(ex, $"ConnectionsVM:KillProcess({exe2}) => could not kill PID {proc.Id}");
							}
						}
						procs = null;
						ok = true;
					}
				}
				catch (Exception ex)
				{
					_logger.Error(ex, $"ConnectionsVM:KillProcess({exe2})");
					ShowError(ex);
				}
				finally
				{
					if (ok)
						ShowInfo("Send the KILL signal.", "However, not all processes can be killed, even as Administrator.");

					await RefreshConnectionsAsync();
				}
			}
		}

		[RelayCommand]
		private async Task RefreshConnection()
		{
			await RefreshConnectionsAsync();
		}

		private async Task RefreshConnectionsAsync()
		{
			await SetProgressAsync(true);
			try
			{
				CurrentSelection = null;
				OnPropertyChanged(nameof(CurrentSelection));
				Connections.Clear();

				// Run the enumeration on a thread pool thread – the API is blocking
				//var conns = await Task.Run(() => ConnectionCollector.GetAllOutgoingConnections());

				var conns = ConnectionCollector.GetAllOutgoingConnections();

				await SetProgressAsync(true);

				foreach (var c in conns)
					Connections.Add(c);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "ConnectionsVM::RefreshConnectionsAsync");
				ShowError(ex);
			}
			finally
			{
				await SetProgressAsync(false);
			}
		}

		private async Task SetProgressAsync(bool doShowProgress)
		{
			// make sure we’re on the UI thread
			if (!Application.Current.Dispatcher.CheckAccess())
			{
				await Application.Current.Dispatcher.InvokeAsync(
					() => SetProgressAsync(doShowProgress), DispatcherPriority.Background);
				return;
			}

			// change the property and force the UI to repaint
			ShowProgress = doShowProgress;

			// a minimal amount of work to guarantee the UI sees the change
			await Dispatcher.Yield(DispatcherPriority.Background);
		}

		[RelayCommand(CanExecute = nameof(CanShowDetails))]
		private async Task ShowDetails(NetworkConnectionInfo info)
		{
		}

		/// <summary>
		/// Displays a Yes/No dialog
		/// </summary>
		/// <returns>TRUE for "yes" and FALSE for "no"</returns>
		private async Task<bool> ShowDialogYesNo(string title, string message)
		{
			var contentDialog = new ContentDialog();

			contentDialog.SetCurrentValue(ContentDialog.TitleProperty, title);
			contentDialog.SetCurrentValue(ContentControl.ContentProperty, message);
			contentDialog.SetCurrentValue(ContentDialog.SecondaryButtonTextProperty, "YES");
			contentDialog.SetCurrentValue(ContentDialog.SecondaryButtonIconProperty, new SymbolIcon(SymbolRegular.Checkmark24));
			contentDialog.SetCurrentValue(ContentDialog.CloseButtonTextProperty, "NO");
			contentDialog.SetCurrentValue(ContentDialog.CloseButtonIconProperty, new SymbolIcon(SymbolRegular.Dismiss24));

			// Pass CancellationToken.None as required by the interface
			var dlgResult = await _dialogService.ShowAsync(contentDialog, CancellationToken.None);
			return dlgResult == ContentDialogResult.Secondary;
		}

		private void ShowInfo(string title, string message)
		{
			_snackbarService.Show(
				title,
				message,
				ControlAppearance.Info,
				new SymbolIcon(SymbolRegular.Info24),
				TimeSpan.FromSeconds(8));
		}

		private void ShowError(Exception ex)
		{
			_snackbarService.Show(
				"Error occured.",
				ex.Message,
				ControlAppearance.Caution,
				new SymbolIcon(SymbolRegular.ErrorCircle24),
				TimeSpan.FromSeconds(8));
		}

		private async Task<string> ShowInputDialog(string title, string message, string defaultValue = "")
		{
			var dlgHost = _dialogService.GetDialogHost();
			var dlg = new InputDialog(dlgHost, title, message, defaultValue);

			var res = await dlg.ShowAsync();

			if (res == ContentDialogResult.Primary)
				return dlg.InputText;
			else
				return string.Empty;
		}
	}
}