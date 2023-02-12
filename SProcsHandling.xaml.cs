#region usings
using System;
using System . Collections;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data;
using System . Diagnostics;
using System . IO;
using System . Linq;
using System . Printing;
using System . Text;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Shapes;
using System . Windows . Xps . Packaging;

using Dapper;

using Microsoft . Data . SqlClient;
using Microsoft . Identity . Client;
using Microsoft . VisualBasic;

using Newtonsoft . Json . Linq;

using SprocsProcessing;

using SqlMethods;

using ViewModels;

using Wpfmain . Dapper;
using Wpfmain . Models;
using Wpfmain . UtilWindows;

//#################################//
#endregion usings
//#################################//

namespace Wpfmain
{
    public partial class SProcsHandling : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Window that hosts datagrid and listboxes used to help process S.Procedures
        /// and manipulalte sqltables in a totally generic manner
        /// </summary>

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged ( string propertyName )
        {
            if ( this . PropertyChanged != null )
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this . PropertyChanged ( this, e );
            }
        }
        #endregion PropertyChanged

        //#################################//
        #region ALL DECLARATIONS
        //++++++++++++++++++++++++++++++++//

        #region MAJOR SETUP VARIABLES
        private bool SHOWGRIDFIRST = false;

        // PRIVATE Declarations
        public struct comboclrs
        {
            public string name;
            public SolidColorBrush Bground;
            public SolidColorBrush Fground;
        }
        public static comboclrs cclrs = new();
        public  List<comboclrs> CbColorsList = new ();

        private List<string> genericlist = new();
        private Stopwatch sw = new();
        private bool ShowSp { get; set; } = false;
        private bool ShowDg { get; set; } = false;
        private bool ShowSc { get; set; } = false;  // Script creation panel visibility
        private bool ShowRt { get; set; } = false;  // Results panel visibility
        public List<string> UpdatedRecordData { get; set; } = new ( );
        public List<string> OriginalRecordData { get; set; } = new ( );
        //= new string [150, 2, 2 ];
        public List<string> DuplicateRecordData { get; set; } = new ( );
        static public bool DragActive { get; set; } = false;
        public static Dictionary<string, SolidColorBrush> ColorsDictionary { get; set; } = new Dictionary<string, SolidColorBrush> ( );
        public static Dictionary<string, SolidColorBrush>ColorsDictionaryOut = new Dictionary<string, SolidColorBrush>();

        public GenericClass gclass = new();
        public GenericClass newgclass = new();
        public bool ScriptEditorOpen { get; set; } = false;


        #endregion MAJOR SETUP VARIABLES

        //#################################//
        #region general full Declarations
        //#################################//
        public bool bdirty { get; set; } = false;
        public int reccount { get; set; } = 0;
        public int currselection { get; set; } = 0;
        public static SProcsHandling sphandling { get; set; }
        public double Dragdistance { get; set; } = 0;

        #endregion general full Declarations

        private bool AllowSplitterReset { get; set; } = true;
        private bool ShowFullGrid { get; set; } = true;
        private int colcount { get; set; } = 0;

        #region  ReSizing  variables
        private double PreEditsplitterheight1 = 0;
        private double PreEditsplitterheight2 = 0;
        private string WinSize = "MED";
        private double DefSmallHeight = 40;
        private double DefMediumHeight = 125;
        private double DefLargeHeight = 275;
        public static int MAXARGSIZE = 6;
        public double TotalWinHeight { get; set; }
        public double TotalDataArea { get; set; }
        public double UpperDataArea { get; set; }
        public double LowerDataArea { get; set; }
        public double splitht { get; set; } = 25.00;
        public double UsablePanelsHeight { get; set; }
        public double UnusedPanelsHeight { get; set; }
        public double SPDatagridHeight { get; set; }
        public bool IsGridFullHeight { get; set; } = false;
        public bool IsEditFullHeight { get; set; } = false;
        public double SpUnusedSpace { get; set; } = 240;
        public double ResultsTextHeight { get; set; } = 1040;
        public int CurrentFontsize { get; set; } = 16;
        public SolidColorBrush ColorCb { get; set; }

        //Container for the splitter height data
        //        public List<RowDefinition> rowlist { get; set; } = new ( );

        //++++++++++++++++++++++++++++++++//
        #endregion resizing

        #region FULL PROPERTY declarations
        //#################################//

        // FULL PROPERTIES
        private double _rowHeights0;
        public double RowHeight0
        {
            get { return _rowHeights0; }
            set { _rowHeights0 = value; OnPropertyChanged ( nameof ( RowHeight0 ) ); }
        }
        private double _rowHeights1;
        public double RowHeight1
        {
            get { return _rowHeights1; }
            set { _rowHeights1 = value; OnPropertyChanged ( nameof ( RowHeight1 ) ); }
        }
        private double _rowHeights2;
        public double RowHeight2
        {
            get { return _rowHeights2; }
            set { _rowHeights2 = value; OnPropertyChanged ( nameof ( RowHeight2 ) ); }
        }
        //private double _rowHeights3;
        //public double RowHeight3
        //{
        //    get { return _rowHeights3; }
        //    set { _rowHeights3 = value; OnPropertyChanged ( nameof ( RowHeight3 ) ); }
        //}
        private double _rowHeights4;
        public double RowHeight4
        {
            get { return _rowHeights4; }
            set { _rowHeights4 = value; OnPropertyChanged ( nameof ( RowHeight4 ) ); }
        }
        private ObservableCollection<GenericClass> _sqlTable;
        public ObservableCollection<GenericClass> SqlTable
        {
            get { return _sqlTable; }
            set { _sqlTable = value; OnPropertyChanged ( nameof ( SqlTable ) ); }
        }
        private bool _WinResizing;
        public bool WinResizing
        {
            get { return _WinResizing; }
            set { _WinResizing = value; OnPropertyChanged ( nameof ( WinResizing ) ); }
        }

        private double _DefEditpanelHeight;
        public double DefEditpanelHeight
        {
            get { return _DefEditpanelHeight; }
            set { _DefEditpanelHeight = value; OnPropertyChanged ( nameof ( DefEditpanelHeight ) ); }
        }

        //#################################//
        #endregion FULL PROPERTY declarations

        #region general public static declarations
        //#################################//

        // PUBLIC Declarations
        static public ObservableCollection<GenericClass> Sprocs = new();
        // Table structure information for currently open SQL table
        public DataGridLayout dglayout = new DataGridLayout();
        public List<DataGridLayout> dglayoutlist = new();
        public List<int> VarCharLength = new();

        // static declarations
        static public GenericClass genclass = new();
        static public SProcsHandling sph { get; set; }
        static public IEnumerable<dynamic> IeSprocs { get; set; }
        static public string ConString = "";
        static public string SqlCommand { get; set; }
        static public string DbDomain { get; set; } = "IAN1";
        static public string CurrentDbName { get; set; } = "BankAccount";
        static public List<SolidColorBrush> Brushcolors = new();
        static public FlowDocument myFlowDocument = new FlowDocument();
        static public string[] arguments = new string[DEFAULTARGSSIZE];
        static public Dictionary<string, string> ConnectionStringsDict = new Dictionary<string, string>();

        //++++++++++++++++++++++++++++++++//
        #endregion general public static declarations

        #region general public declarations
        //#################################//

        public List<string> ExecCommands = new();
        public List<SolidColorBrush> ColorComboForeground= new();
        public const int DEFAULTARGSSIZE = 6;
        private bool ISGRIDVISIBLE;

        #endregion general public declarations

        #region S.Procs Properties declarations
        public static SProcsHandling spviewer { get; set; }
        public SProcsSupport processsprocs { get; set; } = new SProcsSupport ( );
        public int CurrentSelectionIndex { get; set; } = -1;
        public string CurrentSelectionId { get; set; } = "-1";
        public GenericClass CurrentGenclass { get; set; } = new ( );
        public bool LeftMousePressed { get; set; } = false;
        public Brush ScrollViewerBground { get; set; }
        public Brush ScrollViewerFground { get; set; }
        public Brush ScrollViewerHiliteColor { get; set; }
        public string Searchtext { get; set; } = "ARG";
        public string Searchterm { get; set; }
        public bool CloseArgsViewerOnPaste { get; set; } = false;
        public bool ShowTypesInArgsViewer { get; set; } = true;
        public bool ShowParseDetails { get; set; } = false;
        public bool KeepTypes { get; set; } = false;
        public bool IsLoading { get; set; } = true;
        public bool TableReloading { get; set; } = false;
        public bool LEFTMOUSEDOWN { get; set; } = false;

        #region full propertes

        private double scrollViewerFontSize;

        //store for the full SP text
        private string spTextBuffer;
        // Text in create new Sproc editor
        private string _NewSprocText;
        // store  for SP arguments alone
        private string spArgstext;
        public double ScrollViewerFontSize
        {
            get { return scrollViewerFontSize; }
            set { scrollViewerFontSize = value; OnPropertyChanged ( nameof ( ScrollViewerFontSize ) ); }
        }

        public string SpTextBuffer
        {
            get { return spTextBuffer; }
            set { spTextBuffer = value; OnPropertyChanged ( nameof ( SpTextBuffer ) ); }
        }

        public string SpArgsText
        {
            get { return spArgstext; }
            set { spArgstext = value; OnPropertyChanged ( nameof ( spArgstext ) ); }
        }

        public string NewSprocText
        {
            get { return _NewSprocText; }
            set { _NewSprocText = value; OnPropertyChanged ( nameof ( NewSprocText ) ); }
        }

        #endregion full propertes

        #endregion S.Procs Properties declarations

        #region Splitter properties

        private bool WinLoaded { get; set; } = false;
        private double Splitterlastpos { get; set; }
        new public bool MouseRightButtonDown { get; set; } = false;
        public Dictionary<string, string> cmContents { get; set; } = new ( );
        public MenuItem CurrentMenuitem { get; set; }

        #endregion Splitter properties

        #region  DP's

        #endregion  DP's

        #endregion ALL DECLARATIONS

        #region Initialization
        static public  bool startup = true;
        // CONSTRUCTOR
        public SProcsHandling ( bool bl )
        {
        }
        public SProcsHandling ( )
        {
            int colcount = 0;
            "" . Track ( 0 );
            Mouse . OverrideCursor = Cursors . Wait;
            InitializeComponent ( );
            this . DataContext = this;   // ?????
            sph = this;
            spviewer = this;
            ScrollViewerFontSize = 15.00;
            SqlTable = new ObservableCollection<GenericClass> ( );
            ISGRIDVISIBLE = SHOWGRIDFIRST;
            if ( ISGRIDVISIBLE )
            {
                ShowDg = true;
                ShowSp = false;
            }
            else
            {
                ShowDg = false;
                ShowSp = true;
            }
            ConString = SqlBasicSupport . SqlSupport . LoadConnectionStrings ( );
            ScrollViewerBground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
            ScrollViewerFground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
            ScrollViewerHiliteColor = Application . Current . FindResource ( "Red4" ) as SolidColorBrush;

            SPDatagrid . ItemsSource = null;
            SPDatagrid . Items . Clear ( );
            // Get IEnumerable<dynamic> collection of the data requested using S.Proc
            IeSprocs = LoadAllSprocs ( ConString, CurrentDbName, "spGetStoredProcs" );

            if ( ExecCommands . Count == 0 )
            {
                ExecCommands . Add ( "1. SP returning a Table as ObservableCollection" );
                ExecCommands . Add ( "2. SP returning a List<string>" );
                ExecCommands . Add ( "3. SP returning a String" );
                ExecCommands . Add ( "4. SP returning an INT value" );
                ExecCommands . Add ( "5. Execute Stored Procedure with return value" );
                ExecCommands . Add ( "6 . Execute Stored Procedure without return value" );
                ExecList . ItemsSource = ExecCommands;
            }

            // Now parse the data received into GenericClass records
            Sprocs = new ObservableCollection<GenericClass> ( );
            Dictionary<string, object> dict = new();
            List<string> list = new();
            List<string> Fldnames = new();
            if ( IeSprocs != null )
            {
                foreach ( var item in IeSprocs )
                {
                    // get data from DapperRow as Dictionary{string>, object>
                    dict = new ( );
                    dict = DapperGeneric . ParseDapperRowGen ( item, dict, out colcount );
                    // return a list of data as Fieldname=Data:"
                    list = GetGenericListFromDictionary ( dict );
                    // Convert List<string> into GenericClass Record with only one column
                    genclass = ConvertListToSingleColumn ( list, colcount, out Fldnames );
                    // Add GenericClass to relevant version of Observable colection
                    Sprocs . Add ( genclass );
                    SProcsListbox . ItemsSource = null;
                    SProcsListbox . Items . Clear ( );
                    ArrayList SprocsData = new ArrayList();
                    SprocsData = LoadListBoxData ( Sprocs );
                    SProcsListbox . ItemsSource = SprocsData;
                }
            }
            LoadColorsComboFromFile ( );
            //            PerformDataLoading ( );
            Task maintask = new  Task (async  ( ) =>
            {
                Debug . WriteLine ( "\nthread running load commands...." );
                this . Dispatcher . Invoke ( async ( ) =>
                {
//                    Debug . WriteLine ( "\nRunning LoadColorsCombo ()...." );
                    Debug . WriteLine ( "\nRunning LoadFontSizes ()...." );
                    LoadFontSizes ( );
                    Debug . WriteLine ( "\nnRunning LoadSqlTableNames ( );...." );
                    LoadSqlTableNames ( );
                   // Debug . WriteLine ( "\nnRunning LoadColorsCombo ( );...." );
//                    await LoadColorsCombo();
                } );
                Debug . WriteLine ( "All threads ended...." );
            });     // close off child tas
            //// RUN THE WHOLE THING
            maintask . Start ( );

            CreateNewSprocEditorTemplate ( );
            ResetOptionsAccessColors ( );
            Mouse . OverrideCursor = Cursors . Arrow;
            SetupEvents ( );
            "" . Track ( 1 );
            IsLoading = true;
            RowHeight0 = 300.0;
            RowHeight1 = 20.0;
            RowHeight2 = 250.0;

            // this lets me monitor the height changes of  the RowDefinitions in real time - Yeahhhh !
            var heightDescriptor = DependencyPropertyDescriptor.FromProperty(RowDefinition.HeightProperty, typeof(ItemsControl));
            heightDescriptor . AddValueChanged ( SPFullDataContainerGrid . RowDefinitions [ 0 ], HeightChanged );

            startup = false;
        }
        private void HeightChanged ( object sender, EventArgs e )
        {
            // KEEP THESE - Show splitter info (RowDefinitions) in real time 
            //Debug . WriteLine ( $"[0] : {SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height}" );
            //Debug . WriteLine ( $"[2] : {SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height}" );
        }
        private void HSplitter_PreviewQueryContinueDrag ( object sender, System . Windows . Controls . Primitives . DragDeltaEventArgs e )
        {
            Debug . WriteLine ( $"QueryDrag : {SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height}" );
        }


        private void Window_Loaded ( object sender, RoutedEventArgs e )
        {
            double controlheight1 = 0, controlheight2 = 0;
            string Error = "";
            if ( IsLoading == false )
            {
                "IsLoading==false  " . Track ( 1 );
                return;
            }
            "" . Track ( 0 );
            DefEditpanelHeight = 350;
            RowHeight1 = 20.0;

            // FOUND IT - The unusedspace is the correct height to cover all the lower panes below the working area !!!!!
            //           SpUnusedSpace = ArgumentsContainerGrid .Height+ Bottompanel.Height;
            RowHeight4 = SProcsViewer . Height - SpUnusedSpace;
            RowHeight2 = DefEditpanelHeight;
            RowHeight0 = RowHeight4 - ( DefEditpanelHeight );

            //Splitter intial position - Gives  correct proportions
            /*
             *  The missing height has been found, it it made up of ALL the lower panels frm 
             *  the Blanker/Propmpt row down to the bottom (240 pixels is pretty close ot correct anyway)
             */
            SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight4 - ( DefEditpanelHeight + 25 ) );
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( EditPanel . Height );
            PreEditsplitterheight1 = SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight;

            Splitterlastpos = RowHeight0;
            RowHeight1 = 20;
            RowHeight2 = DefEditpanelHeight;


            LeftMousePressed = true;
            IsLoading = false;
            SProcsListbox . SelectedIndex = 1;
            SProcsListbox . SelectedIndex = 0;
            IsLoading = true;
            ExecList . SelectedIndex = 0;
            IsLoading = false;
            Mouse . OverrideCursor = Cursors . Arrow;
            SPDatagrid . SelectedIndex = 0;
            // hide all result panels
            ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
            ResultsContainerListbox . Visibility = Visibility . Collapsed;
            ResultsContainerTextblock . Visibility = Visibility . Collapsed;
            // The  correct reduction - finally.
            //RowHeight4 = SProcsViewer . Height - SpUnusedSpace;
            RowHeight4 = SPFullDataContainerGrid . Height - SpUnusedSpace;
            // set dgrid as  visible
            if ( SHOWGRIDFIRST )
            {
                //Startup with SQL Db tables controls  visible
                TextResult . Visibility = Visibility . Collapsed;
                // Set flags up
                ShowDg = true;
                ShowSp = false;

                SProcsListbox . Visibility = Visibility . Collapsed;
                SPInfopanelGrid . Visibility = Visibility . Collapsed;
                ExecList . Visibility = Visibility . Collapsed;

                SPDatagrid . Visibility = Visibility . Visible;
                EditPanel . Visibility = Visibility . Visible;

                shrink1 . Text = " Hide Edit";
                shrink2 . Text = "   Panel ";
                ShowEditpanel . Visibility = Visibility . Visible;

                RefreshDatagrid ( "BANKACCOUNT" );
                SPFullDataContainerGrid . UpdateLayout ( );
                SPDatagrid . UpdateLayout ( );

                CurrentMenuitem = ShowDgmenu;
                CurrentMenuitem = ShowSpmenu;
                show_DataGrid ( sender, null );
                ResetPanelControlSizes ( );
            }
            else
            {
                //Startup with S Procs controls  visible
                // Set flags up
                ShowDg = false;
                ShowSp = true;
                // hide the edit show button in info panels
                ShowEditpanel . Visibility = Visibility . Collapsed;
                ResultsContainerListbox . Visibility = Visibility . Collapsed;
                EditPanel . Visibility = Visibility . Collapsed;
                Blanker . Visibility = Visibility . Collapsed;
                SPInfopanelGrid . Visibility = Visibility . Visible;
                TextResult . Visibility = Visibility . Visible;
                show_Sprocs ( sender, null );
                ResetPanelControlSizes ( );
                SProcsListbox . Focus ( );
                SProcsListbox . SelectedIndex = 0;
            }
            Splitter_DragCompleted ( sender, null );
            WinLoaded = true;
            SetWindowTitleBar ( );

            "" . Track ( 1 );
            this . Focus ( );
        }
        private void SetGlobals ( )
        {
            GridLength gl1;
            "" . Track ( 0 );

            // Set Global variables
            TotalWinHeight = spviewer . ActualHeight;
            UnusedPanelsHeight = 292;
            splitht = 25;
            if ( SPFullDataContainerGrid . RowDefinitions [ 2 ] . ActualHeight == 1 )
                gl1 = SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height;
            LowerDataArea = SPFullDataContainerGrid . RowDefinitions [ 2 ] . ActualHeight;
            Debug . WriteLine ( $"SetGlobals : LowerDataArea = {LowerDataArea}" );
            UpperDataArea = SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight;
            TotalDataArea = SPFullDataContainerGrid . ActualHeight;
            "" . Track ( 1 );
        }

        public double GetDoubleHeight ( object gname, out string error )
        {
            double dvalue = -1;
            error = "";
            Type type = gname.GetType();
            Debug . WriteLine ( $"type = {type}" );
            if ( gname . GetType ( ) == typeof ( double ) )
                dvalue = Convert . ToDouble ( gname );
            else if ( gname . GetType ( ) == typeof ( Grid ) )
            {
                Grid grid = new Grid();
                grid = gname as Grid;
                dvalue = grid . ActualHeight;
            }
            else
            {
                if ( gname . GetType ( ) == typeof ( DataGrid ) )
                {
                    // eg SPDatagrid
                    DataGrid dgrid = new DataGrid();
                    dgrid = gname as DataGrid;
                    dvalue = dgrid . Height;
                }
                else if ( gname . GetType ( ) == typeof ( FlowDocument ) )
                {
                    // eg TextResult
                    FlowDocumentScrollViewer fd = new();
                    fd = gname as FlowDocumentScrollViewer;
                    dvalue = fd . Height;
                }
                else if ( gname . GetType ( ) == typeof ( GridLength ) )
                {
                    // eg TextResult
                    GridLength glen = new();
                    glen = ( GridLength ) gname;
                    GridLength gflen = new GridLength(glen.Value, GridUnitType.Pixel);
                    dvalue = glen . Value;
                }
                else if ( dvalue == -1 )
                {
                    error = $"Failed to identify Control type received of {gname . GetType ( )}";
                    Debug . WriteLine ( $"{error}" );
                    Utils . DoErrorBeep ( );
                    return -1;

                }
            }
            return dvalue;
        }

        #endregion Initialization

        private void SetupEvents ( )
        {
        }
        public static SProcsHandling GetSProcsHandling ( )
        {
            return sph;
        }

        #region utility methods
        //#################################//
        public ObservableCollection<GenericClass> LoadSqlData ( string tablename, bool LoadGrid = true )
        {
            List<Dictionary<string, string>> dict = new List<Dictionary<string, string>>();

            "" . Track ( 0 );
            CurrentDbName = tablename;

            SqlTable = LoadDbAsGenericData ( $"Select * from {tablename}",
                SqlTable,
                ref dict,
                "",
                DbDomain,
                ref dglayoutlist,
                true );

            return SqlTable;
        }

        public void LoadSqlDataIntoDatagrid ( ObservableCollection<GenericClass> SqlTable, DataGrid SPDatagrid, string tablename )
        {
            if ( SqlTable . Count > 0 )
                LoadTableIntoGrid ( SqlTable, SPDatagrid );
        }

        private ArrayList LoadListBoxData ( ObservableCollection<GenericClass> SprocsView )
        {
            ArrayList itemsList = new ArrayList();
            foreach ( var item in SprocsView )
            {
                itemsList . Add ( item . field1 . ToString ( ) );
            }
            return itemsList;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion utility methods

        #region SQL Fetch SProcs Data
        //#################################//

        static IEnumerable<dynamic> LoadAllSprocs ( string constring, string CurrentDbName, string SqlCommand )
        {
            if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) == false )
            {
                // probably a stored procedure ?  							
                var IsSuccess = new DynamicParameters();
                IEnumerable<dynamic> reslt;
                Debug . WriteLine ( $"Running SP db.Query" );
                using ( IDbConnection db = new SqlConnection ( ConString ) )
                {
                    // Create our aguments using the Dynamic parameters provided by Dapper
                    var Params = new DynamicParameters();

                    //***************************************************************************************************************//
                    //WORKING  JUST FINE FOR OBSERVABLECOLLECTION<GENERICCLASS>
                    reslt = db . Query ( SqlCommand, Params, commandType: CommandType . StoredProcedure );
                }
                if ( reslt != null )
                {
                    return reslt;
                }
                else
                    return null;
            }
            else if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) == true )
            {
                if ( SqlCommand == "" )
                    SqlCommand = $"select * from {CurrentDbName}";

                string Arguments = "";
                string OrderByClause = "";
                string WhereClause = "";
                List<string> strings = new List<string>();
                IEnumerable<dynamic> IeSprocs = DapperGeneric.ExecuteSPGenericClass<GenericClass>(
                   sph.SqlTable,
                   constring,
                   SqlCommand,
                   Arguments,
                   WhereClause,
                   OrderByClause,
                   out List<string> genericlist,
                   out string errormsg);

                if ( IeSprocs != null )
                {
                    return IeSprocs;
                }
                else
                    return null;
            }
            return null;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion SQL Fetch SProcs Data

        #region Support methods
        //#################################//

        public bool LoadShowMatchingSproc ( Window win, FlowDocumentScrollViewer flowdocsv, string spfilename, ref string sptext )
        {
            // Read full script of an SP into memory and display it inFlowdocscrollviewer received
            // This reads the SP into memory in sptext  and displays it in the SpResultsViewer Scrollviewer
            this . FetchStoredProcedureCode ( spfilename, ref sptext );
            if ( sptext == "" )
            {
                return false;
            }
            else
                spTextBuffer = sptext;     // store full sp text in window Property

            // This ensures that both widnows are updated independently
            // depending on which list triggers the reload of the SP.
            if ( win . Name == "SProcsViewer" )
            {
                /// It is Genericgrid that has triggered ths data load, so put the SP details into the ScrollDooc
                TextResult . Document = null;
                myFlowDocument = new FlowDocument ( );
                myFlowDocument . Blocks . Clear ( );
                myFlowDocument = processsprocs . CreateBoldString ( myFlowDocument, sptext, Searchtext );
                myFlowDocument . Background = ScrollViewerBground;
                TextResult . Document = myFlowDocument;
            }
            return true;
        }
        public string FetchStoredProcedureCode ( string spName, ref string stringresult, bool HeaderOnly = false )
        {
            // Load a specified SP file annd show in Scrollviewer
            stringresult = "";
            DataTable dt = new();
            string output = "";
            if ( spName == null )
            {
                stringresult = output;
                return stringresult;
            }
            dt = SProcsSupport . ProcessSqlCommand ( $"spGetNamedSproc  '{spName}'", Flags . CurrentConnectionString );
            List<string> list = new List<string>();
            List<string> headeronlylist = new List<string>();
            foreach ( DataRow row in dt . Rows )
            {
                list . Add ( row . Field<string> ( 0 ) );
            }
            if ( HeaderOnly )
            {
                list [ 0 ] = SProcsSupport . GetSpHeaderTextOnly ( list [ 0 ] );
                // now display the full content of the seleted S.P
                if ( list . Count > 0 )
                    output = list [ 0 ];
                stringresult = output;
                return stringresult;
                //return output;
            }
            // now display the full content of the seleted S.P
            if ( list . Count > 0 )
                output = list [ 0 ];
            stringresult = output;
            return stringresult;
        }

        public static ObservableCollection<GenericClass> GetDbTableColumns (
            ref ObservableCollection<GenericClass> Gencollection,
            ref List<Dictionary<string, string>> ColumntypesList,
             ref List<string> list,
             string dbName,
             string DbDomain,
             ref List<DataGridLayout> dglayoutlist )
        {
            // Make sure we are accessing the correct Db Domain
            DapperSupport . CheckDbDomain ( DbDomain );
            Gencollection = GetSpArgs ( ref Gencollection, ref ColumntypesList, ref list, dbName, DbDomain, ref dglayoutlist );
            return Gencollection;
        }

        public static ObservableCollection<GenericClass> GetSpArgs (
            ref ObservableCollection<GenericClass> Gencollection,
            ref List<Dictionary<string, string>> ColumntypesList,
            ref List<string> list,
            string dbName,
            string DbDomain,
            ref List<DataGridLayout> dglayoutlist )
        {
            DataTable dt = new DataTable();
            GenericClass genclass = new GenericClass();
            try
            {
                // only used by grid2 on initial load cos grid1 uses List for datasource & gets count diffrently.
                //called on initial load to get column name and type (not datagrid data)
                if ( dglayoutlist . Count == 0 )
                    Gencollection = LoadDbAsGenericData ( "spGetTableColumnWithSizes",
                        Gencollection,
                        ref ColumntypesList,
                        dbName,
                        DbDomain,
                        ref dglayoutlist,
                        true );
                // Gencollection now contains FULL schema info on selected table  whereas 
                //dglayoutlist & Columntype contain only columnn name and type
                // list is not effected at all
            }
            catch ( Exception ex )
            {
                MessageBox . Show ( $"SQL ERROR 1125 - {ex . Message}" );
                return Gencollection;
            }
            return Gencollection;
        }

        public static GenericClass ParseDapperRowGen ( dynamic buff,
        Dictionary<string, object> dict, out int colcount )
        {
            GenericClass GenRow = new GenericClass();
            int index = 2;
            colcount = 0;
            foreach ( var item in buff )
            {
                try
                {
                    if ( item . Key == "" || item . Value == null )
                        break;
                    dict . Add ( item . Key, item . Value );
                }
                catch ( Exception ex )
                {
                    MessageBox . Show ( $"ParseDapper error was : \n{ex . Message}\nKey={item . Key} Value={item . Value . ToString ( )}" );
                    break;
                }
            }
            colcount = index;
            return GenRow;
        }

        private static bool IsDuplicatecolumnname ( string Columntypes, Dictionary<string, string> ColumntypesList )
        {
            bool success = false;
            foreach ( KeyValuePair<string, string> item in ColumntypesList )
            {
                if ( item . Key == Columntypes )
                {
                    success = true;
                    break;
                }
            }
            return success;
        }
        private static bool IsDuplicateFieldname ( DataGridLayout dglayout, List<DataGridLayout> dglayoutlist )
        {
            //$"Entering " . dcwinfo(0);
            bool success = false;
            //;			int count = 0;
            foreach ( var item in dglayoutlist )
            {
                if ( item . Fieldname == dglayout . Fieldname )
                    return true;
            }
            return success;
        }
        /// <summary>
        /// called to load all types of data using a Stored procedure (only)        /// </summary>
        /// <param name="SqlCommand"></param>
        /// <param name="collection"></param>
        /// <param name="ColumntypesList"></param>
        /// <param name="Arguments"></param>
        /// <param name="DbDomain"></param>
        /// <param name="dglayoutlist"></param>
        /// <param name="GetLengths"></param>
        /// <returns></returns>
        public static ObservableCollection<GenericClass> LoadDbAsGenericData (
            string SqlCommand,
            ObservableCollection<GenericClass> collection,
            ref List<Dictionary<string, string>> ColumntypesList,
            string Arguments,
            string DbDomain,
            ref List<DataGridLayout> dglayoutlist,
            bool GetLengths = false )
        {
            string result = "";
            string arg1 = "", arg2 = "", arg3 = "", arg4 = "";
            // provide a default connection string
            string ConString = "ConnectionString";
            Dictionary<string, dynamic> dict = new Dictionary<string, object>();
            ObservableCollection<GenericClass> GenClass = new ObservableCollection<GenericClass>();

            // Ensure we have the correct connection string for the current Db Doman
            Utils2 . CheckResetDbConnection ( DbDomain, out string constr );
            Flags . CurrentConnectionString = constr;
            ConString = constr;
            collection . Clear ( );
            using ( IDbConnection db = new SqlConnection ( ConString ) )
            {
                try
                {
                    // Use DAPPER to run  Stored Procedure
                    // One or No arguments
                    arg1 = Arguments;
                    if ( arg1 . Contains ( "," ) )              // trim comma off
                        arg1 = arg1 . Substring ( 0, arg1 . Length - 1 );
                    // Create our aguments using the Dynamic parameters provided by Dapper
                    var Params = new DynamicParameters();
                    if ( arg1 != "" )
                        Params . Add ( "Arg1", arg1, DbType . String, ParameterDirection . Input, arg1 . Length );
                    if ( arg2 != "" )
                        Params . Add ( "Arg2", arg2, DbType . String, ParameterDirection . Input, arg2 . Length );
                    if ( arg3 != "" )
                        Params . Add ( "Arg3", arg3, DbType . String, ParameterDirection . Input, arg3 . Length );
                    if ( arg4 != "" )
                        Params . Add ( "Arg4", arg4, DbType . String, ParameterDirection . Input, arg4 . Length );

                    //***************************************************************************************************************//
                    // This returns the data from SP commands (only) in a GenericClass Structured format
                    // FAILS on parsedapper
                    var reslt = db.Query(SqlCommand, Params, commandType: CommandType.Text);
                    //***************************************************************************************************************//

                    if ( reslt != null )
                    {
                        //Although this is duplicated  with the one below we CANNOT make it a method()
                        string errormsg = "DYNAMIC";
                        int dictcount = 0;
                        int fldcount = 0;
                        int colcount = 0;
                        GenericClass gc = new GenericClass();
                        List<int> VarcharList = new List<int>();
                        Dictionary<string, string> outdict = new Dictionary<string, string>();
                        try
                        {
                            foreach ( var item in reslt )
                            {
                                try
                                {
                                    // we need to create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
                                    string buffer = "";
                                    // WORKS OK
                                    ParseDapperRowGen ( item, dict, out colcount );
                                    gc = new GenericClass ( );
                                    dictcount = 1;
                                    int index = 1;
                                    fldcount = dict . Count;
                                    string tmp = "";

                                    // Parse reslt.item into  single dglayout Dictionary record
                                    foreach ( var pair in dict )
                                    {
                                        Dictionary<string, string> Columntypes = new Dictionary<string, string>();
                                        try
                                        {
                                            if ( pair . Key != null )
                                            {
                                                if ( pair . Value != null )
                                                {
                                                    DapperSupport . AddDictPairToGeneric ( gc, pair, dictcount++ );
                                                    tmp = $"field{index++} = {pair . Value . ToString ( )}";
                                                    outdict . Add ( pair . Key, pair . Value . ToString ( ) );
                                                    buffer += tmp + ",";
                                                }
                                                //List<int>
                                                if ( pair . Key == "character_maximum_length" )
                                                    sph . dglayout . Fieldlength = item . character_maximum_length == null ? 0 : item . character_maximum_length;
                                                if ( pair . Key == "data_type" )
                                                    sph . dglayout . Fieldtype = item . data_type == null ? 0 : item . data_type;
                                                if ( pair . Key == "column_name" )
                                                {
                                                    string temp = item.column_name.ToString();
                                                    if ( IsDuplicatecolumnname ( temp, Columntypes ) == false )
                                                        Columntypes . Add ( temp, item . data_type . ToString ( ) );
                                                    sph . dglayout . Fieldname = item . column_name == null ? "UNSPECIFIED" : item . column_name;
                                                    // Add Dictionary <string,string> to List<Dictionary<string,string>
                                                    ColumntypesList . Add ( Columntypes );
                                                }
                                            }
                                        }
                                        catch ( Exception ex )
                                        {
                                            Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
                                            result = ex . Message;
                                        }
                                    }
                                    //remove trailing comma
                                    string s = buffer.Substring(0, buffer.Length - 1);
                                    buffer = s;
                                    // We now  have ONE single record, but need to add this  to a GenericClass structure 
                                    int reccount = 1;
                                    foreach ( KeyValuePair<string, string> val in outdict )
                                    {  //
                                        switch ( reccount )
                                        {
                                            case 1:
                                                gc . field1 = val . Value . ToString ( );
                                                break;
                                            case 2:
                                                gc . field2 = val . Value . ToString ( );
                                                break;
                                            case 3:
                                                gc . field3 = val . Value . ToString ( );
                                                break;
                                            case 4:
                                                gc . field4 = val . Value . ToString ( );
                                                break;
                                            case 5:
                                                gc . field5 = val . Value . ToString ( );
                                                break;
                                            case 6:
                                                gc . field6 = val . Value . ToString ( );
                                                break;
                                            case 7:
                                                gc . field7 = val . Value . ToString ( );
                                                break;
                                            case 8:
                                                gc . field8 = val . Value . ToString ( );
                                                break;
                                            case 9:
                                                gc . field9 = val . Value . ToString ( );
                                                break;
                                            case 10:
                                                gc . field10 = val . Value . ToString ( );
                                                break;
                                            case 11:
                                                gc . field11 = val . Value . ToString ( );
                                                break;
                                            case 12:
                                                gc . field12 = val . Value . ToString ( );
                                                break;
                                            case 13:
                                                gc . field13 = val . Value . ToString ( );
                                                break;
                                            case 14:
                                                gc . field14 = val . Value . ToString ( );
                                                break;
                                            case 15:
                                                gc . field15 = val . Value . ToString ( );
                                                break;
                                            case 16:
                                                gc . field16 = val . Value . ToString ( );
                                                break;
                                            case 17:
                                                gc . field17 = val . Value . ToString ( );
                                                break;
                                            case 18:
                                                gc . field18 = val . Value . ToString ( );
                                                break;
                                            case 19:
                                                gc . field19 = val . Value . ToString ( );
                                                break;
                                            case 20:
                                                gc . field20 = val . Value . ToString ( );
                                                break;
                                        }
                                        reccount += 1;
                                    }
                                    collection . Add ( gc );
                                }
                                catch ( Exception ex )
                                {
                                    result = $"SQLERROR : {ex . Message}";
                                    errormsg = result;
                                    Debug . WriteLine ( result );
                                }
                                dict . Clear ( );
                                outdict . Clear ( );
                                dictcount = 1;
                            }
                        }
                        catch ( Exception ex )
                        {
                            Debug . WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
                            result = ex . Message;
                            errormsg = result;
                        }
                        if ( errormsg == "" )
                            errormsg = $"DYNAMIC:{fldcount}";
                        return collection;
                    }
                }
                catch ( Exception ex )
                {
                    $"{ex . Message}" . cwerror ( );
                }
            }
            return GenClass;
        }

        static public List<string> GetGenericListFromDictionary ( Dictionary<string, object> dict )
        {
            // called immediately after parsing DapperRow data
            List<string> list = new();
            int count = 0;
            foreach ( var pair in dict )
            {
                try
                {
                    if ( pair . Key != null && pair . Value != null )
                    {
                        genclass = new GenericClass ( );
                        DapperSupport . AddDictPairToGeneric ( genclass, pair, count++ );
                        string tmp = pair.Key.ToString() + "=" + pair.Value.ToString();
                        list . Add ( tmp );
                    }
                }
                catch ( Exception ex )
                {
                    Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
                }
            }
            return list;
        }

        static public GenericClass ConvertListToGenericClassRecord ( List<string> Recordslist, int colcount, out List<string> Fldnames )
        {
            // called after a list<string> contaning a single row of GenericClass data
            // to create a GenericClass Record
            int count = 0;
            Fldnames = new List<string> ( );
            while ( count < colcount )
            {
                foreach ( var item in Recordslist )
                {
                    string[] buff = new string[3];
                    buff = item . Split ( "=" );
                    switch ( count )
                    {
                        case 0:
                            genclass . field1 = buff [ 1 ];
                            break;
                        case 1:
                            genclass . field2 = buff [ 1 ];
                            break;
                        case 2:
                            genclass . field3 = buff [ 1 ];
                            break;
                        case 3:
                            genclass . field4 = buff [ 1 ];
                            break;
                        case 4:
                            genclass . field5 = buff [ 1 ];
                            break;
                        case 5:
                            genclass . field6 = buff [ 1 ];
                            break;
                        case 6:
                            genclass . field7 = buff [ 1 ];
                            break;
                        case 7:
                            genclass . field8 = buff [ 1 ];
                            break;
                        case 8:
                            genclass . field9 = buff [ 1 ];
                            break;
                        case 9:
                            genclass . field10 = buff [ 1 ];
                            break;
                        case 10:
                            genclass . field11 = buff [ 1 ];
                            break;
                        case 11:
                            genclass . field12 = buff [ 1 ];
                            break;
                        case 12:
                            genclass . field13 = buff [ 1 ];
                            break;
                        case 13:
                            genclass . field14 = buff [ 1 ];
                            break;
                        case 14:
                            genclass . field15 = buff [ 1 ];
                            break;
                        case 15:
                            genclass . field16 = buff [ 1 ];
                            break;
                        case 16:
                            genclass . field17 = buff [ 1 ];
                            break;
                        case 17:
                            genclass . field18 = buff [ 1 ];
                            break;
                        case 18:
                            genclass . field19 = buff [ 1 ];
                            break;
                        case 19:
                            genclass . field20 = buff [ 1 ];
                            break;
                        default:
                            break;
                    }
                    Fldnames . Add ( buff [ 0 ] );
                    count++;
                }
            }
            return genclass;
        }

        public static GenericClass ConvertListToSingleColumn ( List<string> Recordslist, int colcount, out List<string> Fldnames )
        {
            // called after a list<string> contaning a single row of GenericClass data
            // to create a GenericClass Record
            int count = 0;
            // Fldnames is NOT used in this version of sinlge column (S.Procs list) mode
            Fldnames = new List<string> ( );
            while ( count < colcount )
            {
                foreach ( var item in Recordslist )
                {
                    string[] buff = new string[3];
                    buff = item . Split ( "=" );
                    genclass . field1 = buff [ 1 ];
                    count++;
                }
            }
            return genclass;

        }

        static public DynamicParameters ParseNewSqlArgs ( DynamicParameters parameters, List<string [ ]> argsbuffer, out string error )
        {
            DynamicParameters pms = new DynamicParameters();
            error = "";
            try
            {
                /*
                 order is :
                @name
                Argument
                Type
                !!!! Size
                Direction
                 */
                int argcount = 0;
                for ( var i = 0 ; i < argsbuffer . Count ; i++ )
                {
                    string[] args = new string[6];
                    args = PadArgsArray ( args );
                    args = argsbuffer [ i ];
                    PadArgsArrayNulls ( ref args );
                    int[] argindx = new int[5];
                    // set  all to zero to initialise flags
                    string printflags="";
                    for ( int z = 0 ; z < 5 ; z++ )
                    {
                        if ( args [ z ] == null )
                            continue;
                        if ( args [ z ] != "" )
                        {
                            printflags += " 1,";
                            argindx [ z ] = 1;
                        }
                        else
                        {
                            argindx [ z ] = 0;
                            printflags += " 0,";
                        }
                    }
                    printflags = printflags . Substring ( 0, printflags . Length - 1 );

                    Debug . WriteLine ( printflags );
                    // MISSING @Arg name - ERROR
                    if ( argindx [ 0 ] == 0 && argindx [ 1 ] == 1 && argindx [ 2 ] == 0 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        error = $"The mandatory Argument name is missing, processing cannot proceed";
                        break;
                    }
                    // Got (1 = 10000) arg name only 
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 0 && argindx [ 2 ] == 0 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                            name: args [ 0 ] );
                        continue;
                    }
                    // got (2 = 11000) @arg name and argument name only - valid anywhere
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 0 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , value: args [ 1 ] );
                        argcount++;
                        continue;
                    }
                    // Got (2 = 10010) arg name + direction only  2nd or subsequent args only !!
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 0 && argindx [ 2 ] == 0 && argindx [ 3 ] == 1 && argindx [ 4 ] == 0 )
                    {
                        // Got @argument but no arg name ! possible an OUTPUT variable name used without an argument name,
                        // //but with al other of the 5 params (argument  type, Ouput/return etc, allowed size)
                        if ( args [ 4 ] != "" )
                            pms . Add (
                                name: args [ 0 ]
                                , dbType: GetArgType ( args [ 2 ] ) );
                        continue;
                    }
                    // Got (2 = 10100) arg name + arg type + type
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 0 && argindx [ 2 ] == 1 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , direction: GetDirection ( args [ 3 ] ) );
                        argcount++;
                        continue;
                    }
                    // got (3 = 11001) @arg name and argument name + size only
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 0 && argindx [ 3 ] == 0 && argindx [ 4 ] == 1 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , value: args [ 1 ]
                            , size: GetArgSize ( args [ 4 ] ) );
                        argcount++;
                        continue;
                    }
                    // Got (3 = 11010) arg name Argumment name + direction
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 0 && argindx [ 3 ] == 1 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                                name: args [ 0 ]
                                , value: args [ 1 ]
                                , dbType: GetArgType ( args [ 2 ] ) );
                        continue;
                    }
                    // Got (3 = 11100) arg name
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 1 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , value: args [ 1 ]
                            , direction: GetDirection ( args [ 3 ] ) );
                        argcount++;
                        continue;
                    }
                    // Got (4 = 11110) arg name+ arg type + arg size only (default direction)
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 1 && argindx [ 3 ] == 1 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                           name: args [ 0 ]
                           , value: args [ 1 ]
                           , dbType: GetArgType ( args [ 2 ] )
                           , direction: GetDirection ( args [ 3 ] ) );
                        argcount++;
                        continue;
                    }
                    // Got (5 = 11111) Full House
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 1 && argindx [ 3 ] == 1 && argindx [ 4 ] == 1 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , value: args [ 1 ]
                            , dbType: GetArgType ( args [ 2 ] )
                            , direction: GetDirection ( args [ 3 ] )
                            , size: GetArgSize ( args [ 4 ] ) );
                        argcount++;
                        continue;
                    }
                    PrintDebugArgs ( args );
                }
                if ( argcount < argsbuffer . Count )
                    error = "One or more invalid arguments identified";
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( ex . Message );
            }
            return pms;
        }

        public static void PrintDebugArgs ( string [ ] args )
        {
            string tmp="";
            tmp = $"\n@Argument" . PadRight ( 20 );
            string output = $"{tmp}{args[0]}\n";
            tmp = $"Arg. Name" . PadRight ( 20 );
            output += $"{tmp}{args [ 1 ]}\n";
            tmp = $"Arg Type" . PadRight ( 20 );
            output += $"{tmp}{args [ 2 ]}\n";
            tmp = $"Direction" . PadRight ( 20 );
            output += $"{tmp}{args [ 4 ]}\n";
            tmp = $"Arg Size" . PadRight ( 20 );
            output += $"{tmp}{args [ 3 ]}\n";
            Debug . WriteLine ( output + $"\n" );
        }

        public static string [ ] PadArgsArray ( string [ ] content )
        {
            string[] tmp = new string[MAXARGSIZE ];
            for ( int x = 0 ; x < MAXARGSIZE ; x++ )
            {
                tmp [ x ] = "";
            }
            return tmp;
        }
        public static string [ ] PadArgsArrayNulls ( ref string [ ] content )
        {
            string[] tmp = new string[MAXARGSIZE ];
            for ( int x = 0 ; x < MAXARGSIZE ; x++ )
            {
                if ( content [ x ] == null )
                    tmp [ x ] = "";
                else tmp [ x ] = content [ x ];
            }
            content = tmp;
            return tmp;
        }

        static public DbType GetArgType ( string type )
        {
            if ( type == "" )
                return DbType . String;
            if ( type . Contains ( "STR" ) || type . Contains ( "STR" ) )
                return DbType . String;
            if ( type . Contains ( "INT" ) )
                return DbType . Int32;
            if ( type . Contains ( "FLOAT" ) )
                return DbType . Double;
            if ( type . Contains ( "VARCHAR" ) )
                return DbType . String;
            if ( type . Contains ( "VARBIN" ) )
                return DbType . Binary;
            if ( type . Contains ( "TEXT" ) )
                return DbType . String;
            if ( type . Contains ( "BIT" ) )
                return DbType . Boolean;
            if ( type . Contains ( "BOOL" ) )
                return DbType . Boolean;
            if ( type . Contains ( "SMALLINT" ) )
                return DbType . Int16;
            if ( type . Contains ( "BIGINT" ) )
                return DbType . Int64;
            if ( type . Contains ( "DOUBLE" ) )
                return DbType . Double;
            if ( type . Contains ( "DEC" ) )
                return DbType . Decimal;
            if ( type . Contains ( "CURR" ) )
                return DbType . Currency;
            if ( type . Contains ( "DATETIME" ) )
                return DbType . DateTime;
            if ( type . Contains ( "DATE" ) )
                return DbType . Date;
            if ( type . Contains ( "TIMESTAMP" ) )
                return DbType . Time;
            if ( type . Contains ( "TIME" ) )
                return DbType . Time;

            return DbType . Object;
        }
        static public int GetArgSize ( string args )
        {
            int size = 0;
            if ( args == null || args == "" )
                return 0;
            if ( args != "" && args != "MAX" )
            {
                char ch;
                for ( int x = 0 ; x < args . Length ; x++ )
                {
                    ch = args [ x ];
                    if ( Char . IsDigit ( ch ) == false )
                        return 0;
                }
                return ( Convert . ToInt32 ( args ) );
            }
            else if ( args == "MAX" )
                return 64000;
            return 64000;
        }
        static public ParameterDirection GetDirection ( string args )
        {
            if ( args == "" || args . Contains ( "IN" ) )
                return ParameterDirection . Input;
            else if ( args . Contains ( "OUT" ) )
                return ParameterDirection . Output;

            return ParameterDirection . Input;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion Support methods
        private void CloseBtn_Click ( object sender, RoutedEventArgs e )
        {
            this . Close ( );
        }

        private void Paste_Click ( object sender, RoutedEventArgs e )
        {
            string output = "";
            string buff = "";
            string interim = "";
            int arraycount = 0;
            string[] arg = null;
            string[] initial = SPHeaderblock.Text.Split("\n");

            for ( int x = 0 ; x < initial . Length ; x++ )
            {   // Check for empty entries after split
                if ( initial [ x ] == null || initial [ x ] . Length != 0 )
                    arraycount++;
            }

            string[] parts = new string[arraycount];
            string fullarg = "";
            // clean up the entries first
            for ( int x = 0 ; x < arraycount ; x++ )
            {
                parts [ x ] = initial [ x ] . TrimEnd ( ) . TrimStart ( );
            }
            for ( int x = 0 ; x < parts . Length ; x++ )
            {
                // strip [  Input : ] presention text from each line in parts[]
                if ( parts [ x ] . ToUpper ( ) . Contains ( "INPUT : " ) )
                    parts [ x ] = parts [ x ] . Substring ( 8 );
                if ( parts [ x ] . ToUpper ( ) . Contains ( "OUTPUT : " ) )
                    parts [ x ] = parts [ x ] . Substring ( 9 );
                parts [ x ] = SProcsSupport . CheckForComment ( parts [ x ] );
            }
            arg = parts;
            for ( int y = 0 ; y < arg . Length ; y++ )
            {
                if ( arg [ y ] . Contains ( "CREATE PROCEDURE" ) )
                    continue;
                // get next argument line
                string input = arg[y];
                parts = arg [ y ] . Split ( ' ' );

                //strip leading/Trailing commas
                for ( int x = 0 ; x < parts . Length ; x++ )
                {
                    parts [ x ] = SProcsSupport . CheckForCommas ( parts [ x ] );
                }
                // now we can parse current phrase out
                for ( int x = 0 ; x < parts . Length ; x++ )
                {
                    // is it an argument name ?
                    if ( parts [ x ] . StartsWith ( "@" ) )
                    {
                        fullarg = parts [ x ];
                        continue;
                    }
                    // check for various data Type identifiers
                    interim = SProcsSupport . CheckForVarchar ( KeepTypes, parts [ x ] . Trim ( ) );
                    fullarg += $" {interim}";
                    continue;
                }
                buff = fullarg;
                if ( output . Length == 0 )
                    output += $"{buff}";
                else
                    output += $", {buff}";
            }

            Clipboard . SetText ( output );
        }

        /// <summary>
        /// Clever method that loads any selected  Stored Procedure into a ScrollViewer
        /// in Genericgrid and SpResultsViewer widows independently of each other.
        /// The Document viewer higlights the current Search term in the SP loaded.
        /// </summary>
        /// <param name="win">Caller window</param>
        /// <param name="spfilename">SP to be loaded</param>
        /// <param name="sptext">Search Text to be highlighted</param>
        /// <returns></returns>	

        private void SProcsListbox_SizeChanged ( object sender, SizeChangedEventArgs e )
        {
            // ListBox lb = sender as ListBox;
        }

        #region  Generic (Combo) Data Loading methods
        //#################################//

        public void LoadColorsCombo ( )
        {
            "" . Track ( 0 );
            List<string> colors = new();
            spviewer . ColorsCombo . Items . Clear ( );
            foreach ( comboclrs item in ColorsCombo . Items )
            {
                colors . Add ( item . name );
                Brushcolors . Add ( item . Bground );
            }
            spviewer . ColorsCombo . ItemsSource = colors;
            spviewer . ColorsCombo . SelectedIndex = 113;
            "" . Track ( 1 );
        }
        public static void LoadFontSizes ( )
        {
            "" . Track ( 0 );
            SProcsHandling sp = SProcsHandling.spviewer;
            List<string> sizes = new();
            sizes . Add ( "10" );
            sizes . Add ( "11" );
            sizes . Add ( "12" );
            sizes . Add ( "14" );
            sizes . Add ( "16" );
            sizes . Add ( "17" );
            sizes . Add ( "18" );
            sizes . Add ( "19" );
            sizes . Add ( "20" );
            sizes . Add ( "22" );
            sizes . Add ( "24" );
            sp . FontSizeCombo . ItemsSource = sizes;
            sp . FontSizeCombo . SelectedIndex = 4; // "16"
            "" . Track ( 1 );
        }
        public void LoadSqlTableNames ( )
        {
            List<string> list = new List<string>();
            List<string> SqlTablesList = new List<string>();
            // Returns an IEnumerable <dynamic> collection
            IEnumerable<dynamic> Ienum = GenDapperQueries.CallStoredProcedure(list, "spGetTablesList");
            "" . Track ( 0 );

            if ( Ienum != null )
            {
                var ie = Ienum.GetEnumerator();
                if ( ie != null )
                {
                    foreach ( var item in Ienum )
                    {
                        if ( ie . MoveNext ( ) )
                            SqlTablesList . Add ( item );
                    }
                }
            }
            GridCombo . ItemsSource = SqlTablesList;
            int counter = 0;
            foreach ( var item in SqlTablesList )
            {
                if ( item . ToUpper ( ) == "BANKACCOUNT" )
                    break;
                counter++;
            }
            GridCombo . SelectedIndex = counter;
            "" . Track ( 1 );
        }

        #endregion   Generic (Combo) Data Loading methods

        private void ColorsCombo_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            // Actually works !! 6/2/23
            //Changes the HiLite color of the selected search term in FlowDocument TextResult.
            ComboBox cb = sender as ComboBox;
            int selindex = cb.SelectedIndex;
            string currColor = cb.SelectedItem.ToString();
            string CurrProc = SProcsListbox.SelectedItem.ToString();
            int indx = SProcsListbox.SelectedIndex;
            comboclrs cc = CbColorsList[indx];
            SolidColorBrush newcolor = cc.Bground;
            SProcsHandling sp = SProcsHandling.spviewer;
            sp . ScrollViewerHiliteColor = newcolor;
            string sptext = Searchtext;
            FlowDocument currentdoc = TextResult.Document;
            LoadShowMatchingSproc ( this, TextResult, SProcsListbox . SelectedItem . ToString ( ), ref sptext );
            TextResult . UpdateLayout ( );
            return;
        }

        public comboclrs GetCurrentSelectionItem ( )
        {
            comboclrs cc = new();
            cc = ( comboclrs ) ColorsCombo . SelectedItem;
            return cc;
        }

        #region  selection changing
        //#################################//

        private void SPDatagrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            DataGrid dg = sender as DataGrid;
            if ( dg . Items . Count == 0 || dg . SelectedIndex == -1 )
                return;
            // Set public properties
            CurrentSelectionIndex = dg . SelectedIndex;
            GenericClass gc = new();
            if ( dg . SelectedItem == null )
                return;
            gc = dg . SelectedItem as GenericClass;
            if ( gc == null )
                return;
            CurrentGenclass = gc;
            CurrentSelectionId = gc . field1;
            DataGridRow dgr = new();
            dgr = dg . SelectedValue as DataGridRow;
            if ( EditPanel . Visibility == Visibility . Visible )
            {
                if ( bdirty )
                {
                    MessageBoxResult mbr = MessageBox.Show("There are 1 or more unsaved changes to the data in the Edit panel.\n\nDo you want to save those changes first ? or discard them !", "Unsaved Changes", MessageBoxButton.YesNo);
                    if ( mbr == MessageBoxResult . Yes )
                        return;

                }
                LoadSqlEditData ( null, null );
            }
        }

        private void FsizeCombo_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if ( IsLoading )
                return;
            SProcsHandling sp = SProcsHandling.spviewer;

            ComboBox cb = sender as ComboBox;
            if ( FontSizeCombo . SelectedItem == null )
                return;
            int selindex = cb.SelectedIndex;
            string currSize = cb.SelectedItem.ToString();
            double Fontsize = Convert . ToDouble ( currSize );
            ScrollViewerFontSize = Fontsize;
            string CurrProc = SProcsListbox.SelectedItem.ToString();
            string sptext = "";
            bool result = LoadShowMatchingSproc(this, TextResult, CurrProc, ref sptext);
            if ( TextResult . Visibility == Visibility . Visible )
            {
                string Arguments = SProcsSupport.GetSpHeaderBlock(sptext, this);
                if ( Arguments . Length == 0 || Arguments . Contains ( "No valid Arguments were found" ) == true
                    || Arguments . Contains ( "Either the \"AS\" or \"BEGIN \" statements are missing" )
                    || Arguments . StartsWith ( "ERROR -" ) )
                {
                    SPArguments . Text = "The Header Block or parameters in the S.Procedure appear to be invalid !";
                    Parameterstop . Text = Arguments;
                }
                else
                {
                    SPArguments . Text = Arguments;
                }
                TextResult . UpdateLayout ( );
            }
            //else if ( ResultsTextbox . Visibility == Visibility . Visible )
            //{
            ResultsTextbox . FontSize = Fontsize;
            ResultsTextbox . UpdateLayout ( );
            //}
            // if ( ResultsListBox . Visibility == Visibility . Visible )
            //{
            ResultsListBox . FontSize = Fontsize;
            ResultsListBox . UpdateLayout ( );
            //}
            //if ( ResultsDatagrid . Visibility == Visibility . Visible )
            //{
            ResultsDatagrid . FontSize = Fontsize;
            ResultsDatagrid . UpdateLayout ( );
            //}
            //if ( SPDatagrid . Visibility == Visibility . Visible )
            //{
            SPDatagrid . FontSize = Fontsize;
            SPDatagrid . RowHeight = Fontsize + 10;
            SPDatagrid . UpdateLayout ( );
            //}
            SProcsListbox . FontSize = Fontsize;
            SProcsListbox . UpdateLayout ( );
            ExecList . FontSize = Fontsize;
            ExecList . UpdateLayout ( );
        }

        #endregion selection changing

        private void SProcsViewer_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
        {
            MainWindow . MainWin . SetValue ( TopmostProperty, true );
        }

        public void RefreshDatagrid ( string tablename )
        {
            int currsel = SPDatagrid.SelectedIndex;
            TableReloading = true;
            CurrentDbName = tablename;
            SqlTable . Clear ( );
            SPDatagrid . ItemsSource = null;
            LoadSqlData ( tablename );
            SPDatagrid_Loaded ( null, null );
            SPDatagrid . SelectedIndex = currsel;
            SPDatagrid . ScrollIntoView ( CurrentDbName );
            Utils2 . ScrollRowInGrid ( SPDatagrid, currsel );
            TableReloading = false;
            SPDatagrid . UpdateLayout ( );
            Mouse . OverrideCursor = Cursors . Arrow;
        }

        private void edit_dgItem ( object sender, RoutedEventArgs e )
        {

            if ( SPDatagrid . Visibility == Visibility . Visible )
            {
                DataGrid dg = SPDatagrid;
                if ( dg == null )
                    return;
                GenericClass selection = dg.SelectedItem as GenericClass;
                if ( selection == null )
                {
                    MessageBox . Show ( "You MUST select a row before you can edit it ?", "No current record selected" );
                    return;
                }// create list of all fields in current record
                List<string> dgData = UnpackDgRecord(selection);
                //We now have all the data for this record in a list
                int totalfields = dgData.Count;
            }
        }

        public static List<string> UnpackDgRecord ( GenericClass selection )
        {
            // selectoin is current record as GenericClass record
            List<string> dgdata = new();
            dgdata . Add ( selection . field1 != null ? selection . field1 . Trim ( ) : "" );
            if ( dgdata [ 0 ] == "" )
                return null;
            else
            {
                while ( true )
                {
                    if ( selection . field2 == null )
                        break;
                    else
                        dgdata . Add ( selection . field2 != null ? selection . field2 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field3 == null )
                        break;
                    else
                        dgdata . Add ( selection . field3 != null ? selection . field3 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field4 == null )
                        break;
                    else
                        dgdata . Add ( selection . field4 != null ? selection . field4 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field5 == null )
                        break;
                    else
                        dgdata . Add ( selection . field5 != null ? selection . field5 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field6 == null )
                        break;
                    else
                        dgdata . Add ( selection . field6 != null ? selection . field6 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field7 == null )
                        break;
                    else
                        dgdata . Add ( selection . field7 != null ? selection . field7 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field8 == null )
                        break;
                    else
                        dgdata . Add ( selection . field8 != null ? selection . field8 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field9 == null )
                        break;
                    else
                        dgdata . Add ( selection . field9 != null ? selection . field9 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field10 == null )
                        break;
                    else
                        dgdata . Add ( selection . field10 != null ? selection . field10 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field11 == null )
                        break;
                    else
                        dgdata . Add ( selection . field11 != null ? selection . field11 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field12 == null )
                        break;
                    else
                        dgdata . Add ( selection . field12 != null ? selection . field12 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field13 == null )
                        break;
                    else
                        dgdata . Add ( selection . field13 != null ? selection . field13 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field14 == null )
                        break;
                    else
                        dgdata . Add ( selection . field14 != null ? selection . field14 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field15 == null )
                        break;
                    else
                        dgdata . Add ( selection . field15 != null ? selection . field15 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field16 == null )
                        break;
                    else
                        dgdata . Add ( selection . field16 != null ? selection . field16 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field17 == null )
                        break;
                    else
                        dgdata . Add ( selection . field17 != null ? selection . field17 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field18 == null )
                        break;
                    else
                        dgdata . Add ( selection . field18 != null ? selection . field18 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field19 == null )
                        break;
                    else
                        dgdata . Add ( selection . field19 != null ? selection . field19 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field20 == null )
                        break;
                    else
                        dgdata . Add ( selection . field20 != null ? selection . field20 . Trim ( ) . Trim ( ) : "" );
                    break;
                    ;
                }
            }
            return dgdata;
        }

        private void Datagrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if ( IsLoading )
                return;
            ComboBox cb = sender as ComboBox;

            if ( cb == null )
                return;
            string tablename = cb.SelectedItem as string;
            SqlTable . Clear ( );
            SPDatagrid . ItemsSource = null;
            SPDatagrid . Columns . Clear ( );
            CurrentDbName = tablename;
            LoadSqlData ( $"{tablename . ToUpper ( )}", true );

            // stop _loaded method from reloading current table
            TableReloading = true;
            SPDatagrid_Loaded ( sender, null );
            TableReloading = false;
            //  load 1st record into edit panel
            if ( EditPanel . Visibility == Visibility . Visible )
            {

                SPDatagrid . SelectedIndex = 0;
                Resetdata ( null, null );
                SPDatagrid . ScrollIntoView ( SPDatagrid . SelectedItem );
            }
            SetWindowTitleBar ( );
        }

        public string GetMenuEntry ( string menu, string key )
        {
            string result = "";
            foreach ( KeyValuePair<string, string> entry in cmContents )
            {
                if ( entry . Key == key )
                {
                    result = entry . Value;
                    break;
                }
            }
            return result;
        }

        public void AddMenuEntry ( string menu, string item, string prompt )
        {
            ContextMenuSupport . AddMenuItem ( ( ContextMenu ) this . FindResource ( menu ), item, prompt );
        }

        public void RemoveMenuEntry ( string menu, string item )
        {
            ContextMenuSupport . RemoveMenuItems ( ( ContextMenu ) this . FindResource ( menu ),
            item, null );
        }

        private void DoHandled ( object sender, MouseButtonEventArgs e )
        {
            MouseRightButtonDown = false;
            e . Handled = true;
        }

        private void SPDatagrid_Loaded ( object sender, RoutedEventArgs e )
        {
            // we  now know the Window is fuly loaded
            List<string> list = new();
            if ( IsLoading == false && TableReloading == false )
                return;
            // get the column info for the current  SQL table
            SqlDataMethods . GetDbTableColumns<GenericClass> (
             SqlTable,
            ref list,
             CurrentDbName,
             DbDomain,
             ref VarCharLength,
            ref this . dglayoutlist );
            "" . Track ( 0 );

            /// create colum headers for datagrid
            if ( TableReloading == false || IsLoading == true )
            {
                // Now we need to get some SQL data
                SqlTable = LoadSqlData ( "BANKACCOUNT", LoadGrid: true );
            }

            LoadTableIntoGrid ( SqlTable, SPDatagrid );

            if ( SPDatagrid . Visibility == Visibility . Visible )
            {
                SPDatagrid . ItemsSource = SqlTable;
                SPDatagrid . UpdateLayout ( );
            }
            else
            {
                SPDatagrid . ItemsSource = SqlTable;
            }
            SPDatagrid . SelectedIndex = 0;
            if ( SHOWGRIDFIRST )
            {
                show_DataGrid ( null, null );
                SetWindowTitleBar ( );
            }
            IsLoading = false;
            "" . Track ( 1 );
        }

        public bool LoadTableIntoGrid ( ObservableCollection<GenericClass> SqlTable, DataGrid SPDatagrid )
        {
            "" . Track ( 0 );
            // Clear datagrid
            SPDatagrid . ItemsSource = null;
            SPDatagrid . Items . Clear ( );
            // create columns matching current table
            CreateDatagridColumns ( );

            SPDatagrid . UpdateLayout ( );
            "" . Track ( 1 );
            return true;
        }

        public void CreateDatagridColumns ( )
        {
            GenericClass gc = new();
            if ( SqlTable . Count == 0 )
                return;
            if ( SPDatagrid . Columns . Count > 0 )
                SPDatagrid . Columns . Clear ( );
            gc = SqlTable [ 0 ] as GenericClass;
            for ( int x = 0 ; x < dglayoutlist . Count ; x++ )
            {
                DataGridTextColumn c1 = new DataGridTextColumn();
                if ( dglayoutlist [ x ] . Fieldname . ToUpper ( ) == "ID" )
                    c1 . IsReadOnly = true;
                c1 . Header = dglayoutlist [ x ] . Fieldname;
                c1 . Binding = new Binding ( GetGenericFieldNameByIndex ( x ) );
                c1 . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
                SPDatagrid . Columns . Add ( c1 );
            }
        }

        public void CreateNewSqlUpdateCommand ( List<string> UpdatedRecordData, GenericClass newgclass )
        {
            /* 
             Update SQL table itself from data in the DataEditWin.
             This is a smart method as it replaces  our generic column header names of field1, field2 ....
             with the valid field names of the SQL table itself via our (magic) dglayoutlist colection.

            This is because our underlying table is always a collection<GenericClass> so all fieldnames
            are fidl1, field2 .... field20, whereas the column headers etc shown are the ACTUAL Sql Table Column names (Clever eh?)
            so we have to massage the SQL statement to match the table column names against our own internal column names 
            of field1, field2 etc.

            This method does exactly that very elegantly.

             IT ACTUALLY WORKS VERY WELL and the table is updated SUCCESSFULLY
            */
            SqlCommand = $"Update {GridCombo . SelectedItem . ToString ( ) . ToUpper ( )} ";
            if ( UpdatedRecordData . Count == 1 )
                return;
            if ( UpdatedRecordData [ 1 ] != "" )
                SqlCommand += $"set {dglayoutlist [ 1 ] . Fieldname}={ConvertDataToSql ( UpdatedRecordData [ 1 ] )}";
            if ( UpdatedRecordData . Count == 2 )
                return;
            if ( UpdatedRecordData [ 2 ] != "" )
                SqlCommand += $",{dglayoutlist [ 2 ] . Fieldname}={ConvertDataToSql ( UpdatedRecordData [ 2 ] )}";
            if ( UpdatedRecordData . Count == 3 )
                return;
            if ( UpdatedRecordData [ 3 ] != "" )
                SqlCommand += $",{dglayoutlist [ 3 ] . Fieldname}={ConvertDataToSql ( UpdatedRecordData [ 3 ] )}";
            if ( UpdatedRecordData . Count == 4 )
                return;
            if ( UpdatedRecordData [ 4 ] != "" )
                SqlCommand += $", {dglayoutlist [ 4 ] . Fieldname} ={ConvertDataToSql ( UpdatedRecordData [ 4 ] )}";
            if ( UpdatedRecordData . Count == 5 )
                return;
            if ( UpdatedRecordData [ 5 ] != "" )
                SqlCommand += $", {dglayoutlist [ 5 ] . Fieldname} ={ConvertDataToSql ( UpdatedRecordData [ 5 ] )}";
            if ( UpdatedRecordData . Count == 6 )
                return;
            if ( UpdatedRecordData [ 6 ] != "" )
                SqlCommand += $", {dglayoutlist [ 6 ] . Fieldname} ={ConvertDataToSql ( UpdatedRecordData [ 6 ] )}";
            if ( UpdatedRecordData . Count == 7 )
                return;
            if ( UpdatedRecordData [ 7 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 7 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 7 ] )}";
            if ( UpdatedRecordData . Count == 8 )
                return;
            if ( UpdatedRecordData [ 8 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 8 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 8 ] )}";
            if ( UpdatedRecordData . Count == 9 )
                return;
            if ( UpdatedRecordData [ 9 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 9 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 9 ] )}";
            if ( UpdatedRecordData . Count == 10 )
                return;
            if ( UpdatedRecordData [ 10 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 10 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 10 ] )}";
            if ( UpdatedRecordData . Count == 11 )
                return;
            if ( UpdatedRecordData [ 11 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 11 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 11 ] )}";
            if ( UpdatedRecordData . Count == 12 )
                return;
            if ( UpdatedRecordData [ 12 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 12 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 12 ] )}";
            if ( UpdatedRecordData . Count == 13 )
                return;
            if ( UpdatedRecordData [ 13 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 13 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 13 ] )}";
            if ( UpdatedRecordData . Count == 14 )
                return;
            if ( UpdatedRecordData [ 14 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 14 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 14 ] )}";
            if ( UpdatedRecordData . Count == 15 )
                return;
            if ( UpdatedRecordData [ 15 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 15 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 15 ] )}";
            if ( UpdatedRecordData . Count == 16 )
                return;
            if ( UpdatedRecordData [ 16 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 16 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 16 ] )}";
            if ( UpdatedRecordData . Count == 17 )
                return;
            if ( UpdatedRecordData [ 17 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 17 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 17 ] )}";
            if ( UpdatedRecordData . Count == 18 )
                return;
            if ( UpdatedRecordData [ 18 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 18 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 18 ] )}";
            if ( UpdatedRecordData . Count == 19 )
                return;
            if ( UpdatedRecordData [ 19 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 19 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 19 ] )}";
            return;
        }

        public bool UpdateSqlTable ( string SqlCommand )
        {
            return DapperSupport . UpdateDbTable ( SqlCommand );
        }

        private void ToggleTopmost ( object sender, RoutedEventArgs e )
        {
            if ( this . Topmost == true )
            {
                this . Topmost = false;
                this . Title = "S.Procs/Sql Tables Window Topmost status is Set to Normal";
            }
            else
            {
                this . Topmost = true;
                this . Title = "S.Procs/Sql Tables Window Topmost status is Set to TOPMOST";
            }
        }

        #region Sprocs mouse/key handlers
        //#################################//
        private void SProcsViewer_KeyDown ( object sender, KeyEventArgs e )
        {
            if ( e . Key == Key . F1 )
            {
                Debugger . Break ( );
            }
            if ( e . Key == Key . F2 )
            {
                string tmp="";
                Debug . WriteLine ( "Window sizes (ActualHeight):-" );
                Debug . WriteLine ( $"Total Window: {this . ActualHeight}		this height" );
                Debug . WriteLine ( $"Top rows: 60" );
                tmp = "SPFullDataContainerGrid" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {SPFullDataContainerGrid . ActualHeight}		Total internal grid" );
                tmp = "SPInfopanelGrid" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {SPInfopanelGrid . ActualHeight}		Top info rows" );
                tmp = "SPDatagrid" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {SPDatagrid . ActualHeight}		DataGrid height" );
                tmp = "EditPanel   " . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {EditPanel . ActualHeight}		Editpanel height" );
                tmp = "Bottompanel" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {Bottompanel . ActualHeight}		bottom panel height" );
                tmp = "ExecResult" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {ExecResult . ActualHeight}\t\tbottom info panel  height" );
                Debug . WriteLine ( $"Grand TOTAL: {SPFullDataContainerGrid . ActualHeight} - {SPDatagrid . ActualHeight} + {EditPanel . ActualHeight} )" );
            }
            if ( e . Key == Key . F4 )
            {
                // show TextResult
                ResultsContainerListbox . Visibility = Visibility . Collapsed;
                ResultsListBox . Visibility = Visibility . Collapsed;
                ResultsContainerTextblock . Visibility = Visibility . Collapsed;
                EditPanel . Visibility = Visibility . Collapsed;
                TextResult . Visibility = Visibility . Collapsed;
                ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
                TextResult . Visibility = Visibility . Visible;
            }
            if ( e . Key == Key . F6 )
            {
                // show Results box
                ResultsContainerTextblock . Visibility = Visibility . Collapsed;
                EditPanel . Visibility = Visibility . Collapsed;
                ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
                TextResult . Visibility = Visibility . Collapsed;
                //ResultsContainerListbox . Visibility = Visibility . Visible;
            }
            if ( e . Key == Key . F7 )
            {
                // show Results TextBlock
                ResultsContainerListbox . Visibility = Visibility . Collapsed;
                ResultsListBox . Visibility = Visibility . Collapsed;
                EditPanel . Visibility = Visibility . Collapsed;
                TextResult . Visibility = Visibility . Collapsed;
                ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
                ResultsDatagrid . Visibility = Visibility . Collapsed;
                ResultsContainerTextblock . Visibility = Visibility . Visible;
            }
            if ( e . Key == Key . F8 )
            {
                // show top 10 entries in SQLTABLE and Datagrid !!! - They SHOULD match
                //set current index for both sources
                int currsel = SPDatagrid.SelectedIndex;
                if ( currsel == -1 )
                {
                    SPDatagrid . SelectedIndex = 0;
                    currsel = 0;
                }
                Debug . WriteLine ( $"\nSqlTable values for top 10 items from {currsel}:-" );
                for ( int x = currsel ; x < currsel + 10 ; x++ )
                {
                    GenericClass gc = new();
                    SPDatagrid . SelectedIndex = x;
                    gc = SPDatagrid . SelectedItem as GenericClass;

                    Debug . WriteLine ( $"Datagrid  : {gc . field3} : {gc . field2}" );
                    Debug . Write ( $"SqlTable  : {SqlTable [ x ] . field3} : {SqlTable [ x ] . field2}" );
                    if ( SqlTable [ x ] . field3 != gc . field3 )
                        Debug . WriteLine ( " **Mismatch identified**" );
                    else
                        Debug . WriteLine ( "\n" );
                    SPDatagrid . SelectedItem = x;
                }
            }
            if ( e . Key == Key . F9 )
            {
                string output = $"\nSizing information\n";
                output += $"\n" + "SProcsViewer" . ToString ( ) . PadRight ( 25 ) + $" : {SProcsViewer . Height}  : ActualHeight (Minus 240 pixels for the bottom panels)";
                Debug . WriteLine ( output );
            }
            if ( e . Key == Key . System )
            {
                string output = $"\nColors Combo data information\n";
                for ( int x = 0 ; x < ColorsCombo . Items . Count ; x++ )
                {
                    comboclrs cc = (comboclrs)ColorsCombo.Items[x];
                    output += $"\n" + $"{cc . name . PadRight ( 27 )} : {cc . Bground . ToString ( ) . PadRight ( 12 )}   {cc . Fground}";
                    Debug . WriteLine ( output );
                }
            }
        }
        private void SProcsListbox_KeyDown ( object sender, KeyEventArgs e )
        {
            ListBox lb = sender as ListBox;
            int index = lb . SelectedIndex;
            if ( lb == null )
                return;
            if ( e . Key == Key . Down )
            {
                string currselection = lb.SelectedItem.ToString();
                lb . SelectedIndex++;
            }
            else if ( e . Key == Key . Up )
            {
                index = lb . SelectedIndex;
                string currselection = lb.SelectedItem.ToString();
                if ( lb . SelectedIndex > 0 )
                    lb . SelectedIndex--;
            }
            else if ( e . Key == Key . Escape )
                HideResultsPanel ( sender, null ); // close S procs resuilts  viewer
        }

        private void SProcsListbox_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
        {
            LeftMousePressed = false;
            e . Handled = true;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion Sprocs mouse/key handlers

        public string GetGenericFieldNameByIndex ( int x )
        {
            string colname = "";
            colname = $"field{x + 1}";
            return colname;
        }
        private void SPDatagrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
        {
            if ( LEFTMOUSEDOWN == true )
            {
                e . Cancel = true;
                return;
            }
        }

        private void SPDatagrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
        {
        }

        private void dg_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
        {
        }

        #region splitter  handlers
        //++++++++++++++++++++++++++++++++//

        private void Splitter_DragStarted ( object sender, System . Windows . Controls . Primitives . DragStartedEventArgs e )
        {
            Dragdistance = 0;
            DragActive = true;
        }


        private void Splitter_DragCompleted ( object sender, System . Windows . Controls . Primitives . DragCompletedEventArgs e )
        {
            // All working fairly fine 7/2/2022  !!!
            if ( WinLoaded == false )
                return;
            double Totalheight = SPFullDataContainerGrid.ActualHeight - 100;
            RowHeight4 = Totalheight;
            double DivisibleArea = Totalheight;
            UpperDataArea = SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight;
            // save upper height to global
            RowHeight0 = UpperDataArea;

            // save lower height to global
            // TODO  do not knoow why  we need to add the 175 ???
            RowHeight2 = LowerDataArea = DivisibleArea - ( UpperDataArea + 185 );
            LowerDataArea = RowHeight2;
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
            // reset lower panel control heights
            if ( ShowSp )
            {
                TextResult . Height = RowHeight2;
            }
            //Debug . WriteLine ( $"\ndiff = 0 in dragcompleted ???\nTotalheight={Totalheight}, DivisibleArea={DivisibleArea}\nRowHeight0={RowHeight0}" +
            //    $",  RowHeight2 ={RowHeight2},  RowHeight4 = {RowHeight4}\nTotal = {RowHeight0 + RowHeight2}  + 175 = {RowHeight0 + RowHeight2 + 175}" );
            SPFullDataContainerGrid . UpdateLayout ( );
            return;
        }

        private void Splitter_MouseEnter ( object sender, MouseEventArgs e )
        {
            //e.Handled= true;
        }

        private void Splitter_MouseLeave ( object sender, MouseEventArgs e )
        {
        }

        private void Splitter_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
        {
        }

        private void Splitter_PreviewMouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
        {
            GridLength gl1 = new(SPFullDataContainerGrid.RowDefinitions[0].ActualHeight, GridUnitType.Pixel);
            GridLength gl2 = new(SPFullDataContainerGrid.RowDefinitions[2].ActualHeight, GridUnitType.Pixel);
            PreEditsplitterheight1 = gl1 . Value;
        }
        private void Togglesplitterreset ( object sender, RoutedEventArgs e )
        {
            AllowSplitterReset = !AllowSplitterReset;
        }

        //#################################//				
        #endregion END of splitter mouse handlers 

        #region SProcs mouse handlers
        //++++++++++++++++++++++++++++++++//

        private void SProcsListbox_MouseLeftButtonDown ( object sender, System . Windows . Input . MouseButtonEventArgs e )
        {
            ListBox lb = sender as ListBox;
            if ( lb == null )
                return;
            string currselection = lb.SelectedItem.ToString();
            SPHeaderblock . Text = currselection;
            LeftMousePressed = true;
            SProcsListbox_SelectionChanged ( sender, null );
            LeftMousePressed = false;
        }
        private void SProcsListbox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            ListBox lb = sender as ListBox;
            "" . Track ( 0 );

            // Load data into Scrollviewer
            string selname = "";
            if ( lb . SelectedItem != null )
            {
                // Reset info panel at bottom of Window
                ExecResult . Background = FindResource ( "Green8" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
                ExecResult . Text = "Execution Results Panel ...";
                ExecResult . UpdateLayout ( );

                selname = lb . SelectedItem . ToString ( );
                // Store search term in our dialog for easier access

                if ( TextResult . Document != null )
                {
                    TextResult . Document . Blocks . Clear ( );
                    TextResult . Document = null;
                }
                string sptext = "";
                // Update cosmetics
                if ( IsLoading == false )
                {
                    bool result = LoadShowMatchingSproc(this, TextResult, selname, ref sptext);
                    ShowParseDetails = true;
                    if ( ShowParseDetails )
                    {
                        string Arguments = SProcsSupport.GetSpHeaderBlock(sptext, this);
                        if ( Arguments . Length == 0 || Arguments . Contains ( "No valid Arguments were found" ) == true )
                        {
                            spviewer . SPArguments . Text = @$"No arguments are required.... to execute the Stored Procedure, just dbl-click the relevant Execution Method ...";
                            spviewer . Parameterstop . Text = @$"[No parameters required (or allowed)]";
                        }
                        else if ( Arguments . Contains ( "Either the \"AS\" or \"BEGIN \" statements are missing" )
                            || Arguments . StartsWith ( "ERROR -" ) )
                        {
                            SPArguments . Text = "The Header Block or parameters in the S.Procedure appear to be invalid !";
                            Parameterstop . Text = Arguments;
                        }
                        else
                            SPArguments . Text = Arguments;
                    }
                }
            }
            LeftMousePressed = false;
            "" . Track ( 1 );
        }

        //++++++++++++++++++++++++++++++++//
        #endregion  END of  SProcs mouse handlers

        #region Stored Procedure Execution handers

        #region ExecListBox Commands mouse handlers
        //#################################//

        private void ExecListbox_MouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
        {
            //if ( LeftMousePressed == false )
            //{
            //	return;
            //}
            ListBox lb = sender as ListBox;
            string currselection = lb.SelectedItem.ToString();
            LeftMousePressed = true;
            ExecListbox_SelectionChanged ( sender, null );
            LeftMousePressed = false;
        }
        private void ExecListbox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            int Count = 0;
            string ResultString = "";
            Type Objtype = null;
            object Obj = null;
            string Err = "";
            if ( IsLoading )
                return;
            // Execute the command selected
            SpExecution spe = new("IAN1");
            spe = SpExecution . GetSpExecution ( );
            SqlCommand = SProcsListbox . SelectedValue . ToString ( );
            // call the execution system from here. the return value is our result
            dynamic dyn = spe.Execute_click(SqlCommand, ref Count, ref ResultString, ref Objtype, ref Obj, out Err);
            if ( Err != "" )
            {
                ExecResult . Text = Err;
                ExecResult . Background = FindResource ( "Red4" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                ShowErrorPanel ( Obj, Count, Err );
            }
            else if ( Count == 0 )
            {
                ShowErrorPanel ( Obj, Count, Err );
            }
            else
            {
                // Handle a valid result
                string commandtype = ExecList.SelectedItem.ToString();
                string[] cmdstrings = new string[]
                    {
                    "1. SP returning a Table as ObservableCollection",
                    "2. SP returning a List<string> ",
                    "3. SP returning a String",
                    "4. SP returning an INT value",
                    "5. Execute Stored Procedure with return value",
                    "6. Execute Stored Procedure without return value"
            };
                ExecResult . Background = FindResource ( "Green4" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                // Clean up receiving controls to avoid exceptions
                ResultsListBox . ItemsSource = null;
                ResultsListBox . Items . Clear ( );
                ResultsDatagrid . ItemsSource = null;
                ResultsDatagrid . Items . Clear ( );

                // Close all previous result panels - JIC
                ResultsContainerTextblock . Visibility = Visibility . Collapsed;
                ResultsTextbox . Visibility = Visibility . Collapsed;
                ResultsContainerListbox . Visibility = Visibility . Collapsed;
                ResultsListBox . Visibility = Visibility . Collapsed;
                ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
                ResultsDatagrid . Visibility = Visibility . Collapsed;
                if ( SprocCreationGrid . Visibility == Visibility . Visible )
                    SprocCreationGrid . Visibility = Visibility . Collapsed;

                // Open results container grid ready for use
                //ResultsContainerListbox . Visibility = Visibility . Visible;

                if ( commandtype == cmdstrings [ 0 ] . Trim ( ) )
                {
                    //Display a Datagrid for results
                    if ( Count > 0 )
                    {
                        // Parse data into OBSCOLLECTION
                        EditPanel . Visibility = Visibility . Collapsed;
                        ResultsDatagrid . ItemsSource = null;
                        ObservableCollection<GenericClass>  querytable = new();
                        querytable = CreateCollectionFromDynamic ( dyn );
                        ResultsContainerDatagrid . Visibility = Visibility . Visible;
                        //// make results Datagrid ViSIBLE
                        ShowRt = true;
                        ResultsContainerDatagrid . Visibility = Visibility . Visible;
                        TextResult . Visibility = Visibility . Collapsed;
                        ResultsDatagrid . Visibility = Visibility . Visible;
                        ExecResult . Text = $"ObservableCollection results from Execution of {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )} has completed successfully";
                        // add data into our grid
                        ResultsDatagrid . ItemsSource = querytable;
                        if ( EditPanel . Visibility == Visibility . Visible )
                            ExecResult . Background = FindResource ( "Green7" ) as SolidColorBrush;
                        ResultsDatagrid . FontSize = ScrollViewerFontSize;
                        ResultsDatagrid . Focus ( );
                        ResultsDatagrid . SelectedIndex = 0;
                        ResultsDatagrid . SelectedItem = 0;
                        Utils2 . DoSuccessBeep ( );
                    }
                    return;
                }
                if ( commandtype == cmdstrings [ 1 ] . Trim ( ) )
                {
                    //Display a ListBox  for results
                    List<string> genericlist = CreateListFromDynamic ( dyn );
                    ResultsListBox . SelectedIndex = 0;
                    ResultsListBox . ScrollIntoView ( 0 );
                    if ( TextResult . Visibility == Visibility . Visible )
                        TextResult . Visibility = Visibility . Collapsed;
                    EditPanel . IsEnabled = false;
                    //                   genericlist = GetSPCollectionData ( SProcsListbox . SelectedItem . ToString ( ) );
                    // make results Listbox ViSIBLE
                    //ResultsContainerListbox . Visibility = Visibility . Visible;
                    ShowRt = true;
                    if ( ResultsListBox . Visibility == Visibility . Visible )
                        ResultsListBox . Visibility = Visibility . Collapsed;
                    ResultsListBox . Visibility = Visibility . Visible;
                    if ( SprocCreationGrid . Visibility == Visibility . Visible )
                    {
                        SprocCreationGrid . Visibility = Visibility . Collapsed;
                        ScriptEditorOpen = true;
                    }
                    ResultsListBox . ItemsSource = null;
                    if ( genericlist != null )
                    {
                        ResultsListBox . ItemsSource = null;
                        ResultsListBox . Items . Clear ( );
                        ResultsListBox . Items . Add ( $"EXECUTION RESULT for {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )}" );
                        ResultsListBox . Items . Add ( $"EXECUTION returned a total of {Count} items : (Hit Escape to return to Script Viewer)" );
                        if ( Count > 0 && ResultString == "SUCCESS" )
                        {
                            foreach ( var item in genericlist )
                            {
                                ResultsListBox . Items . Add ( $"{item}" );
                            }
                        }
                        else
                        {
                            ResultsListBox . Items . Add ( $"\tThe query returned no records, but NO error message was returned..." );
                            ResultsListBox . Items . Add ( $"\tThe parameters passed may be 'suspect', or the table queried is actually empty" );
                        }
                    }
                    if ( ResultsContainerListbox . Visibility == Visibility . Collapsed )
                    {
                        ResultsContainerListbox . Visibility = Visibility . Visible;
                        ResultsListBox . Visibility = Visibility . Visible;
                    }
                    ResultsListBox . Items . Add ( $"" );
                    ResultsListBox . Items . Add ( $"Dbl-Click this line or hit ESC to Close....\n\n" );
                    ResultsListBox . UpdateLayout ( );
                    ExecResult . Text = $"List<string> Results : Execution of {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )} has completed successfully";
                    ExecResult . Background = FindResource ( "Green7" ) as SolidColorBrush;
                    ResultsListBox . FontSize = ScrollViewerFontSize;
                    ResultsListBox . SelectedIndex = 0;
                    ResultsListBox . SelectedItem = 0;
                    ResultsListBox . ScrollIntoView ( 0 );
                    ResultsListBox . Focus ( );
                    Utils2 . DoSuccessBeep ( );
                }
                else if ( commandtype == cmdstrings [ 2 ] . Trim ( ) )
                {
                    //Display a single string result
                    if ( TextResult . Visibility == Visibility . Visible )
                        TextResult . Visibility = Visibility . Collapsed;

                    TextResult . Visibility = Visibility . Visible;
                    ResultsTextbox . Text = dyn . ToString ( );
                    Utils2 . DoSuccessBeep ( );
                }
                else if ( commandtype == cmdstrings [ 3 ] . Trim ( ) )
                {
                    //Display an int result
                    ShowResultsTextbox ( );
                    ResultsTextbox . Text = $"SUCCESSFUL EXECUTION of Stored procedure [ {SqlCommand . ToUpper ( )} ]\nby execution of the Method [ {commandtype . ToUpper ( )} ] \n" +
                        $"has been processed successfully...\n" +
                        $"\nThe Execution returned an integer value of [ {Count . ToString ( )}]  (Arguments entered were [{SPArguments . Text . ToUpper ( )}])\n\n" +
                        $"Hit Escape to hide this results panel and redisplay the S.P Scripts Viewer.\n";
                    ResultsTextbox . Background = FindResource ( "Green5" ) as SolidColorBrush;
                    ResultsTextbox . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    ResultsTextbox . FontSize = ScrollViewerFontSize;
                    Utils2 . DoSuccessBeep ( );
                }
                else if ( commandtype == cmdstrings [ 4 ] . Trim ( ) || commandtype == cmdstrings [ 5 ] . Trim ( ) )
                {
                    //Display an int result
                    ShowResultsTextbox ( );
                    ResultsTextbox . Text = $"Execution of Stored procedure [ {SqlCommand . ToUpper ( )} ] by execution of [ {commandtype . ToUpper ( )} ] \n" +
                        $"has been processed successfully...\n" +
                        $"\nThe Execution returned an integer value of [ {Count . ToString ( )} ]\n\n" +
                        $"Hit Escape to hide this results panel and display the S.Procedure Scripts viewer.\n";
                    ResultsTextbox . Background = FindResource ( "Green5" ) as SolidColorBrush;
                    ResultsTextbox . Foreground = FindResource ( "Red3" ) as SolidColorBrush;
                    ResultsTextbox . FontSize = ScrollViewerFontSize;
                    Utils2 . DoSuccessBeep ( );
                }
            }
        }
        private void ShowErrorPanel ( object Obj, int Count, string Err )
        {
            // Hadle ALL and any errors post S.P Execution by showing the Result TextBlock panel
            ResultsContainerTextblock . Visibility = Visibility . Visible;
            if ( TextResult . Visibility == Visibility . Visible )
                TextResult . Visibility = Visibility . Collapsed;
            else if ( EditPanel . Visibility == Visibility . Visible )
                EditPanel . Visibility = Visibility . Collapsed;
            //// make results Listbox ViSIBLE
            ShowRt = true;
            ResultsTextbox . Background = FindResource ( "Yellow1" ) as SolidColorBrush;
            ResultsTextbox . Foreground = FindResource ( "Blue2" ) as SolidColorBrush;
            ResultsTextbox . Visibility = Visibility . Visible;
            if ( SprocCreationGrid . Visibility == Visibility . Visible )
                SprocCreationGrid . Visibility = Visibility . Collapsed;
            if ( Err == "" )
            {
                ResultsTextbox . Text = $"**** EXECUTION ERROR MESSAGE *** :\n\nExecution of Stored Procedure {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )} was completed without any reported errors, but it failed to return any resulting items ?\n\nThis may be because the Table(s) that were accessed were  Empty, or perhaps you passed one or more incorrect Arguments to the S.Procedure, or finally it is always possible that the actual S.Procedure is faulty in some way ?\n\nYou can Hit ESCAPE to close this Results panel, or right click to choose from the many Context Menu Options...\n";

                ExecResult . Background = FindResource ( "Blue4" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;

                ExecResult . Text = "No return value - Suggest you try a different  Execution Method !";
                ExecResult . UpdateLayout ( );
                //               ResultsTextHeight = ResultsTextbox . ActualHeight;
            }
            else
            {
                ResultsTextbox . Text = $"**** EXECUTION ERROR MESSAGE *** :\nExecution of Stored Procedure {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )} has reported an error as shown below :-\n[{Err}]\n\nPlease correct the reported problem if that is possible, but if it appears to be an error in the script, report the error message shown above to your Data Administrator.\n\nYou can Hit ESCAPE to close this Results panel, or right click to choose from the many Context Menu Options...\n";

                //                ResultsTextHeight = ResultsTextbox . ActualHeight;
            }
            Thickness   th = new();
            th . Top = 3;
            ResultsTextbox . Margin = th;
            ResultsTextbox . FontSize = ScrollViewerFontSize;
            Utils2 . DoErrorBeep ( );
        }

        private void ShowResultsTextbox ( )
        {
            // Show Results TextBlock
            if ( ResultsContainerDatagrid . Visibility == Visibility . Visible )
                ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
            ResultsContainerListbox . Visibility = Visibility . Collapsed;
            ResultsContainerTextblock . Visibility = Visibility . Visible;
            if ( ResultsTextbox . Visibility == Visibility . Visible )
                ResultsTextbox . Visibility = Visibility . Collapsed;
            ResultsTextbox . Visibility = Visibility . Visible;
            ResultsContainerDatagrid . Visibility = Visibility . Visible;
            ResultsContainerDatagrid . UpdateLayout ( );
            ResultsTextbox . FontSize = ScrollViewerFontSize;
            ResultsTextbox . Focus ( );
        }
        private void ExecListbox_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
        {
            LeftMousePressed = false;
            MouseRightButtonDown = true;
            e . Handled = true;
        }

        #endregion ExecListBox Commands mouse handlers

        #endregion  END of  Stored Procedure Execution handers


        #region General supporting methods
        private void LoadSqlEditData ( object sender, RoutedEventArgs e )
        {
            InitDataEditWin ( CurrentDbName, dglayoutlist, SPDatagrid . SelectedIndex );
        }
        public void InitDataEditWin ( string SqlTable, List<DataGridLayout> Dglayoutlist, int currentindex )
        {
            int newweight = 0;
            CreateEditList ( CurrentSelectionIndex );
            // OriginalRecordData holds ORIGINAL data from record
            // so make true copy in datalist
            DuplicateRecordData = ObjectCopier . Clone ( OriginalRecordData );
            dglayoutlist = ObjectCopier . Clone ( Dglayoutlist );
            this . Topmost = true;
            CurrentDbName = SqlTable;
            reccount = OriginalRecordData . Count;
            // create cloned copy of original data so it doesn't get updated automatically
            UpdatedRecordData = ObjectCopier . Clone ( OriginalRecordData );
            currselection = currentindex;
            newweight = newweight = MainWindow . GetfontWeight ( "Normal" );
            editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
            editprompt . Text = "Edit content of current record and click \"Update Table\" to save any changes,  or ESC to exit";
            editprompt . Background = FindResource ( "Yellow2" ) as SolidColorBrush;
            editprompt . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
            Data2 . SelectionLength = Data2 . Text . Length;
            Data2 . Select ( 0, Data2 . Text . Length );
            // enable all fields 1st off
            Clearfields ( );
            DisableNullFields ( true, true );
            // clear all fields of any existing data
            populatedatafields ( dglayoutlist . Count );
            // disable unused fields
            DisableNullFields ( false );
        }
        private void populatedatafields ( int totalrecs )
        {
            int newweight = 0;
            // put data into edit fields and fill GenericClass with it as well
            for ( int x = 0 ; x < OriginalRecordData . Count ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        label1 . Content = dglayoutlist [ 0 ] . Fieldname;
                        Data1 . Text = OriginalRecordData [ x ];
                        gclass . field1 = OriginalRecordData [ x ];
                        if ( dglayoutlist [ 0 ] . fieldname . ToUpper ( ) == "ID" )
                        {
                            label1 . Content = "(Automatic value)";
                            label1 . Foreground = FindResource ( "Orange2" ) as SolidColorBrush;
                            newweight = MainWindow . GetfontWeight ( "DemiBold" );
                            label1 . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
                        }
                        break;
                    case 1:
                        label2 . Content = dglayoutlist [ 1 ] . Fieldname;
                        Data2 . Text = OriginalRecordData [ x ];
                        gclass . field2 = OriginalRecordData [ x ];
                        break;
                    case 2:
                        label3 . Content = dglayoutlist [ 2 ] . Fieldname;
                        Data3 . Text = OriginalRecordData [ x ];
                        gclass . field3 = OriginalRecordData [ x ];
                        break;
                    case 3:
                        label4 . Content = dglayoutlist [ 3 ] . Fieldname;
                        Data4 . Text = OriginalRecordData [ x ];
                        gclass . field4 = OriginalRecordData [ x ];
                        break;
                    case 4:
                        label5 . Content = dglayoutlist [ 4 ] . Fieldname;
                        Data5 . Text = OriginalRecordData [ x ];
                        gclass . field5 = OriginalRecordData [ x ];
                        break;
                    case 5:
                        label6 . Content = dglayoutlist [ 5 ] . Fieldname;
                        Data6 . Text = OriginalRecordData [ x ];
                        gclass . field6 = OriginalRecordData [ x ];
                        break;
                    case 6:
                        label7 . Content = dglayoutlist [ 6 ] . Fieldname;
                        Data7 . Text = OriginalRecordData [ x ];
                        gclass . field7 = OriginalRecordData [ x ];
                        break;
                    case 7:
                        label8 . Content = dglayoutlist [ 7 ] . Fieldname;
                        Data8 . Text = OriginalRecordData [ x ];
                        gclass . field8 = OriginalRecordData [ x ];
                        break;
                    case 8:
                        label9 . Content = dglayoutlist [ 8 ] . Fieldname;
                        Data9 . Text = OriginalRecordData [ x ];
                        gclass . field9 = OriginalRecordData [ x ];
                        break;
                    case 9:
                        label10 . Content = dglayoutlist [ 9 ] . Fieldname;
                        Data10 . Text = OriginalRecordData [ x ];
                        gclass . field10 = OriginalRecordData [ x ];
                        break;
                    case 10:
                        label11 . Content = dglayoutlist [ 10 ] . Fieldname;
                        Data11 . Text = OriginalRecordData [ x ];
                        gclass . field11 = OriginalRecordData [ x ];
                        break;
                    case 11:
                        label12 . Content = dglayoutlist [ 11 ] . Fieldname;
                        Data12 . Text = OriginalRecordData [ x ];
                        gclass . field12 = OriginalRecordData [ x ];
                        break;
                    case 12:
                        label13 . Content = dglayoutlist [ 12 ] . Fieldname;
                        Data13 . Text = OriginalRecordData [ x ];
                        gclass . field13 = OriginalRecordData [ x ];
                        break;
                    case 13:
                        label14 . Content = dglayoutlist [ 13 ] . Fieldname;
                        Data14 . Text = OriginalRecordData [ x ];
                        gclass . field14 = OriginalRecordData [ x ];
                        break;
                    case 14:
                        label15 . Content = dglayoutlist [ 14 ] . Fieldname;
                        Data15 . Text = OriginalRecordData [ x ];
                        gclass . field15 = OriginalRecordData [ x ];
                        break;
                    case 15:
                        label16 . Content = dglayoutlist [ 15 ] . Fieldname;
                        Data16 . Text = OriginalRecordData [ x ];
                        gclass . field16 = OriginalRecordData [ x ];
                        break;
                    case 16:
                        label17 . Content = dglayoutlist [ 16 ] . Fieldname;
                        Data17 . Text = OriginalRecordData [ x ];
                        gclass . field17 = OriginalRecordData [ x ];
                        break;
                    case 17:
                        label18 . Content = dglayoutlist [ 17 ] . Fieldname;
                        Data18 . Text = OriginalRecordData [ x ];
                        gclass . field18 = OriginalRecordData [ x ];
                        break;
                    case 18:
                        label19 . Content = dglayoutlist [ 18 ] . Fieldname;
                        Data19 . Text = OriginalRecordData [ x ];
                        gclass . field19 = OriginalRecordData [ x ];
                        break;
                    case 19:
                        label20 . Content = dglayoutlist [ 19 ] . Fieldname;
                        Data20 . Text = OriginalRecordData [ x ];
                        gclass . field20 = OriginalRecordData [ x ];
                        break;
                }
            }
            for ( int y = reccount + 1 ; y <= 20 ; y++ )
            {
                switch ( y )
                {
                    case 1:
                        label1 . IsEnabled = false;
                        break;
                    case 2:
                        label2 . IsEnabled = false;
                        break;
                    case 3:
                        label3 . IsEnabled = false;
                        break;
                    case 4:
                        label4 . IsEnabled = false;
                        break;
                    case 5:
                        label5 . IsEnabled = false;
                        break;
                    case 6:
                        label6 . IsEnabled = false;
                        break;
                    case 7:
                        label7 . IsEnabled = false;
                        break;
                    case 8:
                        label8 . IsEnabled = false;
                        break;
                    case 9:
                        label9 . IsEnabled = false;
                        break;
                    case 10:
                        label10 . IsEnabled = false;
                        break;
                    case 11:
                        label11 . IsEnabled = false;
                        break;
                    case 12:
                        label12 . IsEnabled = false;
                        break;
                    case 13:
                        label13 . IsEnabled = false;
                        break;
                    case 14:
                        label14 . IsEnabled = false;
                        break;
                    case 15:
                        label15 . IsEnabled = false;
                        break;
                    case 16:
                        label16 . IsEnabled = false;
                        break;
                    case 17:
                        label17 . IsEnabled = false;
                        break;
                    case 18:
                        label18 . IsEnabled = false;
                        break;
                    case 19:
                        label19 . IsEnabled = false;
                        break;
                    case 20:
                        label20 . IsEnabled = false;
                        break;
                }
                bdirty = false;
            }
            editprompt . Text = "Edit content of current record and click \"Update Table\" to save any changes,  or ESC to exit";
            newweight = MainWindow . GetfontWeight ( "Normal" );
            editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
            editprompt . Background = FindResource ( "Yellow2" ) as SolidColorBrush;
            editprompt . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
        }
        private void Closewin ( object sender, RoutedEventArgs e )
        {
            int newweight = 0;
            Mouse . OverrideCursor = Cursors . Wait;
            editprompt . Text = "Just checking for unsaved  data changes ...";
            editprompt . UpdateLayout ( );
            GenericClass gc = new GenericClass();
            if ( bdirty && CheckForChanges ( "", -1, true ) == false )
            {
                Mouse . OverrideCursor = Cursors . Arrow;
                MessageBoxResult mbr = MessageBox.Show("There are changes in the current data being edited.\n\nDo you want to save them ?", "Unsaved changes ?", MessageBoxButton.YesNo);
                if ( mbr == MessageBoxResult . Yes )
                {
                    // create new list of data in newlist
                    GetUpdatedRecordData ( );
                    UpdateDb ( );
                    OriginalRecordData = ObjectCopier . Clone ( UpdatedRecordData );
                }
                else
                {
                    ExecResult . Text = "Changes were made to data but they have NOT been saved !";
                    newweight = MainWindow . GetfontWeight ( "Bold" );
                    if ( newweight != -1 )
                    {
                        editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
                        editprompt . Background = FindResource ( "Red5" ) as SolidColorBrush;
                        editprompt . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    }
                }
                RefreshDatagrid ( CurrentDbName );
            }
            this . Close ( );
            Mouse . OverrideCursor = Cursors . Arrow;
        }
        private bool UpdateDb ( )
        {
            int currsel = SPDatagrid.SelectedIndex;
            Mouse . OverrideCursor = Cursors . Wait;
            List<string> newdata = GetUpdatedRecordData();
            for ( int x = 0 ; x < newdata . Count ; x++ )
            {
                if ( x == 0 )
                    newgclass . field1 = newdata [ x ];
                if ( x == 1 )
                    newgclass . field2 = newdata [ x ];
                if ( x == 2 )
                    newgclass . field3 = newdata [ x ];
                if ( x == 3 )
                    newgclass . field4 = newdata [ x ];
                if ( x == 4 )
                    newgclass . field5 = newdata [ x ];
                if ( x == 5 )
                    newgclass . field6 = newdata [ x ];
                if ( x == 6 )
                    newgclass . field7 = newdata [ x ];
                if ( x == 7 )
                    newgclass . field8 = newdata [ x ];
                if ( x == 8 )
                    newgclass . field9 = newdata [ x ];
                if ( x == 9 )
                    newgclass . field10 = newdata [ x ];
                if ( x == 1 )
                    newgclass . field11 = newdata [ x ];
                if ( x == 11 )
                    newgclass . field12 = newdata [ x ];
                if ( x == 12 )
                    newgclass . field13 = newdata [ x ];
                if ( x == 13 )
                    newgclass . field14 = newdata [ x ];
                if ( x == 14 )
                    newgclass . field15 = newdata [ x ];
                if ( x == 15 )
                    newgclass . field16 = newdata [ x ];
                if ( x == 16 )
                    newgclass . field17 = newdata [ x ];
                if ( x == 17 )
                    newgclass . field18 = newdata [ x ];
                if ( x == 18 )
                    newgclass . field19 = newdata [ x ];
                if ( x == 19 )
                    newgclass . field20 = newdata [ x ];

            }
            CreateNewSqlUpdateCommand ( UpdatedRecordData, newgclass );
            SProcsHandling . SqlCommand += $" where {dglayoutlist [ 0 ] . Fieldname}={UpdatedRecordData [ 0 ]}";
            Debug . WriteLine ( $"{SProcsHandling . SqlCommand}	" );

            if ( UpdateSqlTable ( SProcsHandling . SqlCommand ) == true )
            {
                bdirty = false;
                currsel = SPDatagrid . SelectedIndex;
                if ( currsel == -1 )
                    currsel = 0;
                RefreshDatagrid ( CurrentDbName );
                SPDatagrid . SelectedIndex = currsel;
                Utils2 . DoSuccessBeep ( 2 );
                SPDatagrid . UpdateLayout ( );
                editprompt . Text = $"The Sql Table [{GridCombo . SelectedItem . ToString ( ) . ToUpper ( )}] has been updated successfuly....";
                editprompt . Background = FindResource ( "Purple4" ) as SolidColorBrush;
                editprompt . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            }
            else
            {
                editprompt . Text = $"The Sql Table [{GridCombo . SelectedItem . ToString ( ) . ToUpper ( )}] was NOT updated ....";
                editprompt . Background = FindResource ( "Red4" ) as SolidColorBrush;
                editprompt . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            }
            Mouse . OverrideCursor = Cursors . Arrow;
            return true;
        }
        public string Validatechars ( string dat )
        {
            string newstring = "";
            string validchars = "0123456789";
            if ( dat . Contains ( "'" ) )
                for ( int x = 0 ; x < dat . Length ; x++ )
                {
                    char dc = (char)dat[x];
                    if ( validchars . Contains ( dc ) == true )
                        newstring += dc;
                }
            else
                newstring = dat;
            dat = newstring;
            return newstring;
        }
        private List<string> GetUpdatedRecordData ( )
        {
            string dat = "";
            List<string> UpdatedRecordData = new List<string>();

            // Create UpdatedRecordData  list and genericclass with updated data
            for ( int x = 0 ; x < reccount ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        dat = Data1 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field1 = dat;
                        break;
                    case 1:
                        dat = Data2 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field2 = dat;
                        break;
                    case 2:
                        dat = Data3 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field3 = dat;
                        break;
                    case 3:
                        dat = Data4 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field4 = dat;
                        break;
                    case 4:
                        dat = Data5 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field5 = dat;
                        break;
                    case 5:
                        dat = Data6 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field6 = dat;
                        break;
                    case 6:
                        dat = Data7 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field7 = dat;
                        break;
                    case 7:
                        dat = Data8 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field8 = dat;
                        break;
                    case 8:
                        dat = Data9 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field9 = dat;
                        break;
                    case 9:
                        dat = Data10 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field10 = dat;
                        break;
                    case 10:
                        dat = Data11 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field11 = dat;
                        break;
                    case 11:
                        dat = Data12 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field12 = dat;
                        break;
                    case 12:
                        dat = Data13 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field13 = dat;
                        break;
                    case 13:
                        dat = Data14 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field14 = dat;
                        break;
                    case 14:
                        dat = Data15 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field15 = dat;
                        break;
                    case 15:
                        dat = Data16 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field16 = dat;
                        break;
                    case 16:
                        dat = Data17 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field17 = dat;
                        break;
                    case 17:
                        dat = Data18 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field18 = dat;
                        break;
                    case 18:
                        dat = Data19 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field19 = dat;
                        break;
                    case 19:
                        dat = Data20 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( Data20 . Text );
                        newgclass . field20 = Data20 . Text;
                        break;
                }
            }
            return UpdatedRecordData;
        }
        private void DisableAllfields ( )
        {
            for ( int x = reccount ; x < 20 ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        Data1 . IsEnabled = false;
                        break;
                    case 1:
                        Data2 . IsEnabled = false;
                        break;
                    case 2:
                        Data3 . IsEnabled = false;
                        break;
                    case 3:
                        Data4 . IsEnabled = false;
                        break;
                    case 4:
                        Data5 . IsEnabled = false;
                        break;
                    case 5:
                        Data6 . IsEnabled = false;
                        break;
                    case 6:
                        Data7 . IsEnabled = false;
                        break;
                    case 7:
                        Data8 . IsEnabled = false;
                        break;
                    case 8:
                        Data9 . IsEnabled = false;
                        break;
                    case 9:
                        Data10 . IsEnabled = false;
                        break;
                    case 10:
                        Data11 . IsEnabled = false;
                        break;
                    case 11:
                        Data12 . IsEnabled = false;
                        break;
                    case 12:
                        Data13 . IsEnabled = false;
                        break;
                    case 13:
                        Data14 . IsEnabled = false;
                        break;
                    case 14:
                        Data15 . IsEnabled = false;
                        break;
                    case 15:
                        Data16 . IsEnabled = false;
                        break;
                    case 16:
                        Data17 . IsEnabled = false;
                        break;
                    case 17:
                        Data18 . IsEnabled = false;
                        break;
                    case 18:
                        Data19 . IsEnabled = false;
                        break;
                    case 19:
                        Data20 . IsEnabled = false;
                        break;
                }
            }
        }
        public void CreateEditList ( int currselection )
        {
            if ( currselection == -1 )
                return;
            List<string> list = new();
            SPDatagrid . SelectedItem = currselection;
            GenericClass gc = SPDatagrid.Items[currselection] as GenericClass;
            if ( gc . field1 != null )
            {
                list . Add ( gc . field1 );
                label1 . IsEnabled = false;
            }
            else
                return;
            if ( gc . field2 != null )
                list . Add ( gc . field2 );
            else
            {
                label2 . IsEnabled = false;
            }
            if ( gc . field3 != null )
                list . Add ( gc . field3 );
            else
            {
                label3 . IsEnabled = false;
            }
            if ( gc . field4 != null )
                list . Add ( gc . field4 );
            else
            {
                label4 . IsEnabled = false;
            }
            if ( gc . field5 != null )
                list . Add ( gc . field5 );
            else
            {
                label5 . IsEnabled = false;
            }
            if ( gc . field6 != null )
                list . Add ( gc . field6 );
            else
            {
                label6 . IsEnabled = false;
            }
            if ( gc . field7 != null )
                list . Add ( gc . field7 );
            else
            {
                label7 . IsEnabled = false;
            }
            if ( gc . field8 != null )
                list . Add ( gc . field8 );
            else
            {
                label8 . IsEnabled = false;
            }
            if ( gc . field9 != null )
                list . Add ( gc . field9 );
            else
            {
                label9 . IsEnabled = false;
            }
            if ( gc . field10 != null )
                list . Add ( gc . field10 );
            else
            {
                label10 . IsEnabled = false;
            }


            if ( gc . field11 != null )
                list . Add ( gc . field11 );
            else
            {
                label11 . IsEnabled = false;
            }
            if ( gc . field12 != null )
                list . Add ( gc . field12 );
            else
            {
                label12 . IsEnabled = false;
            }
            if ( gc . field13 != null )
                list . Add ( gc . field13 );
            else
            {
                label13 . IsEnabled = false;
            }
            if ( gc . field14 != null )
                list . Add ( gc . field14 );
            else
            {
                label14 . IsEnabled = false;
            }
            if ( gc . field15 != null )
                list . Add ( gc . field15 );
            else
            {
                label15 . IsEnabled = false;
            }
            if ( gc . field16 != null )
                list . Add ( gc . field16 );
            else
            {
                label16 . IsEnabled = false;
            }
            if ( gc . field17 != null )
                list . Add ( gc . field17 );
            else
            {
                label17 . IsEnabled = false;
            }
            if ( gc . field18 != null )
                list . Add ( gc . field18 );
            else
            {
                label18 . IsEnabled = false;
            }
            if ( gc . field19 != null )
                list . Add ( gc . field19 );
            else
            {
                label19 . IsEnabled = false;
            }
            if ( gc . field20 != null )
                list . Add ( gc . field20 );
            else
            {
                label20 . IsEnabled = false;
            }
            //}
            OriginalRecordData = list;
            // create copy of original data for any changes  to be made  to
            UpdatedRecordData = list;
            populatedatafields ( dglayoutlist . Count );
        }
        private void Savedata ( object sender, RoutedEventArgs e )
        {
            // save data to sql table
            UpdateDb ( );
            bdirty = false;
        }
        public string ConvertDataToSql ( string datestring )
        {
            if ( datestring . Contains ( "/" ) )
            {
                string[] parts = datestring.Split(" ");
                string[] dateparts = parts[0].Split("/");
                string newdate = $"'{dateparts[2]}/{dateparts[1]}/{dateparts[0]}'";// {parts[1]}'";
                return newdate;
            }
            else
                return datestring;
        }
        private void NextRecord ( object sender, RoutedEventArgs e )
        {
            if ( currselection < SPDatagrid . Items . Count )
            {
                currselection++;
                SPDatagrid . SelectedIndex = currselection;
                SPDatagrid . ScrollIntoView ( SPDatagrid . SelectedItem );
                CreateEditList ( currselection );
            }
            editprompt . Text = "Next record shown ... ";
            editprompt . Background = FindResource ( "Purple3" ) as SolidColorBrush;
            editprompt . Foreground = Brushes . White;
            bdirty = false;
        }
        private void PreviousRecord ( object sender, RoutedEventArgs e )
        {
            if ( currselection > 0 )
            {
                currselection--;
                SPDatagrid . SelectedIndex = currselection;
                SPDatagrid . ScrollIntoView ( SPDatagrid . SelectedItem );
                CreateEditList ( currselection );
            }
            editprompt . Text = "Previous record shown ... ";
            editprompt . Background = FindResource ( "Green3" ) as SolidColorBrush;
            editprompt . Foreground = Brushes . White;
            bdirty = false;
        }
        private void AddRecord ( object sender, RoutedEventArgs e )
        {
            GenericClass gc = new();
            for ( int x = 0 ; x < OriginalRecordData . Count ; x++ )
            {
                Data1 . IsEnabled = false;
                Data1 . Text = "Automatic";
                if ( x >= 1 )
                {
                    Data2 . Text = "";
                    Data2 . IsEnabled = true;
                }
                if ( x >= 2 )
                {
                    Data3 . Text = "";
                    Data3 . IsEnabled = true;
                }
                if ( x >= 3 )
                {
                    Data4 . Text = "";
                    Data4 . IsEnabled = true;
                }
                if ( x >= 4 )
                {
                    Data5 . Text = "";
                    Data5 . IsEnabled = true;
                }
                if ( x >= 5 )
                {
                    Data6 . Text = "";
                    Data6 . IsEnabled = true;
                }
                if ( x >= 6 )
                {
                    Data7 . Text = "";
                    Data7 . IsEnabled = true;
                }
                if ( x >= 7 )
                {
                    Data8 . Text = "";
                    Data8 . IsEnabled = true;
                }
                if ( x >= 8 )
                {
                    Data9 . Text = "";
                    Data9 . IsEnabled = true;
                }
                if ( x >= 19 )
                {
                    Data10 . Text = "";
                    Data10 . IsEnabled = true;
                }
                if ( x >= 10 )
                {
                    Data11 . Text = "";
                    Data11 . IsEnabled = true;
                }
                if ( x >= 11 )
                {
                    Data12 . Text = "";
                    Data12 . IsEnabled = true;
                }
                if ( x >= 12 )
                {
                    Data13 . Text = "";
                    Data13 . IsEnabled = true;
                }
                if ( x >= 13 )
                {
                    Data14 . Text = "";
                    Data14 . IsEnabled = true;
                }
                if ( x >= 14 )
                {
                    Data15 . Text = "";
                    Data15 . IsEnabled = true;
                }
                if ( x >= 15 )
                {
                    Data16 . Text = "";
                    Data16 . IsEnabled = true;
                }
                if ( x >= 16 )
                {
                    Data17 . Text = "";
                    Data17 . IsEnabled = true;
                }
                if ( x >= 17 )
                {
                    Data18 . Text = "";
                    Data18 . IsEnabled = true;
                }
                if ( x >= 18 )
                {
                    Data19 . Text = "";
                    Data19 . IsEnabled = true;
                }
                if ( x >= 19 )
                {
                    Data20 . Text = "";
                    Data20 . IsEnabled = true;
                }
                //               Data2 . Focus ( );

            }
            editprompt . Text = "Enter data for this new record";
            editprompt . Background = Brushes . LightYellow;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion  END of  General supporting methods

        //        #region ContextMenu triggers
        //#################################//
        private void show_DataGrid ( object sender, RoutedEventArgs e )
        {
            // configure display  for SQL Tables (only)
            SProcsListbox . Visibility = Visibility . Collapsed;
            ExecList . Visibility = Visibility . Collapsed;
            TextResult . Visibility = Visibility . Collapsed;
            SPInfopanelGrid . Visibility = Visibility . Collapsed;

            ArgumentsContainerGrid . Visibility = Visibility . Visible;
            SPDatagrid . Visibility = Visibility . Visible;
            EditPanel . Visibility = Visibility . Visible;
            ISGRIDVISIBLE = true;
            ShowDg = true;
            ShowSp = false;
            SPDatagrid . Visibility = Visibility . Visible;
            DgridInfo . Visibility = Visibility . Visible;
            Blanker . Visibility = Visibility . Visible;
            SetWindowTitleBar ( );
            ResetOptionsAccessColors ( );
        }

        private void show_Sprocs ( object sender, RoutedEventArgs e )
        {
            // configure display  for Stored Procedures (only)
            SPDatagrid . Visibility = Visibility . Collapsed;
            DgridInfo . Visibility = Visibility . Collapsed;
            Blanker . Visibility = Visibility . Collapsed;
            EditPanel . Visibility = Visibility . Collapsed;
            ArgumentsContainerGrid . Visibility = Visibility . Visible;
            SPInfopanelGrid . Visibility = Visibility . Visible;
            SProcsListbox . Visibility = Visibility . Visible;
            ExecList . Visibility = Visibility . Visible;
            TextResult . Visibility = Visibility . Visible;
            ISGRIDVISIBLE = false;
            ShowDg = false;
            ShowSp = true;
            SetWindowTitleBar ( );
            ResetOptionsAccessColors ( );
        }
        private void ResetOptionsAccessColors ( )
        {
            if ( ShowSp )
            {
                //Showing Stored Procs controls
                if ( IsLoading )
                    return;
                OptionControls . IsEnabled = true;
                ColorsCombo . IsEnabled = true;
                ColorsCombo . Opacity = 1.0;
                FontSizeCombo . IsEnabled = true;
                FontSizeCombo . Opacity = 1.0;
                FontSizeCombo . IsEnabled = true;
                GridCombo . IsEnabled = false;
                DVO . Opacity = 0.5;
                SCST . Opacity = 0.5;
                GridCombo . Opacity = 0.5;
                Scriptoptions . Opacity = 1.0;
                Hilitecolor . Opacity = 1.0;
                Blanker . Visibility = Visibility . Collapsed;
            }
            else
            {
                //Showing Datagrid controls
                if ( IsLoading )
                    return;
                EditPanel . Opacity = 1.0;
                //useroptions . Opacity = 0.5;
                ColorsCombo . IsEnabled = false;
                ColorsCombo . Opacity = 0.5;
                FontSizeCombo . IsEnabled = true;
                FontSizeCombo . Opacity = 1.0;
                //colors combo
                GridCombo . IsEnabled = true;
                Scriptoptions . Opacity = 0.5;
                Hilitecolor . Opacity = 0.5;
                // Datagrid cobo
                DVO . Opacity = 1.0;
                SCST . Opacity = 1.0;
                GridCombo . Opacity = 1.0;
                OptionControls . IsEnabled = true;
                Blanker . Visibility = Visibility . Visible;
                if ( Data1 . Text == "" )
                {
                    SPDatagrid . SelectedIndex = 0;
                    SPDatagrid . SelectedItem = 0;
                    CreateEditList ( 0 );
                    populatedatafields ( SPDatagrid . Items . Count );
                    DisableNullFields ( false );
                }
            }
        }

        private void SetWindowTitleBar ( )
        {
            if ( ISGRIDVISIBLE )
            {
                ListTitle . Visibility = Visibility . Collapsed;
                ExecTitle . Visibility = Visibility . Collapsed;
                DgridInfo . Visibility = Visibility . Visible;
                if ( GridCombo . Items . Count > 0 )
                    DgridInfo . Text = $"Current SQL Table : [{GridCombo . SelectedItem . ToString ( ) . ToUpper ( )}] Total Records = {SqlTable . Count}";
                ExecTitle . Text = "";
            }
            else
            {
                DgridInfo . Visibility = Visibility . Collapsed;
                ListTitle . Visibility = Visibility . Visible;
                ExecTitle . Visibility = Visibility . Visible;
                ListTitle . Text = $"All Stored Procedures";
                ExecTitle . Text = "Available Execution Methods";
            }
        }

        #region Menu pre Show  handlers

        private void SetCurrentEntryMenuHeight ( ContextMenu cm,
            SolidColorBrush bkgrnd,
            SolidColorBrush fgrnd,
            string arg, int height = 0, int boldweight = 1 )
        {
            var v =  cm . Items ;
            for ( int x = 0 ; x < cm . Items . Count ; x++ )
            {
                MenuItem mi = cm . Items [x]  as MenuItem;
                if ( mi == null || mi . Header == null || mi . Header == "" ) continue;
                var vvv = mi.Header.ToString();
                if ( vvv == arg )
                {
                    if ( height == 0 )
                        mi . Height = 40;
                    Thickness th = new();
                    th . Top = 3; ;
                    mi . Padding = th;
                    if ( boldweight == 1 )
                        mi . FontWeight = Utils2 . GetfontWeight ( "DemiBold" );
                    else if ( boldweight == 2 )
                        mi . FontWeight = Utils2 . GetfontWeight ( "Bold" );
                    //else if ( boldweight == 3 )
                    //    mi . FontWeight = Utils2 . GetfontWeight ( "Italic" );
                    mi . Background = bkgrnd;
                    mi . Foreground = fgrnd;
                    break;
                }
            }
        }

        /// <summary>
        /// Handles the presentation of  the Context Menu intelligently depenfing
        /// on what panels are currenty visdible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // Open ContextMenu - conditionally

        //*****************************************//
        // Handle the s. Procedures Context menu options
        //*****************************************//
        private void ExecShowContextMenu ( object sender, MouseButtonEventArgs e )
        {
            // Open ContextMenu - conditionally
            ContextMenu cm = FindResource("SProcsContextmenu") as ContextMenu;
            cm . IsOpen = false;

            RemoveMenuEntry ( "SProcsContextmenu", "showsprocs" );

            // SQL CONTROLS
            RemoveMenuEntry ( "SProcsContextmenu", "showeditpanel" );
            RemoveMenuEntry ( "SProcsContextmenu", "hideeditpanel" );
            RemoveMenuEntry ( "SProcsContextmenu", "showfulldatagrid" );
            RemoveMenuEntry ( "SProcsContextmenu", "seteditpanelheight" );
            RemoveMenuEntry ( "SProcsContextmenu", "resetpanelsplit" );
            // S.Procs items
            RemoveMenuEntry ( "SProcsContextmenu", "showspscripts" );
            //            RemoveMenuEntry ( "SProcsContextmenu", "closeresultspanel" );
            RemoveMenuEntry ( "SProcsContextmenu", "processnewscript" );
            RemoveMenuEntry ( "SProcsContextmenu", "closenewscript" );

            //SP Editor options
            RemoveMenuEntry ( "SProcsContextmenu", "ExecuteSpEditor" );
            RemoveMenuEntry ( "SProcsContextmenu", "SaveResultsListToFile" );
            RemoveMenuEntry ( "SProcsContextmenu", "DiscardCloseSpEditor" );
            RemoveMenuEntry ( "SProcsContextmenu", "SaveCloseSpEditor" );
            RemoveMenuEntry ( "SProcsContextmenu", "SetAutoSplitterPosition" );
            RemoveMenuEntry ( "SProcsContextmenu", "DisableAutoSplitter" );

            // Execution results panel
            RemoveMenuEntry ( "SProcsContextmenu", "closeresultpanel" );
            RemoveMenuEntry ( "SProcsContextmenu", "closeresultspanel" );
            RemoveMenuEntry ( "SProcsContextmenu", "printesultspanel" );

            // Script Editing options
            RemoveMenuEntry ( "SProcsContextmenu", "setscriptpanelheight" );
            RemoveMenuEntry ( "SProcsContextmenu", "printscript" );

            //By here ALL other options are closed !!!!!

            if ( AllowSplitterReset )
                AddMenuEntry ( "SProcsContextmenu", "DisableAutoSplitter", "Stop Splitter bar resetting it's position" );
            else
                AddMenuEntry ( "SProcsContextmenu", "SetAutoSplitterPosition", "Allow splitter bar to reset it's position" );

            // Handle (permanent) Topmost setting
            if ( this . Topmost == false )
            {
                RemoveMenuEntry ( "SProcsContextmenu", "topmost" );
                AddMenuEntry ( "SProcsContextmenu", "nontopmost", "Set Window visibility to TopMost" );
            }
            else
            {
                RemoveMenuEntry ( "SProcsContextmenu", "nontopmost" );
                AddMenuEntry ( "SProcsContextmenu", "topmost", "Remove Window TopMost status" );
            }

            // Handle Results options
            if ( ResultsListBox . Visibility == Visibility . Visible
                || ResultsDatagrid . Visibility == Visibility . Visible
                || ResultsTextbox . Visibility == Visibility . Visible )
            {
                AddMenuEntry ( "SProcsContextmenu", "SaveResultsListToFile", "Save results from Listbox to a file" );
                AddMenuEntry ( "SProcsContextmenu", "closeresultpanel", "Close Execution Results Panel" );

                SetCurrentEntryMenuHeight ( cm,
                    ( SolidColorBrush ) FindResource ( "Green8" ),
                    ( SolidColorBrush ) FindResource ( "Red4" ),
                    "Close Execution Results Panel", height: 30 );

                if ( ResultsListBox . Visibility == Visibility . Visible )
                    AddMenuEntry ( "SProcsContextmenu", "printesultspanel", "Print Results Listbox contents" );
            }

            // Handle Script Creation panel setting
            if ( SprocCreationGrid . Visibility == Visibility . Visible )
            {
                //RemoveMenuEntry ( "SProcsContextmenu", "FulWinHeight" );
                //RemoveMenuEntry ( "SProcsContextmenu", "SetAutoSplitterPosition" );
                //RemoveMenuEntry ( "SProcsContextmenu", "DisableAutoSplitter" );
                //RemoveMenuEntry ( "SProcsContextmenu", "showdgrid" );
                //RemoveMenuEntry ( "SProcsContextmenu", "showsprocs" );
                AddMenuEntry ( "SProcsContextmenu", "ExecuteSpEditor", "EXECUTE new SP Script ..." );
                AddMenuEntry ( "SProcsContextmenu", "SaveCloseSpEditor", "Save new SP Script and close Editor" );
                AddMenuEntry ( "SProcsContextmenu", "printscript", "Print new S.Procedure script" );
                AddMenuEntry ( "SProcsContextmenu", "DiscardCloseSpEditor", "DISCARD new Script and close Editor" );
                AddMenuEntry ( "SProcsContextmenu", "setscriptpanelheight", "Refit script Editor in lower half" );
                //cm . IsOpen = true;
                //cm . Visibility = Visibility . Visible;
                //cm . UpdateLayout ( );
                //return;
            }
            else
            {
                AddMenuEntry ( "SProcsContextmenu", "seteditpanelheight", "Show/Fit lower panel to Window" );
                AddMenuEntry ( "SProcsContextmenu", "showeditpanel", "Show Sql Table edit panel" );
            }


            cm . IsOpen = true;
            cm . Visibility = Visibility . Visible;
            cm . UpdateLayout ( );
        }

        //*****************************************//
        // Handle the Datagrid Context menu options
        //*****************************************//
        private void ExecShowDgridContextMenu ( object sender, MouseButtonEventArgs e )
        {
            // Open ContextMenu - conditionally
            ContextMenu cm2 = FindResource("DgridContextmenu") as ContextMenu;
            cm2 . IsOpen = false;
            cm2 . Visibility = Visibility . Hidden;

            RemoveMenuEntry ( "DgridContextmenu", "dgshowsprocs" );

            // SQL CONTROLS
            RemoveMenuEntry ( "DgridContextmenu", "dgshoweditpanel" );
            RemoveMenuEntry ( "DgridContextmenu", "dgseteditpanelheight" );

            RemoveMenuEntry ( "DgridContextmenu", "dgshowfulldatagrid" );
            RemoveMenuEntry ( "DgridContextmenu", "dgresetpanelsplit" );
            RemoveMenuEntry ( "DgridContextmenu", "dghideeditpanel" );
            // S.Procs items

            //SP creation Editor options
            RemoveMenuEntry ( "DgridContextmenu", "dgprocessnewscript" );
            RemoveMenuEntry ( "DgridContextmenu", "dgclosenewscript" );
            RemoveMenuEntry ( "DgridContextmenu", "dgExecuteSpEditor" );
            RemoveMenuEntry ( "DgridContextmenu", "dgSaveCloseSpEditor" );
            RemoveMenuEntry ( "DgridContextmenu", "dgDiscardCloseSpEditor" );
            RemoveMenuEntry ( "DgridContextmenu", "dgSetAutoSplitterPosition" );
            RemoveMenuEntry ( "DgridContextmenu", "dgDisableAutoSplitter" );
            RemoveMenuEntry ( "DgridContextmenu", "dgSaveResultsListToFile" );

            // Execution results panel
            RemoveMenuEntry ( "DgridContextmenu", "dgCloseResultPanel" );
            RemoveMenuEntry ( "DgridContextmenu", "dgcloseresultspanel" );

            // Script Editing options
            RemoveMenuEntry ( "DgridContextmenu", "dgsetscriptpanelheight" );
            RemoveMenuEntry ( "DgridContextmenu", "dgprintscript" );

            RemoveMenuEntry ( "DgridContextmenu", "dgshowdgrid" );

            // Add default items
            AddMenuEntry ( "DgridContextmenu", "dgshowfulldatagrid", "Show Datagrid at full height" );
            AddMenuEntry ( "DgridContextmenu", "dghideeditpanel", "Hide Table Edit Panel" );
            AddMenuEntry ( "DgridContextmenu", "dgseteditpanelheight", "Reset Edit Panel Height" );

            AddMenuEntry ( "DgridContextmenu", "dgshowsprocs", "Switch to Stored Procedures View" );
            //AddMenuEntry("DgridContextmenu", "dgshowfulldatagrid" ,"");

            //By here ALL other options are closed !!!!!

            #region generic items
            if ( AllowSplitterReset == true )
                AddMenuEntry ( "DgridContextmenu", "dgDisableAutoSplitter", "Stop Splitter bar resetting it's position" );
            else
                AddMenuEntry ( "DgridContextmenu", "dgSetAutoSplitterPosition", "Allow splitter bar to reset it's position" );

            // Handle (permanent) Topmost setting
            if ( this . Topmost == false )
            {
                RemoveMenuEntry ( "DgridContextmenu", "dgtopmost" );
                AddMenuEntry ( "DgridContextmenu", "dgnontopmost", "Set Window visibility to TopMost" );
            }
            else
            {
                RemoveMenuEntry ( "DgridContextmenu", "dgnontopmost" );
                AddMenuEntry ( "DgridContextmenu", "dgtopmost", "Remove Window TopMost status" );
            }

            #endregion generic items

            if ( ResultsContainerListbox . Visibility == Visibility . Visible
                || ResultsContainerDatagrid . Visibility == Visibility . Visible
                || ResultsContainerTextblock . Visibility == Visibility . Visible )
            {
                AddMenuEntry ( "DgridContextmenu", "dgSaveResultsListToFile", "Save results from Listbox to a file" );
                AddMenuEntry ( "DgridContextmenu", "dgCloseResultPanel", "Close Execution Results Panel" );
                SetCurrentEntryMenuHeight ( cm2,
                    ( SolidColorBrush ) FindResource ( "Green8" ),
                    ( SolidColorBrush ) FindResource ( "Red6" ),
                    "Close Execution Results Panel" );
            }

            // Handle Script Creation panel setting
            if ( SprocCreationGrid . Visibility == Visibility . Visible )
            {
                // Creation panel is OPEN
                RemoveMenuEntry ( "DgridContextmenu", "dgFulWinHeight" );
                RemoveMenuEntry ( "DgridContextmenu", "dgSetAutoSplitterPosition" );
                RemoveMenuEntry ( "DgridContextmenu", "dgDisableAutoSplitter" );
                RemoveMenuEntry ( "DgridContextmenu", "dgshoweditpanel" );
                RemoveMenuEntry ( "DgridContextmenu", "dgshowsprocs" );
                AddMenuEntry ( "DgridContextmenu", "dgExecuteSpEditor", "EXECUTE new SP Script ..." );
                AddMenuEntry ( "DgridContextmenu", "dgSaveCloseSpEditor", "Save new SP Script and close Editor" );
                AddMenuEntry ( "DgridContextmenu", "dgDiscardCloseSpEditor", "DISCARD new Script and close Editor" );
                AddMenuEntry ( "DgridContextmenu", "dgprintscript", "Print new S.Procedure script" );
                AddMenuEntry ( "DgridContextmenu", "dgsetscriptpanelheight", "Refit script Editor in lower half" );
                cm2 . IsOpen = true;
                cm2 . Visibility = Visibility . Visible;
                cm2 . Visibility = Visibility . Visible;
                cm2 . UpdateLayout ( );
                return;
            }

            // Datagrid is visible
            if ( EditPanel . Visibility == Visibility . Visible && IsEditFullHeight )
            {
                // Edit panel is also visible at full height
                if ( IsGridFullHeight == false )
                {
                    AddMenuEntry ( "DgridContextmenu", "dghideeditpanel", "Hide Table Edit Panel" );
                    AddMenuEntry ( "DgridContextmenu", "dgseteditpanelheight", "Fit Edit panel to screen" );
                }
                else
                {
                    AddMenuEntry ( "DgridContextmenu", "dgseteditpanelheight", "Fit Edit panel to screen" );
                }
            }
            else if ( EditPanel . Visibility == Visibility . Visible && IsGridFullHeight == true )
            {
                // Edit panel is NOT visible as  grid is at full height
                AddMenuEntry ( "DgridContextmenu", "dgseteditpanelheight", "Show/Fit Data Edit panel in Window" );
            }
            //else if ( EditPanel . Visibility == Visibility . Visible )
            //{
            //    // edit panel half shown ?
            //    AddMenuEntry ( "DgridContextmenu", "dgseteditpanelheight", "Show/Fit Data Edit panel in Window" );
            //}
            else if ( EditPanel . Visibility == Visibility . Collapsed )
            {
                AddMenuEntry ( "DgridContextmenu", "dgseteditpanelheight", "Show/Fit lower panel to Window" );
                // Datagrid open , Edit panel closed
                //               AddMenuEntry ( "DgridContextmenu", "dgshoweditpanel", "Show Sql Table edit panel" );
            }
            cm2 . IsOpen = true;
            cm2 . Visibility = Visibility . Visible;
            cm2 . UpdateLayout ( );
        }


        private void Scrollviewer_MouseRightButtonDown ( object sender, System . Windows . Input . MouseButtonEventArgs e )
        {
            //check if it is FlowdocScrollviewer that is calling us & if so, configure menu options to suit it
            // depending on what is currently visible in top panel
            ContextMenu cm = FindResource("SProcsContextmenu") as ContextMenu;

            if ( SPDatagrid . Visibility == Visibility . Visible )
            {
                // DataGrid is Active in top pane
                RemoveMenuEntry ( "SProcsContextmenu", "ShowDgrid" );
                AddMenuEntry ( "SProcsContextmenu", "ShowSprocs", "Show S.Procedures List" );
            }
            else
            {
                // S.Procs are Active in top pane
                RemoveMenuEntry ( "SProcsContextmenu", "ShowSprocs" );
                AddMenuEntry ( "SProcsContextmenu", "ShowDgrid", "Show Data Grid viewer" );
            }
            cm . IsOpen = true;
        }
        //++++++++++++++++++++++++++++++++//
        #endregion  END of  ContextMenu triggers

        #region UTILITY SUPPORT METHODS

        private void DisableNullFields ( bool Enable, bool doall = false )
        {
            int startval = 1;
            if ( doall == false )
                startval = OriginalRecordData . Count;

            for ( int x = startval ; x < 20 ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        Data1 . IsEnabled = Enable;
                        break;
                    case 1:
                        Data2 . IsEnabled = Enable;
                        break;
                    case 2:
                        Data3 . IsEnabled = Enable;
                        break;
                    case 3:
                        Data4 . IsEnabled = Enable;
                        break;
                    case 4:
                        Data5 . IsEnabled = Enable;
                        break;
                    case 5:
                        Data6 . IsEnabled = Enable;
                        break;
                    case 6:
                        Data7 . IsEnabled = Enable;
                        break;
                    case 7:
                        Data8 . IsEnabled = Enable;
                        break;
                    case 8:
                        Data9 . IsEnabled = Enable;
                        break;
                    case 9:
                        Data10 . IsEnabled = Enable;
                        break;
                    case 10:
                        Data11 . IsEnabled = Enable;
                        break;
                    case 11:
                        Data12 . IsEnabled = Enable;
                        break;
                    case 12:
                        Data13 . IsEnabled = Enable;
                        break;
                    case 13:
                        Data14 . IsEnabled = Enable;
                        break;
                    case 14:
                        Data15 . IsEnabled = Enable;
                        break;
                    case 15:
                        Data16 . IsEnabled = Enable;
                        break;
                    case 16:
                        Data17 . IsEnabled = Enable;
                        break;
                    case 17:
                        Data18 . IsEnabled = Enable;
                        break;
                    case 18:
                        Data19 . IsEnabled = Enable;
                        break;
                    case 19:
                        Data20 . IsEnabled = Enable;
                        break;
                }
            }
        }
        private void Clearfields ( )
        {
            for ( int x = 0 ; x < 20 ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        Data1 . Text = "";
                        break;
                    case 1:
                        Data2 . Text = "";
                        break;
                    case 2:
                        Data3 . Text = "";
                        break;
                    case 3:
                        Data4 . Text = "";
                        break;
                    case 4:
                        Data5 . Text = "";
                        break;
                    case 5:
                        Data6 . Text = "";
                        break;
                    case 6:
                        Data7 . Text = "";
                        break;
                    case 7:
                        Data8 . Text = "";
                        break;
                    case 8:
                        Data9 . Text = "";
                        break;
                    case 9:
                        Data10 . Text = "";
                        break;
                    case 10:
                        Data11 . Text = "";
                        break;
                    case 11:
                        Data12 . Text = "";
                        break;
                    case 12:
                        Data13 . Text = "";
                        break;
                    case 13:
                        Data14 . Text = "";
                        break;
                    case 14:
                        Data15 . Text = "";
                        break;
                    case 15:
                        Data16 . Text = "";
                        break;
                    case 16:
                        Data17 . Text = "";
                        break;
                    case 17:
                        Data18 . Text = "";
                        break;
                    case 18:
                        Data19 . Text = "";
                        break;
                    case 19:
                        Data20 . Text = "";
                        break;
                }
            }
        }

        //#################################//
        #endregion END of  UTILITY SUPPORT METHODS

        /// <summary>
        /// Shows the "TextResults" Scroll viewer panel 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void show_Spscript ( object sender, RoutedEventArgs e )
        {
            //Force display of the contents of the currently selected script
            ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
            ResultsContainerListbox . Visibility = Visibility . Collapsed;
            ResultsContainerTextblock . Visibility = Visibility . Collapsed;
            EditPanel . Visibility = Visibility . Collapsed;
            TextResult . Visibility = Visibility . Visible;
        }

        #region main window menu Handlers
        //++++++++++++++++++++++++++++++++//

        private void Menu_MouseEnter ( object sender, MouseEventArgs e )
        {
            MenuItem mi = sender as MenuItem;
            if ( mi . Name == "viewsmenu" )
                ViewsMenuOpening ( sender, e );
            if ( mi . Name == "optsmenu" )
                OptionsMenuOpening ( sender, e );
            if ( mi . Name == "Helpmenu" )
                HelpMenuOpening ( sender, e );
        }

        private void MainWinmenu_MouseLeave ( object sender, MouseEventArgs e )
        {
            //MainWinmenu . Background = Brushes . Transparent;
        }

        private void Close_Click ( object sender, RoutedEventArgs e )
        {
            this . Close ( );
        }

        private void About_Click ( object sender, RoutedEventArgs e )
        {
            MessageBox . Show ( "I am working on this ...." );
        }

        private void ShowDgmenu_Click ( object sender, RoutedEventArgs e )
        {
            // Switch to SQL Table view from S.Procedure controls
            if ( ISGRIDVISIBLE == false )
            {
                SPDatagrid . Visibility = Visibility . Visible;
                TextResult . Visibility = Visibility . Collapsed;
                SProcsListbox . Visibility = Visibility . Collapsed;
                ShowSp = false;
                ShowDg = true;
                ExecList . Visibility = Visibility . Collapsed;
                EditPanel . Visibility = Visibility . Visible;
                Blanker . Visibility = Visibility . Visible;
                ArgumentsContainerGrid . Visibility = Visibility . Visible;
                ISGRIDVISIBLE = true;
                SPInfopanelGrid . Visibility = Visibility . Collapsed;
                CurrentMenuitem = ShowDgmenu;
                double SqlDataArea = SPFullDataContainerGrid . ActualHeight;
                // Set lower  panel to FULL Edit panel height
                SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( DefEditpanelHeight, GridUnitType . Pixel );
                // store parameters for SQL controls
                if ( SPDatagrid . SelectedIndex == -1 )
                    SPDatagrid . SelectedIndex = 0;
                // Hide reopen panel button
                EditPanel . Visibility = Visibility . Visible;
                if ( SPDatagrid . Items . Count == 0 )
                {
                    RefreshDatagrid ( "BANKACCOUNT" );
                }
                SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
                SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );

                ResetOptionsAccessColors ( );
            }
        }

        private void ShowSpmenu_Click ( object sender, RoutedEventArgs e )
        {
            if ( ISGRIDVISIBLE == true )
            {
                SPDatagrid . Visibility = Visibility . Collapsed;
                Blanker . Visibility = Visibility . Collapsed;
                EditPanel . Visibility = Visibility . Collapsed;

                TextResult . Visibility = Visibility . Visible;
                SProcsListbox . Visibility = Visibility . Visible;
                ExecList . Visibility = Visibility . Visible;
                ListTitle . Visibility = Visibility . Visible;
                SPInfopanelGrid . Visibility = Visibility . Visible;
                ArgumentsContainerGrid . Visibility = Visibility . Visible;
                SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0, GridUnitType . Pixel );
                SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2, GridUnitType . Pixel );
                // store parameters for SQL controls
                RowHeight0 = SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height . Value;
                Splitterlastpos = RowHeight0;
                SPFullDataContainerGrid . UpdateLayout ( );
                ISGRIDVISIBLE = false;
                CurrentMenuitem = ShowSpmenu;
                ShowSp = true;
                ShowDg = false;
                ResetOptionsAccessColors ( );
            }
        }

        private void showSPContent_Click ( object sender, RoutedEventArgs e )
        {
            TextResult . Visibility = Visibility . Visible;
            if ( EditPanel . Visibility == Visibility . Visible )
            {
                TextResult . Height = EditPanel . Height;
                EditPanel . Visibility = Visibility . Collapsed;
            }

        }

        //#################################//
        #endregion END of main window menu Handlers

        #region ALL SQL View Handlers

        private void Data_LostFocus ( object sender, RoutedEventArgs e )
        {
            TextBox tb = sender as TextBox;
            string[] parts = new string[2];

            if ( tb == null )
                return;
            tb . SelectionLength = tb . Text . Length;
            string fldname = tb.Name;
            string text = tb.Text;
            parts = fldname . Split ( "Data" );
            int fieldindex = Convert.ToInt32(parts[1]);

            if ( CheckForChanges ( text, fieldindex ) == false )
            {
                editprompt . Text = "Data now contains 1 or more changed fields... ";
                editprompt . Background = FindResource ( "Red4" ) as SolidColorBrush;
                editprompt . Foreground = Brushes . White;
                bdirty = true;
            }
            tb = sender as TextBox;
            tb . Background = FindResource ( "White0" ) as SolidColorBrush;
            int newweight = MainWindow.GetfontWeight("Normal");
            tb . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
            SetButtonColors ( false );

        }

        private void Data1_KeyDown ( object sender, KeyEventArgs e )
        {
            if ( e . Key == Key . Enter )
                UpdateList ( sender, null );
        }

        /// <summary>
        /// Crucial method to maintain the SQL table edit panel at 
        /// the right height  so all fields are visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="caller"></param>
        /// 

        private bool CheckForChanges ( string arg, int index, bool CheckAll = false )
        {
            //for (int x = 0 ; x < OriginalRecordData.Count ; x++)
            if ( OriginalRecordData . Count == 0 || arg == "" )
                return false;
            if ( CheckAll == false )
            {
                if ( OriginalRecordData [ ( index - 1 ) ] != arg )
                    return false;
            }
            else
            {
                for ( int x = 0 ; x < OriginalRecordData . Count ; x++ )
                {
                    if ( OriginalRecordData [ x ] != UpdatedRecordData [ x ] )
                    {
                        // ignore date felds
                        if ( OriginalRecordData [ x ] . Contains ( "/" ) == false )
                            return false;
                    }
                }
            }
            return true;
        }
        private void Resetdata ( object sender, RoutedEventArgs e )
        {
            int newweight = 0;
            populatedatafields ( dglayoutlist . Count );
            editprompt . Text = $"All entries have been reset to original values for you...";
            editprompt . Background = FindResource ( "Orange3" ) as SolidColorBrush;
            editprompt . Foreground = FindResource ( "Red3" ) as SolidColorBrush;
            newweight = MainWindow . GetfontWeight ( "Bold" );
            editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
        }

        private void UpdateList ( object sender, TextChangedEventArgs e )
        {
            TextBox tb = sender as TextBox;
            string name = tb.Name;
            string[] parts = new string[2];
            parts = name . Split ( "Data" );
            try
            {
                switch ( Convert . ToInt32 ( parts [ 1 ] ) )
                {
                    case 1:
                        UpdatedRecordData [ 0 ] = tb . Text;
                        break;
                    case 2:
                        UpdatedRecordData [ 1 ] = tb . Text;
                        break;
                    case 3:
                        UpdatedRecordData [ 2 ] = tb . Text;
                        break;
                    case 4:
                        UpdatedRecordData [ 3 ] = tb . Text;
                        break;
                    case 5:
                        UpdatedRecordData [ 4 ] = tb . Text;
                        break;
                    case 6:
                        UpdatedRecordData [ 5 ] = tb . Text;
                        break;
                    case 7:
                        UpdatedRecordData [ 6 ] = tb . Text;
                        break;
                    case 8:
                        UpdatedRecordData [ 7 ] = tb . Text;
                        break;
                    case 9:
                        UpdatedRecordData [ 8 ] = tb . Text;
                        break;
                    case 10:
                        UpdatedRecordData [ 9 ] = tb . Text;
                        break;
                    case 11:
                        UpdatedRecordData [ 10 ] = tb . Text;
                        break;
                    case 12:
                        UpdatedRecordData [ 11 ] = tb . Text;
                        break;
                    case 13:
                        UpdatedRecordData [ 12 ] = tb . Text;
                        break;
                    case 14:
                        UpdatedRecordData [ 13 ] = tb . Text;
                        break;
                    case 15:
                        UpdatedRecordData [ 14 ] = tb . Text;
                        break;
                    case 16:
                        UpdatedRecordData [ 15 ] = tb . Text;
                        break;
                    case 17:
                        UpdatedRecordData [ 16 ] = tb . Text;
                        break;
                    case 18:
                        UpdatedRecordData [ 17 ] = tb . Text;
                        break;
                    case 19:
                        UpdatedRecordData [ 18 ] = tb . Text;
                        break;
                    case 20:
                        UpdatedRecordData [ 19 ] = tb . Text;
                        break;
                }
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( "Error encountered..." );
            }
            bdirty = true;
        }

        //#################################//
        #region SPDatagrid mouse handlers
        //++++++++++++++++++++++++++++++++//
        private void SPDatagrid_PreviewMouseMove ( object sender, MouseEventArgs e )
        {
        }

        private void SPDatagrid_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
        {
            LEFTMOUSEDOWN = true;
        }

        private void SPDatagrid_PreviewMouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
        {
            LEFTMOUSEDOWN = false;
        }

        private void SPDatagrid_PreviewMouseDoubleClick ( object sender, MouseButtonEventArgs e )
        {

        }

        private void edit_dgItem ( object sender, MouseButtonEventArgs e )
        {
        }


        private void ShowDateEdtpanel_Click ( object sender, RoutedEventArgs e )
        {
            // forceit to be closed so the (show_Editpanel) method wil reopen it
            EditPanel . Visibility = Visibility . Collapsed;
            show_Editpanel ( null, null );
        }

        //#################################//
        #endregion END of SPDatagrid mouse handlers
        //++++++++++++++++++++++++++++++++//


        //#################################//
        #endregion END of ALL SQL View Handlers

        private void SProcsViewer_MouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
        {
            LeftMousePressed = false;
        }

        private void SProcsViewer_MouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
        {
            LeftMousePressed = true;
        }

        private void Data_GotFocus ( object sender, RoutedEventArgs e )
        {
            editprompt . Text = "Edit content of current record and click \"Update Table\" to save any changes,  or ESC to exit";
            editprompt . Background = FindResource ( "Yellow2" ) as SolidColorBrush;
            editprompt . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
            TextBox tb = sender as TextBox;
            tb . Background = FindResource ( "Yellow0" ) as SolidColorBrush;
            int newweight = MainWindow.GetfontWeight("DemiBold");
            tb . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
            SetButtonColors ( true );
        }

        private Menu Removemainmenu ( Menu sender, MenuItem mainmenu, MenuItem item )
        {
            bool alldone = false;
            foreach ( MenuItem entry in sender . Items )
            {
                if ( entry . Name == mainmenu . Name )
                {
                    foreach ( MenuItem mitem in entry . Items )
                    {
                        if ( mitem . Name == item . Name )
                        {

                            alldone = true;
                            mitem . Visibility = Visibility . Collapsed;
                            break;
                        }
                    }
                    if ( alldone )
                        break;
                }
            }
            return sender;
        }
        private Menu Addmainmenu ( Menu sender, MenuItem mainmenu, MenuItem item, string prompt )
        {
            bool alldone = false;
            foreach ( MenuItem entry in sender . Items )
            {
                if ( entry . Name == mainmenu . Name )
                {
                    foreach ( MenuItem mitem in entry . Items )
                    {
                        if ( mitem . Name == item . Name )
                        {
                            alldone = true;
                            mitem . Visibility = Visibility . Visible;
                            mitem . Header = prompt;
                            break;
                        }
                    }
                    if ( alldone )
                        break;
                }
            }
            return sender;
        }


        #region Main menu handling methods
        private void ViewsMenuOpening ( object sender, RoutedEventArgs e )
        {
            Menu dmenu=new();
            MenuItem mi = sender as MenuItem;
            string[] arr = new string[20];
            var menu = Utils.FindVisualParent<Menu>((UIElement)mi);
            if ( SPDatagrid . Visibility == Visibility . Visible )
            {
                ShowDg = true;
                ShowSp = false;
            }
            else
            {
                ShowDg = false;
                ShowSp = true;
            }
            MainWinmenu = Removemainmenu ( this . MainWinmenu, this . viewsmenu, ShowSpmenu );
            MainWinmenu = Removemainmenu ( this . MainWinmenu, this . viewsmenu, ShowDgmenu );
            if ( ShowDg )
            {
                MainWinmenu = Addmainmenu ( this . MainWinmenu, this . viewsmenu, ShowSpmenu, "Switch to S.Procedures View" );
                CurrentMenuitem = ShowSpmenu;
            }
            else if ( ShowSp )
            {
                MainWinmenu = Removemainmenu ( this . MainWinmenu, this . viewsmenu, ShowSpmenu );
                MainWinmenu = Removemainmenu ( this . MainWinmenu, this . viewsmenu, ShowDgmenu );
                dmenu = Addmainmenu ( this . MainWinmenu, this . viewsmenu, ShowDgmenu, "Switch to SQL Datagrid View" );
                //CurrentMenuitem = ShowSpmenu;
                CurrentMenuitem = ShowDgmenu;
            }
            if ( SprocCreationGrid . Visibility == Visibility . Visible )
            {
                MainWinmenu = Removemainmenu ( this . MainWinmenu, this . viewsmenu, ShowSpEditor );
                MainWinmenu = Addmainmenu ( this . MainWinmenu, this . viewsmenu, HideSpEditor, "Close S.Procedure Script Editor" );
            }
            else
            {
                MainWinmenu = Removemainmenu ( this . MainWinmenu, this . viewsmenu, HideSpEditor );
                MainWinmenu = Addmainmenu ( this . MainWinmenu, this . viewsmenu, ShowSpEditor, "Open S.Procedure Script Editor" );
            }
            var t = this.viewsmenu.IsVisible;
        }

        private void OptionsMenuOpening ( object sender, RoutedEventArgs e )
        {
            MenuItem mi = sender as MenuItem;
            MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, SaveCloseSpEditorMain );
            MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, DiscardCloseSpEditorMain );
            MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, processnewspscript );
            MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, UpdateRecord );
            MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, processnewspscript );
            MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, ShowGridFullheight );
            MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, mainprintscript );

            if ( SPDatagrid . Visibility == Visibility . Visible )
            {
                ShowDg = true;
                ShowSp = false;
                //Datagrid is visible
                MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, ShowSPSchema );
                MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, ShowSPSchema, "Show current SQL Table Schema" );
                MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, ShowGridFullheight, "Expand Datagrid to fit window" );

                // selective options
                if ( SPDatagrid . ActualHeight != SPFullDataContainerGrid . ActualHeight )
                {
                    MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, ShowGridFullheight, "Show Tables Panel at Full Height" );
                }

                if ( SprocCreationGrid . Visibility == Visibility . Visible )
                {
                    MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, SaveCloseSpEditorMain, "Save new S. Procedure script and close editor" );
                    MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, DiscardCloseSpEditorMain, "DISCARD current work in new S. P Editor" );
                    MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, processnewspscript, "EXECUTE new S.Procedure script" );
                    MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, mainprintscript, "Print new S.P script" );
                }
                else
                {
                    //MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, mainprintscript, "Print currently selected S.P script" );
                }
                CurrentMenuitem = ShowSpmenu;
            }
            else
            {
                //Sprocs is  visible
                ShowDg = false;
                ShowSp = true;
                //SProcs panel is visible
                MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, UpdateRecord );
                //This gets overidden by Creation version if present
                MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, mainprintscript, "Print currently selected S.P script" );

                if ( SprocCreationGrid . Visibility == Visibility . Visible )
                {
                    Addmainmenu ( this . MainWinmenu, this . optsmenu, SaveCloseSpEditorMain, "Save new S. Procedure script and close editor" );
                    Addmainmenu ( this . MainWinmenu, this . optsmenu, DiscardCloseSpEditorMain, "DISCARD current work in new S. P Editor" );
                    Addmainmenu ( this . MainWinmenu, this . optsmenu, mainprintscript, "Print content of new S.P script" );
                }
                //else
                //{
                //    //                    MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, DiscardCloseSpEditorMain, "Show S.Procedure Editor" );
                //}
                //CurrentMenuitem = ShowDgmenu;
            }

            if ( ShowDg )
            {
            }
            else if ( ShowSp )
            {
            }
        }

        private void HelpMenuOpening ( object sender, RoutedEventArgs e )
        {
            MenuItem mi = sender as MenuItem;
            if ( SPDatagrid . Visibility == Visibility . Visible )
            {
                ShowDg = true;
                ShowSp = false;
            }
            else
            {
                ShowDg = false;
                ShowSp = true;
            }
            if ( ShowDg )
            {
                MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, ShowSPSchema );
                MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, ShowSPSchema, "Show SQL Table Schema" );

                // selective options
                if ( SPDatagrid . ActualHeight != SPFullDataContainerGrid . ActualHeight )
                {
                    MainWinmenu = Addmainmenu ( this . MainWinmenu, this . optsmenu, ShowGridFullheight, "Show Tables Panel at Full Height" );
                }
                CurrentMenuitem = ShowSpmenu;
            }
            else if ( ShowSp )
            {
                MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, ShowGridFullheight );
                MainWinmenu = Removemainmenu ( this . MainWinmenu, this . optsmenu, UpdateRecord );
                CurrentMenuitem = ShowDgmenu;
            }
        }

        #endregion Main menu handling methods

        private void Overall_Click ( object sender, RoutedEventArgs e )
        {
            InfoWindow infowin = null;
            WindowCollection v = Application.Current.Windows;
            foreach ( Window item in v )
            {
                if ( item . ToString ( ) . Contains ( "InfoWin" ) )
                {
                    item . BringIntoView ( );
                    item . Focus ( );
                    return;
                }
            }

            infowin = new InfoWindow ( 1 );
            SetValue ( TopmostProperty, false );
            infowin . Show ( );
        }

        private void UsingSQLPanel ( object sender, RoutedEventArgs e )
        {
            InfoWindow infowin = null;
            WindowCollection v = Application.Current.Windows;
            foreach ( Window item in v )
            {
                if ( item . ToString ( ) . Contains ( "InfoWin" ) )
                {
                    item . BringIntoView ( );
                    item . Focus ( );
                    return;
                }
            }

            infowin = new InfoWindow ( 2 );
            SetValue ( TopmostProperty, false );
            infowin . Show ( );
        }

        private void UsingSprocsPanel ( object sender, RoutedEventArgs e )
        {
            InfoWindow infowin = null;
            WindowCollection v = Application.Current.Windows;
            foreach ( Window item in v )
            {
                if ( item . ToString ( ) . Contains ( "InfoWin" ) )
                {
                    item . BringIntoView ( );
                    item . Focus ( );
                    return;
                }
            }

            infowin = new InfoWindow ( 3 );
            SetValue ( TopmostProperty, false );
            infowin . Show ( );
        }


        private void SPDatagrid_PreviewKeyDown ( object sender, KeyEventArgs e )
        {
            if ( e . Key == Key . Up )
                SPDatagrid . SelectedIndex -= 1;
            else if ( e . Key == Key . Down )
                SPDatagrid . SelectedIndex += 1;
        }

        private void ReshowEditpane ( object sender, RoutedEventArgs e )
        {
            // reset flag so resize will work
            // Show edit panel full height in window
            MenuItem mi = sender as MenuItem;
            if ( mi == null ) return;
            if ( mi . Name . Contains ( "ShowSpmenu" ) || mi . Name . Contains ( "ShowDgmenu" ) )
            {
                // direct access to switch between SQL and Sprocs windows
                if ( mi . Name == "ShowDgmenu" )
                {
                    // close all S.Procs controls
                    ShowDg = true;
                    ShowSp = false;
                    SProcsListbox . Visibility = Visibility . Collapsed;
                    ExecList . Visibility = Visibility . Collapsed;
                    TextResult . Visibility = Visibility . Collapsed;
                    SPDatagrid . Visibility = Visibility . Visible;
                    EditPanel . Visibility = Visibility . Visible;
                    SPDatagrid . UpdateLayout ( );
                    EditPanel . UpdateLayout ( );
                    ResetOptionsAccessColors ( );
                }
                else if ( mi . Name == "ShowSpmenu" )
                {
                    // Close alll DataGrid controls
                    ShowDg = false;
                    ShowSp = true;
                    SProcsListbox . Visibility = Visibility . Visible;
                    ExecList . Visibility = Visibility . Visible;
                    TextResult . Visibility = Visibility . Visible;
                    SPDatagrid . Visibility = Visibility . Collapsed;
                    EditPanel . Visibility = Visibility . Collapsed;
                    SProcsListbox . UpdateLayout ( );
                    ExecList . UpdateLayout ( );
                    TextResult . UpdateLayout ( );
                    ResetOptionsAccessColors ( );
                }
                SPFullDataContainerGrid . UpdateLayout ( );
            }
            else
            {
                if ( shrink1 . Text . Contains ( "Hide" ) )
                {
                    IsGridFullHeight = true;
                    ExpandSqlDataGrid ( sender, null );
                }
                else
                {
                    IsGridFullHeight = false;
                    EditPanel . Visibility = Visibility . Visible;
                    ResetPanelSplit ( sender, e );
                }
                this . Refresh ( );
            }
        }
        private void SPDatagrid_GotFocus ( object sender, RoutedEventArgs e )
        {
            SetButtonColors ( false );
            this . Focus ( );
        }
        private void ClearEditFocus ( )
        {
            //            Data1 . SelectedText = false;
        }
        private void SetButtonColors ( bool direction )
        {
            if ( direction == true )
            {
                SaveBtn . Style = FindResource ( "DiagonalRedButton" ) as Style;
                SaveBtn . BorderBrush = FindResource ( "Yellow0" ) as SolidColorBrush;
                Thickness th = new();
                th . Top = 10; th . Left = 4; th . Right = 4; th . Bottom = 10;
                SaveBtn . BorderThickness = th;
            }
            else
            {
                SaveBtn . Style = FindResource ( "DiagonalCyanButton" ) as Style;
                SaveBtn . BorderBrush = FindResource ( "White0" ) as SolidColorBrush;
                Thickness th = new();
                th . Top = 1; th . Left = 1; th . Right = 1; th . Bottom = 1;
                SaveBtn . BorderThickness = th;
            }
        }
        private void ResetHideBtnText ( int mode )
        {
            "" . sprocstrace ( 0 );
            if ( mode == 0 )
            {
                shrink1 . Text = " Hide Edit";
                shrink2 . Text = "   Panel ";
            }
            else if ( mode == 1 )
            {
                shrink1 . Text = "  Show Full ";
                shrink2 . Text = " Edit Panel ";
            }
            else if ( mode == 2 )
            {
                shrink1 . Text = "Reset Edit";
                shrink2 . Text = "   Panel ";
            }
            "" . sprocstrace ( 1 );
        }
        public List<string> GetSPCollectionData ( string sproc )
        {
            dynamic dynresult = null;
            dynamic dyn1 = new SingleColumnData();
            List<string > scd = new ( );
            SingleColumnData scoldata = new();
            string ConString = Flags . CurrentConnectionString;
            if ( ConString == "" )
            {
                DapperSupport . CheckDbDomain ( "IAN1" );
                ConString = Flags . CurrentConnectionString;
            }
            using ( IDbConnection db = new SqlConnection ( ConString ) )
            {
                try
                {
                    //SingleColumnData scd = new();
                    dynamic sdw = null;
                    //if ( SqlCommand == "" )
                    //{
                    IEnumerable<dynamic> query = db . Query<dynamic> ( sproc,
                            commandType: CommandType . StoredProcedure );
                    //                            . ToList ( );// as List<SingleColumnData>;
                    if ( dyn1 == null )
                        dynresult = db . Query ( sproc, param: null, transaction: null, buffered: true, commandType: CommandType . StoredProcedure );
                    //} 
                    string keyname="";
                    object keyvalue = null;
                    foreach ( var rows in query )
                    {
                        var fields = rows as IDictionary<string, object>;
                        if ( keyname == "" )
                        {
                            foreach ( KeyValuePair<string, object> pair in fields )
                            {
                                keyname = pair . Key . ToString ( );
                                break;
                            }
                        }
                        var sum = fields[keyname];
                        scd . Add ( sum . ToString ( ) );
                    }
                }
                catch ( Exception ex )
                {
                    Debug . WriteLine ( $"SQL DAPPER error : {ex . Message}, {ex . Data}" );
                }
            }
            return scd;
        }

        private void ResultsEscapeKeyDown ( object sender, KeyEventArgs e )
        {
            // Close All of the execution results panel - if open
            //if ( ResultsListBox . Visibility != Visibility . Visible )
            //    return;
            if ( e . Key == Key . Escape )
            {
                ResultsContainerListbox . Visibility = Visibility . Collapsed;
                ResultsListBox . Visibility = Visibility . Collapsed;
                ResultsContainerTextblock . Visibility = Visibility . Collapsed;
                ResultsTextbox . Visibility = Visibility . Collapsed;
                ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
                ResultsDatagrid . Visibility = Visibility . Collapsed;
                TextResult . Visibility = Visibility . Visible;
            }
        }

        private void HideResultsPanel ( object sender, RoutedEventArgs e )
        {
            ResultsContainerListbox . Visibility = Visibility . Collapsed;
            ResultsListBox . Visibility = Visibility . Collapsed;
            if ( ShowSp )
                TextResult . Visibility = Visibility . Visible;
            else if ( ShowDg )
                EditPanel . Visibility = Visibility . Visible;
            else if ( ShowSc )
                SprocCreationGrid . Visibility = Visibility . Visible;
        }

        private void ColorsCombo_DropDownOpened ( object sender, EventArgs e )
        {
            Mouse . OverrideCursor = Cursors . Wait;
            Thread . Sleep ( 250 );
            Mouse . OverrideCursor = Cursors . Arrow;
            return;
        }

        private void CloseApp_Click ( object sender, RoutedEventArgs e )
        {
            if ( bdirty )
            {
                MessageBoxResult mbr =  MessageBox . Show ( "You have unsaved changes to the current table !  Are you sure you want to discard these changes ?","Possible Data Loss", MessageBoxButton.YesNoCancel);
                if ( mbr == MessageBoxResult . Cancel )
                    return;
                else if ( mbr == MessageBoxResult . No )
                    return;
                else if ( mbr == MessageBoxResult . Yes )
                    Application . Current . Shutdown ( );
            }
            else
            {
                MessageBoxResult mbr =  MessageBox . Show ( "Are you quite sure you want to close this \napplication down completely ?","Confirm Close Down", MessageBoxButton.YesNo);
                if ( mbr == MessageBoxResult . No )
                    return;
                Application . Current . Shutdown ( );
            }
        }


        private void SPEditorKeyDown ( object sender, KeyEventArgs e )
        {
            if ( e . Key == Key . Escape )
            {
                if ( CreateSprocTextbox . Text != NewSprocText )
                {
                    MessageBox . Show ( "Save the changes made to the S.Procedure ??" );
                }
                SprocCreationGrid . Visibility = Visibility . Collapsed;
                ResultsContainerTextblock . Visibility = Visibility . Visible;
                ResultsTextbox . Visibility = Visibility . Visible;

            }
        }

        private void Execnewscript ( object sender, RoutedEventArgs e )
        {
            ExecResult . Text = "processing new SP Script ......";
        }

        private void Closenewscript ( object sender, RoutedEventArgs e )
        {
            SprocCreationGrid . Visibility = Visibility . Collapsed;
            ResultsContainerTextblock . Visibility = Visibility . Visible;
            ResultsTextbox . Visibility = Visibility . Visible;

        }

        public void SetLowerPanelHeight ( )
        {
            // set Dependency property for lower grid panel height
            //Debug . WriteLine ( $"Pre reset = {LowerPanelHeight}" );
            //if ( LowerPanelHeight == 0 )
            //    LowerPanelHeight = SPFullDataContainerGrid . ActualHeight - ( RowHeight0] + hsplitterrow . ActualHeight );
            //else
            //    LowerPanelHeight = SPFullDataContainerGrid . ActualHeight - ( RowHeight 2 ] + hsplitterrow . ActualHeight );
            //LowerPanelHeight = SPFullDataContainerGrid . ActualHeight - SProcsListbox . ActualHeight;
            ////           LowerPanelHeight += 25;
            //Debug . WriteLine ( $"Post reset = {LowerPanelHeight}" );
            //Debug . WriteLine ( $"{SPFullDataContainerGrid . ActualHeight} : = {RowHeight 0 ]} : {hsplitterrow . ActualHeight} : {RowHeight 2 ]} " );
            //    //$"{uppergridrow . ActualHeight + hsplitterrow . ActualHeight + RowHeight 0 ]}" );
            //lowergridrow . Height = new GridLength ( LowerPanelHeight );
        }
        private void ResetPanelControlSizes ( )
        {
            return;
            //"" . sprocstrace ( 0 );
            //if ( ShowSp )
            //{
            //    SprocCreationGrid . Height = RowHeight 2 ];
            //    ResultsContainerDatagrid . Height = RowHeight 2 ];
            //    TextResult . Height = RowHeight 2 ] - 20;
            //    // results controls
            //    //ResultsDatagrid . Height = RowHeight 2 ];
            //    //ResultsContainerListbox . Height = RowHeight 2 ];
            //    //ResultsContainerTextblock . Height = RowHeight 2 ];
            //}
            //else
            //{
            //    EditPanel . Height = RowHeight 2 ];

            //}
            //"" . sprocstrace ( 1 );
        }

        private void ListAllControlHeights ( )
        {
            string output =LowerDataArea.ToString().PadRight(25) + $" : {LowerDataArea}";
            output += UpperDataArea . ToString ( ) . PadRight ( 25 ) + $" : {UpperDataArea}";
            output += EditPanel . ToString ( ) . PadRight ( 25 ) + $" : {EditPanel . Height}";
            output += SProcsListbox . ToString ( ) . PadRight ( 25 ) + $" : {SProcsListbox . ActualHeight}";
            output += SPDatagrid . ToString ( ) . PadRight ( 25 ) + $" : {SPDatagrid . Height}";
            output += TextResult . ToString ( ) . PadRight ( 25 ) + $" : {TextResult . Height}";
            output += $"";
            Debug . WriteLine ( output );
        }

        private void ShowpDg_MouseEnter ( object sender, MouseEventArgs e )
        {
            // ShowSp = true;
        }

        private void ShowpSp_MouseEnter ( object sender, MouseEventArgs e )
        {
            //ShowDg = true;
        }

        private void ShowSp_MouseLeave ( object sender, MouseEventArgs e )
        {
            //ShowSp = false;
        }

        private void ShowDg_MouseLeave ( object sender, MouseEventArgs e )
        {
            //ShowDg = false;
        }

        #region New Script handling
        private void ShowSprocCreationGrid ( object sender, RoutedEventArgs e )
        {
            // Show script creation panel
            // set global flag so we know editor is open
            ShowSc = true;
            SprocCreationGrid . Visibility = Visibility . Visible;
            CreateSprocTextbox . Visibility = Visibility . Visible;
            NewSprocText = CreateNewSprocEditorTemplate ( );
            CreateSprocTextbox . Text = NewSprocText;
            CreateSprocTextbox . UpdateLayout ( );
            ShowDg = false;
            ShowSp = true;
        }

        private void HideSprocCreationGrid ( object sender, RoutedEventArgs e )
        {
            MessageBoxResult  mbr = MessageBox . Show ( $"Any changes yo u have made to the new Script will be lost if you continue.  \nAre you REALLY SURE you want to discard it ?" ,"Discarding new Stored procedure script?",MessageBoxButton.YesNo,MessageBoxImage.Question);
            if ( mbr == MessageBoxResult . No )
                return;
            SprocCreationGrid . Visibility = Visibility . Collapsed;
            CreateSprocTextbox . Visibility = Visibility . Collapsed;
            if ( ShowDg )
                EditPanel . Visibility = Visibility . Visible;
            else if ( ShowSp )
                TextResult . Visibility = Visibility . Visible;
            ShowSc = false;
        }
        private void LoadColorsComboFromFile ( )
        {
            // All  configured correctly (Background/Foreground matching completed)
            // #79 is Red, the default background item.
            // load stored color data for my colored background combo
            string[] names = File . ReadAllLines ( @"C:\wpfmain\combos-names.txt" );
            string[] fcolors = File . ReadAllLines ( @"C:\wpfmain\combos-foreground.txt" );
            string[] bcolors = File . ReadAllLines ( @"C:\wpfmain\combos-colors.txt" );
            int index =0;
            foreach ( string name in names )
            {
                comboclrs cc = new();
                cc . name = names [ index ];
                cc . Fground = ( SolidColorBrush ) new BrushConverter ( ) . ConvertFrom ( fcolors [ index ] );
                cc . Bground = ( SolidColorBrush ) new BrushConverter ( ) . ConvertFrom ( bcolors [ index ] );
                CbColorsList . Add ( cc );
                index++;
            }
            ColorsCombo . ItemsSource = CbColorsList;
            ColorsCombo . SelectedIndex = 79;
            ColorsCombo . SelectedItem = 79;
        }
        private void SaveNewSprocs ( object sender, RoutedEventArgs e )
        {
            // Save text from new S.Procedure script and close panel
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog . FileName = "newuserscript.txt"; // Default file name
            dialog . DefaultExt = ".txt"; // Default file extension
            dialog . Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if ( result == true )
            {
                // Open document
                string filename = dialog.FileName;
                if ( filename != null )
                {
                    File . WriteAllText ( filename, CreateSprocTextbox . Text );
                    MessageBox . Show ( $"New Script saved as [ newuserscript . txt ]", "Script Saved" );
                }
            }
        }

        private void ExecuteNewSprocs ( object sender, RoutedEventArgs e )
        {
            // try to Execuute the  new SP script
            MessageBoxResult  mbr = MessageBox . Show ( $"Your new Script will be executed using the \n[{ExecList.SelectedItem.ToString().ToUpper()} method\nwhich is the currently selected Execution Method.\n\nPlease confirm that you want to proceeed ?\n\nNB - Please make SURE you understand that any changes it may make to your SQL data system, as they may NOT necessarily be REVERSIBLE !" ,"Executing user's Stored procedure ?",MessageBoxButton.YesNo,MessageBoxImage.Question);
            if ( mbr == MessageBoxResult . No )
                return;
            // TODO Add code to run the script and show resullts.

        }
        private string CreateNewSprocEditorTemplate ( )
        {
            string Output="";
            // Create template for S.Proc
            Output = $"USE [IAN1]\nGO\nSET ANSI_NULLS ON\nGO\nSET QUOTED_IDENTIFIER ON\nGO\n\nALTER PROCEDURE [dbo].[spCreateGenericDb]\n@Arg1 NVARCHAR(200)\t='GenericTable'\n" +
                $"AS\nBEGIN\r\n\tSET NOCOUNT ON;\n\n";
            NewSprocText = Output;
            return Output;
        }

        private void ShowSPCreationTextbox_Click ( object sender, RoutedEventArgs e )
        {
            CreateSprocTextbox . Text = NewSprocText;
            SprocCreationGrid . Visibility = Visibility . Visible;
            ResultsContainerTextblock . Visibility = Visibility . Collapsed;
            ResultsTextbox . Visibility = Visibility . Collapsed;
        }

        #endregion New Script handling


        #region Resizing support methods
        private void show_Editpanel ( object sender, RoutedEventArgs e )
        {
            if ( EditPanel . Visibility == Visibility . Collapsed )
            {
                EditPanel . Visibility = Visibility . Visible;
                TextResult . Visibility = Visibility . Collapsed;
                PreEditsplitterheight1 = SPDatagrid . ActualHeight;
                PreEditsplitterheight2 = TextResult . ActualHeight;
                // reset splitter to suitable position so all fields are shown
                LoadSqlEditData ( null, null );
            }
            else
            {
                EditPanel . Visibility = Visibility . Collapsed;
                TextResult . Visibility = Visibility . Visible;
                if ( AllowSplitterReset && ShowFullGrid == false )
                {
                    // reset splitter to original position if flag is true
                    if ( PreEditsplitterheight1 > 0 )
                        SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( PreEditsplitterheight1, GridUnitType . Pixel );
                    else
                        PreEditsplitterheight1 = SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight;
                }
                else if ( ShowFullGrid )
                {
                    GridLength gl2 = new();
                    double fullheight = SPFullDataContainerGrid.ActualHeight;
                    gl2 = new GridLength ( fullheight - 25, GridUnitType . Pixel );
                    SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = gl2;
                    PreEditsplitterheight1 = fullheight;
                }
                Clearfields ( );
            }
            Debug . WriteLine ( $"show_Editpaneln\nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
        }

        private void Win_SizeChanged ( object sender, SizeChangedEventArgs e )
        {
            // Working 4/2/23
            if ( IsLoading )
                return;
            double diff = 0, fullheight =0;
            "" . Track ( 0 );
            fullheight = SPFullDataContainerGrid . Height;
            //            RowHeight4 = fullheight - 55;
            diff = e . NewSize . Height - e . PreviousSize . Height;
            if ( diff == 0 )
                return;
            // Set new total working area
            RowHeight4 += diff;
            RowHeight0 += diff;
            if ( RowHeight0 < 0 )
                RowHeight0 = 0;
            // RowHeight2 should NOT change
            //This is copied from DragCompleted, and WORKS CORRECTLY for row 2
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height =
                new GridLength ( ( SPFullDataContainerGrid . ActualHeight - 100 ) - ( SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight + 185 ) );

            if ( RowHeight0 - splitht < 0 )
            {
                SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( 0 );
            }
            else
            {
                SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );
            }
            SPFullDataContainerGrid . UpdateLayout ( );
            //if ( ShowSp )
            //{
            //    SProcsListbox . UpdateLayout ( );
            //    ExecList . UpdateLayout ( );
            //    TextResult . UpdateLayout ( );
            //}
            //else
            //{
            //    SPDatagrid . UpdateLayout ( );
            //}
            SPFullDataContainerGrid . UpdateLayout ( );
            // This is needed if the splitter gets "pushed down" by the size reduction, it corrects the lower panel  height change
            Splitter_DragCompleted ( sender, null );
            Debug . WriteLine ( $"\nWin_SizeChanged \nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
        }

        private void TriggerFullHeight ( object sender, RoutedEventArgs e )
        {
            // large window
            double screenheight = SystemParameters.VirtualScreenHeight;
            this . Top = 0;
            if ( this . Height < screenheight )
                this . Height = screenheight;
            else
            {
                if ( WinSize == "SMALL" )
                    Showsmallwin ( null, null );
                if ( WinSize == "MED" )
                    Showmediumwin ( null, null );
                if ( WinSize == "LARGE" )
                    Showlargewin ( null, null );
            }
            this . UpdateLayout ( );
        }

        private void Showlargewin ( object sender, RoutedEventArgs e )
        {
            // large window
            this . Top = 50;
            this . Left = 270;
            if ( this . Height <= 970 || this . Width <= 1350 )
            {
                WinSize = "LARGE";
                this . Height = 1000;
                this . Width = 1350;
                EditPanel . UpdateLayout ( );
                this . UpdateLayout ( );
            }
            double screenheight = SystemParameters.VirtualScreenHeight;
        }

        private void Showmediumwin ( object sender, RoutedEventArgs e )
        {
            // medium window
            WinSize = "MED";
            this . Top = 100;
            this . Left = 415;
            this . Height = 900;
            this . Width = 1000;
            EditPanel . UpdateLayout ( );
            this . UpdateLayout ( );
        }

        private void Showsmallwin ( object sender, RoutedEventArgs e )
        {
            // small window
            this . Top = 100;
            this . Left = 450;
            if ( this . Height >= 760 || this . Width >= 750 )
            {
                WinSize = "SMALL";
                this . Height = 760;
                this . Width = 900;
                EditPanel . UpdateLayout ( );
                this . UpdateLayout ( );
            }
        }

        private void ExpandGridHeight_Click ( object sender, RoutedEventArgs e )
        {
            // All working 5/2/23
            // Set sql table datagrid to full height of window
            "" . sprocstrace ( 0 );
            // Set Binding of top row  to full height - Splitter height
            RowHeight0 = RowHeight4 - ( splitht + 65 );
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( 20 );
            SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight4 - ( splitht + 65 ) );
            SPFullDataContainerGrid . UpdateLayout ( );
            SPDatagrid . UpdateLayout ( );
            EditPanel . UpdateLayout ( );
            "" . sprocstrace ( 1 );
        }

        private void ExpandSqlDataGrid ( object sender, RoutedEventArgs e )
        {
            // WORKING 24/1/23
            ExpandGridHeight_Click ( sender, null );
            IsGridFullHeight = true;
            IsEditFullHeight = false;
            ResetHideBtnText ( 1 );
        }

        private void ResetPanelSplit ( object sender, RoutedEventArgs e )
        {
            "" . sprocstrace ( 0 );
            if ( RowHeight0 == 0 )
            {
                var v1 =  SPFullDataContainerGrid.RowDefinitions [ 0 ] . ActualHeight ;
            }
            RowHeight4 = SPFullDataContainerGrid . Height;
            UsablePanelsHeight = SPFullDataContainerGrid . Height - SpUnusedSpace;
            if ( ShowDg && EditPanel . Visibility == Visibility . Visible )
            {
                double topval = (RowHeight4 - (SpUnusedSpace+ DefEditpanelHeight));
                double btmval = DefEditpanelHeight;
                RowHeight0 = topval;
                RowHeight2 = btmval;
                SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
                SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );
                //SPDatagrid . Height += 20;
                SPFullDataContainerGrid . UpdateLayout ( );
                SPDatagrid . UpdateLayout ( );
                EditPanel . UpdateLayout ( );
            }
            else
            {
                // set both panels to 50% height
                double splitval = (RowHeight4 - SpUnusedSpace)/ 4;
                double topval =splitval * 2;
                double btmval = splitval * 2;
                RowHeight0 = topval;
                RowHeight2 = btmval;
                SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );
                SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
                SPFullDataContainerGrid . UpdateLayout ( );
                TextResult . Height = btmval - 55;
                UpdateLayout ( );
            }

            if ( ShowDg )
                ResetHideBtnText ( 0 );
            IsEditFullHeight = true;
            "" . sprocstrace ( 1 );
            Debug . WriteLine ( $"\nResetPanelSplit \nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
            return;
        }

        private void Hidepane_click ( object sender, RoutedEventArgs e )
        {
            ExpandSqlDataGrid ( sender, null );
            if ( ShowDg )
            {
                // Hide reopen panel button
                ShowEditpanel . Visibility = Visibility . Visible;
                IsGridFullHeight = true;
            }
        }
        private void ResetScriptPanelSplit ( object sender, RoutedEventArgs e )
        {
            // reset TextResulr position in window
            "" . sprocstrace ( 0 );
            if ( RowHeight0 == 0 )
            {
                var v1 =  SPFullDataContainerGrid.RowDefinitions [ 0 ] . ActualHeight ;
            }
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( DefEditpanelHeight + RowHeight1 );// + RowHeight1 );
            RowHeight2 = SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height . Value;
            RowHeight0 = RowHeight4 - ( RowHeight1 + RowHeight2 );
            SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );// - ( DefEditpanelHeight ) );
            SPFullDataContainerGrid . UpdateLayout ( );
            EditPanel . UpdateLayout ( );
            SPDatagrid . UpdateLayout ( );

            SetSplitterToMidMidposition ( );
            SPFullDataContainerGrid . UpdateLayout ( );

            IsEditFullHeight = true;
            "" . sprocstrace ( 1 );
            Debug . WriteLine ( $"\nResetScriptPanelSplit \nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
            return;
        }

        private void SetSplitterToMidMidposition ( )
        {
            // called  by SP Creation panel refit option
            double percentages = 0.0;
            percentages = RowHeight4 / 5.0;
            RowHeight0 = ( percentages * 2 ) + RowHeight1;
            RowHeight2 = ( percentages * 3 - ( RowHeight1 + 90 ) );
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
            SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );

            UpdateAllLowerControlSizes ( );
            RowHeight0 = SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height . Value;
            RowHeight2 = SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height . Value;
            Debug . WriteLine ( $"\nSetSplitterToMidMidposition\nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
        }
        private void UpdateAllLowerControlSizes ( )
        {
            TextResult . Height = RowHeight2;
            if ( CreateSprocTextbox . Visibility == Visibility . Visible )
            {
                // lower pane  has Script editor  visible
                SprocCreationGrid . Height = RowHeight2;// - 10;
                CreateSprocTextbox . Height = RowHeight2; // - 10;
            }
            ResultsContainerDatagrid . Height = RowHeight2;
            ResultsDatagrid . Height = RowHeight2;
            ResultsContainerListbox . Height = RowHeight2;
            ResultsContainerTextblock . Height = RowHeight2;
            Debug . WriteLine ( $"\nUpdateAllLowerControlSizes ??? \nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
        }

        #endregion Resizing support methods

        private void SaveResultsToFile ( object sender, RoutedEventArgs e )
        {
            // Save  the results from the ResultsListBox to a user selected file
            string output = "";
            int indx = 0;
            foreach ( string str in ResultsListBox . Items )
            {
                if ( indx != 1
                    && str . Contains ( "Click ESCAPE" ) == false
                    && str != "" )
                {

                    output += $"{str}\n";
                }
                indx++;
            }
            output += $"\nEND OF LISTBOX RESULTS from execution of the Stored Procedure \nshown above (Courtesy of the WpfMain Generic SQL/S.P Access Application\n";
            string path = @"UserDataFiles\ListboxResults.Txt";
            if ( File . Exists ( path ) )
            {
                MessageBoxResult mbr = MessageBox . Show ( $"There is already a file with the name named 'LISTBOXRESULTS.TXT' in the data Folder.  Do you want to Overwrite it ?.\n\nIf you proceed the filename will have a suitable numeric value appended to it's (root) name eg: LISTBOXRESULTS1.TXT", "Save Data",
                    MessageBoxButton . YesNo, MessageBoxImage . Warning );
                if ( mbr == MessageBoxResult . No )
                {
                    return;
                }
                else
                {
                    if ( File . Exists ( path ) == false )
                        File . WriteAllText ( path, output );
                    else
                    {
                        // get filename without ".txt"
                        while ( true )
                        {
                            string root =  path.Substring(0, path.Length - 4);
                            string revstr =new string ( root. Reverse ( ) . ToArray ( ) );
                            string lastval = path.Substring(path.Length - 5,1);
                            char ch =Convert.ToChar(lastval);
                            if ( Char . IsDigit ( ch ) == false )
                            {
                                root = path . Substring ( 0, ( path . Length - 5 ) );
                                root += "1.txt";
                                path = root;
                                if ( File . Exists ( path ) )
                                    continue;                       // go around again
                            }
                            else
                            {
                                path = AddNumericToFilename ( path );
                                if ( File . Exists ( path ) )
                                    continue;
                                else
                                    break;
                            }
                        }
                        File . WriteAllText ( path, output );
                        mbr = MessageBox . Show ( $"Data has been saved successfully to a file with the name\n {path}\n\nDo you want to close  the Script Editor now ?.", "Data Saved", MessageBoxButton . YesNo, MessageBoxImage . Information );
                        if ( mbr == MessageBoxResult . Yes )
                        {
                            ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
                            ResultsContainerListbox . Visibility = Visibility . Collapsed;
                            TextResult . Visibility = Visibility . Visible;
                        }
                    }
                }
            }
            else
            {
                MessageBoxResult mbr = MessageBox . Show ( $"Do you want to close  the Script Editor now ?.", "Data Ignored", MessageBoxButton . YesNo, MessageBoxImage . Information );
                if ( mbr == MessageBoxResult . Yes )
                {
                    ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
                    ResultsContainerListbox . Visibility = Visibility . Collapsed;
                    TextResult . Visibility = Visibility . Visible;
                }

            }
        }
        public static string AddNumericToFilename ( string path )
        {
            // get stub without file .suffix
            string pathroot = path . Substring ( 0, ( path . Length - 4 ) );
            Debug . WriteLine ( $"AddNumericToFilename - original = {pathroot}" );
            string reversedpathroot = new string ( pathroot . Reverse ( ) . ToArray ( ) );
            int index = 0;
            string newpath="";
            string numerics = "";
            // work backwards thruthe already reversed file name looking for digits
            // check for numerics first
            while ( true )
            {
                if ( Char . IsDigit ( reversedpathroot [ index ] ) == true )
                {
                    Char ch =reversedpathroot[ index ];
                    numerics += ch;
                    index++;
                    continue;
                }
                else if ( numerics == "" )
                    path = path + $"1.txt";
                else
                {
                    //No digits in path rot
                    string root = path . Substring ( 0, ( path . Length - 5 ) );
                    root += $"{index}.txt";
                    path = root;
                    if ( File . Exists ( path ) == false )
                        break;
                    else
                    {
                        index++;
                        continue;
                    }
                }
            }
            if ( File . Exists ( path ) == false )
                return path;

            string numstr="";
            for ( int x = 0 ; x < reversedpathroot . Length ; x++ )
            {
                Char  ch = Convert . ToChar ( reversedpathroot[ x ] );
                if ( Char . IsDigit ( ch ) == false )
                {
                    // no digits in curret name, so Reverse file name back to normal
                    if ( numstr != "" )
                    {
                        string numstr2 =new string ( numstr. Reverse ( ) . ToArray ( ) );
                        int numval = Convert . ToInt32 ( numstr2 );
                        numval++;
                        pathroot = path . Substring ( 0, path . Length - ( 4 + x ) );
                        pathroot += $"{numval}.txt";
                        newpath = pathroot;
                        break;
                    }
                    else
                    {
                        // just add "1" to root of filename
                        pathroot = path . Substring ( 0, path . Length - 4 );
                        pathroot += $"1.txt";
                        newpath = pathroot;
                        break;
                    }
                }
                else
                {
                    numstr += ch;
                    continue;
                }
            }
            Debug . WriteLine ( $"AddNumericToFilename - new path = {newpath}" );
            return newpath;
        }

        private void CloseResultsPanel ( object sender, RoutedEventArgs e )
        {
            /// Close ALL Results Container & Content grids
            ShowRt = false;
            ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
            ResultsDatagrid . Visibility = Visibility . Collapsed;
            ResultsContainerListbox . Visibility = Visibility . Collapsed;
            ResultsListBox . Visibility = Visibility . Collapsed;
            ResultsContainerTextblock . Visibility = Visibility . Collapsed;
            ResultsTextbox . Visibility = Visibility . Collapsed;
            if ( ShowSp )
                TextResult . Visibility = Visibility . Visible;
            else if ( ShowDg )
                EditPanel . Visibility = Visibility . Visible;

            if ( ShowSc )
            {
                SprocCreationGrid . Visibility = Visibility . Visible;
                CreateSprocTextbox . Visibility = Visibility . Visible;
            }
        }
        public ObservableCollection<GenericClass> ParseDapperToGenericClass ( IEnumerable<dynamic> reslt, Dictionary<string, object> dict )
        {
            //Although this is duplicated  with the one above we CANNOT make it a method()
            ObservableCollection<GenericClass> objG = new ObservableCollection<GenericClass> ( );
            int dictcount = 0;
            int fldcount = 0;
            int colcount=0;
            string result="";
            bool IsSuccess = false;
            long zero= reslt.LongCount ();
            try
            {
                foreach ( var item in reslt )
                {
                    // trial to get a new  instance of an anonymous object    - fails	 for bankaccountciewmodel
                    //var  gcx = SqlServerCommands .deepClone( objtype);
                    GenericClass gc = new GenericClass();
                    try
                    {
                        //	Create a dictionary entry for each row of data then add it as a row to the Generic (ObervableCollection<xxxxxx>) Class
                        gc = ParseDapperRowGen ( item, dict, out colcount );
                        dictcount = 1;
                        fldcount = dict . Count;
                        //if ( fldcount == 0 )
                        //{
                        //	//TODO - Oooops, maybe, we will use a Datatable or osething
                        //	//return null;
                        //}
                        foreach ( var pair in dict )
                        {
                            try
                            {
                                if ( pair . Key != null && pair . Value != null )
                                {
                                    DapperSupport . AddDictPairToGeneric<GenericClass> ( gc, pair, dictcount++ );
                                }

                            }
                            catch ( Exception ex )
                            {
                                Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
                                result = ex . Message;
                            }
                        }
                        IsSuccess = true;
                    }
                    catch ( Exception ex )
                    {
                        result = $"SQLERROR : {ex . Message}";
                        Debug . WriteLine ( result );
                    }

                    objG . Add ( gc );
                    dict . Clear ( );
                    dictcount = 1;
                }
                return objG;
            }
            catch ( Exception ex ) { return null; }
        }

        //*************************************************************************************//
        //Create a new  Observablecolleciton<GenericClass> from  IEnumerable<dynamic> data
        //*************************************************************************************//
        public ObservableCollection<GenericClass> CreateCollectionFromDynamic ( IEnumerable<dynamic> dyn )
        {
            // Working
            // Create a new  observablecolleciton<GenericClass> from  IEnumerable<dynamic> data
            int index = 0;
            ObservableCollection<GenericClass>  querytable = new();
            foreach ( var rows in dyn )
            {
                index = 1;
                GenericClass gc = new();
                var fields = rows as IDictionary<string, object>;
                foreach ( KeyValuePair<string, object> item in fields )
                {
                    if ( item . Value == null )
                    {
                        index++;
                        break;
                    }
                    string data  = item . Value.ToString();
                    switch ( index )
                    {
                        case 1:
                            gc . field1 = data; break;
                        case 2:
                            gc . field2 = data; break;
                        case 3:
                            gc . field3 = data; break;
                        case 4:
                            gc . field4 = data; break;
                        case 5:
                            gc . field5 = data; break;
                        case 6:
                            gc . field6 = data; break;
                        case 7:
                            gc . field7 = data; break;
                        case 8:
                            gc . field8 = data; break;
                        case 9:
                            gc . field9 = data; break;
                        case 10:
                            gc . field10 = data; break;
                        case 11:
                            gc . field11 = data; break;
                        case 12:
                            gc . field12 = data; break;
                        case 13:
                            gc . field13 = data; break;
                        case 14:
                            gc . field14 = data; break;
                        case 15:
                            gc . field15 = data; break;
                        case 16:
                            gc . field16 = data; break;
                        case 17:
                            gc . field17 = data; break;
                        case 18:
                            gc . field18 = data; break;
                        case 19:
                            gc . field19 = data; break;
                        case 20:
                            gc . field20 = data; break;
                        default:
                            break;
                    }
                    index++;
                }
                querytable . Add ( gc );
            }
            GenericClass gc2 = new();
            gc2 . field1 = "Dbl-Click this row, or Hit ESCAPE";
            querytable . Add ( gc2 );
            gc2 = new ( );
            gc2 . field1 = "to close the results datagrid display";
            querytable . Add ( gc2 );
            return querytable;
        }

        public List<string> CreateListFromDynamic ( IEnumerable<dynamic> dyn )
        {
            // Working
            // Create a new  observablecolleciton<GenericClass> from  IEnumerable<dynamic> data
            List<string>  resultslist = new();
            string currentline="";
            int dupecount=0;
            int total=0;
            foreach ( var entry in dyn )
            {
                if ( currentline != "" && entry == currentline )
                {
                    dupecount++;
                }
                currentline = entry . ToString ( );
                resultslist . Add ( entry );
                total++;
            }
            if ( ++dupecount == total )
            {
                resultslist . Add ( "INFO:" );
                resultslist . Add ( "As all values returned are identical" );
                resultslist . Add ( "a better result may be achiieved by using the" );
                resultslist . Add ( "'SP returning a List<string>' Execution Method or " );
                resultslist . Add ( "'SP returning a table as ObservableCollection' ?" );
            }
            return resultslist;
        }


        private void ResultsDatagrid_PreviewMouseDoubleClick ( object sender, MouseButtonEventArgs e )
        {
            GenericClass gc = new();
            DataGrid dg = sender as DataGrid;
            if ( dg == null ) { return; }
            if ( dg . SelectedItem == null ) { return; }
            gc = dg . SelectedItem as GenericClass;
            string  selection =gc.field1.ToString();
            if ( selection . Contains ( "Dbl-Click this row" )
                || selection . Contains ( "to close the results" ) )
            {
                ResultsDatagrid . Visibility = Visibility . Collapsed;
                ResultsContainerDatagrid . Visibility = Visibility . Collapsed;
                TextResult . Visibility = Visibility . Visible;
            }

        }

        private void ResultsDatagrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            GenericClass gc = new();
            DataGrid dg = sender as DataGrid;
            if ( dg == null ) { return; }
            if ( dg . SelectedItem == null ) { return; }
            gc = dg . SelectedItem as GenericClass;
            string  selection =gc.field1.ToString();
        }

        private void ResultsListBox_SelectionChanged ( object sender, MouseButtonEventArgs e )
        {
            ListBox lb = sender   as ListBox;
            Type type = sender.GetType();
            if ( lb . SelectedItem == null )
                return;
            string selection = lb.SelectedItem . ToString();
            if ( selection . Contains ( "Dbl-Click this line" ) == true )
            {
                ResultsListBox . Items . Clear ( );
                ResultsListBox . Visibility = Visibility . Collapsed;
                ResultsContainerListbox . Visibility = Visibility . Collapsed;
                TextResult . Visibility = Visibility . Visible;
            }

        }

        #region printing support
        private void printscript_click ( object sender, RoutedEventArgs e )
        {

            //Request to print someting
            bool istopmost = this.Topmost;
            this . Topmost = false;
            //default save path for data to be printed
            string savepath = @"C:\PrintDocs\newprintdoc.txt";

            // get calling menu
            MenuItem mitem = sender  as MenuItem;

            if ( mitem . Name == "mainprintscript" )
            {
                // called from main window menu
                ResetFlowDocForprint ( "FLOWDOC", FlowDoc: true );
            }
            else if ( mitem . Name == "printscript" )
            {
                //new  Script printing
                string text = CreateSprocTextbox.Text;
                PrintItem ( documenttext: text );
            }
            if ( istopmost )
            {
                this . Topmost = true;
                //                TextResult . UpdateLayout ( );
            }
        }
        public void ResetFlowDocForprint ( string callertype, bool FlowDoc = false, string filename = "", string TextTobePrinted = "" )
        {
            string sptext="";
            Brush fore=this. ScrollViewerFground;
            Brush back=ScrollViewerBground;

            if ( FlowDoc == true )
            {
                // *** WORKING 12/2/22 *** //
                // We are printing the current Script in viewer
                FlowDocument fd = new();
                FetchStoredProcedureCode ( SProcsListbox . SelectedItem . ToString ( ), ref sptext );
                fd . Blocks . Clear ( );
                // we have to switch color parameters for Flowdoc to Black on Whiite to be able to print it.
                ScrollViewerBground = fd . Background = FindResource ( "White0" ) as SolidColorBrush;
                ScrollViewerFground = fd . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
                fd = processsprocs . CreateBoldString ( fd, sptext, "" );
                PrintItem ( flowDocument: fd );
                // This switches the default flowdoc colors back again
                ScrollViewerBground = back;
                ScrollViewerFground = fore;
                return;
            }
            else if ( TextTobePrinted != "" )
            {
                PrintItem ( documenttext: TextTobePrinted );
                return;
            }
            else if ( filename != "" )
            {
                string currtext = File.ReadAllText(filename);
                PrintItem ( documenttext: currtext );
                return;
            }
        }

        public void PrintItem ( string documenttext = "", string filename = "", FlowDocument flowDocument = null )
        {
            var dialog = new System.Windows.Controls.PrintDialog();
            dialog . PageRangeSelection = System . Windows . Controls . PageRangeSelection . AllPages;
            dialog . UserPageRangeEnabled = true;

            // Show save file dialog box
            Nullable<Boolean> print = dialog.ShowDialog();

            if ( print == true )
            {
                // Pint the file
                if ( flowDocument != null && documenttext == "" && filename == "" )
                {
                    IDocumentPaginatorSource idocument = flowDocument as IDocumentPaginatorSource;
                    dialog . PrintDocument ( idocument . DocumentPaginator, "Printing Flow Document..." );
                    ExecResult . Background = FindResource ( "Green4" ) as SolidColorBrush;
                    ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    ExecResult . Text = "Document has been printed successfully.....";
                }
                else if ( documenttext != "" )
                {
                    Brush fore=this. ScrollViewerFground;
                    Brush back=ScrollViewerBground;
                    ScrollViewerBground = FindResource ( "White0" ) as SolidColorBrush;
                    ScrollViewerFground = FindResource ( "Black0" ) as SolidColorBrush;
                    FlowDocument myFlowDocument=new();
                    myFlowDocument = processsprocs . CreateBoldString ( myFlowDocument, documenttext, "" );

                    IDocumentPaginatorSource idocument = myFlowDocument as IDocumentPaginatorSource;
                    dialog . PrintDocument ( idocument . DocumentPaginator, "Printing New Script..." );
                    ScrollViewerBground = back;
                    ScrollViewerFground = fore;
                    ExecResult . Background = FindResource ( "Green4" ) as SolidColorBrush;
                    ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    ExecResult . Text = "Document has been printed successfully.....";
                }
            }
        }

        private void PrintResultsPanel ( object sender, RoutedEventArgs e )
        {
            // print content of current results panel
            string originalline="";
            ListBox lb = this.ResultsListBox;
            string output = $"Execution Resuts Printout :\n\n";
            //          for(int x = 0 ; x < lb.Items.Count ; x++)
            foreach ( var item in lb . Items )
            {
                originalline = item . ToString ( );
                string line = originalline .ToUpper ();
                if ( line . Contains ( "EXECUTION RESULT FOR" )
                    || line . Contains ( "DBL-CLICK THIS LINE" ) )
                    continue;
                else
                    output += $"{originalline}\n";
            }
            bool istopmost =this . Topmost;
            this . Topmost = false;

            PrintItem ( documenttext: output );

            if ( istopmost )
                this . Topmost = true;
            ExecResult . Background = FindResource ( "Green4" ) as SolidColorBrush;
            ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            ExecResult . Text = "Contents of Results panel have been printed successfully.....";
            Utils2 . DoSuccessBeep ( );
        }
        #endregion printing support

        private void ScriptsMenuOpening ( object sender, RoutedEventArgs e )
        {

        }
    }
}
