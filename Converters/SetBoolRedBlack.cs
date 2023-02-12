using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;
using System . Windows . Media;

namespace Wpfmain. Converters
{
    public class SetBoolRedBlack : IValueConverter {
        string arg = "";
        public object Convert ( object value , Type targetType , object parameter , CultureInfo culture ) {
            if ( value == null ) return null;
            arg = value . ToString ( );
            if ( arg . Contains ( "Entry contains an invalid character" ) )
                return new SolidColorBrush ( Color . FromRgb ( 255 , 0 , 0 ) );   // Red
            else
                return new SolidColorBrush ( Color . FromRgb ( 0 , 0 , 0 ) ); // Black
        }

        public object ConvertBack ( object value , Type targetType , object parameter , CultureInfo culture ) {
            throw new NotImplementedException ( );
        }
    }
}
