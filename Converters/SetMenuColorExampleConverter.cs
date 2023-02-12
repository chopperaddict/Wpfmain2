using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Drawing;
using System . Globalization;
using System . Reflection;
using System . Windows;
using System . Windows . Data;
using System . Windows . Media;

using static Wpfmain . SProcsHandling;

namespace Wpfmain . Converters
{
    public class SetMenuColorExampleConverter : IMultiValueConverter
    {
        // Set background color of SProcshandling combo box
        SolidColorBrush newcolor = new ();
        public object Convert ( object [ ] values, Type targetType, object parameter, CultureInfo culture )
        {
            // Set Background color for my colored combo
            SolidColorBrush scb = new();
            // Works very well indeed , we recieve a comboclrs structure from caller
            try
            {
                SProcsHandling. comboclrs clrs = (SProcsHandling. comboclrs)values [ 0 ] ;
                scb = clrs . Bground;
                if ( scb != null )
                {
 //                   Debug . WriteLine ( $"Returning {scb} for input [{clrs . name}] Background color" );
                    return scb;
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( $"Error in Combo Background color Converter \n{ex.Message}\n"  );
                return Application . Current . FindResource ( "Red5" ) as SolidColorBrush;
            }
            return scb;
        }
        public object [ ] ConvertBack ( object value, Type [ ] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}
