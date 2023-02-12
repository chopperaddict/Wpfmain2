using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data;
using System . Diagnostics;
using System . Diagnostics . Eventing . Reader;
using System . Linq;
using System . Windows;
using System . Windows . Markup;

using Dapper;

using ViewModels;

using Wpfmain;
using Wpfmain . Dapper;

namespace SprocsProcessing
{
    public class SpExecution
    {
        /// <summary>
        /// All methods in this file are standard Dapper requests using SQLCOMMAND
        /// and  returns a dynamic value of the relevant type.
        /// </summary>
        static public SpExecution spe = null;
        static public string CurrentDb = "IAN1";
        static public string Sqlcommand = "";
        public SProcsHandling sphandling = SProcsHandling . GetSProcsHandling ( );
        const int DEFAULTARGSSIZE = 6;
        public SpExecution ( string currentdb )
        {
            spe = this;
            CurrentDb = currentdb;
        }
        static public SpExecution GetSpExecution ( )
        {
            return spe;
        }
        public dynamic Execute_click ( string SqlCommand, ref int Count, ref string ResultString, ref Type Objtype, ref object Obj, out string Err )
        {
            // called when executing an SP
            string currentSp = sphandling . SProcsListbox . SelectedItem as string;
            string [ ] args1 = null;
            string [ ] args = new string [ 0 ];
            bool HasFailed = false;
            // Initiaize ref variables

            Count = 0;
            string Resultstring = "";
            Objtype = null;
            Err = "";
            Sqlcommand = SqlCommand;

            SProcsHandling sph = SProcsHandling . GetSProcsHandling ( );
            if ( sph == null )
            {
                Err = "Processing code encountered an error, it was unable to get a pointer to SProcsHandling() Control...\n\nPlease report this to your Software Support Engineers !";
                Utils2 . DoErrorBeep ( );
                return null;
            }
            string UserArguments = sph . SPArguments . Text;
            if ( UserArguments == "Argument(s) required ?" )
            {
                MessageBox . Show ( "You MUST enter at least the name of the S.P to be processed before it can be executed !", "Execution processing error" );
                return null;
            }
            "" . sprocstrace ( 0 );
            // PARSE THE ARGUMENTS ENTERED BY  OUR  USER
            int cnt = 0;
            string [ ] fullargs = new string [ SProcsHandling . DEFAULTARGSSIZE ];
            string [ ] argparts = new string [ SProcsHandling . DEFAULTARGSSIZE ];
            string [ ] parts = new string [ SProcsHandling . DEFAULTARGSSIZE ];
            string [ ] testcontent = new string [ 1 ];
            string [ ] tempargs = null;
            string valid = "0123456789";
            string validstrings = "STRING NVARCHAR VARCHAR TEXT";
            string validnumerics = "INT DOUBLE FLOAT CURRENCY REAL DATE DATETIME ";
            bool GotCommas = false;
            List<string [ ]> argsbuffer = new List<string [ ]> ( );
            List<string [ ]> parsedbuffer = new List<string [ ]> ( );

            if ( UserArguments != "" && UserArguments . Contains ( "No arguments are required" ) == false )
            {
                tempargs = new string [ 1 ];
                // splt mutliple args into individual strings, (possibly  with more than one  item in each of them) ?
                if ( UserArguments . Contains ( ":" ) )
                {
                    tempargs = UserArguments . Trim ( ) . ToUpper ( ) . Split ( ':' );
                    for ( int x = 0 ; x < tempargs . Length ; x++ )
                    {
                        tempargs [ x ] = tempargs [ x ] . TrimStart ( ) . TrimEnd ( );
                    }
                }
                else
                {
                    // only one set ! ?
                    tempargs [ 0 ] = UserArguments . Trim ( ) . ToUpper ( );
                }
                /* process each set of arguments we have in testcontent[]  and split to its constituent parts (name, value, type, size, direction)
                based on spaces(or comas) between sections of the argument
                fill parts  with the processed fields from the current arg string

                structure used FOR ALL ENTRIES MUST ADHERE TO THESE 5 elements & field offset:-
               0 - @arg (SP argument name)
               1 - Target object (in first argument only, must be SQL command or SP name)
               2 - data type STRING/INT etc (Optional)
               3 - direction (INPUT / OUTPUT/ RETURN)
               4 - size (if relevant) (Optional)
                EG:  @ARG SQLTABLE STRING INPUT 500
                */
                argsbuffer . Clear ( );
                //Spllit tempargs [ ] string into seperate parts (args[] on either comma or space
                if ( tempargs != null )
                {
                    int number = 0;
                    try
                    {
                        //parse command line sections into args structures
                        for ( int y = 0 ; y < tempargs . Length ; y++ )
                        {
                            args = new string [ SProcsHandling . DEFAULTARGSSIZE ];
                            args = PadArgsArray ( args );
                            // Check for error conditions first
                            bool NoArgument = false;
                            if ( tempargs [ y ] . Contains ( "NO PARAMETERS" ) == true )
                                NoArgument = true;
                            if ( tempargs [ y ] . Contains ( "APPEAR TO BE INVALID" ) == true )
                                NoArgument = true;

                            if ( NoArgument == false )
                            {
                                if ( tempargs [ y ] . Contains ( " " ) )
                                {
                                    // split content into fields
                                    string[] temp = tempargs[y].Split(" ");

                                    for ( int j = 0 ; j < temp . Length ; j++ )
                                    {
                                        if ( temp [ j ] != null )
                                            temp [ j ] = temp [ j ] . Trim ( );
                                    }
                                    // need to remove anny spare spaces to avoid  issues
                                    string[] tmp = new string[SProcsHandling . DEFAULTARGSSIZE ];
                                    tmp = PadArgsArray ( tmp );

                                    int newindx=0; ;
                                    for ( int j = 0 ; j < temp . Length ; j++ )
                                    {
                                        if ( temp [ j ] != "" )
                                            tmp [ newindx++ ] = temp [ j ];
                                    }
                                    temp = tmp;

                                    args [ 0 ] = temp [ 0 ];
                                    if ( temp [ 0 ] . Length > 1 )
                                        args [ 1 ] = temp [ 1 ];
                                    if ( temp . Length > 2 )
                                    {
                                        if ( temp [ 2 ] != "" )
                                            args [ 2 ] = temp [ 2 ]; ;
                                    }
                                    if ( temp . Length > 3 )
                                        args [ 3 ] = temp [ 3 ];
                                    if ( temp . Length > 4 )
                                        args [ 4 ] = temp [ 4 ];
                                }
                                else
                                    args [ 0 ] = tempargs [ 0 ];
                            }
                            else
                                args [ 0 ] = "";
                            argsbuffer . Add ( args );
                        }
                    }
                    catch ( Exception ex )
                    {
                        Debug . WriteLine ( $"Parsing error : {ex . Message}, {ex . Data}" );
                        Err = $"Parsing error : {ex . Message}, {ex . Data}";
                        Utils2 . DoErrorBeep ( );
                        "" . sprocstrace ( 1 );
                        return null;
                    }
                    bool isarguments = false;
                    for(int x= 0 ; x < args.Length ; x++ )
                    {
                        if ( args [ x ] != "" )
                        {
                            isarguments = true;
                            break;
                        }
                    }
                    if ( isarguments == true )
                    {
                        try
                        {
                            // now put whatever args contains into our MAIN set (args[]) in correct position
                            // and finally add them to Argsbuffer list
                            int argsbuffcount= 0;
                            int itemlen=0;
                            for ( int i = 0 ; i < argsbuffer . Count ; i++ )
                            {
                                args = new string [ SProcsHandling . DEFAULTARGSSIZE ];
                                args = PadArgsArray ( args );
                                parts = argsbuffer [ i ];
                                for ( int x = 0 ; x < parts . Length ; x++ )
                                {
                                    if ( x == 0 && parts [ x ] . StartsWith ( "@" ) == false )
                                    {
                                        Err = $"Argument name {parts [ x ]} does not conform to ( @....... ) required format , processing of Execution method has been cancelled";
                                        HasFailed = true;
                                        break;
                                    }
                                    else if ( x == 0 )
                                    {
                                        args [ x ] = parts [ x ];
                                        continue;
                                    }
                                    if ( parts [ x ] != "" && ( parts [ x ] == "INPUT" || parts [ x ] == "OUTPUT" || parts [ x ] == "OUT" || parts [ x ] == "RETURN" ) )
                                    {
                                        args [ 3 ] = parts [ x ];
                                        continue;
                                    }
                                    else if ( parts [ x ] != "" && ( parts [ x ] == "STRING" || parts [ x ] == "INTEGER" || parts [ x ] == "DECIMAL" || parts [ x ] == "DOUBLE" ) )
                                    {
                                        args [ 2 ] = parts [ x ];
                                        continue;
                                    }
                                    if ( x == 1 )
                                    {
                                        args [ x ] = parts [ x ];
                                        continue;
                                    }
                                    if ( x == 2 )
                                    {
                                        if ( parts [ x ] != "" && ( parts [ x ] == "INPUT" || parts [ x ] == "OUTPUT" || parts [ x ] == "OUT" || parts [ x ] == "RETURN" ) )
                                        {
                                            args [ 3 ] = parts [ x ];
                                        }
                                        else
                                        {
                                            // ensure it isn't a Size ....
                                            if ( IsNumericString ( parts [ x ], ref itemlen ) == true )
                                                args [ 4 ] = parts [ x ];
                                            else
                                                args [ x ] = parts [ 2 ];
                                        }
                                        continue;
                                    }
                                    if ( x == 3 )
                                    {
                                        if ( parts [ x ] != "" && ( parts [ x ] == "INPUT" || parts [ x ] == "OUTPUT" || parts [ x ] == "OUT" || parts [ x ] == "RETURN" ) )
                                        {
                                            args [ 3 ] = parts [ x ];
                                        }
                                        else
                                        {
                                            if ( IsNumericString ( parts [ x ], ref itemlen ) == true )
                                                args [ 4 ] = parts [ x ];
                                            else
                                                args [ x ] = "";
                                        }
                                        continue;
                                    }
                                    if ( x == 4 )
                                    {
                                        if ( parts [ 4 ] == "MAX" )
                                            args [ 4 ] = "64000";
                                        else if ( parts [ 4 ] != "" )
                                        {
                                            int retval=0;
                                            IsNumericString ( parts [ x ], ref retval );
                                            if ( retval != 0 )
                                                args [ 4 ] = retval . ToString ( );
                                        }
                                        else
                                            args [ 4 ] = "";
                                    }
                                    if ( HasFailed )
                                        return null;
                                }
                                parsedbuffer . Add ( args );
                            }
                        }
                        catch ( Exception ex )
                        {
                            Debug . WriteLine ( $"Parsing error : {ex . Message}, {ex . Data}" );
                            Err = $"Parsing error : {ex . Message}, {ex . Data}";
                            Utils2 . DoErrorBeep ( );
                            "" . sprocstrace ( 1 );
                            return null;
                        }

                        // Finally Add this set of now validated args to our arguments list we pass to the SQL Query
                        string err="";
                        int totalerrcount= 2 * 2;
                        int errcount=0;
                        int z = 0;
                        foreach ( string [ ] item in parsedbuffer )
                        {
                            if ( z == 0 && ( item [ 0 ] == "" || item [ 1 ] == "" ) )
                            {
                                errcount++;
                                z++;
                            }
                            else if ( z == 1 && item [ 0 ] == "" )
                            {
                                errcount++;
                                z++;
                                continue;
                            }
                            else
                                z++;
                        }
                        if ( errcount > 0 )
                        {
                            if ( err != "" )
                                Err = $"Execution code identified {errcount} error(s) in the {totalerrcount} mandatory arguments. \nThe error is that one or more arguments were not provided";
                            else
                                Err = $"Execution code identified {errcount} error(s) in the {totalerrcount} mandatory arguments. \n";
                            return null;
                        }
                    // DEBUG : show args in output window
                    Debug . WriteLine ( $"\nArguments received for {Sqlcommand . ToUpper ( )}" );
                    PrintSPArgs ( parsedbuffer );
                    argsbuffer = parsedbuffer;
                    }
                    else argsbuffer.Clear ();
                }
            }

            //==========================================================================================//
            dynamic result = ExecuteArgument ( SqlCommand, argsbuffer, args, ref Count, ref ResultString, ref Obj, ref Objtype, ref Err, method: 99, currentSp );
            //==========================================================================================//

            "" . sprocstrace ( 1 );
            return result;
        }

        public dynamic ExecuteArgument (
            string SqlCommand,
            List<string [ ]> argsbuffer,
            string [ ] args,
            ref int Count,
            ref string ResultString,
            ref object Obj,
            ref Type Objtype,
            ref string Err,
            int method = -1,
            string Sprocname = "" )
        {
            //*************************************************//
            // WE have now
            // finished parsing the FIRST argument
            // WE have @argname, data type, size as integer,
            // and direction as INPUT, OUTPUT or RETURN
            //*************************************************//
            int innercount = Count;
            string innerresultstring = ResultString;
            object innerobj = Obj;
            string innerrerr = Err;
            // get executoin methoood to be used
            string operationtype = sphandling . ExecList . SelectedItem as string;
            List<string [ ]> tmplist = new ( );
            // Get SP name (from SP listbox)
            string SpCommand = $"{sphandling . SProcsListbox . SelectedItem . ToString ( )}";
            if ( Sprocname == "" ) Sprocname = SqlCommand;
            string fulname = $"{CurrentDb}.{Sprocname}";
            if ( ( method == -1 && operationtype == null ) )
            {
                MessageBox . Show ( "You MUST select an Execution Method before the selected S.P can be executed !", "Execution processing error" );
                return null;
            }
            "" . sprocstrace ( 0 );
            int count = argsbuffer.Count;
            if ( count >= 1 )
            {
                // Not sure I need to do thiis here ????
                for ( int x = 0 ; x < argsbuffer . Count ; x++ )
                {
                    args = new string [ SProcsHandling . MAXARGSIZE ];
                    if ( argsbuffer [ x ] [ 0 ] == "" )
                    {
                        // Error arg[0] cannot be empty
                        Err = $"an Argument name is missing, processing cannot proceed";
                        break;
                    }
                    else
                    {
                        // @Arg name
                        args [ 0 ] = argsbuffer [ x ] [ 0 ];
                    }
                    // Argument data (table name etc)
                    args [ 1 ] = argsbuffer [ x ] [ 1 ];

                    // Include arguent type....
                    if ( argsbuffer [ x ] [ 2 ] != null && argsbuffer [ x ] [ 2 ] != "" )
                        args [ 2 ] = argsbuffer [ x ] [ 2 ];
                    else
                        args [ 2 ] = "STRING";

                    if ( argsbuffer [ x ] [ 3 ] != null && argsbuffer [ x ] [ 3 ] != "" )
                        args [ 3 ] = argsbuffer [ x ] [ 3 ];
                    else
                        args [ 3 ] = "INPUT";

                    if ( argsbuffer [ x ] [ 4 ] != null && argsbuffer [ x ] [ 4 ] != "" )
                        args [ 4 ] = argsbuffer [ x ] [ 4 ];
                    else if ( argsbuffer [ x ] [ 4 ] != "" )
                    {
                        int num=0;
                        IsNumericString ( argsbuffer [ x ] [ 4 ], ref num );
                        if ( num != 0 )
                            args [ 4 ] = argsbuffer [ x ] [ 4 ];
                        else
                            args [ 4 ] = "";
                    }
                    else args [ 4 ] = "";
                    // add to our list of string[] arguments to pass to final methods
                    tmplist . Add ( args );
                    // always blank this out.....
                    args [ 5 ] = "";
                }
            }
            if ( Err != "" )
                return null;

            //*****************************//
            //NOW PROCESS the request
            //*****************************//
            try
            {
                if ( count >= 1 )
                {
                    argsbuffer . Clear ( );
                    argsbuffer = tmplist;
                }
                string output = "";
                // Now find out what method we are going to use
                //*****************************************************************************************//
                if ( method == 1 || operationtype == "4. SP returning an INT value" )
                //*****************************************************************************************//
                {
                    //// tell method what we are expecting back
                    Objtype = typeof ( int );

                    dynamic intresult = GenDapperQueries . Get_DynamicValue_ViaDapper ( SpCommand,
                        argsbuffer,
                        ref innerresultstring,
                        ref innerobj,
                        ref Objtype,
                        ref innercount,
                        ref Err,
                        6 );

                    if ( Err != "" && innerresultstring == "" )
                    {
                        //if ( ReturnProcedureHeader ( SqlCommand, SqlCommand == "DONE" )
                        //    return ( dynamic ) null;
                        ShowError ( operationtype, Err );
                        "" . sprocstrace ( 1 );
                        return ( dynamic ) null;
                    }

                    if ( intresult != null )
                    {
                        // TODO Maybe wrong  8/11/2022
                        ResultString = innerresultstring;
                        Obj = ( object ) intresult;
                        Objtype = typeof ( Int32 );
                        Count = innercount;
                        "" . sprocstrace ( 1 );
                        return ( dynamic ) innerobj;
                    }
                }
                //*****************************************************************************************//
                else if ( method == 2 || operationtype == "3. SP returning a String" )
                //*****************************************************************************************//
                {
                    //Use storedprocedure  version
                    SpCommand = $"{sphandling . SProcsListbox . SelectedItem . ToString ( )}";

                    // tell method what we are expecting back
                    Objtype = typeof ( string );

                    dynamic stringresult = GenDapperQueries . Get_DynamicValue_ViaDapper (
                        SpCommand,
                        argsbuffer,
                        ref innerresultstring,
                        ref innerobj,
                        ref Objtype,
                        ref innercount,
                        ref Err,
                        2 );

                    // Working 8/11/2022
                    if ( Err != "" )
                    {
                        "" . sprocstrace ( 1 );
                        if ( ReturnProcedureHeader ( SpCommand, sphandling . SProcsListbox . SelectedItem . ToString ( ) ) == "DONE" )
                            return ( dynamic ) null;
                        ShowError ( operationtype, Err );
                    }
                    ResultString = innerresultstring;
                    Obj = ( object ) stringresult;
                    Objtype = typeof ( string );
                    Count = innercount;

                    "" . sprocstrace ( 1 );
                    if ( Objtype == typeof ( string ) )
                        return stringresult;
                    else
                        return stringresult . ToString ( );
                }
                //*****************************************************************************************//
                else if ( method == 3 || operationtype == "2. SP returning a List<string>" )
                //*****************************************************************************************//
                {
                    SpCommand = $"{sphandling . SProcsListbox . SelectedItem . ToString ( )}";
                    // tell method what we are expecting back
                    Objtype = typeof ( List<string> );

                    dynamic stringlist = GenDapperQueries . Get_DynamicValue_ViaDapper (
                        SpCommand,
                        argsbuffer,
                        ref innerresultstring,
                        ref innerobj,
                        ref Objtype,
                        ref innercount,
                        ref Err,
                        5 );

                    ResultString = innerresultstring;
                    Obj = ( object ) stringlist;
                    Objtype = typeof ( List<string> );
                    Count = innercount;
                    "" . sprocstrace ( 1 );
                    if ( Objtype == typeof ( List<string> ) )
                        return ( dynamic ) stringlist;
                    else
                        return ( dynamic ) null;

                }
                //*****************************************************************************************//
                else if ( method == -1 || method == 4 || operationtype == "1. SP returning a Table as ObservableCollection" )
                //*****************************************************************************************//
                {
                    //DatagridControl dgc = new ( );

                    // tell method what we are expecting back
                    Objtype = typeof ( ObservableCollection<GenericClass> );
                    IEnumerable<dynamic> tableresult = null;
                    // Should normally  be  '[spLoadTableAsGeneric]' but can be any SP that wants a collection back
                    if ( operationtype == null && method == -1 )
                    {
                        tableresult = GenDapperQueries . Get_DynamicValue_ViaDapper (
                         sphandling . SProcsListbox . SelectedItem . ToString ( ),
                        argsbuffer,
                         ref innerresultstring,
                         ref innerobj,
                         ref Objtype,
                         ref innercount,
                         ref Err,
                         4 );
                    }
                    else
                    {
                        tableresult = GenDapperQueries . Get_DynamicValue_ViaDapper (
                         Sprocname,
                        argsbuffer,
                         ref innerresultstring,
                         ref innerobj,
                         ref Objtype,
                         ref innercount,
                         ref Err,
                         0 );
                    }
                    ResultString = innerresultstring;
                    Obj = ( object ) tableresult;
                    Objtype = typeof ( ObservableCollection<GenericClass> );
                    //                  Objtype = typeof ( IEnumerable<dynamic> );
                    Count = innercount;
                    "" . sprocstrace ( 1 );
                    if ( Count > 0 )
                        return Obj;
                    else
                        return ( dynamic ) null;
                    //********************************************************************************//
                }
                else if ( operationtype == "6. Execute Stored Procedure without return value" )
                {
                    dynamic dynreslt  = DapperGeneric.ExecuteWithDapper (  sphandling . SProcsListbox . SelectedItem . ToString ( ), argsbuffer, out int result , out string innererror);
                    Err = innererror;
                    if ( dynreslt != null )
                    {
                        ResultString = "SUCCESS";
                        Obj = ( object ) result;
                        Objtype = null;
                        "" . sprocstrace ( 1 );
                        return ( dynamic ) dynreslt;
                    }
                    // Working 8/11/2022
                    if ( Err != "" )
                    {
                        "" . sprocstrace ( 1 );
                        if ( ReturnProcedureHeader ( SpCommand, sphandling . SProcsListbox . SelectedItem . ToString ( ) ) == "DONE" )
                            return ( dynamic ) null;
                        ShowError ( operationtype, Err );
                    }
                    "" . sprocstrace ( 1 );
                    return dynreslt;
                }
                else if ( operationtype == "5. Execute Stored Procedure with return value" )
                {
                    dynamic dynreslt  = DapperGeneric.ExecuteWithDapper (  sphandling . SProcsListbox . SelectedItem . ToString ( ), argsbuffer, out int result , out string innererror);
                    Err = innererror;
                    // Working 8/11/2022
                    if ( Err != "" )
                    {
                        "" . sprocstrace ( 1 );
                        if ( ReturnProcedureHeader ( SpCommand, sphandling . SProcsListbox . SelectedItem . ToString ( ) ) == "DONE" )
                            return ( dynamic ) null;
                        ShowError ( operationtype, Err );
                    }
                    "" . sprocstrace ( 1 );
                    return dynreslt;
                }
            }
            catch ( Exception ex )
            {
                Utils . DoErrorBeep ( );
                Debug . WriteLine ( $"Execute_Click ERROR : \n{ex . Message}\n{ex . Data}" );
            }
            "" . sprocstrace ( 1 );
            return ( dynamic ) null;
        }
        static public DynamicParameters ParseNewSqlArgs ( DynamicParameters parameters, List<string [ ]> argsbuffer, out string error )
        {
            DynamicParameters pms = new DynamicParameters();
            error = "";
            try
            {
                /*
                 * PARSE THE ARGSBUFFER CONTENT AND CREATE ONE OR MORE DAPPER PARAMETERS
                 input order in argsbuffer should be  :
                @name
                Argument
                Type
                Direction
                !!!! Size
                 */
                int argcount = 0;
                for ( var i = 0 ; i < argsbuffer . Count ; i++ )
                {
                    string[] args = new string[6];
                    args = PadArgsArray ( args );
                    args = argsbuffer [ i ];
                    int[] argindx = new int[5];
                    // set  all to zero to initialise flags
                    for ( int z = 0 ; z < 5 ; z++ )
                    {
                        if ( args [ z ] != "" )
                            argindx [ z ] = 1;
                        else
                            argindx [ z ] = 0;
                    }
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
                                , direction: GetDirection ( args [ 3 ] ) );
                        continue;
                    }
                    // Got (2 = 10100) arg name + arg type + type
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 0 && argindx [ 2 ] == 1 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add ( name: args [ 0 ]
                        , dbType: GetArgType ( args [ 4 ] )
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
                                , direction: ParameterDirection . Input );
                        continue;
                    }
                    // Got (3 = 11100) arg name
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 1 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , value: args [ 1 ]
                            , dbType: GetArgType ( args [ 2 ] ) );
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
        public static string [ ] PadArgsArray ( string [ ] content )
        {
            // Pad any null entries   to ""
            string [ ] tmp = new string [ 6 ];
            for ( int x = 0 ; x < 6 ; x++ )
            {
                if ( content [ x ] != null )
                    tmp [ x ] = content [ x ];
                else
                    tmp [ x ] = "";
            }
            return tmp;
        }

        public static void PrintDebugArgs ( string [ ] args )
        {   // debug only
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

        static public int GetArgSize ( string args )
        {
            char ch;
            bool isnumber = true;
            string digitsonly="";
            for ( int x = 0 ; x < args . Length ; x++ )
            {
                ch = args [ x ];
                if ( Char . IsDigit ( ch ) == false )
                {
                    if ( ch != '(' && ch != ')' )
                    {
                        isnumber = false;
                        return 0;
                    }
                    else digitsonly += ch;
                }
            }
            return ( Convert . ToInt32 ( digitsonly ) );
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
        static public bool IsNumericString ( string args, ref int number )
        {
            int size = 0;
            number = -1;
            if ( args != "" )
            {
                int count=0;
                for ( int x = 0 ; x < args . Length ; x++ )
                {
                    count++;
                    char ch = args[x];
                    if ( Char . IsDigit ( ch ) == false )
                        break;
                }
                if ( count < args . Length )
                    return false;
                else
                    number = Convert . ToInt32 ( args );

                return true;
            }
            return false;
        }
        static public ParameterDirection GetDirection ( string args )
        {
            if ( args == "" || args . Contains ( "IN" ) )
                return ParameterDirection . Input;
            else if ( args . Contains ( "OUT" ) )
                return ParameterDirection . Output;

            return ParameterDirection . Input;
        }
        static public DynamicParameters ParseSqlArgs ( DynamicParameters parameters, string [ ] args )
        {
            bool error = false;
            // OLD METHOD - no longer used ????
            // WORKING CORRECTLY 6/11/2022 ?
            DynamicParameters pms = new DynamicParameters ( );
            if ( args != null && args . Length > 0 && args [ 0 ] != "-" )
            {
                // pms . AddDynamicParams ( args );
                int counter = 1;
                for ( int x = 0 ; x < args . Length ; x += 4 )
                {
                    if ( args [ x ] == "" )
                        break;

                    string name = "";
                    string type = "";
                    string size = "";
                    string returntype = "";
                    string valid = "0123456789";
                    string arg = "";
                    int index = 0;
                    if ( args [ 4 ] == "INPUT" )
                    {
                        if ( args [ 2 ] . ToUpper ( ) == "STRING" )
                        {
                            // WORKING 20/11/2022 !!
                            pms . Add ( $"{args [ 1 ]}", args [ 0 ],
                             dbType: DbType . String,
                            size: Convert . ToInt32 ( args [ 0 ] . Length ),
                            direction: ParameterDirection . Input );
                        }
                        else if ( args [ 1 ] == "INT" )
                        {
                            pms . Add ( $"{args [ 1 ]}", args [ 0 ],
                             dbType: DbType . Int32,
                            size: Convert . ToInt32 ( args [ 0 ] . Length ),
                            direction: ParameterDirection . Input );
                        }
                    }
                    else
                    {
                        // Output args
                        if ( args [ 1 ] == "STRING" )
                        {
                            pms . Add ( $"{args [ 1 ]}", args [ 0 ],
                             dbType: DbType . String,
                            size: Convert . ToInt32 ( args [ 0 ] . Length ),
                            direction: ParameterDirection . Output );
                        }
                        else if ( args [ 1 ] == "INT" )
                        {
                            pms . Add ( $"{args [ 1 ]}", args [ 0 ],
                             dbType: DbType . Int32,
                            size: Convert . ToInt32 ( args [ 0 ] . Length ),
                            direction: ParameterDirection . Output );
                        }
                    }
                    if ( error == true )
                        return parameters = null;
                }
                counter++;
            }
            return pms;
        }
        private void ShowError ( string optype, string err )
        {
            //if ( err != "" )
            //    MessageBox . Show ( $"Error encountered .....error message was \n{err . ToUpper ( )}\n\nPerhaps the method that you selected as shown below :-\n" +
            //        $"[{optype . ToUpper ( )}]\n was not the correct processing method type for this Stored.Procedure.\n\n" +
            //        $"The help window just opened shows you the parameter types required by this S.P?" , "SQL Error" );
            //else
            //    MessageBox . Show ( $"No Error was encountered,  but the request did NOT return any type of value...\n\n" +
            //        $"Perhaps the processing method that you selected as shown below :-\n[{optype . ToUpper ( )}]\n" +
            //        $"The help window just opened shows you the parameter types required by this S.P?" , "SQL Error" );

        }
        private string ReturnProcedureHeader ( string commandline, string Arguments )
        {
            //*********************************//
            // only called  by Resultsviewer
            //*********************************//
            //sphandling. Parameterstop . Text = GetSpHeaderBlock ( Arguments , spviewer );
            //if ( sphandling. Parameterstop . Text == "" )
            //    return "";
            ////DetailInfo . Visibility = Visibility . Visible;
            //sphandling. ExecResult . Text = $"Stored Procedure {commandline . ToUpper ( )} Header Details :-\n\n{sphandling. SPArguments . Text}";
            return "Done";
        }
        static public string [ ] CleanArgumentblanks ( string [ ] argset )
        {
            string [ ] data
        ;
            int rowcount = 0;
            for ( int x = 0 ; x < argset . Length ; x++ )
            {
                if ( argset [ x ] != "" )
                    rowcount++;
            }
            data = new string [ rowcount ];
            int indx = 0;
            for ( int x = 0 ; x < argset . Length ; x++ )
            {
                if ( argset [ x ] != "" )
                {
                    data [ indx ] = argset [ x ];
                    indx++;
                }
            }
            return data;
        }
        static public bool CheckForArgType ( string type )
        {
            if ( type == "" )
                return false;
            if ( type == "STR"
           || type == "INT"
           || type == "FLOAT"
           || type == "VARCHAR"
           || type == "VARBIN"
           || type == "TEXT"
           || type == "BIT"
           || type == "BOOL"
           || type == "SMALLINT"
           || type == "BIGINT"
           || type == "DOUBLE"
           || type == "DEC"
           || type == "CURR"
           || type == "DATETIME"
           || type == "DATE"
           || type == "TIMESTAMP"
           || type . Contains ( "TIME" ) )
                return true;
            else
                return false;
        }
        public int CheckForParameterArgCount ( string [ ] args, ref string Err )
        {
            int count = 0;
            for ( int x = 0 ; x < args . Length ; x++ )
            {
                if ( args [ 1 ] != "" )
                {
                    if ( args [ 1 ] == "STRING" || args [ 1 ] == "INT" )
                    {
                        args [ 1 ] = "";
                        Err = $"ONE or more PARAMETERS EXPECTED by this procedure were left as 'THE PROMPT TEXT' rather than containing a VALID ARGUMENT";
                        continue;
                    }
                    count++;
                }
            }
            return count;

        }
        public void PrintSPArgs ( List<string [ ]> args )
        {
            foreach ( string [ ] item in args )
            {
                int y = 0;
                for ( int x = 0 ; x < item . Length ; x++ )
                {
                    Debug . WriteLine ( $"({y++})  [{item [ x ]}]" );
                }
                y++;
            }
        }
        public string ValidateSizeParam ( string sizearg )
        {
            if ( sizearg . Substring ( 0, 1 ) == "(" )
            {
                // parse away any leading parenthesis
                string tmp = Utils2 . SpanTrim ( sizearg, 1, sizearg . Length - 1 ) . ToString ( );
                sizearg = tmp;
            }
            // parse away any trailing parenthesis
            if ( sizearg . Contains ( ')' ) )
                sizearg = sizearg . Substring ( 0, sizearg . Length - 1 ) . Trim ( );
            return sizearg;
        }

    }
}
