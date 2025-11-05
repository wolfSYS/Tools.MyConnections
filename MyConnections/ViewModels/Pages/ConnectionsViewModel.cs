using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using MyConnections.Helpers;
using MyConnections.Interfaces;
using MyConnections.Models;
using MyConnections.Services;
using Serilog.Configuration;
using Windows.System;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace MyConnections.ViewModels.Pages
{
	public partial class ConnectionsViewModel : ObservableObject, INavigationAware
	{
		private readonly Interfaces.ILoggerService _logger;
		private bool _isInitialized = false;

		[ObservableProperty]
		private ObservableCollection<NetworkConnectionInfo> _connections =
			new ObservableCollection<NetworkConnectionInfo>();

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
		private NetworkConnectionInfo _currentSelection;

		[ObservableProperty]
		private bool _showProgress = true;

		public ConnectionsViewModel(Interfaces.ILoggerService logger)
		{
			_logger = logger;
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
				if (info.NormalizedProcessPath == "SYSTEM")
					return false;
				else
					return true;
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
		private async Task FirewallDummy(NetworkConnectionInfo info)
		{

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
				return false; // treat it as “not a match”
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
				}
				finally
				{
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
				var conns = await Task.Run(() => ConnectionCollector.GetAllOutgoingConnections());

				foreach (var c in conns)
					Connections.Add(c);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "ConnectionsVM::RefreshConnectionsAsync");
			}
			finally
			{
				OnPropertyChanged(nameof(Connections));
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
	}
}