using System.Globalization;
using System.Windows.Data;

namespace ConnectionMgr.Converters
{
	/// <summary>
	/// Converts an integer count to a <see cref="Visibility"/> value, where a count of zero results in <see
	/// cref="Visibility.Visible"/> and any other value results in <see cref="Visibility.Collapsed"/>.
	/// </summary>
	/// <remarks>This converter is typically used in data binding scenarios to toggle the visibility of UI elements
	/// based on whether a count is empty (zero) or non-empty.</remarks>
	public class EmptyCountToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int count)
				return count == 0 ? Visibility.Visible : Visibility.Collapsed;
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}