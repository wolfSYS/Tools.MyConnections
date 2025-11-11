using System;
using System.Net;
using System.Net.Sockets;

namespace ConnectionMgr.ExtensionMethods
{
	public static class IpAddressExtensions
	{
		/// <summary>
		/// Returns true if the address is *local* – loopback or private.
		/// </summary>
		public static bool IsLocal(this IPAddress ip) => !ip.IsPublic();

		/// <summary>
		/// Returns true if the address is a *public* address (not loopback, not private, not link‑local, not reserved, not IPv6‑ULA, etc.).
		/// </summary>
		public static bool IsPublic(this IPAddress ip)
		{
			// 1.  Loopback (127.0.0.1, ::1)
			if (IPAddress.IsLoopback(ip))
				return false;

			// 2.  IPv4
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				byte[] b = ip.GetAddressBytes();

				if (ip.ToString() == "0.0.0.0")
					return false;

				// 10.0.0.0/8
				if (b[0] == 10)
					return false;

				// 172.16.0.0/12
				if (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
					return false;

				// 192.168.0.0/16
				if (b[0] == 192 && b[1] == 168)
					return false;

				// 169.254.0.0/16 (Link‑local)
				if (b[0] == 169 && b[1] == 254)
					return false;

				// 100.64.0.0/10 (Carrier‑grade NAT)
				if (b[0] == 100 && (b[1] >= 64 && b[1] <= 127))
					return false;

				// 255.255.255.255  – broadcast
				if (b[0] == 255 && b[1] == 255 && b[2] == 255 && b[3] == 255)
					return false;
			}

			// 3.  IPv6
			if (ip.AddressFamily == AddressFamily.InterNetworkV6)
			{
				// ::1
				if (IPAddress.IsLoopback(ip))
					return false;

				if (ip.ToString() == "::")
					return false;

				// fe80::/10 (Link‑local)
				if (ip.IsIPv6LinkLocal)
					return false;

				// fc00::/7 (Unique Local Addresses, RFC 4193)
				if (ip.IsIPv6SiteLocal)
					return false;

				// ff00::/8 (Multicast)
				if (ip.IsIPv6Multicast)
					return false;

				// 2001:db8::/32 (Documentation, RFC 3849)
				if (ip.IsIPv6Teredo)
					return false; // Teredo also uses a reserved prefix
			}

			// 4.  Anything that survived the tests is public
			return true;
		}

		/// <summary>
		/// Get's the host name for the given IP Adr
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public static string GetHostName(this IPAddress ip)
		{
			if (ip.IsLocal())
			{
				return ip.ToString(); //string.Empty;
			}
			else
			{
				try
				{
					IPHostEntry hostEntry = Dns.GetHostEntry(ip);
					return hostEntry.HostName;
				}
				catch (Exception e) 
				{
					return ip.ToString();
				}
			}
		}
	}
}