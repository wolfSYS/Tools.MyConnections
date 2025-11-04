using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using MyConnections.Helpers;
using MyConnections.Models;
using MyConnections.Services;
using MyConnections.Interfaces;
using Serilog.Configuration;

namespace MyConnections.ViewModels.Pages
{
    public partial class ConnectionsViewModel : ObservableObject
    {
		private readonly Interfaces.ILoggerService _logger;

		[ObservableProperty]
        private int _counter = 0;

		[ObservableProperty]
		private List<NetworkConnectionInfo> _connections = new List<NetworkConnectionInfo>();

		public ConnectionsViewModel(Interfaces.ILoggerService logger)
		{
			RefreshConnectionsAsync();
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
