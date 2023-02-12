using System;
using System . Collections . Generic;
using System . Data;
using System . Diagnostics;
using System . IO;
using System . Text . RegularExpressions;
using System . Windows;
using System . Windows . Documents;
using System . Windows . Media;

using Dapper;

using Microsoft . Data . SqlClient;

using Wpfmain;

namespace SprocsProcessing
{
	public class SProcsSupport
	{
		public string SrchTerm { get; set; }  = "ARG";
		public string infotext = "";
		public static bool USERRTBOX = true;
		
		
		static public DynamicParameters ParseSqlArgs ( DynamicParameters parameters , string [ ] args , ref bool hasretval )
		{
			if ( args == null || args . Length == 0 )
				return new DynamicParameters ( );
			string [ ] argtype = new string [ args . Length ];
			// store orginal args so we can reporcess them during debugging !
			string [ ] OriginalArgs = new string [ args . Length ];
			OriginalArgs = args;
			args = OriginalArgs;
			// WORKING  8/11/2022 ?
			if ( args != null && args . Length > 0 && args [ 0 ] != "-" )
			{
				try
				{
					bool UseCalc = true;
					int outindex = 0;
					for ( int x = 0 ; x < args . Length ; x++ )
					{
						string type = null;
						string [ ] argparts = args [ x ] . Split ( ' ' );
						if ( argparts . Length == 1 )
						{
							type = "STRING";
							argtype [ outindex++ ] = $"{argparts [ 0 ]},{type}";
						}
						else
						{
							if ( argparts . Length == 2 )
							{
								type = "STRING";
								argtype [ outindex++ ] = $"{argparts [ 0 ]},{type}";
							}
							else if ( argparts . Length == 3 )
							{
								if ( UseCalc )
								{
									args [ x ] = argparts [ 0 ];
									if ( argparts [ 1 ] . Trim ( ) . ToUpper ( ) == "STRING" )
										type = "STRING";
									else if ( argparts [ 1 ] . Trim ( ) . ToUpper ( ) == "INT" )
										type = "INT";
									else if ( argparts [ 1 ] . Trim ( ) . ToUpper ( ) == "DOUBLE" )
										type = "DOUBLE";
									else if ( argparts [ 1 ] . Trim ( ) . ToUpper ( ) == "FLOAT" )
										type = "FLOAT";
									else if ( argparts [ 1 ] . Trim ( ) . ToUpper ( ) == "DECIMAL" )
										type = "DECIMAL";
									else if ( argparts [ 1 ] . Trim ( ) . ToUpper ( ) == "STRING[]" )
										type = "STRING[]";
									else
										type = "STRING";
									if ( argparts [ 2 ] . Trim ( ) . ToUpper ( ) == "OUTPUT" )
										argtype [ outindex++ ] = $"{argparts [ 0 ]},{type},OUTPUT";
								}
								//else
								//{
								//    type = argparts [ 2 ] . Trim ( ) . ToUpper ( ) . Trim ( );
								//    argtype [ outindex++ ] = $"{argparts [ 0 ]},{type},OUTPUT";
								//}
							}
						}
					}
				}
				catch ( Exception ex ) { Debug . WriteLine ( $"SQL argument parse error [ {ex . Message}, {ex . Data}" ); }
				for ( int x = 0 ; x < args . Length ; x++ )
				{
					// breakout on first unused array element
					if ( args [ x ] == "" )
						break;
					if ( argtype [ x ] . ToUpper ( ) . Contains ( "OUTPUT" ) )
					{
						// OUTPUT ARGUMENTS
						string param = argtype [ x ];
						string [ ] argparts = param . Split ( ',' );
						if ( argparts . Length == 1 || argparts [ 1 ] == "STRING" )
						{
							string [ ] splitter = args [ x ] . Split ( " " );
							// create an OUTPUT Argument STRING=""
							parameters . Add ( $"{splitter [ 0 ]}" , "" ,
											   DbType . String ,
											   ParameterDirection . Output ,
											   splitter [ 0 ] . Length );
						}
						else if ( argparts [ 1 ] == "INT" )
						{
							// create an OUTPUT Argument INT=0
							string [ ] splitter = args [ x ] . Split ( " " );
							parameters . Add ( $"{splitter [ 0 ]}" , 0 ,
											   DbType . Int32 ,
											   ParameterDirection . Output );
						}
						else if ( argparts [ 1 ] == "DOUBLE" )
						{
							// create an OUTPUT Argument INT=0
							string [ ] splitter = args [ x ] . Split ( " " );
							parameters . Add ( $"{splitter [ 0 ]}" , 0.0 ,
											   DbType . Double ,
											   ParameterDirection . Output );
						}
						else if ( argparts [ 1 ] == "FLOAT" || argparts [ 1 ] == "DECIMAL" )
						{
							// create an OUTPUT Argument INT=0
							string [ ] splitter = args [ x ] . Split ( " " );
							parameters . Add ( $"{splitter [ 0 ]}" , 0.0 ,
											   DbType . Currency ,
											   ParameterDirection . Output );
						}
						else if ( argparts [ 1 ] == "STRING[]" )
						{
							// create an OUTPUT Argument INT=0
							string [ ] splitter = args [ x ] . Split ( " " );
							parameters . Add ( $"{splitter [ 0 ]}" , null ,
											   DbType . Object ,
											   ParameterDirection . Output );
						}
						// Reset arg name to single item
						args [ x ] = argparts [ 0 ] . Trim ( );
					}
					else
					{
						// INPUT ARGUMENTS
						string param = argtype [ x ];
						string [ ] argparts = param . Split ( ',' );
						if ( argparts . Length == 1 )
						{
							// create an INPUT Argument
							parameters . Add ( $"Arg{x + 1}" , args [ x ] ,
						   DbType . String ,
						   ParameterDirection . Input ,
						   args [ x ] . Length );
						}
						else if ( argparts . Length == 2 )
						{
							if ( argparts [ 1 ] == "STRING" )
							{
								// create an INPUT Argument
								parameters . Add ( $"Arg{x + 1}" , args [ x ] ,
							   DbType . String ,
							   ParameterDirection . Input ,
							   args [ x ] . Length );
							}
							else if ( argparts [ 1 ] == "INT" )
							{
								// create an INPUT Argument
								parameters . Add ( $"Arg{x + 1}" , args [ x ] ,
						   DbType . Int32 ,
						   ParameterDirection . Input );
							}
							else if ( argparts [ 1 ] == "DOUBLE" )
							{
								// create an INPUT Argument
								parameters . Add ( $"Arg{x + 1}" , args [ x ] ,
						   DbType . Double ,
						   ParameterDirection . Input );
							}
							else if ( argparts [ 1 ] == "FLOAT" )
							{
								// create an INPUT Argument
								parameters . Add ( $"Arg{x + 1}" , args [ x ] ,
						   DbType . Currency ,
						   ParameterDirection . Input );
							}
							else if ( argparts [ 1 ] == "DECIMAL" )
							{
								// create an INPUT Argument
								parameters . Add ( $"Arg{x + 1}" , args [ x ] ,
						   DbType . Currency ,
						   ParameterDirection . Input );
							}
							else if ( argparts [ 1 ] == "STRING[]" )
							{
								// create an INPUT Argument
								parameters . Add ( $"Arg{x + 1}" , args [ x ] ,
						   DbType . Object ,
						   ParameterDirection . Input );
							}
						}
						args [ x ] = argparts [ 0 ] . Trim ( );
					}
				}
			}
			return parameters;
		}

		static public string CheckTypesRequired ( bool KeepTypes , string input )
		{
			if ( KeepTypes == false
				&& ( input . ToUpper ( ) . Contains ( "VARCHAR" ) == true
				|| input . ToUpper ( ) . Contains ( "BIT" ) == true
				|| input . ToUpper ( ) . Contains ( "SYSNAME" ) == true
				|| input . ToUpper ( ) . Contains ( "(MAX)" ) == true ) )
				input = "";
			else if ( KeepTypes == true
				&& ( input . ToUpper ( ) . Contains ( "VARCHAR" ) == true
				|| input . ToUpper ( ) . Contains ( "BIT" ) == true
				|| input . ToUpper ( ) . Contains ( "SYSNAME" ) == true
				|| input . ToUpper ( ) . Contains ( "(MAX)" ) == true ) )
				input = GetReplacementTypestring ( input );
			else
			{
			}
			return input;
		}
		static public string GetReplacementTypestring ( string input )
		{
			string output = "";
			if ( input . ToUpper ( ) . Contains ( "VARCHAR" ) )
				output = " string ";
			else if ( input . ToUpper ( ) . Contains ( "BIT" ) )
				output = " Bit ";
			else if ( input . ToUpper ( ) . Contains ( "SYSNAME" ) )
				output = " string ";
			else if ( input . ToUpper ( ) . Contains ( "(MAX)" ) )
				output = " string ";
			else if ( input . ToUpper ( ) . Contains ( "INT" ) )
				output = " int ";
			else if ( input . ToUpper ( ) . Contains ( "DOUBLE" ) )
				output = " double ";
			else if ( input . ToUpper ( ) . Contains ( "FLOAT" ) )
				output = " float ";
			return output;
		}
		static public string CleanEntry ( bool KeepTypes , string input )
		{
			string output = "";
			string tmpbuff = "";
			string [ ] split;
			if ( input . Contains ( ',' ) )
			{
				split = input . Split ( ',' );
				if ( split [ 0 ] . Length > split [ 1 ] . Length )
					tmpbuff = split [ 0 ];
				else
					tmpbuff = split [ 1 ];
				input = tmpbuff;
			}
			if ( input . Contains ( ']' ) )
			{
				split = input . Split ( ']' );
				if ( split [ 0 ] . Length > split [ 1 ] . Length )
					tmpbuff = split [ 0 ];
				else
					tmpbuff = split [ 1 ];
				input = tmpbuff;
			}
			output = input . Trim ( );
			return output;
		}
		/// <summary>
		/// Routine that checks for varchar or nvarchar or max in SP arguments
		/// and replaces them with the more legal 'string' nomenclature for the arguments string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		static public string CheckForVarchar ( bool KeepTypes , string input )
		{
			string output = "";
			string [ ] parts = input . Split ( ' ' );
			string buff = "";
			string stringbuff = "";
			string outputbuff = "";
			// see if there is a default value in this string
			string defvalue = CheckForDefaultValue ( input );

			if ( input . ToUpper ( ) . Contains ( "OUTPUT" ) )
			{
				int offset = input . IndexOf ( "OUTPUT" );
				input = input . Substring ( 0 , offset ) . Trim ( );
				outputbuff = " Output";
			}
			if ( parts . Length == 1 )
			{
				if ( input . ToUpper ( ) . Contains ( "VARCHAR" )
				|| input . ToUpper ( ) . Contains ( "SYSNAME" )
				|| input . ToUpper ( ) . Contains ( "(MAX)" ) )
				{
					//Its a string type
					if ( KeepTypes )
						stringbuff = "[string]";

					//                   if ( KeepTypes == true )
					buff += $"{defvalue}";
					output += $"{buff}{stringbuff}";
				}
				else
					return $"{parts [ 0 ]}";
			}
			else
			{
				string outbuff = "";
				for ( int x = 0 ; x < parts . Length ; x++ )
				{
					if ( parts [ x ] . Contains ( '@' ) )
					{
						buff = parts [ 0 ];
						buff = CheckForCommas ( parts [ 0 ] );
						continue;
					}
					input = parts [ x ];
					if ( input . ToUpper ( ) . Contains ( "VARCHAR" )
						 || input . ToUpper ( ) . Contains ( "SYSNAME" )
						 || input . ToUpper ( ) . Contains ( "(MAX)" ) )
					{
						//Its a string type
						if ( KeepTypes )
							stringbuff = " [string]";

						if ( KeepTypes == true )    // see if we have a default value
							buff += $"{defvalue}";
						output += $"{buff}{stringbuff}";
					}
					//}
				}
			}
			return $"{output}";
		}

		static public string CheckForCommas ( string input )
		{
			string output = input;
			if ( input . Contains ( ',' ) )
			{
				string [ ] tmp = input . Split ( ',' );
				if ( tmp [ 0 ] . Length > tmp [ 1 ] . Length )
					output = tmp [ 0 ];
				else
					output = tmp [ 1 ];
			}
			return output;
		}
		static public string CheckForDefaultValue ( string input )
		{
			string output = "";
			if ( input . Contains ( '=' ) )
			{
				string [ ] parts = input . Split ( '=' );
				if ( parts . Length > 1 )
					output = $" = {parts [ 1 ]}";
				if ( output . Contains ( "OUTPUT" ) )
				{
					string [ ] tmp = output . Split ( "OUTPUT" );
					if ( tmp [ 1 ] == "OUTPUT" )
						output = tmp [ 0 ];
					else
						output = tmp [ 1 ];
				}
			}
			return output;
		}
		static public string CheckForComment ( string input )
		{
			string output = input;
			if ( input . Contains ( "--" ) )
			{
				string [ ] tmp = input . Split ( "--" );
				output = tmp [ 0 ] . Length > tmp [ 1 ] . Length ? tmp [ 0 ] : tmp [ 1 ];
			}
			return output;
		}

		public static string CreateSProcArgsList ( string Arguments , string procname , out bool success )
		{
			// save orignal string for testing use only
			string original = Arguments;
			Arguments = original;
			// get stripped down header block
			string arguments = Arguments;
			string sizeprompt = "";
			string Output = "";
			// massage input buffer
			string tmpbuff = "";
			string temp = Arguments;
			temp = temp . Trim ( ) . TrimStart ( );
			Arguments = temp . ToUpper ( );

			success = false;
			tmpbuff = Arguments . ToUpper ( );
			if ( tmpbuff . Length == 0 )
				return "";
			temp = tmpbuff . ToUpper ( );
			int stringlen = 0;
			if ( temp . Contains ( "IT CONTAINS INVALID SYNTAX" ) )
			{
				Output = "WARNING - Header/Script appears to be invalid or corrupted...";
				return Output;
			}
			try
			{
				bool invalid = false;
				// First get any  items length argument
				string [ ] items = temp . Split ( ')' );
				if ( items . Length > 0 )
				{
					if ( items . Length == 1 )
					{
						if ( items [ 0 ] . Contains ( "CREATE PROCEDURE" ) == false )
						{
							Output = "WARNING - Header block appears to be invalid or corrupted...";
							invalid = true;
						}
					}
					if ( invalid == false )
					{
						if ( items . Length > 1 )
						{
							bool found = true;
							// Check for a valid size clause
							if ( items [ 1 ] . Contains ( ')' ) )
							{
								string test = items [ 1 ] . Substring ( 0 , items [ 1 ] . IndexOf ( ')' ) );
								if ( test == "MAX" )
								{
									sizeprompt = "(MAX argument size)";
									temp = items [ 0 ];
								}
								else
								{
									string valid = "0123456789";

									// check for double/float size
									if ( test != "" && test . Contains ( ',' ) == true )
									{
										// got ( xx.yy ) value
										sizeprompt = $"Double or Float value : {test}";
									}
									else
									{
										//it seems it may be an int type value
										// See if it contains all digits
										for ( int y = 0 ; y < test . Length ; y++ )
										{
											char validchar = test [ y ];
											if ( valid . Contains ( validchar ) == false )
											{
												found = false;
												break;
											}
										}
										if ( found == true )
											stringlen = Convert . ToInt32 ( test );
										else
											sizeprompt = "(Undefined argument size)";
									}
								}
							}
						}
						// We now have a full header block, so parse the strings
						string [ ] headerbuff = temp . Split ( '\n' );


						// default to being in INPUT mode
						bool outflag = false;
						Output = "[** Target **] ";


						// loop thru each the parameters
						for ( int x = 1 ; x < headerbuff . Length ; x++ )
						{
							if ( x > 1 )
								Output += " : ";

							string [ ] parts = headerbuff [ x ] . ToUpper ( ) . Split ( ' ' );
							for ( int y = 0 ; y < parts . Length ; y++ )
							{
								if ( parts [ y ] == ":" || parts [ y ] == "" )
									continue;
								if ( parts [ y ] . Contains ( "OUTPUT" ) )
								{
									outflag = true;
								}
								if ( !outflag && parts [ y ] . Contains ( "INPUT" ) )
									continue;
								else if ( !outflag && parts [ y ] . Contains ( "@" ) )
								{
									if ( Output != "" )
									{
										if ( parts [ y ] [ 0 ] == ',' )
											Output += $"{parts [ y ] . Substring ( 1 , parts [ y ] . Length - 1 )} ";
										else
											Output += $"{parts [ y ]} ";
									}
									else
										Output += $"{parts [ y ]}";
									continue;
								}
								else if ( !outflag )
								{
									if ( parts [ y ] . Contains ( "VARCHAR" ) )
									{
										string [ ] elements = parts [ y ] . Split ( '(' );
										if ( elements [ 1 ] . Contains ( ')' ) )
										{
											string [ ] splitter = elements [ 1 ] . Split ( ')' );
											if ( splitter . Length > 0 )
												Output += $"STRING ({splitter [ 0 ]})  INPUT";
											else
												Output += $"STRING ({splitter [ 0 ]})  INPUT";
										}
										else
											Output += $" String ({elements [ 1 ]}) INPUT";
									}
									else
									{
										if ( parts [ y ] . Contains ( "SYSNAME" ) || parts [ y ] . Contains ( "MAX" ) )
										{
											Output += $" String MAX INPUT ";
											continue;
										}
										if ( parts [ y ] == "AS" )
											continue;
									}
									continue;
								}
								if ( outflag && parts [ y ] . Contains ( "@" ) )
								{
									if ( parts [ y ] . StartsWith ( "," ) || parts [ y ] . Length < 2 )
									{
										Output += $"  :  {parts [ y ] . Substring ( 1 )} ";
										continue;
									}
									if ( Output . Length > 0 )
										Output += $", {parts [ y ]}";
									else
										Output += $"{parts [ y ]}";
								}
								else if ( outflag )
								{
									if ( parts [ y ] . Contains ( "VARCHAR" ) )
									{
										string [ ] elements = parts [ y ] . Split ( '(' );
										Output += $"String ";
										// Add in the size !!
										if ( elements . Length >= 2 && elements [ 1 ] != "" )
											Output += $"({elements [ 1 ] . Substring ( 0 , elements . Length )}) OUTPUT";
										else
											Output += $")";
										break;
									}
									else if ( parts [ y ] . Contains ( "INT" ) )
									{
										string [ ] elements = parts [ y ] . Split ( '(' );
										Output += $" (Integer OUTPUT";
										if ( elements . Length >= 2 && elements [ 1 ] != "" )
											Output += $"( {elements [ 1 ]} )";
										else
											Output += $")";
										break;
									}
									else if ( parts [ y ] . Contains ( "DATE" ) )
									{
										string [ ] elements = parts [ y ] . Split ( '(' );
										Output += $" (Date OUTPUT)";
										if ( elements . Length >= 2 && elements [ 1 ] != "" )
											Output += $"( {elements [ 1 ]} )";
										else
											Output += $")";
										break;
									}
								}
							}
						}
					}
					else if ( Output == "" )
						Output = "No arguments required.....";
				}
				else
				{
					Output = "WARNING - Header/Script appears to be invalid or corrupted...";
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"{ex . Message}" );
			}
			return Output;
		}
		public static string [ ] ReplaceNullsWithBlankString ( string [ ] cleantest )
		{
			for ( int x = 0 ; x < cleantest . Length ; x++ )
			{
				if ( cleantest [ x ] == null )
					cleantest [ x ] = "";
			}
			return cleantest;
		}

		public static string GetSpHeaderBlock ( string arguments , SProcsHandling spviewer )
		{
			//Save buffer  for reuse if debugging
			string Arguments = arguments;
			string temp = Arguments;
			temp = temp . Trim ( ) . TrimStart ( );
			Arguments = temp . ToUpper ( );
			int CreatePosition = 0;
			string output = "";

			// retrieve header block
			if ( Arguments . Contains ( "CREATE PROCEDURE" ) || Arguments . Contains ( "CREATE  PROCEDURE" ) == true )
			{
				try
				{
					CreatePosition = Arguments . IndexOf ( "CREATE PROCEDURE" );
					if(CreatePosition == -1)
                        CreatePosition = Arguments . IndexOf ( "CREATE  PROCEDURE" );
                    if ( CreatePosition > 0 )
					{
						// Strip out any preamble before the Create Proc line
						Arguments = Arguments . Substring ( CreatePosition );
					}
					// remove Create Proc line
					int offset3 = Arguments . IndexOf ( "\r\n" );

					// strip off the CREATE block to potenntial start of args
					Arguments = Arguments . Substring ( offset3 );

					// split entire data area by \r\n
					string [ ] test = Arguments . Split ( "\r\n" );
					test = Arguments . Split ( "\r\n" );

					// now get the cleaned up header block alone
					test = ExtractSpHeaderBlock ( test );
					if ( test [ 0 ] . StartsWith ( "ERROR -" ) )
						return test [ 0 ];

					string currentrow = "";
					// Sanity check , only CREATE line in header with no args
					for ( int rows = 0 ; rows < test . Length ; rows++ )
					{
						if ( test [ rows ] . Length <= 1 )
							continue;
						// Bypass create line
						if ( test [ rows ] == "" )
						{
							continue;
						}
						currentrow = test [ rows ];

						// chack for commented lines
						if ( currentrow . StartsWith ( "--" ) )
							continue;
						string testbuff = CheckAndRemoveBadCharacters ( currentrow ) . Trim ( );
						// split string into individual items so we can validate them
						if ( output . Length > 0 )
							output += " : ";
						output += testbuff;
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Parsing error {ex . Message}" );
					return "";
				}
			}

			if ( output == "" )
			{
				spviewer . Parameterstop . Text = $"[No Parameters required]";
				output = "No arguments are required, press 'Clear Prompt' button and then select Execute Option.";
			}
			else
			{
				int [ ] count = new int [ 3 ];
				string [ ] str = output . Split ( " " );
				foreach ( var item in str )
				{
					if ( item . Contains ( "@" ) )
						count [ 0 ]++;
					else if ( item . Contains ( "OUTPUT" ) || item . EndsWith ( "OUT" ) )
					{
						count [ 1 ]++;
						count [ 2 ] = count [ 1 ];
					}
				}

				if ( count [ 0 ] == 0 && count [ 1 ] == 1 )
					spviewer . Parameterstop . Text = $"[No Parameters but Single Output parameter]";
				else if ( count [ 0 ] == 1 && count [ 1 ] == 0 )
					spviewer . Parameterstop . Text = $"[Single Target or Input parameter only]";
				else if ( count [ 0 ] == 1 && count [ 1 ] == 1 )
				{
					if ( count [ 2 ] > 0 )
						spviewer . Parameterstop . Text = $"[Single Output parameter only]";
					else
						spviewer . Parameterstop . Text = $"[Single Target and/or Multiple Input parameters + Single Output parameter]";
				}
				else if ( count [ 0 ] > 1 && count [ 1 ] == 0 && str . Length == 1 )
					spviewer . Parameterstop . Text = @$"[Single Target plus one input or Multiple Inputs]";
				else if ( count [ 0 ] > 1 && count [ 1 ] == 0 && str . Length > 1 )
					spviewer . Parameterstop . Text = @$"[Single Target and/or Multiple Inputs]";
				else if ( count [ 0 ] == 1 && count [ 1 ] == 0 && str . Length > 1 )
					spviewer . Parameterstop . Text = @$"[Single Target or Input parameter]";
				else if ( count [ 0 ] == 0 && count [ 1 ] == 1 )
					spviewer . Parameterstop . Text = @$"[Single Output parameter only]";
				else if ( count [ 0 ] > 1 && count [ 1 ] == 1 )
				{
					if ( output . Contains ( ":" ) == false )
						spviewer . Parameterstop . Text = $"[Single Output parameter only]";
					else if ( count [ 0 ] - count [ 1 ] == 1 )
						spviewer . Parameterstop . Text = $"[Single Target or Single Input parameter + Single Output parameter]";
					else
						spviewer . Parameterstop . Text = $"[Single Target and/or Multiple Input parameters + Single Output parameter]";
				}
				else if ( count [ 0 ] > 1 && count [ 1 ] >= 1 )
				{
					if ( count [ 2 ] == count [ 0 ] - 1 )
						spviewer . Parameterstop . Text = @$"[Single Target or Input + Multiple Output parameters]";
					else
						spviewer . Parameterstop . Text = @$"[Single Target and/or Multiple Inputs + Multiple Output parameters]";
				}
				else if ( count [ 0 ] == 0 && count [ 1 ] == 0 )
				{
                    spviewer . SPArguments . Text = @$"No arguments are required, press 'Clear Prompt' button and then select Execute Option.";
                    spviewer . Parameterstop . Text = @$"[No parameters required (or allowed)]";
					output = "No parameters are required";
				}
				// Single input, single output
				else
					spviewer . Parameterstop . Text = $"[Input parameter(s) Only ]";
			}
			return output;
		}

		/******************************************************/
		// main check loop for parsing SProcs header block
		/******************************************************/
		static public string [ ] ExtractSpHeaderBlock ( string [ ] cleantest )
		{
			string Arguments = "";
			int [ ] aspos = new int [ 3 ];
			aspos [ 0 ] = aspos [ 1 ] = -1;
			// clean up any spurious leading characters
			for ( int z = 0 ; z < cleantest . Length ; z++ )
			{
				cleantest [ z ] = cleantest [ z ] . Trim ( );
				if ( cleantest [ z ] == null || cleantest [ z ] == "" )
					continue;
				if ( cleantest [ z ] == "AS" )
				{
					aspos [ 0 ] = z;
					continue;
				}
				if ( cleantest [ z ] == "BEGIN" )
				{
					aspos [ 1 ] = z;
					if ( aspos [ 1 ] > aspos [ 0 ] )
						break;
				}
			}
			if ( aspos [ 0 ] == -1 || aspos [ 1 ] == -1 )
			{
				string [ ] head = new string [ 1 ];
				head [ 0 ] = $"ERROR - Either the \"AS\" or \"BEGIN \" statements are missing";
				return head;
			}

			string [ ] header = new string [ aspos [ 0 ] ];
			if ( aspos [ 1 ] - aspos [ 0 ] >= 1 )
			{
				// got the end of the header block - strip it out into string[]
				for ( int x = 0 ; x < aspos [ 0 ] ; x++ )
				{
					if ( cleantest [ x ] != "" )
						header [ x ] = cleantest [ x ];
					else
						header [ x ] = "";
				}
			}

			int blanklines = 0;
			if ( header.Length > 0)
			{
				header = ReplaceNullsWithBlankString ( header );
				// got it - now cleanup the header block
				for ( int z = 0 ; z < header . Length ; z++ )
				{
					if ( header [ z ] == "" )
					{
						blanklines++;
						continue;
					}
					if ( header [ z ] != null && header [ z ] . StartsWith ( '\t' ) )
					{
						// check for multiple \t
						header [ z ] = header [ z ] . Substring ( 1 );

						while ( header [ z ] . Contains ( "\t" ) )
						{
							int offset2 = header [ z ] . IndexOf ( '\t' );
							header [ z ] = header [ z ] . Substring ( 0, offset2 );
						}
						if ( header [ z ] == "" )
							blanklines++;
					}
					// check for leading ,
					if ( header [ z ] != null && header [ z ] . StartsWith ( ',' ) )
					{
						header [ z ] = header [ z ] . Substring ( 1 );
						// check again for \t in case they were in revese order
						while ( header [ z ] . Contains ( "," ) )
							header [ z ] = header [ z ] . Substring ( 1 );
					}
					// check for leading --
					if ( header [ z ] != null && header [ z ] . StartsWith ( "-" ) )
					{
						header [ z ] = header [ z ] . Substring ( 1 );
						if ( header [ z ] . StartsWith ( "-" ) )
						{
							header [ z ] = "";
							blanklines++;
						}
					}

					//check for Trailing \t
					if ( header [ z ] . Contains ( "\t" ) )
					{
						while ( header [ z ] . Contains ( "\t" ) )
						{
							int offset2 = header [ z ] . IndexOf ( '\t' );
							header [ z ] = header [ z ] . Substring ( 0, offset2 );
						}
						if ( header [ z ] == "" )
							blanklines++;
					}
					// check for Trailing --
					if ( header [ z ] . Contains ( "--" ) )
					{
						while ( header [ z ] . Contains ( "--" ) )
						{
							int offset2 = header [ z ] . IndexOf ( '-' );
							header [ z ] = header [ z ] . Substring ( 0, offset2 );
						}
						if ( header [ z ] == "" )
							blanklines++;
					}
					if ( header [ z ] . StartsWith ( "/*" ) || header [ z ] . Contains ( "*/" ) )
					{
						header [ z ] = "";
						blanklines++;
					}
					header [ z ] = header [ z ] . Trim ( );
				}
				Arguments = "";
				int newcount = 0;
				if ( blanklines == header . Length )
				{
					string [ ] output = new string [ 1 ];
					output [ 0 ] = "ERROR - No valid Arguments were found in the current Stored P.rocedure......";
					return output;
				}
				else
				{
					for ( int x = 0 ; x < header . Length ; x++ )
					{
						if ( header [ x ] != "" )
						{
							Arguments += $"{header [ x ]}:";
							newcount++;
						}
					}
					Arguments = Arguments . Substring ( 0, Arguments . Length - 1 );
				}
			}

			string [ ] head2 = Arguments . Split ( ":" );
			// we should now have a totally clean string of the entire header block with lines seperated by :
			return head2;
		}


		public static string [ ] ClearStringArray ( string [ ] arry )
		{
			for ( int x = 0 ; x < arry . Length ; x++ )
			{
				arry [ x ] = "";
			}
			return arry;
		}
		public static string CheckAndRemoveBadCharacters ( string testbuff )
		{
			if ( testbuff == null )
				return "";
			// Test for : '\t'
			if ( testbuff . Contains ( "\r" ) )
			{
				string newbuff = "";
				string [ ] tmp = testbuff . Split ( '\r' );
				foreach ( var item in tmp )
				{
					newbuff += $"{item} ";
				}
				testbuff = newbuff;
			}
			// Test for : '\n'
			if ( testbuff . Contains ( "\n" ) )
			{
				string newbuff = "";
				string [ ] tmp = testbuff . Split ( '\n' );
				foreach ( var item in tmp )
				{
					newbuff += $"{item} ";
				}
				testbuff = newbuff;
			}
			// Test for : '\t'
			if ( testbuff . Contains ( "\t" ) )
			{
				string newbuff = "";
				string [ ] tmp = testbuff . Split ( '\t' );
				foreach ( var item in tmp )
				{
					newbuff += $"{item} ";
				}
				testbuff = newbuff;
			}
			// Test for :  ','
			if ( testbuff . Contains ( "," ) )
			{
				string newbuff = "";
				string [ ] tmp = testbuff . Split ( ',' );
				foreach ( var item in tmp )
				{
					newbuff += $"{item} ";
				}
				testbuff = newbuff;
			}
			// Test for : '-'
			if ( testbuff . Contains ( "-" ) )
			{
				string newbuff = "";
				string [ ] tmp = testbuff . Split ( "-" );
				newbuff = tmp [ 0 ];
				testbuff = newbuff;
			}
			if ( testbuff . Contains ( "'" ) )
			{
				string newbuff = "";
				string [ ] tmp = testbuff . Split ( "'" );
				for ( int x = 0 ; x < tmp . Length ; x++ )
				{
					if ( tmp [ x ] != "" )
						newbuff += tmp [ x ];
				}
				testbuff = newbuff;
			}
			// Test for : "(xxx)"
			if ( testbuff . Contains ( '(' ) && testbuff . Contains ( ')' ) )
			{
				string [ ] args = new string [ 5 ];
				string [ ] tmp2 = new string [ 5 ];
				testbuff = testbuff . Trim ( );
				tmp2 = testbuff . Split ( ' ' );
				if ( tmp2 [ 0 ] . Contains ( '@' ) )
					args [ 0 ] = tmp2 [ 0 ];
				if ( tmp2 . Length > 2 && tmp2 [ 2 ] . Contains ( "OUTPUT" ) )
					args [ 3 ] = tmp2 [ 2 ];
				ClearStringArray ( tmp2 );
				string output = "";
				string tempoutput = "";

				int offset = testbuff . IndexOf ( '(' );
				// strip out "(xxx)"
				string [ ] tmp = testbuff . Split ( "(" );
				tmp2 [ 0 ] = tmp [ 0 ];
				tempoutput = $"{tmp [ 0 ]} ";
				offset = tmp [ 1 ] . IndexOf ( ')' );
				tmp2 [ 1 ] = tmp [ 1 ] . Substring ( 0 , offset );
				// store size for access later
				args [ 2 ] = tmp2 [ 1 ];

				tempoutput += tmp2 [ 1 ] . Trim ( );
				tmp = tempoutput . Split ( ' ' );
				if ( tmp [ 1 ] . Contains ( "VARCHAR" ) )
				{
					args [ 1 ] = "STRING";
					tmp [ 1 ] = args [ 1 ];
				}
				// Test for : "MAX" for size
				if ( tmp [ 2 ] == "MAX" || tmp [ 1 ] == "SYSNAME" )
					args [ 2 ] = " STRING 32000";
				else
					args [ 2 ] = $"({args [ 2 ]})";

				if ( tmp2 [ 1 ] != "" )
					testbuff = $"{args [ 0 ]} {args [ 1 ]} {tmp2 [ 1 ]}";
				else
					testbuff = $"{args [ 0 ]} {args [ 1 ]}{args [ 2 ]} {args [ 3 ]}";
			}
			// Test for : Sysname as size
			if ( testbuff . Contains ( "SYSNAME" ) )
			{
				string [ ] tmp2 = new string [ 5 ];
				string [ ] args = new string [ 5 ];

				tmp2 = testbuff . Split ( ' ' );
				if ( tmp2 [ 1 ] . Contains ( "SYSNAME" ) )
					tmp2 [ 1 ] = " STRING 32000";
				testbuff = $"{tmp2 [ 0 ]}{tmp2 [ 1 ]}";
				if ( tmp2 . Length > 4 && args [ 4 ] != "" )
					testbuff += $"=\"{tmp2 [ 4 ] . ToLower ( )}\"";

			}
			return testbuff;
		}
		/// <summary>
		/// Method to strip out the header block of any SP and
		/// present it in a presentation format for help system etc
		/// </summary>
		/// <param name="Arguments"></param>

		public static string GetRegexForHeaderEnd ( string argument )
		{
			// use REGEX to find AS and BEGIN so we can get JUST the header block
			int asindex = 0;
			int beginindex = 0;
			string Output = "";
			string tmpbuff = "";
			bool found = false;
			int offset = 0;

			string buff = argument . ToUpper ( );

			try
			{
				int [ ] offsetAs = FindWithRegex ( buff , "AS" );
				// working Regex statement - "\sbegin\s" will match 'begin' starting and ending in space | Cr |Lf characters
				int [ ] offsetBegin = FindWithRegex ( buff , $@"\sBEGIN\s" );
				if ( offsetAs . Length > 0 && offsetBegin . Length > 0 )
					Output = FindHeaderBlock ( argument , offsetAs , offsetBegin );
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Regex failure : {ex . Message}" );
			}
			return Output;
		}
		public static int [ ] FindWithRegex ( string buff , string argument )
		{
			string tmpbuff = "";
			bool found = false;
			int offset = 0;
			argument = argument . ToUpper ( );
			buff = buff . ToUpper ( );
			// working Regex statement - "\sbegin\s" will match 'begin' starting and ending in space | Cr |Lf characters
			// or ".begin." will match 'begin' starting and ending in ANY character at  all
			MatchCollection mc = Regex . Matches ( buff , @$"\s{argument}\s" , RegexOptions . IgnoreCase );
			int [ ] offsets = new int [ mc . Count ];
			for ( int x = 0 ; x < mc . Count ; x++ )
			{
				offsets [ x ] = mc [ x ] . Index;
			}
			return offsets;
		}
		public static string FindHeaderBlock ( string buff , int [ ] item1 , int [ ] item2 )
		{
			int beginindex = 0;
			int asindex = 0;
			int diff = 0;
			string tmpbuff = "";
			bool found = false;
			// also works cos begin starts a line but has a space after it in the file
			if ( item1 . Length > 0 && item2 . Length > 0 )
			{
				// iterate thru collection of matches to AS till we find the one closest to our BEGIN statement
				for ( int x = 0 ; x < item1 . Length ; x++ )
				{
					for ( int y = 0 ; y < item2 . Length ; y++ )
					{
						asindex = item1 [ x ];
						beginindex = item2 [ y ];
						diff = beginindex - asindex;

						if ( diff <= 10 )
						{
							// GOT IT - AS is within 10 bytes of BEGIN, so we are almost certainly good to go 
							tmpbuff = buff . Substring ( 0 , asindex );
							found = true;
							break;
						}
						else if ( diff > 0 )
						{
							// begin is too far ahead, so get next As
							break;
						}
					}
					if ( found )
						return tmpbuff;
				}
			}
			return "";
		}
		private void LoadRTbox ( )
		{
			//FlowDocument myFlowDocument = new FlowDocument ( );
			infotext = File . ReadAllText ( @$"C:\users\ianch\documents\GenericGridInfo.Txt" );
//			myFlowDocument = LoadFlowDoc ( RTBox , Application . Current . FindResource ( "Black3" ) as SolidColorBrush , infotext );
		}
		public FlowDocument CreateFlowDocumentScroll ( string line1 , string clr1 = "" , string line2 = "" , string clr2 = "" , string line3 = "" , string clr3 = "" , string header = "" , string clr4 = "" ,
			int fontsize = 0 , string fground = "" , string bground = "" )
		{
			FlowDocument myFlowDocument = new FlowDocument ( );
			Paragraph para1 = new Paragraph ( );
			//NORMAL
			// This is  the only paragraph that uses the user defined Font Size....
			if ( fontsize == 0 )
				para1 . FontSize = 14;
			else
				para1 . FontSize = fontsize;
			para1 . FontFamily = new FontFamily ( "Arial" );
			if ( USERRTBOX )
				para1 . Foreground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
			else
				para1 . Foreground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
			// handle user defined optional parameters
			if ( fground != "" )
				para1 . Foreground = Application . Current . FindResource ( fground ) as SolidColorBrush;
			if ( bground != "" )
				para1 . Background = Application . Current . FindResource ( bground ) as SolidColorBrush;
			Thickness th = new Thickness ( );
			th . Top = 10;
			th . Left = 10;
			th . Right = 10;
			th . Bottom = 10;
			para1 . Padding = th;
			para1 . Inlines . Add ( new Run ( line1 ) );
			//Add paragraph to flowdocument
			myFlowDocument . Blocks . Clear ( );
			myFlowDocument . Blocks . Add ( para1 );
			return myFlowDocument;
		}
		public FlowDocument CreateBoldString ( FlowDocument myFlowDocument , string SpText , string SrchTerm )
		{
			string original = SpText;
			string originalSearchterm = "";
			original = Utils2 . CopyCollection ( SpText , original );
			string input = SpText . ToUpper ( );
			string [ ] NonSearchText;
			List<int> NonSearchTextlength = new List<int> ( );
			List<string> NonCapitlisedString = new List<string> ( );
			originalSearchterm = SrchTerm;
			int newpos = 0;
			if ( SrchTerm == null )
				SrchTerm = "";
			SrchTerm = SrchTerm . ToUpper ( );

			// split source down based on searchterm (using non capitalised version
			// // Only searchterm is capitalised !!!!))
			NonSearchText = input . Split ( $"{SrchTerm}" );
			foreach ( var item in NonSearchText )
			{
				NonSearchTextlength . Add ( item . Length );
			}
			for ( int x = 0 ; x < NonSearchTextlength . Count ; x++ )
			{
				string temp = original . Substring ( newpos , NonSearchTextlength [ x ] );
				NonCapitlisedString . Add ( temp );
				newpos += NonSearchTextlength [ x ] + SrchTerm . Length;
			}
			// Now create a (formatted) list of lines from all  paragraphs identified previously
			// using temp paragraph so I can access it from my public para variable
			Paragraph tmppara = new Paragraph ( );

			for ( int x = 0 ; x < NonCapitlisedString . Count ; x++ )
			{
				Run run1 = AddStdNewDocumentParagraph ( NonCapitlisedString [ x ] , SrchTerm );
				tmppara . Inlines . Add ( run1 );
				Run run2 = AddDecoratedNewDocumentParagraph ( NonCapitlisedString [ x ] , SrchTerm );

				if ( x < NonCapitlisedString . Count - 1 )
					tmppara . Inlines . Add ( run2 );
			}
			Paragraph para1 = tmppara;
			SProcsHandling sp = SProcsHandling.spviewer;
			para1 . Background = sp . ScrollViewerBground;
			//para1 . Foreground = sp . ScrollViewerFground;
			// build  document by adding all blocks to Document
			myFlowDocument . Blocks . Add ( para1 );
			return myFlowDocument;
		}

		public Run AddStdNewDocumentParagraph ( string textstring , string SearchText )
		{
			SProcsHandling sp = SProcsHandling.spviewer;
			// Main text
			Run run1 = new Run ( textstring );
			run1 . FontSize = sp.ScrollViewerFontSize;;
			run1 . FontFamily = new FontFamily ( "Arial" );
			run1 . FontWeight = FontWeights . Normal;
			run1 . Background = sp . ScrollViewerBground;
			run1 . Foreground = sp . ScrollViewerFground;

//run1 . Foreground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
			return run1;
		}
		public Run AddDecoratedNewDocumentParagraph ( string textstring , string SearchText )
		{
			SProcsHandling sp = SProcsHandling.spviewer;
			Run run2 = new Run ( SearchText );
			run2 . FontSize = 18;
			run2 . FontFamily = new FontFamily ( "Arial" );
			run2 . FontWeight = FontWeights . Bold;
			run2 . Background = sp . ScrollViewerBground;
//			run2 . Foreground = sp. ScrollViewerHiliteColor;
			SProcsHandling. comboclrs cc = new();
			cc = sp . GetCurrentSelectionItem ( );
            run2 . Foreground =  cc . Bground;
            // as SProcsHandling . comboclrs ;

            //            sp. Comboclrs  cc =(sp. Comboclrs)sp . ColorsCombo . SelectedItem;
            //) sp . ColorsCombo . SelectedItem;



            return run2;
		}
		public  static DataTable ProcessSqlCommand ( string SqlCommand )
		{
			SqlConnection con;
			DataTable dt = new DataTable ( );
			string ConString = ( string ) Wpfmain . Properties . Settings . Default [ "ConnectionString" ];
		con = new SqlConnection ( ConString );
			try
			{
				Debug . WriteLine ( $"Using new SQL connection in PROCESSSQLCOMMAND" );
				using ( con )
				{
					SqlCommand cmd = new SqlCommand ( SqlCommand , con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"ERROR in PROCESSSQLCOMMAND(): Failed to load Datatable :\n {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"ERROR in PROCESSSQLCOMMAND(): Failed to load datatable\n{ex . Message}" );
			}
			finally
			{
				Debug . WriteLine ( $" SQL data loaded from SQLCommand [{SqlCommand . ToUpper ( )}]" );
				con . Close ( );
			}
			return dt;
		}
		public static string GetSpHeaderTextOnly ( string spText )
		{
			// strip the header block only out of any Stored Procedure, (ending after the AS line)
			int counter = 0;
			int line1 = 0;
			int line2 = 0;
			string testline = "";
			string [ ] lines = spText . Split ( "\r\n" );

			foreach ( string item in lines )
			{
				testline = item . Trim ( ) . ToUpper ( );
				if ( testline == "" )
				{ counter++; continue; }
				if ( testline . Length >= 2
					&& ( testline . Substring ( 0 , 2 ) == "AS"
					|| testline . Length >= 4 && testline . Substring ( 0 , 4 ) == "--AS" ) )
				{
					line1 = counter;
					//continue;
				}
				else if ( testline . Length >= 5
					&& ( testline . Substring ( 0 , 5 ) == "BEGIN"
					|| testline . Length >= 7 && testline . Substring ( 0 , 7 ) == "--BEGIN" ) )
				{
					line2 = counter;
				}
				counter++;
				if ( line1 > 0 && line2 > 0 )
				{
					for ( int x = 0 ; x < counter - 1 ; x++ )
						testline += lines [ x ] + "\r\n";
					spText = testline;
					break;
				}
			}
			return spText;
		}
		static public DataTable ProcessSqlCommand ( string SqlCommand , string connstring , string [ ] args = null )
		{
			SqlConnection con;
			DataTable dt = new DataTable ( );
			string filterline = "";
			string ConString = connstring;
			if ( connstring == null  || connstring == "" )
				ConString = ( string ) Wpfmain . Properties . Settings . Default [ "ConnectionString" ];

			con = new SqlConnection ( ConString );
			try
			{
				using ( con )
				{
					if ( args != null )
						SqlCommand = SqlCommand + $" '{args [ 0 ]}'";
					SqlCommand cmd = new SqlCommand ( SqlCommand , con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"ERROR in PROCESSSQLCOMMAND(): Failed to load Datatable :\n {ex . Message}" );
				MessageBox . Show ( $"ERROR in PROCESSSQLCOMMAND(): Failed to load datatable\n{ex . Message}" );
			}
			finally
			{
				Debug . WriteLine ( $" SQL data loaded from SQLCommand [{SqlCommand . ToUpper ( )}]" );
				con . Close ( );
			}
			return dt;
		}


	}
}
