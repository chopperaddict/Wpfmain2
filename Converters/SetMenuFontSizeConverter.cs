using System;
using System . Globalization;
using System . Windows . Data;

namespace Wpfmain . Converters
{
    public class SetMenuFontSizeConverter : IMultiValueConverter
    {
        double [ ] valid = {10,11,12,14,16,17, 18,19,20,22,24 };
        public object Convert ( object [ ] values, Type targetType, object parameter, CultureInfo culture )
        {
            // Works very well indeed 
            string returnstring="";
            if ( values [ 0 ] == null )
                return values [ 0 ];

            // temp, cos it is not pleasannt in a combo dropdown....
            return values [ 0 ];



            for ( int x = 0 ; x < 11 ; x++ )
            {
                if ( System.Convert.ToDouble(values [ 0 ]) == valid [x] )
                    return valid [ x ]+4;
            }
            return values[0];
        }
        public object [ ] ConvertBack ( object value, Type [ ] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}
