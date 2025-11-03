using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using MyConnections.Interfaces;
using ILoggerService = MyConnections.Interfaces.ILoggerService;

namespace MyConnections.Services
{
	/// <summary>
	/// Singleton for Logging
	/// </summary>
	public sealed class LoggerService : ILoggerService, IDisposable
	{
		private Serilog.Core.Logger log;

		/// <summary>
		/// CTor
		/// </summary>
		public LoggerService()
		{
			this.InitLoggerDebug();
		}

		public void Debug(string message)
		{
			log.Debug(message);
		}

		public void Dispose()
		{
			if (log != null)
				log.Dispose();
		}

		public void Error(Exception theException, string AddInfo)
		{
			log.Error(theException, AddInfo);
		}

		public void Fatal(Exception theException, string AddInfo)
		{
			log.Fatal(theException, AddInfo);
		}

		public void Information(string message)
		{
			log.Information(message);
		}

		public void SwitchLoggerState(bool enableDebugInformation)
		{
			Dispose();

			if (enableDebugInformation)
				InitLoggerDebug();
			else
				InitLoggerWarning();
		}

		public void Warning(string message)
		{
			log.Warning(message);
		}

		public void Warning(Exception theException, string AddInfo)
		{
			log.Warning(theException, AddInfo);
		}


		public void InitLoggerDebug()
		{
			var logFilePath = "log.txt"; // ApplicationData.Current.LocalFolder.Path + @"\Log\log.txt";
			log = new LoggerConfiguration()
						.MinimumLevel.Debug()
						.WriteTo.Console()
						.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
						.CreateLogger();
		}

		public void InitLoggerWarning()
		{
			var logFilePath = "log.txt"; // ApplicationData.Current.LocalFolder.Path + @"\Log\log.txt";
			log = new LoggerConfiguration()
						.MinimumLevel.Warning()
						.WriteTo.Console()
						.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
						.CreateLogger();
		}
	}
}