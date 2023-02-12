using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;

namespace Wpfmain . Converters
{
    public class DiameterAndThicknessToDasharrayConverter : IMultiValueConverter
    {
        public object Convert ( object [ ] values , Type targetType , object parameter , CultureInfo culture )
        {
            if ( values . Length < 3 ||
                !double . TryParse ( values [ 0 ] . ToString ( ) , out double diameter ) ||
                !double . TryParse ( values [ 1 ] . ToString ( ) , out double thickness ) ||
                !double . TryParse ( values [ 2 ] . ToString ( ) , out double gap))
            return 0;

            //gap=25
//            double circumference = Math . PI * diameter;    // 141.37
            double circumference = Math . PI * (diameter - thickness);    // 141.37
            double lineLength = circumference * (1.0 - gap);       // 66.37   //76

            double gapLength = circumference - lineLength;// 75     15  // 35
            return new System . Windows . Media . DoubleCollection ( new [ ] {lineLength / thickness, gapLength / thickness } );


            //double lineLength = circumference * 0.65;       //91.89
            //double gapLength = circumference - lineLength;
            //return new System . Windows . Media . DoubleCollection ( new [ ] { lineLength / thickness , gapLength / thickness } );

        }
        public object [ ] ConvertBack ( object value , Type [ ] targetTypes , object parameter , CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }


}
