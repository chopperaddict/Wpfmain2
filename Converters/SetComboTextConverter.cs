using System;
using System . Diagnostics;
using System . Globalization;
using System . Windows . Data;
using System . Windows . Media;

using Wpfmain;

namespace Wpfmain . Converters
{
    public class SetComboTextConverter : IMultiValueConverter
    {
        //Sets the  combo text for each entry
        public object Convert ( object [ ] values, Type targetType, object parameter, CultureInfo culture )
        {
            // Works very well indeed , we recieve a comboclrs structure from caller
            string str  ="";
            try
            {
                SProcsHandling. comboclrs clrs = (SProcsHandling. comboclrs)values [ 0 ] ;
                str  = clrs . name;
                if ( str != null )
                {
//                    Debug . WriteLine ( $"Returning {str} for input [{clrs . name}] Foreground color" );
                    return str;
                }
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"Error in Combo Text Converter \n{ex . Message}\n" );
                return str;
            }
            return str;
        }
        public object [ ] ConvertBack ( object value, Type [ ] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}