using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionMgr.Helpers
{
	/// <summary>
	/// Class for Native API wrappers
	/// </summary>
	/// <remarks>
	/// Run the executable as administrator or grant the app a "full control" UAC permission.
	/// </remarks>
	internal static class NativeMethods
	{
		// --------------------------------------------------------------------
		//  IP Helper API
		// --------------------------------------------------------------------
		internal enum TcpTableClass
		{
			TCP_TABLE_BASIC_LISTENER,
			TCP_TABLE_BASIC_CONNECTIONS,
			TCP_TABLE_BASIC_ALL,
			TCP_TABLE_OWNER_PID_LISTENER,
			TCP_TABLE_OWNER_PID_CONNECTIONS,
			TCP_TABLE_OWNER_PID_ALL,
			TCP_TABLE_OWNER_MODULE_LISTENER,
			TCP_TABLE_OWNER_MODULE_CONNECTIONS,
			TCP_TABLE_OWNER_MODULE_ALL
		}

		internal enum UdpTableClass
		{
			UDP_TABLE_BASIC,
			UDP_TABLE_OWNER_PID,
			UDP_TABLE_OWNER_MODULE
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MIB_TCPROW_OWNER_PID
		{
			public uint state;        // MIB_TCP_STATE
			public uint localAddr;    // IPv4
			public uint localPort;    // in network byte order
			public uint remoteAddr;   // IPv4
			public uint remotePort;   // in network byte order
			public uint owningPid;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MIB_TCP6ROW_OWNER_PID
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] localAddr;

			public ushort localPort;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] remoteAddr;

			public ushort remotePort;
			public uint state;
			public uint owningPid;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MIB_TCPROW_OWNER_MODULE
		{
			public uint state;
			public uint localAddr;
			public uint localPort;
			public uint remoteAddr;
			public uint remotePort;
			public uint owningPid;
			public uint owningModuleID;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MIB_UDPTABLE_OWNER_PID
		{
			public uint dwNumEntries;
			public MIB_UDPROW_OWNER_PID[] table;   // placeholder, we allocate dynamic memory
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MIB_UDPROW_OWNER_PID
		{
			public uint localAddr;   // IPv4
			public uint localPort;   // in network byte order
			public uint owningPid;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MIB_UDP6ROW_OWNER_PID
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] localAddr;

			public ushort localPort;
			public uint owningPid;
		}

		[DllImport("iphlpapi.dll", SetLastError = true)]
		internal static extern int GetExtendedTcpTable(
			IntPtr pTcpTable,
			ref int dwOutBufLen,
			bool sort,
			int ipVersion,
			TcpTableClass tblClass,
			uint reserved);

		[DllImport("iphlpapi.dll", SetLastError = true)]
		internal static extern int GetExtendedUdpTable(
			IntPtr pUdpTable,
			ref int dwOutBufLen,
			bool sort,
			int ipVersion,
			UdpTableClass tblClass,
			uint reserved);

		// --------------------------------------------------------------------
		//  Process API – for the full executable path
		// --------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern bool QueryFullProcessImageName(
			IntPtr hProcess,
			int dwFlags,
			System.Text.StringBuilder lpExeName,
			ref int lpdwSize);

		[Flags]
		internal enum ProcessAccessFlags : uint
		{
			QueryLimitedInformation = 0x1000
		}
	}
}