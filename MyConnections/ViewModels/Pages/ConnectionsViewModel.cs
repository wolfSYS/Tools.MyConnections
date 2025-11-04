using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
        private int _counter = 0;

		[ObservableProperty]
		private List<NetworkConnectionInfo> _connections = new List<NetworkConnectionInfo>();

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
		private NetworkConnectionInfo _currentSelection;

		public ConnectionsViewModel(Interfaces.ILoggerService logger)
		{
			_logger = logger;
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

		Task INavigationAware.OnNavigatedFromAsync()
		{
			return Task.CompletedTask;
		}

		private async Task InitializeViewModel()
		{
			await RefreshConnectionsAsync();
			_isInitialized = true;
		}

		private async Task RefreshConnectionsAsync()
		{
			//SetProgress(true);
			try
			{
				CurrentSelection = null;
				OnPropertyChanged(nameof(CurrentSelection));
				Connections.Clear();
				Connections = new List<NetworkConnectionInfo>();
				OnPropertyChanged(nameof(Connections));

				// Run the enumeration on a thread pool thread – the API is blocking
				var conns = await Task.Run(() => ConnectionCollector.GetAllOutgoingConnections());

				Connections.AddRange(conns);
				OnPropertyChanged(nameof(Connections));
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "ConnectionsVM::RefreshConnectionsAsync");
			}
			finally
			{
				//SetProgress(false);
				string x = "";
			}
		}

		[RelayCommand]
        private async Task RefreshConnection()
        {
			await RefreshConnectionsAsync();
		}

		[RelayCommand(CanExecute = nameof(CanKillProcess))]
		private async Task KillProcess(NetworkConnectionInfo info)
		{
			if (info != null)
			{
				var exe = info.ProcessPath;
				try
				{
					if (string.IsNullOrWhiteSpace(exe))
						return;

					var procs = Process.GetProcesses()
						.Where(p => IsSameExecutable(p, exe))
						.ToArray();

					CurrentSelection = null;
					OnPropertyChanged(nameof(CurrentSelection));

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
							_logger.Warning(ex, $"ConnectionsVM:KillProcess({exe}) => could not kill PID {proc.Id}");
						}
					}
					await RefreshConnection();
				}
				catch (Exception ex)
				{
					_logger.Error(ex, $"ConnectionsVM:KillProcess({exe})");
				}
			}
		}

		[RelayCommand(CanExecute = nameof(CanShowDetails))]
		private async Task ShowDetails(NetworkConnectionInfo info)
		{
			//
		}

		[RelayCommand(CanExecute = nameof(CanShowFirewall))]
		private async Task FirewallDummy(NetworkConnectionInfo info)
		{
			//
		}

		private bool CanShowFirewall(NetworkConnectionInfo info)
		{
			return info != null ? true : false;
		}

		private bool CanShowDetails(NetworkConnectionInfo info)
		{
			return info != null ? true : false;
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

		private bool IsSameExecutable(Process proc, string exePath)
		{
			try
			{
				// Null‑conditional (`?.`) keeps us from a null‑ref if MainModule is null
				return string.Equals(proc.MainModule?.FileName,
									 exePath,
									 StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception ex) // catch *any* exception from accessing the process
			{
				_logger.Debug($"ConnectionsVM:IsSameExecutable => {ex.Message}");
				return false; // treat it as “not a match”
			}
		}

	}
}
