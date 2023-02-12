using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data;
using System . Diagnostics;

using Dapper;

using ViewModels;

using Wpfmain;
using Wpfmain . Dapper;

namespace SprocsProcessing
{
	// All Support methods for main class file GenDapperQueries.cs (3 partial files)
	public partial class GenDapperQueries
	{
		public static string CheckSetSqlDomain ( string domain )
		{
			string ConString = "";
			if ( domain == "" )
				ConString = MainWindow . SqlCurrentConstring;
			else
			{
				DapperSupport . CheckDbDomain ( domain );
				ConString = Flags . CurrentConnectionString;
				if ( ConString == "" )
				{
					// set to our local definition
					ConString = MainWindow . SqlCurrentConstring;
				}
				else
					MainWindow . SqlCurrentConstring = ConString;
			}
			string [ ] tmp = ConString . Split ( ';' );
			int offset = 0;
			foreach ( var item in tmp )
			{
				if ( item . Contains ( "Catalog" ) )
				{
					//                   Debug . WriteLine ( $"Sql Domain of {item} confirmed..." );
					break;
				}
				offset++;
			}
			return ConString;
		}
		static public DynamicParameters ParseSqlArgs ( DynamicParameters parameters , string [ ] args )
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
		public static GenericClass ParseDapperRow ( dynamic buff , Dictionary<string , object> dict , out int colcount , ref List<int> varcharlen , bool GetLength = false )
		{
			GenericClass GenRow = new GenericClass ( );
			int index = 0;
			colcount = 0;
			try
			{
				foreach ( var item in buff )
				{
					index += 1;
					if ( item . Key == "" || ( item . Value == null && item . Key . Contains ( "character_maximum_length" ) == false ) )
						break;
					if ( GetLength && item . Key . Contains ( "character_maximum_length" ) )
					{
						varcharlen . Add ( item . Value == null ? 0 : item . Value );
					}
					else
						dict . Add ( item . Key , item . Value );

					//var v = buff .ToString();
					//varcharlen.Add(item.Key, buff [ 2 ]);
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"ParseDapperRow failed [{ex . Message}]" );
				return null;
			}
			colcount = index;
			return GenRow;
		}
		public static void AddDictPairToGeneric ( GenericClass gc , KeyValuePair<string , object> dict , int dictcount )
		{
			switch ( dictcount )
			{
				case 1:
					gc . field1 = dict . Value . ToString ( );
					break;
				case 2:
					gc . field2 = dict . Value . ToString ( );
					break;
				case 3:
					gc . field3 = dict . Value . ToString ( );
					break;
				case 4:
					gc . field4 = dict . Value . ToString ( );
					break;
				case 5:
					gc . field5 = dict . Value . ToString ( );
					break;
				case 6:
					gc . field6 = dict . Value . ToString ( );
					break;
				case 7:
					gc . field7 = dict . Value . ToString ( );
					break;
				case 8:
					gc . field8 = dict . Value . ToString ( );
					break;
				case 9:
					gc . field9 = dict . Value . ToString ( );
					break;
				case 10:
					gc . field10 = dict . Value . ToString ( );
					break;
				case 11:
					gc . field10 = dict . Value . ToString ( );
					break;
				case 12:
					gc . field12 = dict . Value . ToString ( );
					break;
				case 13:
					gc . field13 = dict . Value . ToString ( );
					break;
				case 14:
					gc . field14 = dict . Value . ToString ( );
					break;
				case 15:
					gc . field15 = dict . Value . ToString ( );
					break;
				case 16:
					gc . field16 = dict . Value . ToString ( );
					break;
				case 17:
					gc . field17 = dict . Value . ToString ( );
					break;
				case 18:
					gc . field18 = dict . Value . ToString ( );
					break;
				case 19:
					gc . field19 = dict . Value . ToString ( );
					break;
				case 20:
					gc . field20 = dict . Value . ToString ( );
					break;
			}
		}
		public static string GetCheckCurrentConnectionString ( string CurrentTableDomain )
		{
			string Con = "";
			if ( DapperSupport . CheckResetDbConnection ( CurrentTableDomain , out string constring ) == false )
			{
				Debug . WriteLine ( $"Failed to set connection string for {CurrentTableDomain . ToUpper ( )} Db" );
				return null;
			}
			else
			{
				Con = CheckSetSqlDomain ( CurrentTableDomain );
				if ( Con == "" )
				{   // drop down to default of IAN1
					Con = MainWindow . SqlCurrentConstring;
				}
			}
			return Con;
		}


		/// <summary>
		/// Method to parse any Dynamic collection into a genericlass table
		/// </summary>
		/// <param name="data">Dynamic  data</param>
		/// <param name="errormsg">string</param>
		/// <param name="reccount">int</param>
		/// <param name="genericlist">List<string></string></param>
		/// <returns></returns>
		public static ObservableCollection<GenericClass> ParseDynamicToCollection (
			IEnumerable<dynamic> data ,
			out string errormsg ,
			out int reccount ,
			out List<string> genericlist )
		{
			List<int> varcharlen= new List<int>();
			Dictionary<string, object> dict = new Dictionary<string, object> ();
			bool boolval = false;
			int colcount = 0;
			errormsg = "";
			reccount = 0;
			ObservableCollection<GenericClass> gcc= new ( );

			genericlist = new List<string> ( );

			if ( data == null )
				return null;
			var genlist = data . AsList ( );
			Debug . WriteLine ( $"{genlist[0]}");
			foreach ( var item in genlist)
			{
				GenericClass gc = new GenericClass ( );

				Dictionary<string, object> dictout= new();
				dictout = DapperGeneric. ParseDapperRowGen ( genlist, dict, out reccount);

                DapperSupport . ParseDapperRow ( item, dictout, out colcount, ref varcharlen,  boolval);
				gc . field1 = genlist [ reccount ++].ToString();
				gcc . Add ( gc );
				reccount++;
				if ( reccount == genlist . Count )
					break;
			}
				//Debug . WriteLine ( $"{data.AsList()");
			//	Dictionary<string , object> dict = new Dictionary<string , object> ( );
			//ObservableCollection<GenericClass> collection = new ObservableCollection<GenericClass> ();
			////Although this is duplicated  with the one below we CANNOT make it a method()
			//errormsg = "";
			//int dictcount = 0;
			//string result = "";
			//int fldcount = 0;
			//int colcount = 0;
			//try
			//{
			//	foreach ( var item in data )
			//	{
			//		GenericClass gc = new GenericClass ( );
			//		gc.field1= item.ToString ( );
			//		//KeyValuePair< string, string>itemdata = KeyValuePair . Create ( item . Key.ToString(), item . Value.ToString() );
			//		genericlist . Add ( gc.field1 );
			//		//continue;
			//		try
			//		{
			//			// we need to create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
			//			string buffer = "";
			//			List<int> VarcharList = new List<int> ( );
			//			gc = ParseDapperRow ( item , dict , out colcount , ref VarcharList );
			//			dictcount = 1;
			//			fldcount = dict . Count;
			//			string tmp = "";
			//			foreach ( var pair in dict )
			//			{
			//				try
			//				{
			//					if ( pair . Key != null && pair . Value != null )
			//					{
			//						AddDictPairToGeneric ( gc , pair , dictcount++ );
			//						tmp = pair . Key . ToString ( ) + "=" + pair . Value . ToString ( );
			//						buffer += tmp + ",";
			//					}
			//				}
			//				catch ( Exception ex )
			//				{
			//					Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
			//					Utils . DoErrorBeep ( );
			//					result = ex . Message;
			//				}
			//			}
			//			//remove trailing comma
			//			if ( buffer . Length > 0 )
			//			{
			//				string s = buffer . Substring ( 0, buffer . Length - 1 );
			//				buffer = s;
			//				genericlist . Add ( buffer );
			//			}
			//			//else

			//		}
			//		catch ( Exception ex )
			//		{
			//			result = $"SQLERROR : {ex . Message}";
			//			Utils . DoErrorBeep ( );
			//			errormsg = result;
			//			Debug . WriteLine ( result );
			//		}
			//		collection . Add ( gc );
			//		dict . Clear ( );
			//		dictcount = 1;
			//	}
			//}
			//catch ( Exception ex )
			//{
			//	Debug . WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
			//	result = ex . Message;
			//	Utils . DoErrorBeep ( );
			//	errormsg = result;
			//}
			//reccount = collection . Count;
//			return collection;
			return gcc;
		}
	}
}
