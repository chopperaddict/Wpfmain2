using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . IO;
using System . Net;
using System . Transactions;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Media3D;

using ViewModels;

using Canvas = System . Windows . Controls . Canvas;


namespace Wpfmain
{
	#region Generic System wide structures and Definitions
	// All System wide Delegates are declared here...
	#region DELEGATE DECLARATIONS
	//   public delegate void LoadTableDelegate ( string Sqlcommand , string TableType , object bvm );
	//public delegate void LoadTableWithDapperDelegate ( string Sqlcommand , string TableType , object bvm , object Args);
	#endregion DELEGATE DECLARATIONS


	// Required to allow various  items  towork without these classes actually being in this Project
	#region dummy classes
	public class EditDb
	{
	}
	public class SqlDbViewer
	{
	}
	public class DbSelector
	{
		public ListBox ViewersList;
	}
	public class BankDbView
	{
	}
	public class CustDbView
	{
	}
	public class DetailsDbView
	{
	}
	public class DragDropClient
	{
	}
	public class MultiViewer
	{
	}
	public class AllCustomers
	{
	}

	#endregion dummy classes

	#region My MessageBox Definitions

	public struct mb
	{
		static public int nnull = 0;
		static public int NNULL = 0;
		static public int ok = 1;
		static public int OK = 1;
		static public int yes = 2;
		static public int YES = 2;
		static public int no = 3;
		static public int NO = 3;
		static public int cancel = 4;
		static public int CANCEL = 4;
		static public int iconexclm = 5;
		static public int ICONEXCLM = 5;
		static public int iconwarn = 6;
		static public int ICONWARN = 6;
		static public int iconerr = 7;
		static public int ICONERR = 7;
		static public int iconinfo = 8;
		static public int ICONINFO = 8;
	}

	public struct MB
	{
		static public int nnull = 0;
		static public int NNULL = 0;
		static public int ok = 1;
		static public int OK = 1;
		static public int yes = 2;
		static public int YES = 2;
		static public int no = 3;
		static public int NO = 3;
		static public int cancel = 4;
		static public int CANCEL = 4;
		static public int iconexclm = 5;
		static public int ICONEXCLM = 5;
		static public int iconwarn = 6;
		static public int ICONWARN = 6;
		static public int iconerr = 7;
		static public int ICONERR = 7;
		static public int iconinfo = 8;
		static public int ICONINFO = 8;
	}

	/// <summary>
	/// output parameters (return values) for my Message Box
	/// </summary>
	public struct Dlgresult
	{
		static public bool result;
		static public int returnint;
		static public string returnstring;
		static public string returnerror;
		static public object obj;
	}

	#endregion My MessageBox Definitions

	#region My MessageBox argument structuress
	/// <summary>
	/// Input parameters for my Message Box
	/// </summary>
	public struct DlgInput
	{
		//static public Msgbox MsgboxWin;
		//static public Msgboxs MsgboxSmallWin;
		//static public Msgboxs MsgboxMinWin;
		//		public static SysMenu sysmenu;
		static public bool isClean;
		static public bool resultboolin;
		static public bool UseDarkMode;
		static public bool resetdata;
		static public bool UseIcon;
		static public int intin;
		static public int returnint;
		static public string stringin;
		static public object obj;
		static public string iconstring;
		static public Thickness thickness;
		static public Image image;
		static public Brush dlgbackground;
		static public Brush dlgforeground;
		static public Brush btnbackground;
		static public Brush btnforeground;
		static public Brush Btnborder;
		static public Brush Btnmousebackground;
		static public Brush Btnmouseforeground;
		static public Brush defbtnbackground;
		static public Brush defbtnforeground;
		// Dark mode
		static public Brush BtnborderDark;
		static public Brush btnforegroundDark;
		static public Brush btnbackgroundDark;
		static public Brush defbtnforegroundDark;
		static public Brush defbtnbackgroundDark;
		static public Brush mouseborderDark;
		static public Brush mousebackgroundDark;
		static public Brush mouseforegroundDark;
		static public bool ShowButtonHitMaster;
		static public bool ShowButtonHit;
		static public Thickness BorderSizeNormal;                    // Normal display shadow
		static public Thickness BorderSizeDefault;            // Mouse over / (current Default) display
	}
	#endregion My MessageBox arguments

	#region  Cookies handling
	public struct defvars
	{
		public static Uri cookierootname = new Uri ( @"C:\Cookie" );
		public static String CookieDictionarypath = @"J:\users\ianch\documents\CookieDictionary.ser";
		public static String CookieCollectionpath = @"J:\users\ianch\documents\CookieCollection.ser";
		public static Dictionary<string, string> Cookiedictionary;
		public static CookieCollection Cookiecollection;
		public static int NextCookieIndex = 0;
		public static bool CookieAdded = false;
		public static bool FullViewer = false;
	}
	#endregion  Cookies handling

	//public struct TreeExplorer
	//{
	//	public static ExplorerClass Explorer = new ExplorerClass();
	//	public static DirectoryInfo DirInfo = new DirectoryInfo(@"C:\\");
	//}

	#region My GridColors arguments
	public struct GridControl
	{
		public string Controller { get; set; }
		public Brush transparency { get; set; }
		public Brush normalBackground { get; set; }
		public Brush normalForeground { get; set; }
		public Brush selectedBackground { get; set; }
		public Brush selectedForeground { get; set; }
		public Brush normalMouseBackground { get; set; }
		public Brush normalMouseForeground { get; set; }
		public Brush selectedMouseBackground { get; set; }
		public Brush selectedMouseForeground { get; set; }
		public double fontsize { get; set; }
	}

	#endregion My GridColors arguments

	// structure to hold all arguments required by DapperSuport data loading calls
	public struct DbLoadArgs
	{
		public string dbname;
		public string Orderby;
		public string Conditions;
		public bool wantSort;
		public bool wantDictionary;
		public bool Notify;
		public string Caller;
		public int [ ] args;
	}
	#endregion Generic System wide structures and Definitions

	//=======================//
	// Start of Mainwindow class
	//=======================//

	public partial class MainWindow : Window
	{
		// Global pointers to Viewmodel classes
		public static BankAccountViewModel bvm = null;
		public static CustomerViewModel cvm = null;
		public static DetailsViewModel dvm = null;
		//public static GenericSelectBoxControl glb = new GenericSelectBoxControl ( null , null );
		////public static ExpandoObject expobj = null;

		// SQL data - Default domain, current Table, current conn string for this domain
		public static string SqlCurrentConstring { get; set; }
		public static string CurrentSqlTableDomain { get; set; } = "IAN1";
		public static string CurrentActiveTable { get; set; }
		public static Window MainWin { get; set; }

		static public bool USE_ID_IDENTITY = false;
		static public bool SQL_USE_DMY_DATES = false;
		static public bool LogCWOutput = false;
		static public bool LOGTRACK = true;

		public enum fontWeight
		{
			Black = 900,
			UltraBold = 800,
			DemiBold = 600,
			Regular = 400,
			UltraLight = 200,
			Heavy = 900,
			ExtraBold = 800,
			Bold = 700,
			SemiBold = 600,
			Medium = 500,
			ExtraLight = 200,
			Thin = 100,
			DoNotCare = 0,
			Normal = 400,
			Light = 300
		}


		#region Dynamic variables

		//public static dynamic DgridDynamic;
		//public static dynamic LboxDynamic;
		//public static dynamic LviewDynamic;
		//public static dynamic UserctrlDynamic;
		//public static dynamic InterFaceDynamic;

		#endregion Dynamic variables

		public MainWindow ( )
		{
			InitializeComponent ( );
			Utils . SetupWindowDrag ( this );
			Mouse . SetCursor ( Cursors . Wait );
			this . Top = 200;
			this . Left = 500;
			MainWin = this;
			//DapperSupport.CheckDbDomain ( Flags . DbDomain );
			////ConString = SqlSupport.LoadConnectionStrings ( );
			//string ConString = Flags . CurrentConnectionString;
			//this . Show ( );

			/*   
				Utils.SetupWindowDrag ( this );
						this . Show ( );
						Mouse . SetCursor ( Cursors . Wait );
						Flags . CurrentConnectionString = Properties . Settings . Default [                
			string str = (string)Utils.ReadConfigSetting("ConnectionString");
 ] as string ;
						SqlCurrentConstring = Flags . CurrentConnectionString;
						Flags . FlowdocCrMultplier = 3.0;
						Flags . UseFlowdoc = Properties . Settings . Default . UseFlowDoc . ToUpper ( ) == "TRUE" ? true : false;
						Properties . Settings . Default . Save ( );
						Flags . UseScrollView = Properties . Settings . Default . UseScrollViewer . ToUpper ( ) == "TRUE" ? true : false;
						Properties . Settings . Default . Save ( );
						Flags . ReplaceFldNames = Properties . Settings . Default . ReplaceFldNames . ToUpper ( ) == "TRUE" ? true : false;
						Properties . Settings . Default . Save ( );
						Flags. UseMagnify = Properties. Settings. Default. UseMagnify == "TRUE" ? true : false;
						Properties . Settings . Default . Save ( );

						// delete track() log
						File . Delete ( $@"C:\users\ianch\Documents\LatestWpfApp.Trace.log" );

						string startpath = Properties . Settings . Default . AppRootPath;
						if ( startpath == "" )
						{
							startpath = SupportMethods . GetCurrentApplicationFullPath ( );
							Properties . Settings . Default . AppRootPath = startpath;
							Properties . Settings . Default . Save ( );
							Utils . SaveProperty ( "AppRootPath" , startpath );
						}
						//*This is a bit complex, but ....
						// * Currently, Listviews and GenericGridControl support my ListSelectBoxControl , a popup listbox
						 //* that allows users to choose fonts (in this case) for FlowDoc.
						 //* To allow this to be used in different Host controls we need to create an instance of the Control
						 //* here, and then simply add it to the relevant Canvas that   has to be used for FLowdoc usage.
						 //* Each subsequnet FlowDoc Host has to trigger an event in the Selectbox to set it up correctly to
						 //* allow it handle resizing and other commands



						Listviews . SetListboxHost += glb . Listbox_SetListboxHost;
						GenericGridControl . SetListboxHost += glb . Listbox_SetListboxHost;
						// Grab an ExpandoObject so we can pass loads of stuff around in it.
						expobj = new ExpandoObject ( );
						expobj . AddDictionaryEntry ( "Phone" , "0757 9062440" );
						expobj . AddDictionaryEntry ( "fname" , "Ian" );
						expobj . AddDictionaryEntry ( "lname" , "Turner" );
			*/

			//try
			//{
			// TESTING ; create Python environment and call method that returns a string
			// Working 15/11/22
			//              PatchParameter ( "SayHello" , 1 );



			// run_cmd ( "SayHello" , "" );
			//var paths = pyEngine . GetSearchPaths ( );
			//paths . Add ( @"C:\Users\ianch\source\repos\LatestWpfApp" );
			//pyEngine . SetSearchPaths ( paths );
			//var ipy = Python . CreateRuntime ( );
			//if(ipy != null)
			//    Debug . WriteLine ($"Python ScriptRuntime loaded successfully...");
			//dynamic py2 = ipy . UseFile ( @"C:\Users\ianch\source\repos\MyPython\PythonHello.py" );
			//Debug . WriteLine ( $"Python UseFile pythontest.py loaded successfully..." );
			//dynamic py3 = ipy . UseFile ( @"C:\Users\ianch\source\repos\LatestWpfApp\Python\pythonhello.py" );
			//dynamic py = ipy . UseFile ( @"C:\Users\ianch\source\repos\PythonMETHODS\PythonMETHODS.py" );
			//Debug . WriteLine ( $"Python UseFile PythonMETHODS.PY loaded successfully..." );
			//// Call Python method that returns a dynamic value
			//var res = py . Find_DividerInBuffer ( "this is a test of the Python string finder.." , " p" );
			//int v = py . SayHello ( 12 , 127 );
			//string str = v . ToString ( );
			//Debug . WriteLine ( $"Data returned from PYTHON is {str}" );

			//}
			//catch ( Exception ex )
			//{
			//    Debug . WriteLine ( $"Python failed to load script PythonMethods : [{ex . Message}]" );
			//    //LatestWpfApp . Utils . DoErrorBeep ( repeat: 1 );
			//}

			//Button16_Click ( null, null );
			Mouse . SetCursor ( Cursors . Arrow );
		}

		public string run_cmd ( string cmd, string args )
		{
			ProcessStartInfo start = new ProcessStartInfo ( );
			start . FileName = @"C:\Users\ianch\AppData\Local\Programs\Python\Python39\python.exe";
			start . Arguments = string . Format ( "\"{0}\" \"{1}\"", cmd, args );
			start . UseShellExecute = false;// Do not use OS shell
			start . CreateNoWindow = true; // We don't need new window
			start . RedirectStandardOutput = true;// Any output, generated by application will be redirected back
			start . RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)
			using ( Process process = Process . Start ( start ) )
			{
				using ( StreamReader reader = process . StandardOutput )
				{
					string stderr = process . StandardError . ReadToEnd ( ); // Here are the exceptions from our Python script
					string result = reader . ReadToEnd ( ); // Here is the result of StdOut(for example: print "test")
					return result;
				}
			}
		}

		private void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{

		}

		public static void SaveSystemSetting ( string name, object value, Type type = null )
		{
			if ( type != null )
			{
				if ( value . GetType ( ) == type )
				{
					if ( type . GetType ( ) == typeof ( int ) )
						Properties . Settings . Default [ name ] = ( int ) value;
					else if ( type . GetType ( ) == typeof ( double ) )
						Properties . Settings . Default [ name ] = ( double ) value;
					else if ( type . GetType ( ) == typeof ( string ) )
						Properties . Settings . Default [ name ] = ( string ) value;
					else if ( type . GetType ( ) == typeof ( GenericClass ) )
						Properties . Settings . Default [ name ] = ( GenericClass ) value;
					else if ( type . GetType ( ) == typeof ( float ) )
						Properties . Settings . Default [ name ] = ( float ) value;
					else if ( type . GetType ( ) == typeof ( List<string> ) )
						Properties . Settings . Default [ name ] = ( List<string> ) value;
					Properties . Settings . Default . Save ( );
				}
			}
			else
			{
				Properties . Settings . Default [ name ] = value;
				Properties . Settings . Default . Save ( );
			}
		}

		public static object GetSystemSetting ( string name )
		{
			return ( object ) Properties . Settings . Default [ name ];
		}
		public static void RemoveGenericlistboxcontrol ( Canvas canvas )
		{
			var kids = canvas . Children;
			bool exists = false;
			int index = 0;
			do
			{
				foreach ( var item in kids )
				{
					string str = item . ToString ( );
					if ( str . ToString ( ) . Contains ( "GenericSelectBoxControl" ) )
					{
						kids . Remove ( ( UIElement ) item );
						exists = true;
						break;
					}
					index++;
				}
				if ( exists != true )
				{
					exists = false;
					break;
				}
				else
				{
					exists = false;
					break;
				}
			} while ( true );
		}

		//		private void button1_Click ( object sender , RoutedEventArgs e )
		//		{
		//			Datagrids dg = new Datagrids();
		//			dg . Show ( );
		//		}

		//		private void button2_Click ( object sender , RoutedEventArgs e )
		//		{
		//			Listviews lv = new Listviews();
		//			lv. Show ( );
		//		}

		//		private void button3_Click ( object sender , RoutedEventArgs e )
		//		{
		//			TreeViews tv = new TreeViews();
		//#pragma warning disable CS0168 // The variable 'ex' is declared but never used
		//			try
		//			{
		//				tv . Show ( );
		//			}
		//            catch ( Exception ex ) { Debug. WriteLine ("TreeViews already open"); }
		//#pragma warning restore CS0168 // The variable 'ex' is declared but never used
		//		}

		//		private void button4_Click ( object sender , RoutedEventArgs e )
		//		{
		//			VmTest vmt = new VmTest();
		//			vmt . Show ( );
		//		}

		private void button5_Click ( object sender, RoutedEventArgs e )
		{
			// Show the S.Procs / SQL table edit system Window
			this . Topmost = false;
			SProcsHandling gmvvm = new SProcsHandling ( );			
			gmvvm . Show ( );
			gmvvm . Topmost = true;
		}

		//		private void button6_Click ( object sender , RoutedEventArgs e )
		//		{
		//			MvvmDataGrid bg = new MvvmDataGrid();
		//			bg . Show ( );
		//		}

		//		private void button7_Click ( object sender , RoutedEventArgs e )
		//		{
		//			MvvmUserTest mut = new MvvmUserTest();
		//			mut . Show ( );
		//		}

		//		private void button8_Click ( object sender , RoutedEventArgs e )
		//		{
		//			ModernViews ga = new ModernViews ( );
		//			ga . Show ( );
		//		}

		//private void button9_Click ( object sender , RoutedEventArgs e )
		//{
		//	SysConfig scfg = new         SysConfig();
		//	scfg . Show ( );

		private void button10_Click ( object sender, RoutedEventArgs e )
		{
			this . Close ( );
			Application . Current . Shutdown ( );
		}
		/*
				private void Window_PreviewKeyDown ( object sender , KeyEventArgs e )
				{
					if ( e . Key == Key . D )
						button1_Click ( sender , null );
					else if ( e . Key == Key . L )
						button3_Click ( sender , null );
					else if ( e . Key == Key . T )
						button3_Click ( sender , null );
					else if ( e . Key == Key . V )
						button4_Click ( sender , null );
					else if ( e . Key == Key . M )
						button5_Click ( sender , null );
					else if ( e . Key == Key . G )
						button6_Click ( sender , null );
					else if ( e . Key == Key . U )
						button7_Click ( sender , null );
					else if ( e . Key == Key . Enter )
						button7_Click ( sender , null );
					else if ( e . Key == Key . Escape )
						Application . Current . Shutdown ( );
				}

				private void Button10_Click ( object sender , RoutedEventArgs e )
				{
					Application . Current . Shutdown ( );
				}
				private void Button1_Click ( object sender , RoutedEventArgs e )
				{
					//Datagrids dg = new Datagrids ( );
					//dg . Show ( );
				}
				private void Button2_Click ( object sender , RoutedEventArgs e )
				{
					//Listviews lv = new Listviews ( );
					//lv . Show ( );
				}
				private void Button3_Click ( object sender , RoutedEventArgs e )
				{
					//TreeViews tvs = new TreeViews ( );
					//tvs . Show ( );
				}
				private void Button6_Click ( object sender , RoutedEventArgs e )
				{
					//MvvmContainerWin mvvm = new MvvmContainerWin ( );
					//mvvm . Show ( );
				}
				private void Button7_Click ( object sender , RoutedEventArgs e )
				{
					//MvvmUserTest mut = new MvvmUserTest ( );
					//mut . Show ( );
				}

				//        private void button11_Click ( object sender, RoutedEventArgs e )
				//        {
				//			ExpanderTest et = new ExpanderTest ( );
				//			et. Show ( );
				//        }

				//        private void button12_Click ( object sender, RoutedEventArgs e )
				//        {
				//			SplitViewer sv = new SplitViewer ( );
				//			sv. Show ( );
				//        }

				private void Button13_Click ( object sender , RoutedEventArgs e )
				{
					//FourwaySplitViewer fv = new FourwaySplitViewer ( );
					//fv . Show ( );
				}
				//        private void button14_Click ( object sender, RoutedEventArgs e )
				//        {
				//			Mouse . OverrideCursor = Cursors . Wait;
				//			InterWinComms iw= new InterWinComms ( );
				//            iw . Show ( );
				//		}

				//		private void button15_Click ( object sender, RoutedEventArgs e )
				//        {
				//			Menutest mt = new Menutest ( );
				//			mt . Show ( );
				//        }

				private void Button16_Click ( object sender , RoutedEventArgs e )
				{
					//BankAcHost bah = new  BankAcHost ( );
					//bah . Show ( );
				}

				//        private void button17_Click ( object sender , RoutedEventArgs e )
				//        {
				//			MvvmContainerWin ucm = new MvvmContainerWin ( );
				//			ucm . Show ( );
				//        }

				//        private void button18_Click ( object sender , RoutedEventArgs e )
				//        {
				//			TabViewInfo tvvi = new TabViewInfo ("NewWPfDevInfo.txt" );
				//			tvvi . Show ( );
				//		}


				//        private void button19_Click ( object sender , RoutedEventArgs e )
				//        {
				//			//get a pointer to class view mdel
				//			//UserCtrlViewModel ucvm = new UserCtrlViewModel ( );			
				//			UcHostWindow uch = new UcHostWindow ( );
				//			uch . Show ( );
				//			uch . LoadHostWindow ( );
				//		}

				private void Button20_Click ( object sender , RoutedEventArgs e )
				{
					//Tabview ss = new Tabview ( );
					//ss . Show ( );
				}
				private void Button21_Click ( object sender , RoutedEventArgs e )
				{
					//YieldWindow yw = new YieldWindow ( );
					//yw . Show ( );
				}

				private void Button4_Click ( object sender , RoutedEventArgs e )
				{

				}

				private void Button5_Cick ( object sender , RoutedEventArgs e )
				{
					//Genericgrid grid = new Genericgrid ( );
					//grid . Show ( );
				}

				private void ShowTrackLog ( object sender , RoutedEventArgs e )
				{
					string buffer = "";
					if ( MainWindow . LOGTRACK )
					{
						//TraceViewer trv = new TraceViewer ( );
						//trv . Show ( );
					}
					//buffer = File .ReadAllText( $@"C:\users\ianch\Documents\LatestWpfApp.Trace.log" );

				}
		*/
		//static public int GetfontWeight ( string type )
		//{
		//	switch ( type )
		//	{
		//		case "Normal":
		//			return Convert . ToInt32 ( fontWeight . Normal );
		//		case "Black":
		//			return Convert . ToInt32 ( fontWeight . Black );
		//		case "UltraBold":
		//			return Convert . ToInt32 ( fontWeight . UltraBold );
		//		case "DemiBold":
		//			return Convert . ToInt32 ( fontWeight . DemiBold );
		//		case "Regular":
		//			return Convert . ToInt32 ( fontWeight . Regular );
		//		case "Heavy":
		//			return Convert . ToInt32 ( fontWeight . Heavy );
		//		case "ExtraBold":
		//			return Convert . ToInt32 ( fontWeight . ExtraBold );
		//		case "Bold":
		//			return Convert . ToInt32 ( fontWeight . Bold );
		//		case "SemiBold":
		//			return Convert . ToInt32 ( fontWeight . SemiBold );
		//		case "Medium":
		//			return Convert . ToInt32 ( fontWeight . Medium );
		//		case "ExtraLight":
		//			return Convert . ToInt32 ( fontWeight . ExtraLight );
		//		case "Thin":
		//			return Convert . ToInt32 ( fontWeight . Thin );
		//		case "UltraLight":
		//			return Convert . ToInt32 ( fontWeight . UltraLight );
		//		case "Light":
		//			return Convert . ToInt32 ( fontWeight . Light );
		//	}
		//	return -1;
		//}

		static public int GetfontWeight ( string type )
		{
			switch ( type )
			{
				case "Normal":
					return Convert . ToInt32 ( fontWeight . Normal );
				case "Black":
					return Convert . ToInt32 ( fontWeight . Black );
				case "UltraBold":
					return Convert . ToInt32 ( fontWeight . UltraBold );
				case "DemiBold":
					return Convert . ToInt32 ( fontWeight . DemiBold );
				case "Regular":
					return Convert . ToInt32 ( fontWeight . Regular );
				case "Heavy":
					return Convert . ToInt32 ( fontWeight . Heavy );
				case "ExtraBold":
					return Convert . ToInt32 ( fontWeight . ExtraBold );
				case "Bold":
					return Convert . ToInt32 ( fontWeight . Bold );
				case "SemiBold":
					return Convert . ToInt32 ( fontWeight . SemiBold );
				case "Medium":
					return Convert . ToInt32 ( fontWeight . Medium );
				case "ExtraLight":
					return Convert . ToInt32 ( fontWeight . ExtraLight );
				case "Thin":
					return Convert . ToInt32 ( fontWeight . Thin );
				case "UltraLight":
					return Convert . ToInt32 ( fontWeight . UltraLight );
				case "Light":
					return Convert . ToInt32 ( fontWeight . Light );
			}
			return -1;
		}
	}
}
