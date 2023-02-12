
using System;
using System . Diagnostics;
using System . Globalization;
using System . Windows . Data;

namespace Wpfmain . Converters
{ 
    public class SprocsControlsMultiConverter : IMultiValueConverter
    {
        public object Convert ( object [ ] values, Type targetType, object parameter, CultureInfo culture )
        {
            Debug . WriteLine ( $"SprocsControlsMultiConverter entered for {values [ 1 ]} :-" );
            try
            {
                if ( values [ 1 ] == null ) return values [ 2 ];
                if ( values [ 1 ] . ToString ( ) == "SPFullDataContainerGrid" )
                    return values [ 2 ];
                double newheight=System . Convert . ToDouble ( values [1]);
                double currentheight = System . Convert . ToDouble ( values [1]);
                double paramheight = System . Convert . ToDouble ( values [2]);
                if ( values [ 1 ] . ToString ( ) == "SPDatagrid" )//&& currentheight > 0 && paramheight != 0 )
                    newheight = ( double ) values [ 1 ] - System . Convert . ToDouble ( values [ 2 ] );
                else if ( values [ 1 ] . ToString ( ) == "SProcListBox" )//&& currentheight > 0 && paramheight != 0 )
                    newheight = ( double ) values [ 1 ] - System . Convert . ToDouble ( values [ 2 ] );
                else if ( values [ 1 ] . ToString ( ) == "ExecList" )//&& currentheight > 0 && paramheight != 0 )
                    newheight = ( double ) values [ 1 ] - System . Convert . ToDouble ( values [ 2 ] );
                else if ( values [ 1 ] . ToString ( ) == "TextResult" )//&& currentheight > 0 && paramheight != 0 )
                    newheight = ( double ) values [ 1 ] - System . Convert . ToDouble ( values [ 2 ] );
                else newheight = 300;
                //else if ( values [ 2 ] . ToString ( ) == "SProcsListbox" )
                //    newheight = ( double ) values [ 1 ] - 15;
                //else if ( values [ 2 ] . ToString ( ) == "ExecList" )
                //    newheight = ( double ) values [ 1 ] - 25;
                //else if ( values [ 2 ] . ToString ( ) == "TextResult" )
                //    newheight = ( double ) values [ 1 ] - 5;
                Debug . WriteLine ( $"Returning newvalue of {newheight}" );
                return newheight;
            }
            catch ( Exception ex ) { Debug . WriteLine ( $"{ex . Message} : {ex . Data}\n{values [ 0 ]},{values [ 1 ]}" ); return 210.00; }

        }

        public object [ ] ConvertBack ( object value, Type [ ] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}
