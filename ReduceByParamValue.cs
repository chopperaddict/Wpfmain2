using System;
using System . Diagnostics;
using System . Globalization;
using System . Windows . Data;

namespace Wpfmain
{
	public class ReduceByParamValue2: IValueConverter
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

            if ( SProcsHandling . startup == true) return value;
//            if ( SProcsHandling . DragActive ) return value;
//   			Debug. WriteLine ( $"value = {value}, Parameter = {parameter}, TargetType={targetType}" );
            "" . Track ( 0 );
            try
            {
                if ( parameter != null && value != null )
                {
                    d = ( double ) value;
                    if ( d == 0 )
                        return value;
                    double param = System . Convert . ToDouble(parameter);
                    if ( param > 0 )
                    {
                        currentvalue = d - ( param );
                    }
                    else
                    {
                        currentvalue = d - param;
                    }
                    if ( currentvalue < 0 )
                    {
                        Debug . WriteLine ( $"New value={currentvalue} - Reset to ZERO" );
                        currentvalue = 0;
                    }
                    //Debug . WriteLine ($"Reduce = {currentvalue} from parameter {parameter}");
                    "" . Track ( 1 );
                    return currentvalue;
                }
                else
                {
                    d = ( double ) value;
                    currentvalue = d - ( double ) 35;
                    Debug . WriteLine ( $"ReduceByParamValue Converter has returned {currentvalue} from {d} - 35" );
                }
            }
            catch ( Exception ex ) { Debug . WriteLine ( $"ReduceByParamValue2 Converter error new  value={currentvalue} from ({d} - 35)" ); }
			"" . Track ( 1 );
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
