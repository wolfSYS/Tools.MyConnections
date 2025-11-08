using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace ConnectionMgr.Interfaces
{
	public interface ILoggerService
	{
		public void Debug(string message);

		public void Error(Exception theException, string AddInfo);

		public void Fatal(Exception theException, string AddInfo);

		public void Information(string message);

		public void SwitchLoggerState(bool enableDebugInformation);

		public void Warning(string message);

		public void Warning(Exception theException, string AddInfo);

		public void InitLoggerDebug();
		public void InitLoggerWarning();
	}
}