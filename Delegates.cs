using ViewModels;

using Views;

namespace Wpfmain
{
	/// <summary>
	/// A class to hold all generic Delegate methods
	/// </summary>
	public class Delegates
    {
        // int = (int, int)
        public delegate int  MyFunc(int arg, int arg2);

        public delegate bool  CompareBankRecords( BankAccountViewModel arg1 , BankAccountViewModel arg2 );

        // MainWindow.xaml.cs
        public delegate void LoadTableDelegate ( string Sqlcommand , string TableType , object bvm );
        public delegate void LoadTableWithDapperDelegate ( string Sqlcommand , string TableType , object bvm , object Args );

        // DargDropClient.xaml.cs
        public delegate string QualifyingFileLocations ( string filename );

        // MenutestViewModel.xaml.cs
        public delegate void RunMenuCommand ( string command , object data );

        
        //YieldWindow.xaml.cs
        public delegate void UpdateBankAccountSelection ( object sender , DbArgs args );

        // TextBoxWithDataError.xaml.cs
        // define the delegate handler signature and the event that will be raised
        // to send the message using my own specific Arguments
        public delegate void SendUserHandler ( object sender , MessageEventArgs args );
        public event SendUserHandler SendUser;

    }
}
