using System;
using System . Globalization;
using System . Windows . Data;

namespace Wpfmain . Converters {
    public class MultDivIntSize : IValueConverter {
        public object Convert ( object value , Type targetType , object parameter , CultureInfo culture ) {
            // Pass minus value to divide, positive to multply
            int val = System . Convert . ToInt32 ( value );
            int param = System . Convert . ToInt32 ( parameter );
            if ( param < 0 )
                val = val / (param *-1);
            else
                val = val *  param ;
            return val;
        }

        public object ConvertBack ( object value , Type targetTypes , object parameter , CultureInfo culture ) {
            throw new NotImplementedException ( );
        }
    }
}
