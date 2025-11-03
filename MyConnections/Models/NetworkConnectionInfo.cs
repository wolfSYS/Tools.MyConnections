using System.Net;

namespace MyConnections.Models
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

		/// <summary>
		/// “Established”, “Close‑wait”, etc.
		/// </summary>
		public string? State
		{
			get; set;
		}
	}
}