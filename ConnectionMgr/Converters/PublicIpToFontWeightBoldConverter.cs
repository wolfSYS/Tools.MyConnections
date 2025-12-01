using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ConnectionMgr.ExtensionMethods;

namespace ConnectionMgr.Converters
{
	public class PublicIpToFontWeightBoldConverter : IValueConverter
	{
		// font weight bold for public IP adr
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is IPAddress actualIpAdr)
			{
				if (actualIpAdr.IsPublic())
					return FontWeights.Bold;
			}
			return FontWeights.Normal;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}



