using System;
using System . Globalization;
using System . Windows . Data;

namespace Wpfmain . Converters {
    public class BoolToNotBool : IValueConverter
	{
		public object Convert ( object value , Type targetType , object parameter , CultureInfo culture )
		{
			if ( ( bool ) value == true )
			{
				value = false;
				if ( parameter . ToString ( ) == "NO" )
					value = true;
			}
			else
			{
				value = true;
				if ( parameter . ToString ( ) == "NO" )
					value = false;
			}

			return value;
		}

		public object ConvertBack ( object value , Type targetType , object parameter , CultureInfo culture )
		{
			throw new NotImplementedException ( );
		}
	}
}
