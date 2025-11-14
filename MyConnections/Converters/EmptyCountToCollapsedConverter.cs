using System.Globalization;
using System.Windows.Data;

namespace ConnectionMgr.Converters
{
	/// <summary>
	/// Converter for hiding (Visibility.Collapsed) an empty DataGrid.
	/// </summary>
	public class EmptyCountToCollapsedConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int count)
				return count == 0 ? Visibility.Collapsed : Visibility.Visible;
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}