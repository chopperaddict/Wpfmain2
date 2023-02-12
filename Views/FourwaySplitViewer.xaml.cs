
using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Controls . Primitives;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;

using Microsoft . Data . SqlClient;

using SprocsProcessing;

using ViewModels;

using Wpfmain;
using Wpfmain . SqlBasicSupport;
using Wpfmain . SqlMethods;

namespace  Views
{
	public partial class FourwaySplitViewer : Window {
        #region  Public variables
        // Set  up our data collections

        // Individual records
        public BankAccountViewModel bvm = new BankAccountViewModel ( );
        public CustomerViewModel cvm = new CustomerViewModel ( );
        public DetailsViewModel dvm = new DetailsViewModel ( );
        public GenericClass gvm = new GenericClass ( );

        // Collections
        public ObservableCollection<BankAccountViewModel> bankaccts = new ObservableCollection<BankAccountViewModel> ( );
        public ObservableCollection<CustomerViewModel> custaccts = new ObservableCollection<CustomerViewModel> ( );
        public ObservableCollection<DetailsViewModel> detaccts = new ObservableCollection<DetailsViewModel> ( );
        public ObservableCollection<GenericClass> genaccts = new ObservableCollection<GenericClass> ( );

        // supporting sources
        public List<string> TablesList = new List<string> ( );

        // internal Flag data
        private string CurrentType = "BANKACCOUNT";
        //		private string [ ] ACTypes = {"BANK", "CUSTOMER", "DETAILS", "NWCUSTOMER", "NWCUSTLIMITED", "GENERIC"};
        //		private string [ ] DefaultTables = {"BANKACCOUNT", "CUSTOMER", "SECACCOUNTS", "CUSTOMERS", "GENERICS"};
        private string SqlCommand = "";
        private string DefaultSqlCommand = "Select * from BankAccount";
#pragma warning disable CS0414 // The field 'FourwaySplitViewer.Nwconnection' is assigned but its value is never used
        string Nwconnection = "NorthwindConnectionString";
#pragma warning restore CS0414 // The field 'FourwaySplitViewer.Nwconnection' is assigned but its value is never used
        private double MaxColWidth1 { get; set; }
        private double MinRowHeight1 { get; set; }
        private double MaxRowHeight { get; set; }
        private bool FillListBox { get; set; }

        #endregion  Public variables
        public FourwaySplitViewer ( ) {
            InitializeComponent ( );
            this . DataContext = this;
            //Utils . SetupWindowDrag ( this );
            // setup check boxes & ListBox
            LoadViaSqlCmd . IsChecked = false;
            BgWorker . IsChecked = true;
            UseBGThread = true;
            Usetimer = true;
            UseTimer . IsChecked = true;
            Flags . UseScrollView = false;
            // Set flags so we get the right SQL command method used...
            UseDirectLoad = true;
            SqlCommand = DefaultSqlCommand;
            canvas . Visibility = Visibility . Visible;
            MovingObject = null;
            UseTrueColNames . IsChecked = true;
            UseScrollview . IsChecked = Flags . UseScrollView;
            UseFlowdoc = Flags . UseFlowdoc; ;
            Showinfo . IsChecked = UseFlowdoc;
            if ( UseFlowdoc == false )
                //Flowdoc . Visibility = Visibility . Hidden;
            MaxColWidth1 = 340;
            MinRowHeight1 = 255;
            MaxRowHeight = 275;
            imgup = new BitmapImage ( new Uri ( @"\icons\down arroiw red.png" , UriKind . Relative ) );
            vimgmove = new BitmapImage ( new Uri ( @"\icons\right arroiw red.png" , UriKind . Relative ) );
            LhHsplitter = new BitmapImage ( new Uri ( @"\icons\down arroiw red.png" , UriKind . Relative ) );
            //            lsplitrow1 . Height = (GridLength)1;
            FillListBox = true;
            // Toggle magnify if set to disable
            List<object> list = new List<object> ( );
            if ( Flags . UseMagnify == false ) {
                list . Add ( DataGrid1 );
                list . Add ( listbox );
                list . Add ( DbRecordInfo );
                list . Add ( TablesPanel );
                Utils . Magnify ( list , false );
            }
            else {
                list . Add ( DataGrid1 );
                list . Add ( listbox );
                list . Add ( DbRecordInfo );
                list . Add ( TablesPanel );
                Utils . Magnify ( list , true );
            }
            // Handle the magnify sytem to handle global flag
            Flags . UseMagnify = ( bool ) Wpfmain . Properties . Settings . Default [ "UseMagnify" ];
            if ( Flags . UseMagnify == false ) {
                DataGrid1 . Style = ( Style ) FindResource ( "DatagridMagnifyAnimation0" );
                DbRecordInfo . Style = ( Style ) FindResource ( "ListBoxMagnifyAnimation0" );
                dbName . Style = ( Style ) FindResource ( "ComboBoxMagnifyAnimation0" );
                Storedprocs . Style = ( Style ) FindResource ( "ComboBoxMagnifyAnimation0" );
                Magnifyrate . Text = "0";
            }
            else {
                DataGrid1 . Style = ( Style ) FindResource ( "DatagridMagnifyAnimation4" );
                DbRecordInfo . Style = ( Style ) FindResource ( "ListBoxMagnifyAnimation4" );
                dbName . Style = ( Style ) FindResource ( "ComboBoxMagnifyAnimation4" );
                Storedprocs . Style = ( Style ) FindResource ( "ComboBoxMagnifyAnimation4" );
                Magnifyrate . Text = "+4";
            }
            ShowFlowdoc . IsChecked = UseFlowdoc = true;
            // This sets the relative height of a Grid's row heights - works  too
            //LeftPanelgrid . RowDefinitions [ 0 ] . Height = new GridLength ( 0.01 , GridUnitType . Star );
            //LeftPanelgrid . RowDefinitions [ 1 ] . Height = new GridLength ( 20 , GridUnitType . Pixel );
            //LeftPanelgrid . RowDefinitions [ 2 ] . Height = new GridLength ( 20 , GridUnitType . Star );

            //Maingrid . ColumnDefinitions [ 0 ] . Width= new GridLength ( 0 , GridUnitType . Pixel );
            //Maingrid . ColumnDefinitions [ 1 ] . Width = new GridLength ( 30 , GridUnitType . Pixel );
            //Maingrid . ColumnDefinitions [ 2 ] . Width = new GridLength ( 1 , GridUnitType . Star );
            //Maingrid . ColumnDefinitions [ 3 ] . Width = new GridLength ( 10 , GridUnitType . Pixel );
        }

        #region DP.s
        public string LeftSplitterText {
            get { return ( string ) GetValue ( LeftSplitterTextProperty ); }
            set { SetValue ( LeftSplitterTextProperty , value ); }
        }
        public static readonly DependencyProperty LeftSplitterTextProperty =
           DependencyProperty . Register ( "LeftSplitterText" , typeof ( string ) , typeof ( FourwaySplitViewer ) , new PropertyMetadata ( "Drag Down  " ) );

        public string ShowText {
            get { return ( string ) GetValue ( ShowTextProperty ); }
            set { SetValue ( ShowTextProperty , value ); }
        }
        public static readonly DependencyProperty ShowTextProperty =
            DependencyProperty . Register ( "ShowText" , typeof ( string ) , typeof ( FourwaySplitViewer ) , new PropertyMetadata ( "Show More Records" ) );

        public string ShowdragText {
            get { return ( string ) GetValue ( ShowdragTextProperty ); }
            set { SetValue ( ShowdragTextProperty , value ); }
        }
        public static readonly DependencyProperty ShowdragTextProperty =
            DependencyProperty . Register ( "ShowdragText" , typeof ( string ) , typeof ( FourwaySplitViewer ) , new PropertyMetadata ( "Drag Up/Down to  " ) );

        public BitmapImage imgup {
            get { return ( BitmapImage ) GetValue ( imgupProperty ); }
            set { SetValue ( imgupProperty , value ); }
        }
        public static readonly DependencyProperty imgupProperty =
            DependencyProperty . Register ( "imgup" , typeof ( BitmapImage ) ,
                typeof ( FourwaySplitViewer ) ,
                 new PropertyMetadata ( null ) );
  
        public BitmapImage imgdn {
            get { return ( BitmapImage ) GetValue ( imgdnProperty ); }
            set { SetValue ( imgdnProperty , value ); }
        }
        public static readonly DependencyProperty imgdnProperty =
            DependencyProperty . Register ( "imgdn" ,
                 typeof ( BitmapImage ) ,
                   typeof ( FourwaySplitViewer ) ,
                new PropertyMetadata ( null ) );
  
        public BitmapImage imgmv {
            get { return ( BitmapImage ) GetValue ( imgmvProperty ); }
            set { SetValue ( imgmvProperty , value ); }
        }
        public static readonly DependencyProperty imgmvProperty =
             DependencyProperty . Register ( "imgmv" ,
                  typeof ( BitmapImage ) ,
                    typeof ( FourwaySplitViewer ) ,
                 new PropertyMetadata ( null ) );
 
        public BitmapImage vimgleft {
            get { return ( BitmapImage ) GetValue ( vimgleftProperty ); }
            set { SetValue ( vimgleftProperty , value ); }
        }
        public static readonly DependencyProperty vimgleftProperty =
            DependencyProperty . Register ( "vimgleft" , typeof ( BitmapImage ) , typeof ( FourwaySplitViewer ) , new PropertyMetadata ( null ) );
  
        public BitmapImage vimgright {
            get { return ( BitmapImage ) GetValue ( vimgrightProperty ); }
            set { SetValue ( vimgrightProperty , value ); }
        }
        public static readonly DependencyProperty vimgrightProperty =
           DependencyProperty . Register ( "vimgright" , typeof ( BitmapImage ) , typeof ( FourwaySplitViewer ) , new PropertyMetadata ( null ) );

        public BitmapImage vimgmove {
            get { return ( BitmapImage ) GetValue ( vimgmoveProperty ); }
            set { SetValue ( vimgmoveProperty , value ); }
        }
        public static readonly DependencyProperty vimgmoveProperty =
            DependencyProperty . Register ( "vimgmove" , typeof ( BitmapImage ) , typeof ( FourwaySplitViewer ) , new PropertyMetadata ( null ) );
 
        public BitmapImage LhHsplitter {
            get { return ( BitmapImage ) GetValue ( LhHsplitterProperty ); }
            set { SetValue ( LhHsplitterProperty , value ); }
        }
        public static readonly DependencyProperty LhHsplitterProperty =
            DependencyProperty . Register ( "LhHsplitter" , typeof ( BitmapImage ) , typeof ( FourwaySplitViewer ) , new PropertyMetadata ( null ) );

        public int DbCount {
            get { return ( int ) GetValue ( DbCountProperty ); }
            set { SetValue ( DbCountProperty , value ); }
        }
        // Using a DependencyProperty as the backing store for DbCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DbCountProperty =
            DependencyProperty . Register ( "DbCount" , typeof ( int ) , typeof ( FourwaySplitViewer ) , new PropertyMetadata ( 0 ) );
        #endregion DP.s


        #region private variables
        private bool UseDirectLoad = true;
        private bool UseBGThread = true;
        private bool LoadDirect = false;
        // pro temp variables
        private bool UseFlowdoc = false;
#pragma warning disable CS0414 // The field 'FourwaySplitViewer.UseFlowdocBeep' is assigned but its value is never used
        private bool UseFlowdocBeep = false;
#pragma warning restore CS0414 // The field 'FourwaySplitViewer.UseFlowdocBeep' is assigned but its value is never used
        private bool showall = false;
        private bool ShowFullScript = false;
        private bool LoadAll = false;
        private bool Usetimer = true;
        //private bool ReplaceFldNames=true;
        //		private bool UseScrollViewer= true;
        private static Stopwatch timer = new Stopwatch ( );

        // Flowdoc file wide variables
//        public FlowdocLib fdl = new FlowdocLib ( );
#pragma warning disable CS0414 // The field 'FourwaySplitViewer.XLeft' is assigned but its value is never used
        private double XLeft = 0;
#pragma warning restore CS0414 // The field 'FourwaySplitViewer.XLeft' is assigned but its value is never used
#pragma warning disable CS0414 // The field 'FourwaySplitViewer.YTop' is assigned but its value is never used
        private double YTop = 0;
#pragma warning restore CS0414 // The field 'FourwaySplitViewer.YTop' is assigned but its value is never used

        #endregion private variables

        #region Binding full props
        // Full properties used in Binding to I/f objects


        private bool ismouseDown;
        public bool isMouseDown {
            get { return ismouseDown; }
            set { ismouseDown = value; }
        }
        private object movingobject;
        public object MovingObject {
            get { return movingobject; }
            set { movingobject = value; }
        }

#pragma warning disable CS0414 // The field 'FourwaySplitViewer.FirstXPos' is assigned but its value is never used
        private double FirstXPos = 0;
#pragma warning restore CS0414 // The field 'FourwaySplitViewer.FirstXPos' is assigned but its value is never used
#pragma warning disable CS0414 // The field 'FourwaySplitViewer.FirstYPos' is assigned but its value is never used
        private double FirstYPos = 0;
#pragma warning restore CS0414 // The field 'FourwaySplitViewer.FirstYPos' is assigned but its value is never used

        #endregion Binding full props

        #region Startup/Close

        private void Window_Loaded ( object sender , RoutedEventArgs e ) {
            // Set up notification from "Normal" Db Loading system
            EventControl . BankDataLoaded += EventControl_BankDataLoaded;
            EventControl . CustDataLoaded += EventControl_CustDataLoaded;
            EventControl . DetDataLoaded += EventControl_DetDataLoaded;
            EventControl . GenDataLoaded += EventControl_GenDataLoaded;
            // FlowDoc support
            //Flowdoc . ExecuteFlowDocMaxmizeMethod += new EventHandler ( MaximizeFlowDoc );
            //           Flowdoc. ExecuteFlowDocBorderMethod += FlowDoc_ExecuteFlowDocBorderMethod;
            this . SizeChanged += Datagrids_SizeChanged;

            LoadTablesList ( );
            LoadSpList ( );
            SqlCommand = "Select * from BankAccount";
            LoadData ( );
            Datagrids_SizeChanged ( sender , null );
            ShowdragText = "Drag Down here to ";
            ShowText = "Show more records";
        }
        private void Datagrids_SizeChanged ( object sender , SizeChangedEventArgs e ) {
            //Info . Width = Col2 . ActualWidth + Col3 . ActualWidth;
            canvas . Width = this . Width;
            canvas . Height = this . Height;
            canvas . SetValue ( HeightProperty , ( object ) DataGrid1 . Height );
            canvas . SetValue ( WidthProperty , ( object ) DataGrid1 . Width );
        }

        private void Window_Closing ( object sender , CancelEventArgs e ) {
            EventControl . BankDataLoaded -= EventControl_BankDataLoaded;
            EventControl . CustDataLoaded -= EventControl_CustDataLoaded;
            EventControl . DetDataLoaded -= EventControl_DetDataLoaded;
            EventControl . GenDataLoaded -= EventControl_GenDataLoaded;
            //Flowdoc . ExecuteFlowDocMaxmizeMethod -= new EventHandler ( MaximizeFlowDoc );
        }
        private void App_Close ( object sender , RoutedEventArgs e ) {
            this . Close ( );
            Application . Current . Shutdown ( );
        }
        private void Datagrids_Close ( object sender , RoutedEventArgs e ) {
            this . Close ( );
        }
        #endregion Startup/Close

        #region EventControl data loaded methods
        private void EventControl_BankDataLoaded ( object sender , LoadedEventArgs e ) {
            // Works - 2/2/22
            // Received notification from Bank Load system via an Event
            bankaccts = e . DataSource as ObservableCollection<BankAccountViewModel>;
            DbCount = bankaccts . Count;
            LoadGrid ( );
            if ( UseFlowdoc == true )
                //fdl . ShowInfo ( Flowdoc , canvas , $"The request for the default Bank Accounts table [{CurrentType}] was successful, and the {DbCount} results returned are shown in the datagrid ..." ,
                //      "Blue3" ,
                //      "" ,
                //      "" ,
                //      "" ,
                //      "" ,
                //       "Default Bank Account data table" ,
                //        "Red3" );
            DataGrid1 . SelectedIndex = 0;

            DataGrid1 . Focus ( );
            //ShowLoadtime ( timer . ElapsedMilliseconds );
        }
        private void EventControl_CustDataLoaded ( object sender , LoadedEventArgs e ) {
            // Works - 2/2/22
            // Received notification from Bank Load system via an Event
            custaccts = e . DataSource as ObservableCollection<CustomerViewModel>;
            DbCount = custaccts . Count;
            LoadGrid ( );
            if ( UseFlowdoc == true )
                //fdl . ShowInfo ( Flowdoc , canvas , line1: $"The request for the default Customer Accounts table [{CurrentType}] was successful, and the {DbCount} results returned are shown in the datagrid..." , "Orange0" , header: "Default Customers data table" );
            DataGrid1 . SelectedIndex = 0;
            DataGrid1 . Focus ( );
            //ShowLoadtime ();
        }
        private void EventControl_DetDataLoaded ( object sender , LoadedEventArgs e ) {
            // Works - 2/2/22
            // Received notification from Bank Load system via an Event
            detaccts = e . DataSource as ObservableCollection<DetailsViewModel>;
            DbCount = detaccts . Count;
            LoadGrid ( );
            if ( UseFlowdoc == true )
                //fdl . ShowInfo ( Flowdoc , canvas , line1: $"The request for the default Secondary Accounts table [{CurrentType}] was successful, and the {DbCount} results returned are shown in the datagrid ..." , clr1: "Green0" , header: "Default Secondary Bank Accounts data table" );
            DataGrid1 . SelectedIndex = 0;
            DataGrid1 . Focus ( );
            //ShowLoadtime ( );
        }
        private void EventControl_GenDataLoaded ( object sender , LoadedEventArgs e ) {
            // Works - 2/2/22
            // Received notification from Bank Load system via an Event
            genaccts = e . DataSource as ObservableCollection<GenericClass>;
            DbCount = genaccts . Count;
            LoadGrid ( );
            if ( UseFlowdoc == true )
                //fdl . ShowInfo ( Flowdoc , canvas , line1: $"The requested Generic table type [{CurrentType}] request succeeded, and the results are shown in the datagrid ..." , "header:Generic data table" );
            DataGrid1 . SelectedIndex = 0;
            DataGrid1 . Focus ( );
            //ShowLoadtime ( );
        }


        #endregion EventControl data loaded methods

        #region Checkbox/Combo handlers

        private void Autoload_Click ( object sender , RoutedEventArgs e ) {
            UseDirectLoad = LoadViaSqlCmd . IsChecked == true ? true : false;
            if ( UseDirectLoad ) {
                BgWorker . IsChecked = false;
                UseBGThread = false;
            }
            else {
                BgWorker . IsChecked = true;
                UseBGThread = true;
            }
        }
        private void BgWorker_Click ( object sender , RoutedEventArgs e ) {
            UseBGThread = BgWorker . IsChecked == true ? true : false;
            if ( UseBGThread ) {
                UseDirectLoad = false;
                LoadViaSqlCmd . IsChecked = false;
            }
            else {
                UseDirectLoad = true;
                LoadViaSqlCmd . IsChecked = true;
            }
        }
        private void dbName_SelectionChanged ( object sender , SelectionChangedEventArgs e ) {
            ComboBox cb = sender as ComboBox;
            if ( cb . Items . Count == 0 )
                return;
            CurrentType = cb . SelectedItem . ToString ( ) . ToUpper ( );
        }
        private void dbName_SelectionChanged_1 ( object sender , SelectionChangedEventArgs e ) {

        }

        #endregion Checkbox/Combo handlers

        #region Data Handling

		public string GetSqlCommand ( int count = 0 , int table = 0 , string condition = "" , string sortorder = "" ) {
            // Parse fields into a valid SQL Command string
            string output = "Select  ";
            output += count == 0 ? " * From " : $"top ({count}) * From ";
            output += dbName . SelectedItem . ToString ( );
            output += condition . Trim ( ) != "" ? " Where " + condition + " " : "";
            output += sortorder . Trim ( ) != "" ? " Order by " + sortorder . Trim ( ) : "";
            CurrentType = dbName . Items [ table ] . ToString ( ) . ToUpper ( );
            return output;
        }

        // TRIGGER method for all load requests
        private void ReloadDatagrids ( object sender , RoutedEventArgs e ) {
            int max = 0;
            //ShowInfo ( "" );
            // Load Db based on Parameters entered by user
            var result = int . TryParse ( RecCount . Text , out max );
            SqlCommand = GetSqlCommand ( max , dbName . SelectedIndex , "" , "" );
            string conds = Conditions . Text;
            if ( conds . Length > 0 )
                SqlCommand += $" where {conds} ";
            string ord = orderby . Text;
            if ( ord . Length > 0 )
                SqlCommand += $" Order by {ord}";
            bankaccts = new ObservableCollection<BankAccountViewModel> ( );
            DataGrid1 . ItemsSource = null;
            ShowTableStructure ( );

            DataGrid1 . Refresh ( );
            //ShowInfo ( info: $"Processing command [{SqlCommand}] ..." );
            LoadData ( );
        }
        private void LoadData ( ) {
            if ( UseBGThread ) {
                // This calls various methods that run on a Background Thread
                if ( SqlCommand . Contains ( " " ) == false || SqlCommand . ToUpper ( ) . Trim ( ) . Substring ( 0 , 2 ) == "SP" ) {
                    // process a Stored procedure
                     ProcessSqlCommand  ( SqlCommand );
                }
                else {     //process any other type of cmomand
                    SqlCommand = CheckLimits ( );
                    BackgroundWorkerLoad ( );
                }
            }
            else {
                if ( LoadDirect ) {
                    if ( Usetimer )
                        timer . Start ( );
					DataTable dt =  ProcessSprocs.ProcessSqlCommand ( SqlCommand );					
                    if ( CurrentType == "BANK" ) 
                        SqlMethods. LoadBankCollection ( dt , true );
                  if ( CurrentType == "CUSTOMER" ) 
						SqlMethods . LoadCustomerCollection ( dt , true );
                    if ( CurrentType == "DETAILS" ) 
                        SqlMethods . LoadDetailsCollection ( dt , true );
                   else {
                        // WORKING 5.2.22
                        // This creates and loads a GenericClass table if data is found in the selected table
                        SqlMethods . LoadGenericCollection ( dt );
                    }
                }
                if ( UseDirectLoad ) {
                    // CheckBox (UseBackgound Worker) is set to Unchecked for this branch to be activated

                    // This method is called on the UI Thread, and require the Events system to be notified when data is ready for us
                    // It accepts a fully qualified Sql Command line string to process, a maximum # of recrods to load, and a Notify Event completed flag
                    // SIMPLER METHODS !!
                    if ( Usetimer )
                        timer . Start ( );
 //                   if ( CurrentType == "BANKACCOUNT" ) {
 //                       bankaccts = new ObservableCollection<BankAccountViewModel> ( );
 //ObservableCollection<BankAccountViewModel> LoadBankCollection ( DataTable dtBank , bool Notify = false )
	//						bankaccts = SqlMethods . LoadBankCollection ( SqlCommand , 0 , true );
 //                   }
 //                   else if ( CurrentType == "CUSTOMER" ) {
 //                       custaccts = new ObservableCollection<CustomerViewModel> ( );
 //                       custaccts = SqlMethods. LoadCustomer ( SqlCommand , 0 , true );
 //                   }
 //                   else if ( CurrentType == "SECACCOUNTS" ) {
 //                       detaccts = new ObservableCollection<DetailsViewModel> ( );
 //                       detaccts = SqlMethods. LoadDetails ( SqlCommand , 0 , true );
 //                   }
 //                   else {
 //                       // WORKING 5.2.22
 //                       // This creates and loads a GenericClass table if data is found in the selected table
 //                       string ResultString = "";
 //                       string tablename = dbName . SelectedItem . ToString ( );
 //                       SqlCommand = $"Select *from {tablename}";
 //                       genaccts = SqlSupport . LoadGeneric ( SqlCommand , out ResultString , 0 , true );
 //                       if ( genaccts . Count > 0 ) {
 //                           LoadGrid ( genaccts );
 //                       }
 //                   }
 //               }
 //               else {
 //                   // MORE COMPLEX METHODS !!
 //                   if ( Usetimer )
 //                       timer . Start ( );
 //                   if ( CurrentType == "BANKACCOUNT" )
 //                       DapperSupport . GetBankObsCollection ( bankaccts , DbNameToLoad: "BankAccount" , Orderby: "Custno, BankNo" , wantSort: true , Caller: "DATAGRIDS" , Notify: true );
 //                   else if ( CurrentType == "CUSTOMER" )
 //                       DapperSupport . GetCustObsCollection ( custaccts , DbNameToLoad: "Customer" , Orderby: "Custno, BankNo" , wantSort: true , Caller: "DATAGRIDS" , Notify: true );
 //                   else if ( CurrentType == "SECACCOUNTS" )
 //                       DapperSupport . GetDetailsObsCollection ( detaccts , DbNameToLoad: "SecAccounts" , Orderby: "Custno, BankNo" , wantSort: true , Caller: "DATAGRIDS" , Notify: true );
 //               }
 //           }
        }
        private void ReloadAll ( object sender , RoutedEventArgs e ) {
            int max = 0;
            // Load ALL records
            SqlCommand = GetSqlCommand ( max , dbName . SelectedIndex , "" , "" );
            if ( CurrentType == "BANKACCOUNT" )
                bankaccts = null;
            else if ( CurrentType == "CUSTOMER" )
                custaccts = null;
            else if ( CurrentType == "SECACCOUNTS" )
                detaccts = null;
            else
                genaccts = null;
            DataGrid1 . ItemsSource = null;
            DataGrid1 . Refresh ( );
            ShowTableStructure ( );
            // Set flag  to ignore limits check
            LoadAll = true;
            LoadData ( );
            // Clear flag again
            LoadAll = false;
        }
        private void ShowSPArgs ( object sender , RoutedEventArgs e ) {
            //Preview  SP arguments  info in TextBox for current item in Combo
            Mouse . OverrideCursor = Cursors . Wait;
            //showall = false;
            if ( Usetimer )
                timer . Start ( );
            string str = GetSpArgs ( Storedprocs . SelectedItem . ToString ( ) );
            //DbCopiedResult . Text = $"Display selected Stored Procedure Command completed successfully ...";
            Mouse . OverrideCursor = Cursors . Arrow;
        }
        public string GetSpArgs ( string spName , bool showfull = false ) {
            string output = "";
            string errormsg = "";
            int columncount = 0;
            DataTable dt = new DataTable ( );
            ObservableCollection<GenericClass> Generics = new ObservableCollection<GenericClass> ( );
            ObservableCollection<BankAccountViewModel> bvmparam = new ObservableCollection<BankAccountViewModel> ( );
            List<string> genericlist = new List<string> ( );
            try {
                DapperSupport . CreateGenericCollection (
                    ref Generics ,
                    "spGetSpecificSchema  " ,
                    $"{Storedprocs . SelectedItem . ToString ( )}" ,
                    "" ,
                    "" ,
                    ref genericlist ,
                    ref errormsg );
                dt = ProcessSprocs.ProcessSqlCommand ( "spGetSpecificSchema  " + spName );
                if ( dt . Rows . Count == 0 ) {
                    if ( errormsg == "" )
                        MessageBox . Show ( $"No Argument information is available" , $"[{spName}] SP Script Information" , MessageBoxButton . OK , MessageBoxImage . Warning );
                    return "";
                }
            }
            catch ( Exception ex ) {
                MessageBox . Show ( $"SQL ERROR 1125 - {ex . Message}" );
                return "";
            }
#pragma warning disable CS0219 // The variable 'count' is assigned but its value is never used
            int count = 0;
#pragma warning restore CS0219 // The variable 'count' is assigned but its value is never used
            columncount = 0;
            //			Generics . Clear ( );
            foreach ( var item in dt . Rows ) {
                GenericClass gc = new GenericClass ( );
                string store = "";
                DataRow dr = item as DataRow;
                columncount = dr . ItemArray . Count ( );
                if ( columncount == 0 )
                    columncount = 1;
                // we only need max cols - 1 here !!!
                for ( int x = 0 ; x < columncount ; x++ )
                    store += dr . ItemArray [ x ] . ToString ( ) + ",";
                output += store;
                //CreateGenericRecord ( store , gc , Generics );
            }
            if ( showfull == false ) {
                // we now have the result, so lets process them
                string buffer = output;
                string [ ] lines = buffer . Split ( '\n' );
                output = "";
                //output = $"Procedure Name : \n{SPCombo . SelectedItem . ToString ( ) . ToUpper ( )}\n\n";
                foreach ( var item in lines ) {
                    if ( ShowFullScript ) {
                        output += item;
                    }
                    else {
                        if ( item . ToUpper ( ) . Contains ( "@" ) ) {
                            if ( item [ 0 ] == '@' && item . ToUpper ( ) . Contains ( "@SQL" ) == false )
                                output += item;
                        }
                        if ( showall == false && item . ToUpper ( ) == "AS\r" )
                            break;
                    }
                }
                // we now have a list of the Args for the selected SP in output
                // Show it in a TextBox if it takes 1 or more args
                if ( output != "" )//&& UseFlowdoc )
                {
                    string fdinput = $"Procedure Name : {Storedprocs . SelectedItem . ToString ( ) . ToUpper ( )}\n\n";
                    fdinput += output;
                    fdinput += $"\n\nPress ESCAPE to close this window...\n";

                    //fdl . ShowInfo ( Flowdoc , canvas , line1: fdinput , clr1: "Black0" , line2: "" , clr2: "Black0" , line3: "" , clr3: "Black0" , header: "" , clr4: "Black0" );
                    //GridData_Display . Visibility = Visibility . Visible;
                    //SetViewButtons ( 2 , ( GridData_Display . Visibility == Visibility . Visible ? true : false ) , ( DisplayGrid . Visibility == Visibility . Visible ? true : false ) );
                    //GridData_Display . Focus ( );
                }
                else if ( output == "" ) {
                    string fdinput = $"Procedure Name : {Storedprocs . SelectedItem . ToString ( ) . ToUpper ( )}\n\n";
                    fdinput += "The requested command compleed successfuly, but no data was returned.";
                    //fdl . ShowInfo ( Flowdoc , canvas , line1: fdinput , clr1: "Black0" , line2:
                    //"This is almost certainly because this particular Stored Procedure does NOT require any Arguments..." ,
                    //clr2: "Black0" , line3: "Use the Full Script Option to confirm this.\n\nPress ESCAPE to close this window...\n" , clr3: "Black0" , header: "" , clr4: "Black0" );
                    //GridData_Display . Visibility = Visibility . Visible;
                    //SetViewButtons ( 2 , ( GridData_Display . Visibility == Visibility . Visible ? true : false ) , ( DisplayGrid . Visibility == Visibility . Visible ? true : false ) );
                    //GridData_Display . Focus ( );
                }
                else if ( Flags . UseScrollView ) {
                    Mouse . OverrideCursor = Cursors . Arrow;
                    //Utils . Mbox ( this , string1: $"Procedure [{Storedprocs . SelectedItem . ToString ( ) . ToUpper ( )}] \ndoes not Support / Require any arguments" , string2: "" , caption: "" , iconstring: "\\icons\\Information.png" , Btn1: MB . OK , Btn2: MB . NNULL , defButton: MB . OK );
                    //if ( UseFlowdoc )
                    //    fdl . ShowInfo ( Flowdoc , canvas , line1: $"Procedure [{Storedprocs . SelectedItem . ToString ( ) . ToUpper ( )}] \ndoes not Support / Require any arguments" , clr1: "Black0" , line2: "" , clr2: "Black0" , line3: "" , clr3: "Black0" , header: "" , clr4: "Black0" );
                }
            }
            ShowLoadtime ( );
            return output;
        }
        public static DataTable ProcessSqlCommand ( string SqlCommand ) {
            SqlConnection con;
            DataTable dt = new DataTable ( );
#pragma warning disable CS0219 // The variable 'filterline' is assigned but its value is never used
            string filterline = "";
#pragma warning restore CS0219 // The variable 'filterline' is assigned but its value is never used
            string ConString = Flags . CurrentConnectionString;
            //			string ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
            //Debug . WriteLine ( $"Making new SQL connection in DETAILSCOLLECTION,  Time elapsed = {timer . ElapsedMilliseconds}" );
            //SqlCommand += " TempDb";
            con = new SqlConnection ( ConString );
            try {
                Debug . WriteLine ( $"Using new SQL connection in PROCESSSQLCOMMAND" );
                using ( con ) {
                    SqlCommand cmd = new SqlCommand ( SqlCommand , con );
                    SqlDataAdapter sda = new SqlDataAdapter ( cmd );
                    sda . Fill ( dt );
                }
            }
            catch ( Exception ex ) {
                Debug . WriteLine ( $"ERROR in PROCESSSQLCOMMAND(): Failed to load Datatable :\n {ex . Message}, {ex . Data}" );
                MessageBox . Show ( $"ERROR in PROCESSSQLCOMMAND(): Failed to load datatable\n{ex . Message}" );
            }
            finally {
                Debug . WriteLine ( $" SQL data loaded from SQLCommand [{SqlCommand . ToUpper ( )}]" );
                con . Close ( );
            }
            return dt;
        }
        private void ShowSPScript ( object sender , RoutedEventArgs e ) {
            ShowFullScript = true;
            ShowSPArgs ( sender , e );
            ShowFullScript = false;

        }
        private void ExecuteSP ( object sender , RoutedEventArgs e ) {
            Storedprocs_MouseRightButtonUp ( null , null );
        }

        #endregion Data Handling

        #region Background Worker loading
        //Only Triggers a Worker thread
        public void BackgroundWorkerLoad ( ) {
            // Instantiate the Background Worker system, and then Run it.
            BackgroundWorker worker = new BackgroundWorker ( );
            worker . RunWorkerCompleted += new RunWorkerCompletedEventHandler ( worker_RunWorkerCompleted );
            worker . DoWork += new DoWorkEventHandler ( worker_DoWork );
            worker . RunWorkerAsync ( );
        }

        // The real Meat & Potatoes are here !
        private void worker_DoWork ( object sender , DoWorkEventArgs e ) {
            // Use the background worker system to execute either
            // my Background worker class ( SqlBackgroundLoad Class) methods, or
            // the Methods in the BankCollection Class
            int [ ] args = { 0 , 0 , 0 , 0 };

            if ( UseDirectLoad ) {
                // using our own SQLCOMMAND string to call
                // our Background support class using a DELEGATE declared in the DataLoadController Class
                DataLoadControl dlc = new DataLoadControl ( );
                Delegates . LoadTableDelegate glc = dlc . LoadTableInBackground;
                if ( Usetimer )
                    timer . Start ( );
                if ( CurrentType == "BANKACCOUNT" ) {
                    bankaccts = new ObservableCollection<BankAccountViewModel> ( );
                    glc ( SqlCommand , "BANKACCOUNT" , bankaccts );
                }
                else if ( CurrentType == "CUSTOMER" ) {
                    custaccts = new ObservableCollection<CustomerViewModel> ( );
                    glc ( SqlCommand , "CUSTOMER" , custaccts );
                }
                else if ( CurrentType == "SECACCOUNTS" ) {
                    detaccts = new ObservableCollection<DetailsViewModel> ( );
                    glc ( SqlCommand , "SECACCOUNTS" , detaccts );
                }
                else {
                    // WORKING 5.2.22
                    // This creates and loads a GenericClass table(genaccts)  if data is found in the selected table
                    DataTable dt =  ProcessSqlCommand  ( SqlCommand );
                    Application . Current . Dispatcher . Invoke ( ( ) => {
                        genaccts = new ObservableCollection<GenericClass> ( );
                        genaccts = SqlSupport . LoadGenericCollection ( dt );
 //                       if ( genaccts . Count == 0 ) {
  //                          if ( UseFlowdoc == true )
//                                fdl . ShowInfo ( Flowdoc , canvas , line1: $"Although the request you made was completed succesfully " , line2: $"the table [{CurrentType}] that was queried returned a zero record count, so it\nappears that it does not contain any records" , header: "Unrecognised table type Accessed" , clr4: "Red5" );
 //                       }
                    } );
                }
            }
            else {
                // default table loading methods (
                DataLoadControl dlc = new DataLoadControl ( );
                Delegates . LoadTableWithDapperDelegate glc = dlc . LoadTablewithDapper;
                DbLoadArgs dbla = new DbLoadArgs ( );
                if ( Usetimer )
                    timer . Start ( );
                if ( CurrentType == "BANKACCOUNT" ) {
                    dbla . dbname = "BANKACCOOUNT";
                    dbla . Notify = true;
                    glc ( SqlCommand , "BANKACCOUNT" , bankaccts , dbla );
                }
                else if ( CurrentType == "CUSTOMER" ) {
                    dbla . dbname = "CUSTOMERS";
                    dbla . Notify = true;
                    glc ( SqlCommand , "CUSTOMER" , custaccts , dbla );
                }
                else if ( CurrentType == "DETAILS" ) {
                    dbla . dbname = "SECACCOUNTS";
                    dbla . Notify = true;
                    glc ( SqlCommand , "SECACCOUNTS" , detaccts , dbla );
                }
                else {
                    // WORKING 5.2.22
                    // This creates and loads a GenericClass table(genaccts)  if data is found in the selected table
                    DataTable dt =  ProcessSqlCommand  ( SqlCommand );
                    genaccts = SqlSupport . LoadGenericCollection ( dt );
                }
            }
            {
                //}
                //else
                //{
                //if ( UseDirectLoad)
                //{
                //	Application . Current . Dispatcher . Invoke ( ( ) =>
                //	{
                //		bankaccts = SqlBackgroundLoad . LoadBackground_Bank (
                //		bankaccts ,
                //		SqlCommand ,
                //		"" ,
                //		"" ,
                //		"" ,
                //		false ,
                //		false ,
                //		false ,
                //		"" ,
                //		args );
                //	} );
                //}
                //else
                //{
                //	Application . Current . Dispatcher . Invoke ( ( ) =>
                //	{
                //		bankaccts = SqlBackgroundLoad . LoadBackground_Bank (
                //		bankaccts ,
                //		"Select top (50) * from bankaccount" ,
                //		"BANKACCOUNT" ,
                //		"" ,
                //		"" ,
                //		false ,
                //		false ,
                //		false ,
                //		"" ,
                //		args );
                //	} );
                //}
                //}
            }
        }

        public string CheckLimits ( ) {
            int val = 0;
            string [ ] fields = { "" , "" , "" , "" , "" , "" , "" , "" , "" , "" };
            DataLoadControl dlc = new DataLoadControl ( );
            Delegates . LoadTableDelegate glc = dlc . LoadTableInBackground;
            fields [ 0 ] = "select";
            if ( LoadAll == false && RecCount . Text != "" && RecCount . Text != "*" ) {
                if ( int . TryParse ( RecCount . Text , out val ) == true )
                    fields [ 1 ] = $" top ({RecCount . Text}) * ";
                else
                    fields [ 1 ] = $" *";
            }
            else
                fields [ 1 ] = $" *";
            if ( dbName . Text != "" )
                fields [ 2 ] = $" from {dbName . Text} ";
            else
                return "";      // no DbName to select from, so abort
            if ( LoadAll == false ) {
                if ( Conditions . Text != "" && Conditions . Text != "limits..." )
                    fields [ 3 ] = $" where {Conditions . Text} ";
                if ( orderby . Text != "" && orderby . Text != "Order by..." )
                    fields [ 4 ] = $" order by {orderby . Text}";
            }
            SqlCommand = fields [ 0 ] + fields [ 1 ] + fields [ 2 ] + fields [ 3 ] + fields [ 4 ];
            return SqlCommand;

        }

        // This handles the return value from a background thread, BUT it is running on the main UI thread,
        // so we can access controls normally
        // It is called automatically by the Background Worker system
        private void worker_RunWorkerCompleted ( object sender , RunWorkerCompletedEventArgs e ) {
            LoadGrid ( );
        }

        #endregion Background Worker loading

        #region Utility  support Methods
        // Create SQLCommand string from fields on UI
        public string GetSqlCommand ( int count = 0 , int table = 0 , string condition = "" , string sortorder = "" ) {
            // Parse fields into a valid SQL Command string
            string output = "Select  ";
            output += count == 0 ? " * From " : $"top ({count}) * From ";
            output += dbName . SelectedItem . ToString ( );
            output += condition . Trim ( ) != "" ? " Where " + condition + " " : "";
            output += sortorder . Trim ( ) != "" ? " Order by " + sortorder . Trim ( ) : "";
            CurrentType = dbName . Items [ table ] . ToString ( ) . ToUpper ( );
            return output;
        }

        // Just Assign data to grids to display it
        private void LoadGrid ( object obj = null ) {

            ShowLoadtime ( );

            // Load whatever data we have received into DataGrid
            if ( CurrentType . ToUpper ( ) == "BANKACCOUNT" ) {
                if ( bankaccts == null )
                    return;
                DataGrid1 . ItemsSource = bankaccts;
                DbCount = bankaccts . Count;
                if ( UseFlowdoc == true )
//                    fdl . ShowInfo ( Flowdoc , canvas , line1: $"The requested table [{CurrentType}] was loaded successfully, and the {DbCount} records returned are displayed in the table below" , clr1: "Black0" ,
                    //line2: $"The command line used was" , clr2: "Red2" ,
                    //line3: $"{SqlCommand . ToUpper ( )}" , clr3: "Blue4" ,
                    //header: "Bank Accounts data table" , clr4: "Red5" );
                DataGrid1 . SelectedIndex = 0;
                DataGrid1 . Focus ( );
                ShowTableStructure ( );
            }
            else if ( CurrentType . ToUpper ( ) == "CUSTOMER" ) {
                if ( custaccts == null )
                    return;
                DataGrid1 . ItemsSource = custaccts;
                DbCount = custaccts . Count;
                if ( UseFlowdoc == true )
                    //fdl . ShowInfo ( Flowdoc , canvas , line1: $"The requested table [{CurrentType}] was loaded successfully, and the {DbCount} records returned are displayed in the table below" , clr1: "Black0" ,
                    //                    line2: $"The command line used was" , clr2: "Red2" ,
                    //                    line3: $"{SqlCommand . ToUpper ( )}" , clr3: "Blue4" ,
                    //                    header: "All Customers data table" , clr4: "Red5" );
                DataGrid1 . SelectedIndex = 0;
                DataGrid1 . Focus ( );
                ShowTableStructure ( );
            }
            else if ( CurrentType . ToUpper ( ) == "SECACCOUNTS" ) {
                if ( detaccts == null )
                    return;
                DataGrid1 . ItemsSource = detaccts;
                DbCount = detaccts . Count;
                if ( UseFlowdoc == true )
                    //fdl . ShowInfo ( Flowdoc , canvas , line1: $"The requested table [{CurrentType}] was loaded successfully, and the {DbCount} records returned are displayed in the table below" , clr1: "Black0" ,
                    //line2: $"The command line used was" , clr2: "Red2" ,
                    //                    line3: $"{SqlCommand . ToUpper ( )}" , clr3: "Blue4" ,
                    //                    header: "Secondary Accounts data table" );
                DataGrid1 . SelectedIndex = 0;
                DataGrid1 . Focus ( );
                ShowTableStructure ( );
            }
            else {
                if ( genaccts . Count == 0 ) {
                    if ( UseFlowdoc == true ) {
                        //fdl . ShowInfo ( Flowdoc , canvas , line1: $"The requested table [ {CurrentType} ] succeeded, but returned Zero rows of data." , clr1: "Green5" , header: "It is quite likely that the table is actually empty !" , clr4: "Cyan1" );
                    }
                    GenericClass gc = new GenericClass ( );
                    //gc . field1 = $"Sorry, but no data was returned for the '{CurrentType}' Database Table  you requested...";
                    //genaccts . Add ( gc );
                    //SqlServerCommands . LoadActiveRowsOnlyInGrid ( DataGrid1, genaccts , SqlServerCommands . GetGenericColumnCount ( genaccts ) );
                    GenericDbUtilities . SetNullRecords ( genaccts , DataGrid1 , CurrentType );
                    //					DataGrid1. Columns [ 0 ] . Header = "Error Message";
                    DataGrid1 . Refresh ( );
                    return;
                }
                // Caution : This loads the data into the Datarid with only the selected rows
                // //visible in the grid so do NOT repopulate the grid after making this call
                //				SqlServerCommands sqlc = new SqlServerCommands();
                DataGrid1 . ItemsSource = null;
                DataGrid1 . Items . Clear ( );
                DataGrid1 . Columns . Clear ( );
                DataGrid1 . ItemsSource = genaccts;
                ShowTableStructure ( );
                SqlServerCommands . LoadActiveRowsOnlyInGrid ( DataGrid1 , genaccts , SqlServerCommands . GetGenericColumnCount ( genaccts ) );
                if ( Flags . ReplaceFldNames ) {
                    GenericDbUtilities . ReplaceDataGridFldNames ( CurrentType , ref DataGrid1 );
                }
                DbCount = genaccts . Count;
                if ( UseFlowdoc == true )
                    //fdl . ShowInfo ( Flowdoc , canvas , header: "Unrecognised table accessed successfully" , clr4: "Red5" ,
                    //line1: $"Request made was completed succesfully!" , clr1: "Red3" ,
                    //line2: $"the table [{CurrentType}] that was queried returned a record count of {DbCount}.\nThe structure of this data is not recognised, so a generic structure has been used..." ,
                    //line3: $"{SqlCommand . ToUpper ( )}" , clr3: "Blue4" );
                DataGrid1 . SelectedIndex = 0;
                DataGrid1 . Focus ( );
                ShowTableStructure ( );

            }
        }


        //Get list of all Tables in our Db (Ian1.MDF)
        public void LoadTablesList ( ) {
            int bankindex = 0, count = 0;
            List<string> list = new List<string> ( );
            SqlCommand = "spGetTablesList";
            dbName . Items . Clear ( );
            CallStoredProcedure ( list , SqlCommand );
            //This call returns us a DataTable
            DataTable dt =  ProcessSqlCommand  ( SqlCommand );
            //			Grid2 . ItemsSource = dt . DefaultView;
            //			Grid2 . Refresh ( );
            // This how to access  Row data from  a grid the easiest way.... parsed into a List <xxxxx>
            list = Utils . GetDataDridRowsAsListOfStrings ( dt );
            foreach ( string row in list ) {
                dbName . Items . Add ( row );
                if ( row . ToUpper ( ) == "BANKACCOUNT" )
                    bankindex = count;
                count++;
            }
            // how to Sort Combo/Listbox contents
            dbName . Items . SortDescriptions . Add ( new SortDescription ( "" , ListSortDirection . Ascending ) );
            dbName . SelectedIndex = bankindex;
            TablesPanel . ItemsSource = null;
            TablesPanel . Items . Clear ( );
            TablesPanel . ItemsSource = list;
        }

        // load a list of all SP's
        private void LoadSpList ( ) {
            List<string> SpList = new List<string> ( );
            SpList = CallStoredProcedure ( SpList , "spGetStoredProcs" );
            Storedprocs . ItemsSource = SpList;
            Storedprocs . Items . SortDescriptions . Add ( new SortDescription ( "" , ListSortDirection . Ascending ) );
            Storedprocs . SelectedIndex = 0;
            Storedprocs . SelectedItem = 0;
            Storedprocs . Refresh ( );

        }



        #endregion Utility  support Methods

        #region Trigger methods  for Stored Procedures (string, Int, Double, Decimal) that return a List<xxxxx>
        // These all return just a single column from any table by calling a Stored Procedure  in MSSQL Server
        public static List<string> CallStoredProcedureWithSizes ( List<string> list , string sqlcommand ) {
            //This call returns us a DataTable
            DataTable dt =  ProcessSqlCommand  ( sqlcommand );
            if ( dt != null )
                list = GenericDbUtilities . GetDataDridRowsWithSizes ( dt );
            //list = Utils . GetDataDridRowsAsListOfStrings ( dt );
            return list;
        }
        public static List<string> CallStoredProcedure ( List<string> list , string sqlcommand ) {
            //This call returns us a DataTable
            DataTable dt =  ProcessSqlCommand  ( sqlcommand );
            if ( dt != null )
                //				list = GenericDbHandlers.GetDataDridRowsWithSizes ( dt );
                list = Utils . GetDataDridRowsAsListOfStrings ( dt );
            return list;
        }
        public static List<int> CallStoredProcedure ( List<int> list , string sqlcommand ) {
            //This call returns us a DataTable
            DataTable dt =  ProcessSqlCommand  ( sqlcommand );
            list = Utils . GetDataDridRowsAsListOfInts ( dt );
            return list;
        }
        public static List<double> CallStoredProcedure ( List<double> list , string sqlcommand ) {
            //This call returns us a DataTable
            DataTable dt =  ProcessSqlCommand  ( sqlcommand );
            list = Utils . GetDataDridRowsAsListOfDoubles ( dt );
            return list;
        }
        public static List<decimal> CallStoredProcedure ( List<decimal> list , string sqlcommand ) {
            //This call returns us a DataTable
            DataTable dt =  ProcessSqlCommand  ( sqlcommand );
            list = Utils . GetDataDridRowsAsListOfDecimals ( dt );
            return list;
        }
        public static List<DateTime> CallStoredProcedure ( List<DateTime> list , string sqlcommand ) {
            //This call returns us a DataTable
            DataTable dt =  ProcessSqlCommand  ( sqlcommand );
            list = Utils . GetDataDridRowsAsListOfDateTime ( dt );
            return list;
        }
        #endregion Trigger methods  for Stored Procedures

        #region Mouse handlers
        private void dbName_PreviewMouseRightButtonUp ( object sender , MouseButtonEventArgs e ) {
            string currsel = dbName . SelectedItem . ToString ( );
            e . Handled = true;
            dbName . Items . Clear ( );
            LoadTablesList ( );
            for ( int x = 0 ; x < dbName . Items . Count ; x++ ) {
                if ( dbName . Items [ x ] . ToString ( ) . ToUpper ( ) == currsel ) {
                    dbName . SelectedIndex = x;
                    break;
                }
            }
        }
        private void scrollview_Click ( object sender , RoutedEventArgs e ) {
            Flags . UseScrollView = !Flags . UseScrollView;
        }
        private void Storedprocs_MouseRightButtonUp ( object sender , MouseButtonEventArgs e ) {
            string errmsg = "";
            string args = "";
            DataGrid1 . ItemsSource = null;
            DataGrid1 . Refresh ( );
            ShowTableStructure ( );
            string cmd = Storedprocs . SelectedItem . ToString ( );
            if ( SpArgs . Text != "" && SpArgs . Text . Contains ( "Enter Arg" ) == false )
                args += $"{SpArgs . Text}";
            genaccts = SqlSupport . ExecuteStoredProcedure ( cmd , genaccts , out errmsg , Arguments: args );
            DbCount = genaccts . Count;
            if ( DbCount > 0 ) {
                SqlServerCommands . LoadActiveRowsOnlyInGrid ( DataGrid1 , genaccts , SqlServerCommands . GetGenericColumnCount ( genaccts ) );
                if ( Flags . ReplaceFldNames ) {
                    GenericDbUtilities . ReplaceDataGridFldNames ( CurrentType , ref DataGrid1 );
                }
                //				DataGrid1. ItemsSource = genaccts;
                //				if ( UseFlowdoc == true )
                if ( UseFlowdoc )
                    //fdl . ShowInfo ( Flowdoc , canvas , line1: $"Stored Procedure was completed successfully, and returned the {DbCount} records shown in the Grid below !" , "Black0" ,
                    //line2: $"Procedure executed was :" , clr2: "Black0" , line3: $"{cmd . ToUpper ( )}" , clr3: "" , header: "Stored Procedure execution" , "Orange1" );
                ShowTableStructure ( );
            }
            else if ( errmsg != "" ) {
                //				if ( UseFlowdoc == true )
                //if ( UseFlowdoc )
                    //fdl . ShowInfo ( Flowdoc , canvas , line1: $"Stored Procedure [{cmd . ToUpper ( )}] returned the following information !\n\n[{errmsg}]\n " , "Red4" , "Stored Procedure execution" , "Orange1" );
            }
        }
        #endregion Mouse handlers

        #region keyboard handlers
        private void Window_PreviewKeyDown ( object sender , System . Windows . Input . KeyEventArgs e ) {
            // Allow quick window close, (but only close FlowDoc if it is currently open)
            //if ( e . Key == Key . Escape && Flowdoc . Visibility == Visibility . Visible )
            //    Flowdoc . Visibility = Visibility . Hidden;       // Just hide  the FlowDoc
            //else if ( e . Key == Key . Escape )
            //    this . Close ( );           // Close the window
            //else if ( e . Key == Key . F8 ) {
            //    // Short form usageo f a 2 /3 line FlowDoc
            //    fdmsg ( "A test of my message system with only one line of text only supplied to ensure it prefills the other two lines" );
            //}
        }

        #endregion keyboard handlers

        #region GotFocus
        private void Args_GotFocus ( object sender , RoutedEventArgs e ) {
            SpArgs . Background = FindResource ( "White0" ) as SolidColorBrush;
            SpArgs . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
            if ( SpArgs . Text == "Enter Arguments  for current S.P" )
                SpArgs . Text = "";
        }
        //Handle gray text in text fields on field entry
        private void tb_GotFocus ( object sender , RoutedEventArgs e ) {
            TextBox tb = sender as TextBox;
            tb . Background = FindResource ( "White0" ) as SolidColorBrush;
            tb . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
            tb . CaretBrush = FindResource ( "Red0" ) as SolidColorBrush;
            //tb . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
            tb . Text = "";
        }

        #endregion GotFocus

        #region LostFocus
        private void SpArgs_LostFocus ( object sender , RoutedEventArgs e ) {
            SpArgs . Background = FindResource ( "Gray2" ) as SolidColorBrush;
            if ( SpArgs . Text == "" ) {
                SpArgs . Text = "Enter Arguments  for current S.P";
            }
        }

        private void Conditions_LostFocus ( object sender , RoutedEventArgs e ) {
            if ( Conditions . Text == "" )
                Conditions . Background = FindResource ( "Gray2" ) as SolidColorBrush;

        }

        private void orderby_LostFocus ( object sender , RoutedEventArgs e ) {
            if ( orderby . Text == "" )
                orderby . Background = FindResource ( "Gray2" ) as SolidColorBrush;
        }
        #endregion LostFocus	

        #region Checkbox handlers
        private void ShowInfo_Click ( object sender , RoutedEventArgs e ) {
            UseFlowdoc = !UseFlowdoc;
            Showinfo . IsChecked = UseFlowdoc;
        }
        private void Timer_Click ( object sender , RoutedEventArgs e ) {
            Usetimer = !Usetimer;
            LoadTime . Text = "xxx";
        }
        private void ShowLoadtime ( ) {
            if ( Usetimer ) {
                timer . Stop ( );
                if ( timer . ElapsedMilliseconds != 0 )
                    LoadTime . Text = timer . ElapsedMilliseconds . ToString ( ) + " m/secs";
                timer . Reset ( );
            }
        }
        private void checkBox_Click ( object sender , RoutedEventArgs e ) {
            var v = sender as CheckBox;
            //if ( v . IsChecked == true ) {
            //    Flags . PinToBorder = true;
            //    /// Move it to top left corbner
            //    ( Flowdoc as FrameworkElement ) . SetValue ( Canvas . LeftProperty , ( double ) 0 );
            //    ( Flowdoc as FrameworkElement ) . SetValue ( Canvas . TopProperty , ( double ) 0 );
            //}
            //else
            //    Flags . PinToBorder = false;
        }



        #endregion Checkbox handlers

        #region FlowDoc support
        /// <summary>
        ///  These are the only methods any window needs ot provide support for my FlowDoc system.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       #region FlowDoc methods
        // Allows this class to control maximizing FlowDoc window
        public event EventHandler ExecuteFlowDocMaxmizeMethod;
        protected virtual void OnExecuteMethod ( ) {
            if ( ExecuteFlowDocMaxmizeMethod != null )
                ExecuteFlowDocMaxmizeMethod ( this , EventArgs . Empty );
        }
        private void Image_PreviewMouseLeftButtonUp ( object sender , MouseButtonEventArgs e ) {
            //allows remote window to maximize /resize  this control ?
            OnExecuteMethod ( );
        }
        #endregion FlowDoc methods

        protected void MaximizeFlowDoc ( object sender , EventArgs e ) {
            // Clever "Hook" method that Allows the flowdoc to be resized to fill window
            // or return to its original size and position courtesy of the Event declard in FlowDoc
            OnExecuteMethod ( );
            //fdl . MaximizeFlowDoc ( Flowdoc , canvas , e );
        }
        private void Flowdoc_MouseLeftButtonUp ( object sender , MouseButtonEventArgs e ) {
            // Window wide  !!
            // Called  when a Flowdoc MOVE has ended
            //MovingObject = fdl . Flowdoc_MouseLeftButtonUp ( sender , Flowdoc , MovingObject , e );
            ReleaseMouseCapture ( );
        }
        private void Flowdoc_PreviewMouseLeftButtonDown ( object sender , MouseButtonEventArgs e ) {
            //In this event, we get current mouse position on the control to use it in the MouseMove event.
            //MovingObject = fdl . Flowdoc_PreviewMouseLeftButtonDown ( sender , Flowdoc , e );
        }
        private void Flowdoc_MouseMove ( object sender , MouseEventArgs e ) {
            // We are Resizing the Flowdoc using the mouse on the border  (Border.Name=FdBorder)
            //fdl . Flowdoc_MouseMove ( Flowdoc , canvas , MovingObject , e );
        }
        // Shortened version proxy call		
        private void Flowdoc_LostFocus ( object sender , RoutedEventArgs e ) {
//            Flowdoc . BorderClicked = false;
        }
        public void FlowDoc_ExecuteFlowDocBorderMethod ( object sender , EventArgs e ) {
            // EVENTHANDLER to Handle resizing
            //FlowDoc fd = sender as FlowDoc;
            Point pt = Mouse . GetPosition ( canvas );
            double dLeft = pt . X;
            double dTop = pt . Y;
        }
        public void fdmsg ( string line1 , string line2 = "" , string line3 = "" ) {
            //We have to pass the Flowdoc.Name, and Canvas.Name as well as up   to 3 strings of message
            //  you can  just provie one if required
            // eg fdmsg("message text");
            //fdl . FdMsg ( Flowdoc , canvas , line1 , line2 , line3 );
        }
        #endregion FlowDoc support

        private void UseColumnNames_Click ( object sender , RoutedEventArgs e ) {
            if ( UseTrueColNames . IsChecked == true )
                Flags . ReplaceFldNames = true;
            else
                Flags . ReplaceFldNames = false;
        }

        private void ViewTableColumns ( object sender , RoutedEventArgs e ) {
            bool flowdocswitch = false;
#pragma warning disable CS0219 // The variable 'count' is assigned but its value is never used
            int count = 0;
#pragma warning restore CS0219 // The variable 'count' is assigned but its value is never used
            List<string> list = new List<string> ( );
            List<string> fldnameslist = new List<string> ( );
            string output = "";
            SqlCommand = $"spGetTableColumnWithSize {dbName . SelectedItem . ToString ( )}";
            //SqlCommand = SqlCommand = $"spGetTableColumns";
            fldnameslist = Datagrids . CallStoredProcedureWithSizes ( list , SqlCommand );
            output = Utils . ParseTableColumnData ( fldnameslist );

            // Fiddle  to allow Flowdoc  to show Field info even though Flowdoc use is disabled
            if ( UseFlowdoc == false ) {
                flowdocswitch = true;
                UseFlowdoc = true;
            }
            //Debug. WriteLine ( $"loaded {count} records for table columns" );
            if ( UseFlowdoc )
                //Flowdoc . ShowInfo ( Flowdoc , canvas , header: "Table Columns informaton accessed successfully" , clr4: "Red5" ,
                //line1: $"Request made was completed succesfully!" , clr1: "Red3" ,
                //line2: $"the structure of the table [{dbName . SelectedItem . ToString ( )}] is listed below : \n{output}" ,
                //line3: $"Results created by Stored Procedure : \n({SqlCommand . ToUpper ( )})" , clr3: "Blue4"
                //);
            if ( flowdocswitch == true ) {
                flowdocswitch = false;
                UseFlowdoc = false;
            }

        }

        #region Horizontal splitter resize handlers
        /// <summary>
        /// Right side horizontal slitter
        /// </summary>
        private void Splitter_DragStarted ( object sender , System . Windows . Controls . Primitives . DragStartedEventArgs e ) {
            if ( Row0 . ActualHeight <= 3 ) {
                imgup = new BitmapImage ( new Uri ( @"\icons\down arroiw red.png" , UriKind . Relative ) );
                //                imgup = new BitmapImage ( new Uri ( @"\icons\sync.ico" , UriKind . Relative ) );
                ShowdragText = "Drag";
                ShowText = "Show upper DataGrid";
                //                RotateTransform rotateTransform1 = new RotateTransform ( 90 , -15 , 15 );
            }
            else if ( Row1 . ActualHeight <= 21 ) {
                imgup = new BitmapImage ( new Uri ( @"\icons\up arroiw red.png" , UriKind . Relative ) );
                ShowdragText = "Drag";
                ShowText = "Show lower panel";
            }
            else {
                imgup = new BitmapImage ( new Uri ( @"\icons\Lrg updown arrow red copy.png" , UriKind . Relative ) );
                ShowdragText = "Drag";
                ShowText = "Adjust view proportions";
                //                imgup = new BitmapImage ( new Uri ( @"\icons\sync.ico" , UriKind . Relative ) );
                //                ShowText = "Adjust View";
                //ShowText = "   show Data Access panel";
            }
            magnifyeimage . UpdateLayout ( );
            Magnifyrate . UpdateLayout ( ); ;
        }
        private void Splitter_DragCompleted ( object sender , System . Windows . Controls . Primitives . DragCompletedEventArgs e ) {
            if ( Row0 . ActualHeight <= 3 ) {
                imgup = new BitmapImage ( new Uri ( @"\icons\down arroiw red.png" , UriKind . Relative ) );
                ShowdragText = "Drag";
                ShowText = "to view Db Tables DataGrid";
                double SplitterOffset = ( this . ActualHeight - DataGrid1 . ActualHeight ) - 140;
                SPselection . Height = SplitterOffset > 0 ? SplitterOffset : 0;
                listbox . Height = SPselection . Height;
            }
            else if ( Row1 . ActualHeight <= 21 ) {
                imgup = new BitmapImage ( new Uri ( @"\icons\up arroiw red.png" , UriKind . Relative ) );
                ShowdragText = "Drag";
                ShowText = "to view Data Access panel";
                double SplitterOffset = ( this . ActualHeight - DataGrid1 . ActualHeight ) - 140;
                SPselection . Height = SplitterOffset > 0 ? SplitterOffset : 0;
                listbox . Height = SPselection . Height;
            }
            else {
                imgup = new BitmapImage ( new Uri ( @"\icons\Lrg updown arrow red copy.png" , UriKind . Relative ) );
                //                imgmv = new BitmapImage ( new Uri ( @"\icons\sync.ico" , UriKind . Relative ) );
                ShowdragText = "Drag";
                ShowText = "Adjust view proportions";
                double SplitterOffset = ( this . ActualHeight - DataGrid1 . ActualHeight ) - 140;
                SPselection . Height = SplitterOffset > 0 ? SplitterOffset : 0;
                Debug . WriteLine ( $"{SplitterOffset}, {SPselection . Height}" );
                listbox . Height = SPselection . Height;
            }
            magnifyeimage . UpdateLayout ( );
            Magnifyrate . UpdateLayout ( ); ;
        }
        #endregion Horizontal splitter resize handlers

        #region Vertical splitter resize handlers
        private void VSplitter_DragStarted ( object sender , DragStartedEventArgs e ) {
            //vimgmove = null;
            if ( Col0 . ActualWidth >= MaxColWidth1 ) {
                vimgmove = new BitmapImage ( new Uri ( @"\icons\left arroiw red.png" , UriKind . Relative ) );
            }
            else if ( Col0 . ActualWidth <= 11 ) {
                vimgmove = new BitmapImage ( new Uri ( @"\icons\right arroiw red.png" , UriKind . Relative ) );
            }
            else {
                vimgmove = new BitmapImage ( new Uri ( @"\icons\Lrg ltrt arrow red copy.png" , UriKind . Relative ) );
            }
        }
        private void VSplitter_DragCompleted ( object sender , DragCompletedEventArgs e ) {
            //            vimgmove = new BitmapImage ( new Uri ( @"\icons\left arroiw red.png" , UriKind . Relative ) );

            if ( Col0 . ActualWidth >= MaxColWidth1 ) {
                vimgmove = new BitmapImage ( new Uri ( @"\icons\left arroiw red.png" , UriKind . Relative ) );
            }
            else if ( Col0 . ActualWidth <= 11 ) {
                vimgmove = new BitmapImage ( new Uri ( @"\icons\right arroiw red.png" , UriKind . Relative ) );
            }
            else {
                vimgmove = new BitmapImage ( new Uri ( @"\icons\Lrg ltrt arrow red copy.png" , UriKind . Relative ) );
                //vimgmove . Rotation = Rotation . Rotate180;
            }
        }
        #endregion Vertical splitter resize handlers

        private void LeftSplitter_DragStarted ( object sender , DragStartedEventArgs e ) {
            if ( lsplitrow1 . ActualHeight <= MinRowHeight1 ) {
                LeftSplitterText = "Drag Up or Down  ";
                LhHsplitter = new BitmapImage ( new Uri ( @"\icons\Lrg updown arrow red copy.png" , UriKind . Relative ) );
                //                LhHsplitter = new BitmapImage ( new Uri ( @"\icons\down arroiw red copy.png" , UriKind . Relative ) );
                //              LhHsplitter = new BitmapImage ( new Uri ( @"\icons\up arroiw red.png" , UriKind . Relative ) );
            }
            else if ( lsplitrow1 . ActualHeight <= 10 ) {
                LeftSplitterText = "Drag Down ";
                LhHsplitter = new BitmapImage ( new Uri ( @"\icons\down arroiw red.png" , UriKind . Relative ) );
            }
            else {
                LeftSplitterText = "Drag Up or Down  ";
                LhHsplitter = new BitmapImage ( new Uri ( @"\icons\Lrg updown arrow red copy.png" , UriKind . Relative ) );
            }
        }
        private void LeftSplitter_DragCompleted ( object sender , DragCompletedEventArgs e ) {
            if ( lsplitrow1 . ActualHeight >= Maingrid . ActualHeight - 100 ) {
                LeftSplitterText = "Drag Up  ";
                LhHsplitter = new BitmapImage ( new Uri ( @"\icons\up arroiw red.png" , UriKind . Relative ) );
            }
            else if ( lsplitrow1 . ActualHeight <= 11 ) {
                LeftSplitterText = "Drag Down  ";
                LhHsplitter = new BitmapImage ( new Uri ( @"\icons\down arroiw red.png" , UriKind . Relative ) );
            }
            else {
                LeftSplitterText = "Drag Up or Down  ";
                LhHsplitter = new BitmapImage ( new Uri ( @"\icons\Lrg updown arrow red copy.png" , UriKind . Relative ) );
            }
            //         LeftSplitterText = "Drag Up/Down to access secondary viewers ";
        }


        private void Window_PreviewMouseMove ( object sender , MouseEventArgs e ) {
        }

        private void DataGrid1_PreviewMouseRightButtonDown ( object sender , MouseButtonEventArgs e ) {
            if ( DataGrid1 . SelectedIndex != -1 )
                ParseGridRowData ( );
        }
        private void ShowTableStructure ( ) {
            string output = "";
            string [ ] lines;
            char ch = '\n';
            listbox . ItemsSource = null;
            listbox . Items . Clear ( );
            if ( DataGrid1 . Items . Count == 0 )
                return;
            List<string> list = new List<string> ( );
            // Gets the Db Table structure, with narchar sizes
            list = DbRowToList ( DataGrid1 , out output );
            list . Clear ( );
            lines = output . Split ( ch );
            for ( int x = 0 ; x < lines . Length ; x++ ) {
                list . Add ( lines [ x ] );
            }
            listbox . ItemsSource = list;

            //DataGrid1 . SelectedItem

        }
        //===============================================================================
        /// <summary>
        /// Special method to check the data format we are going to write to the CSV file 
        /// and creates the output line by line from a datarow of the DataTable we have just read in
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="objRow"></param>
        /// <returns></returns>
        private List<string> DbRowToList ( DataGrid dgrid , out string output ) {
            output = "";
#pragma warning disable CS0219 // The variable 'flowdocswitch' is assigned but its value is never used
            bool flowdocswitch = false;
#pragma warning restore CS0219 // The variable 'flowdocswitch' is assigned but its value is never used
#pragma warning disable CS0219 // The variable 'count' is assigned but its value is never used
            int count = 0;
#pragma warning restore CS0219 // The variable 'count' is assigned but its value is never used
            List<string> list = new List<string> ( );
            List<string> fldnameslist = new List<string> ( );
            SqlCommand = $"spGetTableColumnWithSize {dbName . SelectedItem . ToString ( )}";
            //SqlCommand = SqlCommand = $"spGetTableColumns";
            fldnameslist = Datagrids . CallStoredProcedureWithSizes ( list , SqlCommand );
            output = Utils . ParseTableColumnData ( fldnameslist );
            return fldnameslist;
        }

        private void ParseGridRowData ( ) {
#pragma warning disable CS0219 // The variable 'dbtype' is assigned but its value is never used
            int dbtype = -1;
#pragma warning restore CS0219 // The variable 'dbtype' is assigned but its value is never used
#pragma warning disable CS0219 // The variable 'record' is assigned but its value is never used
            string record = "";
#pragma warning restore CS0219 // The variable 'record' is assigned but its value is never used
            string tablename = dbName . SelectedItem . ToString ( );


            object vn = new object ( );
            vn = DataGrid1 . SelectedItem as BankAccountViewModel;
            if ( vn != null ) {
                DbRecordInfo . ItemsSource = null;
                DbRecordInfo . Items . Clear ( );
                DbRecordInfo . ItemsSource = GetBankDataRecord ( vn as BankAccountViewModel );
                //if ( DbRecordInfo . Items . Count > 0 )
                //{
                //    fdmsg ( $"The data for the selected record has been recovered and Can be viewed by sliding  the left side horizontal spliter downwards" );
                //}
            }
            else {
                vn = DataGrid1 . SelectedItem as CustomerViewModel;
                if ( vn != null ) {
                    DbRecordInfo . ItemsSource = null;
                    DbRecordInfo . Items . Clear ( );
                    DbRecordInfo . ItemsSource = GetCustDataRecord ( vn as CustomerViewModel );
                    //if ( DbRecordInfo . Items . Count > 0 )
                    //{
                    //    fdmsg ( $"The data for the selected record has been recovered and Can be viewed by sliding  the left side horizontal spliter downwards" );
                    //}
                }
                else {
                    vn = DataGrid1 . SelectedItem as GenericClass;
                    if ( vn != null ) {
                        GetGenericDataRecord ( vn as GenericClass );
                    }
                    else {
                        //                       List<string> list = new List<string> ( );
                        //                        list = CreateTableStructure ( DataGrid1.SelectedItem.ToString());
                        DbRecordInfo . ItemsSource = null;
                        DbRecordInfo . Items . Clear ( );
                        DbRecordInfo . ItemsSource = CreateTableStructure ( DataGrid1 . SelectedItem . ToString ( ) );
                    }
                    return;
                }
            }
        }
        private List<string> CreateTableStructure ( string rowdata ) {
            List<string> list = new List<string> ( );
            string [ ] lines;
            string [ ] entry;
#pragma warning disable CS0219 // The variable 'itm' is assigned but its value is never used
            string itm = "";
#pragma warning restore CS0219 // The variable 'itm' is assigned but its value is never used
            char ch = ',';
            lines = rowdata . Split ( ch );
            for ( int x = 0 ; x < lines . Length ; x++ ) {
                entry = lines [ x ] . Split ( '=' );
                if ( entry . Length == 1 )
                    return null;
                if ( entry [ 1 ] . Contains ( "}" ) )
                    list . Add ( entry [ 1 ] . Substring ( 0 , entry [ 1 ] . Length - 1 ) );
                else
                    list . Add ( entry [ 1 ] );
            }
            return list;
        }
        private List<string> GetGenericDataRecord ( GenericClass bvm ) {
            List<string> list = new List<string> ( );
#pragma warning disable CS0219 // The variable 's' is assigned but its value is never used
            string s = "";
#pragma warning restore CS0219 // The variable 's' is assigned but its value is never used
            list . Add ( bvm . field1 . ToString ( ) );
            list . Add ( bvm . field2 . ToString ( ) );
            list . Add ( bvm . field3 . ToString ( ) );
            list . Add ( bvm . field4 . ToString ( ) );
            list . Add ( bvm . field5 . ToString ( ) );
            list . Add ( bvm . field6 . ToString ( ) );
            list . Add ( bvm . field7 . ToString ( ) );
            list . Add ( bvm . field8 . ToString ( ) );
            list . Add ( bvm . field9 . ToString ( ) );
            list . Add ( bvm . field10 . ToString ( ) );
            list . Add ( bvm . field11 . ToString ( ) );
            list . Add ( bvm . field12 . ToString ( ) );
            list . Add ( bvm . field13 . ToString ( ) );
            list . Add ( bvm . field14 . ToString ( ) );
            list . Add ( bvm . field15 . ToString ( ) );
            list . Add ( bvm . field16 . ToString ( ) );
            list . Add ( bvm . field17 . ToString ( ) );
            list . Add ( bvm . field18 . ToString ( ) );
            list . Add ( bvm . field19 . ToString ( ) );
            list . Add ( bvm . field20 . ToString ( ) );
            return list;
        }

        private List<string> GetCustDataRecord ( CustomerViewModel bvm ) {
            List<string> list = new List<string> ( );
#pragma warning disable CS0219 // The variable 's' is assigned but its value is never used
            string s = "";
#pragma warning restore CS0219 // The variable 's' is assigned but its value is never used
            list . Add ( bvm . Id . ToString ( ) );
            list . Add ( bvm . CustNo );
            list . Add ( bvm . BankNo );
            list . Add ( bvm . AcType . ToString ( ) );
            list . Add ( bvm . FName );
            list . Add ( bvm . LName );
            list . Add ( bvm . Addr1 );
            list . Add ( bvm . Addr2 );
            list . Add ( bvm . Town );
            list . Add ( bvm . County );
            list . Add ( bvm . PCode );
            list . Add ( bvm . Phone );
            list . Add ( bvm . Mobile );
            list . Add ( bvm . Dob . ToString ( ) ); ;
            list . Add ( bvm . ODate . ToString ( ) );
            list . Add ( bvm . CDate . ToString ( ) );
            return list;
        }
        private List<string> GetBankDataRecord ( BankAccountViewModel bvm ) {
            List<string> list = new List<string> ( );
#pragma warning disable CS0219 // The variable 's' is assigned but its value is never used
            string s = "";
#pragma warning restore CS0219 // The variable 's' is assigned but its value is never used
            list . Add ( bvm . Id . ToString ( ) );
            list . Add ( bvm . CustNo );
            list . Add ( bvm . BankNo );
            list . Add ( bvm . AcType . ToString ( ) );
            list . Add ( bvm . IntRate . ToString ( ) );
            list . Add ( bvm . Balance . ToString ( ) );
            list . Add ( bvm . ODate . ToString ( ) );
            list . Add ( bvm . CDate . ToString ( ) );
            return list;

            {
                //    char [ ] ch = { ' ' };
                //    char [ ] ch2 = { '/' };
                //    s = $"{objRow [ "Odate" ] . ToString ( )}', '";
                //    odat = s . Split ( ch );
                //    string odate = odat [ 0 ];
                //    // now reverse it  to YYYY/MM/DD format as this is what SQL understands
                //    revstr = odate . Split ( ch2 );
                //    odate = revstr [ 2 ] + "/" + revstr [ 1 ] + "/" + revstr [ 0 ];
                //    // thats  the Open date handled - now do close data
                //    s = $"{objRow [ "cDate" ] . ToString ( )}', '";
                //    cdat = s . Split ( ch );   // split date on '/'
                //    string cdate = cdat [ 0 ];
                //    // now reverse it  to YYYY/MM/DD format as this is what SQL understands
                //    revstr = cdate . Split ( ch2 );
                //    cdate = revstr [ 2 ] + "/" + revstr [ 1 ] + "/" + revstr [ 0 ];
                //    string acTypestr = objRow [ "AcType" ] . ToString ( ) . Trim ( );

                //    //Creates the correct format for the CSV fle output, including adding single quotes to DATE fields
                //    // Tested and working 7/6/21
                //    tmp = $"{objRow [ "Id" ] . ToString ( )}, "
                //        + $"{objRow [ "BankNo" ] . ToString ( )}, "
                //        + $"{objRow [ "CustNo" ] . ToString ( )}, "
                //        + $"{acTypestr}, "
                //        + $"{objRow [ "Balance" ] . ToString ( )}, "
                //        + $"{objRow [ "Intrate" ] . ToString ( )}, "
                //        + $"'{odate}', '"
                //        + $"{cdate}'\r\n";
            }
            //return tmp;
        }

        private void DataGrid1_SelectionChanged ( object sender , SelectionChangedEventArgs e ) {
            if ( DataGrid1 . SelectedIndex != -1 && FillListBox == true )
                ParseGridRowData ( );
        }

        private void ShowRecordData_Click ( object sender , RoutedEventArgs e ) {
            List<string> list = new List<string> ( );
            if ( ShowRecordData . IsChecked == true ) {
                FillListBox = true;
                DbRecordInfo . ItemsSource = null;
                DbRecordInfo . Items . Clear ( );
                list . Add ( "Option is now Enabled " );
                list . Add ( "Changes of selection in DataGrid" );
                list . Add ( "will be show here..." );
                DbRecordInfo . ItemsSource = list;
            }
            else {
                FillListBox = false;
                DbRecordInfo . ItemsSource = null;
                DbRecordInfo . Items . Clear ( );
                list . Add ( "Option is currently disabled..." );
                DbRecordInfo . ItemsSource = list;
            }
        }


        private void Image_PreviewMouseLeftButtonDown ( object sender , MouseButtonEventArgs e ) {
            //List<object> list = new List<object> ( );
            //list . Add ( DataGrid1 );
            //list . Add ( listbox );
            //list . Add ( DbRecordInfo );
            //list . Add ( TablesPanel );
            //if ( listbox . Style == null )
            //Utils . Magnify (list, true );
            //else
            //    Utils . Magnify ( list , false);
            Utils . SwitchMagnifyStyle ( DataGrid1 , ref Magnifyrate );
            Utils . SwitchMagnifyStyle ( listbox , ref Magnifyrate , false );
           Utils . SwitchMagnifyStyle ( TablesPanel , ref Magnifyrate , false );
           Utils . SwitchMagnifyStyle ( DbRecordInfo , ref Magnifyrate , false );
           Utils . SwitchMagnifyStyle ( dbName , ref Magnifyrate , false );
           Utils . SwitchMagnifyStyle ( Storedprocs , ref Magnifyrate , false );
        }

        private void Showfd_Click ( object sender , RoutedEventArgs e ) {
            UseFlowdoc = ( bool ) ShowFlowdoc . IsChecked;
        }
    }
}