using System.Diagnostics.Eventing.Reader;
using System.Net;

namespace ConnectionMgr.Models
{
	public class NetworkConnectionInfo
	{
		public IPAddress? LocalAddress
		{
			get; set;
		}

		public int LocalPort
		{
			get; set;
		}

		/// <summary>
		/// Full path to the .exe
		/// </summary>
		public string? ProcessPath
		{
			get; set;
		}

		public string? NormalizedProcessPath
		{
			get
			{
				if (!string.IsNullOrEmpty(ProcessPath))
				{
					if (string.Equals(ProcessPath, "PID:0", StringComparison.OrdinalIgnoreCase))
						return ProcessPath + " (System Idle Process)";
					else if (string.Equals(ProcessPath, "PID:4", StringComparison.OrdinalIgnoreCase))
						return ProcessPath + " (Windows Kernel)";
					else
						return ProcessPath;
				}
				else
					return ProcessPath;
			}
		}

		/// <summary>
		/// TCP or UDP
		/// </summary>
		public string? Protocol
		{
			get; set;
		}

		public IPAddress? RemoteAddress
		{
			get; set;
		}

		public int RemotePort
		{
			get; set;
		}

		public string RemoteConnection
		{
			get
			{
				return RemoteAddress?.ToString() + " : " + RemotePort.ToString();
			}
		}

		public string LocalConnection
		{
			get
			{
				return LocalAddress?.ToString() + " : " + LocalPort.ToString();
			}
		}

		public string StateProtInfo
		{
			get
			{
				string ret = Protocol;
				if (!string.IsNullOrEmpty(State))
					ret += " / " + State;

				return ret;
			}
		}

		/// <summary>
		/// “Established”, “Close‑wait”, etc.
		/// </summary>
		public string? State
		{
			get; set;
		}
	}
}