using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MyConnections.Models;

namespace MyConnections.Helpers
{
	public static class ConnectionCollector
	{
		private const int AF_INET = 2;    // IPv4

		/// <summary>
		/// Enumerates every outbound TCP/UDP connection on the machine.
		/// </summary>
		public static List<NetworkConnectionInfo> GetAllOutgoingConnections()
		{
			var result = new List<NetworkConnectionInfo>();

			// ----------------------- TCP connections -----------------------
			result.AddRange(GetTcpConnections());

			// ----------------------- UDP connections -----------------------
			result.AddRange(GetUdpConnections());

			return result.OrderBy(con => con.ProcessPath).ToList();
		}

		private static string GetProcessPath(int pid)
		{
			try
			{
				// Try the high‑level API first – it works for most processes
				var proc = Process.GetProcessById(pid);
				return proc.MainModule?.FileName ?? $"PID:{pid}";
			}
			catch
			{
				// If the process has already exited or we lack rights, fall back to native call
				var h = NativeMethods.OpenProcess(NativeMethods.ProcessAccessFlags.QueryLimitedInformation, false, pid);
				if (h == IntPtr.Zero)
					return $"PID:{pid}";
				try
				{
					var sb = new StringBuilder(260);
					int size = sb.Capacity;
					if (NativeMethods.QueryFullProcessImageName(h, 0, sb, ref size))
						return sb.ToString();
					return $"PID:{pid}";
				}
				finally
				{
					NativeMethods.CloseHandle(h);
				}
			}
		}

		private static List<NetworkConnectionInfo> GetTcpConnections()
		{
			var result = new List<NetworkConnectionInfo>();

			int buffSize = 0;
			int ret = NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, AF_INET,
				NativeMethods.TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

			if (ret != 122) // 122 == ERROR_INSUFFICIENT_BUFFER
				throw new Win32Exception(ret);

			var ptr = Marshal.AllocHGlobal(buffSize);
			try
			{
				ret = NativeMethods.GetExtendedTcpTable(ptr, ref buffSize, true, AF_INET,
					NativeMethods.TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

				if (ret != 0)
					throw new Win32Exception(ret);

				// The first DWORD is the number of entries
				int entries = Marshal.ReadInt32(ptr);
				IntPtr rowPtr = IntPtr.Add(ptr, sizeof(int));

				for (int i = 0; i < entries; i++)
				{
					var row = Marshal.PtrToStructure<NativeMethods.MIB_TCPROW_OWNER_PID>(rowPtr);

					// We only care about outbound sockets – those with a *remote* address
					if (row.remoteAddr == 0)   // 0.0.0.0 means listening or not yet connected
					{
						rowPtr = IntPtr.Add(rowPtr, Marshal.SizeOf<NativeMethods.MIB_TCPROW_OWNER_PID>());
						continue;
					}

					var info = new NetworkConnectionInfo
					{
						Protocol = "TCP",
						LocalAddress = new IPAddress(row.localAddr),
						LocalPort = (ushort)IPAddress.NetworkToHostOrder((short)row.localPort),
						RemoteAddress = new IPAddress(row.remoteAddr),
						RemotePort = (ushort)IPAddress.NetworkToHostOrder((short)row.remotePort),
						State = TcpStateToString(row.state),
						ProcessPath = GetProcessPath((int)row.owningPid)
					};

					result.Add(info);

					rowPtr = IntPtr.Add(rowPtr, Marshal.SizeOf<NativeMethods.MIB_TCPROW_OWNER_PID>());
				}
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
			return result;
		}

		private static List<NetworkConnectionInfo> GetUdpConnections()
		{
			var result = new List<NetworkConnectionInfo>();

			int buffSize = 0;
			int ret = NativeMethods.GetExtendedUdpTable(IntPtr.Zero, ref buffSize, true, AF_INET,
				NativeMethods.UdpTableClass.UDP_TABLE_OWNER_PID, 0);

			if (ret != 122)
				throw new Win32Exception(ret);

			var ptr = Marshal.AllocHGlobal(buffSize);
			try
			{
				ret = NativeMethods.GetExtendedUdpTable(ptr, ref buffSize, true, AF_INET,
					NativeMethods.UdpTableClass.UDP_TABLE_OWNER_PID, 0);

				if (ret != 0)
					throw new Win32Exception(ret);

				// First DWORD: number of entries
				int entries = Marshal.ReadInt32(ptr);
				IntPtr rowPtr = IntPtr.Add(ptr, sizeof(int));

				for (int i = 0; i < entries; i++)
				{
					var row = Marshal.PtrToStructure<NativeMethods.MIB_UDPROW_OWNER_PID>(rowPtr);

					// UDP is connectionless Unfortunately the API doesn't give the remote port/address for UDP, so we leave those fields empty.
					var info = new NetworkConnectionInfo
					{
						Protocol = "UDP",
						LocalAddress = new IPAddress(row.localAddr),
						LocalPort = (ushort)IPAddress.NetworkToHostOrder((short)row.localPort),
						RemoteAddress = IPAddress.None,
						RemotePort = 0,
						State = "",   // UDP has no TCP‑style state
						ProcessPath = GetProcessPath((int)row.owningPid)
					};

					result.Add(info);

					rowPtr = IntPtr.Add(rowPtr, Marshal.SizeOf<NativeMethods.MIB_UDPROW_OWNER_PID>());
				}
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}

			return result;
		}

		// -------------------------------------------------------------------- Helpers --------------------------------------------------------------------
		private static string TcpStateToString(uint state)
		{
			// MIB_TCP_STATE
			return state switch
			{
				1 => "CLOSED",
				2 => "LISTEN",
				3 => "SYN_SENT",
				4 => "SYN_RECEIVED",
				5 => "ESTABLISHED",
				6 => "FIN_WAIT_1",
				7 => "FIN_WAIT_2",
				8 => "CLOSE_WAIT",
				9 => "CLOSING",
				10 => "LAST_ACK",
				11 => "TIME_WAIT",
				12 => "DELETE_TCB",
				_ => $"UNKNOWN({state})"
			};
		}
	}
}