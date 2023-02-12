using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Data;
using System . Windows . Media;
using System . Windows . Shapes;

namespace Wpfmain . Converters
{
    public class ComboTextColorConverter : IMultiValueConverter
    {
        // Set foreground text color formy colored combo
        SolidColorBrush newcolor = new ();
        public object Convert ( object [ ] values, Type targetType, object parameter, CultureInfo culture )
        {
            // Works very well indeed , we recieve a comboclrs structure from caller
            SolidColorBrush scb = new();
            try
            {
                SProcsHandling. comboclrs clrs = (SProcsHandling. comboclrs)values [ 0 ] ;
                scb = clrs . Fground;
                if ( scb != null )
                {
 //                   Debug . WriteLine ( $"Returning {scb} for input [{clrs . name}] Foreground color" );
                    return scb;
                }
            }
            catch(Exception ex )
            {
                Debug . WriteLine ( $"Error in Combo Foreground text color Converter \n{ex . Message}\n" );
                return scb;
            }
            return scb;
        }
         public object [ ] ConvertBack ( object value, Type [ ] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}
