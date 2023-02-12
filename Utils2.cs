using System;
using System . Collections;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Configuration;
using System . Data;
using System . Diagnostics;
using System . IO;
using System . Reflection;
using System . Runtime . InteropServices;
using System . Runtime . Serialization . Formatters . Binary;
using System . Text;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Controls . Primitives;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Navigation;
using System . Windows . Shapes;

using Microsoft . Data . SqlClient;
using Microsoft . Win32;

using ViewModels;

using Wpfmain . SqlBasicSupport;



using Brush = System . Windows . Media . Brush;

namespace Wpfmain
{
    /// <summary>
    /// Class to handle various utility functions such as fetching 
    /// Style/Templates/Brushes etc to Set/Reset control styles 
    /// from various Dictionary sources for use in "code behind"
    /// </summary>
    public class Utils2
    {
        //public static Action<DataGrid , int> GridInitialSetup = WpfLib1Utils . SetUpGridSelection;
        ////		public static Func<bool, BankAccountViewModel, CustomerViewModel, DetailsViewModel> IsMatched = CheckRecordMatch; 
        //public static Func<object , object , bool> IsRecordMatched = Utils . CompareDbRecords;

        // list each window that wants to support control capture needs to have so
        // mousemove can add current item under cursor to the list, and then F11 will display it.
        public static List<HitTestResult> ControlsHitList = new List<HitTestResult> ( );

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
            public Note ( Tone frequency, Duration time )
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
            Note [ ] Mary =
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
                    Console . Beep ( ( int ) n . NoteTone, ( int ) n . NoteDuration );
            }
        }

        #endregion play tunes 

        #region play beeps

        public static void DoSingleBeep ( int freq = 280, int count = 300, int repeat = 1 )
        {
            if ( Flags . UseBeeps )
            {
                for ( int i = 0 ; i < repeat ; i++ )
                {
                    Console . Beep ( freq, count );
                }
            }
        }
        public static void DoErrorBeep ( int freq = 280, int count = 100, int repeat = 1 )
        {
            List<Task<bool>> list = new List<Task<bool>> ( );
            list . Add ( Utils2 . Dobeep ( 280, 100 ) );
            list . Add ( Utils2 . Dobeep ( 200, 500 ) );
            return;
        }
        public static void DoLongSuccessBeep ( int beeptype = 4, int repeat = 1 )
        {
            if ( Flags . UseBeeps )
            {
                // Need to do it this way to avoid delays bewtween beeps
                List<Task<bool>> list = new List<Task<bool>> ( );
                if ( beeptype == 4 )
                {
                    list . Add ( Utils2 . Dobeep ( 340, 150 ) );
                    list . Add ( Utils2 . Dobeep ( 430, 150 ) );
                    list . Add ( Utils2 . Dobeep ( 340, 150 ) );
                    list . Add ( Utils2 . Dobeep ( 440, 550 ) );
                }
                else if ( beeptype == 2 )
                {
                    //					while (true){
                    list . Add ( Utils2 . Dobeep ( 240, 150 ) );
                    //	list . Add ( Utils2 . Dobeep ( 340, 150 ) );
                    list . Add ( Utils2 . Dobeep ( 360, 450 ) );
                    //					}
                }
            }
            return;
        }
        public static void DoSuccessBeep ( int beeptype = 4, int repeat = 1 )
        {
            if ( Flags . UseBeeps )
            {
                // Need to do it this way to avoid delays bewtween beeps
                List<Task<bool>> list = new List<Task<bool>> ( );
                if ( beeptype == 4 )
                {
                    list . Add ( Utils2 . Dobeep ( 340, 50 ) );
                    list . Add ( Utils2 . Dobeep ( 430, 50 ) );
                    list . Add ( Utils2 . Dobeep ( 340, 50 ) );
                    list . Add ( Utils2 . Dobeep ( 440, 250 ) );
                }
                //else if ( beeptype == 2 )
                //{
                //    //					while (true){
                //    list . Add ( Utils2 . Dobeep ( 240, 150 ) );
                //    //	list . Add ( Utils2 . Dobeep ( 340, 150 ) );
                //    list . Add ( Utils2 . Dobeep ( 360, 450 ) );
                //    //					}
                //}
            }
            return;
        }
        public static void PlayErrorBeep ( int freq = 280, int count = 100, int repeat = 1 )
        {
            List<Task<bool>> list = new List<Task<bool>> ( );
            list . Add ( Utils2 . Dobeep ( 320, 300 ) );
            list . Add ( Utils2 . Dobeep ( 260, 800 ) );
            return;
        }
        public async static Task<bool> Dobeep ( int freq, int duration )
        {
            Console . Beep ( freq, duration );
            return true;
        }

        #endregion play beeps

        #region Dictionary Handlers
        public static string GetDictionaryEntry ( Dictionary<string, string> dict, string key, out string dictvalue )
        {
            string keyval = "";

            if ( dict . Count == 0 )
                SqlSupport . LoadConnectionStrings ( );
            if ( dict . TryGetValue ( key . ToUpper ( ), out keyval ) == false )
            {
                if ( dict . TryGetValue ( key, out keyval ) == false )
                {
                    Debug . WriteLine ( $"Unable to access Dictionary {dict} to identify key value [{key}]" );
                    key = key + "ConnectionString";
                    DoErrorBeep ( 370, 100, 1 );
                    DoErrorBeep ( 270, 500, 1 );
                    //WpfLib1 . Utils .DoErrorBeep ( 250 , 50 , 1 );
                }
            }
            dictvalue = keyval;
            return keyval;
        }
        //public static string GetDictionaryEntry ( Dictionary<int , string> dict , int key , out string dictvalue )
        //{
        //    string keyval = "";
        //    if ( dict . Count == 0 )
        //        SqlSupport . LoadConnectionStrings ( );

        //    if ( dict . TryGetValue ( key , out keyval ) == false )
        //    {
        //        Debug . WriteLine ( $"Unable to access Dictionary {dict} to identify key value [{key}]" );
        //        WpfLib1 . Utils . DoErrorBeep ( 250 , 50 , 1 );
        //    }
        //    dictvalue = keyval;
        //    return keyval;
        //}
        //public static int GetDictionaryEntry ( Dictionary<int , int> dict , int key , out int dictvalue )
        //{
        //    int keyval = 0;
        //    if ( dict . Count == 0 )
        //        Utils . LoadConnectionStrings ( );

        //    if ( dict . TryGetValue ( key , out keyval ) == false )
        //    {
        //        Debug . WriteLine ( $"Unable to access Dictionary {dict} to identify key value [{key}]" );
        //        WpfLib1 . Utils . DoErrorBeep ( 250 , 50 , 1 );
        //    }
        //    dictvalue = keyval;
        //    return keyval;
        //}
        //public static bool DeleteDictionaryEntry ( Dictionary<string , string> dict , string value )
        //{
        //    try
        //    {
        //        dict . Remove ( value );
        //    }
        //    catch ( Exception ex )
        //    {
        //        Debug . WriteLine ( $"Unable to access Dictionary {dict} to delete key value [{value}]\n{ex . Message}" );
        //        WpfLib1 . Utils . DoErrorBeep ( 250 , 50 , 1 );
        //        return false;
        //    }
        //    return true;
        //}
        //public static bool DeleteDictionaryEntry ( Dictionary<string , int> dict , string value )
        //{
        //    try
        //    {
        //        dict . Remove ( value );
        //    }
        //    catch ( Exception ex )
        //    {
        //        Debug . WriteLine ( $"Unable to access Dictionary {dict} to delete key value [{value}]\n{ex . Message}" );
        //        WpfLib1 . Utils . DoErrorBeep ( 250 , 50 , 1 );
        //        return false;
        //    }
        //    return true;
        //}
        //public static bool DeleteDictionaryEntry ( Dictionary<int , int> dict , int value )
        //{
        //    try
        //    {
        //        dict . Remove ( value );
        //    }
        //    catch ( Exception ex )
        //    {
        //        Debug . WriteLine ( $"Unable to access Dictionary {dict} to delete key value [{value}]\n{ex . Message}" );
        //        WpfLib1 . Utils . DoErrorBeep ( 250 , 50 , 1 );
        //        return false;
        //    }
        //    return true;
        //}

        // Create dictionary of ALL Sql Connection strings we may want to use
        //public static void LoadConnectionStrings ( )
        //{
        //    // This one works just fine - its in NewWpfDev
        //    Settings defaultInstance = ( ( Settings ) ( global::System . Configuration . ApplicationSettingsBase . Synchronized ( new Settings ( ) ) ) );
        //    try
        //    {
        //        if ( Flags . ConnectionStringsDict . Count > 0 )
        //            Flags . ConnectionStringsDict . Clear ( );
        //        Flags . ConnectionStringsDict . Add ( "IAN1" , ( string ) Properties . Settings . Default [ "BankSysConnectionString" ] );
        //        Flags . ConnectionStringsDict . Add ( "NORTHWIND" , ( string ) Properties . Settings . Default [ "NorthwindConnectionString" ] );
        //        Flags . ConnectionStringsDict . Add ( "PUBS" , ( string ) Properties . Settings . Default [ "PubsConnectionString" ] );
        //        Flags . ConnectionStringsDict . Add ( "ADVENTUREWORKS2019" , ( string ) Properties . Settings . Default [ "AdventureWorks2019" ] );
        //        WpfLib1 . Utils . WriteSerializedCollectionJSON ( Flags . ConnectionStringsDict , @"C:\users\ianch\DbConnectionstrings.dat" );
        //    }
        //    catch ( NullReferenceException ex )
        //    {
        //        Debug . WriteLine ( $"Dictionary  entrry [{( string ) Properties . Settings . Default [ "BankSysConnectionString" ]}] already exists" );
        //    }
        //    finally
        //    {

        //    }
        //}

        #endregion Dictionary Handlers

        #region datagrid row  to List methods (string, int, double, decimal, DateTime)
        public static List<string> GetTableColumnsList ( DataTable dt )
        {
            //Return a list of strings Containing table column info
            List<string> list = new List<string> ( );
            foreach ( DataRow row in dt . Rows )
            {
                string output = "";
                var colcount = row . ItemArray . Length;
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
            List<string> list = new List<string> ( );
            foreach ( DataRow row in dt . Rows )
            {
                var txt = row . Field<string> ( 0 );
                list . Add ( txt );
            }
            return list;
        }
        public static List<int> GetDataDridRowsAsListOfInts ( DataTable dt )
        {
            List<int> list = new List<int> ( );
            foreach ( DataRow row in dt . Rows )
            {
                // ... Write value of first field as integer.
                list . Add ( row . Field<int> ( 0 ) );
            }
            return list;
        }
        public static List<double> GetDataDridRowsAsListOfDoubles ( DataTable dt )
        {
            List<double> list = new List<double> ( );
            foreach ( DataRow row in dt . Rows )
            {
                // ... Write value of first field as integer.
                list . Add ( row . Field<double> ( 0 ) );
            }
            return list;
        }
        public static List<decimal> GetDataDridRowsAsListOfDecimals ( DataTable dt )
        {
            List<decimal> list = new List<decimal> ( );
            foreach ( DataRow row in dt . Rows )
            {
                // ... Write value of first field as integer.
                list . Add ( row . Field<decimal> ( 0 ) );
            }
            return list;
        }
        public static List<DateTime> GetDataDridRowsAsListOfDateTime ( DataTable dt )
        {
            List<DateTime> list = new List<DateTime> ( );
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
                            outp = output . Substring ( 0, output . Length - trimlen );// += "\n";
                            output += "\n";
                            indx = 3;
                        }
                        else
                        {
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

        //public static bool CompareDbRecords ( object obj1 , object obj2 )
        //{
        //    bool result = false;
        //    BankAccountViewModel bvm = new BankAccountViewModel ( );
        //    CustomerViewModel cvm = new CustomerViewModel ( );
        //    DetailsViewModel dvm = new DetailsViewModel ( );
        //    if ( obj1 == null || obj2 == null )
        //        return result;
        //    if ( obj1 . GetType ( ) == bvm . GetType ( ) )
        //        bvm = obj1 as BankAccountViewModel;
        //    if ( obj1 . GetType ( ) == cvm . GetType ( ) )
        //        cvm = obj1 as CustomerViewModel;
        //    if ( obj1 . GetType ( ) == dvm . GetType ( ) )
        //        dvm = obj1 as DetailsViewModel;

        //    if ( obj2 . GetType ( ) == bvm . GetType ( ) )
        //        bvm = obj2 as BankAccountViewModel;
        //    if ( obj2 . GetType ( ) == cvm . GetType ( ) )
        //        cvm = obj2 as CustomerViewModel;
        //    if ( obj2 . GetType ( ) == dvm . GetType ( ) )
        //        dvm = obj2 as DetailsViewModel;

        //    if ( bvm != null && cvm != null )
        //    {
        //        if ( bvm . CustNo == cvm . CustNo )
        //            result = true;
        //    }
        //    else if ( bvm != null && dvm != null )
        //    {
        //        if ( bvm . CustNo == dvm . CustNo )
        //            result = true;
        //    }
        //    else if ( cvm != null && dvm != null )
        //    {
        //        if ( cvm . CustNo == dvm . CustNo )
        //            result = true;
        //    }
        //    result = false;
        //    return result;
        //}
        public static RenderTargetBitmap CreateControlImage ( FrameworkElement control, string filename = "", bool savetodisk = false )
        {
            if ( control == null )
                return null;
            // Get the Visual (Control) itself and the size of the Visual and its descendants.
            // This is the clever bit that gets the requested control, not the full window
            Rect rect = VisualTreeHelper . GetDescendantBounds ( control );

            // Make a DrawingVisual to make a screen
            // representation of the control.
            DrawingVisual dv = new DrawingVisual ( );

            // Fill a rectangle the same size as the control
            // with a brush containing images of the control.
            using ( DrawingContext ctx = dv . RenderOpen ( ) )
            {
                VisualBrush brush = new VisualBrush ( control );
                ctx . DrawRectangle ( brush, null, new Rect ( rect . Size ) );
            }

            // Make a bitmap and draw on it.
            int width = ( int ) control . ActualWidth;
            int height = ( int ) control . ActualHeight;
            if ( height == 0 || width == 0 )
                return null;
            RenderTargetBitmap rtb = new RenderTargetBitmap ( width, height, 96, 96, PixelFormats . Pbgra32 );
            rtb . Render ( dv );
            if ( savetodisk && filename != "" )
                SaveImageToFile ( rtb, filename );
            return rtb;
        }

        public static void Magnify ( List<object> list, bool magnify )
        {
            // lets other controls have magnification, providing other Templates do not overrule these.....
            for ( int i = 0 ; i < list . Count ; i++ )
            {
                var obj = list [ i ] as ListBox;
                if ( obj != null )
                {
                    obj . Style = magnify ? System . Windows . Application . Current . FindResource ( "ListBoxMagnifyAnimation4" ) as Style : null;
                    continue;
                }
                var lv = list [ i ] as ListView;
                if ( lv != null )
                {
                    lv . Style = magnify ? System . Windows . Application . Current . FindResource ( "ListViewMagnifyAnimation4" ) as Style : null;
                    continue;
                }
                var dg = list [ i ] as DataGrid;
                if ( dg != null )
                {
                    dg . Style = magnify ? System . Windows . Application . Current . FindResource ( "DatagridMagnifyAnimation4" ) as Style : null;
                    continue;
                }
                var bd = list [ i ] as Border;
                if ( bd != null )
                {
                    bd . Style = magnify ? System . Windows . Application . Current . FindResource ( "BorderMagnifyAnimation4" ) as Style : null;
                    continue;

                }
                var cb = list [ i ] as ComboBox;
                if ( cb != null )
                {
                    cb . Style = magnify ? System . Windows . Application . Current . FindResource ( "ComboBoxMagnifyAnimation4" ) as Style : null;
                    continue;
                }
                var btn = list [ i ] as Button;
                if ( btn != null )
                {
                    btn . Style = magnify ? System . Windows . Application . Current . FindResource ( "ButtonMagnifyAnimation4" ) as Style : null;
                    continue;
                }
                var rct = list [ i ] as Rectangle;
                if ( rct != null )
                {
                    rct . Style = magnify ? System . Windows . Application . Current . FindResource ( "RectangleMagnifyAnimation4" ) as Style : null;
                    continue;
                }
                var tb = list [ i ] as TextBlock;
                if ( tb != null )
                {
                    tb . Style = magnify ? System . Windows . Application . Current . FindResource ( "TextBlockMagnifyAnimation4" ) as Style : null;
                    continue;
                }
                var tbx = list [ i ] as TextBox;
                if ( tbx != null )
                {
                    tbx . Style = magnify ? System . Windows . Application . Current . FindResource ( "TextBoxMagnifyAnimation4" ) as Style : null;
                    continue;
                }
            }
        }
        //****************************//
        #region Find Child/Parent etc 
        //****************************//

        public static DependencyObject FindChild ( DependencyObject o, Type childType )
        {
            DependencyObject foundChild = null;
            if ( o != null )
            {
                int childrenCount = VisualTreeHelper . GetChildrenCount ( o );
                for ( int i = 0 ; i < childrenCount ; i++ )
                {
                    var child = VisualTreeHelper . GetChild ( o, i );
                    if ( child . GetType ( ) != childType )
                    {
                        foundChild = FindChild ( child, childType );
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
        public static T FindChild<T> ( DependencyObject parent, string childName )
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if ( parent == null )
                return null;
            T foundChild = null;
            int childrenCount = VisualTreeHelper . GetChildrenCount ( parent );
            for ( int i = 0 ; i < childrenCount ; i++ )
            {
                var child = VisualTreeHelper . GetChild ( parent, i );
                // If the child is not of the request child type child
                T childType = child as T;
                if ( childType == null )
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T> ( child, childName );
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
                    else
                    {
                        // child element found.
                        Type type = childName . GetType ( );
                        foundChild = ( T ) FindChild ( child, type );
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
        public static T FindVisualChildByName<T> ( DependencyObject parent, string name ) where T : DependencyObject
        {
            for ( int i = 0 ; i < VisualTreeHelper . GetChildrenCount ( parent ) ; i++ )
            {
                var child = VisualTreeHelper . GetChild ( parent, i );
                string controlName = child . GetValue ( Control . NameProperty ) as string;
                if ( controlName == name )
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildByName<T> ( child, name );
                    if ( result != null )
                        return result;
                }
            }
            return null;
        }
        public static void FindVisualParent ( UIElement element, out object [ ] array )
        {
            int indx = 0;
            object [ ] arr = new object [ 20 ];
            UIElement parent = element as UIElement;
            if ( parent . GetType ( ) == typeof ( Border ) )
            {
                arr [ 0 ] = parent;
                indx++;
            }
            array = arr;
            while ( parent != null )
            {
                parent = VisualTreeHelper . GetParent ( parent ) as UIElement;
                if ( parent != null )
                {
                    // see if this is the top level (Canvas is next up the chain)
                    if ( parent . GetType ( ) == typeof ( Canvas ) )
                        break;  // Yes it is
                    else
                    {   // nope !!  try next oen 
                        arr [ indx ] = parent as object;
                        indx++;
                    }
                }
            }
            array = arr;
        }

        public static T FindVisualParent<T> ( UIElement element, out string [ ] array ) where T : UIElement
        {
            int indx = 1;
            string [ ] arr = new string [ 20 ];
            UIElement parent = element;
            arr [ 0 ] = parent . ToString ( );
            array = arr;
            while ( parent != null )
            {
                var correctlyTyped = parent as T;
                if ( correctlyTyped != null )
                {
                    array = arr;
                    return correctlyTyped;
                }
                parent = VisualTreeHelper . GetParent ( parent ) as UIElement;
                if ( parent != null )
                    arr [ indx ] = parent . ToString ( );
                indx++;
            }
            array = arr;
            return null;
        }
        public static parentItem FindVisualParent<parentItem> ( DependencyObject obj, out string [ ] objectarray ) where parentItem : DependencyObject
        {
            // Climbs UP the visual tree
            string [ ] array = new string [ 20 ];
            int index = 0;
            DependencyObject parent = VisualTreeHelper . GetParent ( obj );
            Type type = parent . GetType ( );
            array [ index++ ] = type . ToString ( );
            while ( parent != null && !parent . GetType ( ) . Equals ( typeof ( parentItem ) ) )
            {
                parent = VisualTreeHelper . GetParent ( parent );
                if ( parent == null )
                    break;
                type = parent . GetType ( );
                if ( index < 20 )
                    array [ index++ ] = type . ToString ( );
            }
            objectarray = array;
            return parent as parentItem;
        }

        #endregion Find Child/Parent etc 

        public static string GetDataSortOrder ( string commandline )
        {
            if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . DEFAULT )
                commandline += "Custno, BankNo";
            else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . ID )
                commandline += "ID";
            else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . BANKNO )
                commandline += "BankNo, CustNo";
            else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . CUSTNO )
                commandline += "CustNo";
            else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . ACTYPE )
                commandline += "AcType";
            else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . DOB )
                commandline += "Dob";
            else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . ODATE )
                commandline += "Odate";
            else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . CDATE )
                commandline += "Cdate";
            return commandline;
        }
        public static void SaveImageToFile ( RenderTargetBitmap bmp, string file, string imagetype = "PNG" )
        {
            string [ ] items;
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
                using ( FileStream fs = new FileStream ( file,
                            FileMode . Create, FileAccess . Write, FileShare . ReadWrite ) )
                {
                    if ( imagetype == "PNG" )
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder ( );
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    else if ( imagetype == "GIF" )
                    {
                        GifBitmapEncoder encoder = new GifBitmapEncoder ( );
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    else if ( imagetype == "JPG" || imagetype == "JPEG" )
                    {
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder ( );
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    else if ( imagetype == "BMP" )
                    {
                        BmpBitmapEncoder encoder = new BmpBitmapEncoder ( );
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    else if ( imagetype == "TIF" || imagetype == "TIFF" )
                    {
                        TiffBitmapEncoder encoder = new TiffBitmapEncoder ( );
                        encoder . Frames . Add ( BitmapFrame . Create ( bmp ) );
                        encoder . Save ( fs );
                    }
                    fs . Close ( );
                }
            }
            catch ( Exception ex )
            {
                //TODO
                //    WpfLib1 . Utils .Mbox ( null , string1: "The image could not be saved for the following reason " , string2: $"{ex . Message}" , caption: "" , iconstring: "\\icons\\Information.png" , Btn1: MB . OK , Btn2: MB . NNULL , defButton: MB . OK );
            }
        }
        //******************************************//
        #region Scroll Controls toselected Record
        //******************************************//
        public static void ScrollLBRecordIntoView ( ListBox lbox, int CurrentRecord )
        {
            // Works well 26/5/21

            //update and scroll to bottom first
            lbox . ScrollIntoView ( lbox . SelectedIndex );
            lbox . UpdateLayout ( );
            lbox . ScrollIntoView ( lbox . SelectedItem );
            lbox . UpdateLayout ( );
        }
        //public static void ScrollLVRecordIntoView ( ListView Dgrid , int CurrentRecord )
        //{
        //    // Works well 26/5/21

        //    //update and scroll to bottom first
        //    Dgrid . SelectedIndex = ( int ) CurrentRecord;
        //    Dgrid . SelectedItem = ( int ) CurrentRecord;
        //    Dgrid . ScrollIntoView ( Dgrid . SelectedItem );
        //    Dgrid . UpdateLayout ( );
        //}
        //public static void ScrollRecordIntoView ( DataGrid Dgrid , int CurrentRecord , object row = null )
        //{
        //    // Works well 26/5/21
        //    double currentTop = 0;
        //    double currentBottom = 0;
        //    if ( CurrentRecord == -1 || Dgrid == null )
        //        return;
        //    if ( Dgrid . Name == "CustomerGrid" || Dgrid . Name == "DataGrid1" )
        //    {
        //        currentTop = Flags . TopVisibleBankGridRow;
        //        currentBottom = Flags . BottomVisibleBankGridRow;
        //    }
        //    else if ( Dgrid . Name == "BankGrid" || Dgrid . Name == "DataGrid2" )
        //    {
        //        currentTop = Flags . TopVisibleCustGridRow;
        //        currentBottom = Flags . BottomVisibleCustGridRow;
        //    }
        //    else if ( Dgrid . Name == "DetailsGrid" || Dgrid . Name == "DetailsGrid" )
        //    {
        //        currentTop = Flags . TopVisibleDetGridRow;
        //        currentBottom = Flags . BottomVisibleDetGridRow;
        //    }     // Believe it or not, it takes all this to force a scrollinto view correctly

        //    if ( Dgrid == null || Dgrid . Items . Count == 0 )//|| Dgrid . SelectedItem == null )
        //        return;

        //    if ( Dgrid . SelectedIndex != CurrentRecord )
        //        Dgrid . SelectedIndex = CurrentRecord;
        //    Dgrid . SelectedItem = CurrentRecord;
        //    Dgrid . ScrollIntoView ( Dgrid . Items . Count - 1 );
        //    Dgrid . SelectedItem = CurrentRecord;
        //    Dgrid . UpdateLayout ( );
        //    Dgrid . BringIntoView ( );
        //    Dgrid . ScrollIntoView ( Dgrid . Items [ Dgrid . Items . Count - 1 ] );
        //    Dgrid . UpdateLayout ( );
        //    Dgrid . ScrollIntoView ( row ?? Dgrid . SelectedItem );
        //    Dgrid . UpdateLayout ( );
        //}
        //public static void ScrollCBRecordIntoView ( ComboBox lbox , int CurrentRecord )
        //{
        //    // Works well 26/5/21
        //    //update and scroll to bottom first
        //    lbox . SelectedIndex = CurrentRecord;
        //    lbox . UpdateLayout ( );
        //}

        #endregion Scroll Controls toselected Record

        //Generic form of Selection forcing code below
        public static void SetupWindowDrag ( Window inst )
        {
            try
            {
                //Handle the button NOT being the left mouse button
                // which will crash the DragMove Fn.....
                MouseButtonState mbs = Mouse . RightButton;
                if ( mbs == MouseButtonState . Pressed )
                    return;
                inst . MouseDown += delegate
                {
                    {
                        try
                        {
                            inst?.DragMove ( );
                        }
                        catch ( Exception ex )
                        {
                            return;
                        }
                    }
                };
            }
            catch ( Exception ex )
            {
                return;
            }
        }

        #region Sql connections support

        public static bool CheckResetDbConnection ( string currdb, out string constring )
        {
            currdb?.ToUpper ( );
            // This resets the current database connection to the one we re working with (currdb - in UPPER Case!)- should be used anywhere that We switch between databases in Sql Server
            // It also sets the Flags.CurrentConnectionString - Current Connectionstring  and local variable
            if ( Utils . GetDictionaryEntry ( Flags . ConnectionStringsDict, currdb, out string connstring ) != "" )
            {
                if ( connstring != null )
                {
                    Flags . CurrentConnectionString = connstring;
                    SqlConnection con;
                    con = new SqlConnection ( Flags . CurrentConnectionString );
                    if ( con != null )
                    {
                        //test it
                        constring = connstring;
                        con . Close ( );
                        return true;
                    }
                    else
                    {
                        constring = connstring;
                        return false;
                    }
                }
                else
                {
                    constring = "";
                    return false;
                }
            }
            else
            {
                constring = "";
                return false;
            }
        }

        //public static string GetCheckCurrentConnectionString ( string CurrentTableDomain )
        //{
        //    string Con = "";
        //    if ( NewWpfDev . Utils . CheckResetDbConnection ( CurrentTableDomain , out string constring ) == false )
        //    {
        //        Debug . WriteLine ( $"Failed to set connection string for {CurrentTableDomain . ToUpper ( )} Db" );
        //        return null;
        //    }
        //    else
        //    {
        //        Con = GenericDbUtilities . CheckSetSqlDomain ( CurrentTableDomain );
        //        if ( Con == "" )
        //        {   // drop down to default of IAN1
        //            Con = MainWindow . SqlCurrentConstring;
        //        }
        //    }
        //    return Con;
        //}

        #endregion Sql connections support

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
        public static IEnumerable ReadGenericCollection<T> ( ObservableCollection<T> collection, IEnumerator ie = null )
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

        #region Nullable handlers
        public static bool? CompareNullable ( int? a, int? b )
        {
            if ( Nullable . Compare<int> ( a, b ) == 0 )
            {
                $"Nullable int {a} is Equal to {b}" . cwinfo ( );
                return true;
            }
            else
            {
                $"Nullable int {a} is NOT Equal to {b}" . cwinfo ( );
                return false;
            }
        }
        public static bool? CompareNullable ( long? a, long? b )
        {
            if ( Nullable . Compare<long> ( a, b ) == 0 )
            {
                $"Nullable long {a} is Equal to {b}" . cwinfo ( );
                return true;
            }
            else
            {
                $"Nullable long {a} is NOT Equal to {b}" . cwinfo ( );
                return false;
            }
        }
        public static bool? CompareNullable ( double? a, double? b )
        {
            if ( Nullable . Compare<double> ( a, b ) == 0 )
            {
                $"Nullable double {a} is Equal to {b}" . cwinfo ( );
                return true;
            }
            else
            {
                $"Nullable double int {a} is NOT Equal to {b}" . cwinfo ( );
                return false;
            }
        }
        public static bool? CompareNullable ( float? a, float? b )
        {
            if ( Nullable . Compare<float> ( a, b ) == 0 )
            {
                $"Nullable float {a} is Equal to {b}" . cwinfo ( );
                return true;
            }
            else
            {
                $"Nullable float {a} is NOT Equal to {b}" . cwinfo ( );
                return false;
            }
        }
        public static bool? CompareNullable ( decimal? a, decimal? b )
        {
            if ( Nullable . Compare<decimal> ( a, b ) == 0 )
            {
                $"Nullable decimal {a} is Equal to {b}" . cwinfo ( );
                return true;
            }
            else
            {
                $"Nullable decimal {a} is NOT Equal to {b}" . cwinfo ( );
                return false;
            }
        }


        #endregion Nullable handlers

        #region file read/Write methods

        public static StringBuilder ReadFileGeneric ( string path, ref StringBuilder sb )
        {
            string s = File . ReadAllText ( path );
            sb . Append ( s );
            return sb;
        }
        public static bool ReadStringFromFile ( string path, out string str, bool Trimlines = false, bool TrimBlank = false )
        {
            str = ReadStringFromFileComplex ( path, Trimlines, TrimBlank );
            if ( str . Length > 0 )
                return true;
            else
                return false;
        }
        public static string ReadStringFromFileComplex ( string path, bool Trimlines = false, bool TrimBlank = false )
        {
            string result = "";
            result = File . ReadAllText ( path );
            if ( Trimlines == true )
            {
                StringBuilder sbb = new StringBuilder ( );
                string [ ] strng = result . Split ( '\n' );
                foreach ( string item in strng )
                {
                    if ( TrimBlank == true )
                    {
                        sbb . Append ( item . Trim ( ) );
                    }
                    else
                        sbb . Append ( item );
                }
                result = sbb . ToString ( );
            }
            return result;
        }
        public static StringBuilder ReadStringBuilderFromFile ( string path, ref string str )
        {
            StringBuilder sb = new StringBuilder ( );
            ReadStringBuilderFromFile ( path, out sb );
            return sb;
        }
        public static bool ReadStringBuilderFromFile ( string path, out StringBuilder sb, bool Trimlines = false, bool TrimBlank = false )
        {
            StringBuilder sbb = new StringBuilder ( );
            StringSplitOptions options = StringSplitOptions . None;
            string str = File . ReadAllText ( path );
            if ( Trimlines == true )
                options = StringSplitOptions . TrimEntries;
            if ( TrimBlank == true )
                options = StringSplitOptions . RemoveEmptyEntries;
            string [ ] strng = str . Split ( '\n', options );
            foreach ( string item in strng )
            {
                sbb . Append ( item );
            }
            sb = sbb;
            if ( sbb . Length > 0 )
                return true;
            else
                return false;
        }
        public static StringBuilder ReadStringBuilderAllLinesFromFile ( string path )
        {
            StringBuilder sb = new StringBuilder ( );
            string str = File . ReadAllText ( path );
            sb . Append ( str );
            return sb;
        }
        public static bool WriteStringToFile ( string path, string data )
        {
            File . WriteAllText ( path, data );
            return true;
        }
        public static bool WriteStringBuilderToFile ( string path, StringBuilder data )
        {
            File . WriteAllText ( path, data . ToString ( ) );
            return true;
        }
        #endregion file read/Write methods

        #region Serialization
        //public static bool WriteSerializedObject ( object obj , string filename , string Modeltype )
        //{
        //    bool result = false;
        //    try
        //    {
        //        if ( Modeltype == "LbUserControl" )
        //        {
        //            LbUserControl item = new LbUserControl ( );
        //            Debug . WriteLine ( GetObjectSize ( item ) . ToString ( ) );
        //            item = obj as LbUserControl;

        //            //overview . title = "Serialization Overview";
        //            XmlSerializer writer = new XmlSerializer ( item . GetType ( ) );

        //            var path = Environment . GetFolderPath ( Environment . SpecialFolder . MyDocuments ) + "//SerializationOverview.xml";
        //            System . IO . FileStream file = System . IO . File . Create ( filename );

        //            writer . Serialize ( file , item );
        //            file . Close ( );
        //        }
        //        result = true;
        //    }
        //    catch { }
        //    return result;
        //}

        //private void ReadSerializedObject ( dynamic obj , string file ) {
        //    using ( var stream = File . Open ( file , FileMode . Open ) ) {
        //        using ( var reader = new BinaryReader ( stream , Encoding . UTF8 , false ) ) {
        //            obj = reader . ReadBytes ( file . Length );
        //        }
        //    }
        //}
        static private int GetObjectSize ( object TestObject )
        {
            BinaryFormatter bf = new BinaryFormatter ( );
            MemoryStream ms = new MemoryStream ( );
            byte [ ] Array;
            bf . Serialize ( ms, TestObject );
            Array = ms . ToArray ( );
            return Array . Length;
        }
        //static public bool WriteSerializedObjectJSON ( dynamic obj , string file = "" , int Jsontype = 1 ) {
        //    //Writes any linear style object as a JSON file (Observable collection works fine)
        //    // Doesnt handle Datagrids or UserControl etc
        //    //Create JSON String
        //    if ( file == "" )
        //        file = "DefaultJsonText.json";

        //    if ( Jsontype == 1 ) {
        //        try {
        //            var options = new JsonSerializerOptions { WriteIndented = true , IncludeFields = true , MaxDepth = 12 };
        //            string jsonString = System . Text . Json . JsonSerializer . Serialize<object> ( obj , options );
        //            // Save JSON file to disk 
        //            XmlSerializer mySerializer = new XmlSerializer ( typeof ( string ) );
        //            StreamWriter myWriter = new StreamWriter ( file );
        //            mySerializer . Serialize ( myWriter , jsonString );
        //            myWriter . Close ( );
        //            return true;
        //        }
        //        catch ( Exception ex ) {
        //            Debug . WriteLine ( $"Serialization FAILED :[{ex . Message}]" );
        //        }
        //    }
        //    else if ( Jsontype == 2 ) {
        //        try {
        //            FieldInvestigation ( obj . GetType ( ) );
        //            MethodInvestigation ( obj . GetType ( ) );

        //            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings ( );
        //            settings . IgnoreExtensionDataObject = true;
        //            DataContractJsonSerializer js = new DataContractJsonSerializer ( obj . GetType ( ) );
        //            MemoryStream msObj = new MemoryStream ( );
        //            js . WriteObject ( msObj , obj );
        //            msObj . Close ( );
        //        }
        //        catch ( Exception ex ) { Debug . WriteLine ( $"Json serialization failed. Reason : {ex . Message}" ); }
        //        return true;
        //    }
        //    return false;
        //}


        static void FieldInvestigation ( Type t )
        {
            Debug . WriteLine ( $"*********Fields for {t}*********" );
            FieldInfo [ ] fld = t . GetFields ( );
            foreach ( FieldInfo f in fld )
            {
                Debug . WriteLine ( "-->{0} : {1} ", f . MemberType, f . Name );
                //              Debug . WriteLine ( "-->{0}" , f .MemberType);
            }
        }

        static void MethodInvestigation ( Type t )
        {
            Debug . WriteLine ( $"*********Methods for {t}*********" );
            MethodInfo [ ] mth = t . GetMethods ( );
            foreach ( MethodInfo m in mth )
            {
                Debug . WriteLine ( "-->{0}", m . ReflectedType );
            }
        }
        public bool WriteSerializedObjectXML ( object obj, string file = "" )
        {
            //string myPath = "new.xml";
            //XmlSerializer s = new XmlSerializer ( settings . GetType ( ) );
            //StreamWriter streamWriter = new StreamWriter ( myPath );
            //s . Serialize ( streamWriter , settings ); 
            return true;
        }
        public bool ReadSerializedObjectXML ( object obj, string file = "" )
        {
            //MySettings mySettings = new MySettings ( );
            //string myPath = "new.xml";
            //XmlSerializer s = new XmlSerializer ( typeof ( mySettings ) ); return true;
            return true;
        }

        //public static ObservableCollection<BankAccountViewModel> CreateBankAccountFromJson ( string sb )
        //{
        //    ObservableCollection<BankAccountViewModel> Bvm = new ObservableCollection<BankAccountViewModel> ( );
        //    int index = 0;
        //    string [ ] entries, entry;
        //    bool start = true, End = false;
        //    string item = "";
        //    entries = sb . Split ( '\n' );
        //    while ( true )
        //    {
        //        BankAccountViewModel bv = new BankAccountViewModel ( );
        //        entry = entries [ index ] . Split ( ',' );
        //        if ( entry [ 0 ] == "" ) break;
        //        item = entry [ 1 ];
        //        bv . Id = int . Parse ( item );
        //        index++;
        //        entry = entries [ index ] . Split ( ',' );
        //        item = entry [ 1 ];
        //        bv . BankNo = item;
        //        index++;
        //        entry = entries [ index ] . Split ( ',' );
        //        item = entry [ 1 ];
        //        bv . CustNo = item;
        //        index++;
        //        entry = entries [ index ] . Split ( ',' );
        //        item = entry [ 1 ];
        //        bv . AcType = int . Parse ( item );
        //        index++;
        //        entry = entries [ index ] . Split ( ',' );
        //        item = entry [ 1 ];
        //        bv . Balance = Decimal . Parse ( item );
        //        index++;
        //        entry = entries [ index ] . Split ( ',' );
        //        item = entry [ 1 ];
        //        bv . IntRate = Decimal . Parse ( item );
        //        index++;
        //        entry = entries [ index ] . Split ( ',' );
        //        item = entry [ 1 ];
        //        bv . ODate = DateTime . Parse ( item );
        //        index++;
        //        entry = entries [ index ] . Split ( ',' );
        //        item = entry [ 1 ];
        //        bv . CDate = DateTime . Parse ( item );
        //        Bvm . Add ( bv );
        //        index++;
        //    }
        //    return Bvm;
        //}
        #endregion Serialization

        #region ZERO referennces
        public static string convertToHex ( double temp )
        {
            int intval = ( int ) Convert . ToInt32 ( temp );
            string hexval = intval . ToString ( "X" );
            return hexval;
        }
        //Working well 4/8/21
        public static string trace ( string prompt = "" )
        {
            // logs all the calls made upwards in a tree
            string output = "", tmp = "";
            int indx = 1;
            var v = new StackTrace ( 0 );
            if ( prompt != "" )
                output = prompt + $"\nStackTrace  for {prompt}:\n";
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
        public static Brush BrushFromColors ( Color color )
        {
            Brush brush = new SolidColorBrush ( color );
            return brush;
        }
        public static string ConvertInputDate ( string datein )
        {
            // only   do this id our SQL server is NOT configured to use 
            if ( MainWindow . SQL_USE_DMY_DATES == false )
            {
                string YYYMMDD = "";
                string [ ] datebits;
                // This filter will strip off the "Time" section of an excel date
                // and return us a valid YYYY/MM/DD string
                char [ ] ch = { '/', ' ' };
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
            else
                return datein;
        }
        //public static CustomerViewModel CreateCustomerRecordFromString ( string input )
        //{
        //    int index = 1;
        //    CustomerViewModel cvm = new CustomerViewModel ( );
        //    char [ ] s = { ',' };
        //    string [ ] data = input . Split ( s );
        //    string donor = data [ 0 ];
        //    //We have the sender type in the string recvd
        //    cvm . Id = int . Parse ( data [ index++ ] );
        //    cvm . CustNo = data [ index++ ];
        //    cvm . BankNo = data [ index++ ];
        //    cvm . AcType = int . Parse ( data [ index++ ] );
        //    cvm . FName = data [ index++ ];
        //    cvm . LName = data [ index++ ];
        //    cvm . Addr1 = data [ index++ ];
        //    cvm . Addr2 = data [ index++ ];
        //    cvm . Town = data [ index++ ];
        //    cvm . County = data [ index++ ];
        //    cvm . PCode = data [ index++ ];
        //    cvm . Phone = data [ index++ ];
        //    cvm . Mobile = data [ index++ ];
        //    cvm . Dob = DateTime . Parse ( data [ index++ ] );
        //    cvm . ODate = DateTime . Parse ( data [ index++ ] );
        //    cvm . CDate = DateTime . Parse ( data [ index ] );
        //    return cvm;
        //}
        //public static int FindMatchingRecord ( string Custno , string Bankno , DataGrid Grid , string currentDb = "" )
        //{
        //    int index = 0;
        //    bool success = false;
        //    if ( currentDb == "BANKACCOUNT" )
        //    {
        //        foreach ( var item in Grid . Items )
        //        {
        //            BankAccountViewModel cvm = item as BankAccountViewModel;
        //            if ( cvm == null )
        //                break;
        //            if ( cvm . CustNo == Custno && cvm . BankNo == Bankno )
        //            {
        //                success = true;
        //                break;
        //            }
        //            index++;
        //        }
        //        if ( !success )
        //            index = -1;
        //        return index;
        //    }
        //    else if ( currentDb == "CUSTOMER" )
        //    {
        //        foreach ( var item in Grid . Items )
        //        {
        //            CustomerViewModel cvm = item as CustomerViewModel;
        //            if ( cvm == null )
        //                break;
        //            if ( cvm . CustNo == Custno && cvm . BankNo == Bankno )
        //            {
        //                break;
        //            }
        //            index++;
        //        }
        //        if ( index == Grid . Items . Count )
        //            index = -1;
        //        return index;
        //    }
        //    else if ( currentDb == "DETAILS" )
        //    {
        //        foreach ( var item in Grid . Items )
        //        {
        //            DetailsViewModel dvm = item as DetailsViewModel;
        //            if ( dvm == null )
        //                break;
        //            if ( dvm . CustNo == Custno && dvm . BankNo == Bankno )
        //            {
        //                break;
        //            }
        //            index++;
        //        }
        //        if ( index == Grid . Items . Count )
        //            index = -1;
        //        return index;
        //    }
        //    return -1;
        //}
        //public static string CreateFullCsvTextFromRecord ( BankAccountViewModel bvm , DetailsViewModel dvm , CustomerViewModel cvm = null , bool IncludeType = true )
        //{
        //    if ( bvm == null && cvm == null && dvm == null )
        //        return "";
        //    string datastring = "";
        //    if ( bvm != null )
        //    {
        //        // Handle a BANK Record
        //        if ( IncludeType )
        //            datastring = "BANKACCOUNT";
        //        datastring += bvm . Id + ",";
        //        datastring += bvm . CustNo + ",";
        //        datastring += bvm . BankNo + ",";
        //        datastring += bvm . AcType . ToString ( ) + ",";
        //        datastring += bvm . IntRate . ToString ( ) + ",";
        //        datastring += bvm . Balance . ToString ( ) + ",";
        //        datastring += "'" + bvm . CDate . ToString ( ) + "',";
        //        datastring += "'" + bvm . ODate . ToString ( ) + "',";
        //    }
        //    else if ( dvm != null )
        //    {
        //        if ( IncludeType )
        //            datastring = "DETAILS,";
        //        datastring += dvm . Id + ",";
        //        datastring += dvm . CustNo + ",";
        //        datastring += dvm . BankNo + ",";
        //        datastring += dvm . AcType . ToString ( ) + ",";
        //        datastring += dvm . IntRate . ToString ( ) + ",";
        //        datastring += dvm . Balance . ToString ( ) + ",";
        //        datastring += "'" + dvm . CDate . ToString ( ) + "',";
        //        datastring += dvm . ODate . ToString ( ) + ",";
        //    }
        //    else if ( cvm != null )
        //    {
        //        if ( IncludeType )
        //            datastring = "CUSTOMER,";
        //        datastring += cvm . Id + ",";
        //        datastring += cvm . CustNo + ",";
        //        datastring += cvm . BankNo + ",";
        //        datastring += cvm . AcType . ToString ( ) + ",";
        //        datastring += "'" + cvm . CDate . ToString ( ) + "',";
        //        datastring += cvm . ODate . ToString ( ) + ",";
        //    }
        //    return datastring;
        //}
        //public static DetailsViewModel CreateDetailsRecordFromString ( string input )
        //{
        //    int index = 0;
        //    DetailsViewModel bvm = new DetailsViewModel ( );
        //    char [ ] s = { ',' };
        //    string [ ] data = input . Split ( s );
        //    string donor = data [ 0 ];
        //    //Check to see if the data includes the data type in it
        //    //As we have to parse it diffrently if not - see index....
        //    if ( donor . Length > 3 )
        //        index = 1;
        //    bvm . Id = int . Parse ( data [ index++ ] );
        //    bvm . CustNo = data [ index++ ];
        //    bvm . BankNo = data [ index++ ];
        //    bvm . AcType = int . Parse ( data [ index++ ] );
        //    bvm . IntRate = decimal . Parse ( data [ index++ ] );
        //    bvm . Balance = decimal . Parse ( data [ index++ ] );
        //    bvm . ODate = DateTime . Parse ( data [ index++ ] );
        //    bvm . CDate = DateTime . Parse ( data [ index ] );
        //    return bvm;
        //}
        public static bool HitTestScrollBar ( object sender, MouseButtonEventArgs e )
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
            object original = e . OriginalSource;
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            try
            {
                var v = original . GetType ( );
                bool isScrollbar = original . GetType ( ) . Equals ( typeof ( ScrollBar ) );
                if ( isScrollbar . Equals ( typeof ( ScrollBar ) ) == false )
                {
                    if ( original . GetType ( ) . Equals ( typeof ( DataGrid ) ) )
                    {
                        return false;
                    }
                    else if ( original . GetType ( ) . Equals ( typeof ( Paragraph ) ) )
                    {
                        return false;
                    }
                    else if ( original . GetType ( ) . Equals ( typeof ( Border ) ) )
                    {
                        return false;
                    }
                    else if ( FindVisualParent<ScrollBar> ( original as DependencyObject, out string [ ] objectarray ) != null )
                    {
                        //scroll bar is clicked
                        return true;
                    }
                    return false;
                }
                else
                    return true;
            }
            catch ( Exception ex )
            {
            }
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            return true;
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
            OpenFileDialog ofd = new OpenFileDialog ( );
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
            string fnameonly = ofd . SafeFileName;
            return ofd . FileName;
        }
        public static string GetImportFileName ( string filespec )
        // opens  the common file open dialog
        {
            OpenFileDialog ofd = new OpenFileDialog ( );
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
        public static void Grab_MouseMove ( object sender, MouseEventArgs e )
        {
            Point pt = e . GetPosition ( ( UIElement ) sender );
            HitTestResult hit = VisualTreeHelper . HitTest ( ( Visual ) sender, pt );
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
        public static void Grab_Object ( object sender, Point pt )
        {
            //Point pt = e.GetPosition((UIElement)sender);
            HitTestResult hit = VisualTreeHelper . HitTest ( ( Visual ) sender, pt );
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
        //public static void Grabscreen ( Window parent , object obj , GrabImageArgs args , Control ctrl = null )
        //{
        //    UIElement ui = obj as UIElement;
        //    UIElement OBJ = new UIElement ( );
        //    int indx = 0;
        //    bool success = false;
        //    string [ ] arr = new string [ 20 ];

        //    // try to step up the visual tree ?
        //    do
        //    {
        //        indx++;
        //        if ( indx > 30 || indx < 0 )
        //            break;
        //        switch ( indx )
        //        {
        //            case 1:
        //                OBJ = FindVisualParent<DataGrid> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 2:
        //                OBJ = FindVisualParent<Button> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 3:
        //                OBJ = FindVisualParent<Slider> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 4:
        //                OBJ = FindVisualParent<ListView> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 5:
        //                OBJ = FindVisualParent<ListBox> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 6:
        //                OBJ = FindVisualParent<ComboBox> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 7:
        //                OBJ = FindVisualParent<WrapPanel> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 8:
        //                OBJ = FindVisualParent<CheckBox> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 9:
        //                OBJ = FindVisualParent<DataGridRow> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 10:
        //                OBJ = FindVisualParent<DataGridCell> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 11:
        //                OBJ = FindVisualParent<Canvas> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 12:
        //                OBJ = FindVisualParent<GroupBox> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 13:
        //                OBJ = FindVisualParent<ProgressBar> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 14:
        //                OBJ = FindVisualParent<Ellipse> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 15:
        //                OBJ = FindVisualParent<RichTextBox> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 16:
        //                OBJ = FindVisualParent<TextBlock> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 17:
        //                //if(ui Equals TextBox)
        //                if ( ui == null )
        //                    break;
        //                DependencyObject v = new DependencyObject ( );
        //                DependencyObject prev = new DependencyObject ( );
        //                //OBJ = FindVisualParent<TextBox> ( ui );
        //                do
        //                {
        //                    v = VisualTreeHelper . GetParent ( ui );
        //                    if ( v == null )
        //                        break;
        //                    prev = v;
        //                    TextBox tb = v as TextBox;
        //                    if ( tb != null && tb . Text . Length > 0 )
        //                    {
        //                        Debug . WriteLine ( $"UI = {tb . Text}" );
        //                        OBJ = tb as UIElement;
        //                        success = true;
        //                        break;
        //                    }
        //                    Debug . WriteLine ( $"UI = {v . ToString ( )}" );
        //                    ui = v as UIElement;
        //                } while ( true );
        //                break;
        //            case 18:
        //                OBJ = FindVisualParent<ContentPresenter> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 19:
        //                OBJ = FindVisualParent<Grid> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 20:
        //                OBJ = FindVisualParent<Window> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 21:
        //                OBJ = FindVisualParent<ScrollContentPresenter> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            case 22:
        //                OBJ = FindVisualParent<Rectangle> ( ui , out arr );
        //                if ( OBJ != null )
        //                    success = true;
        //                break;
        //            //case 23:
        //            //	OBJ = FindVisualParent<TextBoxLineDrawingVisual> ( ui );
        //            //	if ( OBJ != null )
        //            //		success = true;
        //            //	break;
        //            default:
        //                OBJ = ui;
        //                success = false;
        //                break;
        //        }
        //        if ( success == true && OBJ != null )
        //            break;
        //    } while ( true );
        //    if ( success == false )
        //        return;
        //    Debug . WriteLine ( $"Element Identified for display = : [{OBJ . ToString ( )}]" );
        //    //string str = OBJ . ToString ( );
        //    //if ( str . Contains ( ".TextBox" ) )
        //    //{
        //    //	var v  = OBJ.GetType();
        //    //	Debug. WriteLine ( $"Type is {v}" );
        //    //}
        //    ////OBJ = OBJ . Text;
        //    var bmp = Utils . CreateControlImage ( OBJ as FrameworkElement );
        //    if ( bmp == null )
        //        return;
        //    WpfLib1 . Utils . SaveImageToFile ( ( RenderTargetBitmap ) bmp , "C:\\Dev2\\Icons\\Grabimage.png" , "PNG" );
        //    //TODO           Grabviewer gv = new Grabviewer ( parent , ctrl , bmp );
        //    //Setup the  image in our viewer
        //    //gv . Grabimage . Source = bmp;
        //    //gv . Title = "C:\\Dev2\\Icons\\Grabimage.png";
        //    //gv . Title += $"  :  RESIZE window to magnify the contents ...";
        //    //gv . Show ( );
        //    //// Save to disk file
        //}
        public static bool HitTestValidMoveType ( object sender, MouseButtonEventArgs e )
        {
            object original = e . OriginalSource;
            try
            {
                var v = original . GetType ( );
                //object = v . Parent;
                if ( original . GetType ( ) . Equals ( typeof ( Border ) ) )
                {
                    Debug . WriteLine ( "Border" );
                    return false;
                }
                Type type = original . GetType ( );
                if ( type . Equals ( typeof ( TextBlock ) ) )
                {
                    //Debug . WriteLine ( "TextBlock" ); return false;
                }
                else if ( type . Equals ( typeof ( Rectangle ) ) )
                {
                    //var vv = FindVisualParent<ScrollBar>(sender as DependencyObject , out string [ ] objectarray);
                    string [ ] arr = new string [ 20 ];
                    var vv = FindVisualParent<Rectangle> ( sender as UIElement, out arr );
                    if ( vv == null )
                    {
                        string arrayitem = arr [ 0 ];
                    }
                    Debug . WriteLine ( "Rectangle" );
                    return true;
                }
                else if ( type . Equals ( typeof ( ScrollBar ) ) || type . Equals ( typeof ( Thumb ) ) )
                {
                    //Debug . WriteLine ( "ScrollBar / Thumb" ); return true;
                }
                else if ( type . Equals ( typeof ( Grid ) ) )
                {
                    Debug . WriteLine ( "Grid" );
                    return false;
                }
                else if ( type . Equals ( typeof ( ListBox ) ) )
                {
                    Debug . WriteLine ( "ListBox" );
                    return true;
                }
                else if ( type . Equals ( typeof ( ListView ) ) )
                {
                    Debug . WriteLine ( "ListView" );
                    return true;
                }
                else if ( type . Equals ( typeof ( Button ) ) )
                {
                    //Debug . WriteLine ( "Button" ); return false;
                }
                else if ( type . Equals ( typeof ( TreeViewItem ) ) )
                {
                    //Debug . WriteLine ( "TreeViewItem" ); return true;
                }
                else if ( FindVisualParent<Border> ( original as DependencyObject, out string [ ] objectarray ) != null )
                {
                    //scroll bar is clicked
                    Debug . WriteLine ( "FindVisualParent FAILED in HitTestValidMoveType() - 2155" );
                    return false;
                }
                return false;
            }
            catch ( Exception ex )
            {
                return false;
            }
        }
        public static bool HitTestTreeViewItem ( object sender, MouseButtonEventArgs e )
        {
            TreeView tv = sender as TreeView;
            object original = e . OriginalSource;
            var vv = e . Source;
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            try
            {
                var v = original . GetType ( );
                if ( original . GetType ( ) . Equals ( typeof ( Border ) ) )
                {
                    return true;
                }
                Type type = original . GetType ( );
                if ( type . Equals ( typeof ( System . Windows . Shapes . Path ) ) )
                {
                    return false;
                }
                else if ( FindVisualParent<Border> ( original as DependencyObject, out string [ ] objectarray ) != null )
                {
                    //scroll bar is clicked
                    Debug . WriteLine ( "Calling FindVisualParent" );
                    return true;
                }
                return false;
            }
            catch ( Exception ex )
            {
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
                    string [ ] fields = input . Split ( '\r' );
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
        static public DataGridRow GetRow ( DataGrid dg, int index )
        {
            DataGridRow row = ( DataGridRow ) dg . ItemContainerGenerator . ContainerFromIndex ( index );
            if ( row == null )
            {
                // may be virtualized, bring into view and try again
                dg . ScrollIntoView ( dg . Items [ index ] );
                row = ( DataGridRow ) dg . ItemContainerGenerator . ContainerFromIndex ( index );
            }
            return row;
        }
        // allows any image to be saved as PNG/GIF/JPG format, defaullt is PNG
        public static void SetUpGridSelection ( DataGrid grid, int row = 0 )
        {
            if ( row == -1 )
                row = 0;
            // This triggers the selection changed event
            grid . SelectedIndex = row;
            grid . SelectedItem = row;
            grid . SelectedIndex = row;
            grid . SelectedItem = row;
            Utils . ScrollRecordIntoView ( grid, row );
            grid . ScrollIntoView ( grid . SelectedItem );
            grid . UpdateLayout ( );
        }


        public static List<object> GetChildControls ( UIElement parent, string TypeRequired )
        {
            // this uses  the TabControlHelper class
            UIElement element = new UIElement ( );
            object o = null;
            List<object> objects = new List<object> ( );
            //            IEnumerable alltabcontrols = null;
            //TODO
            if ( TypeRequired == "Grid" )
            {
                o = FindChild ( parent, typeof ( Grid ) );
                objects . Add ( o );
            }
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
            //else if ( TypeRequired == "WrapPanel" )
            //    alltabcontrols = TabControlHelper . FindChildren<WrapPanel> ( parent );
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
            //    IEnumerator enumerator = alltabcontrols . GetEnumerator ( );
            //    try
            //    {
            //        while ( enumerator . MoveNext ( ) )
            //        {
            //            count++;
            //            var v = enumerator . Current;
            //            objects . Add ( v );
            //        }
            //    }
            //    finally
            //    {
            //        Debug . WriteLine ( $"Found {count} controls of  type {TypeRequired}" );
            //    }
            //}
            //Debug . WriteLine ( "Finished FindChildren() 4\n" );

            return objects;
        }

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
        //public static Brush BrushFromHashString ( string color )
        //{
        //    //Must start with  '#'
        //    string s = color . ToString ( );
        //    if ( !s . Contains ( "#" ) )
        //        return WpfLib1 . Utils . BrushFromColors ( Colors . Transparent );
        //    Brush brush = ( Brush ) new BrushConverter ( ) . ConvertFromString ( color );
        //    return brush;
        //}
        //public static bool CheckForExistingGuid ( Guid guid )
        //{
        //    bool retval = false;
        //    for ( int x = 0 ; x < Flags . DbSelectorOpen . ViewersList . Items . Count ; x++ )
        //    {
        //        ListBoxItem lbi = new ListBoxItem ( );
        //        //lbi.Tag = viewer.Tag;
        //        lbi = Flags . DbSelectorOpen . ViewersList . Items [ x ] as ListBoxItem;
        //        if ( lbi . Tag == null )
        //            return retval;
        //        Guid g = ( Guid ) lbi . Tag;
        //        if ( g == guid )
        //        {
        //            retval = true;
        //            break;
        //        }
        //    }
        //    return retval;
        //}
        //        public static bool CheckRecordMatch ( BankAccountViewModel bvm , CustomerViewModel cvm , DetailsViewModel dvm )
        //        {
        //            bool result = false;
        //            if ( bvm != null && cvm != null )
        //            {
        //                if ( bvm . CustNo == cvm . CustNo )
        //                    result = true;
        //            }
        //            else if ( bvm != null && dvm != null )
        //            {
        //                if ( bvm . CustNo == dvm . CustNo )
        //                    result = true;
        //            }
        //            else if ( cvm != null && dvm != null )
        //            {
        //                if ( cvm . CustNo == dvm . CustNo )
        //                    result = true;
        //            }
        //            return result;
        //        }
        //        public static BankAccountViewModel CreateBankRecordFromString ( string type , string input )
        //        {
        //            int index = 0;
        //            BankAccountViewModel bvm = new BankAccountViewModel ( );
        //            char [ ] s = { ',' };
        //            string [ ] data = input . Split ( s );
        //            string donor = data [ 0 ];
        //            try
        //            {
        //                DateTime dt;
        //                if ( type == "BANKACCOUNT" || type == "SECACCOUNTS" )
        //                {
        //                    // This WORKS CORRECTLY 12/6/21 when called from n SQLDbViewer DETAILS grid entry && BANK grid entry					
        //                    // this test confirms the data layout by finding the Odate field correctly
        //                    // else it drops thru to the Catch branch
        //                    dt = Convert . ToDateTime ( data [ 7 ] );
        //                    //We can have any type of record in the string recvd
        //                    index = 1;  // jump the data type string
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
        //                else if ( type == "CUSTOMER" )
        //                {
        //                    // this test confirms the data layout by finding the Odate field correctly
        //                    // else it drops thru to the Catch branch
        //                    dt = Convert . ToDateTime ( data [ 5 ] );
        //                    // We have a customer record !!
        //                    //Check to see if the data includes the data type in it
        //                    //As we have to parse it diffrently if not - see index....
        //                    index = 1;
        //                    bvm . Id = int . Parse ( data [ index++ ] );
        //                    bvm . CustNo = data [ index++ ];
        //                    bvm . BankNo = data [ index++ ];
        //                    bvm . AcType = int . Parse ( data [ index++ ] );
        //                    bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
        //                    bvm . CDate = Convert . ToDateTime ( data [ index ] );
        //                }
        //                return bvm;
        //            }
        //            catch
        //            {
        //                //Check to see if the data includes the data type in it
        //                //As we have to parse it diffrently if not - see index....
        //                index = 0;
        //                try
        //                {
        //                    int x = int . Parse ( donor );
        //                    // if we get here, it IS a NUMERIC VALUE
        //                    index = 0;
        //                }
        //                catch
        //                {
        //                    //its probably the Data Type string, so ignore it for our Data creation processing
        //                    index = 1;
        //                }
        //                //We have a CUSTOMER record
        //                bvm . Id = int . Parse ( data [ index++ ] );
        //                bvm . CustNo = data [ index++ ];
        //                bvm . BankNo = data [ index++ ];
        //                bvm . AcType = int . Parse ( data [ index++ ] );
        //                bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
        //                bvm . CDate = Convert . ToDateTime ( data [ index ] );
        //                return bvm;
        //            }
        //        }
        //        public static CustomerDragviewModel CreateCustGridRecordFromString ( string input )
        //        {
        //            int index = 0;
        //            string type = "";
        //            //			BankAccountViewModel bvm = new BankAccountViewModel ( );
        //            CustomerDragviewModel cvm = new CustomerDragviewModel ( );

        //            char [ ] s = { ',' };
        //            string [ ] data = input . Split ( s );
        //            string donor = data [ 0 ];
        //            try
        //            {
        //                DateTime dt;
        //                type = data [ 0 ];
        //                // this test confirms the data layout by finding the Dob field correctly
        //                // else it drops thru to the Catch branch
        //                dt = Convert . ToDateTime ( data [ 10 ] );
        //                // We have a customer record !!
        //                //Check to see if the data includes the data type in it
        //                //As we have to parse it diffrently if not - see index....
        //                index = 0;
        //                cvm . RecordType = type;
        //                cvm . Id = int . Parse ( data [ index++ ] );
        //                cvm . CustNo = data [ index++ ];
        //                cvm . BankNo = data [ index++ ];
        //                cvm . AcType = int . Parse ( data [ index++ ] );

        //                cvm . FName = data [ index++ ];
        //                cvm . LName = data [ index++ ];
        //                cvm . Town = data [ index++ ];
        //                cvm . County = data [ index++ ];
        //                cvm . PCode = data [ index++ ];

        //                cvm . Dob = Convert . ToDateTime ( data [ index++ ] );
        //                cvm . ODate = Convert . ToDateTime ( data [ index++ ] );
        //                cvm . CDate = Convert . ToDateTime ( data [ index ] );
        //                return cvm;
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
        //            }
        //            return cvm;
        //        }
        public static bool FindWindowFromTitle ( string searchterm, ref Window handle )
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
        public static Window FindWindowFromClass ( string searchterm, ref Window handle )
        {
            Window result = null;
            foreach ( Window window in Application . Current . Windows )
            {
                if ( window . ToString ( ) . ToUpper ( ) . Contains ( searchterm . ToUpper ( ) ) )
                {
                    handle = window;
                    result = handle;
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
                Brush b = ( Brush ) Utils . GetDictionaryBrush ( parameter . ToString ( ) );
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
            //switch ( value ) {
            //    case 0:
            //        return ( Brushes . White );
            //    case 1:
            //        return ( Brushes . Yellow );
            //    case 2:
            //        return ( Brushes . Orange );
            //    case 3:
            //        return ( Brushes . Red );
            //    case 4:
            //        return ( Brushes . Magenta );
            //    case 5:
            //        return ( Brushes . Gray );
            //    case 6:
            //        return ( Brushes . Aqua );
            //    case 7:
            //        return ( Brushes . Azure );
            //    case 8:
            //        return ( Brushes . Brown );
            //    case 9:
            //        return ( Brushes . Crimson );
            //    case 10:
            //        return ( Brushes . Transparent );
            //}
            //return ( Brush ) null;
        }
        public static string GetPrettyGridStatistics ( DataGrid Grid, int current )
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
            ControlTemplate ctmp = System . Windows . Application . Current . FindResource ( tempname ) as ControlTemplate;
            return ctmp;
        }
        public static Style GetDictionaryStyle ( string tempname )
        {
            Style ctmp = System . Windows . Application . Current . FindResource ( tempname ) as Style;
            return ctmp;
        }
        public static object GetTemplateControl ( Control RectBtn, string CtrlName )
        {
            var template = RectBtn . Template;
            object v = template . FindName ( CtrlName, RectBtn ) as object;
            return v;
        }
        public static void ReadAllConfigSettings ( )
        {
            try
            {
                var appSettings = ConfigurationManager . AppSettings;

                if ( appSettings . Count == 0 )
                {
                    Debug . WriteLine ( "AppSettings is empty." );
                }
                else
                {
                    foreach ( var key in appSettings . AllKeys )
                    {
                        Debug . WriteLine ( "Key: {0} Value: {1}", key, appSettings [ key ] );
                    }
                }
            }
            catch ( ConfigurationErrorsException )
            {
                Debug . WriteLine ( "Error reading app settings" );
            }
        }
        //public static void HandleCtrlFnKeys ( bool key1 , KeyEventArgs e )
        //{
        //    if ( key1 && e . Key == Key . F5 )
        //    {
        //        // list Flags in Console
        //        WpfLib1 . Utils . GetWindowHandles ( );
        //        e . Handled = true;
        //        key1 = false;
        //        return;
        //    }
        //    else if ( key1 && e . Key == Key . F6 )  // CTRL + F6
        //    {
        //        // list various Flags in Console
        //        //Debug . WriteLine ( $"\nCTRL + F6 pressed..." );
        //        Flags . UseBeeps = !Flags . UseBeeps;
        //        e . Handled = true;
        //        key1 = false;
        //        //Debug . WriteLine ( $"Flags.UseBeeps reset to  {Flags . UseBeeps }" );
        //        return;
        //    }
        //    else if ( key1 && e . Key == Key . F7 )  // CTRL + F7
        //    {
        //        // list various Flags in Console
        //        //Debug . WriteLine ( $"\nCTRL + F7 pressed..." );
        //        //TRANSFER				Flags . PrintDbInfo ( );
        //        e . Handled = true;
        //        key1 = false;
        //        return;
        //    }
        //    else if ( key1 && e . Key == Key . F8 )     // CTRL + F8
        //    {
        //        //Debug . WriteLine ( $"\nCTRL + F8 pressed..." );
        //        EventHandlers . ShowSubscribersCount ( );
        //        e . Handled = true;
        //        key1 = false;
        //        return;
        //    }
        //    else if ( key1 && e . Key == Key . F9 )     // CTRL + F9
        //    {
        //        //Debug . WriteLine ( "\nCtrl + F9 NOT Implemented" );
        //        key1 = false;
        //        return;

        //    }
        //    else if ( key1 && e . Key == Key . System )     // CTRL + F10
        //    {
        //        // Major  listof GV[] variables (Guids etc]
        //        //Debug . WriteLine ( $"\nCTRL + F10 pressed..." );
        //        //TRANSFER				Flags . ListGridviewControlFlags ( 1 );
        //        key1 = false;
        //        e . Handled = true;
        //        return;
        //    }
        //    else if ( key1 && e . Key == Key . F11 )  // CTRL + F11
        //    {
        //        // list various Flags in Console
        //        //Debug . WriteLine ( $"\nCTRL + F11 pressed..." );
        //        //				Flags . PrintSundryVariables ( );
        //        e . Handled = true;
        //        key1 = false;
        //        return;
        //    }
        //}
        public static string ReadConfigSetting ( string key )
        {
            string result = "";
            try
            {
                string appSettings = ( string ) Properties . Settings . Default [ key ];
                return appSettings;
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
        public static RenderTargetBitmap RenderBitmap ( Visual element, double objwidth = 0, double objheight = 0, string filename = "", bool savetodisk = false )
        {
            double topLeft = 0;
            double topRight = 0;
            int width = 0;
            int height = 0;

            if ( element == null )
                return null;
            Rect bounds = VisualTreeHelper . GetDescendantBounds ( element );
            if ( objwidth == 0 )
                width = ( int ) bounds . Width;
            if ( objheight == 0 )
                height = ( int ) bounds . Height;
            double dpiX = 96; // this is the magic number
            double dpiY = 96; // this is the magic number

            PixelFormat pixelFormat = PixelFormats . Default;
            VisualBrush elementBrush = new VisualBrush ( element );
            DrawingVisual visual = new DrawingVisual ( );
            DrawingContext dc = visual . RenderOpen ( );

            dc . DrawRectangle ( elementBrush, null, new Rect ( topLeft, topRight, width, height ) );
            dc . Close ( );
            RenderTargetBitmap rtb = new RenderTargetBitmap ( ( int ) ( bounds . Width * dpiX / 96.0 ), ( int ) ( bounds . Height * dpiY / 96.0 ), dpiX, dpiY, PixelFormats . Pbgra32 );
            DrawingVisual dv = new DrawingVisual ( );
            using ( DrawingContext ctx = dv . RenderOpen ( ) )
            {
                VisualBrush vb = new VisualBrush ( element );
                ctx . DrawRectangle ( vb, null, new Rect ( new Point ( ), bounds . Size ) );
            }
            rtb . Render ( dv );

            if ( savetodisk && filename != "" )
                SaveImageToFile ( rtb, filename );
            return rtb;
        }
        public static void SaveProperty ( string setting, string value )
        {
            try
            {
                if ( value . ToUpper ( ) . Contains ( "TRUE" ) )
                    Properties . Settings . Default [ setting ] = true;
                else if ( value . ToUpper ( ) . Contains ( "FALSE" ) )
                    Properties . Settings . Default [ setting ] = false;
                else
                    Properties . Settings . Default [ setting ] = value;
                Properties . Settings . Default . Save ( );
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
        public static bool HitTestHeaderBar ( object sender, MouseButtonEventArgs e )
        {
            //			HitTestResult hit = VisualTreeHelper . HitTest ( ( Visual ) sender, e . GetPosition ( ( InputElement ) sender ) );
            //			return hit . VisualHit . GetVisualAncestor<ScrollBar> ( ) != null;
            object original = e . OriginalSource;

            if ( !original . GetType ( ) . Equals ( typeof ( DataGridColumnHeader ) ) )
            {
                if ( original . GetType ( ) . Equals ( typeof ( DataGrid ) ) )
                {
                    Debug . WriteLine ( "DataGrid is clicked" );
                }
                else if ( FindVisualParent<DataGridColumnHeader> ( original as DependencyObject, out string [ ] objectarray ) != null )
                {
                    //Header bar is clicked
                    return true;
                }
                return false;
            }
            return true;
        }
        //public static void SetGridRowSelectionOn ( DataGrid dgrid , int index )
        //{
        //    if ( dgrid . Items . Count > 0 && index != -1 )
        //    {
        //        try
        //        {
        //            //Setup new selected index
        //            dgrid . SelectedIndex = index;
        //            dgrid . SelectedItem = index;
        //            dgrid . UpdateLayout ( );
        //            dgrid . BringIntoView ( );
        //            object obj = dgrid . Items [ index ];
        //            if ( obj . GetType ( ) == typeof ( BankAccountViewModel ) )
        //            {
        //                BankAccountViewModel item = dgrid . Items [ index ] as BankAccountViewModel;
        //                dgrid . ScrollIntoView ( item );
        //            }
        //            else if ( obj . GetType ( ) == typeof ( CustomerViewModel ) )
        //            {
        //                CustomerViewModel item = dgrid . Items [ index ] as CustomerViewModel;
        //                dgrid . ScrollIntoView ( item );
        //            }
        //            else if ( obj . GetType ( ) == typeof ( GenericClass ) )
        //            {
        //                GenericClass item = dgrid . Items [ index ] as GenericClass;
        //                dgrid . ScrollIntoView ( item );
        //            }
        //        }
        //        catch ( Exception ex )
        //        {
        //            Debug . WriteLine ( $"{ex . Message}" );
        //        }
        //    }
        //}
        public static void SetSelectedItemFirstRow ( object dataGrid, object selectedItem )
        {
            //If target datagrid Empty, throw exception
            if ( dataGrid == null )
            {
                throw new ArgumentNullException ( "Target none" + dataGrid + "Cannot convert to DataGrid" );
            }
            //Get target DataGrid，If it is empty, an exception will be thrown
            System . Windows . Controls . DataGrid dg = dataGrid as System . Windows . Controls . DataGrid;
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
        //public static void SetUpGListboxSelection ( ListBox grid , int row = 0 )
        //{
        //    if ( row == -1 )
        //        row = 0;
        //    // This triggers the selection changed event
        //    grid . SelectedIndex = row;
        //    grid . SelectedItem = row;
        //    WpfLib1 . Utils . ScrollLBRecordIntoView ( grid , row );
        //    //grid . UpdateLayout ( );
        //    //grid . Refresh ( );
        //    //			var v = grid .VerticalAlignment;
        //}
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
        public static void ScrollRowInGrid ( DataGrid dGrid, int row )
        {
            if ( dGrid . SelectedItem == null )
                return;

            dGrid . ScrollIntoView ( dGrid . SelectedItem );
            dGrid . UpdateLayout ( );
            //           WpfLib1 . Utils .ScrollRecordIntoView ( dGrid , row );
            //            dGrid . UpdateLayout ( );
        }
        //********************************************************************************************************************************************************************************//
        public static void NewCookie_Click ( object sender, RoutedEventArgs e )
        //********************************************************************************************************************************************************************************//
        {
            //NewCookie nc = new NewCookie(sender as Window);
            //nc . ShowDialog ( );
            //defvars . CookieAdded = false;
        }
        //public static void LoadBankDbGeneric ( BankCollection bvm , string caller = "" , bool Notify = false , int lowvalue = -1 , int highvalue = -1 , int maxrecords = -1 )
        //{
        //    if ( maxrecords == -1 )
        //    {
        //        DataTable dt = new DataTable ( );
        //        BankCollection . LoadBank ( bvm , caller: caller , ViewerType: 99 , NotifyAll: Notify );
        //    }
        //    else
        //    {
        //        DataTable dtBank = new DataTable ( );
        //        dtBank = BankCollection . LoadSelectedBankData ( Min: lowvalue , Max: highvalue , Tot: maxrecords );
        //        bvm = BankCollection . LoadSelectedCollection ( bankCollection: bvm , max: -1 , dtBank: dtBank , Notify: Notify );
        //    }

        //}
        public static bool IsMousRightBtnDn ( object sender, MouseEventArgs e )
        {
            e . Handled = true;
            if ( e . RightButton == MouseButtonState . Pressed )
                return true;
            return false;
        }
        public static void TrackSplitterPosition ( TextBlock Textblock, double MaxWidth, DragDeltaEventArgs e )
        {
            Thickness th = new Thickness ( 0, 0, 0, 0 );
            th = Textblock . Margin;
            if ( th . Left < MaxWidth )
            {
                th . Left += e . HorizontalChange;
                if ( th . Left > 10 )
                    Textblock . Margin = th;
            }
        }

        #endregion ZERO referennces

        #region Dynamic Handlers
        //public static bool GetDynamicVarType ( dynamic Ctrlptr , out string [ ] strs , bool showinfo = false ) {
        //    //*************************************************************************************
        //    // Discovers what type the dynamic item is and returns a string containing its type
        //    //*************************************************************************************
        //    strs = new string [ ] { "" , "" , "" };
        //    bool isvalid = false;
        //    string [ ] ctrltype = { "" , "" , "" };
        //    string line = Ctrlptr . GetType ( ) . ToString ( );
        //    strs [ 2 ] = Ctrlptr . GetType ( ) . ToString ( );
        //    int offset = line . LastIndexOf ( '.' ) + 1;
        //    strs [ 0 ] = line . Substring ( offset );
        //    strs [ 1 ] = $"Tabview . Tabcntrl . {ctrltype [ 0 ]}";
        //    if ( Ctrlptr . GetType ( ) == typeof ( DgUserControl ) ) { if ( showinfo ) Debug . WriteLine ( $"Type of DataGrid received in Dynamic variable is {strs [ 0 ] . ToUpper ( )}" ); }
        //    else if ( Ctrlptr . GetType ( ) == typeof ( LbUserControl ) ) { if ( showinfo ) Debug . WriteLine ( $"Type of ListBox received in Dynamic variable is {strs [ 0 ] . ToUpper ( )}" ); }
        //    else if ( Ctrlptr . GetType ( ) == typeof ( LvUserControl ) ) { if ( showinfo ) Debug . WriteLine ( $"Type of ListView  received in Dynamic variable is {strs [ 0 ] . ToUpper ( )}" ); }
        //    isvalid = Ctrlptr . GetType ( ) != null ? true : false; ;
        //    return isvalid;
        //}

        #endregion Dynamic Handlers

        public static List<string> GetAllDgStyles ( )
        {
            List<string> validpaths = new List<string> ( );
            List<string> fullvalidpaths = new List<string> ( );
            List<string> fullvalidstyles = new List<string> ( );
            List<string> Paths = new List<string> ( );
            List<string> Styles = new List<string> ( );
            List<string> AllKeys = new List<string> ( );
            List<string> donorfiles = new List<string> ( );
            Dictionary<string, string> Matchbuffs = new Dictionary<string, string> ( );
            //            string Currentbuffer = "";
            string testbuffer = "";
            string [ ] buffer, FullBuffer;
            bool isfullkey = false;//, HasEntry = false; ;
            Dictionary<string, string> StyleKeys = new Dictionary<string, string> ( );
            string path = "";
            int index = 0, indexer = 1, offset = 0, chkoffset = 0;
            Application app = Application . Current;
            Uri uri = app . StartupUri;
            string rootpath = "";
            string root = @"C:\Users\ianch\source\repos\NewWpfDev\";
            string [ ] dirs = Directory . GetDirectories ( root );
            // now iterate thru them all 1 by 1
            foreach ( string dir in dirs )
            {
                int pointer = dir . LastIndexOf ( '\\' );
                if ( indexer > 0 )
                {
                    rootpath = dir . Substring ( 0, pointer );
                    validpaths . Add ( rootpath );
                    indexer = 0;
                }
                path = dir . Substring ( pointer + 1 );
                pointer = dir . LastIndexOf ( '\\' );
                path = dir . Substring ( pointer + 1 );
                path = path switch
                {
                    "Dicts" => "Dicts",
                    "Styles" => "Styles",
                    "Views" => "Views",
                    "ViewModels" => "ViewModels",
                    "UserControls" => "UserControls",
                    "Themes" => "Themes",
                    _ => null
                };
                if ( path != null )
                    validpaths . Add ( dir );
            }
            foreach ( string validfiles in validpaths )
            {
                string srchstring = validfiles;
                string [ ] files = Directory . GetFiles ( srchstring );
                foreach ( string item in files )
                {
                    if ( item . Contains ( ".xaml" ) && item . Contains ( "xaml.cs" ) == false )
                        fullvalidpaths . Add ( item );
                }
            }
            foreach ( var entry in fullvalidpaths )
            {
                index = 0;
                if ( entry . ToUpper ( ) . Contains ( "MVVMSTYLES" ) )
                    Debug . WriteLine ( "" );
                FullBuffer = File . ReadAllLines ( entry );
                buffer = File . ReadAllLines ( entry );
                while ( index < buffer . Length )
                {
                    if ( FullBuffer [ index ] . ToUpper ( ) . Contains ( $"TARGETTYPE=\"DATAGRID}}\"" ) == true
                          || FullBuffer [ index ] . ToUpper ( ) . Contains ( $"TARGETTYPE = \"DATAGRID}}\"" ) == true
                          || FullBuffer [ index ] . ToUpper ( ) . Contains ( $"TARGETTYPE=\"DATAGRIDCELL}}\"" ) == true
                          || FullBuffer [ index ] . ToUpper ( ) . Contains ( $"TARGETTYPE=\"DATAGRIDCELL\"" ) == true
                          || FullBuffer [ index ] . ToUpper ( ) . Contains ( $"TARGETTYPE = \"DATAGRIDCELL\"" ) == true )
                    {
                        offset = FullBuffer [ index ] . ToUpper ( ) . IndexOf ( $"TARGETTYPE=\"DATAGRID" );
                        if ( offset == -1 )
                            offset = FullBuffer [ index ] . ToUpper ( ) . IndexOf ( $"TARGETTYPE = \"DATAGRID" );
                        if ( offset == -1 )
                            offset = FullBuffer [ index ] . ToUpper ( ) . IndexOf ( $"TARGETTYPE=\"DATAGRIDCELL\"" );
                        if ( offset == -1 )
                            offset = FullBuffer [ index ] . ToUpper ( ) . IndexOf ( $"TARGETTYPE = \"DATAGRIDCELL\"" );
                        if ( offset != -1 )
                            isfullkey = false;

                        //                        HasEntry = true;
                        chkoffset = FullBuffer [ index ] . ToUpper ( ) . IndexOf ( $"X:KEY=\"" );
                        if ( chkoffset < offset )
                        {
                            // X:KEY is BEFORE DATAGRID
                            if ( FullBuffer [ index ] . StartsWith ( "<!--" ) == false )
                                CheckStyle ( entry, offset, isfullkey, ref Paths, ref Styles, ref FullBuffer [ index ] );
                        }
                        else
                        {
                            // DATAGRID is BEFORE X:KEY
                            testbuffer = FullBuffer [ index ] . Substring ( offset );
                            FullBuffer [ index ] = testbuffer;
                            if ( FullBuffer [ index ] . StartsWith ( "<!--" ) == false )
                                CheckStyle ( entry, offset, isfullkey, ref Paths, ref Styles, ref FullBuffer [ index ] );
                        }
                        index++;
                    }
                    else
                    {
                        index++;
                    }
                }   // END WHILE index < max
            }
            int counter = 0;
            foreach ( var item in Styles )
            {
                if ( item == "Dark Mode" )
                    fullvalidstyles . Add ( "Dark Mode" );
                else
                {
                    Debug . WriteLine ( $"DataGrid key [{item}] : identified in [{Paths [ counter ]}" );
                    fullvalidstyles . Add ( item );
                }
                counter++;
            }
            Debug . WriteLine ( $"Identified {counter} valid DataGrid styles in {fullvalidpaths . Count} Style source files in {dirs . Length} valid folders" );
            return fullvalidstyles;
        }   // METHOD END


        private static void CheckStyle ( string file, int offset, bool isfullkey, ref List<string> Paths, ref List<string> styles, ref string fullBuffer )
        {
            // This DOES maintain the much need Case Sensitivity of the Template names
            // as they are case sensitive in FindResource(), so our list will adhere to (Camel Casing)
            string Currentbuffer = "";
            string FullBuffer = "";
            string testbuffer = "", buffer = "";
            FullBuffer = fullBuffer;

            if ( styles . Count == 0 )
            {
                styles . Add ( "Dark Mode" );
                Paths . Add ( "Dark Mode" );
            }

            if ( isfullkey )
            {
                // STYLE is in search
                int chkoffset = FullBuffer . ToUpper ( ) . IndexOf ( $"STYLE X:KEY" );
                if ( chkoffset > offset && chkoffset - offset < 200 )
                {   //DATAGRID  is 1st
                    buffer = FullBuffer . Substring ( offset );
                    offset = buffer . ToUpper ( ) . IndexOf ( $"STYLE TARGETTYPE" );
                }
                else if ( chkoffset < offset )
                {   // X:KEY is 1st
                    buffer = FullBuffer . Substring ( chkoffset );
                    offset = buffer . ToUpper ( ) . IndexOf ( $"STYLE X:KEY" );
                }
                Currentbuffer = buffer;
                testbuffer = buffer;
                if ( testbuffer . ToUpper ( ) . Contains ( $"STYLE X:KEY" ) == true )
                {
                    offset = testbuffer . ToUpper ( ) . IndexOf ( $"STYLE X:KEY" );
                    string tmp = testbuffer . Substring ( offset );
                    if ( tmp . Length >= 200 )
                        testbuffer = tmp . Substring ( 0, 200 );

                    buffer = testbuffer . Substring ( 7 );
                    offset = buffer . ToUpper ( ) . IndexOf ( $"\"" );
                    buffer = buffer . Substring ( 0, offset );

                    Paths . Add ( file );
                    styles . Add ( buffer );

                    FullBuffer = Currentbuffer . Substring ( buffer . Length + 20 );
                    //FullBuffer = testbuffer;
                }
                else
                    FullBuffer = testbuffer;
            }   // END IF
            else
            {
                // STYLE not in search
                int chkoffset = FullBuffer . ToUpper ( ) . IndexOf ( $"X:KEY" );
                if ( chkoffset != -1 )
                {   //DATAGRID  is 1st
                    buffer = FullBuffer . Substring ( chkoffset + 7 );
                    offset = buffer . ToUpper ( ) . IndexOf ( $"\"" );
                    buffer = buffer . Substring ( 0, offset );
                }
                Paths . Add ( file );
                styles . Add ( buffer );
            }   // END WHILE
            fullBuffer = "";
        }
        //public static void ClearAttachedProperties ( UIElement ctrl )
        //{
        //    return;
        //    ctrl . SetValue ( MenuAttachedProperties . MouseoverBackgroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( MenuAttachedProperties . MouseoverBackgroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( MenuAttachedProperties . MousoverForegroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( MenuAttachedProperties . NormalBackgroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( MenuAttachedProperties . NormalForegroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( DataGridColumnsColorAP . HeaderBackgroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( DataGridColumnsColorAP . HeaderForegroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . BackgroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . ForegroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . BackgroundColorProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . BorderBrushProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . BorderThicknessProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . FontSizeProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . FontWeightProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . FontWeightSelectedProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . ItemHeightProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . MouseoverBackgroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . MouseoverForegroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . MouseoverSelectedBackgroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . MouseoverSelectedForegroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . SelectionBackgroundProperty , DependencyProperty . UnsetValue );
        //    ctrl . SetValue ( ListboxColorCtrlAP . SelectionForegroundProperty , DependencyProperty . UnsetValue );
        //}
        public static string ReverseString ( string input )
        {
            string output = "";
            for ( int x = input . Length - 1 ; x >= 0 ; x-- )
                output += input [ x ];
            return output;
        }

        #region Safe Object Copiers

        public static T CopyCollection<T> ( T input, T output )
        {
            output = ObjectCopier . Clone<T> ( input );
            return output;
            ;
        }

        #endregion Safe Object Copiers

        public static int GetCollectionColumnCount ( GenericClass gc )
        {
            int count = 0;
            if ( gc . field1 == null || gc . field1 == "" )
            { count = 0; return count; }
            if ( gc . field2 == null || gc . field2 == "" )
            { count = 1; return count; }
            if ( gc . field3 == null || gc . field3 == "" )
            { count = 2; return count; }
            if ( gc . field4 == null || gc . field4 == "" )
            { count = 3; return count; }
            if ( gc . field5 == null || gc . field5 == "" )
            { count = 4; return count; }
            if ( gc . field6 == null || gc . field6 == "" )
            { count = 5; return count; }
            if ( gc . field7 == null || gc . field7 == "" )
            { count = 6; return count; }
            if ( gc . field8 == null || gc . field8 == "" )
            { count = 7; return count; }
            if ( gc . field9 == null || gc . field9 == "" )
            { count = 8; return count; }
            if ( gc . field10 == null || gc . field10 == "" )
            { count = 9; return count; }
            if ( gc . field11 == null || gc . field11 == "" )
            { count = 10; return count; }
            if ( gc . field12 == null || gc . field12 == "" )
            { count = 11; return count; }
            if ( gc . field13 == null || gc . field13 == "" )
            { count = 12; return count; }
            if ( gc . field14 == null || gc . field14 == "" )
            { count = 13; return count; }
            if ( gc . field15 == null || gc . field15 == "" )
            { count = 14; return count; }
            if ( gc . field16 == null || gc . field16 == "" )
            { count = 15; return count; }
            if ( gc . field17 == null || gc . field17 == "" )
            { count = 16; return count; }
            if ( gc . field18 == null || gc . field18 == "" )
            { count = 17; return count; }
            if ( gc . field19 == null || gc . field19 == "" )
            { count = 18; return count; }
            return count;
        }


        static public FontFamily ResetFont ( string fontname )
        {
            FontFamily fontfamily = null;
            try
            {
                string test = ( string ) Properties . Settings . Default [ fontname ];
                if ( test != "" )
                    fontfamily = new FontFamily ( test );
            }
            catch ( Exception ex ) { }
            return fontfamily;
        }

        static public string GetFlowdocFont ( string font = "" )
        {
            FontFamily fontfamily = new FontFamily ( "Arial" );
            try
            {
                if ( font == "" )
                {
                    fontfamily = new FontFamily ( "Arial" );
                }
                else
                {
                    string test = ( string ) Properties . Settings . Default [ font ];
                    if ( test != "" )
                        fontfamily = new FontFamily ( test );
                }
            }
            catch ( Exception ex ) { }
            return fontfamily . ToString ( );
        }
        static public int GetDataViewerFontSize ( )
        {
            try
            {
                string test = ( string ) Properties . Settings . Default [ "DataViewerFontSize" ];
                int newweight = Convert . ToInt32 ( test );
                return newweight;
            }
            catch ( Exception ex ) { }
            return 13;
        }

        #region IENUMERABLE processing

        static public int GetIntFromEnumerable ( IEnumerable value )
        {
            // process an IEnumerable returned by Dapper Query's and return an int value
            int recordcount = 0;
            IEnumerator enummer = value . GetEnumerator ( );
            while ( enummer . MoveNext ( ) )
            {
                var val = enummer . Current . ToString ( );
                if ( val != null )
                {
                    string [ ] parts = val . Split ( '=' );
                    string val1 = parts [ 1 ] . Trim ( );
                    string [ ] more = val1 . Split ( "'" );
                    recordcount = Convert . ToInt32 ( more [ 1 ] );
                }
            }
            return recordcount;
        }

        #endregion IENUMERABLE processing

        static public ReadOnlySpan<char> SpanTrim ( string str, int start, int len )
        {
            //Faster Replacement for Substring
            ReadOnlySpan<char> span = str;
            span = span . Slice ( start, len );
            return span;
        }

        public static void ParseDictIntoGenericClass ( Dictionary<string, string> outdict, int reccount, ref GenericClass gc )
        {
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
        }

        public static string GetThemeName ( )
        {
            StringBuilder themeNameBuffer = new StringBuilder ( 260 );
            var error = GetCurrentThemeName ( themeNameBuffer, themeNameBuffer . Capacity, null, 0, null, 0 );
            if ( error != 0 )
                Marshal . ThrowExceptionForHR ( error );
            return themeNameBuffer . ToString ( );
        }
        [DllImport ( "uxtheme.dll", CharSet = CharSet . Auto )]
        public static extern int GetCurrentThemeName ( StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars );

        #region create font size range  for any control

        public static void SetComboDefaultFontSizes ( ComboBox cb, int start, int total, int defindex )
        {
            int indx = 0;
            List<string> sizes = new List<string> ( );
            for ( int x = start ; x < start + total ; x++ )
            {
                sizes . Add ( $"{x}" );
                if ( x == defindex )
                    indx = x - start;
            }
            cb . ItemsSource = sizes;
            cb . SelectedIndex = indx;
        }
        public static void SetListboxDefaultFontSizes ( ListBox lb, int start, int total, int defindex )
        {
            int indx = 0;
            List<string> sizes = new List<string> ( );
            for ( int x = start ; x < start + total ; x++ )
            {
                sizes . Add ( $"{x}" );
                if ( x == defindex )
                    indx = x - start;
            }
            lb . ItemsSource = sizes;
            lb . SelectedIndex = indx;
        }

        #endregion create font size range  for any control

        public static string GetEnvironmentalPath ( string varname )
        {
            String EnvironmentPath = System . Environment . GetEnvironmentVariable ( varname, EnvironmentVariableTarget . Process );
            string getEnv = Environment . GetEnvironmentVariable ( varname );
            if ( getEnv . EndsWith ( ";" ) )
                getEnv = getEnv . Substring ( 0, getEnv . Length - 1 );
            return getEnv;
        }

        static public FontWeight GetfontWeight ( string type )
        {
            FontWeight fontWeight =new();
            int newweight = 0;
            switch ( type )
            {
                case "Normal":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . Normal );
                    break;
                case "Black":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . Black );
                    break;
                case "UltraBold":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . UltraBold );
                    break;
                case "DemiBold":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . DemiBold );
                    break;
                case "Regular":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . Regular );
                    break;
                case "Heavy":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . Heavy );
                    break;
                case "ExtraBold":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . ExtraBold );
                    break;
                case "Bold":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . Bold );
                    break;
                case "SemiBold":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . SemiBold );
                    break;
                case "Medium":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . Medium );
                    break;
                case "ExtraLight":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . ExtraLight );
                    break;
                case "Thin":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . Thin );
                    break;
                case "UltraLight":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . UltraLight );
                    break;
                case "Light":
                    newweight = Convert . ToInt32 ( MainWindow . fontWeight . Light );
                    break;
            }
            FontWeight fw = FontWeight . FromOpenTypeWeight ( newweight );
            return fw;
        }
        static public Dictionary<string, SolidColorBrush> GetAllSysBrushColors ( Dictionary<string, SolidColorBrush> colordict )
        {
        //    Dictionary<string, SolidColorBrush> colordict = new();
            List<string> newcolor = new();
            Type brushesType = typeof(System.Windows.Media.Brushes);
            // Get all static properties
            var properties = brushesType.GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach ( var prop in properties )
            {
                string name = prop.Name;
                SolidColorBrush color = prop . GetValue ( null, null ) as SolidColorBrush;
                newcolor . Add ( name );
                colordict.Add ( name, color );
            }
            return colordict;
        }
    }
}