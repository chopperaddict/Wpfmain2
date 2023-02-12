using System;
using System . Diagnostics;
using System . Globalization;
using System . Windows . Data;

namespace Converters
{
	public class ReduceByParamValue : IValueConverter
    {
        /// <summary>
        /// Adds a dependency value received an XPath Converter parameter to move a textbolock downwrds to fit correctly
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert (object value , Type targetType , object parameter , CultureInfo culture)
        {
            double currentvalue = 0;
            double d = 0;
            Type t = targetType;
            //			Debug. WriteLine ( $"value = {value}, Parameter = {parameter}, TargetType={targetType}" );

            if ( parameter != null && value != null )
            {
                d = ( double )value;
                if ( d == 0 )
                    return value;
                double param = System . Convert . ToDouble(parameter);
                if ( param > 0 )
                {
                    currentvalue = d - ( param );
                }
                else
                {
                    currentvalue = d + param;
                }
                //                Debug. WriteLine ($"value={value}, Cv= {currentvalue}");
                //Debug . WriteLine ($"Reduce = {currentvalue} from parameter {parameter}");
                return currentvalue;
            }
            else
            {
                d = ( double )value;
                currentvalue = d - ( double )35;
                Debug. WriteLine($"ReduceByParamValue Converter has returned {currentvalue} from {d} - 35");
            }
            return currentvalue;
        }

        public object ConvertBack (object value , Type targetType , object parameter , CultureInfo culture)
        {
            //if ( temp <= 255 )
            //	return ( string ) temp . ToString ( "X2" );
            //else if ( temp <= 65535 )
            //	return ( string ) temp . ToString ( "X4" );
            //else if ( temp <= 16777215 )
            //	return ( string ) temp . ToString ( "X6" );

            return value;
        }
    }
}
