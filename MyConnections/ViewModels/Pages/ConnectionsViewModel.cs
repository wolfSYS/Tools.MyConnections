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

		private ObservableCollection<NetworkConnectionInfo> Connections { get; }
			= new ObservableCollection<NetworkConnectionInfo>();

		public ConnectionsViewModel(Interfaces.ILoggerService logger)
		{
			_logger = logger;
			_logger.Debug("Hello");
			string x = "";
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
        private void OnCounterIncrement()
        {
            Counter++;
        }
    }
}
