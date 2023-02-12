#define USENEWARGS
//#undef USENEWARGS
using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Windows;
using System . Windows . Input;

using Dapper;

using Microsoft . Data . SqlClient;

using ViewModels;

using Wpfmain;



namespace SprocsProcessing

{
	// Dynamic QUERY methods for main class GenDapperQueries
	// most supporting methods can be found in GenDapperQuerySupport.cs
	public static partial class GenDapperQueries
	{
		//#####################################################################################//
		/// <summary>
		/// Generic Method to implement dapper Queries  of all types
		/// it implements various methods access via the 'method=x' argument
		/// which basically controls  the return type exepcted from each call received
		/// 
		/// method  0 :  returns an IEnumerable[dynamic] INT commandType  is StoredProcedure
		/// method  1 :  returns an IEnumerable[dynamic] STRING where commandType  is TEXT (no args array received)
		/// method  2 :  returns an IEnumerable[dynamic] STRING FROM AN OUTPUT VARIABLE commandType  is StoredProcedure
		/// method  3 :  currenty unused
		/// method  4 :  returns an IEnumerable[dynamic] OBSERVABLECOLLECTION[GENERICLASS] commandType  is StoredProcedure
		/// </summary>
		/// <param name="SqlCommand"></param>
		/// <param name="args"></param>
		/// <param name="resultstring"></param>
		/// <param name="obj"></param>
		/// <param name="Objtype"></param>
		/// <param name="count"></param>
		/// <param name="error"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		public static dynamic Get_DynamicValue_ViaDapper (
			string SqlCommand ,
			List<string [ ]> argsbuffer ,
			ref string resultstring ,
			ref object obj ,
			ref Type Objtype ,
			ref int count ,
			ref string error ,
			int method = 0 )
		{
			#region declarations
			string connectionString = MainWindow . SqlCurrentConstring;
			SqlConnection sqlCon = null;
			ObservableCollection<GenericClass> temp = new ObservableCollection<GenericClass> ( );
			List<Dictionary<string , string>> list = new List<Dictionary<string , string>> ( );

			// initialize ref variables
			Objtype = null;
			count = 0;

			List<string> stringlist = new List<string> ( );
			ObservableCollection<GenericClass> collection = new ObservableCollection<GenericClass> ( );

			#endregion declarations

			string Con = CheckSetSqlDomain ( MainWindow . CurrentSqlTableDomain );
			if ( Con == "" )
			{
				// set to our local definition
				Con = MainWindow . CurrentSqlTableDomain;
				MessageBox . Show ( $"It was not possible to Identify a valid Sql Connection string for \nthe Database [ {MainWindow . CurrentSqlTableDomain . ToUpper ( )} ]\n\n Please report  this error to DB Technical Support" , "Connection Error" );
				error = $"Could not get correct SQL initialization string for domain {MainWindow . CurrentSqlTableDomain}";
				return null;
			}
			try
			{
				using ( sqlCon = new SqlConnection ( Con ) )
				{
					DynamicParameters parameters = new DynamicParameters ( );
					if ( argsbuffer . Count > 0 )
					{
#if USENEWARGS
						parameters = SProcsHandling . ParseNewSqlArgs ( parameters , argsbuffer , out error );
						SpExecution spe= new SpExecution("DUMMY");
                        spe .PrintSPArgs ( argsbuffer );
#else
                        parameters = StoredprocsProcessing . ParseSqlArgs ( parameters , args );
#endif
                    }
					$"{SqlCommand}" . DapperTrace ( );

					//********************************************************************************************************//                    
					// COLLECTION - STOREDPROC  - use StoredProcedure version returning IEnumerable<dynamic>
					//********************************************************************************************************//                    
					if ( method == 0 )
					{
						//=================================================================================//
						var table = sqlCon . Query ( SqlCommand , parameters , commandType: CommandType . StoredProcedure );
						//=================================================================================//

						int intresult = 0;
						count = table . Count ( );

						// this gets total rows returned by query
	//					count = table . Count ( );
						Debug . WriteLine ( $"total rows returned from GENDAPPERQUERIES . GET_DYNAMICVALUEVIADAPPER() line 105 \nExecuting SP [ {SqlCommand} ]= {count}" );
						Objtype = typeof ( IEnumerable<dynamic> );
						obj = ( object ) table;
						if(count > 0)
							resultstring = "SUCCESS";
						else
                            resultstring = "FAILURE";
                        // return IEnumerable<dynamic>
                        return obj;

					}

					//**************************************************************************************************************************************************//
					// INT - TEXT - using Text command version returning IEnumerable<dynamic> (for int results)
					//**************************************************************************************************************************************************//
					else if ( method == 1 )
					{

						//=================================================================================//
						var IEintval = sqlCon . Query ( SqlCommand , null , commandType: CommandType . Text );
						//=================================================================================//

						// parse out the int value from the IEnumerable <dynamic> IEintval  variable returned to us

						Objtype = typeof ( int );
						obj = ( object ) IEintval;
						count = 1;
						resultstring = "SUCCESS";
						return obj;
					}

					//**************************************************************************************************************************************************//
					// STRING - STOREDPROC using SP to get a string from an output variable such as '@result' 
					//**************************************************************************************************************************************************//
					else if ( method == 2 )
					{
						Debug . WriteLine ( $"ProcessUniversalQueryStoredProcedure() : [ {SqlCommand . ToUpper ( )} ]" );

						//=================================================================================//
						var strresult = sqlCon . Query<string> ( SqlCommand , parameters , commandType: CommandType . StoredProcedure );
						//=================================================================================//

						Debug . WriteLine ( $"ProcessUniversalQueryStoredProcedure() : {SqlCommand} returned  RESULT = {strresult}" );
						//string s = parameters . Get<string> ("returnval");
						// set (ref) parameters  to be  returned
						Objtype = typeof ( string );
						if ( strresult != null )
						{
							//int tryintresult = 0;
							//int . TryParse ( strresult , out int tryintresult );
							//if ( tryintresult > 0 )
							//count = tryintresult;
							obj = ( object ) strresult;
							Objtype = typeof ( string );
							resultstring = "SUCCESS";
						}
						else
						{
							obj = null;
							count = -1;
							resultstring = "FAIL";
						}
						//if ( strresult . ToList ( ) . Count == 0 )
						//    return null;
						return obj;
						//}
					}

					//**************************************************************************************************************************************************//
					// COLLECTION - TEXT -  using Text command to get an IEnumerable  collection 
					//**************************************************************************************************************************************************//
					else if ( method == 3 )
					{
						//=================================================================================//
						var IEList = sqlCon . Query<dynamic> ( SqlCommand , parameters , commandType: CommandType . Text );
						//=================================================================================//

						Debug . WriteLine ( $"ProcessUniversalQueryStoredProcedure() (method=4): {SqlCommand} returned  RESULT = {IEList}" );
						//string restring = parameters . Get<string> ( "result" );
						ObservableCollection<GenericClass> newtable = new ObservableCollection<GenericClass> ( );
						foreach ( var rows in IEList )
						{
							var fields = rows as IDictionary<string , object>;
							var sum = fields [ "" ];
							newtable . Add ( fields as GenericClass );
							// ...
						}
						Objtype = typeof ( IEnumerable<dynamic> );
						obj = ( object ) IEList;
						count = IEList . Count ( );
						resultstring = "SUCCESS";
						return IEList;
					}


					//**************************************************************************************************************************************************//
					// INT - TEXT using Text command version to return a dynamic (int )
					// ACTUALLY BLOODY WORKING 18/11/2022
					//**************************************************************************************************************************************************//
					else if ( method == 4 )
					{
						//=================================================================================//
						IEnumerable<string> reslt = sqlCon . Query<string> ( SqlCommand , parameters , commandType: CommandType . StoredProcedure );
						//=================================================================================//

						// this is how to parse the int value out of the dynamic 'reslt' variable returned to us
						//int intresult = reslt [ 0 ];
						Objtype = typeof ( int );
						obj = ( object ) reslt;
						count = 1;
						resultstring = "SUCCESS";
						return obj;
					}

					//**************************************************************************************************************************************************//
					// LIST<STRING> - STOREDPPROC - using   Stord Procedure version to return a dynamic List<string>
					//**************************************************************************************************************************************************//
					else if ( method == 5 )
					{
						$" calling {SqlCommand} ()" . CW ( );
						//**************************************************************************************************************************************************//
						var queryresults = sqlCon . Query<string> ( SqlCommand , parameters , commandType: CommandType . StoredProcedure ) . ToList ( );
						//**************************************************************************************************************************************************//
						//result = sqlCon . Execute ( SqlCommand , parameters , commandType: CommandType . StoredProcedure );
						Debug . WriteLine ( $"{SqlCommand} returned  RESULT = {queryresults . Count}" );
						Objtype = typeof ( List<string> );
						obj = ( object ) queryresults;
						count = queryresults . Count;
						resultstring = "SUCCESS";
						return obj;
					}
					//**************************************************************************************************************************************************//
					// INT - STORED PPROC - to return a dynamic (int )
					// ACTUALLY BLOODY WORKING 29/11/2022
					//**************************************************************************************************************************************************//
					else if ( method == 6 )
					{
//						if(argsbuffer.Count > 1)
						//parameters=
						//=================================================================================//
						var reslt = sqlCon . Query<int> ( SqlCommand , parameters , commandType: CommandType . StoredProcedure );
						//=================================================================================//
						// How to retrieve an int from IEnumerable<int>
						int result = 0;
						foreach ( var item in reslt )
						{
							result = item;
						}
						Dictionary<string , object> dict = new Dictionary<string , object> ( );
						//dict = reslt . ToDictionary (x => reslt.count);
						Objtype = typeof ( int );
						obj = ( object ) reslt;
						count = result;
						resultstring = "SUCCESS";
						return obj;
					}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"SQL error : [ {ex . Message} ]" );
				error = $"SQL error : [ {ex . Message} ]";
				$"{ex . Message} [ {ex . Data} ]" . err ( );
				Utils . DoErrorBeep ( );

			}
			return null;
		}

		//#####################################################################################//

		public static int CreateGenericCollection (
				 ref ObservableCollection<GenericClass> collection ,
				 string SqlCommand ,
				 string ConnectionString ,
				 string Arguments ,
				 string [ ] args ,
				 string WhereClause ,
				 string OrderByClause ,
				 ref List<string> genericlist ,
				 ref string errormsg )
		{
			//====================================
			// Use DAPPER to run a Stored Procedure
			//====================================
			string result = "";
			errormsg = "";
			genericlist = new List<string> ( );
			string arg1 = "", arg2 = "", arg3 = "", arg4 = "";
			Dictionary<string , object> dict = new Dictionary<string , object> ( );

			string Con = GetCheckCurrentConnectionString ( MainWindow . CurrentSqlTableDomain );
			using ( IDbConnection db = new SqlConnection ( Con ) )
			{
				try
				{
					// Use DAPPER to run  Stored Procedure
					try
					{
						// Parse out the arguments and put them in correct order for all SP's
						if ( Arguments . Length > 0 )
						{
							if ( Arguments . Contains ( "'" ) )
							{
								bool [ ] argsarray = { false , false , false , false };
								int argscount = 0;
								// we maybe have args in quotes
								args = Arguments . Trim ( ) . Split ( '\'' );
								for ( int x = 0 ; x < args . Length ; x++ )
								{
									if ( args [ x ] . Trim ( ) . Contains ( "," ) )
									{
										string tmp = args [ x ] . Trim ( );
										if ( tmp . Substring ( tmp . Length - 1 , 1 ) == "," )
										{
											tmp = tmp . Substring ( 0 , tmp . Length - 1 );
											args [ x ] = tmp;
											argsarray [ x ] = true;
											argscount++;
										}
										else
										{
											if ( args [ x ] != "" )
											{
												argsarray [ x ] = true;
												argscount++;
											}
										}
									}
								}
								for ( int x = 0 ; x < argsarray . Length ; x++ )
								{
									switch ( x )
									{
										case 0:
											if ( argsarray [ x ] == true )
												arg1 = args [ x ];
											break;
										case 1:
											if ( argsarray [ x ] == true )
												arg2 = args [ x ];
											break;
										case 2:
											if ( argsarray [ x ] == true )
												arg3 = args [ x ];
											break;
										case 3:
											if ( argsarray [ x ] == true )
												arg4 = args [ x ];
											break;
									}
								}
							}
							else if ( Arguments . Contains ( "," ) )
							{
								args = Arguments . Trim ( ) . Split ( ',' );
								//string[] args = DbName.Split(',');
								for ( int x = 0 ; x < args . Length ; x++ )
								{
									switch ( x )
									{
										case 0:
											arg1 = args [ x ];
											if ( arg1 . Contains ( "," ) )              // trim comma off
												arg1 = arg1 . Substring ( 0 , arg1 . Length - 1 );
											break;
										case 1:
											arg2 = args [ x ];
											if ( arg2 . Contains ( "," ) )              // trim comma off
												arg2 = arg2 . Substring ( 0 , arg2 . Length - 1 );
											break;
										case 2:
											arg3 = args [ x ];
											if ( arg3 . Contains ( "," ) )         // trim comma off
												arg3 = arg3 . Substring ( 0 , arg3 . Length - 1 );
											break;
										case 3:
											arg4 = args [ x ];
											if ( arg4 . Contains ( "," ) )         // trim comma off
												arg4 = arg4 . Substring ( 0 , arg4 . Length - 1 );
											break;
									}
								}
							}
							else
							{
								// One or No arguments
								arg1 = Arguments;
								if ( arg1 . Contains ( "," ) )              // trim comma off
									arg1 = arg1 . Substring ( 0 , arg1 . Length - 1 );
							}
							// Create our aguments using the Dynamic parameters provided by Dapper
							var Params = new DynamicParameters ( );
							if ( arg1 != "" )
								Params . Add ( "Arg1" , arg1 , DbType . String , ParameterDirection . Input , arg1 . Length );
							if ( arg2 != "" )
								Params . Add ( "Arg2" , arg2 , DbType . String , ParameterDirection . Input , arg2 . Length );
							if ( arg3 != "" )
								Params . Add ( "Arg3" , arg3 , DbType . String , ParameterDirection . Input , arg3 . Length );
							if ( arg4 != "" )
								Params . Add ( "Arg4" , arg4 , DbType . String , ParameterDirection . Input , arg4 . Length );
						}
						else
						{
							// doing it the string[] way
							var parameters = new DynamicParameters ( );
							parameters = ParseSqlArgs ( parameters , args );
						}
						// Call Dapper to get results using it's StoredProcedures method which returns
						// a Dynamic IEnumerable that we then parse via a dictionary into collection of GenericClass  records
						int colcount = 0, maxcols = 0;

						if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) )
						{
							//***************************************************************************************************************//
							// Performing a standard SELECT command but returning the data in a GenericClass structure	  (Bank/Customer/Details/etc)
							$"{SqlCommand}" . DapperTrace ( );
							IEnumerable<dynamic> reslt = null;
							if ( args . Length > 0 )
								reslt = db . Query ( SqlCommand , args , commandType: CommandType . Text );
							else
								reslt = db . Query ( SqlCommand , commandType: CommandType . Text );
							//***************************************************************************************************************//
							if ( reslt == null )
							{
								errormsg = "DT";
								return 0;
							}
							else
							{
								//Although this is duplicated  with the one below we CANNOT make it a method()
								errormsg = "DYNAMIC";
								int dictcount = 0;
								int fldcount = 0;
								try
								{
									foreach ( var item in reslt )
									{
										GenericClass gc = new GenericClass ( );
										try
										{
											// we need to create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
											string buffer = "";
											List<int> VarcharList = new List<int> ( );
											gc = ParseDapperRow ( item , dict , out colcount , ref VarcharList );
											dictcount = 1;
											fldcount = dict . Count;
											string tmp = "";
											foreach ( var pair in dict )
											{
												try
												{
													if ( pair . Key != null && pair . Value != null )
													{
														AddDictPairToGeneric ( gc , pair , dictcount++ );
														tmp = pair . Key . ToString ( ) + "=" + pair . Value . ToString ( );
														buffer += tmp + ",";
													}
												}
												catch ( Exception ex )
												{
													Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
													Utils . DoErrorBeep ( );
													result = ex . Message;
												}
											}
											//remove trailing comma
											string s = buffer . Substring ( 0 , buffer . Length - 1 );
											buffer = s;
											genericlist . Add ( buffer );
										}
										catch ( Exception ex )
										{
											result = $"SQLERROR : {ex . Message}";
											Utils . DoErrorBeep ( );
											errormsg = result;
											Debug . WriteLine ( result );
										}
										collection . Add ( gc );
										dict . Clear ( );
										dictcount = 1;
									}
								}
								catch ( Exception ex )
								{
									Debug . WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
									result = ex . Message;
									Utils . DoErrorBeep ( );
									errormsg = result;
								}
								if ( errormsg == "" )
									errormsg = $"DYNAMIC:{fldcount}";
								return collection . Count;
							}
						}
						else
						{
							// probably a stored procedure ?  							
							bool IsSuccess = false;
							int fldcount = 0;

							//***************************************************************************************************************//
							// This returns the data from SP commands (only) in a GenericClass Structured format
							string argsbuff = "";
							if ( arg1 != "" )
								argsbuff += $"{arg1}";
							if ( arg2 != "" )
								argsbuff += $", {arg2}";
							if ( arg3 != "" )
								argsbuff += $", {arg3}";
							if ( arg4 != "" )
								argsbuff += $", {arg4}";
							$"{SqlCommand} {argsbuff}" . DapperTrace ( );

							var reslt = db . Query ( SqlCommand , null , commandType: CommandType . StoredProcedure );
							//***************************************************************************************************************//

							if ( reslt != null )
							{
								//Although this is duplicated  with the one above we CANNOT make it a method()
								int dictcount = 0;
								dict . Clear ( );
								long zero = reslt . LongCount ( );
								try
								{
									foreach ( var item in reslt )
									{
										GenericClass gc = new GenericClass ( );
										try
										{
											//	Create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
											List<int> VarcharList = new List<int> ( );
											gc = ParseDapperRow ( item , dict , out colcount , ref VarcharList );
											dictcount = 1;
											fldcount = dict . Count;
											if ( fldcount == 0 )
											{
												//no problem, we will get a Datatable anyway
												return 0;
											}
											string buffer = "", tmp = "";
											foreach ( var pair in dict )
											{
												try
												{
													if ( pair . Key != null && pair . Value != null )
													{
														AddDictPairToGeneric ( gc , pair , dictcount++ );
														tmp = pair . Key . ToString ( ) + "=" + pair . Value . ToString ( );
														buffer += tmp + ",";
													}
												}
												catch ( Exception ex )
												{
													Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
													Utils . DoErrorBeep ( );
													result = ex . Message;
												}
											}
											IsSuccess = true;
											string s = buffer . Substring ( 0 , buffer . Length - 1 );
											buffer = s;
											genericlist . Add ( buffer );
										}
										catch ( Exception ex )
										{
											result = $"SQLERROR : {ex . Message}";
											Utils . DoErrorBeep ( );
											Debug . WriteLine ( result );
											return 0;
										}
										//										gc . ActiveColumns = dict . Count;
										//ParseListToDbRecord ( genericlist , out gc );
										collection . Add ( gc );
										dict . Clear ( );
										dictcount = 1;
									}
								}
								catch ( Exception ex )
								{
									Debug . WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
									if ( ex . Message . Contains ( "not find stored procedure" ) )
									{
										result = $"SQL PARSE ERROR - [{ex . Message}]";
										$"SQL PARSE ERROR - [{ex . Message}]" . err ( );
										Utils . DoErrorBeep ( );
										errormsg = $"{result}";
										return 0;
									}
									else
									{
										long x = reslt . LongCount ( );
										if ( x == ( long ) 0 )
										{
											result = $"ERROR : [{SqlCommand}] returned ZERO records... ";
											$"ERROR : [{SqlCommand}] returned ZERO records... " . err ( );
											errormsg = $"DYNAMIC:0";
											return 0;
										}
										else
										{
											result = ex . Message;
											errormsg = $"UNKNOWN :{ex . Message}";
										}
										return 0;
									}
								}
							}
							if ( IsSuccess == false )
							{
								errormsg = $"Dapper request returned zero results, maybe one or more arguments are required, or the Procedure does not return any values ?";
								$"Dapper request returned zero results, maybe one or more arguments are required, or the Procedure does not return any values ?" . err ( );
								Debug . WriteLine ( errormsg );
							}
							else
								return fldcount;
							//return 0;
						}
					}
					catch ( Exception ex )
					{
						Debug . WriteLine ( $"STORED PROCEDURE ERROR : {ex . Message}" );
						$"STORED PROCEDURE ERROR : {ex . Message}" . err ( );
						Utils . DoErrorBeep ( );
						result = ex . Message;
						errormsg = $"SQLERROR : {result}";
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Sql Error, {ex . Message}" );
					Utils . DoErrorBeep ( );
					$"Sql Error, {ex . Message}" . err ( );
					result = ex . Message;
				}
			}
			$"{SqlCommand} Loaded {dict . Count} records" . DapperTrace ( );

			return dict . Count;
		}
		private static void GetReturnString ( int newcount , out int count , out string resultstring )
		{
			if ( newcount > 0 )
			{
				count = newcount;
				resultstring = "SUCCESS";
			}
			else
			{
				count = 0;
				resultstring = "FAIL";
			}
			return;
		}
		public static List<string> CallStoredProcedure ( List<string> list , string sqlcommand , string [ ] args = null )
		{
			//            List<string> list = new List<string> ( );
			list = GenDapperQueries . ProcessUniversalQueryStoredProcedure ( sqlcommand , args , MainWindow . CurrentSqlTableDomain , out string err );
			//This call returns us a List<string>
			// This method is NOT a dynamic method
			return list;
		}

		///************************************************************************************************///
		/// <summary>
		/// Class to handle calls via Dapper that returns a list<string> by calling Dapper
		/// </summary>
		/// <param name="spCommand">The SP to be run</param>
		/// <param name="args">whatever arguments the SP requires</param>
		/// <param name="CurrentTableDomain">Current Db Domain</param>
		/// <param name="err">out string to hold any error messages that may occur</param>
		/// <returns>List<string></string></returns>
		///************************************************************************************************///
		static public List<string> ProcessUniversalQueryStoredProcedure ( string spCommand , string [ ] args , string CurrentTableDomain , out string err )
		{
			int result = -1;
			string Con = "";
			err = "";
			Con = CheckSetSqlDomain ( CurrentTableDomain );
			if ( Con == "" )
			{
				// set to our local definition
				Con = MainWindow . CurrentSqlTableDomain;
				MessageBox . Show ( $"It was not possible to Identify a valid Sql Connection string for \nthe Database [ {CurrentTableDomain . ToUpper ( )} ]\n\n Please report  this error to DB Technical Support" , "Connection Error" );
				return null;
			}

			SqlConnection sqlCon = new SqlConnection ( );
			List<string> queryresults = new List<string> ( );

			//"" . Track ( );
			Mouse . OverrideCursor = Cursors . Wait;
			Debug . WriteLine ( $"Running Stored Procedure {spCommand}" );
			using ( sqlCon = new SqlConnection ( Con ) )
			{
				sqlCon . Open ( );
				// Now add record  to SQL table
				bool hasoutput = false;
				bool hasretval = false;
				var parameters = new DynamicParameters ( );

				parameters = SProcsSupport . ParseSqlArgs ( parameters , args , ref hasoutput );
				try
				{
					$" calling {spCommand} ()" . CW ( );

					Debug . WriteLine ( $"ProcessUniversalQueryStoredProcedure() : [ {spCommand . ToUpper ( )} ]" );

					//***********************************************************************************************************************************************//
					// returns a list<string>
					queryresults = sqlCon . Query<string> ( spCommand , parameters , commandType: CommandType . StoredProcedure ) . ToList ( );
					//***********************************************************************************************************************************************//

					//result = sqlCon . Execute ( spCommand , parameters , commandType: CommandType . StoredProcedure );
					Debug . WriteLine ( $"{spCommand} returned  RESULT = {queryresults . Count}" );
				}
				catch ( Exception ex )
				{
					Utils . DoErrorBeep ( );
					Debug . WriteLine ( $"{ex . Message}" );
					result = -1;
					err = ex . Message;
				}
				Mouse . OverrideCursor = Cursors . Arrow;
				//"" . Track ( 1 );
				return queryresults;
			}
		}

		static public DynamicParameters ParseSqlArgs ( DynamicParameters parameters , string [ ] args , bool hasretval = false )
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
		//#####################################################################################//

	}
}
