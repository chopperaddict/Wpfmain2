using System;
using System . Diagnostics;
using System . Globalization;
using System . Windows;
using System . Windows . Data;

namespace Wpfmain . Converters {
    public class InverseNullVisibilityConverter : IValueConverter {
        public object Convert ( object value , Type targetType , object parameter , CultureInfo culture ) {
            int val = 0;
            if ( value != null ) {
                val = System . Convert . ToInt16 ( value );
                Debug . WriteLine ($"converter value = {val}");
                if(val == 1)
                    return Visibility . Hidden;
                else
                return Visibility . Visible;
            }
            return Visibility . Hidden;
        }

        public object ConvertBack ( object value , Type targetType , object parameter , CultureInfo culture ) {
            throw new NotImplementedException ( );
        }
    }

}
