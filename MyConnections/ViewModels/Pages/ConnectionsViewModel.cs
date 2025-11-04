using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using MyConnections.Helpers;
using MyConnections.Interfaces;
using MyConnections.Models;
using MyConnections.Services;
using Serilog.Configuration;
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
				Connections.Clear();

				// Run the enumeration on a thread pool thread – the API is blocking
				var conns = await Task.Run(() => ConnectionCollector.GetAllOutgoingConnections());

				foreach (var conn in conns)
					Connections.Add(conn);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "MainVM::RefreshConnectionsAsync");
			}
			finally
			{
				//SetProgress(false);
			}
		}

		[RelayCommand]
        private async Task RefreshConnection()
        {
            await RefreshConnectionsAsync();
        }

	}
}
