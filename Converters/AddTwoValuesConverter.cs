
using System;
using System . Diagnostics;
using System . Globalization;
using System . Windows . Data;

namespace Wpfmain . Converters
{
    public class AddTwoValuesConverter : IMultiValueConverter {
        public object Convert ( object [ ] values , Type targetType , object parameter , CultureInfo culture ) {
            double dblval1 = 0.0, dblval2 = 0.0;
            object result = null;
            object [  ] objout = new object [3 ] ;
            objout [ 0 ] = values [ 1 ];     // ConverterParameter
            objout [ 1 ] = parameter;     // parameter
            objout [ 2 ] = values [ 0 ];     // main value received (some parent object usually
            //Debug . WriteLine ( $"Converter arguments are :\n{objout [ 0 ] ?. ToString ( )},\n{objout [ 1 ]? . ToString ( )},\n{objout [ 2 ]? . ToString ( )}\n" );
            
            return objout ;

        }

        public object [ ] ConvertBack ( object value , Type [ ] targetTypes , object parameter , CultureInfo culture ) {
            throw new NotImplementedException ( );
        }
    }
}
