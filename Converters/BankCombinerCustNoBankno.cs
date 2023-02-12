using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;

using ViewModels;

namespace Wpfmain . Converters
{
    internal class BankCombinerCustNoBankno: IMultiValueConverter
	{
		public object Convert ( object [ ] values , Type targetType , object parameter , CultureInfo culture )
		{
    		string output = "";
            //TODO
            return output;
			
   //         if ( values [0] != null && values [ 2 ] != null )
			//{
			//	YieldWindowViewModel yvm = values [ 2 ] as YieldWindowViewModel;
   //             //ObservableCollection<BankAccountViewModel > bvm = values[0] as ObservableCollection<BankAccountViewModel>;
   //             BankAccountViewModel bv1 = values [ 0 ] as BankAccountViewModel;
   //             //BankAccountViewModel bv2 = values [ 1 ] as BankAccountViewModel;
   //             //string bv5 = values [ 6 ] . ToString ( );
   //             //BankAccountViewModel bv6 = values [ 5 ] as BankAccountViewModel;
   //             if ( bv1 != null )
   //             {
   //                 output = yvm.SelectedAccount1.CustNo + " : " + bv1 . BankNo;
   //                 //values [ 0 ] = output;
   //                 //Debug. WriteLine ( $"custno Label  : {bv5}" );
   //                 //Debug. WriteLine ( $"grid2 : {bv6.Balance}" );
   //             }
   //         }
   //         return output;
		}
		  
        public object [ ] ConvertBack ( object value , Type [ ] targetTypes , object parameter , CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}
