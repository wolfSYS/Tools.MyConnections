using System;
using System.Globalization;
using System.Windows.Data;

namespace ConnectionMgr.Converters
{
	/// <summary>
	/// Used to remove the App Tag ("#ConnectionMgr") from Rules Name in the FirewallPage DataGrid.
	/// </summary>
	public class StripAppTagFromFwRuleConverter : IValueConverter
	{
		// Called when the value is displayed (Source → Target)
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s)
			{
				return s.Replace("#ConnectionMgr", string.Empty);
			}
			return value;
		}

		// Called when the user edits the cell (Target → Source)
		// In this case we simply return the string unchanged.
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
}
