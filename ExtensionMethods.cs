#define USECW
#define USETRACK
#undef USETRACK
using System;
using System . Diagnostics;
using System . IO;
using System . Runtime . CompilerServices;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Media;
using System . Windows . Threading;

namespace Wpfmain
{
    public static class ExtensionMethods
    {

        private static Action EmptyDelegate = delegate ( ) { };
        //Snippet = trk
        //[Conditional ( "USECW" )]
        public static void Track (
            this string message,
            int direction = 0,
            int level = 1,
            [CallerFilePath] string path = null,
            [CallerMemberName] string name = null,
            [CallerLineNumber] int line = -1 )
        {
#if USETRACK
            if ( level == 0 ) return;
            string [ ] tmp = path . Split ( '\\' );
            int len = tmp . Length;
            string filedetails = $"{tmp [ len - 3 ]}/{tmp [ len - 2 ]}/{tmp [ len - 1 ]}";
            if ( MainWindow . LOGTRACK )
            {
                if ( direction == 0 )
                    File . AppendAllText ( $@"C:\users\ianch\Documents\NewWpfDev.Trace.log" , $"** IN  ** : {DateTime . Now .Second} / {DateTime.Now.Millisecond} : {line} :  {filedetails} : {name . ToUpper ( )}\t{message}\n" );
                else
                    File . AppendAllText ( $@"C:\users\ianch\Documents\NewWpfDev.Trace.log" , $"** OUT ** : {DateTime . Now . Second} / {DateTime . Now . Millisecond} : {line} :  {filedetails} : {name . ToUpper ( )}\t {message}\n" );

            }
            if ( direction == 0 )
                Debug . WriteLine ( $"** TRACK - IN ** : [{DateTime . Now . Minute}:{DateTime . Now . Second}:{DateTime . Now . Millisecond}] : {line} :  {filedetails} : {name . ToUpper ( )}\t : {message}" );
            else
                Debug . WriteLine ( $"** TRACK - OUT ** : [{DateTime . Now . Minute}:{DateTime . Now . Second}:{DateTime . Now . Millisecond}] : {line} :  {filedetails} : {name . ToUpper ( )}\t : {message}" );
#endif
        }
        public static void log (
            this string message,
            [CallerLineNumber] int line = -1,
            [CallerFilePath] string path = null,
            [CallerMemberName] string name = null )
        {
            string output = "";
            output = line < 0 ? "No line  : " : "Line " + $"{line} : ";
            output += path == null ? "No file path" : $"\t{path}  : ";
            output += name == null ? " No member name" : $"( {name} )";
            Debug . WriteLine ( $"{output}\n{message}" );
        }

        public static void DapperTrace (
            this string message,
            int level = 1,
            [CallerFilePath] string path = null,
            [CallerMemberName] string name = null,
            [CallerLineNumber] int line = -1 )
        {
            string newpath = "";
            string [ ] parts = path . Split ( '\\' );
            int partslength = parts . Length;
            for ( int x = partslength - 2 ; x < partslength ; x++ )
                newpath += $"{parts [ x ]}\\";
            newpath = newpath . Substring ( 0, newpath . Length - 1 );
            if ( message == "" ) message = $"Default processing trace...";
            Debug . WriteLine ( $"\nTRACE : {line} : {newpath}\\{name} \nExecuting : [ {message . ToUpper ( )} ]" );
        }

        public static void CW (
            this string message,
            int level = 1,
            [CallerFilePath] string path = null,
            [CallerMemberName] string name = null,
            [CallerLineNumber] int line = -1 )
        {
            if ( level == 0 ) return;
            string [ ] tmp = path . Split ( '\\' );
            string errmsg = $"\n{name} : {line} ";
            errmsg += $"in {tmp [ tmp . Length - 2 ]}";
            errmsg += $"\\{tmp [ tmp . Length - 1 ]}\n**INFO** = [  {message} ]";
            Debug . WriteLine ( $"\n{errmsg}" );
            if ( MainWindow . LogCWOutput )
                File . AppendAllText ( @"C:\users\ianch\documents\CW.log", errmsg );
        }
        //-------------------------------------------------------------------------------------------------------//
        //Snippet = cwe
        //[Conditional ( "USECW" )]
        public static void cwerror (
            this string message,
            int level = 1,
            [CallerFilePath] string path = null,
            [CallerMemberName] string name = null,
            [CallerLineNumber] int line = -1 )
        {
            if ( level == 0 ) return;
            string [ ] tmp = path . Split ( '\\' );
            if(tmp.Length >=6 )
            Debug . WriteLine ( $"** ERROR ** : {line} ** {message} ** : : {name} (.) in {tmp [ 5 ] + "\\" + tmp [ 6 ]}" );
            else
                Debug . WriteLine ( $"** ERROR ** : {line} ** {message} ** : : {name} " );

        }
        //-------------------------------------------------------------------------------------------------------//
        //Snippet = cww
        //[Conditional ( "USECW" )]
        public static void cwwarn (
            this string message,
            int level = 1,
            [CallerFilePath] string path = null,
            [CallerMemberName] string name = null,
            [CallerLineNumber] int line = -1 )
        {
            if ( level == 0 ) return;
            string [ ] tmp = path . Split ( '\\' );
            Debug . WriteLine ( $"WARN : {line} ** {message} ** : : {name} (.) in {tmp [ 5 ] + "\\" + tmp [ 6 ]}" );
        }
        //-------------------------------------------------------------------------------------------------------//
        //Snippet = cwi
        //[Conditional ( "USECW" )]
        public static void cwinfo (
            this string message,
            int level = 1,
            [CallerFilePath] string path = null,
            [CallerMemberName] string name = null,
            [CallerLineNumber] int line = -1 )
        {
            if ( level == 0 ) return;
            string [ ] tmp = path . Split ( '\\' );
            string namestr = $"{name + " ()" . PadRight ( 25 )}";
            Debug . WriteLine ( $"INFO : [{DateTime . Now . Minute}:{DateTime . Now . Second}:{DateTime . Now . Millisecond}] : {line . ToString ( ) . PadRight ( 6 )} : {namestr . Trim ( )} ::** {message . PadRight ( 20 )}  : : File= {tmp [ 5 ] + "\\" + tmp [ 6 ]}" );
        }
        //-------------------------------------------------------------------------------------------------------//
        public static void trace (
            this string message,
            int level = 0,
            [CallerFilePath] string path = null,
            [CallerMemberName] string name = null,
            [CallerLineNumber] int line = -1 )
        {
            if ( level == 0 ) return;
            string [ ] tmp = path . Split ( '\\' );
            string namestr = $"{name + " ()" . PadRight ( 25 )}";
            Debug . WriteLine ( $"TRACE : [{DateTime . Now . Minute}:{DateTime . Now . Second}:{DateTime . Now . Millisecond}] : {line . ToString ( ) . PadRight ( 6 )} : {namestr . Trim ( )} ::** {message . PadRight ( 20 )}  : : File= {tmp [ 5 ] + "\\" + tmp [ 6 ]}" );
        }
        public static void sprocstrace (
             this string message,
             int level = 0,
             [CallerFilePath] string path = null,
             [CallerMemberName] string name = null,
             [CallerLineNumber] int line = -1 )
        {
            if ( level == 0 ) return;
            string [ ] tmp = path . Split ( '\\' );
            string namestr = $"{name + " ()" . PadRight ( 25 )}";
            int maxargs = tmp.Length;
            if ( maxargs > 6 ) 
            {
                Debug . WriteLine ( $"SPROCS-TRACE : [{DateTime . Now . Minute}:{DateTime . Now . Second}:{DateTime . Now . Millisecond}] : {line . ToString ( ) . PadRight ( 6 )} : {namestr . Trim ( )} ::** {message . PadRight ( 20 )}  : : File= {tmp [ 5 ] + "\\" + tmp [ 6 ]}" );
            }
            else if ( maxargs > 5 )
            {
                Debug . WriteLine ( $"SPROCS-TRACE : [{DateTime . Now . Minute}:{DateTime . Now . Second}:{DateTime . Now . Millisecond}] : {line . ToString ( ) . PadRight ( 6 )} : {namestr . Trim ( )} ::** {message . PadRight ( 20 )}  : : File= {tmp [ 4 ] + "\\" + tmp [ 5 ]}" );
            }
            else if ( maxargs > 5 ) 
            {
                Debug . WriteLine ( $"SPROCS-TRACE : [{DateTime . Now . Minute}:{DateTime . Now . Second}:{DateTime . Now . Millisecond}] : {line . ToString ( ) . PadRight ( 6 )} : {namestr . Trim ( )} ::** {message . PadRight ( 20 )}  : : File= {tmp [ 4 ] + "\\" + tmp [ 5 ]}" );
            }
            else if ( maxargs > 4 )
            {
                Debug . WriteLine ( $"SPROCS-TRACE : [{DateTime . Now . Minute}:{DateTime . Now . Second}:{DateTime . Now . Millisecond}] : {line . ToString ( ) . PadRight ( 6 )} : {namestr . Trim ( )} ::** {message . PadRight ( 20 )}  : : File= {tmp [ 3 ] + "\\" + tmp [ 4 ]}" );
            }
            else if ( maxargs > 3 )
            {
                Debug . WriteLine ( $"SPROCS-TRACE : [{DateTime . Now . Minute}:{DateTime . Now . Second}:{DateTime . Now . Millisecond}] : {line . ToString ( ) . PadRight ( 6 )} : {namestr . Trim ( )} ::** {message . PadRight ( 20 )}  : : File= {tmp [ 2 ] + "\\" + tmp [ 3 ]}" );
            }
        }


        public static void err (
            this string message,
            int level = 1,
            [CallerFilePath] string path = null,
            [CallerMemberName] string name = null,
            [CallerLineNumber] int line = -1,
            [CompilerGenerated] bool isCompilerGenerated = false )
        {
            string cgen="";
            if ( level == 0 ) return;
            string [ ] tmp = path . Split ( '\\' );
            string namestr = $"{name + " ()" . PadRight ( 5 )}";
            string date = DateTime . Now .ToShortTimeString ( );
            if ( isCompilerGenerated )
                cgen = "Compiler Error";
            Debug . WriteLine ( $"## Error ## : {date} : ** {message . PadRight ( 20 )}\n\tAt :{line . ToString ( ) . PadRight ( 6 )} : {namestr} : {tmp [ 5 ] + "\\" + tmp [ 6 ]} : {cgen}" );
        }
        //-------------------------------------------------------------------------------------------------------//
        public static void Refresh ( this UIElement uiElement )
        {
            try
            {
                uiElement . Dispatcher . Invoke ( DispatcherPriority . Render, EmptyDelegate );
            }
            catch ( Exception ex ) { Debug . WriteLine ( $"REFRESH FAILEd !!  {ex . Message}" ); }
        }

        public static void RefreshGrid ( this Control uiElement )
        {
            try
            {
                uiElement . Dispatcher . Invoke ( DispatcherPriority . Render, EmptyDelegate );
            }
            catch ( Exception ex ) { Debug . WriteLine ( $"REFRESH FAILEd !!  {ex . Message}" ); }
        }


        public static Brush ToSolidColorBrush ( this string HexColorString )
        {
            if ( HexColorString . Length != 9 )
            {
                MessageBox . Show ( "The Hex value entered is invalid. It needs to be # + 4 hex pairs\n\neg: [#FF0000FF] = BLUE " );
                return null;
            }
            try
            {
                if ( HexColorString != null && HexColorString != "" )
                    return ( SolidColorBrush ) System . Windows . Application . Current . FindResource ( HexColorString );
                else
                    return null;
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"ToSolidColorBrush failed - input = {HexColorString}" );
                return null;
            }
        }

        public static Brush ToSolidBrush ( this string HexColorString )
        {
            if ( HexColorString . Length < 9 )
            {
                //				MessageBox.Show( "The Hex value entered is invalid. It needs to be # + 4 hex pairs\n\neg: [#FF0000FF] = BLUE ");
                return null;
            }
            try
            {
                if ( HexColorString != null && HexColorString != "" )
                    return ( Brush ) ( new BrushConverter ( ) . ConvertFrom ( HexColorString ) );
                else
                    return null;
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"ToSolidbrush failed - input = {HexColorString}" );
                return null;
            }
        }
        public static LinearGradientBrush ToLinearGradientBrush ( this string Colorstring )
        {
            try
            {
                return Application . Current . FindResource ( Colorstring ) as LinearGradientBrush;
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"ToLinearGradientbrush failed - input = {Colorstring}" );
                return null;
            }
            //return ( LinearGradientBrush ) ( new BrushConverter ( ) . ConvertFrom ( color ) );
        }
        public static string BrushtoText ( this Brush brush )
        {
            try
            {
                if ( brush != null )
                    return ( string ) brush . ToString ( );
                else
                    return null;
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"BrushtoText failed - input = {brush}" );
                return null;
            }
        }
        //public static string ToBankRecordCommaDelimited ( this BankAccountViewModel record )
        //{
        //    BankAccountViewModel bvm = new BankAccountViewModel ( );
        //    string [ ] fields = { "" , "" , "" , "" , "" , "" , "" , "" , "" };
        //    fields [ 0 ] = record . Id . ToString ( );
        //    fields [ 1 ] = record . BankNo . ToString ( );
        //    fields [ 2 ] = record . CustNo . ToString ( );
        //    fields [ 3 ] = record . Balance . ToString ( );
        //    fields [ 4 ] = record . IntRate . ToString ( );
        //    fields [ 5 ] = record . AcType . ToString ( );
        //    fields [ 6 ] = record . ODate . ToString ( );
        //    fields [ 7 ] = record . CDate . ToString ( );
        //    return fields [ 0 ] + "," + fields [ 1 ] + "," + fields [ 2 ] + "," + fields [ 3 ] + "," + fields [ 4 ] + "," + fields [ 5 ] + "," + fields [ 6 ] + "," + fields [ 7 ] + "\n";
        //}

    }
}
