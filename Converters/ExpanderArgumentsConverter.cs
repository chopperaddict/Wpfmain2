using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;
using System . Windows . Data;

//using static NewWpfDev. Views. TreeViews;

namespace Wpfmain . Converters
{
    public class ExpanderArgumentsConverter : IMultiValueConverter
    {
        public object Convert ( object [ ] values , Type targetType , object parameter , CultureInfo culture )
        {
            return values;
            //eargs . tvitem = values [ 0 ] as TreeViewItem;
            //eargs . Levels = System.Convert . ToInt16 ( values [ 1 ] );
            //return ( object ) eargs;
        }

        public object [ ] ConvertBack ( object value , Type [ ] targetTypes , object parameter , CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}
