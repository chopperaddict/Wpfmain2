using System;
using System . Collections;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Configuration;
using System . Data;
using System . Diagnostics;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Controls . Primitives;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;
using System . Xml . Serialization;

using Microsoft . Win32;

using Newtonsoft . Json;

using Wpfmain . Properties;

namespace Wpfmain
{

    /// <summary>
    /// Class to handle various utility functions such as fetching 
    /// Style/Templates/Brushes etc to Set/Reset control styles 
    /// from various Dictionary sources for use in "code behind"
    /// </summary>
    public class Utils
    {
        public static Action<DataGrid, int> GridInitialSetup = Utils.SetUpGridSelection;

        // list each window that wants to support control capture needs to have so
        // mousemove can add current item under cursor to the list, and then F11 will display it.
        public static List<HitTestResult> ControlsHitList = new List<HitTestResult>();

        #region structures
        public struct bankrec
        {
            public string custno
            {
                get; set;
            }
            public string bankno
            {
                get; set;
            }
            public int actype
            {
                get; set;
            }
            public decimal intrate
            {
                get; set;
            }
            public decimal balance
            {
                get; set;
            }
            public DateTime odate
            {
                get; set;
            }
            public DateTime cdate
            {
                get; set;
            }
        }
        #endregion structures

        #region play tunes / sounds
        // Declare the first few notes of the song, "Mary Had A Little Lamb".
        // Define the frequencies of notes in an octave, as well as
        // silence (rest).
        protected enum Tone
        {
            REST = 0,
            GbelowC = 196,
            A = 220,
            Asharp = 233,
            B = 247,
            C = 262,
            Csharp = 277,
            D = 294,
            Dsharp = 311,
            E = 330,
            F = 349,
            Fsharp = 370,
            G = 392,
            Gsharp = 415,
        }

        // Define the duration of a note in units of milliseconds.
        protected enum Duration
        {
            WHOLE = 1600,
            HALF = WHOLE / 2,
            QUARTER = HALF / 2,
            EIGHTH = QUARTER / 2,
            SIXTEENTH = EIGHTH / 2,
        }

        protected struct Note
        {
            Tone toneVal;
            Duration durVal;

            // Define a constructor to create a specific note.
            public Note ( Tone frequency , Duration time )
            {
                toneVal = frequency;
                durVal = time;
            }

            // Define properties to return the note's tone and duration.
            public Tone NoteTone
            {
                get
                {
                    return toneVal;
                }
            }
            public Duration NoteDuration
            {
                get
                {
                    return durVal;
                }
            }
        }
        public static void PlayMary ( )
        {
            Note[] Mary =
                {
                            new Note(Tone.B, Duration.QUARTER),
                            new Note(Tone.A, Duration.QUARTER),
                            new Note(Tone.GbelowC, Duration.QUARTER),
                            new Note(Tone.A, Duration.QUARTER),
                            new Note(Tone.B, Duration.QUARTER),
                            new Note(Tone.B, Duration.QUARTER),
                            new Note(Tone.B, Duration.HALF),
                            new Note(Tone.A, Duration.QUARTER),
                            new Note(Tone.A, Duration.QUARTER),
                            new Note(Tone.A, Duration.HALF),
                            new Note(Tone.B, Duration.QUARTER),
                            new Note(Tone.D, Duration.QUARTER),
                            new Note(Tone.D, Duration.HALF)
                };
            // Play the song
            Play ( Mary );
        }
        // Play the notes in a song.
        protected static void Play ( Note [ ] tune )
        {
            foreach ( Note n in tune )
            {
                if ( n . NoteTone == Tone . REST )
                    Thread . Sleep ( ( int ) n . NoteDuration );
                else
                    Console . Beep ( ( int ) n . NoteTone , ( int ) n . NoteDuration );
            }
        }
        // Define a note as a frequency (tone) and the amount of
        //// time (duration) the note plays.
        //public static Task DoBeep ( int freq = 180, int count = 300, bool swap = false )
        //{
        //	int tone = freq;
        //	int duration = count;
        //	int x = 0;
        //	Task t = new Task ( ( ) => x = 1 );
        //	if ( Flags . UseBeeps )
        //	{
        //		if ( swap )
        //		{
        //			tone = ( tone / 4 ) * 3;
        //			duration = ( count * 5 ) / 2;
        //			t = Task . Factory . StartNew ( ( ) => Console . Beep ( freq, count ) )
        //				. ContinueWith ( Action => Console . Beep ( tone, duration ) );
        //			Thread . Sleep ( 500 );
        //		}
        //		else
        //		{
        //			tone = ( tone / 4 ) * 3;
        //			duration = ( count * 5 ) / 2;
        //			t = Task . Factory . StartNew ( ( ) => Console . Beep ( tone, duration ) )
        //				. ContinueWith ( Action => Console . Beep ( freq, count ) );
        //			Thread . Sleep ( 500 );
        //		}
        //	}
        //	else
        //	{
        //		Task task = Task . Factory . StartNew ( ( ) => Debug. WriteLine ( ) );
        //		t = task ,TaskScheduler . FromCurrentSynchronizationContext ( );
        //			}

        //	TaskScheduler . FromCurrentSynchronizationContext ( ));
        //	return t;
        //}
        public static void DoSingleBeep ( int freq = 280 , int count = 300 , int repeat = 1 )
        {
            for ( int i = 0 ; i < repeat ; i++ )
            {
                Console . Beep ( freq , count );
                Thread . Sleep ( 200 );
            }
        }
        public static void DoErrorBeep ( int freq = 280 , int count = 100 , int repeat = 3 )
        {
            for ( int i = 0 ; i < repeat ; i++ )
            {
                Console . Beep ( freq , count );
            }
            Thread . Sleep ( 100 );
        }

        #endregion play tunes / sounds

        public static Dictionary<string, string> ConnectionStringsDict = new Dictionary<string, string>();

        //public static void LoadConnectionStrings ( )
        //{
        //    string str = "";
        //    try
        //    {
        //        if ( ConnectionStringsDict . Count == 1 )
        //        {
        //            str = ( string ) Utils . ReadConfigSetting ( "ConnectionString" );
        //            Utils . WriteSerializedCollectionJSON ( ConnectionStringsDict , @"C:\users\ianch\DbConnectionstrings.dat" );
        //        }
        //        else if ( ConnectionStringsDict . Count > 0 )
        //        {
        //            {
        //                str = ( string ) Utils . ReadConfigSetting ( "ConnectionString" );

        //                ConnectionStringsDict . Add ( "IAN1" , Utils . ReadConfigSetting ( "ConnectionString" ) );
        //                //ConnectionStringsDict.Add("NORTHWIND", Utils.ReadConfigSetting("NorthwindConnectionString"));
        //                //ConnectionStringsDict.Add("PUBS", Utils.ReadConfigSetting("PubsConnectionString"));
        //                Utils . WriteSerializedCollectionJSON ( ConnectionStringsDict , @"C:\users\ianch\DbConnectionstrings.dat" );
        //            }
        //        }
        //    }
        //    catch ( NullReferenceException ex )
        //    {
        //        string s = Utils.ReadConfigSetting("ConnectionString");
        //        Debug . WriteLine ( $"Dictionary  entrry [{s}] already exists" );
        //    }
        //    return;
        //}

        /// <summary>
        /// Simulate Application.DoEvents function of 
        /// <see cref=" System.Windows.Forms.Application"/> class.
        /// </summary>
        //[SecurityPermissionAttribute ( SecurityAction . Demand ,
        //    Flags = SecurityPermissionFlag . UnmanagedCode )]
        //public static void SetSynchforDbCollections ( object _lock ,
        //	ObservableCollection<BankAccountViewModel> bvmcollection ,
        //	ObservableCollection<CustomerViewModel> cvmcollection ,
        //	ObservableCollection<DetailsViewModel> dvmcollection
        //	)
        //{
        //	_lock = new object ( );
        //	BindingOperations . EnableCollectionSynchronization ( bvmcollection , _lock );
        //	_lock = new object ( );
        //	BindingOperations . EnableCollectionSynchronization ( cvmcollection , _lock );
        //	_lock = new object ( );
        //	BindingOperations . EnableCollectionSynchronization ( dvmcollection , _lock );
        //}

        #region Dictionary Handlers

        public static string GetDictionaryEntry ( Dictionary<string , string> dict , string key , out string dictvalue )
        {
            string keyval = "";

            if ( dict . TryGetValue ( key . ToUpper ( ) , out keyval ) == false )
            {
                Debug . WriteLine ( $"Unable to access Dictionary {dict} to identify key value [{key}]" );
                key = key + "ConnectionString";
                Utils . DoErrorBeep ( 250 , 50 , 1 );
            }
            dictvalue = keyval;
            return keyval;
        }
        public static string GetDictionaryEntry ( Dictionary<int , string> dict , int key , out string dictvalue )
        {
            string keyval = "";

            if ( dict . TryGetValue ( key , out keyval ) == false )
            {
                Debug . WriteLine ( $"Unable to access Dictionary {dict} to identify key value [{key}]" );
                Utils . DoErrorBeep ( 250 , 50 , 1 );
            }
            dictvalue = keyval;
            return keyval;
        }
        public static int GetDictionaryEntry ( Dictionary<int , int> dict , int key , out int dictvalue )
        {
            int keyval = 0;

            if ( dict . TryGetValue ( key , out keyval ) == false )
            {
                Debug . WriteLine ( $"Unable to access Dictionary {dict} to identify key value [{key}]" );
                Utils . DoErrorBeep ( 250 , 50 , 1 );
            }
            dictvalue = keyval;
            return keyval;
        }
        //public static bool AddDictionaryEntry ( Dictionary<string , string> dict , string key , string dictvalue )
        //{
        //	try
        //	{
        //		dict . Add ( key , dictvalue );
        //	}
        //	catch ( Exception ex )
        //	{
        //		Debug. WriteLine ( $"Unable to access Dictionary {dict} to identify key value [{key}]" );
        //		Utils . DoErrorBeep ( 250 , 50 , 1 );
        //		return false;
        //	}
        //	return true;
        //}
        public static bool DeleteDictionaryEntry ( Dictionary<string , string> dict , string value )
        {
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            try
            {
                dict . Remove ( value );
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"Unable to access Dictionary {dict} to delete key value [{value}]" );
                Utils . DoErrorBeep ( 250 , 50 , 1 );
                return false;
            }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            return true;
        }
        public static bool DeleteDictionaryEntry ( Dictionary<string , int> dict , string value )
        {
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            try
            {
                dict . Remove ( value );
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"Unable to access Dictionary {dict} to delete key value [{value}]" );
                Utils . DoErrorBeep ( 250 , 50 , 1 );
                return false;
            }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            return true;
        }
        public static bool DeleteDictionaryEntry ( Dictionary<int , int> dict , int value )
        {
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            try
            {
                dict . Remove ( value );
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"Unable to access Dictionary {dict} to delete key value [{value}]" );
                Utils . DoErrorBeep ( 250 , 50 , 1 );
                return false;
            }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            return true;
        }
        #endregion Dictionary Handlers

        #region datagrid row  to List methods (string, int, double, decimal, DateTime)
        public static List<string> GetTableColumnsList ( DataTable dt )
        {
            //Return a list of strings Containing table column info
            List<string> list = new List<string>();
            foreach ( DataRow row in dt . Rows )
            {
                string output = "";
                var colcount = row.ItemArray.Length;
                object type = null;

                switch ( colcount )
                {
                    case 1:
                        type = row . Field<object> ( 0 );
                        output += row . Field<string> ( 0 );
                        break;
                    case 2:
                        output += row . Field<string> ( 0 ) + ", ";
                        type = row . Field<object> ( 1 );
                        output += row . Field<string> ( 1 ) + " ";
                        break;
                    case 3:
                        output += row . Field<string> ( 0 ) + ", ";
                        output += row . Field<string> ( 1 ) + ", ";
                        type = row . Field<object> ( 2 );
                        if ( type != null )
                        {
                            if ( ( Type ) type == typeof ( int ) )
                            {
                                output += row . Field<string> ( 2 ) . ToString ( ) + "";
                            }
                        }
                        break;
                    case 4:
                        output += row . Field<string> ( 0 ) + ", ";
                        output += row . Field<string> ( 1 ) + ", ";
                        output += row . Field<string> ( 2 ) + ", ";
                        type = row . Field<object> ( 3 );
                        output += row . Field<string> ( 3 ) + " ";
                        break;
                    case 5:
                        output += row . Field<string> ( 0 ) + ", ";
                        output += row . Field<string> ( 1 ) + ", ";
                        output += row . Field<string> ( 2 ) + ", ";
                        output += row . Field<string> ( 3 ) + ", ";
                        type = row . Field<object> ( 4 );
                        output += row . Field<string> ( 4 ) + "";
                        break;
                }
                list . Add ( output );
            }
            return list;
        }
        public static List<string> GetDataDridRowsAsListOfStrings ( DataTable dt )
        {
            List<string> list = new List<string>();
            foreach ( DataRow row in dt . Rows )
            {
                var txt = row.Field<string>(0);
                list . Add ( txt );
            }
            return list;
        }
        public static List<int> GetDataDridRowsAsListOfInts ( DataTable dt )
        {
            List<int> list = new List<int>();
            foreach ( DataRow row in dt . Rows )
            {
                // ... Write value of first field as integer.
                list . Add ( row . Field<int> ( 0 ) );
            }
            return list;
        }
        public static List<double> GetDataDridRowsAsListOfDoubles ( DataTable dt )
        {
            List<double> list = new List<double>();
            foreach ( DataRow row in dt . Rows )
            {
                // ... Write value of first field as integer.
                list . Add ( row . Field<double> ( 0 ) );
            }
            return list;
        }
        public static List<decimal> GetDataDridRowsAsListOfDecimals ( DataTable dt )
        {
            List<decimal> list = new List<decimal>();
            foreach ( DataRow row in dt . Rows )
            {
                // ... Write value of first field as integer.
                list . Add ( row . Field<decimal> ( 0 ) );
            }
            return list;
        }
        public static List<DateTime> GetDataDridRowsAsListOfDateTime ( DataTable dt )
        {
            List<DateTime> list = new List<DateTime>();
            foreach ( DataRow row in dt . Rows )
            {
                // ... Write value of first field as integer.
                list . Add ( row . Field<DateTime> ( 0 ) );
            }
            return list;
        }
        #endregion datagrid row  to List methods

        public static string ParseTableColumnData ( List<string> fldnameslist )
        {
            int indx = 0;
            string entry = "", outp = "";
            int trimlen = 3;
            string output = "";
            foreach ( string row in fldnameslist )
            {
                if ( indx < 3 )
                {
                    entry = row;
                    if ( indx == 0 )
                        output += entry . ToUpper ( ) + ",  ";
                    else if ( indx == 1 )
                        output += entry + ",  ";
                    else
                    {
                        if ( entry == "---" )
                        {
                            outp = output . Substring ( 0 , output . Length - trimlen );// += "\n";
                            output += "\n";
                            indx = 3;
                        }
                        else
                        {
                            //outp = output . Substring ( 0 , output . Length - trimlen );// += "\n";
                            output += entry + "\n";
                            indx = 3;
                        }
                    }
                    if ( indx < 3 )
                        indx++;
                    else
                        indx = 0;
                }
                else
                {
                    indx = 0;
                }
            }
            return output;
        }
        // Record the names of the method that called this one in an iterative tree.
        public static string trace ( string prompt = "" )
        {
            // logs all the calls made upwards in a tree
            string output = "", tmp = "";
            int indx = 1;
            var v = new StackTrace(0);
            if ( prompt == "" )
                output = prompt + $"\nStackTrace  :\n";
            else
                output = prompt + $"\nStackTrace  for {prompt . ToString ( )}:\n";
            while ( true )
            {
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                try
                {

                    tmp = v . GetFrame ( indx++ ) . GetMethod ( ) . Name + '\n';
                    if ( tmp . Contains ( "Invoke" ) || tmp . Contains ( "RaiseEvent" ) )
                        break;
                    else
                        output += tmp;
                }
                catch ( Exception ex )
                {
                    Debug . WriteLine ( "trace() Crashed...\n" );
                    output += "\ntrace() Crashed...\n";
                    break;
                }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            }
            Debug . WriteLine ( $"\n{output}\n" );
            return $"\n{output}\n";
        }
        //TODO
        //        public static void Mbox ( Window win , string string1 = "" , string string2 = "" , string caption = "" , string iconstring = "" , int Btn1 = 1 , int Btn2 = 0 , int Btn3 = 0 , int Btn4 = 0 , int defButton = 1 , bool minsize = false , bool modal = false )
        //        {
        //            // We NEED to remove any \r as part of \r\n as textboxes ONLY accept \n on its own for Cr/Lf
        //            string1 = ParseforCR ( string1 );
        ////TODO            Msgboxs m = new Msgboxs ( string1: string1 , string2: string2 , caption: caption , Btn1: Btn1 , Btn2: Btn2 , Btn3: Btn3 , Btn4: Btn4 , defButton: defButton , iconstring: iconstring , MinSize: minsize , modal: modal );
        //            //			m . Owner = win;

        //            if ( modal == false )
        //                m . Show ( );
        //            else
        //                m . ShowDialog ( );
        //        }
        //        public static void Mssg (
        //                        string caption = "" ,
        //                        string string1 = "" ,
        //                        string string2 = "" ,
        //                        string string3 = "" ,
        //                        string title = "" ,
        //                        string iconstring = "" ,
        //                        int defButton = 1 ,
        //                        int Btn1 = 1 ,
        //                        int Btn2 = 2 ,
        //                        int Btn3 = 3 ,
        //                        int Btn4 = 4 ,
        //                        string btn1Text = "" ,
        //                        string btn2Text = "" ,
        //                        string btn3Text = "" ,
        //                        string btn4Text = "" ,
        //                        bool usedialog = true
        //        )
        //        {
        //            Msgbox msg = new Msgbox (
        //                caption: caption ,
        //                string1: string1 ,
        //                string2: string2 ,
        //                string3: string3 ,
        //                title: title ,
        //                Btn1: Btn1 ,
        //                Btn2: Btn2 ,
        //                Btn3: Btn3 ,
        //                Btn4: Btn4 ,
        //                defButton: defButton ,
        //                iconstring: iconstring ,
        //                btn1Text: btn1Text ,
        //                btn2Text: btn2Text ,
        //                btn3Text: btn3Text ,
        //                btn4Text: btn4Text );
        //            //msg . Owner = win;
        //            if ( usedialog )
        //                msg . ShowDialog ( );
        //            else
        //                msg . Show ( );
        //        }
        public static Brush BrushFromColors ( Color color )
        {
            Brush brush = new SolidColorBrush(color);
            return brush;
        }
        //Working well 4/8/21
        public static string ConvertInputDate ( string datein )
        {
            string YYYMMDD = "";
            string[] datebits;
            // This filter will strip off the "Time" section of an excel date
            // and return us a valid YYYY/MM/DD string
            char[] ch = { '/', ' ' };
            datebits = datein . Split ( ch );
            if ( datebits . Length < 3 )
                return datein;

            // check input to see if it needs reversing ?
            if ( datebits [ 0 ] . Length == 4 )
                YYYMMDD = datebits [ 0 ] + "/" + datebits [ 1 ] + "/" + datebits [ 2 ];
            else
                YYYMMDD = datebits [ 2 ] + "/" + datebits [ 1 ] + "/" + datebits [ 0 ];
            return YYYMMDD;
        }
        //TODO        public static BankDragviewModel CreateBankGridRecordFromString ( string input )
        //        {
        //            int index = 1;
        //            string type = "";
        //            //			BankAccountViewModel bvm = new BankAccountViewModel ( );
        //            BankDragviewModel bvm = new BankDragviewModel ( );
        //            CustomerDragviewModel cvm = new CustomerDragviewModel ( );

        //            char [ ] s = { ',' };
        //            string [ ] data = input . Split ( s );
        //            string donor = data [ 0 ];
        //            try
        //            {
        //                DateTime dt;
        //                type = data [ 0 ];
        //                if ( type == "BANKACCOUNT" || type == "BANK" || type == "DETAILS" )
        //                {
        //                    // This WORKS CORRECTLY 12/6/21 when called from n SQLDbViewer DETAILS grid entry && BANK grid entry					
        //                    // this test confirms the data layout by finding the Odate field correctly
        //                    // else it drops thru to the Catch branch
        //                    dt = Convert . ToDateTime ( data [ 7 ] );
        //                    //We can have any type of record in the string recvd
        //                    index = 1;  // jump the data type string
        //                    bvm . RecordType = type;
        //                    bvm . Id = int . Parse ( data [ index++ ] );
        //                    bvm . CustNo = data [ index++ ];
        //                    bvm . BankNo = data [ index++ ];
        //                    bvm . AcType = int . Parse ( data [ index++ ] );
        //                    bvm . IntRate = decimal . Parse ( data [ index++ ] );
        //                    bvm . Balance = decimal . Parse ( data [ index++ ] );
        //                    bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
        //                    bvm . CDate = Convert . ToDateTime ( data [ index ] );
        //                    return bvm;
        //                }
        //            }
        //            catch
        //            {
        //                //Check to see if the data includes the data type in it
        //                //As we have to parse it diffrently if not - see index....
        //                index = 0;
        //#pragma warning disable CS0168 // The variable 'ex' is declared but never used
        //                try
        //                {
        //                    int x = int . Parse ( donor );
        //                    // if we get here, it IS a NUMERIC VALUE
        //                    index = 0;
        //                }
        //                catch ( Exception ex )
        //                {
        //                    //its probably the Data Type string, so ignore it for our Data creation processing
        //                    index = 1;
        //                }
        //#pragma warning restore CS0168 // The variable 'ex' is declared but never used
        //                //We have a CUSTOMER record
        //                bvm . RecordType = type;
        //                bvm . Id = int . Parse ( data [ index++ ] );
        //                bvm . CustNo = data [ index++ ];
        //                bvm . BankNo = data [ index++ ];
        //                bvm . AcType = int . Parse ( data [ index++ ] );
        //                bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
        //                bvm . CDate = Convert . ToDateTime ( data [ index ] );
        //                return bvm;
        //            }
        //            return bvm;
        //        }

        public static void Magnify ( List<object> list , bool magnify )
        {
            // lets other controls have magnification, providing other Templates do not overrule these.....
            for ( int i = 0 ; i < list . Count ; i++ )
            {
                var obj = list[i] as ListBox;
                if ( obj != null )
                {
                    obj . Style = magnify ? Application . Current . FindResource ( "ListBoxMagnifyAnimation" ) as Style : null;
                    continue;
                }
                var lv = list[i] as ListView;
                if ( lv != null )
                {
                    lv . Style = magnify ? Application . Current . FindResource ( "ListViewMagnifyAnimation" ) as Style : null;
                    continue;
                }
                var dg = list[i] as DataGrid;
                if ( dg != null )
                {
                    dg . Style = magnify ? Application . Current . FindResource ( "DatagridMagnifyAnimation0" ) as Style : null;
                    continue;
                }
                var bd = list[i] as Border;
                if ( bd != null )
                {
                    bd . Style = magnify ? Application . Current . FindResource ( "BorderMagnifyAnimation" ) as Style : null;
                    continue;
                }
                var cb = list[i] as ComboBox;
                if ( cb != null )
                {
                    cb . Style = magnify ? Application . Current . FindResource ( "ComboBoxMagnifyAnimation" ) as Style : null;
                    continue;
                }
                var btn = list[i] as Button;
                if ( btn != null )
                {
                    btn . Style = magnify ? Application . Current . FindResource ( "ButtonMagnifyAnimation" ) as Style : null;
                    continue;
                }
                var rct = list[i] as Rectangle;
                if ( rct != null )
                {
                    rct . Style = magnify ? Application . Current . FindResource ( "RectangleMagnifyAnimation" ) as Style : null;
                    continue;
                }
                var tb = list[i] as TextBlock;
                if ( tb != null )
                {
                    tb . Style = magnify ? Application . Current . FindResource ( "TextBlockMagnifyAnimation" ) as Style : null;
                    continue;
                }
                var tbx = list[i] as TextBox;
                if ( tbx != null )
                {
                    tbx . Style = magnify ? System . Windows . Application . Current . FindResource ( "TextBoxMagnifyAnimation" ) as Style : null;
                    continue;
                }
            }
        }
        public static bool HitTestScrollBar ( object sender , MouseButtonEventArgs e )
        {
            //TODO
            //FlowDoc fd = new FlowDoc ( );

            //HitTestResult hit = VisualTreeHelper . HitTest ( ( Visual ) sender, e . GetPosition ( ( InputElement ) sender ) );
            //string str = hit .VisualHit. ToString ( );
            //bool bl = str. Contains ( "Rectangle" );

            //RichTextBox rtb = sender as RichTextBox;
            //if ( rtb != null)
            //{
            //    fd.IsEnabled = true;
            //    return true;
            //}
            //fd = sender as FlowDoc;
            //if ( fd != null )
            //{
            //    fd.doc . IsEnabled = true;
            //    //fd.S
            //}
            //// Retrieve the coordinate of the mouse position.
            //Point pt = e . GetPosition ( fd );

            //PointHitTestParameters php = new PointHitTestParameters (pt );
            //Point pt2 = php . HitPoint;
            // pt & pt give same response
            // Perform the hit test against a given portion of the visual object tree.



            //HitTestResult result = VisualTreeHelper . HitTest (  pt2 , pt );
            //          double min = fd . ActualWidth - 30;
            //          double maxmin = fd . ActualWidth - 10;
            //{
            //    // Perform action on hit visual object.
            //    return true;
            //}
            ////			return hit . VisualHit . GetVisualAncestor<ScrollBar> ( ) != null;
            object original = e.OriginalSource;
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            try
            {
                var v = original.GetType();
                bool isScrollbar = original.GetType().Equals(typeof(ScrollBar));
                //if ( bl == true )
                //{
                //    return true;
                //}
                if ( !isScrollbar . Equals ( typeof ( ScrollBar ) ) )
                {
                    if ( original . GetType ( ) . Equals ( typeof ( DataGrid ) ) )
                    {
                        //						Debug. WriteLine ( "DataGrid is clicked" );
                        return false;
                    }
                    //else if ( original . GetType ( ) . Equals ( typeof ( Grid ) ) )
                    //{
                    //    //						Debug. WriteLine ( "Pararaph clicked" );
                    //    //return true;
                    //}
                    else if ( original . GetType ( ) . Equals ( typeof ( Paragraph ) ) )
                    {
                        //						Debug. WriteLine ( "Pararaph clicked" );
                        return false;
                    }
                    else if ( original . GetType ( ) . Equals ( typeof ( Border ) ) )
                    {
                        //						Debug. WriteLine ( "Border clicked" );
                        return false;
                    }
                    else if ( FindVisualParent<ScrollBar> ( original as DependencyObject ) != null )
                    {
                        //scroll bar is clicked
                        //						Debug. WriteLine ( "Calling FindVisualParent" );
                        return true;
                    }
                    return false;
                }
                else
                    return true;
            }
            catch ( Exception ex )
            {
                //               Debug . WriteLine ( $"Error in HitTest ScriollBar Function (Utils-1520({ex . Data}" );
            }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            return true;
        }
        public static DependencyObject FindChild ( DependencyObject o , Type childType )
        {
            DependencyObject foundChild = null;
            if ( o != null )
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(o);
                for ( int i = 0 ; i < childrenCount ; i++ )
                {
                    var child = VisualTreeHelper.GetChild(o, i);
                    if ( child . GetType ( ) != childType )
                    {
                        foundChild = FindChild ( child , childType );
                        //if(foundChild == null)
                        //        FindChild ( child, childType );
                    }
                    else
                    {
                        foundChild = child;
                        break;
                    }
                }
            }
            return foundChild;
        }
        public static T FindChild<T> ( DependencyObject parent , string childName )
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if ( parent == null )
                return null;
            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for ( int i = 0 ; i < childrenCount ; i++ )
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if ( childType == null )
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T> ( child , childName );
                    // If the child is found, break so we do not overwrite the found child. 
                    if ( foundChild != null )
                        break;
                }
                else if ( !string . IsNullOrEmpty ( childName ) )
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if ( frameworkElement != null && frameworkElement . Name == childName )
                    {
                        // if the child's name is of the request name
                        foundChild = ( T ) child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = ( T ) child;
                    break;
                }
            }
            return foundChild;
        }
        public static T FindVisualChildByName<T> ( DependencyObject parent , string name ) where T : DependencyObject
        {
            for ( int i = 0 ; i < VisualTreeHelper . GetChildrenCount ( parent ) ; i++ )
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                string controlName = child.GetValue(Control.NameProperty) as string;
                if ( controlName == name )
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildByName<T>(child, name);
                    if ( result != null )
                        return result;
                }
            }
            return null;
        }
        public static IEnumerable<T> FindVisualChildren<T> ( DependencyObject depObj ) where T : DependencyObject
        {
            if ( depObj == null ) yield return ( T ) Enumerable . Empty<T> ( );
            for ( int i = 0 ; i < VisualTreeHelper . GetChildrenCount ( depObj ) ; i++ )
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if ( ithChild == null ) continue;
                if ( ithChild is T t ) yield return t;
                foreach ( T childOfChild in FindVisualChildren<T> ( ithChild ) ) yield return childOfChild;
            }
        }
        public static T FindVisualParent<T> ( UIElement element ) where T : UIElement
        {
            UIElement parent = element;
            while ( parent != null )
            {
                var correctlyTyped = parent as T;
                if ( correctlyTyped != null )
                {
                    return correctlyTyped;
                }
                parent = VisualTreeHelper . GetParent ( parent ) as UIElement;
            }
            return null;
        }
        public static parentItem FindVisualParent<parentItem> ( DependencyObject obj ) where parentItem : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while ( parent != null && !parent . GetType ( ) . Equals ( typeof ( parentItem ) ) )
            {
                parent = VisualTreeHelper . GetParent ( parent );
            }
            return parent as parentItem;
        }
        public static Brush GetDictionaryBrush ( string brushname )
        {
            Brush brs = null;
            try
            {
                brs = System . Windows . Application . Current . FindResource ( brushname ) as Brush;
            }
            catch
            {

            }
            return brs;
        }
        public static string GetExportFileName ( string filespec = "" )
        // opens  the common file open dialog
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd . InitialDirectory = @"C:\Users\ianch\Documents\";
            ofd . CheckFileExists = false;
            ofd . AddExtension = true;
            ofd . Title = "Select name for Exported data file.";
            if ( filespec . ToUpper ( ) . Contains ( "XL" ) )
                ofd . Filter = "Excel Spreadsheets (*.xl*) | *.xl*";
            else if ( filespec . ToUpper ( ) . Contains ( "CSV" ) )
                ofd . Filter = "Comma seperated data (*.csv) | *.csv";
            else if ( filespec . ToUpper ( ) . Contains ( "*.*" ) )
                ofd . Filter = "All Files (*.*) | *.*";
            if ( filespec . ToUpper ( ) . Contains ( "PNG" ) )
                ofd . Filter = "Image (*.png*) | *.pb*";
            else if ( filespec == "" )
            {
                ofd . Filter = "All Files (*.*) | *.*";
                ofd . DefaultExt = ".CSV";
            }
            ofd . FileName = filespec;
            ofd . ShowDialog ( );
            string fnameonly = ofd.SafeFileName;
            return ofd . FileName;
        }
        public static string GetImportFileName ( string filespec )
        // opens  the common file open dialog
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd . InitialDirectory = @"C:\Users\ianch\Documents\";
            ofd . CheckFileExists = true;
            if ( filespec . ToUpper ( ) . Contains ( "XL" ) )
                ofd . Filter = "Excel Spreadsheets (*.xl*) | *.xl*";
            else if ( filespec . ToUpper ( ) . Contains ( "CSV" ) )
                ofd . Filter = "Comma seperated data (*.csv) | *.csv";
            else if ( filespec . ToUpper ( ) . Contains ( "*.*" ) || filespec == "" )
                ofd . Filter = "All Files (*.*) | *.*";
            ofd . AddExtension = true;
            ofd . ShowDialog ( );
            return ofd . FileName;
        }
        public static Brush GetNewBrush ( string color )
        {
            if ( color == "" || color == "#Unknown" )
                return null;
            else
            {
                if ( color [ 0 ] != '#' )
                    color = "#" + color;
                return ( Brush ) new BrushConverter ( ) . ConvertFrom ( color );
            }
        }
        public static void GetWindowHandles ( )
        {
#if SHOWWINDOWDATA
            Debug. WriteLine ( $"Current Windows\r\n" + "===============" );
            foreach ( Window window in System . Windows . Application . Current . Windows )
            {
                if ( ( string ) window . Title != "" && ( string ) window . Content != "" )
                {
                    Debug. WriteLine ( $"Title:  {window . Title },\r\nContent - {window . Content}" );
                    Debug. WriteLine ( $"Name = [{window . Name}]\r\n" );
                }
            }
#endif
        }
        public static void Grab_MouseMove ( object sender , MouseEventArgs e )
        {
            Point pt = e.GetPosition((UIElement)sender);
            HitTestResult hit = VisualTreeHelper.HitTest((Visual)sender, pt);
            if ( hit?.VisualHit != null )
            {
                if ( ControlsHitList . Count != 0 )
                {
                    if ( hit . VisualHit == ControlsHitList [ 0 ] . VisualHit )
                        return;
                }
                ControlsHitList . Clear ( );
                ControlsHitList . Add ( hit );
            }
        }
        public static void Grab_Object ( object sender , Point pt )
        {
            //Point pt = e.GetPosition((UIElement)sender);
            HitTestResult hit = VisualTreeHelper.HitTest((Visual)sender, pt);
            if ( hit?.VisualHit != null )
            {
                if ( ControlsHitList . Count != 0 )
                {
                    if ( hit . VisualHit == ControlsHitList [ 0 ] . VisualHit )
                        return;
                }
                ControlsHitList . Clear ( );
                ControlsHitList . Add ( hit );
            }
        }
        public static bool HitTestBorder ( object sender , MouseButtonEventArgs e )
        {
            object original = e.OriginalSource;
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            try
            {
                var v = original.GetType();
                if ( original . GetType ( ) . Equals ( typeof ( Border ) ) )
                {
                    //                Debug. WriteLine ( "Border clicked" );
                    //					Mouse . SetCursor ( Cursors . SizeAll );
                    return true;
                }
                Type type = original.GetType();
                if ( type . Equals ( typeof ( TextBlock ) ) )
                {
                    //                    Debug. WriteLine ( "TextBlock clicked" );
                    return false;
                }
                if ( type . Equals ( typeof ( Grid ) ) )
                {
                    //                  Debug. WriteLine ( "Grid clicked" );
                    return false;
                }
                if ( type . Equals ( typeof ( TreeViewItem ) ) )
                {
                    //                  Debug. WriteLine ( "Grid clicked" );
                    return false;
                }
                else if ( FindVisualParent<Border> ( original as DependencyObject ) != null )
                {
                    //scroll bar is clicked
                    Debug . WriteLine ( "Calling FindVisualParent" );
                    return true;
                }
                return false;
                //}
            }
            catch ( Exception ex )
            {
                //              Debug . WriteLine ( $"Error in HitTest ScriollBar Function (Utils-1520({ex . Data}" );
                return false;
            }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            //		return true;
        }
        public static bool HitTestTreeViewItem ( object sender , MouseButtonEventArgs e )
        {
            TreeView tv = sender as TreeView;
            object original = e.OriginalSource;
            var vv = e.Source;
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            try
            {
                var v = original.GetType();
                if ( original . GetType ( ) . Equals ( typeof ( Border ) ) )
                {
                    //                Debug. WriteLine ( "Border clicked" );
                    //					Mouse . SetCursor ( Cursors . SizeAll );
                    return true;
                }
                Type type = original.GetType();
                if ( type . Equals ( typeof ( System . Windows . Shapes . Path ) ) )
                {
                    //                  Debug. WriteLine ( "Grid clicked" );
                    //InputElement dropNode = tv . InputHitTest ( (Point)tv.GetPosition(tv).);
                    return false;
                }
                else if ( FindVisualParent<Border> ( original as DependencyObject ) != null )
                {
                    //scroll bar is clicked
                    Debug . WriteLine ( "Calling FindVisualParent" );
                    return true;
                }
                return false;
                //}
            }
            catch ( Exception ex )
            {
                //              Debug . WriteLine ( $"Error in HitTest ScriollBar Function (Utils-1520({ex . Data}" );
                return false;
            }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            //		return true;
        }
        private static string ParseforCR ( string input )
        {
            string output = "";
            if ( input . Length == 0 )
                return input;
            if ( input . Contains ( "\r\n" ) )
            {
                do
                {
                    string[] fields = input.Split('\r');
                    foreach ( var item in fields )
                    {
                        output += item;
                    }
                    if ( output . Contains ( "\r" ) == false )
                        break;
                } while ( true );
            }
            else
                return input;
            return output;
        }
        // allows any image to be saved as PNG/GIF/JPG format, defaullt is PNG
        public static void SaveImageToFile ( RenderTargetBitmap bmp , string file , string imagetype = "PNG" )
        {
            string[] items;
            // Make a PNG encoder.
            if ( bmp == null )
                return;
            if ( file == "" && imagetype != "" )
                file = @"c:\users\ianch\pictures\defaultimage";
            items = file . Split ( '.' );
            file = items [ 0 ];
            if ( imagetype == "PNG" )
                file += ".png";
            else if ( imagetype == "GIF" )
                file += ".gif";
            else if ( imagetype == "JPG" )
                file += ".jpg";
            else if ( imagetype == "BMP" )
                file += ".bmp";
            else if ( imagetype == "TIF" )
                file += ".tif";
            try
            {
                using ( FileStream fs = new FileStream ( file ,
                            FileMode . Create , FileAccess . Write , FileShare . ReadWrite ) )
                {
                    if ( imagetype == "PNG" )
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    else if ( imagetype == "GIF" )
                    {
                        GifBitmapEncoder encoder = new GifBitmapEncoder();
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    else if ( imagetype == "JPG" || imagetype == "JPEG" )
                    {
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    else if ( imagetype == "BMP" )
                    {
                        BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    else if ( imagetype == "TIF" || imagetype == "TIFF" )
                    {
                        TiffBitmapEncoder encoder = new TiffBitmapEncoder();
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    fs . Close ( );
                }
            }
            catch ( Exception ex )
            {
                //TODO
                //    Utils . Mbox ( null , string1: "The image could not be saved for the following reason " , string2: $"{ex . Message}" , caption: "" , iconstring: "\\icons\\Information.png" , Btn1: MB . OK , Btn2: MB . NNULL , defButton: MB . OK );
            }
        }
        public static void ScrollLBRecordIntoView ( ListBox lbox , int CurrentRecord )
        {
            // Works well 26/5/21

            //update and scroll to bottom first
            //lbox . SelectedIndex = ( int ) CurrentRecord;
            //lbox . SelectedItem = ( int ) CurrentRecord;
            //            lbox . UpdateLayout ( );
            lbox . ScrollIntoView ( lbox . SelectedIndex );
            lbox . UpdateLayout ( );
            lbox . ScrollIntoView ( lbox . SelectedItem );
            lbox . UpdateLayout ( );
        }
        public static void ScrollLVRecordIntoView ( ListView Dgrid , int CurrentRecord )
        {
            // Works well 26/5/21

            //update and scroll to bottom first
            Dgrid . SelectedIndex = ( int ) CurrentRecord;
            Dgrid . SelectedItem = ( int ) CurrentRecord;
            //            Dgrid . UpdateLayout ( );
            Dgrid . ScrollIntoView ( Dgrid . SelectedItem );
            Dgrid . UpdateLayout ( );
            //            Dgrid . ScrollIntoView ( Dgrid . SelectedItem );
            //            Dgrid . UpdateLayout ( );
        }
        public static void ScrollRecordIntoView ( DataGrid Dgrid , int CurrentRecord , object row = null )
        {
            // Works well 26/5/21
            Dgrid . ScrollIntoView ( Dgrid . Items . Count - 1 );
            Dgrid . SelectedIndex = CurrentRecord;
            Dgrid . SelectedItem = CurrentRecord;
            Dgrid . UpdateLayout ( );
            Dgrid . BringIntoView ( );
            Dgrid . ScrollIntoView ( Dgrid . Items [ Dgrid . Items . Count - 1 ] );
            Dgrid . UpdateLayout ( );
            Dgrid . ScrollIntoView ( row ?? Dgrid . SelectedItem );
            Dgrid . UpdateLayout ( );
            //                Dgrid . ScrollIntoView ( row == null ? Dgrid . SelectedItem : row );
            //Dgrid . ScrollIntoView ( Dgrid . SelectedIndex );
            //ScrollRowInGrid ( Dgrid , Convert.ToInt16(row) );
            //            Dgrid . UpdateLayout();
            //if ( CurrentRecord == 0 )
            //    Debug. WriteLine($"DataGrid Scroll is selecting record ZERO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //Dgrid . Refresh ( );
            //} );
        }

        static public DataGridRow GetRow ( DataGrid dg , int index )
        {
            DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(index);
            if ( row == null )
            {
                // may be virtualized, bring into view and try again
                dg . ScrollIntoView ( dg . Items [ index ] );
                row = ( DataGridRow ) dg . ItemContainerGenerator . ContainerFromIndex ( index );
            }
            return row;
        }

        // Method that quickly & easily makes an entire window draggable from Left mouse down in all Non Control areas
        public static void SetupWindowDrag ( Window inst )
        {
            try
            {
                //Handle the button NOT being the left mouse button
                // which will crash the DragMove Fn.....
                MouseButtonState mbs = Mouse.RightButton;
                //Debug. WriteLine ( $"{mbs . ToString ( )}" );
                if ( mbs == MouseButtonState . Pressed )
                    return;
                inst . MouseDown += delegate
                {
                    {
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                        try
                        {
                            inst?.DragMove ( );
                        }
                        catch ( Exception ex )
                        {
                            return;
                        }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                    }
                };
            }
            catch ( Exception ex )
            {
                return;
            }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
        }
        public static void SetUpGridSelection ( DataGrid grid , int row = 0 )
        {
            //			bool inprogress = false;
            //			int scrollrow = 0;
            if ( row == -1 )
                row = 0;
            // This triggers the selection changed event
            grid . SelectedIndex = row;
            grid . SelectedItem = row;
            //			grid . SetDetailsVisibilityForItem ( grid . SelectedItem, Visibility . Visible );
            grid . SelectedIndex = row;
            grid . SelectedItem = row;
            Utils . ScrollRecordIntoView ( grid , row );
            grid . ScrollIntoView ( grid . SelectedItem );
            grid . UpdateLayout ( );
            //grid . Refresh ( );
            //			var v = grid .VerticalAlignment;
        }

        public static void SwitchMagnifyStyle ( DataGrid dGrid , ref TextBlock info , bool updateText = true )
        {
            // Toggle magnification of DataGrid thru from 4 > 0 and return to 4
            if ( info . Text == "+4" )
            {
                if ( updateText )
                {
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation3" );
                    info . Text = "+3";
                }
                else
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation4" );
            }
            else if ( info . Text == "+3" )
            {
                if ( updateText )
                {
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation2" );
                    info . Text = "+2";
                }
                else
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation3" );
            }
            else if ( info . Text == "+2" )
            {
                if ( updateText )
                {
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation1" );
                    info . Text = "+1";
                }
                else
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation2" );
            }
            else if ( info . Text == "+1" )
            {
                if ( updateText )
                {
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation0" );
                    info . Text = "0";
                }
                else
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation1" );
            }
            else if ( info . Text == "0" )
            {
                if ( updateText )
                {
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation4" );
                    info . Text = "+4";
                }
                else
                    dGrid . Style = ( Style ) Application . Current . FindResource ( "DatagridMagnifyAnimation0" );
            }
        }
        public static void SwitchMagnifyStyle ( ListBox lbox , ref TextBlock info , bool updateText = true )
        {
            // Toggle magnification of DataGrid thru from 4 > 0 and return to 4
            if ( info . Text == "+4" )
            {
                if ( updateText )
                {
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation3" );
                    info . Text = "+3";
                }
                else
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation4" );
            }
            else if ( info . Text == "+3" )
            {
                if ( updateText )
                {
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation2" );
                    info . Text = "+2";
                }
                else
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation3" );
            }
            else if ( info . Text == "+2" )
            {
                if ( updateText )
                {
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation1" );
                    info . Text = "+1";
                }
                else
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation2" );
            }
            else if ( info . Text == "+1" )
            {
                if ( updateText )
                {
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation0" );
                    info . Text = "0";
                }
                else
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation1" );
            }
            else if ( info . Text == "0" )
            {
                if ( updateText )
                {
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation4" );
                    info . Text = "+4";
                }
                else
                    lbox . Style = ( Style ) Application . Current . FindResource ( "ListBoxMagnifyAnimation0" );
            }
        }
        public static void SwitchMagnifyStyle ( ListView lview , ref TextBlock info , bool updateText = true )
        {
            // Toggle magnification of DataGrid thru from 4 > 0 and return to 4
            if ( info . Text == "+4" )
            {
                if ( updateText )
                {
                    lview . Style = ( Style ) Application . Current . FindResource ( "ListViewMagnifyAnimation3" );
                    info . Text = "+3";
                }
                else
                    lview . Style = ( Style ) Application . Current . FindResource ( "ListViewMagnifyAnimation4" );
            }
            else if ( info . Text == "+3" )
            {
                if ( updateText )
                {
                    lview . Style = ( Style ) Application . Current . FindResource ( "ListViewMagnifyAnimation2" );
                    info . Text = "+2";
                }
                else
                    lview . Style = ( Style ) Application . Current . FindResource ( "ListViewMagnifyAnimation3" );
            }
            else if ( info . Text == "+2" )
            {
                if ( updateText )
                {
                    lview . Style = ( Style ) Application . Current . FindResource ( "ListViewMagnifyAnimation1" );
                    info . Text = "+1";
                }
                else
                    lview . Style = ( Style ) System . Windows . Application . Current . FindResource ( "ListViewMagnifyAnimation2" );
            }
            else if ( info . Text == "+1" )
            {
                if ( updateText )
                {
                    lview . Style = ( Style ) Application . Current . FindResource ( "ListViewMagnifyAnimation0" );
                    info . Text = "0";
                }
                else
                    lview . Style = ( Style ) Application . Current . FindResource ( "ListViewMagnifyAnimation1" );
            }
            else if ( info . Text == "0" )
            {
                if ( updateText )
                {
                    lview . Style = ( Style ) Application . Current . FindResource ( "ListViewMagnifyAnimation4" );
                    info . Text = "+4";
                }
                else
                    lview . Style = ( Style ) Application . Current . FindResource ( "ListViewMagnifyAnimation0" );
            }
        }
        public static void SwitchMagnifyStyle ( ComboBox cb , ref TextBlock info , bool updateText = true )
        {
            // Toggle magnification of DataGrid thru from 4 > 0 and return to 4
            if ( info . Text == "+4" )
            {
                if ( updateText )
                {
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation3" );
                    info . Text = "+3";
                }
                else
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation4" );
            }
            else if ( info . Text == "+3" )
            {
                if ( updateText )
                {
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation2" );
                    info . Text = "+2";
                }
                else
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation3" );
            }
            else if ( info . Text == "+2" )
            {
                if ( updateText )
                {
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation1" );
                    info . Text = "+1";
                }
                else
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation2" );
            }
            else if ( info . Text == "+1" )
            {
                if ( updateText )
                {
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation0" );
                    info . Text = "+0";
                }
                else
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation1" );
            }
            else if ( info . Text == "0" )
            {
                if ( updateText )
                {
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation4" );
                    info . Text = "+4";
                }
                else
                    cb . Style = ( Style ) Application . Current . FindResource ( "ComboBoxMagnifyAnimation0" );
            }
        }

        /// <summary>
        /// A Generic data reader for any ObservableCollection&lt;T&gt; type
        /// </summary>
        ///<example>
        ///Call Method using loop such as :
        ///<code>
        ///<c>foreach ( int item in Utils. ReadGenericCollection ( <paramref name="collection"/> intcollection) )</c>
        ///</code>
        /// </example>
        /// or any similar looping construct
        /// <typeparam name="T">Any Generic Observable Collection</typeparam>
        /// <param name="collection"/>
        /// <returns>Individual records via yield return system to return items on demand, or NULL if collection cannot provide an Iterator 
        /// </returns>
        public static IEnumerable ReadGenericCollection<T> ( ObservableCollection<T> collection , IEnumerator ie = null )
        {
            // Generic method to supply content of ANY Observablecollection<> type
            // Call it by a call such as  :-
            //  foreach ( BankCollection item in Utils.GenericRead ( BankCollection ) )
            //      {Debug. WriteLine ( item );}
            //or
            //  foreach ( int item in Utils.GenericRead ( integerCollection ) )
            //      {Debug. WriteLine ( item );}
            if ( collection . Count > 0 )
            {
                ie = collection . GetEnumerator ( );
                if ( ie != null )
                {
                    foreach ( var item in collection )
                    {

                        if ( ie . MoveNext ( ) )
                            yield return item;
                    }
                }
            }
        }
        public static StringBuilder ReadFileGeneric ( string path , ref StringBuilder sb )
        {
            string s = File.ReadAllText(path);
            sb . Append ( s );
            return sb;
        }
        public static string ReadFileGeneric ( string path , ref string sb )
        {
            sb = File . ReadAllText ( path );
            return sb;
        }
        public static bool WriteFileGeneric ( string path , string data )
        {
            File . WriteAllText ( path , data );
            return true;
        }
        public static bool WriteFileGeneric ( string path , StringBuilder data )
        {
            File . WriteAllText ( path , data . ToString ( ) );
            return true;
        }

        public static List<object> GetChildControls ( UIElement parent , string TypeRequired )
        {
            // this uses  the TabControlHelper class
            UIElement element = new UIElement();
            List<object> objects = new List<object>();
            //IEnumerable alltabcontrols = null;
            //TODO
            //if ( TypeRequired == "*" )
            //    alltabcontrols = TabControlHelper . FindChildren<UIElement> ( parent );
            //else if ( TypeRequired == "Button" )
            //    alltabcontrols = TabControlHelper . FindChildren<Button> ( parent );
            //else if ( TypeRequired == "TextBlock" )
            //    alltabcontrols = TabControlHelper . FindChildren<TextBlock> ( parent );
            //else if ( TypeRequired == "DataGrid" )
            //    alltabcontrols = TabControlHelper . FindChildren<DataGrid> ( parent );
            //else if ( TypeRequired == "ListBox" )
            //    alltabcontrols = TabControlHelper . FindChildren<ListBox> ( parent );
            //else if ( TypeRequired == "ListView" )
            //    alltabcontrols = TabControlHelper . FindChildren<ListView> ( parent );
            //else if ( TypeRequired == "TextBox" )
            //    alltabcontrols = TabControlHelper . FindChildren<TextBox> ( parent );
            ////else if ( TypeRequired == "WrapPanel" )
            ////    alltabcontrols = TabControlHelper . FindChildren<WrapPanel> ( parent );
            //else if ( TypeRequired == "Border" )
            //    alltabcontrols = TabControlHelper . FindChildren<Border> ( parent );
            //else if ( TypeRequired == "Slider" )
            //    alltabcontrols = TabControlHelper . FindChildren<Slider> ( parent );
            //else if ( TypeRequired == "TabControl" )
            //    alltabcontrols = TabControlHelper . FindChildren<TabControl> ( parent );
            //else if ( TypeRequired == "TabItem" )
            //    alltabcontrols = TabControlHelper . FindChildren<TabItem> ( parent );
            //else if ( TypeRequired == "" )
            //    alltabcontrols = TabControlHelper . FindChildren<UIElement> ( parent );
            //if ( alltabcontrols != null )
            //{
            //    int count = 0;
            //    IEnumerator enumerator = alltabcontrols . GetEnumerator();
            //    try
            //    {
            //        while ( enumerator . MoveNext() )
            //        {
            //            count++;
            //            var v = enumerator . Current;
            //            objects . Add(v);
            //        }
            //    } finally
            //    {
            //        Debug . WriteLine($"Found {count} controls of  type {TypeRequired}");
            //    }
            //}
            //Debug . WriteLine("Finished FindChildren() 4\n");

            return objects;
        }
        #region ZERO referennces
        public static string convertToHex ( double temp )
        {
            int intval = (int)Convert.ToInt32(temp);
            string hexval = intval.ToString("X");
            return hexval;
        }
        //Working well 4/8/21
        /// <summary>
        /// Accepts color in Colors.xxxx format = "Blue" etc
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        /// <summary>
        /// Accpets string in "#XX00FF00" or similar
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Brush BrushFromHashString ( string color )
        {
            //Must start with  '#'
            string s = color.ToString();
            if ( !s . Contains ( "#" ) )
                return Utils . BrushFromColors ( Colors . Transparent );
            Brush brush = (Brush)new BrushConverter().ConvertFromString(color);
            return brush;
        }
        public static bool FindWindowFromTitle ( string searchterm , ref Window handle )
        {
            bool result = false;
            foreach ( Window window in Application . Current . Windows )
            {
                if ( window . Title . ToUpper ( ) . Contains ( searchterm . ToUpper ( ) ) )
                {
                    handle = window;
                    result = true;
                    break;
                }
            }
            return result;
        }
        public static Brush GetBrush ( string parameter )
        {
            if ( parameter == "BLUE" )
                return Brushes . Blue;
            else if ( parameter == "RED" )
                return Brushes . Red;
            else if ( parameter == "GREEN" )
                return Brushes . Green;
            else if ( parameter == "CYAN" )
                return Brushes . Cyan;
            else if ( parameter == "MAGENTA" )
                return Brushes . Magenta;
            else if ( parameter == "YELLOW" )
                return Brushes . Yellow;
            else if ( parameter == "WHITE" )
                return Brushes . White;
            else
            {
                //We appear to have received a Brushes Resource Name, so return that Brushes value
                Brush b = (Brush)Utils.GetDictionaryBrush(parameter.ToString());
                return b;
            }
        }
        public static Brush GetBrushFromInt ( int value )
        {
            Brush brush;
            brush = value switch
            {
                0 => Brushes . White,
                1 => Brushes . Yellow,
                2 => Brushes . Orange,
                3 => Brushes . Red,
                4 => Brushes . Magenta,
                5 => Brushes . Gray,
                6 => Brushes . Aqua,
                7 => Brushes . Azure,
                8 => Brushes . Brown,
                9 => Brushes . Crimson,
                _ => Brushes . Transparent
            };
            return brush; 
        }
        public static string GetPrettyGridStatistics ( DataGrid Grid , int current )
        {
            string output = "";
            if ( current != -1 )
                output = $"{current} / {Grid . Items . Count}";
            else
                output = $"0 / {Grid . Items . Count}";
            return output;
        }
        public static ControlTemplate GetDictionaryControlTemplate ( string tempname )
        {
            ControlTemplate ctmp = System.Windows.Application.Current.FindResource(tempname) as ControlTemplate;
            return ctmp;
        }
        public static Style GetDictionaryStyle ( string tempname )
        {
            Style ctmp = System.Windows.Application.Current.FindResource(tempname) as Style;
            return ctmp;
        }
        public static object GetTemplateControl ( Control RectBtn , string CtrlName )
        {
            var template = RectBtn.Template;
            object v = template.FindName(CtrlName, RectBtn) as object;
            return v;
        }
        public static void ReadAllConfigSettings ( )
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if ( appSettings . Count == 0 )
                {
                    Debug . WriteLine ( "AppSettings is empty." );
                }
                else
                {
                    foreach ( var key in appSettings . AllKeys )
                    {
                        Debug . WriteLine ( "Key: {0} Value: {1}" , key , appSettings [ key ] );
                    }
                }
            }
            catch ( ConfigurationErrorsException )
            {
                Debug . WriteLine ( "Error reading app settings" );
            }
        }
        public static void HandleCtrlFnKeys ( bool key1 , KeyEventArgs e )
        {
            if ( key1 && e . Key == Key . F5 )
            {
                // list Flags in Console
                Utils . GetWindowHandles ( );
                e . Handled = true;
                key1 = false;
                return;
            }
            else if ( key1 && e . Key == Key . F7 )  // CTRL + F7
            {
                // list various Flags in Console
                //Debug . WriteLine ( $"\nCTRL + F7 pressed..." );
                //TRANSFER				Flags . PrintDbInfo ( );
                e . Handled = true;
                key1 = false;
                return;
            }
            else if ( key1 && e . Key == Key . F9 )     // CTRL + F9
            {
                //Debug . WriteLine ( "\nCtrl + F9 NOT Implemented" );
                key1 = false;
                return;

            }
            else if ( key1 && e . Key == Key . System )     // CTRL + F10
            {
                // Major  listof GV[] variables (Guids etc]
                //Debug . WriteLine ( $"\nCTRL + F10 pressed..." );
                //TRANSFER				Flags . ListGridviewControlFlags ( 1 );
                key1 = false;
                e . Handled = true;
                return;
            }
            else if ( key1 && e . Key == Key . F11 )  // CTRL + F11
            {
                // list various Flags in Console
                //Debug . WriteLine ( $"\nCTRL + F11 pressed..." );
                //				Flags . PrintSundryVariables ( );
                e . Handled = true;
                key1 = false;
                return;
            }
        }
        public static string ReadConfigSetting ( string key )
        {
            Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

            string result = "";
            try
            {
                //var appSettings = ConfigurationManager . AppSettings;
                result = ( string ) defaultInstance [ key ] ?? "Not Found";
                Debug . WriteLine ( result );
            }
            catch ( ConfigurationErrorsException )
            {
                Debug . WriteLine ( "Error reading app settings" );
            }
            return result;
        }
        /// <summary>
        /// Creates a BMP from any control passed into it   ???
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static RenderTargetBitmap RenderBitmap ( Visual element , double objwidth = 0 , double objheight = 0 , string filename = "" , bool savetodisk = false )
        {
            double topLeft = 0;
            double topRight = 0;
            int width = 0;
            int height = 0;

            if ( element == null )
                return null;
            Rect bounds = VisualTreeHelper.GetDescendantBounds(element);
            if ( objwidth == 0 )
                width = ( int ) bounds . Width;
            if ( objheight == 0 )
                height = ( int ) bounds . Height;
            double dpiX = 96; // this is the magic number
            double dpiY = 96; // this is the magic number

            PixelFormat pixelFormat = PixelFormats.Default;
            VisualBrush elementBrush = new VisualBrush(element);
            DrawingVisual visual = new DrawingVisual();
            DrawingContext dc = visual.RenderOpen();

            dc . DrawRectangle ( elementBrush , null , new Rect ( topLeft , topRight , width , height ) );
            dc . Close ( );
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)(bounds.Width * dpiX / 96.0), (int)(bounds.Height * dpiY / 96.0), dpiX, dpiY, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using ( DrawingContext ctx = dv . RenderOpen ( ) )
            {
                VisualBrush vb = new VisualBrush(element);
                ctx . DrawRectangle ( vb , null , new Rect ( new Point ( ) , bounds . Size ) );
            }
            rtb . Render ( dv );

            if ( savetodisk && filename != "" )
                SaveImageToFile ( rtb , filename );
            return rtb;
        }
        public static void SaveProperty ( string setting , string value )
        {
            Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

            try
            {
                if ( value . ToUpper ( ) . Contains ( "TRUE" ) )
                    defaultInstance [ setting ] = true;
                //Properties . Settings . Default [ setting ] = true;
                else if ( value . ToUpper ( ) . Contains ( "FALSE" ) )
                    defaultInstance [ setting ] = false;
                else
                    defaultInstance [ setting ] = value;
                defaultInstance . Save ( );
                Properties . Settings . Default . Upgrade ( );
                ConfigurationManager . RefreshSection ( setting );
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"Unable to save property {setting} of [{value}]\nError was {ex . Data}, {ex . Message}, Stack trace = \n{ex . StackTrace}" );
            }
        }
        public static void SelectTextBoxText ( TextBox txtbox )
        {
            txtbox . SelectionLength = txtbox . Text . Length;
            txtbox . SelectionStart = 0;
            txtbox . SelectAll ( );
        }
        public static bool HitTestHeaderBar ( object sender , MouseButtonEventArgs e )
        {
            //			HitTestResult hit = VisualTreeHelper . HitTest ( ( Visual ) sender, e . GetPosition ( ( InputElement ) sender ) );
            //			return hit . VisualHit . GetVisualAncestor<ScrollBar> ( ) != null;
            object original = e.OriginalSource;

            if ( !original . GetType ( ) . Equals ( typeof ( DataGridColumnHeader ) ) )
            {
                if ( original . GetType ( ) . Equals ( typeof ( DataGrid ) ) )
                {
                    Debug . WriteLine ( "DataGrid is clicked" );
                }
                else if ( FindVisualParent<DataGridColumnHeader> ( original as DependencyObject ) != null )
                {
                    //Header bar is clicked
                    return true;
                }
                return false;
            }
            return true;
        }
        public static void SetSelectedItemFirstRow ( object dataGrid , object selectedItem )
        {
            //If target datagrid Empty, throw exception
            if ( dataGrid == null )
            {
                throw new ArgumentNullException ( "Target none" + dataGrid + "Cannot convert to DataGrid" );
            }
            //Get target DataGrid，If it is empty, an exception will be thrown
            System.Windows.Controls.DataGrid dg = dataGrid as System.Windows.Controls.DataGrid;
            if ( dg == null )
            {
                throw new ArgumentNullException ( "Target none" + dataGrid + "Cannot convert to DataGrid" );
            }
            //If the data source is empty, return
            if ( dg . Items == null || dg . Items . Count < 1 )
            {
                return;
            }

            dg . SelectedItem = selectedItem;
            dg . CurrentColumn = dg . Columns [ 0 ];
            dg . ScrollIntoView ( dg . SelectedItem );
        }
        public static void SetUpGListboxSelection ( ListBox grid , int row = 0 )
        {
            if ( row == -1 )
                row = 0;
            // This triggers the selection changed event
            grid . SelectedIndex = row;
            grid . SelectedItem = row;
            Utils . ScrollLBRecordIntoView ( grid , row );
            grid . UpdateLayout ( );
            //			var v = grid .VerticalAlignment;
        }
        /// <summary>
        /// MASTER UPDATE METHOD
        /// This handles repositioning of a selected item in any grid perfectly
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="row"></param>
        /// <summary>
        /// Metohd that almost GUARANTESS ot force a record into view in any DataGrid
        /// /// This is called by method above - MASTER Updater Method
        /// </summary>
        /// <param name="dGrid"></param>
        /// <param name="row"></param>
        public static void ScrollRowInGrid ( DataGrid dGrid , int row )
        {
            if ( dGrid . SelectedItem == null ) return;

            dGrid . ScrollIntoView ( dGrid . SelectedItem );
            dGrid . UpdateLayout ( );
            //           Utils . ScrollRecordIntoView ( dGrid , row );
            //            dGrid . UpdateLayout ( );
        }
        //********************************************************************************************************************************************************************************//
        public static void NewCookie_Click ( object sender , RoutedEventArgs e )
        //********************************************************************************************************************************************************************************//
        {
            //NewCookie nc = new NewCookie(sender as Window);
            //nc . ShowDialog ( );
            //defvars . CookieAdded = false;
        }
        public static bool IsMousRightBtnDn ( object sender , MouseEventArgs e )
        {
            e . Handled = true;
            if ( e . RightButton == MouseButtonState . Pressed )
                return true;
            return false;
        }
        public static void TrackSplitterPosition ( TextBlock Textblock , double MaxWidth , DragDeltaEventArgs e )
        {
            Thickness th = new Thickness(0, 0, 0, 0);
            th = Textblock . Margin;
            if ( th . Left < MaxWidth )
            {
                th . Left += e . HorizontalChange;
                if ( th . Left > 10 )
                    Textblock . Margin = th;
            }
        }

        #endregion ZERO referennces
        public static bool IsReferenceEqual ( object a , object b , string astr = "" , string bstr = "" , bool output = false )
        {
            bool result = ReferenceEquals(a, b);
            if ( output )
            {
                if ( result )
                {
                    Debug . WriteLine ( $"Ref Equals : TRUE\n    [{astr}] \n-v- [{bstr}] \nBoth are Type [{a . GetType ( ) . ToString ( )}]\n" );
                }
                else
                {
                    Debug . WriteLine ( $"Ref Equals : FALSE\n    [{astr}] \n-v- [{bstr}] \n1st is {a . GetType ( ) . ToString ( )}\n2nd is {b . GetType ( ) . ToString ( )}\n" );
                }
            }
            return result;
        }
        public static bool IsHashEqual ( object a , object b , string astr = "" , string bstr = "" , bool output = false )
        {
            bool result = a.GetHashCode() == b.GetHashCode();
            if ( output )
            {
                if ( result )
                {
                    Debug . WriteLine ( $"Hash Equals : TRUE\n    [{astr}] \n-v- [{bstr}] \nBoth are Type [{a . GetHashCode ( ) . ToString ( )}]\n" );
                }
                else
                {
                    Debug . WriteLine ( $"Hash Equals : FALSE\n    [{astr}] \n-v- [{bstr}] \n1st is {a . GetHashCode ( ) . ToString ( )}\n2nd is {b . GetHashCode ( ) . ToString ( )}\n" );
                }
            }
            return result;
        }

        public static bool WriteSerializedCollectionJSON ( object obj , string file = "" )
        {
            //Writes any linear style object as a JSON file (Observable collection works fine)
            // Doesnt handle Datagrids or UserControl etc
            //Create JSON String
            if ( file == "" )
                file = "DefaultJsonText.json";
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, IncludeFields = true, MaxDepth = 12 };
                string jsonString = System.Text.Json.JsonSerializer.Serialize<object>(obj, options);
                // Save JSON file to disk 
                XmlSerializer mySerializer = new XmlSerializer(typeof(string));
                StreamWriter myWriter = new StreamWriter(file);
                mySerializer . Serialize ( myWriter , jsonString );
                myWriter . Close ( );
                return true;
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"Serialization FAILED :[{ex . Message}]" );
            }
            return false;
        }
        public static string ReadSerializedCollectionJson ( string file )
        {
            string fileName = file;

            string data = "";
            string myPath = file;
            XmlSerializer s = new XmlSerializer(typeof(string));
            StreamReader streamReader = new StreamReader(file);
            data = ( string ) s . Deserialize ( streamReader );
            JsonTextReader reader = new JsonTextReader(new StringReader(data));
            // create Sb for speed......
            StringBuilder sb = new StringBuilder();
            int index = 0;
            while ( reader . Read ( ) )
            {
                string strg = String.Format("{0}", reader.Value);
                string[] st = strg.Split(',');
                if ( index < 1 && st [ 0 ] . Length > 0 )
                {
                    sb . Append ( String . Format ( "{0}," , st [ 0 ] . Trim ( ) ) );
                    index++;
                }
                else if ( st [ 0 ] . Length > 0 )
                {
                    sb . Append ( String . Format ( "{0}\n" , st [ 0 ] . Trim ( ) ) );
                    index = 0;
                }

            }
            //data is formatted  as "xxx, yyyyy\n"
            return sb . ToString ( );
        }
    }

}
