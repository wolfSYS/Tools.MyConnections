using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ConnectionMgr.Converters
{
	public class HeightMinusMarginsConverter : IValueConverter
	{
		// Subtract the vertical margin (Top + Bottom) from the page height
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double actualHeight)
			{
				const double margin = 24; // 12 top + 12 bottom
				return actualHeight - margin;
			}
			return Binding.DoNothing;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}



