using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsFirewallHelper;

namespace ConnectionMgr.Converters
{
	/// <summary>
	/// Converter for displaying enabled/disabled state of a Firewall Rule on the DataGrid
	/// </summary>
	public class FirewallEnabledStateToImage : IValueConverter
	{
		// Lazy‑load the images once – improves perf & keeps ctor empty.
		private static readonly ImageSource EnabledImage = LoadImage("Assets/FwIsEnable.png");
		private static readonly ImageSource DisabledImage = LoadImage("Assets/FwIsDisabled.png");

		private static ImageSource LoadImage(string relativeUri)
		{
			var uri = new Uri($"pack://application:,,,/{relativeUri}", UriKind.Absolute);
			return new BitmapImage(uri);
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Defensive: treat null or non‑bool as “disabled”
			bool isEnabled = value is bool b && b;

			// Return the pre‑loaded image that matches the state
			return isEnabled ? EnabledImage : DisabledImage;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}