using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectionMgr.Helpers;
using ConnectionMgr.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConnectionMgr.Services
{
	public class BackgroundConnectionService : IHostedService
	{
		private readonly ILogger<BackgroundConnectionService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly CancellationTokenSource _cts = new();
		private readonly object _lock = new();
		private List<NetworkConnectionInfo> _currentConnections = new();

		public BackgroundConnectionService(
			ILogger<BackgroundConnectionService> logger,
			IServiceProvider serviceProvider)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		public async Task StartAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var connections = await Task.Run(() => ConnectionCollector.GetAllOutgoingConnections(), stoppingToken);
					lock (_lock)
					{
						_currentConnections = connections;
					}
					// Notify UI to update
					OnConnectionsUpdated?.Invoke(this, _currentConnections);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error in background connection service");
				}

				// Wait for 5 seconds before next refresh
				await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
			}
		}

		public Task StopAsync(CancellationToken stoppingToken)
		{
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public event EventHandler<List<NetworkConnectionInfo>> OnConnectionsUpdated;

		public List<NetworkConnectionInfo> GetCurrentConnections()
		{
			lock (_lock)
			{
				return _currentConnections.ToList();
			}
		}
	}
}
