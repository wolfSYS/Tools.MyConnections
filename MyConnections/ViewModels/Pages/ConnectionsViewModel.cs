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
	public partial class ConnectionsViewModel : PagesBaseViewModel
	{
		[ObservableProperty]
		private ObservableCollection<NetworkConnectionInfo> _connections =
			new ObservableCollection<NetworkConnectionInfo>();

		[ObservableProperty]
		private NetworkConnectionInfo _currentSelection;

		private bool _isInitialized = false;

		public ConnectionsViewModel(
					Interfaces.ILoggerService logger,
					IContentDialogService dialogService,
					ISnackbarService snackbarService)
					: base(logger, dialogService, snackbarService)
		{
			// BaseVM will care about DY
		}

		public override Task OnNavigatedFromAsync()
		{
			CurrentSelection = null;
			Connections.Clear();
			_isInitialized = false;

			return Task.CompletedTask;
		}

		public override Task OnNavigatedToAsync()
		{
			if (!_isInitialized)
				InitializeViewModel();

			return Task.CompletedTask;
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

		[RelayCommand(CanExecute = nameof(CanShowFirewall))]
		private async Task FirewallBlockPort(NetworkConnectionInfo info)
		{
			try
			{
				if (await FirewallConfirmWarning())
				{
					var portNr = info.LocalPort;
					var ruleName = await ShowInputDialog("Rule Name",
						"Enter a unique name for the new rule that should be added to Windows Firewall.",
						$"BLOCK LOCAL PORT {portNr}");

					if (!string.IsNullOrEmpty(ruleName))
					{
						var rule = FirewallManager.Instance.CreatePortRule(
							ruleName,
							FirewallAction.Block,
							(ushort)portNr,
							FirewallProtocol.Any
						);
						FirewallManager.Instance.Rules.Add(rule);

						_logger.Information($"Added rule '{ruleName}' for local port '{portNr}' to the Windows Firewall.");
						ShowInfo("Sucess", $"New rule for blocking local port {portNr} added to Windows Firewall.");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "ConnectionVM::FirewallBlockPort");
				ShowError(ex);
			}
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

							_logger.Information($"Added rule '{ruleName}' for process '{exe}' to the Windows Firewall.");
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

		[RelayCommand(CanExecute = nameof(CanShowFirewall))]
		private void FirewallDummy(NetworkConnectionInfo info)
		{
			// nothing to see|do here^^
		}

		/// <summary>
		/// Does a rule with this name already exist in windows firewall?
		/// </summary>
		private bool FirewallRuleAlreadyExists(string ruleName)
		{
			var myRule = FirewallManager.Instance.Rules.SingleOrDefault(r => r.Name == ruleName);
			if (myRule != null)
			{
				_logger.Warning($"The Windows Firewall rule '{ruleName}' already exists and can't be added a second time.");
				ShowError(new Exception($"There already exists a rule with the name {ruleName}, please choose another name."));
				return true;
			}
			return false;
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

		[RelayCommand(CanExecute = nameof(CanShowDetails))]
		private async Task ShowDetails(NetworkConnectionInfo info)
		{
		}
	}
}