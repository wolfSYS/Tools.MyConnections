using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using ConnectionMgr.Interfaces;

namespace ConnectionMgr.Converters
{
	/// <summary>
	/// Turns the full path to an executable into the icon that Windows associates with that file.
	/// </summary>
	public sealed class ProcessPathToIconConverter : IValueConverter
	{
		// simple caching for avoiding duplicate ICO allocations
		private static readonly Dictionary<string, BitmapSource> _iconCache = new();

		// get access to logger class
		private static ILoggerService _logger => App.Services.GetRequiredService<ILoggerService>();

		// Win32 API: SHGetFileInfo
		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr SHGetFileInfo(
			string pszPath,
			uint dwFileAttributes,
			out SHFILEINFO psfi,
			uint cbSizeFileInfo,
			uint uFlags);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public IntPtr iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		// Flags for SHGetFileInfo
		private const uint SHGFI_ICON = 0x000000100;
		private const uint SHGFI_LARGEICON = 0x000000000;   // 32×32 (default)
		private const uint SHGFI_SMALLICON = 0x000000001;   // 16×16

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string? path = value as string;
			if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
				return null;

			try
			{
				// Return cached icon if we already extracted it
				if (_iconCache.TryGetValue(path, out var cached))
					return cached;

				// Ask Windows for the icon that belongs to the file
				SHFILEINFO sfi;
				IntPtr hInfo = SHGetFileInfo(path, 0, out sfi, (uint)Marshal.SizeOf<SHFILEINFO>(), SHGFI_ICON | SHGFI_LARGEICON);
				if (hInfo == IntPtr.Zero || sfi.hIcon == IntPtr.Zero)
					return null;

				// Convert HICON to a BitmapSource WPF can use
				BitmapSource image = Imaging.CreateBitmapSourceFromHIcon(
					sfi.hIcon,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());

				// IMPORTANT – the HICON returned by SHGetFileInfo must be destroyed
				DestroyIcon(sfi.hIcon);

				// Store new ICO in cache before returning
				_iconCache[path] = image;

				return image;
			}
			catch (Exception ex)
			{
				_logger.Warning(ex, "ProcessPathToIconConverter::Convert");
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		// Free the icon handle
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool DestroyIcon(IntPtr hIcon);
	}
}