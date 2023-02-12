using System;
using System . Globalization;
using System . Windows;
using System . Windows . Data;

namespace Wpfmain . Converters
{
    public class MultInverseNullVisibilityConverter : IMultiValueConverter {

        public object Convert ( object [ ] values , Type targetType , object parameter , CultureInfo culture ) {
            int val = 0;
            try {
                val = System . Convert . ToInt16 ( values [ 1 ] );
                if ( values [ 1 ] != null ) {
                    if ( val == 1 )
                        return Visibility . Hidden;
                }
                return Visibility . Visible;
            }
            catch ( Exception ex ) {
                return Visibility . Visible;
            }
            return Visibility . Visible;
        }

        public object [ ] ConvertBack ( object value , Type [ ] targetTypes , object parameter , CultureInfo culture ) {
            throw new NotImplementedException ( );
        }
    }
}
