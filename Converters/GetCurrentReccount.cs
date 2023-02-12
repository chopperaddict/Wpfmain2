using System;
using System . Globalization;
using System . Windows . Data;

namespace Wpfmain . Converters
{
    public class GetCurrentReccount : IValueConverter
    {
        public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
        {
			double currentvalue = 0;
			double d = 0;
			Type t = targetType;
            //TODO
			d = ( int) value;
            return value;


   //         if ( parameter != null && value != null )
   //         {
   //             ModernViews win = parameter as ModernViews;
   //             if ( d == 0 || win == null )
   //                 return value;
   //             ListView lv = new ListView ( );
   //             if ( lv == null )
   //                 return value;
   //             else
   //             {
   //                 lv = win. lview3;
   //                 if ( win. lview3. Visibility == Visibility. Visible )
   //                     currentvalue = win. RecCountlv;
   //                 else if ( win. lbox1. Visibility == Visibility. Visible )
   //                     currentvalue = win. RecCountlb;
   //                 else if ( win. Dgrid1. Visibility == Visibility. Visible )
   //                     currentvalue = win. RecCountdg;

   //                 return currentvalue;
   //             }
            //}
            ////else
            //return d;
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}
