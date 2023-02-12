using System;
using System . Collections;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Xml . Linq;

using Microsoft . Data . SqlClient;

using ViewModels;

using Views;

using Wpfmain;

using static Dapper . SqlMapper;

namespace Dapper
{
	public static class DapperSupport

	{
		static int[] dummyargs = { 0 , 0 , 0 };

		public static string ConnString { get; private set; }

		/// <summary>
		/// returns the  List<string " genericlist as well as filling out the  "collection"
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="SqlCommand"></param>
		/// <param name="Arguments"></param>
		/// <param name="WhereClause"></param>
		/// <param name="OrderByClause"></param>
		/// <param name="genericlist"></param>
		/// <param name="errormsg"></param>
		/// <returns></returns>
		public static int CreateGenericCollection (
			string Domain ,
			ref ObservableCollection<GenericClass> collection ,
			string SqlCommand ,
			string Arguments ,
			string WhereClause ,
			string OrderByClause ,
			ref List<string> genericlist ,
			ref string errormsg )
		{
			//			out string DbToOpen ,
			//====================================
			// Use DAPPER to run a Stored Procedure
			//====================================
			string result = "";
			bool HasArgs = false;
			int argcount = 0;
			//DbToOpen = "";
			errormsg = "";
			IEnumerable resultDb;
			genericlist = new List<string> ( );
			string arg1 = "", arg2 = "", arg3 = "", arg4 = "";
			Dictionary<string , object> dict = new Dictionary<string , object> ( );
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				// This sets Flags.CurrentConnectionString for us if possible
				CheckDbDomain ( Flags . DbDomain );
				//ConString = SqlSupport.LoadConnectionStrings ( );
				ConString = Flags . CurrentConnectionString;
			}
			using ( IDbConnection db = new SqlConnection ( ConString ) )
			{

				try
				{
					// Use DAPPER to run  Stored Procedure
					try
					{
						// Parse out the arguments and put them in correct order for all SP's
						if ( Arguments . Contains ( "'" ) )
						{
							bool[] argsarray = { false , false , false , false };
							int argscount = 0;
							// we maybe have args in quotes
							string[] args = Arguments . Trim ( ) . Split ( '\'' );
							for ( int x = 0 ; x < args . Length ; x++ )
							{
								if ( args [ x ] . Trim ( ) . Contains ( "," ) )
								{
									string tmp = args[x] . Trim ( );
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
							string[] args = Arguments . Trim ( ) . Split ( ',' );
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
						// Call Dapper to get results using it's StoredProcedures method which returns
						// a Dynamic IEnumerable that we then parse via a dictionary into collection of GenericClass  records
						int colcount = 0;

						if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) )
						{
							//***************************************************************************************************************//
							// Performing a standard SELECT command but returning the data in a GenericClass structure	  (Bank/Customer/Details/etc)
							var reslt = db . Query ( SqlCommand , CommandType . Text );
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
#pragma warning disable CS0219 // The variable 'index' is assigned but its value is never used
											int index = 0;
#pragma warning restore CS0219 // The variable 'index' is assigned but its value is never used
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
							using ( IDbConnection db1 = new SqlConnection ( ConString ) )
							{

								//***************************************************************************************************************//
								// This returns the data from SP commands (only) in a GenericClass Structured format
								var reslt = db1 . Query ( SqlCommand , Params , commandType: CommandType . StoredProcedure );
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
											errormsg = $"{result}";
											return 0;
										}
										else
										{
											long x = reslt . LongCount ( );
											if ( x == ( long ) 0 )
											{
												result = $"ERROR : [{SqlCommand}] returned ZERO records... ";
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
									Debug . WriteLine ( errormsg );
								}
								else
									return fldcount;
								//return 0;
							}
						}
					}
					catch ( Exception ex )
					{
						Debug . WriteLine ( $"STORED PROCEDURE ERROR : {ex . Message}" );
						result = ex . Message;
						errormsg = $"SQLERROR : {result}";
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
					result = ex . Message;
				}
			}
			return dict . Count;
		}
		private static void ParseListToDbRecord ( List<string> genericlist , out GenericClass collection )
		{
			GenericClass gc = new GenericClass ( );
#pragma warning disable CS0168 // The variable 'fields' is declared but never used
			string[] fields;
#pragma warning restore CS0168 // The variable 'fields' is declared but never used
			string input = genericlist[0];
#pragma warning disable CS0219 // The variable 'outerindex' is assigned but its value is never used
			int index = 1, outerindex = 0;
#pragma warning restore CS0219 // The variable 'outerindex' is assigned but its value is never used
			//foreach ( var item in genericlist )
			//{
			//	outerindex++;
			string[] data = input . Split ( ',' );
			//foreach ( var item in data )
			//{
			foreach ( var itemfld in data )
			{
				string[] dataitem = itemfld . Split ( '=' );
				FieldsToGenericRecord ( dataitem [ 1 ] , index++ , ref gc );
			}
			//Debug. WriteLine ( data[1] );
			//}
			collection = gc;
		}
		private static void FieldsToGenericRecord ( string item , int index , ref GenericClass gcc )
		{
			//			GenericClass gcc = new GenericClass();
			switch ( index )
			{
				case 1:
					gcc . field1 = ( item . ToString ( ) );
					break;
				case 2:
					gcc . field2 = ( item . ToString ( ) );
					break;
				case 3:
					gcc . field3 = ( item . ToString ( ) );
					break;
				case 4:
					gcc . field4 = ( item . ToString ( ) );
					break;
				case 5:
					gcc . field5 = ( item . ToString ( ) );
					break;
				case 6:
					gcc . field6 = ( item . ToString ( ) );
					break;
				case 7:
					gcc . field7 = ( item . ToString ( ) );
					break;
				case 8:
					gcc . field8 = ( item . ToString ( ) );
					break;
				case 9:
					gcc . field9 = ( item . ToString ( ) );
					break;
				case 11:
					gcc . field10 = ( item . ToString ( ) );
					break;
				case 12:
					gcc . field11 = ( item . ToString ( ) );
					break;
				case 13:
					gcc . field12 = ( item . ToString ( ) );
					break;
				case 14:
					gcc . field13 = ( item . ToString ( ) );
					break;
				case 15:
					gcc . field14 = ( item . ToString ( ) );
					break;
				case 16:
					gcc . field15 = ( item . ToString ( ) );
					break;
				case 17:
					gcc . field16 = ( item . ToString ( ) );
					break;
				case 18:
					gcc . field17 = ( item . ToString ( ) );
					break;
				case 19:
					gcc . field18 = ( item . ToString ( ) );
					break;
				case 20:
					gcc . field19 = ( item . ToString ( ) );
					break;
				case 21:
					gcc . field20 = ( item . ToString ( ) );
					break;
			}
			//			gc = gcc;
		}

		public static DataTable GetDataTable ( string commandline )
		{
			DataTable dt = new DataTable ( );
			try
			{
				SqlConnection con;
				string ConString = Flags . CurrentConnectionString;
				if ( ConString == "" )
				{
					CheckDbDomain ( "IAN1" );
					ConString = Flags . CurrentConnectionString;
				}
				con = new SqlConnection ( ConString );
				using ( con )
				{
					SqlCommand cmd = new SqlCommand ( commandline , con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Db - {ex . Message}, {ex . Data}" );
				return null;
			}
			return dt;
		}


		#region	PERFORMSQLEXECUTECOMMAND
		public static int PerformSqlExecuteCommand ( string SqlCommand , string [ ] args , out string err )
		//--------------------------------------------------------------------------------------------------------------------------------------------------------
		{
			//####################################################################################//
			// Handles running a dapper stored procedure call with transaction support & thrws exceptions back to caller
			//####################################################################################//
			int gresult = -1;
			//string Con = Flags . CurrentConnectionString;
			string Con = ( string ) Wpfmain . Properties . Settings . Default["ConnectionString"];
			SqlConnection sqlCon = null;
			err = "";

			try
			{
				using ( sqlCon = new SqlConnection ( Con ) )
				{
					sqlCon . Open ( );
					using ( var tran = sqlCon . BeginTransaction ( ) )
					{
						if ( ( SqlCommand . ToUpper ( ) == "SPINSERTSPECIFIEDROW" || SqlCommand . ToUpper ( ) == "SPCREATETABLE" || SqlCommand . ToUpper ( ) == "SPDROPTABLE" ) && args . Length > 0 )
						{
							var parameters = new DynamicParameters ( );
							if ( args [ 0 ] != "" )
								parameters . Add ( "Tablename" , args [ 0 ] , DbType . String , ParameterDirection . Input , args [ 0 ] . Length );
							if ( args [ 1 ] != "" )
								parameters . Add ( "cmd" , args [ 1 ] , DbType . String , ParameterDirection . Input , args [ 1 ] . Length );
							if ( args [ 2 ] != "" )
								parameters . Add ( "Values" , args [ 2 ] , DbType . String , ParameterDirection . Input , args [ 2 ] . Length );

							gresult = sqlCon . Execute ( @SqlCommand , parameters , commandType: CommandType . StoredProcedure , transaction: tran );
						}
						else
						{
							// Perform the sql command requested
							var parameters = "";
							gresult = sqlCon . Execute ( @SqlCommand , parameters , commandType: CommandType . StoredProcedure , transaction: tran );// as IEnumerable<GenericClass>;
						}
						// Commit the transaction
						tran . Commit ( );
					}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Error {ex . Message}, {ex . Data}" );
				err = $"Error {ex . Message}";
			}

			"" . trace ( 1 );

			return gresult;
		}
		#endregion	PERFORMSQLEXECUTECOMMAND

		#region	PERFORMSQLEXECUTECOMMANDASYNC
		public static async Task<int> PerformSqlExecuteCommandAsync ( string SqlCommand , string [ ] args )
		//--------------------------------------------------------------------------------------------------------------------------------------------------------
		{
			//####################################################################################//
			// Handles running a dapper stored procedure call with transaction support & thrws exceptions back to caller
			//####################################################################################//
			int gresult = -1;
			//string Con= Flags . CurrentConnectionString;
			string Con = ( string ) Wpfmain . Properties . Settings . Default["ConnectionString"];
			SqlConnection sqlCon = null;
			//err = "";

			try
			{
				using ( sqlCon = new SqlConnection ( Con ) )
				{
					sqlCon . Open ( );
					using ( var tran = sqlCon . BeginTransaction ( ) )
					{
						if ( ( SqlCommand . ToUpper ( ) == "SPINSERTSPECIFIEDROW" || SqlCommand . ToUpper ( ) == "SPCREATETABLE" || SqlCommand . ToUpper ( ) == "SPDROPTABLE" ) && args . Length > 0 )
						{
							var parameters = new DynamicParameters ( );
							if ( args [ 0 ] != "" )
								parameters . Add ( "Tablename" , args [ 0 ] , DbType . String , ParameterDirection . Input , args [ 0 ] . Length );
							if ( args [ 1 ] != "" )
								parameters . Add ( "cmd" , args [ 1 ] , DbType . String , ParameterDirection . Input , args [ 1 ] . Length );
							if ( args [ 2 ] != "" )
								parameters . Add ( "Values" , args [ 2 ] , DbType . String , ParameterDirection . Input , args [ 2 ] . Length );

							//************************************************************************************************************************************************************************//
							gresult = sqlCon . Execute ( @SqlCommand , parameters , commandType: CommandType . StoredProcedure , transaction: tran );
							//************************************************************************************************************************************************************************//
						}
						else
						{
							// Perform the sql command requested
							var parameters = "";
							//************************************************************************************************************************************************************************//
							gresult = await sqlCon . ExecuteAsync ( @SqlCommand , parameters , commandType: CommandType . StoredProcedure , transaction: tran );// as IEnumerable<GenericClass>;
																																								//************************************************************************************************************************************************************************//
						}
						// Commit the transaction
						tran . Commit ( );
					}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Error {ex . Message}, {ex . Data}" );
			}
			return gresult;
		}
		#endregion	PERFORMSQLEXECUTECOMMANDASYNC

		#region	PERFORMSQLDBTEST
		//--------------------------------------------------------------------------------------------------------------------------------------------------------
		// 
		public static int PerformSqlDbTest ( string SqlCommand , out string err )
		{
			int gresult = -1;
			//string Con= Flags . CurrentConnectionString;
			string Con = ( string ) Wpfmain . Properties . Settings . Default["ConnectionString"];
			SqlConnection sqlCon = null;
			err = "";
			try
			{
				using ( sqlCon = new SqlConnection ( Con ) )
				{
					var parameters = "";
					var result = sqlCon . Query<GenericClass> ( @SqlCommand , parameters , commandType: CommandType . Text );
					foreach ( var item in result )
					{
						Debug . WriteLine ( $"content = {item . GetType ( ) . ToString ( )}" );
					}
					var v = result as GenericClass;
					//if ( Count > 0 )
					//result . Field1;
					err = result . ToString ( );
					;                       // RESULT HAS AT LEAST 1 FIELD, SO IT EXISTS
											//else
											//	err = "NOTFOUND";
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Error {ex . Message}, {ex . Data}" );
				if ( ex . Message . Contains ( "There is already an object named" ) )
					err = ex . Message;
				else
				{
					err = $"Error {ex . Message}";
				}
			}
			return gresult;
		}
		#endregion	PERFORMSQLDBTEST

		/// <summary>
		/// Special Method to return a  a single column from the requested Db  in a List
		/// if command includes more than one column name, only the 1st specified column is returned
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="SqlCommand"></param>
		/// <param name="err"></param>
		/// <returns>List 'of a single string/Column'</returns>

		public static List<string> LoadSingleColumnStringfromDb (
			string SqlCommand ,
			out string err )
		{
			err = "";
			List<string> entries = new List<string> ( );
			//string Con = Flags . CurrentConnectionString;
			string Con = ( string ) Wpfmain . Properties . Settings . Default["ConnectionString"];
			// only used tocreta Global collectionfrom our list
			//			IEnumerable  <SelectionEntry>  bvmi;
			// Read data via Dapper into list<BVM> cos Dapper uses Linq, so we cannot get other types returned
#pragma warning disable CS0219 // The variable 'sqlCon' is assigned but its value is never used
			SqlConnection sqlCon = null;
#pragma warning restore CS0219 // The variable 'sqlCon' is assigned but its value is never used
			using ( IDbConnection db = new SqlConnection ( Con ) )
			{
				try
				{
					//***************************************************************************************************************//
					entries = db . Query<string> ( SqlCommand ) . ToList ( );
					//***************************************************************************************************************//

					//foreach ( var item in entries )
					//{
					//	SelectionEntry newentry = new SelectionEntry();
					//	newentry . Entry = item;
					//	collection . Add ( newentry );
					//}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Sql Error : {ex . Message}" );
				}
				return entries;
			}
		}

		/// <summary>
		/// ASYNC version that Loads JUST the Customers that have More than ONE Bank Account with this bank
		/// </summary>
		/// <param name="collection">ObservableCollection<BankAccount></BankAccount></param>
		/// <param name="SqlCommand"></param>
		/// <param name="DbNameToLoad">The Bank Account  to load from</param>
		/// <param name="Notify">Trigger notification or not</param>
		/// <param name="Caller">Our Caller  Window name</param>
		/// <returns></returns>

		public static void GetMultiBankCollectionAsync ( ObservableCollection<BankAccountViewModel> collection ,
			string SqlCommand = "" ,
			string DbNameToLoad = "" ,
			string Orderby = "" ,
			string Conditions = "" ,
			bool Notify = false ,
			string Caller = "" ,
			int [ ] args = null )
		{
			ObservableCollection<BankAccountViewModel> bvmcollection = new ObservableCollection<BankAccountViewModel> ( );
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			string[] ValidFields =
							{
								"ID",
								"CUSTNO",
								"BANKNO",
								"ACTYPE",
								"INTRATE" ,
								"BALANCE" ,
								"ODATE" ,
								"CDATE"
								};
			string[] errorcolumns;
			using ( IDbConnection db = new SqlConnection ( ConString ) )
			{
				// Utility Support Methods to validate data
				if ( ValidateSortConditionColumns ( ValidFields , "Bank" , Orderby , Conditions , out errorcolumns ) == false )
				{
					if ( Orderby . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
					{
						MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Column name has been \nidentified in the Sorting Clause provided.\n\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}.\n\nTherefore No Sort will be performed for this Db" );
						Orderby = "";
					}
					else if ( Conditions . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
					{
						MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Match clause or Column Name \nhas been identified in the Data Selection Clause.\n\nThe Invalid item identified was :\n\t{errorcolumns [ 0 ]}\n\nTherefore No Data Matching will be performed for this Db" );
						Conditions = "";
					}
					else
					{
						MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but the Loading of the BankAccount Db is being aborted due to \na non existent Column name.\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}" );
						return;
					}
				}

				//// make sure order by clause is correctly formatted
				//if ( Orderby . Trim ( ) != "" )
				//{
				//	if ( Orderby . ToUpper ( ) . Contains ( "ORDER BY " ) == false )
				//	{
				//		Orderby = " Order by " + Orderby;
				//	}
				//}
				//if ( Conditions != "" )
				//{
				//	if ( Conditions . ToUpper ( ) . Contains ( "WHERE" ) == false )
				//		Conditions = " Where " + Conditions;
				//}

				try
				{
					// Use DAPPER to to load Bank data using Stored Procedure
					try
					{
						Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
						Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
						var Args = new { Arg1 = "" , Arg2 = " " , Arg3 = "" };
						SqlCommand = $"spLoadMultiBankAccountsOnly";
						if ( args [ 2 ] == 0 )
							Args = new { Arg1 = $" {DbNameToLoad} " , Arg2 = $" {Conditions} " , Arg3 = $" {Orderby}" };
						else if ( args [ 2 ] > 0 )
							Args = new { Arg1 = $" {DbNameToLoad} " , Arg2 = $" {Conditions}  " , Arg3 = $" {Orderby}" };

						//This syntax WORKS CORRECTLY

						var result = db . Query<BankAccountViewModel> ( SqlCommand , Args , null , false , null , CommandType . StoredProcedure ) . ToList ( );

						Debug . WriteLine ( result );
						//process the list of multi a/ c Bank accounts
						foreach ( var item in result )
						{
							bvmcollection . Add ( item );
						}

						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {result . Count} records successfuly" );
					}
					catch ( Exception ex )
					{
						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  FAILED : {ex . Message}" );
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  FAILED : {ex . Message}" );
				}
			}
			Application . Current . Dispatcher . Invoke ( ( ) =>
				EventControl . TriggerBankDataLoaded ( null ,
				new LoadedEventArgs
				{
					CallerType = "DAPPERSUPPORT" ,
					CallerDb = Caller ,
					DataSource = bvmcollection ,
					RowCount = bvmcollection . Count
				} )
				);
			return;
		}

		/// <summary>
		/// Loads JUST the Customers that have More than ONE Bank Account with this bank
		/// </summary>
		/// <param name="collection">ObservableCollection<BankAccount></BankAccount></param>
		/// <param name="SqlCommand"></param>
		/// <param name="DbNameToLoad">The Bank Account  to load from</param>
		/// <param name="Notify">Trigger notification or not</param>
		/// <param name="Caller">Our Caller  Window name</param>
		/// <returns></returns>
		//       public static ObservableCollection<BankAccountViewModel> GetMultiBankCollection(ObservableCollection<BankAccountViewModel> collection,
		//           string SqlCommand = "",
		//           string DbNameToLoad = "",
		//           string Orderby = "",
		//           string Conditions = "",
		//           bool Notify = false,
		//           string Caller = "",
		//           int[] args = null)
		//       {
		//           ObservableCollection<BankAccountViewModel> bvmcollection = new ObservableCollection<BankAccountViewModel>();
		//           string ConString = Flags.CurrentConnectionString;
		//           if (ConString == "")
		//           {
		//               DapperSupport.CheckDbDomain("IAN1");
		//               ConString = Flags.CurrentConnectionString;
		//           }
		//           string[] ValidFields =
		//                           {
		//                               "ID",
		//                               "CUSTNO",
		//                               "BANKNO",
		//                               "ACTYPE",
		//                               "INTRATE" ,
		//                               "BALANCE" ,
		//                               "ODATE" ,
		//                               "CDATE"
		//                               };
		//           string[] errorcolumns;
		//           using (IDbConnection db = new SqlConnection(ConString))
		//           {
		//               //Utility Support Methods to validate data
		//               if (ValidateSortConditionColumns(ValidFields, "Bank", Orderby, Conditions, out errorcolumns) == false)
		//               {
		//                   if (Orderby.ToUpper().Contains(errorcolumns[0]))
		//                   {
		//                       MessageBox.Show($"BANKACCOUNT dB\nSorry, but an invalid Column name has been \nidentified in the Sorting Clause provided.\n\nThe Invalid Column identified was :\n{errorcolumns[0]}.\n\nTherefore No Sort will be performed for this Db");
		//                       Orderby = "";
		//                   }
		//                   else if (Conditions.ToUpper().Contains(errorcolumns[0]))
		//                   {
		//                       MessageBox.Show($"BANKACCOUNT dB\nSorry, but an invalid Match clause or Column Name \nhas been identified in the Data Selection Clause.\n\nThe Invalid item identified was :\n\t{errorcolumns[0]}\n\nTherefore No Data Matching will be performed for this Db");
		//                       Conditions = "";
		//                   }
		//                   else
		//                   {
		//                       MessageBox.Show($"BANKACCOUNT dB\nSorry, but the Loading of the BankAccount Db is being aborted due to \na non existent Column name.\nThe Invalid Column identified was :\n{errorcolumns[0]}");
		//                       return null;
		//                   }
		//               }
		//               DynamicParameters Args = new DynamicParameters();
		//               //make sure order by clause is correctly formatted
		//               if (Orderby.Trim() != "")
		//               {
		//                   if (Orderby.ToUpper().Contains("ORDER BY ") == false)
		//                   {
		//                       Orderby = " Order by " + Orderby;
		//                   }
		//               }
		//               if (Conditions != "")
		//               {
		//                   if (Conditions.ToUpper().Contains("WHERE") == false)
		//                       Conditions = " Where " + Conditions;
		//               }
		//               // Format is "SELECT " + [*] [Top(x) *] [fields + (fields list...) ]
		//               Args.Add("@arg1", DbNameToLoad);
		//               try
		//               {
		//                   //Use DAPPER to to load Bank data using Stored Procedure
		//                   try
		//                   {
		//                       Orderby = Orderby.Contains("Order by") ? Orderby.Substring(9) : Orderby;
		//                       Conditions = Conditions.Contains("where ") ? Conditions.Substring(6) : Conditions;
		////                       var Args = new { Arg1 = "", Arg2 = " ", Arg3 = "" };
		//                       SqlCommand = $"spLoadMultiBankAccountsOnly";
		//                       //if (args[2] == 0)
		//                       //    Args = new { Arg1 = $" {DbNameToLoad} ", Arg2 = $" {Conditions} ", Arg3 = $" {Orderby}" };
		//                       //else if (args[2] > 0)
		//                       //    Args = new { Arg1 = $" {DbNameToLoad} ", Arg2 = $" {Conditions}  ", Arg3 = $" {Orderby}" };
		//                       if (args[2] == 0)
		//                       {
		//                           string str = $" DbName = {DbNameToLoad}, * , Conditions = {Conditions} , SortBy = {Orderby}";
		//                           Args.Add("@arg1", DbNameToLoad);
		//                           Args.Add("@arg2", $"*");
		//                         }

		//                       else if (args[2] > 0)
		//                       {

		//                           Args = new { "DbName = " + $" {DbNameToLoad} " + $" Arg = Top ({args[2].ToString()}) *  Conditions = {Conditions},   SortBy = {Orderby}" };
		//                       }
		//                       //This syntax WORKS CORRECTLY

		//                       var result = db.Query<BankAccountViewModel>(SqlCommand, Args, null, false, null, CommandType.StoredProcedure).ToList();

		//                       Debug.WriteLine(result);
		//                       foreach (var item in result)
		//                       {
		//                           bvmcollection.Add(item);
		//                       }
		//                       Debug.WriteLine($"SQL DAPPER {DbNameToLoad}  loaded : {result.Count} records successfuly");
		//                   }
		//                   catch (Exception ex)
		//                   {
		//                       Debug.WriteLine($"SQL DAPPER {DbNameToLoad}  FAILED : {ex.Message}");
		//                   }
		//               }
		//               catch (Exception ex)
		//               {
		//                   Debug.WriteLine($"SQL DAPPER {DbNameToLoad}  FAILED : {ex.Message}");
		//               }
		//           }
		//           if (Notify)
		//           {
		//               EventControl.TriggerBankDataLoaded(null,
		//                   new LoadedEventArgs
		//                   {
		//                       CallerType = "SQLSERVER",
		//                       CallerDb = Caller,
		//                       DataSource = bvmcollection,
		//                       RowCount = bvmcollection.Count
		//                   });
		//           }
		//           return bvmcollection;
		//       }


		#region Bank loading methods
		public async static Task<bool> GetBankObsCollectionAsync ( ObservableCollection<BankAccountViewModel> collection ,
			string SqlCommand = "" ,
			string DbNameToLoad = "" ,
			string Orderby = "" ,
			string Conditions = "" ,
			bool wantSort = false ,
			bool wantDictionary = false ,
			bool Notify = true ,
			string Caller = "" ,
			int [ ] args = null )
		{
			ObservableCollection<BankAccountViewModel> bvmcollection = new ObservableCollection<BankAccountViewModel> ( );
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			string[] ValidFields =
							{
								"ID",
								"CUSTNO",
								"BANKNO",
								"ACTYPE",
								"INTRATE" ,
								"BALANCE" ,
								"ODATE" ,
								"CDATE"
								};
			string[] errorcolumns;

			// Use defaullt Db if none received frm caller
			if ( DbNameToLoad == "" )
				DbNameToLoad = "BankAccount";

			// Utility Support Methods to validate data
			if ( ValidateSortConditionColumns ( ValidFields , "Bank" , Orderby , Conditions , out errorcolumns ) == false )
			{
				if ( Orderby . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Column name has been \nidentified in the Sorting Clause provided.\n\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}.\n\nTherefore No Sort will be performed for this Db" );
					Orderby = "";
				}
				else if ( Conditions . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Match clause or Column Name \nhas been identified in the Data Selection Clause.\n\nThe Invalid item identified was :\n\t{errorcolumns [ 0 ]}\n\nTherefore No Data Matching will be performed for this Db" );
					Conditions = "";
				}
				else
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but the Loading of the BankAccount Db is being aborted due to \na non existent Column name.\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}" );
					return false;
				}
			}

			//// make sure order by clause is correctly formatted
			//if ( Orderby . Trim ( ) != "" )
			//{
			//	if ( Orderby . ToUpper ( ) . Contains ( "ORDER BY " ) == false )
			//	{
			//		Orderby = " Order by " + Orderby;
			//	}
			//}

			//if ( Conditions != "" )
			//{
			//	if ( Conditions . ToUpper ( ) . Contains ( "WHERE" ) == false )
			//		Conditions = " Where " + Conditions;
			//}

			if ( Flags . USEADOWITHSTOREDPROCEDURES )
			{
				//====================================================
				// Use standard ADO.Net to to load Bank data to run Stored Procedure
				//====================================================
				BankAccountViewModel bvm = new BankAccountViewModel ( );
				//string Con= Flags . CurrentConnectionString;
				string Con = ( string ) Wpfmain . Properties . Settings . Default["BankSysConnectionString"];
				SqlConnection sqlCon = null;

				// Works with default command 31/10/21
				// works with Records limited 31/10/21
				// works with Selection conditions limited 31/10/21
				// works with Sort conditions 31/10/21
				try
				{
					using ( sqlCon = new SqlConnection ( Con ) )
					{
						SqlCommand sql_cmnd;
						sqlCon . Open ( );
						if ( SqlCommand != "" )
							sql_cmnd = new SqlCommand ( SqlCommand , sqlCon );
						else
						{
							sql_cmnd = new SqlCommand ( "dbo.spLoadBankAccountComplex " , sqlCon );

							sql_cmnd . CommandType = CommandType . StoredProcedure;
							// Now handle parameters
							sql_cmnd . Parameters . AddWithValue ( "@Arg1" , SqlDbType . NVarChar ) . Value = DbNameToLoad;
							if ( args == null )
								args = dummyargs;
							if ( args . Length > 0 )
							{
								if ( args [ 2 ] > 0 )
								{
									string limits = $" Top ({args[2]}) ";
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = limits;
								}
								else
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";
							}
							else
								sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";

							Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
							sql_cmnd . Parameters . AddWithValue ( "@Arg3" , SqlDbType . NVarChar ) . Value = Conditions;
							Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
							sql_cmnd . Parameters . AddWithValue ( "@Arg4" , SqlDbType . NVarChar ) . Value = Orderby;
						}
						var sqlDr = sql_cmnd . ExecuteReader ( );
						while ( sqlDr . Read ( ) )
						{
							bvm . Id = int . Parse ( sqlDr [ "ID" ] . ToString ( ) );
							bvm . CustNo = sqlDr [ "CustNo" ] . ToString ( );
							bvm . BankNo = sqlDr [ "BankNo" ] . ToString ( );
							bvm . AcType = int . Parse ( sqlDr [ "ACTYPE" ] . ToString ( ) );
							bvm . Balance = Decimal . Parse ( sqlDr [ "BALANCE" ] . ToString ( ) );
							bvm . IntRate = Decimal . Parse ( sqlDr [ "INTRATE" ] . ToString ( ) );
							bvm . ODate = DateTime . Parse ( sqlDr [ "ODATE" ] . ToString ( ) );
							bvm . CDate = DateTime . Parse ( sqlDr [ "CDATE" ] . ToString ( ) );
							bvmcollection . Add ( bvm );
							bvm = new BankAccountViewModel ( );
						}
						sqlDr . Close ( );
						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {bvmcollection . Count} records successfuly" );
					}
					sqlCon . Close ( );
				}
				catch ( Exception ex )

				{
					Debug . WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
					return false;
				}
			}
			else
			{
				//====================================
				// Use STD DAPPER QUERY to load Bank data
				//====================================
				IEnumerable<BankAccountViewModel> bvmi;
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					try
					{
						// Use DAPPER to to load Bank data using Stored Procedure
						if ( Flags . USEDAPPERWITHSTOREDPROCEDURE )
						{
							try
							{
								Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
								Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
								SqlCommand = $"spLoadBankAccountComplex";
								var Args = new { Arg1 = "" , Arg2 = " " , Arg3 = "" , Arg4 = "" };
								if ( args [ 2 ] == 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								else if ( args [ 2 ] > 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								// This syntax WORKS CORRECTLY

								var result = db . Query<BankAccountViewModel> ( SqlCommand , Args , null , false , null , CommandType . StoredProcedure ) . ToList ( );

								Debug . WriteLine ( result );
								foreach ( var item in result )
								{
									bvmcollection . Add ( item );
								}
								Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {result . Count} records successfuly" );
							}
							catch ( Exception ex )
							{
								Debug . WriteLine ( $"BANK  DB ERROR : {ex . Message}" );
							}
						}
						else if ( Flags . USESDAPPERSTDPROCEDURES == true )
						{
							//====================================
							// Use standard DAPPER code to load Bank data
							//====================================
							Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
							Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
							if ( Conditions != "" )
							{
								if ( args [ 2 ] > 0 && Orderby != "" )
									SqlCommand = $" Select top ({args [ 2 ]}) * from {DbNameToLoad} where {Conditions} Order by {Orderby}";
								else
									SqlCommand = $" Select * from {DbNameToLoad} where {Conditions}";
							}
							else
							{
								if ( args != null )
								{
									if ( Conditions == "" && Orderby == "" && args [ 0 ] == 0 && args [ 1 ] == 0 && args [ 2 ] == 0 )   // we dont even  have args for total records
										SqlCommand = $" Select * from {DbNameToLoad} ";
									else if ( args [ 0 ] != 0 || args [ 1 ] != 0 || args [ 2 ] != 0 )   // we do have args for total records
									{
										if ( args [ 2 ] == 0 )       // no limit on how many records to get
										{
											SqlCommand = $" Select * from {DbNameToLoad} ";
											if ( Conditions != "" )
												SqlCommand += $" {Conditions} ";
											else if ( args [ 1 ] != 0 )
												SqlCommand += $" where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]} ";
										}
										else if ( args [ 2 ] > 0 && args [ 1 ] == 0 )
											SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} ";
										else if ( args [ 1 ] > 0 )// All 3 args are received
											SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
										else
											SqlCommand = $" Select * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
									}
								}
								if ( Conditions != "" )  // We have conditions
									SqlCommand = $"Select * from {DbNameToLoad} {Conditions} ";
								else if ( args == null || args . Length == 0 )    // No args or conditions, so use generic command
									SqlCommand = $"Select * from {DbNameToLoad}  ";
								// Final Trap to ensure we have a valid command line
								if ( SqlCommand == "" )
									SqlCommand = $" Select * from {DbNameToLoad} ";

								if ( wantSort && Orderby != "" )

									SqlCommand += $" order by {Orderby}";
							}
							// Read data via Dapper into list<BVM> cos Dapper uses Linq, so we cannot get other types returned
							bvmi = db . Query<BankAccountViewModel> ( SqlCommand );

							foreach ( var item in bvmi )
							{
								bvmcollection . Add ( item );
							}
							collection = bvmcollection;
						}
					}
					catch ( Exception ex )
					{
						Debug . WriteLine ( $"SQL DAPPER error : {ex . Message}, {ex . Data}" );
						return false;
					}
					finally
					{
						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {bvmcollection . Count} records successfuly" );
					}
				}
			}
			if ( Notify )
			{
				EventControl . TriggerBankDataLoaded ( null ,
					new LoadedEventArgs
					{
						CallerType = "SQLSERVER" ,
						CallerDb = Caller ,
						DataSource = bvmcollection ,
						RowCount = bvmcollection . Count
					} );
			}
			return true;
		}

		public static ObservableCollection<BankAccountViewModel> GetBankObsCollection ( ObservableCollection<BankAccountViewModel> collection ,
		string SqlCommand = "" ,
		string DbNameToLoad = "" ,
		string Orderby = "" ,
		string Conditions = "" ,
		bool wantSort = false ,
		bool wantDictionary = false ,
		bool Notify = false ,
		string Caller = "" ,
		int [ ] args = null )
		{
			ObservableCollection<BankAccountViewModel> bvmcollection = new ObservableCollection<BankAccountViewModel> ( );
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			string[] ValidFields =
							{
								"ID",
								"CUSTNO",
								"BANKNO",
								"ACTYPE",
								"INTRATE" ,
								"BALANCE" ,
								"ODATE" ,
								"CDATE"
								};
			string[] errorcolumns;

			// Use defaullt Db if none received frm caller
			if ( DbNameToLoad == "" )
				DbNameToLoad = "BankAccount";

			if ( SqlCommand == null || ( SqlCommand == "" && DbNameToLoad == "" ) )
				return null;
			if ( DbNameToLoad == null )
				DbNameToLoad = "";
			if ( Orderby == null )
				Orderby = "";
			if ( Conditions == null )
				Conditions = "";
			// Utility Support Methods to validate data
			if ( ValidateSortConditionColumns ( ValidFields , "Bank" , Orderby , Conditions , out errorcolumns ) == false )
			{
				if ( Orderby . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Column name has been \nidentified in the Sorting Clause provided.\n\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}.\n\nTherefore No Sort will be performed for this Db" );
					Orderby = "";
				}
				else if ( Conditions . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Match clause or Column Name \nhas been identified in the Data Selection Clause.\n\nThe Invalid item identified was :\n\t{errorcolumns [ 0 ]}\n\nTherefore No Data Matching will be performed for this Db" );
					Conditions = "";
				}
				else
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but the Loading of the BankAccount Db is being aborted due to \na non existent Column name.\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}" );
					return null;
				}
			}
			if ( Flags . USEADOWITHSTOREDPROCEDURES )
			{
				//====================================================
				// Use standard ADO.Net to to load Bank data to run Stored Procedure
				//====================================================
				BankAccountViewModel bvm = new BankAccountViewModel ( );
				//string Con= Flags . CurrentConnectionString;
				string Con = ( string ) Wpfmain . Properties . Settings . Default["BankSysConnectionString"];
				SqlConnection sqlCon = null;

				// Works with default command 31/10/21
				// works with Records limited 31/10/21
				// works with Selection conditions limited 31/10/21
				// works with Sort conditions 31/10/21
				try
				{
					using ( sqlCon = new SqlConnection ( Con ) )
					{
						SqlCommand sql_cmnd;
						sqlCon . Open ( );
						Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
						Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
						if ( SqlCommand != "" )
							sql_cmnd = new SqlCommand ( SqlCommand , sqlCon );
						else
						{
							sql_cmnd = new SqlCommand ( "dbo.spLoadBankAccountComplex " , sqlCon );
							sql_cmnd . CommandType = CommandType . StoredProcedure;
							// Now handle parameters
							sql_cmnd . Parameters . AddWithValue ( "@Arg1" , SqlDbType . NVarChar ) . Value = DbNameToLoad;
							if ( args == null )
								args = dummyargs;
							if ( args . Length > 0 )
							{
								if ( args [ 2 ] > 0 )
								{
									string limits = $" Top ({args[2]}) ";
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = limits;
								}
							}
							Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
							if ( Conditions != "" )
								sql_cmnd . Parameters . AddWithValue ( "@Arg3" , SqlDbType . NVarChar ) . Value = Conditions;
							if ( Orderby != "" )
								sql_cmnd . Parameters . AddWithValue ( "@Arg4" , SqlDbType . NVarChar ) . Value = Orderby . Trim ( );
						}
						var sqlDr = sql_cmnd . ExecuteReader ( );
						while ( sqlDr . Read ( ) )
						{
							bvm . Id = int . Parse ( sqlDr [ "ID" ] . ToString ( ) );
							bvm . CustNo = sqlDr [ "CustNo" ] . ToString ( );
							bvm . BankNo = sqlDr [ "BankNo" ] . ToString ( );
							bvm . AcType = int . Parse ( sqlDr [ "ACTYPE" ] . ToString ( ) );
							bvm . Balance = Decimal . Parse ( sqlDr [ "BALANCE" ] . ToString ( ) );
							bvm . IntRate = Decimal . Parse ( sqlDr [ "INTRATE" ] . ToString ( ) );
							bvm . ODate = DateTime . Parse ( sqlDr [ "ODATE" ] . ToString ( ) );
							bvm . CDate = DateTime . Parse ( sqlDr [ "CDATE" ] . ToString ( ) );
							bvmcollection . Add ( bvm );
							bvm = new BankAccountViewModel ( );
						}
						sqlDr . Close ( );
						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {bvmcollection . Count} records successfuly" );
					}
					sqlCon . Close ( );
				}
				catch ( Exception ex )

				{
					Debug . WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
				}
				if ( Notify )
				{
					EventControl . TriggerBankDataLoaded ( null ,
					new LoadedEventArgs
					{
						CallerType = "DAPPERSUPPORT" ,
						CallerDb = Caller ,
						DataSource = bvmcollection ,
						RowCount = bvmcollection . Count
					} );
				}
			}
			else
			{
				//====================================
				// Use STD DAPPER QUERY to load Bank data
				//====================================
				IEnumerable<BankAccountViewModel> bvmi;
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
					Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
					try
					{
						// Use DAPPER to to load Bank data using Stored Procedure
						if ( Flags . USEDAPPERWITHSTOREDPROCEDURE )
						{
							try
							{
								var Args = new { Arg1 = "" , Arg2 = " " , Arg3 = "" , Arg4 = "" };
								SqlCommand = $"spLoadBankAccountComplex";
								if ( args [ 2 ] == 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								else if ( args [ 2 ] > 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };

								// This syntax WORKS CORRECTLY
								var result = db . Query<BankAccountViewModel> ( SqlCommand , Args , null , false , null , CommandType . StoredProcedure ) . ToList ( );

								Debug . WriteLine ( result );
								foreach ( var item in result )
								{
									bvmcollection . Add ( item );
								}
								Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {result . Count} records successfuly" );
							}
							catch ( Exception ex )
							{
								Debug . WriteLine ( $"BANK  DB ERROR : {ex . Message}" );
							}
						}
						else if ( Flags . USESDAPPERSTDPROCEDURES == true )
						{
							//====================================
							// Use standard DAPPER code to load Bank data
							//====================================
							if ( Conditions != "" )
							{
								if ( args [ 2 ] > 0 && Orderby != "" )
									SqlCommand = $" Select top ({args [ 2 ]}) * from {DbNameToLoad}  where {Conditions} Order by {Orderby}";
								else
									SqlCommand = $" Select * from {DbNameToLoad} where {Conditions}";
							}
							else
							{
								if ( args != null )
								{
									if ( Conditions == "" && Orderby == "" && args [ 0 ] == 0 && args [ 1 ] == 0 && args [ 2 ] == 0 )   // we dont even  have args for total records
										SqlCommand = $" Select * from {DbNameToLoad} ";
									else if ( args [ 0 ] != 0 || args [ 1 ] != 0 || args [ 2 ] != 0 )   // we do have args for total records
									{
										if ( args [ 2 ] == 0 )       // no limit on how many records to get
										{
											SqlCommand = $" Select * from {DbNameToLoad} ";
											if ( Conditions != "" )
												SqlCommand += $" {Conditions} ";
											else if ( args [ 1 ] != 0 )
												SqlCommand += $" where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]} ";
										}
										else if ( args [ 2 ] > 0 && args [ 1 ] == 0 )
											SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} ";
										else if ( args [ 1 ] > 0 )// All 3 args are received
											SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
										else
											SqlCommand = $" Select * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
									}
								}
								if ( Conditions != "" )  // We have conditions
									SqlCommand = $"Select * from {DbNameToLoad} {Conditions} ";
								else if ( args == null || args . Length == 0 )    // No args or conditions, so use generic command
									SqlCommand = $"Select * from {DbNameToLoad}  ";
								// Final Trap to ensure we have a valid command line
								if ( SqlCommand == "" )
									SqlCommand = $" Select * from {DbNameToLoad} ";

								if ( wantSort && Orderby != "" )
									SqlCommand += $" Order by {Orderby}";
							}
							// Read data via Dapper into list<BVM> cos Dapper uses Linq, so we cannot get other types returned
							bvmi = db . Query<BankAccountViewModel> ( SqlCommand );

							foreach ( var item in bvmi )
							{
								bvmcollection . Add ( item );
							}
							collection = bvmcollection;
						}
					}
					catch ( Exception ex )
					{
						Debug . WriteLine ( $"SQL DAPPER error : {ex . Message}, {ex . Data}" );
					}
					finally
					{
						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {bvmcollection . Count} records successfuly" );
					}
				}
			}
			if ( Notify )
			{
				EventControl . TriggerBankDataLoaded ( null ,
					new LoadedEventArgs
					{
						CallerType = "SQLSERVER" ,
						CallerDb = Caller ,
						DataSource = bvmcollection ,
						RowCount = bvmcollection . Count
					} );
			}
			collection = bvmcollection;
			return bvmcollection;
		}


		public static ObservableCollection<BankAccountViewModel> GetBankObsCollectionWithDict ( ObservableCollection<BankAccountViewModel> collection ,
			out Dictionary<int , int> Dict ,
			bool wantDictionary = false ,
			string SqlCommand = "" ,
			string DbNameToLoad = "" ,
			bool wantSort = false ,
			bool Notify = false ,
			string Caller = "" ,
			int [ ] args = null )
		{
			ObservableCollection<BankAccountViewModel> bvmcollection = new ObservableCollection<BankAccountViewModel> ( );
			List<BankAccountViewModel> bvmlist = new List<BankAccountViewModel> ( );
			Dictionary<int , int> BankDict = new Dictionary<int , int> ( );


			Dict = BankDict;
			//collection = bvmcollection;
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			if ( DbNameToLoad == "" )
				DbNameToLoad = "BankAccount";

			using ( IDbConnection db = new SqlConnection ( ConString ) )
			{
				try
				{
					if ( SqlCommand == "" && args != null )   // no command line received, but we do have args
					{
						if ( args [ 2 ] == 0 )       // no limit on how many records to get
							SqlCommand = $" Select * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
						else  // All 3 args are received
							SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
					}
					else if ( SqlCommand == "" && args == null )    // No inforeceived at all, so use generic command
						SqlCommand = "Select * from {DbNameToLoad} ";

					if ( wantSort )

						SqlCommand += $" order by CustNo, BankNo";
					// Read data via Dapper into list<BVM> cos Dapper uses Linq, so we cannot get other types returned
					bvmlist = db . Query<BankAccountViewModel> ( SqlCommand ) . ToList ( );

					if ( bvmlist . Count > 0 )
					{
						// We want a ObservableCollection<BankAccountViewModel>, so create it here, and also a dictionary<int, int>
						int counter = 0;
						foreach ( var item in bvmlist )
						{
							bvmcollection . Add ( item );
							if ( wantDictionary )
							{
								if ( BankDict . ContainsKey ( int . Parse ( item . CustNo ) ) == false )
									BankDict . Add ( int . Parse ( item . CustNo ) , counter++ );
							}
						}
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER error : {ex . Message}, {ex . Data}" );
				}
				finally
				{
					Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad} loaded : {bvmcollection . Count} records successfuly" );
				}
			}
			if ( Notify )
			{
				EventControl . TriggerBankDataLoaded ( null ,
					new LoadedEventArgs
					{
						CallerType = "SQLSERVER" ,
						CallerDb = Caller ,
						DataSource = bvmcollection ,
						RowCount = bvmcollection . Count
					} );
			}
			collection = bvmcollection;
			return bvmcollection;
		}

		public static ObservableCollection<BankAccountViewModel> LoadBankDataToList (
			ObservableCollection<BankAccountViewModel> BankCollection ,
			out Dictionary<int , int> Dict ,
			bool wantDictionary = false ,
			string SqlCommand = "" ,
			string DbNameToLoad = "" ,
			bool wantSort = false ,
			string Caller = "" ,
			bool NotifyCaller = false ,
			int [ ] args = null )
		{
			ObservableCollection<BankAccountViewModel> DbData = new ObservableCollection<BankAccountViewModel> ( );
			List<BankAccountViewModel> bvmlist = new List<BankAccountViewModel> ( );
			Dictionary<int , int> BankDict = new Dictionary<int , int> ( );
			int counter = 0;

			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			Dict = BankDict;
			if ( BankCollection == null )
				BankCollection = DbData;

			if ( DbNameToLoad == "" )
				DbNameToLoad = "BankAccount";
			using ( IDbConnection db = new SqlConnection ( ConString ) )
			{
				try
				{
					if ( SqlCommand == "" && args != null )   // no command line received, but we do have args
					{
						if ( args [ 2 ] == 0 )       // no limit on how many records to get
							SqlCommand = $" Select * from {DbNameToLoad}  where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
						else  // All 3 args are received
							SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
					}
					else if ( SqlCommand == "" && args == null )    // No inforeceived at all, so use generic command
						SqlCommand = "Select * from {DbNameToLoad} ";

					if ( wantSort )

						SqlCommand += $" order by CustNo, BankNo";

					bvmlist = db . Query<BankAccountViewModel> ( SqlCommand ) . ToList ( );

					if ( bvmlist . Count > 0 )
					{
						foreach ( var item in bvmlist )
						{
							if ( BankDict . ContainsKey ( int . Parse ( item . BankNo ) ) == false )
								BankDict . Add ( int . Parse ( item . BankNo ) , counter++ );
						}
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER error in DAPPERSUPPPORT. LOADBANKDATAVIALIST: {ex . Message}, {ex . Data}" );
				}
				finally
				{
					Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad} loaded : {BankCollection . Count} records successfuly" );
				}
			}
			//if ( NotifyCaller )
			//{
			//	EventControl . TriggerBankDataLoaded ( null ,
			//		new LoadedEventArgs
			//		{
			//			CallerType = "SQLSERVER" ,
			//			CallerDb = Caller ,
			//			DataSource = BankCollection ,
			//			RowCount = BankCollection . Count
			//		} );
			//}
			return BankCollection;
		}

		#endregion Bank loading methods


		#region Customer Db Data Loading methods

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
		public async static Task<bool> GetCustObsCollectionAsync ( ObservableCollection<CustomerViewModel> collection ,
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
			string SqlCommand = "" ,
			string DbNameToLoad = "" ,
			string Orderby = "" ,
			string Conditions = "" ,
			bool wantSort = false ,
			bool wantDictionary = false ,
			bool Notify = false ,
			string Caller = "" ,
			int [ ] args = null )

		{
			ObservableCollection<CustomerViewModel> cvmcollection = new ObservableCollection<CustomerViewModel> ( );
			IEnumerable<CustomerViewModel> cvm;
			string[] ValidFields =
							{
								"ID",
								"CUSTNO",
								"BANKNO",
								"ACTYPE",
								"FNAME" ,
								"LNAME" ,
								"ADDR1" ,
								"ADDR2" ,
								"TOWN" ,
								"COUNTY",
								"PCODE" ,
								"PHONE" ,
								"MOBILE",
								"ODATE" ,
								"CDATE"
								};
			string[] errorcolumns;
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			if ( DbNameToLoad == "" )
				DbNameToLoad = "Customer";


			// Utility Support Methods to validate data
			if ( ValidateSortConditionColumns ( ValidFields , "Bank" , Orderby , Conditions , out errorcolumns ) == false )
			{
				if ( Orderby . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Column name has been \nidentified in the Sorting Clause provided.\n\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}.\n\nTherefore No Sort will be performed for this Db" );
					Orderby = "";
				}
				else if ( Conditions . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Match clause or Column Name \nhas been identified in the Data Selection Clause.\n\nThe Invalid item identified was :\n\t{errorcolumns [ 0 ]}\n\nTherefore No Data Matching will be performed for this Db" );
					Conditions = "";
				}
				else
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but the Loading of the BankAccount Db is being aborted due to \na non existent Column name.\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}" );
					return false;
				}
			}

			//// make sure order by clause is correctly formatted
			//if ( Orderby . Trim ( ) != "" )
			//{
			//	if ( Orderby . ToUpper ( ) . Contains ( "ORDER BY " ) == false )
			//	{
			//		Orderby = " Order by " + Orderby;
			//	}
			//}

			//if ( Conditions != "" )
			//{
			//	if ( Conditions . ToUpper ( ) . Contains ( "WHERE" ) == false )
			//		Conditions = " Where " + Conditions;
			//}
			if ( Flags . GETMULTIACCOUNTS )
			{

			}
			if ( Flags . USEADOWITHSTOREDPROCEDURES )
			{
				//====================================================
				// Use standard ADO.Net to to load Bank data to run Stored Procedure
				//====================================================
				CustomerViewModel cvmi = new CustomerViewModel ( );
				//string Con = Flags . CurrentConnectionString;
				string Con = ( string ) Wpfmain . Properties . Settings . Default["BankSysConnectionString"];

				SqlConnection sqlCon = null;
				Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
				Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;

				// Works with default command 31/10/21
				// works with Records limited 31/10/21
				// works with Selection conditions limited 31/10/21
				// works with Sort conditions 31/10/21
				try
				{
					using ( sqlCon = new SqlConnection ( Con ) )
					{
						SqlCommand sql_cmnd;
						sqlCon . Open ( );
						if ( SqlCommand != "" )
							sql_cmnd = new SqlCommand ( SqlCommand , sqlCon );
						else
						{
							sql_cmnd = new SqlCommand ( "dbo.spLoadCustomersComplex " , sqlCon );
							sql_cmnd . CommandType = CommandType . StoredProcedure;
							sql_cmnd . Parameters . AddWithValue ( "@Arg1" , SqlDbType . NVarChar ) . Value = DbNameToLoad;
							if ( args == null )
								args = dummyargs;
							if ( args . Length > 0 )
							{
								if ( args [ 2 ] > 0 )
								{
									string limits = $" Top ({args[2]}) ";
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = limits;
								}
								else
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";
							}
							else
								sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";

							sql_cmnd . Parameters . AddWithValue ( "@Arg3" , SqlDbType . NVarChar ) . Value = Conditions;
							sql_cmnd . Parameters . AddWithValue ( "@Arg4" , SqlDbType . NVarChar ) . Value = Orderby;
						}
						// Handle  max records, if any
						var sqlDr = sql_cmnd . ExecuteReader ( );
						while ( sqlDr . Read ( ) )
						{
							cvmi . Id = int . Parse ( sqlDr [ "ID" ] . ToString ( ) );
							cvmi . CustNo = sqlDr [ "CUSTNO" ] . ToString ( );
							cvmi . BankNo = sqlDr [ "BANKNO" ] . ToString ( );
							cvmi . AcType = int . Parse ( sqlDr [ "ACTYPE" ] . ToString ( ) );
							cvmi . FName = sqlDr [ "FNAME" ] . ToString ( );
							cvmi . LName = sqlDr [ "LNAME" ] . ToString ( );
							cvmi . Addr1 = sqlDr [ "ADDR1" ] . ToString ( );
							cvmi . Addr2 = sqlDr [ "ADDR2" ] . ToString ( );
							cvmi . Town = sqlDr [ "TOWN" ] . ToString ( );
							cvmi . County = sqlDr [ "COUNTY" ] . ToString ( );
							cvmi . PCode = sqlDr [ "PCODE" ] . ToString ( );
							cvmi . Phone = sqlDr [ "PHONE" ] . ToString ( );
							cvmi . Mobile = sqlDr [ "MOBILE" ] . ToString ( );
							cvmi . ODate = DateTime . Parse ( sqlDr [ "ODATE" ] . ToString ( ) );
							cvmi . CDate = DateTime . Parse ( sqlDr [ "CDATE" ] . ToString ( ) );
							cvmcollection . Add ( cvmi );
							cvmi = new CustomerViewModel ( );
						}
						sqlDr . Close ( );
						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {cvmcollection . Count} records successfuly" );
					}
					sqlCon . Close ( );
				}
				catch ( Exception ex )

				{
					Debug . WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
					return false;
				}
			}
			else
			{
				Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
				Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					if ( Flags . USEDAPPERWITHSTOREDPROCEDURE )
					{
						try
						{
							var Args = new { Arg1 = "" , Arg2 = " " , Arg3 = "" , Arg4 = "" };
							SqlCommand = $"spLoadCustomersComplex";
							if ( args [ 2 ] == 0 )
								Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
							else if ( args [ 2 ] > 0 )
								Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
							if ( SqlCommand == "" )
							{
								if ( args [ 2 ] == 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								else if ( args [ 2 ] > 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								// This syntax WORKS CORRECTLY
							}
							var result = db . Query<CustomerViewModel> ( SqlCommand , Args , null , false , null , CommandType . StoredProcedure ) . ToList ( );

							Debug . WriteLine ( result );
							foreach ( var item in result )
							{
								cvmcollection . Add ( item );
							}
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {result . Count} records successfuly" );
						}
						catch ( Exception ex )
						{
							Debug . WriteLine ( $"CUSTOMER DB ERROR : {ex . Message}" );
						}
					}
					else if ( Flags . USESDAPPERSTDPROCEDURES == true )
					{
						try
						{

							if ( Conditions != "" )
							{
								if ( args [ 2 ] > 0 && Orderby != "" )
									SqlCommand = $" Select top ({args [ 2 ]}) * from {DbNameToLoad} where {Conditions} Order by {Orderby}";
								else
									SqlCommand = $" Select * from {DbNameToLoad} where {Conditions}";
							}
							else
							{
								if ( args != null )
								{
									if ( Conditions == "" && Orderby == "" && args [ 0 ] == 0 && args [ 1 ] == 0 && args [ 2 ] == 0 )   // we dont even  have args for total records
										SqlCommand = $" Select * from {DbNameToLoad} ";
									else if ( args [ 0 ] != 0 || args [ 1 ] != 0 || args [ 2 ] != 0 )   // we do have args for total records
									{
										if ( args [ 2 ] == 0 )       // no limit on how many records to get
										{
											SqlCommand = $" Select * from {DbNameToLoad} ";
											if ( Conditions != "" )
												SqlCommand += $" {Conditions} ";
											else if ( args [ 1 ] != 0 )
												SqlCommand += $" where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]} ";
										}
										else if ( args [ 2 ] > 0 && args [ 1 ] == 0 )
											SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} ";
										else if ( args [ 1 ] > 0 )// All 3 args are received
											SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
										else
											SqlCommand = $" Select * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
									}
								}
								if ( Conditions != "" )  // We have conditions
									SqlCommand = $"Select * from {DbNameToLoad} {Conditions} ";
								else if ( args == null || args . Length == 0 )    // No args or conditions, so use generic command
									SqlCommand = $"Select * from {DbNameToLoad}  ";

								// Final Trap to ensure we have a valid command line
								if ( SqlCommand == "" )
									SqlCommand = $" Select * from {DbNameToLoad} ";

								if ( wantSort && Orderby != "" )

									SqlCommand += $" order by  {Orderby}";
							}

							cvm = db . Query<CustomerViewModel> ( SqlCommand );

							foreach ( var item in cvm )
							{
								cvmcollection . Add ( item );
							}

						}
						catch ( Exception ex )
						{
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  error : {ex . Message}, {ex . Data}" );
							return false;
						}
						finally
						{
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad} loaded : {cvmcollection . Count} records successfuly" );
						}
					}
				}
			}
			Application . Current . Dispatcher . Invoke ( ( ) =>
				EventControl . TriggerCustDataLoaded ( null ,
				new LoadedEventArgs
				{
					CallerType = "SQLSERVER" ,
					CallerDb = Caller ,
					DataSource = cvmcollection ,
					RowCount = cvmcollection . Count
				} )
				);
			return true;
		}

		public static ObservableCollection<CustomerViewModel> GetCustObsCollection ( ObservableCollection<CustomerViewModel> collection ,
			string SqlCommand = "" ,
			string DbNameToLoad = "" ,
			string Orderby = "" ,
			string Conditions = "" ,
			bool wantSort = false ,
			bool wantDictionary = false ,
			bool Notify = false ,
			string Caller = "" ,
			int [ ] args = null )

		{
			ObservableCollection<CustomerViewModel> cvmcollection = new ObservableCollection<CustomerViewModel> ( );
			IEnumerable<CustomerViewModel> cvm;
			string[] ValidFields =
							{
								"ID",
								"CUSTNO",
								"BANKNO",
								"ACTYPE",
								"FNAME" ,
								"LNAME" ,
								"ADDR1" ,
								"ADDR2" ,
								"TOWN" ,
								"COUNTY",
								"PCODE" ,
								"PHONE" ,
								"MOBILE",
								"ODATE" ,
								"CDATE"
								};
			string[] errorcolumns;
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			if ( DbNameToLoad == "" )
				DbNameToLoad = "Customer";


			// Utility Support Methods to validate data
			if ( ValidateSortConditionColumns ( ValidFields , "Bank" , Orderby , Conditions , out errorcolumns ) == false )
			{
				if ( Orderby . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Column name has been \nidentified in the Sorting Clause provided.\n\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}.\n\nTherefore No Sort will be performed for this Db" );
					Orderby = "";
				}
				else if ( Conditions . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Match clause or Column Name \nhas been identified in the Data Selection Clause.\n\nThe Invalid item identified was :\n\t{errorcolumns [ 0 ]}\n\nTherefore No Data Matching will be performed for this Db" );
					Conditions = "";
				}
				else
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but the Loading of the BankAccount Db is being aborted due to \na non existent Column name.\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}" );
					return null;
				}
			}

			//// make sure order by clause is correctly formatted
			//if ( Orderby . Trim ( ) != "" )
			//{
			//	if ( Orderby . ToUpper ( ) . Contains ( "ORDER BY " ) == false )
			//	{
			//		Orderby = " Order by " + Orderby;
			//	}
			//}

			//if ( Conditions != "" )
			//{
			//	if ( Conditions . ToUpper ( ) . Contains ( "WHERE" ) == false )
			//		Conditions = " Where " + Conditions;
			//}
			if ( Flags . GETMULTIACCOUNTS )
			{

			}
			if ( Flags . USEADOWITHSTOREDPROCEDURES )
			{
				//====================================================
				// Use standard ADO.Net to to load Bank data to run Stored Procedure
				//====================================================
				CustomerViewModel cvmi = new CustomerViewModel ( );
				string Con = Flags . CurrentConnectionString;
				//						string Con = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				SqlConnection sqlCon = null;
				Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
				Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;

				// Works with default command 31/10/21
				// works with Records limited 31/10/21
				// works with Selection conditions limited 31/10/21
				// works with Sort conditions 31/10/21
				try
				{
					using ( sqlCon = new SqlConnection ( Con ) )
					{
						SqlCommand sql_cmnd;
						sqlCon . Open ( );
						if ( SqlCommand != "" )
							sql_cmnd = new SqlCommand ( SqlCommand , sqlCon );
						else
						{
							sql_cmnd = new SqlCommand ( "dbo.spLoadCustomersComplex " , sqlCon );
							sql_cmnd . CommandType = CommandType . StoredProcedure;
							sql_cmnd . Parameters . AddWithValue ( "@Arg1" , SqlDbType . NVarChar ) . Value = DbNameToLoad;
							if ( args == null )
								args = dummyargs;
							if ( args . Length > 0 )
							{
								if ( args [ 2 ] > 0 )
								{
									string limits = $" Top ({args[2]}) ";
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = limits;
								}
								//else
								//	sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";
							}
							//else
							//	sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";
							sql_cmnd . Parameters . AddWithValue ( "@Arg3" , SqlDbType . NVarChar ) . Value = Conditions;
							sql_cmnd . Parameters . AddWithValue ( "@Arg4" , SqlDbType . NVarChar ) . Value = Orderby;
						}
						// Handle  max records, if any
						var sqlDr = sql_cmnd . ExecuteReader ( );
						while ( sqlDr . Read ( ) )
						{
							cvmi . Id = int . Parse ( sqlDr [ "ID" ] . ToString ( ) );
							cvmi . CustNo = sqlDr [ "CUSTNO" ] . ToString ( );
							cvmi . BankNo = sqlDr [ "BANKNO" ] . ToString ( );
							cvmi . AcType = int . Parse ( sqlDr [ "ACTYPE" ] . ToString ( ) );
							cvmi . FName = sqlDr [ "FNAME" ] . ToString ( );
							cvmi . LName = sqlDr [ "LNAME" ] . ToString ( );
							cvmi . Addr1 = sqlDr [ "ADDR1" ] . ToString ( );
							cvmi . Addr2 = sqlDr [ "ADDR2" ] . ToString ( );
							cvmi . Town = sqlDr [ "TOWN" ] . ToString ( );
							cvmi . County = sqlDr [ "COUNTY" ] . ToString ( );
							cvmi . PCode = sqlDr [ "PCODE" ] . ToString ( );
							cvmi . Phone = sqlDr [ "PHONE" ] . ToString ( );
							cvmi . Mobile = sqlDr [ "MOBILE" ] . ToString ( );
							cvmi . ODate = DateTime . Parse ( sqlDr [ "ODATE" ] . ToString ( ) );
							cvmi . CDate = DateTime . Parse ( sqlDr [ "CDATE" ] . ToString ( ) );
							cvmcollection . Add ( cvmi );
							cvmi = new CustomerViewModel ( );
						}
						sqlDr . Close ( );
						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {cvmcollection . Count} records successfuly" );
					}
					sqlCon . Close ( );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
				}
			}
			else
			{
				Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
				Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					if ( Flags . USEDAPPERWITHSTOREDPROCEDURE )
					{
						try
						{
							//var Args = new { DbName = "" , Arg = " " , Conditions = "" , SortBy = "" };
							//List<CustomerViewModel>  result = new List<CustomerViewModel>();
							//if ( SqlCommand == "" )
							//{
							//	SqlCommand = $"spLoadCustomersComplex";
							//	if ( args [ 2 ] == 0 )
							//		Args = new { DbName = $"{DbNameToLoad}" , Arg = $" * " , Conditions = $"{Conditions}" , SortBy = $"{ Orderby}" };
							//	else if ( args [ 2 ] > 0 )
							//		Args = new { DbName = $"{DbNameToLoad}" , Arg = $"Top ({args [ 2 ] . ToString ( )}) * " , Conditions = $"{Conditions}" , SortBy = $"{Orderby}" };
							//	// This syntax WORKS CORRECTLY
							//}
							var Args = new { Arg1 = "" , Arg2 = " " , Arg3 = "" , Arg4 = "" };
							//List<CustomerViewModel>  result = new List<CustomerViewModel>();
							if ( SqlCommand == "" )
							{
								SqlCommand = $"spLoadCustomersComplex";
								if ( args [ 2 ] == 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								else if ( args [ 2 ] > 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $" {Orderby}" };
								if ( SqlCommand == "" )
								{
									if ( args [ 2 ] == 0 )
										Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
									else if ( args [ 2 ] > 0 )
										Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
									// This syntax WORKS CORRECTLY
								}
							}

							var result = db . Query<CustomerViewModel> ( SqlCommand , Args , null , false , null , CommandType . StoredProcedure ) . ToList ( );

							Debug . WriteLine ( result );
							foreach ( var item in result )
							{
								cvmcollection . Add ( item );
							}
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {result . Count} records successfuly" );
						}
						catch ( Exception ex )
						{
							Debug . WriteLine ( $"CUSTOMER DB ERROR : {ex . Message}" );
						}
					}
					else if ( Flags . USESDAPPERSTDPROCEDURES == true )
					{
						try
						{
							if ( SqlCommand == "" )
							{
								if ( Conditions != "" )
								{
									if ( args [ 2 ] > 0 && Orderby != "" )
										SqlCommand = $" Select top ({args [ 2 ]}) * from {DbNameToLoad}  where {Conditions} Order by {Orderby}";
									else
										SqlCommand = $" Select * from {DbNameToLoad} where {Conditions}";
								}
								else
								{
									if ( args != null )
									{
										if ( Conditions == "" && Orderby == "" && args [ 0 ] == 0 && args [ 1 ] == 0 && args [ 2 ] == 0 )   // we dont even  have args for total records
											SqlCommand = $" Select * from {DbNameToLoad} ";
										else if ( args [ 0 ] != 0 || args [ 1 ] != 0 || args [ 2 ] != 0 )   // we do have args for total records
										{
											if ( args [ 2 ] == 0 )       // no limit on how many records to get
											{
												SqlCommand = $" Select * from {DbNameToLoad} ";
												if ( Conditions != "" )
													SqlCommand += $" {Conditions} ";
												else if ( args [ 1 ] != 0 )
													SqlCommand += $" where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]} ";
											}
											else if ( args [ 2 ] > 0 && args [ 1 ] == 0 )
												SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} ";
											else if ( args [ 1 ] > 0 )// All 3 args are received
												SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
											else
												SqlCommand = $" Select * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
										}
									}
									if ( Conditions != "" )  // We have conditions
										SqlCommand = $"Select * from {DbNameToLoad} {Conditions} ";
									else if ( args == null || args . Length == 0 )    // No args or conditions, so use generic command
										SqlCommand = $"Select * from {DbNameToLoad}  ";

									// Final Trap to ensure we have a valid command line
									if ( SqlCommand == "" )
										SqlCommand = $" Select * from {DbNameToLoad} ";

									if ( wantSort && Orderby != "" )

										SqlCommand += $"Order by {Orderby}";
								}
							}

							cvm = db . Query<CustomerViewModel> ( SqlCommand );

							foreach ( var item in cvm )
							{
								cvmcollection . Add ( item );
							}

						}
						catch ( Exception ex )
						{
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  error : {ex . Message}, {ex . Data}" );
						}
						finally
						{
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad} loaded : {cvmcollection . Count} records successfuly" );
						}
					}
				}
			}
			if ( Notify )
			{
				Application . Current . Dispatcher . Invoke ( ( ) =>
					EventControl . TriggerCustDataLoaded ( null ,
					new LoadedEventArgs
					{
						CallerType = "SQLSERVER" ,
						CallerDb = Caller ,
						DataSource = cvmcollection ,
						RowCount = cvmcollection . Count
					} )
					);
			}
			return cvmcollection;
		}

		#endregion  Customer Db Loading  Methods

		#region Load Customer Db with Dictionary etc (MultiviewViewer only)
		public static ObservableCollection<CustomerViewModel> GetCustObsCollectionWithDict ( ObservableCollection<CustomerViewModel> collection ,
			out Dictionary<int , int> Dict ,
			bool wantDictionary = false ,
			string SqlCommand = "" ,
			string DbNameToLoad = "" ,
			bool wantSort = false ,
			string Caller = "" ,
			bool NotifyCaller = false ,
			int [ ] args = null )

		{
			object Bankcollection = new object ( );
			ObservableCollection<CustomerViewModel> DbData = new ObservableCollection<CustomerViewModel> ( );
			int counter = 0;
			//			if ( collection == null )
			collection = DbData;
			Dictionary<int , int> CustDict = new Dictionary<int , int> ( );
			Dict = CustDict;
			List<CustomerViewModel> cvmlist = new List<CustomerViewModel> ( );
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			if ( DbNameToLoad == "" )
				DbNameToLoad = "Customer";

			using ( IDbConnection db = new SqlConnection ( ConString ) )
			{
				try
				{
					if ( SqlCommand == "" && args != null )   // no command line received, but we do have args
					{
						if ( args [ 2 ] == 0 )       // no limit on how many records to get
							SqlCommand = $" Select * from {DbNameToLoad}  where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
						else  // All 3 args are received
							SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
					}
					else if ( SqlCommand == "" && args == null )    // No inforeceived at all, so use generic command
						SqlCommand = $"Select * from {DbNameToLoad} ";

					if ( wantSort && SqlCommand . Contains ( "order by" ) == false )
						SqlCommand += $" order by CustNo, BankNo";

					cvmlist = db . Query<CustomerViewModel> ( SqlCommand ) . ToList ( );

					if ( cvmlist . Count > 0 )
					{
						foreach ( var item in cvmlist )
						{
							DbData . Add ( item );
							if ( CustDict . ContainsKey ( int . Parse ( item . CustNo ) ) == false )
								CustDict . Add ( int . Parse ( item . CustNo ) , counter++ );
						}
						collection = DbData;
					}
					//					Debug. WriteLine ( $"SQL DAPPER has loaded : {cvmcollection . Count} Customer  Records" );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  error : {ex . Message}, {ex . Data}" );
					return null;
				}
				finally
				{
					Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad} loaded : {DbData . Count} records successfuly" );
				}
			}
			// ensure we have the data in obscollection
			collection = DbData;

			if ( NotifyCaller )
			{
				Application . Current . Dispatcher . Invoke ( ( ) =>
			EventControl . TriggerCustDataLoaded ( null ,
					new LoadedEventArgs
					{
						CallerType = "SQLSERVER" ,
						CallerDb = Caller ,
						DataSource = collection ,
						RowCount = collection . Count
					} )
			);
			}
			return collection;
		}
		#endregion Load Cusmoer Db with Dictionary etc (MultiviewViewer only)


		#region Details Db Data Loading methods

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
		public async static Task<bool> GetDetailsObsCollectionAsync ( ObservableCollection<DetailsViewModel> collection ,
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
			string SqlCommand = "" ,
			string DbNameToLoad = "" ,
			string Orderby = "" ,
			string Conditions = "" ,
			bool wantSort = false ,
			bool wantDictionary = false ,
			bool Notify = false ,
			string Caller = "" ,
			int [ ] args = null )
		{
			IEnumerable<DetailsViewModel> dvm;
			ObservableCollection<DetailsViewModel> dvmcollection = new ObservableCollection<DetailsViewModel> ( );
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			string[] ValidFields =
							{
								"ID",
								"CUSTNO",
								"BANKNO",
								"ACTYPE",
								"INTRATE" ,
								"BALANCE" ,
								"ODATE" ,
								"CDATE"
								};
			string[] errorcolumns;

			// Use defaullt Db if none received frm caller
			if ( DbNameToLoad == "" )
				DbNameToLoad = "BankAccount";


			// Utility Support Methods to validate data
			if ( ValidateSortConditionColumns ( ValidFields , "Bank" , Orderby , Conditions , out errorcolumns ) == false )
			{
				if ( Orderby . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Column name has been \nidentified in the Sorting Clause provided.\n\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}.\n\nTherefore No Sort will be performed for this Db" );
					Orderby = "";
				}
				else if ( Conditions . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Match clause or Column Name \nhas been identified in the Data Selection Clause.\n\nThe Invalid item identified was :\n\t{errorcolumns [ 0 ]}\n\nTherefore No Data Matching will be performed for this Db" );
					Conditions = "";
				}
				else
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but the Loading of the BankAccount Db is being aborted due to \na non existent Column name.\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}" );
					return false;
				}
			}

			//// make sure order by clause is correctly formatted
			//if ( Orderby . Trim ( ) != "" )
			//{
			//	if ( Orderby . ToUpper ( ) . Contains ( "ORDER BY " ) == false )
			//	{
			//		Orderby = " Order by " + Orderby;
			//	}
			//}

			//if ( Conditions != "" )
			//{
			//	if ( Conditions . ToUpper ( ) . Contains ( "WHERE" ) == false )
			//		Conditions = " Where " + Conditions;
			//}
			if ( DbNameToLoad == "" )
				DbNameToLoad = "SecAccounts";

			if ( Flags . USEADOWITHSTOREDPROCEDURES )
			{
				Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
				Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
				//====================================================
				// Use standard ADO.Net to to load Bank data to run Stored Procedure
				//====================================================
				DetailsViewModel dvmi = new DetailsViewModel ( );
				string Con = Flags . CurrentConnectionString;
				//						string Con = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				SqlConnection sqlCon = null;

				// Works with default command 31/10/21
				// works with Records limited 31/10/21
				// works with Selection conditions limited 31/10/21
				// works with Sort conditions 31/10/21
				try
				{
					using ( sqlCon = new SqlConnection ( Con ) )
					{
						SqlCommand sql_cmnd;
						sqlCon . Open ( );
						if ( SqlCommand != "" )
							sql_cmnd = new SqlCommand ( SqlCommand , sqlCon );
						else
						{
							sql_cmnd = new SqlCommand ( "dbo.spLoadDetailsComplex " , sqlCon );
							sql_cmnd . CommandType = CommandType . StoredProcedure;
							sql_cmnd . Parameters . AddWithValue ( "@Arg1" , SqlDbType . NVarChar ) . Value = DbNameToLoad;
							if ( args == null )
								args = dummyargs;
							if ( args . Length > 0 )
							{
								if ( args [ 2 ] > 0 )
								{
									string limits = $" Top ({args[2]}) ";
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = limits;
								}
								else
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";
							}
							else
								sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";

							sql_cmnd . Parameters . AddWithValue ( "@Arg3" , SqlDbType . NVarChar ) . Value = Conditions;
							sql_cmnd . Parameters . AddWithValue ( "@Arg4" , SqlDbType . NVarChar ) . Value = Orderby;
						}
						// Handle  max records, if any
						var sqlDr = sql_cmnd . ExecuteReader ( );
						while ( sqlDr . Read ( ) )
						{
							dvmi . Id = int . Parse ( sqlDr [ "ID" ] . ToString ( ) );
							dvmi . CustNo = sqlDr [ "CustNo" ] . ToString ( );
							dvmi . BankNo = sqlDr [ "BankNo" ] . ToString ( );
							dvmi . AcType = int . Parse ( sqlDr [ "ACTYPE" ] . ToString ( ) );
							dvmi . Balance = Decimal . Parse ( sqlDr [ "BALANCE" ] . ToString ( ) );
							dvmi . IntRate = Decimal . Parse ( sqlDr [ "INTRATE" ] . ToString ( ) );
							dvmi . ODate = DateTime . Parse ( sqlDr [ "ODATE" ] . ToString ( ) );
							dvmi . CDate = DateTime . Parse ( sqlDr [ "CDATE" ] . ToString ( ) );
							dvmcollection . Add ( dvmi );
							dvmi = new DetailsViewModel ( );
						}
						sqlDr . Close ( );
						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {dvmcollection . Count} records successfuly" );
					}
					sqlCon . Close ( );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
					return false;
				}
			}
			else
			{
				Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
				Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					if ( Flags . USEDAPPERWITHSTOREDPROCEDURE )
					{
						try
						{
							//var Args = new { DbName = "" , Arg = " " , Conditions = "" , SortBy = "" };
							//if ( SqlCommand == "" )
							//{
							//	SqlCommand = $"spLoadDetailsComplex";
							//	if ( args [ 2 ] == 0 )
							//		Args = new { DbName = $"{DbNameToLoad}" , Arg = $" * " , Conditions = $"{Conditions}" , SortBy = $"{ Orderby}" };
							//	else if ( args [ 2 ] > 0 )
							//		Args = new { DbName = $"{DbNameToLoad}" , Arg = $"Top ({args [ 2 ] . ToString ( )}) * " , Conditions = $"{Conditions}" , SortBy = $"{Orderby}" };
							//	// This syntax WORKS CORRECTLY
							//}
							//							List<DetailsViewModel>  result = new List<DetailsViewModel>();
							var Args = new { Arg1 = "" , Arg2 = " " , Arg3 = "" , Arg4 = "" };
							SqlCommand = $"spLoadDetailsComplex";
							if ( args [ 2 ] == 0 )
								Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
							else if ( args [ 2 ] > 0 )
								Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
							if ( SqlCommand == "" )
							{
								if ( args [ 2 ] == 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								else if ( args [ 2 ] > 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								// This syntax WORKS CORRECTLY
							}
							var result = db . Query<DetailsViewModel> ( SqlCommand , Args , null , false , null , CommandType . StoredProcedure ) . ToList ( );

							Debug . WriteLine ( result );
							foreach ( var item in result )
							{
								dvmcollection . Add ( item );
							}
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {result . Count} records successfuly" );
						}
						catch ( Exception ex )
						{
							Debug . WriteLine ( $"DETAILS DB ERROR : {ex . Message}" );
						}
					}
					else if ( Flags . USESDAPPERSTDPROCEDURES == true )
					{

						try
						{
							if ( SqlCommand == "" )
							{
								if ( Conditions != "" )
								{
									if ( args [ 2 ] > 0 && Orderby != "" )
										SqlCommand = $" Select top ({args [ 2 ]}) * from {DbNameToLoad} where  {Conditions}  Order by  {Orderby}";
									else
										SqlCommand = $" Select * from {DbNameToLoad} where {Conditions}";
								}
								else
								{
									if ( args != null )
									{
										if ( Conditions == "" && Orderby == "" && args [ 0 ] == 0 && args [ 1 ] == 0 && args [ 2 ] == 0 )   // we dont even  have args for total records
											SqlCommand = $" Select * from {DbNameToLoad} ";
										else if ( args [ 0 ] != 0 || args [ 1 ] != 0 || args [ 2 ] != 0 )   // we do have args for total records
										{
											if ( args [ 2 ] == 0 )       // no limit on how many records to get
											{
												SqlCommand = $" Select * from {DbNameToLoad} ";
												if ( Conditions != "" )
													SqlCommand += $" {Conditions} ";
												else if ( args [ 1 ] != 0 )
													SqlCommand += $" where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]} ";
											}
											else if ( args [ 2 ] > 0 && args [ 1 ] == 0 )
												SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} ";
											else if ( args [ 1 ] > 0 )// All 3 args are received
												SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
											else
												SqlCommand = $" Select * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
										}
									}
									if ( Conditions != "" )  // We have conditions
										SqlCommand = $"Select * from {DbNameToLoad} {Conditions} ";
									else if ( args == null || args . Length == 0 )    // No args or conditions, so use generic command
										SqlCommand = $"Select * from {DbNameToLoad}  ";

									// Final Trap to ensure we have a valid command line
									if ( SqlCommand == "" )
										SqlCommand = $" Select * from {DbNameToLoad} ";

									if ( wantSort && Orderby != "" )
										SqlCommand += $" order by {Orderby}";
								}

								// Read data via Dapper into IEnumerable<DbType>
								dvm = db . Query<DetailsViewModel> ( SqlCommand );

								foreach ( var item in dvm )
								{
									dvmcollection . Add ( item );
								}
							}
							else
							{
								// Read data via Dapper into IEnumerable<DbType>
								dvm = db . Query<DetailsViewModel> ( SqlCommand );

								foreach ( var item in dvm )
								{
									dvmcollection . Add ( item );
								}
							}
						}
						catch ( Exception ex )
						{
							Debug . WriteLine ( $"SQL DAPPER error : {ex . Message}, {ex . Data}" );
						}
						finally
						{
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {dvmcollection . Count} records successfuly" );
						}
					}
				}
			}
			EventControl . TriggerDetDataLoaded ( null ,
					new LoadedEventArgs
					{
						CallerType = "SQLSERVER" ,
						CallerDb = Caller ,
						DataSource = dvmcollection ,
						RowCount = dvmcollection . Count
					} );
			return true;
		}
		public static ObservableCollection<DetailsViewModel> GetDetailsObsCollection ( ObservableCollection<DetailsViewModel> collection ,
			string SqlCommand = "" ,
			string DbNameToLoad = "" ,
			string Orderby = "" ,
			string Conditions = "" ,
			bool wantSort = false ,
			bool wantDictionary = false ,
			bool Notify = false ,
			string Caller = "" ,
			int [ ] args = null )
		{
			IEnumerable<DetailsViewModel> dvm;
			ObservableCollection<DetailsViewModel> dvmcollection = new ObservableCollection<DetailsViewModel> ( );
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			string[] ValidFields =
							{
								"ID",
								"CUSTNO",
								"BANKNO",
								"ACTYPE",
								"INTRATE" ,
								"BALANCE" ,
								"ODATE" ,
								"CDATE"
								};
			string[] errorcolumns;

			// Use defaullt Db if none received frm caller
			if ( DbNameToLoad == "" )
				DbNameToLoad = "BankAccount";


			// Utility Support Methods to validate data
			if ( ValidateSortConditionColumns ( ValidFields , "Bank" , Orderby , Conditions , out errorcolumns ) == false )
			{
				if ( Orderby . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Column name has been \nidentified in the Sorting Clause provided.\n\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}.\n\nTherefore No Sort will be performed for this Db" );
					Orderby = "";
				}
				else if ( Conditions . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but an invalid Match clause or Column Name \nhas been identified in the Data Selection Clause.\n\nThe Invalid item identified was :\n\t{errorcolumns [ 0 ]}\n\nTherefore No Data Matching will be performed for this Db" );
					Conditions = "";
				}
				else
				{
					MessageBox . Show ( $"BANKACCOUNT dB\nSorry, but the Loading of the BankAccount Db is being aborted due to \na non existent Column name.\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}" );
					return null;
				}
			}

			//// make sure order by clause is correctly formatted
			//if ( Orderby . Trim ( ) != "" )
			//{
			//	if ( Orderby . ToUpper ( ) . Contains ( "ORDER BY " ) == false )
			//	{
			//		Orderby = " Order by " + Orderby;
			//	}
			//}

			//if ( Conditions != "" )
			//{
			//	if ( Conditions . ToUpper ( ) . Contains ( "WHERE" ) == false )
			//		Conditions = " Where " + Conditions;
			//}
			if ( DbNameToLoad == "" )
				DbNameToLoad = "SecAccounts";

			if ( Flags . USEADOWITHSTOREDPROCEDURES )
			{
				Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
				Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
				//====================================================
				// Use standard ADO.Net to to load Bank data to run Stored Procedure
				//====================================================
				DetailsViewModel dvmi = new DetailsViewModel ( );
				string Con = Flags . CurrentConnectionString;
				//						string Con = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				SqlConnection sqlCon = null;

				// Works with default command 31/10/21
				// works with Records limited 31/10/21
				// works with Selection conditions limited 31/10/21
				// works with Sort conditions 31/10/21
				try
				{
					using ( sqlCon = new SqlConnection ( Con ) )
					{
						SqlCommand sql_cmnd;
						sqlCon . Open ( );
						if ( SqlCommand != "" )
							sql_cmnd = new SqlCommand ( SqlCommand , sqlCon );
						else
						{
							sql_cmnd = new SqlCommand ( "dbo.spLoadDetailsComplex " , sqlCon );
							sql_cmnd . CommandType = CommandType . StoredProcedure;
							sql_cmnd . Parameters . AddWithValue ( "@Arg1" , SqlDbType . NVarChar ) . Value = DbNameToLoad;
							if ( args == null )
								args = dummyargs;
							if ( args . Length > 0 )
							{
								if ( args [ 2 ] > 0 )
								{
									string limits = $" Top ({args[2]}) ";
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = limits;
								}
								else
									sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";
							}
							else
								sql_cmnd . Parameters . AddWithValue ( "@Arg2" , SqlDbType . NVarChar ) . Value = " * ";

							sql_cmnd . Parameters . AddWithValue ( "@Arg3" , SqlDbType . NVarChar ) . Value = Conditions;
							sql_cmnd . Parameters . AddWithValue ( "@Arg4" , SqlDbType . NVarChar ) . Value = Orderby;
						}
						// Handle  max records, if any
						var sqlDr = sql_cmnd . ExecuteReader ( );
						while ( sqlDr . Read ( ) )
						{
							dvmi . Id = int . Parse ( sqlDr [ "ID" ] . ToString ( ) );
							dvmi . CustNo = sqlDr [ "CustNo" ] . ToString ( );
							dvmi . BankNo = sqlDr [ "BankNo" ] . ToString ( );
							dvmi . AcType = int . Parse ( sqlDr [ "ACTYPE" ] . ToString ( ) );
							dvmi . Balance = Decimal . Parse ( sqlDr [ "BALANCE" ] . ToString ( ) );
							dvmi . IntRate = Decimal . Parse ( sqlDr [ "INTRATE" ] . ToString ( ) );
							dvmi . ODate = DateTime . Parse ( sqlDr [ "ODATE" ] . ToString ( ) );
							dvmi . CDate = DateTime . Parse ( sqlDr [ "CDATE" ] . ToString ( ) );
							dvmcollection . Add ( dvmi );
							dvmi = new DetailsViewModel ( );
						}
						sqlDr . Close ( );
						Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {dvmcollection . Count} records successfuly" );
					}
					sqlCon . Close ( );
				}
				catch ( Exception ex )

				{
					Debug . WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
				}
			}
			else
			{
				Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
				Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					if ( Flags . USEDAPPERWITHSTOREDPROCEDURE )
					{
						try
						{

							var Args = new { Arg1 = "" , Arg2 = " " , Arg3 = "" , Arg4 = "" };
							if ( SqlCommand == "" )
							{
								SqlCommand = $"spLoadDetailsComplex";
								if ( args [ 2 ] == 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								else if ( args [ 2 ] > 0 )
									Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
								if ( SqlCommand == "" )
								{
									if ( args [ 2 ] == 0 )
										Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"" , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
									else if ( args [ 2 ] > 0 )
										Args = new { Arg1 = $"{DbNameToLoad}" , Arg2 = $"Top ({args [ 2 ] . ToString ( )}) " , Arg3 = $"{Conditions}" , Arg4 = $"{Orderby}" };
									// This syntax WORKS CORRECTLY
								}
							}

							var result = db . Query<DetailsViewModel> ( SqlCommand , Args , null , false , null , CommandType . StoredProcedure ) . ToList ( );

							Debug . WriteLine ( result );
							foreach ( var item in result )
							{
								dvmcollection . Add ( item );
							}
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {result . Count} records successfuly" );
						}
						catch ( Exception ex )
						{
							Debug . WriteLine ( $"DETAILS DB ERROR : {ex . Message}" );
						}
					}
					else if ( Flags . USESDAPPERSTDPROCEDURES == true )
					{
						try
						{
							if ( SqlCommand == "" )
							{
								if ( Conditions != "" )
								{
									if ( args [ 2 ] > 0 && Orderby != "" )
										SqlCommand = $" Select top ({args [ 2 ]}) * from {DbNameToLoad} where {Conditions} Order by {Orderby}";
									else
										SqlCommand = $" Select * from {DbNameToLoad} where {Conditions}";
								}
								else
								{
									if ( args != null )
									{
										if ( Conditions == "" && Orderby == "" && args [ 0 ] == 0 && args [ 1 ] == 0 && args [ 2 ] == 0 )   // we dont even  have args for total records
											SqlCommand = $" Select * from {DbNameToLoad} ";
										else if ( args [ 0 ] != 0 || args [ 1 ] != 0 || args [ 2 ] != 0 )   // we do have args for total records
										{
											if ( args [ 2 ] == 0 )       // no limit on how many records to get
											{
												SqlCommand = $" Select * from {DbNameToLoad} ";
												if ( Conditions != "" )
													SqlCommand += $" {Conditions} ";
												else if ( args [ 1 ] != 0 )
													SqlCommand += $" where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]} ";
											}
											else if ( args [ 2 ] > 0 && args [ 1 ] == 0 )
												SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} ";
											else if ( args [ 1 ] > 0 )// All 3 args are received
												SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
											else
												SqlCommand = $" Select * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
										}
									}
									if ( Conditions != "" )  // We have conditions
										SqlCommand = $"Select * from {DbNameToLoad} {Conditions} ";
									else if ( args == null || args . Length == 0 )    // No args or conditions, so use generic command
										SqlCommand = $"Select * from {DbNameToLoad}  ";

									// Final Trap to ensure we have a valid command line
									if ( SqlCommand == "" )
										SqlCommand = $" Select * from {DbNameToLoad} ";

									if ( wantSort && Orderby != "" )

										SqlCommand += $" Order by {Orderby}";
								}
							}
							// Read data via Dapper into IEnumerable<DbType>
							dvm = db . Query<DetailsViewModel> ( SqlCommand );

							foreach ( var item in dvm )
							{
								dvmcollection . Add ( item );
							}
						}
						catch ( Exception ex )
						{
							Debug . WriteLine ( $"SQL DAPPER error : {ex . Message}, {ex . Data}" );
						}
						finally
						{
							Debug . WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {dvmcollection . Count} records successfuly" );
						}
					}
				}
			}
			if ( Notify )
			{
				EventControl . TriggerDetDataLoaded ( null ,
					new LoadedEventArgs
					{
						CallerType = "SQLSERVER" ,
						CallerDb = Caller ,
						DataSource = dvmcollection ,
						RowCount = dvmcollection . Count
					} );
			}
			//collection = dvmcollection;
			return dvmcollection;
		}

		#endregion Details Db Data Loading methods


		/// <summary>
		/// Update a complete Db (specifiable) from the datagrid passed in
		/// it also accepts a fully qualified SQLCommand string if required....
		/// </summary>
		/// <param name="dgrid">DataGrid ti update Db from</param>
		/// <param name="DbName">SQL DB name</param>
		/// <param name="SqlCommand">Fully qualified command string</param>
		/// <param name="Conditions">order by params as a string.  Eg : "where Id=@ID</param>
		/// <returns></returns>
		public static bool UpdateBankDb (
			DataGrid dgrid ,
			string DbName = "" ,
			string SqlCommand = "" ,
			string Conditions = "" )
		{
			// Works very well 27/10/21
			bool result = true;
#pragma warning disable CS0219 // The variable 'indexer' is assigned but its value is never used
			int indexer = 0;
#pragma warning restore CS0219 // The variable 'indexer' is assigned but its value is never used
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			BankAccountViewModel bvm = new BankAccountViewModel ( );

			if ( DbName == "" )
				DbName = "BankAccount";

			using ( IDbConnection connection = new SqlConnection ( ConString ) )
			{
				try
				{

					for ( int x = 0 ; x < dgrid . Items . Count - 1 ; x++ )
					{
						dgrid . SelectedIndex = x;
						bvm = dgrid . SelectedItem as BankAccountViewModel;
						//This is how to save and use parameters for dapper
						var parameters = new
						{
							id = bvm . Id ,
							actype = bvm . AcType ,
							intrate = bvm . IntRate ,
							balance = bvm . Balance ,
							bankno = bvm . BankNo ,
							custno = bvm . CustNo ,
							odate = bvm . ODate ,
							cdate = bvm . CDate
						};
						SqlCommand = $" Update  {DbName} set  CustNo=@custno, BankNo =@bankno, AcType = @actype, IntRate=@intrate, Balance=@balance,ODate =@odate, CDate = @cdate where Id=@Id";
						if ( Conditions != "" )
							SqlCommand += Conditions;
						connection . Execute ( @SqlCommand , parameters );
					}

				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbName} Update error : {ex . Message}, {ex . Data}" );
					result = false;
				}
				finally
				{
					if ( result )
						Debug . WriteLine ( $"SQL [{DbName . ToUpper ( )}] Db Updated using DAPPER successfuly" );
				}
			}
			return result;
		}
		public static bool UpdateSingleBankDb (
			BankAccountViewModel bvm ,
			string DbName = "" ,
			string Conditions = "" )
		{
			// Works very well 27/10/21
			string SqlCommand = "";
			bool result = true;
#pragma warning disable CS0219 // The variable 'indexer' is assigned but its value is never used
			int indexer = 0;
#pragma warning restore CS0219 // The variable 'indexer' is assigned but its value is never used
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			if ( DbName == "" )
				DbName = "BankAccount";

			using ( IDbConnection connection = new SqlConnection ( ConString ) )
			{
				try
				{
					//This is how to save and use parameters for dapper
					var parameters =
								new
								{
									id = bvm . Id ,
									actype = bvm . AcType ,
									intrate = bvm . IntRate ,
									balance = bvm . Balance ,
									bankno = bvm . BankNo ,
									custno = bvm . CustNo ,
									odate = bvm . ODate ,
									cdate = bvm . CDate
								};
					SqlCommand = $" Update  {DbName} set  CustNo=@custno, BankNo =@bankno, AcType = @actype, IntRate=@intrate, Balance=@balance,ODate =@odate, CDate = @cdate ";
					if ( Conditions != "" )
						SqlCommand += Conditions;
					else
						SqlCommand += " where Id =@Id";

					connection . Execute ( @SqlCommand , parameters );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbName} Update error : {ex . Message}, {ex . Data}" );
					result = false;
				}
				finally
				{
					if ( result )
						Debug . WriteLine ( $"SQL [{DbName . ToUpper ( )}] Db record Updated using DAPPER successfuly" );
				}
			}
			return result;
		}


		public static bool UpdateCustomersDb (
			DataGrid dgrid ,
			string DbName = "" ,
			string Conditions = "" )
		{
			// Works very well 27/10/21
			string SqlCommand = "";
			bool result = true;
#pragma warning disable CS0219 // The variable 'indexer' is assigned but its value is never used
			int indexer = 0;
#pragma warning restore CS0219 // The variable 'indexer' is assigned but its value is never used
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			CustomerViewModel cvm = new CustomerViewModel ( );

			if ( DbName == "" )
				DbName = "Customer";

			using ( IDbConnection connection = new SqlConnection ( ConString ) )
			{
				try
				{
					for ( int x = 0 ; x < dgrid . Items . Count - 1 ; x++ )
					{
						dgrid . SelectedIndex = x;
						cvm = dgrid . SelectedItem as CustomerViewModel;
						var parameters = new
						{
							id = cvm . Id ,
							custno = cvm . CustNo ,
							bankno = cvm . BankNo ,
							actype = cvm . AcType ,
							fname = cvm . FName ,
							lname = cvm . LName ,
							addr1 = cvm . Addr1 ,
							addr2 = cvm . Addr2 ,
							town = cvm . Town ,
							county = cvm . County ,
							pcode = cvm . PCode ,
							phone = cvm . Phone ,
							mobile = cvm . Mobile ,
							dob = cvm . Dob ,
							odate = cvm . ODate ,
							cdate = cvm . CDate
						};
						SqlCommand = $" Update  {DbName} set  CustNo=@custno, BankNo =@bankno, AcType = @actype, FName=@fname, LName=@lname, " +
							$" Addr1=@addr1, Addr2=@addr2, Town=@town, County=@County, PCode=@pcode, Phone=@phone, Mobile=@mobile, Dob=@dob, " +
							$"ODate =@odate, CDate = @cdate where Id=@Id ";
						if ( Conditions != "" )
							SqlCommand += Conditions;
						connection . Execute ( @SqlCommand , parameters );
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbName} Update error : {ex . Message}, {ex . Data}" );
					result = false;
				}
				finally
				{
					if ( result )
						Debug . WriteLine ( $"SQL [{DbName . ToUpper ( )}] Db Updated using DAPPER successfuly" );
				}
			}
			return result;
		}

		public static bool SaveGenericDb (
			GenericClass gclass ,
			string DbName = "" )
		{
			// Works very well 27/10/21

			//CAUTION THIS ONLY WORKS FOR TABLES WITH FIELD1 - FIELD20 STRUCTURE (GENERIC TABLE)
			string SqlCommand = "";
			bool result = true;
#pragma warning disable CS0219 // The variable 'indexer' is assigned but its value is never used
			int indexer = 0;
#pragma warning restore CS0219 // The variable 'indexer' is assigned but its value is never used
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			CustomerViewModel cvm = new CustomerViewModel ( );

			if ( DbName == "" )
				DbName = "GenericTable";

			using ( IDbConnection connection = new SqlConnection ( ConString ) )
			{
				try
				{
					string fld1 = gclass . field1;
					string fld2 = gclass . field2;
					string fld3 = gclass . field3;
					SqlCommand = $" Insert into  {DbName} (FIELD1, FIELD2, FIELD3, FIELD4, FIELD5, FIELD6, FIELD7, FIELD8, FIELD9, FIELD10, FIELD11, FIELD12, FIELD13, FIELD14, FIELD15, FIELD16, FIELD17, FIELD18, FIELD19, FIELD20)" +
						$" VALUES ('{fld1}', '{fld2}', '{fld3}', '{gclass . field4}', '{gclass . field5}', '{gclass . field6}', '{gclass . field7}', '{gclass . field8}', '{gclass . field9}', '{gclass . field10}'," +
						$"'{gclass . field11}', '{gclass . field12}', '{gclass . field13}', '{gclass . field14}', '{gclass . field15}', '{gclass . field16}', '{gclass . field17}', '{gclass . field18}', '{gclass . field19}', '{gclass . field20}' )";

					// GO	AHEAD and INSERT new record into generic table 'GENERICxxxxx'
					connection . Execute ( @SqlCommand );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbName} Update error : {ex . Message}', {ex . Data}" );
					result = false;
				}
				finally
				{
					if ( result )
						Debug . WriteLine ( $"SQL [{DbName . ToUpper ( )}] Db Updated using DAPPER successfuly" );
				}
			}
			return result;
		}

		public static bool UpdateGenericDb (
			GenericClass gclass ,
			string DbName = "" )
		{
			// Works very well 27/10/21
			string SqlCommand = "";
			bool result = true;
#pragma warning disable CS0219 // The variable 'indexer' is assigned but its value is never used
			int indexer = 0;
#pragma warning restore CS0219 // The variable 'indexer' is assigned but its value is never used
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			CustomerViewModel cvm = new CustomerViewModel ( );

			if ( DbName == "" )
				DbName = "GenericTable";

			using ( IDbConnection connection = new SqlConnection ( ConString ) )
			{
				try
				{
					for ( int x = 0 ; x < 20 ; x++ )
					{
						var parameters = new
						{
							field1 = gclass . field1 ,
							field2 = gclass . field2 ,
							field3 = gclass . field3 ,
							field4 = gclass . field4 ,
							field5 = gclass . field5 ,
							field6 = gclass . field6 ,
							field7 = gclass . field7 ,
							field8 = gclass . field8 ,
							field9 = gclass . field9 ,
							field10 = gclass . field10 ,
							field11 = gclass . field11 ,
							field12 = gclass . field12 ,
							field13 = gclass . field13 ,
							field14 = gclass . field14 ,
							field15 = gclass . field15 ,
							field16 = gclass . field16 ,
							field17 = gclass . field17 ,
							field18 = gclass . field18 ,
							field19 = gclass . field19 ,
							field20 = gclass . field20
						};
						SqlCommand = $" Update  {DbName} set field2=@field2, field3=@field3, field4=@field4, field5=@field5, field6=@field6, field7=@field7, field8=@field8, field9=@field9, field10=@field10, " +
							$"field11=@field11, field12=@field12, field13=@field13, field14=@field14, field15=@field15, field16=@field16, field17=@field17, field18=@field18, field19=@field19, field20=@field20  where field1=@field1";
						connection . Execute ( @SqlCommand , parameters );
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbName} Update error : {ex . Message}, {ex . Data}" );
					result = false;
				}
				finally
				{
					if ( result )
						Debug . WriteLine ( $"SQL [{DbName . ToUpper ( )}] Db Updated using DAPPER successfuly" );
				}
			}
			return result;
		}

		#region   Customer record Db Update Method

		public static bool UpdateSingleCustomersDb (
			CustomerViewModel cvm ,
			string DbName = "" ,
			string Conditions = "" )
		{
			// Works very well 27/10/21
			string SqlCommand = "";
			bool result = true;
#pragma warning disable CS0219 // The variable 'indexer' is assigned but its value is never used
			int indexer = 0;
#pragma warning restore CS0219 // The variable 'indexer' is assigned but its value is never used
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			if ( DbName == "" )
				DbName = "Customer";

			using ( IDbConnection connection = new SqlConnection ( ConString ) )
			{
				try
				{
					var parameters = new
					{
						id = cvm . Id ,
						custno = cvm . CustNo ,
						bankno = cvm . BankNo ,
						actype = cvm . AcType ,
						fname = cvm . FName ,
						lname = cvm . LName ,
						addr1 = cvm . Addr1 ,
						addr2 = cvm . Addr2 ,
						town = cvm . Town ,
						county = cvm . County ,
						pcode = cvm . PCode ,
						phone = cvm . Phone ,
						mobile = cvm . Mobile ,
						dob = cvm . Dob ,
						odate = cvm . ODate ,
						cdate = cvm . CDate
					};
					SqlCommand = $" Update  {DbName} set  CustNo=@custno, BankNo =@bankno, AcType = @actype, FName=@fname, LName=@lname, " +
						$" Addr1=@addr1, Addr2=@addr2, Town=@town, County=@County, PCode=@pcode, Phone=@phone, Mobile=@mobile, Dob=@dob, " +
						$"ODate =@odate, CDate = @cdate ";
					if ( Conditions != "" )
						SqlCommand += Conditions;
					else
						SqlCommand += " where Id =@Id";
					connection . Execute ( @SqlCommand , parameters );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbName} Update error : {ex . Message}, {ex . Data}" );
					result = false;
				}
				finally
				{
					if ( result )
						Debug . WriteLine ( $"SQL [{DbName . ToUpper ( )}] Db record Updated using DAPPER successfuly" );
				}
			}
			return result;
		}
		#endregion   Customer record Db Update Method

		#region  Standard DetailsDb Update Method

		/// <summary>
		/// Update a complete Db (specifiable) from the datagrid passed in
		/// it also accepts a fully qualified SQLCommand string if required....
		/// </summary>
		/// <param name="dgrid">DataGrid ti update Db from</param>
		/// <param name="DbName">SQL DB name</param>
		/// <param name="SqlCommand">Fully qualified command string</param>
		/// <param name="Conditions">order by params as a string.  Eg : "where Id=@ID</param>
		/// <returns></returns>
		public static bool UpdateDetailsDb (
			DataGrid dgrid ,
			string DbName = "" ,
			string SqlCommand = "" ,
			string Conditions = "" )
		{
			// Works very well 27/10/21
			bool result = true;
#pragma warning disable CS0219 // The variable 'indexer' is assigned but its value is never used
			int indexer = 0;
#pragma warning restore CS0219 // The variable 'indexer' is assigned but its value is never used
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			DetailsViewModel bvm = new DetailsViewModel ( );

			if ( DbName == "" )
				DbName = "SecAccounts";

			using ( IDbConnection connection = new SqlConnection ( ConString ) )
			{
				try
				{

					for ( int x = 0 ; x < dgrid . Items . Count - 1 ; x++ )
					{
						dgrid . SelectedIndex = x;
						bvm = dgrid . SelectedItem as DetailsViewModel;
						var parameters = new
						{
							id = bvm . Id ,
							actype = bvm . AcType ,
							intrate = bvm . IntRate ,
							balance = bvm . Balance ,
							bankno = bvm . BankNo ,
							custno = bvm . CustNo ,
							odate = bvm . ODate ,
							cdate = bvm . CDate
						};
						SqlCommand = $" Update  {DbName} set  CustNo=@custno, BankNo =@bankno, AcType = @actype, IntRate=@intrate, Balance=@balance,ODate =@odate, CDate = @cdate where Id=@Id";
						if ( Conditions != "" )
							SqlCommand += Conditions;
						connection . Execute ( @SqlCommand , parameters );
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbName} Update error : {ex . Message}, {ex . Data}" );
					result = false;
				}
				finally
				{
					if ( result )
						Debug . WriteLine ( $"SQL [{DbName . ToUpper ( )}] Db Updated using DAPPER successfuly" );
				}
			}
			return result;
		}
		#endregion  Standard Details Db Update Method

		#region  Details record Db Update Method

		/// <summary>
		/// Update a complete Db (specifiable) from the datagrid passed in
		/// it also accepts a fully qualified SQLCommand string if required....
		/// </summary>
		/// <param name="dgrid">DataGrid ti update Db from</param>
		/// <param name="DbName">SQL DB name</param>
		/// <param name="SqlCommand">Fully qualified command string</param>
		/// <param name="Conditions">order by params as a string.  Eg : "where Id=@ID</param>
		/// <returns></returns>
		public static bool UpdateSingleDetailsDb (
			DetailsViewModel dvm ,
			string DbName = "" ,
			string Conditions = "" )
		{
			// Works very well 27/10/21
			string SqlCommand = "";
			bool result = true;
#pragma warning disable CS0219 // The variable 'indexer' is assigned but its value is never used
			int indexer = 0;
#pragma warning restore CS0219 // The variable 'indexer' is assigned but its value is never used
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				DapperSupport . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			if ( DbName == "" )
				DbName = "SecAccounts";

			using ( IDbConnection connection = new SqlConnection ( ConString ) )
			{
				try
				{
					var parameters = new
					{
						id = dvm . Id ,
						actype = dvm . AcType ,
						intrate = dvm . IntRate ,
						balance = dvm . Balance ,
						bankno = dvm . BankNo ,
						custno = dvm . CustNo ,
						odate = dvm . ODate ,
						cdate = dvm . CDate
					};
					SqlCommand = $" Update  {DbName} set  CustNo=@custno, BankNo =@bankno, AcType = @actype, IntRate=@intrate, Balance=@balance,ODate =@odate, CDate = @cdate  ";
					if ( Conditions != "" )
						SqlCommand += Conditions;
					else
						SqlCommand += " where Id =@Id";

					connection . Execute ( @SqlCommand , parameters );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {DbName} Update error : {ex . Message}, {ex . Data}" );
					result = false;
				}
				finally
				{
					if ( result )
						Debug . WriteLine ( $"SQL [{DbName . ToUpper ( )}] Db record updated using DAPPER successfuly" );
				}
			}
			return result;
		}
		#endregion  Details record Db Update Method

		/// <summary>
		///  This is a MASSIVE Function that handles updating the Dbs via SQL plus sorting the current grid
		///  out & notifying all other viewers that a change has occurred so they can (& in fact do) update
		///  their own data grids rather nicely right now - 22/4/21
		/// </summary>
		public static void UpdateAllDb ( string CurrentDb , string DbName , DataGrid Bankgrid , DataGrid Custgrid , DataGrid Detgrid )
		{
			/// This ONLY gets called when a cell is edited in THIS viewer



			//			BankAccountViewModel ss = new BankAccountViewModel();
			//			CustomerViewModel cs = new CustomerViewModel();
			//			DetailsViewModel sa = new DetailsViewModel();

			//			Mouse . OverrideCursor = Cursors . Wait;

			//			// Set the control flags so that we know we have changed data when we notify other windows
			//			Flags . UpdateInProgress = true;

			//			// Set a global flag so we know we are in editing mode in the grid
			//			//
			//			//Only called whn an edit has been completed
			//			SQLHandlers sqlh = new SQLHandlers ( );
			//			// These get the row with all the NEW data
			//			if ( CurrentDb == "BANKACCOUNT" )
			//			{
			//				int currow = 0;

			//				currow = bguv . SelectedIndex != -1 ? bguv . SelectedIndex : 0;
			//				ss = bguv . SelectedItem as BankAccountViewModel;
			//			}
			//			else if ( CurrentDb == "CUSTOMER" )
			//			{
			//				int currow = 0;
			//				currow = Custgrid . SelectedIndex != -1 ? Custgrid . SelectedIndex : 0;
			//				cs = Custgrid . SelectedItem as CustomerViewModel;
			//			}
			//			else if ( CurrentDb == "DETAILS" )
			//			{
			//				int currow = 0;
			//				currow = Detgrid . SelectedIndex != -1 ? Detgrid . SelectedIndex : 0;
			//				sa = Detgrid . SelectedItem as DetailsViewModel;
			//			}



			//			if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
			//			{
			//				// Editdb is NOT OPEN
			//				SqlCommand cmd = null;
			//				try
			//				{
			//					//Sanity check - are values actualy valid ???
			//					//They should be as Grid vlaidate entries itself !!
			//					int x;
			//					decimal Y;
			//					if ( CurrentDb == "BANKACCOUNT" )
			//					{
			//						//						ss = e.Row.Item as BankAccount;
			//						x = Convert . ToInt32 ( ss . Id );
			//						x = Convert . ToInt32 ( ss . AcType );
			//						//Check for invalid A/C Type
			//						if ( x < 1 || x > 4 )
			//						{
			//							Debug . WriteLine ( $"SQL Invalid A/c type of {ss . AcType} in grid Data" );
			//							Mouse . OverrideCursor = Cursors . Arrow;
			//							MessageBox . Show ( $"Invalid A/C Type ({ss . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
			//							return;
			//						}
			//						Y = Convert . ToDecimal ( ss . Balance );
			//						Y = Convert . ToDecimal ( ss . IntRate );
			//						//Check for invalid Interest rate
			//						if ( Y > 100 )
			//						{
			//							Debug . WriteLine ( $"SQL Invalid Interest Rate of {ss . IntRate} > 100% in grid Data" );
			//							Mouse . OverrideCursor = Cursors . Arrow;
			//							MessageBox . Show ( $"Invalid Interest rate ({ss . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
			//							return;
			//						}
			//						DateTime dtm = Convert . ToDateTime ( ss . ODate );
			//						dtm = Convert . ToDateTime ( ss . CDate );
			//					}
			//					else if ( CurrentDb == "DETAILS" )
			//					{
			//						x = Convert . ToInt32 ( sa . Id );
			//						x = Convert . ToInt32 ( sa . AcType );
			//						//Check for invalid A/C Type
			//						if ( x < 1 || x > 4 )
			//						{
			//							Debug . WriteLine ( $"SQL Invalid A/c type of {sa . AcType} in grid Data" );
			//							Mouse . OverrideCursor = Cursors . Arrow;
			//							MessageBox . Show ( $"Invalid A/C Type ({sa . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
			//							return;
			//						}
			//						Y = Convert . ToDecimal ( sa . Balance );
			//						Y = Convert . ToDecimal ( sa . IntRate );
			//						//Check for invalid Interest rate
			//						if ( Y > 100 )
			//						{
			//							Debug . WriteLine ( $"SQL Invalid Interest Rate of {sa . IntRate} > 100% in grid Data" );
			//							Mouse . OverrideCursor = Cursors . Arrow;
			//							MessageBox . Show ( $"Invalid Interest rate ({sa . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
			//							return;
			//						}
			//						DateTime dtm = Convert . ToDateTime ( sa . ODate );
			//						dtm = Convert . ToDateTime ( sa . CDate );
			//					}
			//					//					string sndr = sender.ToString();
			//				}
			//				catch ( Exception ex )
			//				{
			//					Debug . WriteLine ( $"SQL Invalid grid Data - {ex . Message} Data = {ex . Data}" );
			//					Mouse . OverrideCursor = Cursors . Arrow;
			//					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
			//					return;
			//				}
			//			}
			//			SqlConnection con;
			//			string ConString = "";
			//			ConString = ( string ) Settings . Default [ "BankSysConnectionString" ];
			//			//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
			//			con = new SqlConnection ( ConString );
			//			try
			//			{
			//				//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
			//				using ( con )
			//				{
			//					con . Open ( );

			//					if ( CurrentDb == "BANKACCOUNT" )
			//					{
			//						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( ss . Balance ) );
			//						cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( ss . IntRate ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of BankAccounts successful..." );

			//						cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
			//						cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of SecAccounts successful..." );

			//						cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of Customers successful..." );
			//					}
			//					else if ( CurrentDb == "DETAILS" )
			//					{
			//						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
			//						cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of BankAccounts successful..." );

			//						cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
			//						cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of SecAccounts successful..." );

			//						cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of customers successful..." );
			//					}
			//					if ( CurrentDb == "SECACCOUNTS" )
			//					{
			//						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( ss . Balance ) );
			//						cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( ss . IntRate ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of BankAccounts successful..." );

			//						cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
			//						cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of SecAccounts successful..." );

			//						cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of Customers successful..." );
			//					}
			//					StatusBar . Text = "ALL THREE Databases updated successfully....";
			//					Debug . WriteLine ( "ALL THREE Databases updated successfully...." );
			//				}
			//			}
			//			catch ( Exception ex )
			//			{
			//				Debug . WriteLine ( $"SQL Error - {ex . Message} Data = {ex . Data}" );

			//#if SHOWSQLERRORMESSAGEBOX
			//						Mouse . OverrideCursor = Cursors . Arrow;
			//						MessageBox . Show ( "SQL error occurred - See Output for details" );
			//#endif
			//			}
			//			finally
			//			{
			//				Mouse . OverrideCursor = Cursors . Arrow;
			//				con . Close ( );
			//			}




			//			if ( CurrentDb == "CUSTOMER" )
			//			{
			//				if ( e == null && CurrentDb == "CUSTOMER" )
			//					cs = e . Row . Item as CustomerViewModel;

			//				try
			//				{
			//					//Sanity check - are values actualy valid ???
			//					//They should be as Grid vlaidate entries itself !!
			//					int x;
			//					x = Convert . ToInt32 ( cs . Id );
			//					//					string sndr = sender.ToString();
			//					x = Convert . ToInt32 ( cs . AcType );
			//					//Check for invalid A/C Type
			//					if ( x < 1 || x > 4 )
			//					{
			//						Debug . WriteLine ( $"SQL Invalid A/c type of {cs . AcType} in grid Data" );
			//						Mouse . OverrideCursor = Cursors . Arrow;
			//						MessageBox . Show ( $"Invalid A/C Type ({cs . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
			//						return;
			//					}
			//					DateTime dtm = Convert . ToDateTime ( cs . ODate );
			//					dtm = Convert . ToDateTime ( cs . CDate );
			//					dtm = Convert . ToDateTime ( cs . Dob );
			//				}
			//				catch ( Exception ex )
			//				{
			//					Debug . WriteLine ( $"SQL Invalid grid Data - {ex . Message} Data = {ex . Data}" );
			//					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
			//					Mouse . OverrideCursor = Cursors . Arrow;
			//					return;
			//				}
			//				SqlConnection con;
			//				string ConString = "";
			//				ConString = ( string ) Settings . Default [ "BankSysConnectionString" ];
			//				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
			//				con = new SqlConnection ( ConString );
			//				try
			//				{
			//					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
			//					using ( con )
			//					{
			//						con . Open ( );
			//						SqlCommand cmd = new SqlCommand ( "UPDATE Customer SET CUSTNO=@custno, BANKNO=@bankno, ACTYPE=@actype, " +
			//											"FNAME=@fname, LNAME=@lname, ADDR1=@addr1, ADDR2=@addr2, TOWN=@town, COUNTY=@county, PCODE=@pcode," +
			//											"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );

			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@fname" , cs . FName . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@lname" , cs . LName . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@addr1" , cs . Addr1 . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@addr2" , cs . Addr2 . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@town" , cs . Town . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@county" , cs . County . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@pcode" , cs . PCode . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@phone" , cs . Phone . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@mobile" , cs . Mobile . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@dob" , Convert . ToDateTime ( cs . Dob ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of Customers successful..." );

			//						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, " +
			//							" ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of BankAccounts successful..." );

			//						cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, " +
			//							"ODATE=@odate, CDATE=@cdate where CUSTNO=@custno AND BANKNO = @bankno" , con );
			//						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
			//						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
			//						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
			//						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
			//						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
			//						cmd . ExecuteNonQuery ( );
			//						Debug . WriteLine ( "SQL Update of SecAccounts successful..." );
			//					}
			//					StatusBar . Text = "ALL THREE Databases updated successfully....";
			//					Debug . WriteLine ( "ALL THREE Databases updated successfully...." );
			//				}
			//				catch ( Exception ex )
			//				{
			//					Debug . WriteLine ( $"SQL Error - {ex . Message} Data = {ex . Data}" );
			//#if SHOWSQLERRORMESSAGEBOX
			//						Mouse . OverrideCursor = Cursors . Arrow;
			//						MessageBox . Show ( "SQL error occurred - See Output for details" );
			//#endif
			//				}
			//				finally
			//				{
			//					Mouse . OverrideCursor = Cursors . Arrow;
			//					con . Close ( );
			//				}
			//				Mouse . OverrideCursor = Cursors . Arrow;
			//				// Set the control flags so that we know we have changed data when we notify other windows
			//				Flags . UpdateInProgress = true;
			//				return;
			//			}

			//			// This is the NEW DATA from the current row that we are sending to SQL each update handler to update the DB's
			//			if ( Flags . USECOPYDATA )
			//			{
			//				UpdateSingleBankDb ( ss , DbName );

			//			}
			//			else
			//			{
			//				UpdateSingleBankDb ( ss , "BANKACCOUNT" );
			//				UpdateSingleCustomersDb ( ss , "BANKACCOUNT" );
			//				UpdateSingleDetailsDb ( ss , "BANKACCOUNT" );
			//			}

			//			Mouse . OverrideCursor = Cursors . Arrow;
			//			// Set the control flags so that we know we have changed data when we notify other windows
			//			Flags . UpdateInProgress = false;

			//			return;
		}

		#region Generic Db load	  ASYNC
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
		public static async Task<bool> GetGenericCollectionAsync ( List<string> collection ,
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
		string SqlCommand = "" ,
		bool Notify = false ,
		string Caller = "" )
		{
			string[] datain = { "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" };  // 40 elements 
			List<string> DbData = new List<string> ( );
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			try
			{
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					//***************************************************************************************************************//
					var Data = db . Query<object> ( SqlCommand ) . ToList ( );
					//***************************************************************************************************************//

					Debug . WriteLine ( $"SQL DAPPER {Data . Count} records successfuly" );
					var dat = Data . Select ( x => x . ToString ( ) ) . ToList ( );
					string str = "";
					foreach ( var item in dat )
					{
						str = item . Substring ( 12 );
						collection . Add ( str );
					}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"GENERIC DB ERROR : {ex . Message}" );
				return false;
			}
			return true;
		}

		#endregion Generic Db load

		#region Generic Db load
		public static List<string> GetGenericCollection ( List<string> collection ,
		string SqlCommand = "" ,
		bool Notify = false ,
		string Caller = ""
		)

		{
			string[] datain = { "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" };  // 40 elements 
			List<string> DbData = new List<string> ( );
			//static IEnumerable  List<string> strarray;
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			try
			{
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					//object data=null;
					//var parameters= new DynamicParameters(data);

					//***************************************************************************************************************//
					var Data = db . Query ( SqlCommand , CommandType . Text ) . ToList ( );
					//***************************************************************************************************************//

					//var Data = db . Query<dynamic>( SqlCommand  ). ToList();
					Debug . WriteLine ( $"SQL DAPPER {Data . Count} records successfuly" );
					var dat = Data . Select ( x => x . ToString ( ) ) . ToList ( );
					string str = "";
					foreach ( var item in dat )
					{
						str = item?.Substring ( 12 );
						if ( str != null )
							DbData . Add ( str );
					}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"GENERIC DB ERROR : {ex . Message}" );
			}
			//{
			//	EventControl . TriggerCustDataLoaded ( null ,
			//		new LoadedEventArgs
			//		{
			//			CallerType = "SQLSERVER" ,
			//			CallerDb = Caller 
			//			//DataSource = cvmcollection ,
			//			//RowCount = cvmcollection . Count
			//		} );
			return DbData;// (List<List<string>>)null;
		}

		public static bool DeleteDbTable ( string DbName )
		{
			bool result = false;
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			try
			{
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					//object data=null;
					//var parameters= new DynamicParameters(data);
					var qres = db . Execute ( $"Drop Table {DbName}" );
					if ( qres != -1 )
						result = true;

					//var Data = db . Query<dynamic>( SqlCommand  ). ToList();
					//					Debug. WriteLine ( $"SQL DAPPER {Data . Count} records successfuly" );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"GENERIC DB ERROR : {ex . Message}" );
			}
			return result;

		}
		#endregion Generic Db load

		public static bool UpdateDbTable ( string sqlcommand)
		{
			bool result = false;
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}

			try
			{
				using ( IDbConnection db = new SqlConnection ( ConString ) )
				{
					//object data=null;
					//var parameters= new DynamicParameters(data);

					Debug . WriteLine ( $"calling {sqlcommand}");
					var qres = db . Execute ( sqlcommand );
					if ( qres != -1 )
						result = true;

					//var Data = db . Query<dynamic>( SqlCommand  ). ToList();
					//					Debug. WriteLine ( $"SQL DAPPER {Data . Count} records successfuly" );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"GENERIC DB ERROR : {ex . Message}" );
				MessageBox . Show ( ex . Message , "SQL Update Failure");
			}
			return result;

		}

		#region Create Copy of any specified Db

		public static bool CreateDbCopy (
			string OriginalDb ,
			string NewDb )
		{
			// All working WELL!!! 28/10/21
			string SqlCommand = "", TestCommand = "";
			bool result = false;
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			using ( IDbConnection connection = new SqlConnection ( ConString ) )
			{
				// Check for existence of Db to be created
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
				try
				{
					TestCommand = $"Select top(1) * from  {NewDb}";
					var res = connection . QueryFirst<int> ( TestCommand );
					if ( res > 0 )
						return false;
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Database not found by test call to Db, proceeding with copy operation" );
				}
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
				// All is well, carry on and Copy Db
				try
				{
					SqlCommand = $"Select * into {NewDb} from {OriginalDb}";

					connection . Execute ( SqlCommand , CommandType . Text );

				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"SQL DAPPER {NewDb} could NOT be created from {OriginalDb}, Error info : {ex . Message}, {ex . Data}" );
				}
				finally
				{
					Debug . WriteLine ( $"SQL DAPPER {NewDb} has been created from {OriginalDb} successfully" );
					result = true;
				}
			}
			return result;
		}
		#endregion Create Copy of any specified Db

		//		#region	CREATEBANKCOMBINEDASYNC
		/*		public async static Task<ObservableCollection<BankCombinedViewModel>> CreateBankCombinedAsync ( ObservableCollection<BankCombinedViewModel> collection ,
        #pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
                string SqlCommand = "" ,
                bool Notify = false )
                {
                    //====================================
                    // Use STD DAPPER QUERY to load Bank data
                    //====================================
                    ObservableCollection<BankCombinedViewModel> bvmcollection = new ObservableCollection<BankCombinedViewModel>();
                    string ConString = Flags . CurrentConnectionString;
                    if ( ConString == "" )
                    {
                        GenericDbUtilities . CheckDbDomain ( "IAN1" );
                        ConString = Flags . CurrentConnectionString;
                    }
                    using ( IDbConnection db = new SqlConnection ( ConString ) )
                    {
                        try
                        {
                            // Use DAPPER to to load Bank data using Stored Procedure
                            //if ( Flags . USEDAPPERWITHSTOREDPROCEDURE )
                            //{
                            try
                            {
                                //var Args = new { DbName = "" , Arg = " " , Conditions = "" , SortBy = "" };
                                SqlCommand = $"spCreateBankCombinedDb";
                                // This syntax WORKS CORRECTLY
                                var result  = db . Query<BankCombinedViewModel>( SqlCommand , null,null,false, null,CommandType.StoredProcedure).ToList();

                                //Debug. WriteLine ( result );
                                foreach ( var item in result )
                                {
                                    bvmcollection . Add ( item );
                                }
                                Debug. WriteLine ( $"SQL DAPPER BANKCOMBINED DB created successfuly" );
                                collection = bvmcollection;
                                return collection;
                            }
                            catch ( Exception ex )
                            {
                                Debug. WriteLine ( $"BANK  DB ERROR : {ex . Message}" );
                            }
                            //}
                            //else
                            //{
                            //	Debug. WriteLine ("Flags.USEDAPPERWITHSTOREDPROCEDURE not set !!!!");
                            //}
                        }
                        catch ( Exception ex )
                        {
                            Debug. WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
                            Utils .trace ( "CreateBankCombinedAsync : " );
                            return null;
                        }
                    }
                    return collection;
                }
                #endregion	CREATEBASNKCOMBINEDASYNC
        /*

                #region	GETBANKCOMBINEDDB
                // NOT ASYNC !!
                public static ObservableCollection<BankCombinedViewModel> GetBankCombinedDb ( ObservableCollection<BankCombinedViewModel> collection ,
                string SqlCommand = "" ,
                string DbNameToLoad = "" ,
                string Orderby = "" ,
                string Conditions = "" ,
                bool wantSort = false ,
                bool Notify = false ,
                string Caller = "" ,
                int [ ] args = null )
                {
                    ObservableCollection<BankCombinedViewModel> bvmcollection = new ObservableCollection<BankCombinedViewModel>();
                    string ConString = Flags . CurrentConnectionString;
                    if ( ConString == "" )
                    {
                        GenericDbUtilities . CheckDbDomain ( "IAN1" );
                        ConString = Flags . CurrentConnectionString;
                    }
                    //============================================
                    // Use STD DAPPER QUERY to load BankCombined data
                    //============================================
                    string[] ValidFields=
                    {
                        "ID",
                        "CUSTNO",
                        "BANKNO",
                        "ACTYPE",
                        "INTRATE" ,
                        "BALANCE" ,
                        "ODATE" ,
                        "CDATE",
                        "FNAME",
                        "LNAME",
                        "ADDR1",
                        "ADDR2",
                        "TOWN",
                        "COUNTY",
                        "PCODE",
                        "PHONE"
                        };
                    string[] errorcolumns;

                    IEnumerable < BankCombinedViewModel> bvmi;
                    using ( IDbConnection db = new SqlConnection ( ConString ) )
                    {
                        if ( ValidateSortConditionColumns ( ValidFields , "BankCombined" , Orderby , Conditions , out errorcolumns ) == false )
                        {
                            if ( Orderby . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
                            {
                                MessageBox . Show ( $"BANKCOMBINED dB\nSorry, but an invalid Column name has been \nidentified in the Sorting Clause provided.\n\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}.\n\nTherefore No Sort will be performed for this Db" );
                                Orderby = "";
                            }
                            else if ( Conditions . ToUpper ( ) . Contains ( errorcolumns [ 0 ] ) )
                            {
                                MessageBox . Show ( $"BANKCOMBINED dB\nSorry, but an invalid Match clause or Column Name \nhas been identified in the Data Selection Clause.\n\nThe Invalid item identified was :\n\t{errorcolumns [ 0 ]}\n\nTherefore No Data Matching will be performed for this Db" );
                                Conditions = "";
                            }
                            else
                            {
                                MessageBox . Show ( $"BANKCOMBINED dB\nSorry, but the Loading of the BankCombined Db is being aborted due to \na non existent Column name.\nThe Invalid Column identified was :\n{errorcolumns [ 0 ]}" );
                                return collection;
                            }
                        }

                        //// make sure order by clause is correctly formatted
                        //if ( Orderby . Trim ( ) != "" )
                        //{
                        //	if ( Orderby . ToUpper ( ) . Contains ( "ORDER BY " ) == false )
                        //	{
                        //		Orderby = " Order by " + Orderby;
                        //	}
                        //}
                        //if ( Conditions != "" )
                        //{
                        //	if ( Conditions . ToUpper ( ) . Contains ( "WHERE" ) == false )
                        //		Conditions = " Where " + Conditions;
                        //}
                        try
                        {
                            Orderby = Orderby . Contains ( "Order by" ) ? Orderby . Substring ( 9 ) : Orderby;
                            Conditions = Conditions . Contains ( "where " ) ? Conditions . Substring ( 6 ) : Conditions;
                            //====================================
                            // Use standard DAPPER code to load Bank data
                            //====================================
                            if ( Conditions != "" )
                            {
                                if ( args [ 2 ] > 0 )
                                    SqlCommand = $" Select top ({args [ 2 ]}) * from {DbNameToLoad} {Conditions} {Orderby}";
                                else
                                    SqlCommand = $" Select * from {DbNameToLoad} {Conditions} {Orderby}";
                            }
                            else
                            {
                                if ( args != null )
                                {
                                    if ( Conditions == "" && Orderby == "" && args [ 0 ] == 0 && args [ 1 ] == 0 && args [ 2 ] == 0 )   // we dont even  have args for total records
                                        SqlCommand = $" Select * from {DbNameToLoad} ";
                                    else if ( args [ 0 ] != 0 || args [ 1 ] != 0 || args [ 2 ] != 0 )   // we do have args for total records
                                    {
                                        if ( args [ 2 ] == 0 )       // no limit on how many records to get
                                        {
                                            SqlCommand = $" Select * from {DbNameToLoad} ";
                                            if ( Conditions != "" )
                                                SqlCommand += $" {Conditions} ";
                                            else if ( args [ 1 ] != 0 )
                                                SqlCommand += $" where CustNo >= { args [ 0 ]} AND CustNo <= { args [ 1 ]} ";
                                        }
                                        else if ( args [ 2 ] > 0 && args [ 1 ] == 0 )
                                            SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} ";
                                        else if ( args [ 1 ] > 0 )// All 3 args are received
                                            SqlCommand = $" Select Top ({args [ 2 ]}) * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
                                        else
                                            SqlCommand = $" Select * from {DbNameToLoad} where CustNo >= {args [ 0 ]} AND CustNo <= {args [ 1 ]}";
                                    }
                                }
                                if ( Conditions != "" )  // We have conditions
                                    SqlCommand = $"Select * from {DbNameToLoad} {Conditions} ";
                                else if ( args == null || args . Length == 0 )    // No args or conditions, so use generic command
                                    SqlCommand = $"Select * from {DbNameToLoad}  ";
                                // Final Trap to ensure we have a valid command line
                                if ( SqlCommand == "" )
                                    SqlCommand = $" Select * from {DbNameToLoad} ";

                                if ( wantSort && Orderby != "" )

                                    SqlCommand += $" {Orderby}";
                            }
                            // Read data via Dapper into list<BVMI> cos Dapper uses Linq, so we cannot get other types returned
                            bvmi = db . Query<BankCombinedViewModel> ( SqlCommand );

                            foreach ( var item in bvmi )
                            {
                                bvmcollection . Add ( item );
                            }
                            collection = bvmcollection;
                        }
                        catch ( Exception ex )
                        {
                            Debug. WriteLine ( $"SQL DAPPER error : {ex . Message}, {ex . Data}" );
                        }
                        finally
                        {
                            Debug. WriteLine ( $"SQL DAPPER {DbNameToLoad}  loaded : {bvmcollection . Count} records successfuly" );
                        }
                    }
                    return bvmcollection;
                }
                #endregion	GETBANKCOMBINEDDB
        */
		#region VALIDATESORTCONDITIONCOLUMNS
		public static bool ValidateSortConditionColumns ( string [ ] validFields , string caller , string orderby , string sortby , out string [ ] errorcolumns )
		{
			string[] errors = { "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" };
			string Searchstring = "";
			int counter = 0;
			int errorcount = 0;
			string[] FieldsToFind = { "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , };
			bool result = true;
			string[] temp;
			char breakchar = ' ';

			if ( orderby != "" && orderby != null )
			{
				temp = orderby . Split ( breakchar );
				if ( temp . Length > 0 )
				{
					foreach ( var item in temp )
					{
						string tmp = item . ToUpper ( ) . Trim ( );
						if ( tmp != "ORDER" && tmp != "BY" && tmp . Contains ( "DESC" ) == false && tmp . Contains ( "ASC" ) == false )
						{
							if ( tmp . Contains ( "," ) )
								tmp = tmp . Substring ( 0 , tmp . Length - 1 );
							FieldsToFind [ counter++ ] = tmp;
						}
					}
				}
			}
			if ( sortby != "" )
			{
				temp = sortby . Split ( breakchar );
				if ( temp . Length > 0 )
				{
					bool isnumeric = false;
					string ignorechars = "  <   <=    =>    >  >=   ==    =  !=   !  1 2 3 4 5 6 7 8 9 0 ";
					foreach ( var item in temp )
					{
						string tmp = item . ToUpper ( ) . Trim ( );
						if ( tmp != "" )
						{
							try
							{
								double d = double . Parse ( tmp );
								isnumeric = true;
							}
							catch // let it drop thru
							{ }

							if ( tmp != "WHERE" && isnumeric == false )
							{
								// Is it  a maths sign etc 
								bool b = ( ignorechars . Contains ( tmp ) == true );
								if ( b == false )
								{
									string c = tmp[0] . ToString ( );
									string d = tmp[tmp . Length - 1] . ToString ( );
									// check if it's a quoted string, if so, let it through
									if ( c != "'" || d != "'" )
									{
										if ( tmp . Contains ( "," ) )
											tmp = tmp . Substring ( 0 , tmp . Length - 1 );
										FieldsToFind [ counter++ ] = tmp;
									}
								}
							}
						}
					}
				}
			}
			foreach ( var item in validFields )
			{
				if ( item != "" )
					Searchstring += item + ",";
			}
			foreach ( var item in FieldsToFind )
			{
				if ( item == "" )
					break;
				if ( validFields . Contains ( item ) == false )
				{
					errors [ errorcount++ ] = item . ToUpper ( );
					result = false;
				}
			}
			errorcolumns = errors;
			return result;
			//		}
		}

		#endregion VALIDATESORTCONDITIONCOLUMNS

		#region GENERAL METHODS

		public static List<string> GetConnectionStrings ( )
		{
			List<string> Constrings = new List<string> ( );
			var v = System . Configuration . ConfigurationManager . ConnectionStrings;
			foreach ( var item in v )
			{
				Constrings . Add ( item . ToString ( ) );
			}
			return Constrings;
		}
		public static int GetGenericColumnCount ( ObservableCollection<GenericClass> collection , GenericClass gcc = null )
		{
			GenericClass gc = new GenericClass ( );
			try
			{
				if ( gcc == null )
					gc = collection [ 0 ] as GenericClass;
				else
					gc = gcc;

				if ( gc . field20 != null )
				{ return 20; }
				else if ( gc . field19 != null )
				{ return 19; }
				else if ( gc . field18 != null )
				{ return 18; }
				else if ( gc . field17 != null )
				{ return 17; }
				else if ( gc . field16 != null )
				{ return 16; }
				else if ( gc . field15 != null )
				{ return 15; }
				else if ( gc . field14 != null )
				{ return 14; }
				else if ( gc . field13 != null )
				{ return 13; }
				else if ( gc . field12 != null )
				{ return 12; }
				else if ( gc . field11 != null )
				{ return 11; }
				else if ( gc . field10 != null )
				{ return 10; }
				else if ( gc . field9 != null )
				{ return 9; }
				else if ( gc . field8 != null )
				{ return 8; }
				else if ( gc . field7 != null )
				{ return 7; }
				else if ( gc . field6 != null )
				{ return 6; }
				else if ( gc . field5 != null )
				{ return 5; }
				else if ( gc . field4 != null )
				{ return 4; }
				else if ( gc . field3 != null )
				{ return 3; }
				else if ( gc . field2 != null )
				{ return 2; }
				else if ( gc . field1 != null )
				{ return 1; }
				return 0;
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Column count error '{ex . Message}, {ex . Data}'" );
			}
			return 0;
		}
		public static void AddDictPairToGeneric<T> ( T gc , KeyValuePair<string , object> dict , int dictcount ) where T : GenericClass
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
		public static GenericClass ParseDapperRow ( dynamic buff , Dictionary<string , object> dict , out int colcount , ref List<int> varcharlen , bool GetLength = false )
		{
			GenericClass GenRow = new GenericClass ( );
			int index = 0;
			colcount = 0;
			foreach ( var item in buff )
			{
				try
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
				catch ( Exception ex )
				{
					MessageBox . Show ( $"ParseDapper error was : \n{ex . Message}\nKey={item . Key} Value={item . Value . ToString ( )}" );
					break;
				}
			}
			colcount = index;
			return GenRow;
		}
		public static string GetStringFromGenericRow ( GenericClass GenRow )
		{
			// Create a string containg data from ALL non null fields  in a GenericClass record
			string output = "";
			for ( int i = 0 ; i < 20 ; i++ )
			{
				if ( GenRow . field1 != "" )
					output += GenRow . field1 . Trim ( );
				if ( GenRow . field2 != "" )
					output += GenRow . field2 . Trim ( ) + ",";
				if ( GenRow . field3 != "" )
					output += GenRow . field3 . Trim ( ) + ",";
				if ( GenRow . field4 != "" )
					output += GenRow . field4 . Trim ( ) + ",";
				if ( GenRow . field5 != "" )
					output += GenRow . field5 . Trim ( ) + ",";
				if ( GenRow . field6 != "" )
					output += GenRow . field6 . Trim ( ) + ",";
				if ( GenRow . field7 != "" )
					output += GenRow . field7 . Trim ( ) + ",";
				if ( GenRow . field8 != "" )
					output += GenRow . field8 . Trim ( ) + ",";
				if ( GenRow . field9 != "" )
					output += GenRow . field9 . Trim ( ) + ",";
				if ( GenRow . field10 != "" )
					output += GenRow . field10 . Trim ( ) + ",";
				if ( GenRow . field11 != "" )
					output += GenRow . field11 . Trim ( ) + ",";
				if ( GenRow . field12 != "" )
					output += GenRow . field12 . Trim ( ) + ",";
				if ( GenRow . field13 != "" )
					output += GenRow . field13 . Trim ( ) + ",";
				if ( GenRow . field14 != "" )
					output += GenRow . field14 . Trim ( ) + ",";
				if ( GenRow . field15 != "" )
					output += GenRow . field15 . Trim ( ) + ",";
				if ( GenRow . field16 != "" )
					output += GenRow . field16 . Trim ( ) + ",";
				if ( GenRow . field17 != "" )
					output += GenRow . field17 . Trim ( ) + ",";
				if ( GenRow . field18 != "" )
					output += GenRow . field18 . Trim ( ) + ",";
				if ( GenRow . field19 != "" )
					output += GenRow . field19 . Trim ( ) + ",";
				if ( GenRow . field20 != "" )
					output += GenRow . field20 . Trim ( ) + ",";
			}
			output = output . Substring ( 0 , output . Length - 1 );
			return output;
		}
		public static GenericClass SaveToField ( GenericClass GenRow , int index , string outstr )
		{
			switch ( index )
			{
				case 0:
					GenRow . field1 = outstr;
					break;
				case 1:
					GenRow . field2 = outstr;
					break;
				case 2:
					GenRow . field3 = outstr;
					break;
				case 3:
					GenRow . field4 = outstr;
					break;
				case 4:
					GenRow . field5 = outstr;
					break;
				case 5:
					GenRow . field6 = outstr;
					break;
				case 6:
					GenRow . field7 = outstr;
					break;
				case 7:
					GenRow . field8 = outstr;
					break;
				case 8:
					GenRow . field9 = outstr;
					break;
				case 9:
					GenRow . field10 = outstr;
					break;
				case 10:
					GenRow . field11 = outstr;
					break;
				case 11:
					GenRow . field12 = outstr;
					break;
				case 12:
					GenRow . field13 = outstr;
					break;
				case 13:
					GenRow . field14 = outstr;
					break;
				case 14:
					GenRow . field15 = outstr;
					break;
				case 15:
					GenRow . field16 = outstr;
					break;
				case 16:
					GenRow . field17 = outstr;
					break;
				case 17:
					GenRow . field18 = outstr;
					break;
				case 18:
					GenRow . field19 = outstr;
					break;
				case 19:
					GenRow . field20 = outstr;
					break;
			}
			return GenRow;
		}
		public static void CheckDbDomain ( string DbDomain )
		{
			if ( Flags . ConnectionStringsDict == null || Flags . ConnectionStringsDict . Count == 0 )
				DapperSupport . LoadConnectionStrings ( );

			if ( CheckResetDbConnection ( DbDomain , out string constring ) == true )
			{
				if ( constring == null )
					Flags . CurrentConnectionString = "";
				else
				{
					if ( constring != "Not Found" )
					{
						ConnString = constring;
						Flags . CurrentConnectionString = ConnString;
					}
					else
						Flags . CurrentConnectionString = constring;
				}
			}
			else
				Flags . CurrentConnectionString = constring;
		}

		public static bool CheckResetDbConnection ( string currdb , out string constring )
		{
			//string constring = "";
			currdb?.ToUpper ( );
			// This resets the current database connection to the one we re working with (currdb - in UPPER Case!)- should be used anywhere that We switch between databases in Sql Server
			// It also sets the Flags.CurrentConnectionString - Current Connectionstring  and local variable
			if ( Utils . GetDictionaryEntry ( Flags . ConnectionStringsDict , currdb , out string connstring ) != "" )
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

		//private static void test ( )
		//{
		//	Dictionary <string, CustomerViewModel> dict = new Dictionary<string, CustomerViewModel>();
		//}
		#endregion GENERAL METHODS

		public static string LoadConnectionStrings ( )
		{
			string cstr = "";
			try
			{
				if ( Flags . ConnectionStringsDict . Count > 0 )
				{
					string str = ( string ) Utils . ReadConfigSetting ( "ConnectionString" );
					return str;
				}
				//                cstr = Utils . ReadConfigSetting ( "ConnectionString" );
				cstr = Utils . ReadConfigSetting ( "NewConstring" );
				Flags . ConnectionStringsDict . Add ( "IAN1" , cstr );
				//ConnectionStringsDict . Add ( "IAN1" , Utils . ReadConfigSetting ( "ConnectionString" ) );
				//                ConnectionStringsDict . Add("IAN2", Utils.ReadConfigSetting("Constring"));
				//ConnectionStringsDict.Add("PUBS", Utils.ReadConfigSetting("PubsConnectionString"));
				//Utils . WriteSerializedCollectionJSON ( ConnectionStringsDict , @"C:\users\ianch\DbConnectionstrings.dat" );
				// string connstr = $@"Data Source=DESKTOP-BEQTQ1J; Initial Catalog="IAN1"; Integrated Security=True; Connect Timeout=30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False"
			}
			catch ( NullReferenceException ex )
			{
				string s = Utils . ReadConfigSetting ( "ConnectionString" );
				Debug . WriteLine ( $"Dictionary  entrry [{s}] already exists" );
			}
			finally
			{
			}
			return cstr;
		}

	}
}
