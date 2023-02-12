using System;
using System . Collections;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Windows;

using Dapper;

using Microsoft . Data . SqlClient;

using Models;

using Wpfmain . ViewModels;

namespace NewWpfDev . Dapper
{

	[Serializable]
	public static class DapperGeneric<T, U, V>
	{
		// All these declarations are required
		public static ObservableCollection<GenericClass>  objG=new ObservableCollection<GenericClass>();
		public static ObservableCollection<BankAccountViewModel>  objB = new ObservableCollection<BankAccountViewModel>();
		public static ObservableCollection<CustomerViewModel>  objC = new ObservableCollection<CustomerViewModel>();
		public static ObservableCollection<DetailsViewModel>  objD = new ObservableCollection<DetailsViewModel>();
		public static GenericClass objGclass = new GenericClass();
		public static List<string> objL=new List<string>();

		// Handles SP and Txt requests.....
		public static IEnumerable<T> ExecuteSPGenericClass (
			ObservableCollection<T> collection ,
			string SqlCommand ,
			string Arguments ,
			string WhereClause ,
			string OrderByClause ,
			out List<string> genericlist ,
			out string errormsg )
		{

			genericlist = new List<string> ( );
			errormsg = "";

			Debug. WriteLine ( $"objG.Count = { objG . Count ( )}" );
			//====================================
			// Use DAPPER to run a Stored Procedure
			//====================================
			string result = "";
			errormsg = "";
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				GenericDbUtilities . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			genericlist = new List<string> ( );

			string arg1="", arg2="", arg3="", arg4="";
			Dictionary<string , object> dict = new Dictionary<string, object>();
			using ( IDbConnection db = new SqlConnection ( ConString ) )
			{
				try
				{
					// Use DAPPER to run  Stored Procedure
					try
					{
						// Parse out the arguments and put them in correct order for all SP's
						if ( Arguments . Contains ( "," ) )
						{
							string []   args =Arguments .Trim().Split(',');
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
						var Params = new DynamicParameters();
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

						// process a standard sql query string
						if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) )
						{
							Debug. WriteLine ( $"Running Select db.Query" );
							//***************************************************************************************************************//
							var reslt = db . Query ( SqlCommand, CommandType.Text);
							//***************************************************************************************************************//

							if ( reslt == null )
							{
								errormsg = "DT";
								return null;
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
										// create new obs<T>
										// Create an instance - Activator.CreateInstance will call the default constructor.
										// This is equivalent to calling new ObservableCollection<T>().

										var openType = typeof ( ObservableCollection<T> );
										Type[] tArgs = {typeof( ObservableCollection <T>) };
										Type target = openType.MakeGenericType(tArgs);
										var gc = (GenericClass)Activator.CreateInstance (target);

										try
										{
											// we need to create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
											string buffer = "";
											gc = ParseDapperRowGen ( gc , dict , out colcount );
											// Create a dictionary entry for each record
											StringBuilder sb = new StringBuilder();
											int indx=2;
											colcount = 0;
											foreach ( var item2 in item )
											{
												try
												{
													if ( item . Key == "" || item . Value == null )
														break;
													dict . Add ( item . Key , item . Value );
												}
												catch ( Exception ex )
												{
													MessageBox . Show ( $"ParseDapper error was : \n{ex . Message}\nKey={item . Key} Value={item . Value . ToString ( )}" );
												}
											}
											colcount = indx;
											dictcount = 1;
											fldcount = dict . Count;
											string tmp="";
											foreach ( var pair in dict )
											{
												try
												{
													if ( pair . Key != null && pair . Value != null )
													{
														DapperSupport . AddDictPairToGeneric<GenericClass> ( gc , pair , dictcount++ );
														tmp = pair . Key . ToString ( ) + "=" + pair . Value . ToString ( );
														buffer += tmp + ",";
													}
												}
												catch ( Exception ex )
												{
													Debug. WriteLine ( $"Dictionary ERROR : {ex . Message}" );
													result = ex . Message;
												}
											}
											//remove trailing comma
											string s = buffer . Substring (0, buffer . Length - 1 );
											buffer = s;
											genericlist . Add ( buffer );
										}
										catch ( Exception ex )
										{
											result = $"SQLERROR : {ex . Message}";
											Debug. WriteLine ( result );
										}

										objG . Add ( gc );


										dict . Clear ( );
										dictcount = 1;
									}
								}
								catch ( Exception ex )
								{
									Debug. WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
									result = ex . Message;
								}
								errormsg = $"DYNAMIC:{fldcount}";
								return null;
							}
						}
						else
						{
							// probably a stored procedure ?  							
							bool IsSuccess=false;
							Debug. WriteLine ( $"Running SP db.Query" );

							//***************************************************************************************************************//
							//WORKING  JUST FINE FOR OBSERVABLECOLLECTION<GENERICCLASS>
							var reslt = db . Query ( SqlCommand ,
																			   Params
																			   ,commandType: CommandType . StoredProcedure );
							//***************************************************************************************************************//
							// Generic trial 	     Both work as long as classes are marked as [Serializable]
							var Tobjtype = typeof(T );
							//							var  z = SqlServerCommands .deepClone( collection);

							if ( reslt != null )
							{
								//Although this is duplicated  with the one above we CANNOT make it a method()
								int dictcount = 0;
								int fldcount = 0;
								long zero= reslt.LongCount ();
								try
								{
									foreach ( var item in reslt )
									{
										GenericClass gc = new GenericClass();
										try
										{
											//	Create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
											gc = ParseDapperRowGen ( item , dict , out colcount );
											dictcount = 1;
											fldcount = dict . Count;
											if ( fldcount == 0 )
											{
												//no problem, we will get a Datatable anyway
												return null;
											}
											foreach ( var pair in dict )
											{
												try
												{
													if ( pair . Key != null && pair . Value != null )
													{
														DapperSupport . AddDictPairToGeneric ( gc , pair , dictcount++ );
													}

												}
												catch ( Exception ex )
												{
													Debug. WriteLine ( $"Dictionary ERROR : {ex . Message}" );
													result = ex . Message;
												}
											}
											IsSuccess = true;
										}
										catch ( Exception ex )
										{
											result = $"SQLERROR : {ex . Message}";
											Debug. WriteLine ( result );
										}

										objG . Add ( gc );
										dict . Clear ( );
										dictcount = 1;
									}
								}
								catch ( Exception ex )
								{
									Debug. WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
									if ( ex . Message . Contains ( "not find stored procedure" ) )
									{
										result = $"SQL PARSE ERROR - [{ex . Message}]";
										errormsg = $"{result}";
									}
									else
									{
										long x= reslt.LongCount ();
										if ( x == ( long ) 0 )
										{
											result = $"ERROR : [{SqlCommand}] returned ZERO records... ";
											errormsg = $"DYNAMIC:0";
											return null;
										}
										else
										{
											result = ex . Message;
											errormsg = $"UNKNOWN :{ex . Message}";
										}
									}
								}
							}
							else
							{
								if ( IsSuccess == false )
									Debug. WriteLine ( $"Dapper request returned zero results" );
							}
						}
					}
					//	else
					//{
					//	errormsg = "DT";
					//	return null;
					//}
					catch ( Exception ex )
					{
						Debug. WriteLine ( $"STORED PROCEDURE ERROR : {ex . Message}" );
						result = ex . Message;
						errormsg = $"SQLERROR : {result}";
					}
				}
				catch ( Exception ex )
				{
					Debug. WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
					result = ex . Message;
				}
			}
			return null;
		}
		public static ObservableCollection<GenericClass> ExecuteSPFullGenericClass<U1> (
				ref U1 fullCollection ,
				bool UseFull ,
				ref ObservableCollection<T> collection ,
				string SqlCommand ,
				string Arguments ,
				string WhereClause ,
				string OrderByClause ,
				ref List<GenericClass> genericlist ,
				out string errormsg )
		{
			errormsg = "";
#pragma warning disable CS0219 // The variable 'ArgType' is assigned but its value is never used
			int ArgType = 0;
#pragma warning restore CS0219 // The variable 'ArgType' is assigned but its value is never used
			Debug. WriteLine ( $"objG.Count = { objG . Count ( )}" );
			//====================================
			// Use DAPPER to run a Stored Procedure or Select query
			//====================================
			string result = "";
#pragma warning disable CS0219 // The variable 'HasArgs' is assigned but its value is never used
			bool HasArgs = false;
#pragma warning restore CS0219 // The variable 'HasArgs' is assigned but its value is never used
#pragma warning disable CS0219 // The variable 'argcount' is assigned but its value is never used
			int argcount = 0;
#pragma warning restore CS0219 // The variable 'argcount' is assigned but its value is never used
			//DbToOpen = "";
			errormsg = "";
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				GenericDbUtilities . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
#pragma warning disable CS0168 // The variable 'resultDb' is declared but never used
			IEnumerable  resultDb;
#pragma warning restore CS0168 // The variable 'resultDb' is declared but never used

			string arg1="", arg2="", arg3="", arg4="";
			Dictionary<string , object> dict = new Dictionary<string, object>();
			using ( IDbConnection db = new SqlConnection ( ConString ) )
			{
				try
				{
					// Use DAPPER to run  Stored Procedure
					try
					{
						if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) == false )
						// Parse out the arguments and put them in correct order for all SP's
						{
							if ( Arguments . Contains ( "," ) )
							{
								string []   args =Arguments .Trim().Split(',');
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
								// SP with One or No arguments
								arg1 = Arguments;
								if ( arg1 . Contains ( "," ) )              // trim comma off
									arg1 = arg1 . Substring ( 0 , arg1 . Length - 1 );
							}
							{
								// Create our aguments using the Dynamic parameters provided by Dapper
								var Params = new DynamicParameters();
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
#pragma warning disable CS0219 // The variable 'maxcols' is assigned but its value is never used
								int colcount = 0, maxcols = 0;
#pragma warning restore CS0219 // The variable 'maxcols' is assigned but its value is never used
								var reslt = db . Query ( SqlCommand ,
																			   null
																			   ,commandType: CommandType . StoredProcedure );


								// How to test the type if a generic
								if ( typeof ( T ) == typeof ( ObservableCollection<GenericClass> ) )
								{
#pragma warning disable CS0219 // The variable 'k' is assigned but its value is never used
									int k = 0;
#pragma warning restore CS0219 // The variable 'k' is assigned but its value is never used
								}
								if ( reslt != null )
								{
									//Although this is duplicated  with the one above we CANNOT make it a method()
#pragma warning disable CS0219 // The variable 'IsSuccess' is assigned but its value is never used
									bool IsSuccess=false;
#pragma warning restore CS0219 // The variable 'IsSuccess' is assigned but its value is never used
									int dictcount = 0;
									int fldcount = 0;
									//									int colcount=0;
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
												gc = ParseDapperRowGen ( item , dict , out colcount );
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
															DapperSupport . AddDictPairToGeneric<GenericClass> ( gc , pair , dictcount++ );
														}

													}
													catch ( Exception ex )
													{
														Debug. WriteLine ( $"Dictionary ERROR : {ex . Message}" );
														result = ex . Message;
													}
												}
												IsSuccess = true;
											}
											catch ( Exception ex )
											{
												result = $"SQLERROR : {ex . Message}";
												Debug. WriteLine ( result );
											}

											objG . Add ( gc );
											dict . Clear ( );
											dictcount = 1;
										}
										return objG;
									}
									catch ( Exception ex )
									{
										Debug. WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
										if ( ex . Message . Contains ( "not find stored procedure" ) )
										{
											result = $"SQL PARSE ERROR - [{ex . Message}]";
											errormsg = $"{result}";
										}
										else
										{
											long x= reslt.LongCount ();
											if ( x == ( long ) 0 )
											{
												result = $"ERROR : [{SqlCommand}] returned ZERO records... ";
												errormsg = $"DYNAMIC:0";
												return null;
											}
											else
											{
												result = ex . Message;
												errormsg = $"UNKNOWN :{ex . Message}";
											}
										}
									}

								}
							}
						}
						// process a standard sql query string
						else if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) )
						{
							Debug. WriteLine ( $"Running Select db.Query" );
							if ( UseFull == true )
							{
								////***************************************************************************************************************//
								//// Full Generic type received, so we want the original Db Structure received, NOT the Generic structure
								////***************************************************************************************************************//
								//if ( typeof ( U ) == typeof ( ObservableCollection<BankAccountViewModel> ) && typeof ( U ) != null )
								//{
								//	var reslt =
								//	db . Query <BankAccountViewModel >( SqlCommand, CommandType.Text).ToList();
								//	//List< BankAccountViewModel> lo = new   List< BankAccountViewModel>();
								//	// just gotta process the Enumbarable 
								//	foreach ( var   item  in reslt )
								//	{
								//		genericlist . Add ( item );
								//	}
								//	return null;
								//}
								//else if ( typeof ( U ) == typeof ( ObservableCollection<CustomerViewModel> ) && typeof ( U ) != null )
								//{
								//	var reslt =
								//	 db . Query <CustomerViewModel>( SqlCommand, CommandType.Text).ToList();

								//	Debug. WriteLine ( $"Generic query returned {objC . Count}" );
								//	List< CustomerViewModel > lo = new   List< CustomerViewModel >();
								//	// just gotta process the Enumbarable 
								//	foreach ( var  item in reslt )
								//	{
								//		genericlist . Add ( item );
								//	}
								//	return null;
								//}
								//else if ( typeof ( U ) == typeof ( ObservableCollection<DetailsViewModel> ) && typeof ( U ) != null )
								//{
								//	var reslt =
								//	db . Query <ObservableCollection<DetailsViewModel> >( SqlCommand, CommandType.Text).ToList();
								//	Debug. WriteLine ( $"Generic query returned {objD . Count}" );
								//	List< DetailsViewModel > lo = new   List< DetailsViewModel >();
								//	// just gotta process the Enumbarable 
								//	foreach ( var item in reslt )
								//	{
								//		genericlist . Add ( item );
								//	}
								//	return null;
								//}
							}
							else
							{
								//***************************************************************************************************************//
								// NO Full Generic type received, so we want the Generic structure returned
								//***************************************************************************************************************//
								if ( typeof ( T ) == typeof ( GenericClass ) )
								{
									var reslt = db . Query <T>( SqlCommand, CommandType.Text).ToList();
									List< BankAccountViewModel> lo = new   List< BankAccountViewModel>();
									// just gotta process the Enumbarable 
									//BankAccountViewModel bvm = new BankAccountViewModel();
									genericlist = reslt . Select ( r => new List<GenericClass> ( ) ) as List<GenericClass>;
									foreach ( var item in reslt )
									{
										lo . Add ( item as BankAccountViewModel );
										//genericlist . Add ( item );
									}

									//collection = lo as ObservableCollection < BankAccountViewModel >;
									return null;
								}
								else if ( typeof ( T ) == typeof ( BankAccountViewModel ) )
								{
									var reslt = db . Query <T>( SqlCommand, CommandType.Text).ToList();
									//									List< BankAccountViewModel> lo = new   List< BankAccountViewModel>();
									// just gotta process the Enumbarable 
									//									BankAccountViewModel bvm = new BankAccountViewModel();
									foreach ( var item in reslt )
									{
										//										lo . Add ( item as BankAccountViewModel );
										//genericlist . Add ( item );
									}
									return null;
								}
								else if ( typeof ( T ) == typeof ( CustomerViewModel ) )
								{
									var reslt = db . Query <T>( SqlCommand, CommandType.Text).ToList();
									//									List< CustomerViewModel > lo = new   List< CustomerViewModel >();
									// just gotta process the Enumbarable of the bankaccount table
									//									CustomerViewModel bvm = new CustomerViewModel ();
									foreach ( var item in reslt )
									{
										//										lo . Add ( item as CustomerViewModel );
										//genericlist . Add ( item );
									}
									return null;
								}
								else if ( typeof ( T ) == typeof ( DetailsViewModel ) )
								{
									var reslt = db . Query <T>( SqlCommand, CommandType.Text).ToList();
									//									List< DetailsViewModel > lo = new   List< DetailsViewModel >();
									// just gotta process the Enumbarable of the bankaccount table
									//									DetailsViewModel bvm = new DetailsViewModel ();
									foreach ( var item in reslt )
									{
										//										lo . Add ( item as DetailsViewModel );
										//genericlist . Add ( item );
									}
									return null;
								}
								else
								{
									//***************************************************************************************************************//
									// WE are just performing a (Non SELECT) style query and returning result in Generic structure
									//***************************************************************************************************************//
									var reslt = db . Query ( SqlCommand, CommandType.Text);
									//***************************************************************************************************************//

									if ( reslt == null )
									{
										errormsg = "DT";
										//return null;
									}
									else
									{
										//Although this is duplicated  with the one below we CANNOT make it a method()
										errormsg = "DYNAMIC";
										int dictcount = 0;
										int fldcount = 0;
										int colcount = 0;
										try
										{
											foreach ( dynamic item in reslt )
											{
												if ( typeof ( T ) == typeof ( ObservableCollection<GenericClass> ) )
												{
													var gc = new GenericClass ( );
													try
													{
														// we need to create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
														string buffer = "";
														gc = ParseDapperRowGen ( gc , dict , out colcount );
														// Create a dictionary entry for each record
#pragma warning disable CS0219 // The variable 'outstr' is assigned but its value is never used
														string outstr="";
#pragma warning restore CS0219 // The variable 'outstr' is assigned but its value is never used
														StringBuilder sb = new StringBuilder();
														int indx=2;
														colcount = 0;
														foreach ( dynamic item2 in item )
														{
															try
															{
																if ( item . Key == "" || item . Value == null )
																	break;
																dict . Add ( item . Key , item . Value );
															}
															catch ( Exception ex )
															{
																MessageBox . Show ( $"ParseDapper error was : \n{ex . Message}\nKey={item . Key} Value={item . Value . ToString ( )}" );
															}
														}
														colcount = indx;
														dictcount = 1;
														fldcount = dict . Count;
#pragma warning disable CS0219 // The variable 'index' is assigned but its value is never used
														int index = 0;
#pragma warning restore CS0219 // The variable 'index' is assigned but its value is never used
														string tmp="";
														foreach ( var pair in dict )
														{
															try
															{
																if ( pair . Key != null && pair . Value != null )
																{
																	DapperSupport . AddDictPairToGeneric<GenericClass> ( gc , pair , dictcount++ );
																	tmp = pair . Key . ToString ( ) + "=" + pair . Value . ToString ( );
																	buffer += tmp + ",";
																}
															}
															catch ( Exception ex )
															{
																Debug. WriteLine ( $"Dictionary ERROR : {ex . Message}" );
																result = ex . Message;
															}
														}
														//remove trailing comma
														string s = buffer . Substring (0, buffer . Length - 1 );
														buffer = s;
														//											genericlist . Add ( buffer );
													}
													catch ( Exception ex )
													{
														result = $"SQLERROR : {ex . Message}";
														Debug. WriteLine ( result );
													}

													objG . Add ( gc );
													dict . Clear ( );
													dictcount = 1;
												}
												else if ( typeof ( T ) == typeof ( ObservableCollection<BankAccountViewModel> ) )
												{
													// process it as a Bank style object (7 fields)
												}
											}
										}
										catch ( Exception ex )
										{
											Debug. WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
											result = ex . Message;
										}
										errormsg = $"DYNAMIC:{fldcount}";
										//								collection = objG;
										//								return collection;
										return objG;
									}
								}
							}
						}
						else
						{
							// probably a stored procedure ?  							
							bool IsSuccess=false;
							Debug. WriteLine ( $"Running SP db.Query" );

							//***************************************************************************************************************//
							//WORKING  JUST FINE FOR OBSERVABLECOLLECTION<GENERICCLASS>
							var reslt = db . Query ( SqlCommand ,
																			   null
																			   ,commandType: CommandType . StoredProcedure );
							//***************************************************************************************************************//
							// Generic trial 	     Both work as long as classes are marked as [Serializable]

							// How to test the type if a generic
							if ( typeof ( T ) == typeof ( ObservableCollection<GenericClass> ) )
							{
#pragma warning disable CS0219 // The variable 'k' is assigned but its value is never used
								int k = 0;
#pragma warning restore CS0219 // The variable 'k' is assigned but its value is never used
							}
							if ( reslt != null )
							{
								//Although this is duplicated  with the one above we CANNOT make it a method()
								int dictcount = 0;
								int fldcount = 0;
								int colcount=0;
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
											gc = ParseDapperRowGen ( item , dict , out colcount );
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
														DapperSupport . AddDictPairToGeneric<GenericClass> ( gc , pair , dictcount++ );
													}

												}
												catch ( Exception ex )
												{
													Debug. WriteLine ( $"Dictionary ERROR : {ex . Message}" );
													result = ex . Message;
												}
											}
											IsSuccess = true;
										}
										catch ( Exception ex )
										{
											result = $"SQLERROR : {ex . Message}";
											Debug. WriteLine ( result );
										}

										objG . Add ( gc );
										dict . Clear ( );
										dictcount = 1;
									}
									return objG;
								}
								catch ( Exception ex )
								{
									Debug. WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
									if ( ex . Message . Contains ( "not find stored procedure" ) )
									{
										result = $"SQL PARSE ERROR - [{ex . Message}]";
										errormsg = $"{result}";
									}
									else
									{
										long x= reslt.LongCount ();
										if ( x == ( long ) 0 )
										{
											result = $"ERROR : [{SqlCommand}] returned ZERO records... ";
											errormsg = $"DYNAMIC:0";
											return null;
										}
										else
										{
											result = ex . Message;
											errormsg = $"UNKNOWN :{ex . Message}";
										}
									}
								}
							}
							else
							{
								if ( IsSuccess == false )
									Debug. WriteLine ( $"Dapper request returned zero results" );
							}
						}
					}
					catch ( Exception ex )
					{
						Debug. WriteLine ( $"STORED PROCEDURE ERROR : {ex . Message}" );
						result = ex . Message;
						errormsg = $"SQLERROR : {result}";
					}
				}
				catch ( Exception ex )
				{
					Debug. WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
					result = ex . Message;
				}
			}
			return null;
		}

		#region support methods

		public static T1 GetInstance<T1> ( ) where T1 : new()
		{
			T1 instance = new T1();
			return instance;
		}

		// how to access <T> parameers in any generic method
		public static bool CallGeneric<T1> ( T1 arg )
		{

			Debug. WriteLine ( $"direct = [{arg}]\nstring() = [{arg . ToString ( )}]" );
			Debug. WriteLine ( $"test  : {arg}" );


			var obiG = GetInstance<ObservableCollection<GenericClass>> ( );

			// find out what we have got, & then we MUST process it inside the case structure
			//switch ( arg )
			//{
			//	//case ObservableCollection<GenericClass>:
			//	//	objG = arg as ObservableCollection<GenericClass>;
			//	//	ArgTpe = 1;
			//	//	break;
			//	//case GenericClass:
			//	//	objGclass = arg as GenericClass;
			//	//	ArgTpe = 2;
			//	//	break;
			//	//case List<string>:
			//	//	objL = arg as List<string>;
			//	//	ArgTpe = 3;
			//	//	break;
			//	//case ObservableCollection<BankAccountViewModel>:
			//	//	objB = arg as ObservableCollection<BankAccountViewModel>;
			//	//	ArgTpe = 4;
			//	//	break;
			//}
			return true;
		}
		public static GenericClass ParseDapperRowGen ( dynamic buff ,
			Dictionary<string , object> dict , out int colcount )
		{
			GenericClass GenRow = new GenericClass();
			int index=2;
			colcount = 0;
			foreach ( var item in buff )
			{
				try
				{
					if ( item . Key == "" || item . Value == null )
						break;
					dict . Add ( item . Key , item . Value );
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

		//public static T ParseDapperRowGeneric<T> ( T objtype , dynamic buff , Dictionary<string , object> dict , out int colcount )
		//{
		//	T GenRow= SqlServerCommands. deepClone ( objtype);
		//	int index=2;
		//	colcount = 0;
		//	foreach ( var item in buff )
		//	{
		//		try
		//		{
		//			if ( item . Key == "" || item . Value == null )
		//				break;
		//			dict . Add ( item . Key , item . Value );
		//		}
		//		catch ( Exception ex )
		//		{
		//			MessageBox . Show ( $"ParseDapper error was : \n{ex . Message}\nKey={item . Key} Value={item . Value . ToString ( )}" );
		//			break;
		//		}
		//	}
		//	colcount = index;
		//	return GenRow;
		//}


		/// <summary>
		///  Main call method to action a "Select" style SQL query and return the data in an ObservableCollection<GenericClass>  object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dict"></param>
		/// <param name="gclass"></param>
		/// <param name="SqlCommand"></param>
		/// <returns></returns>
		public static ObservableCollection<GenericClass> CreateFromDictionary<T1> (
			T1 dict ,
			GenericClass gclass ,
			string SqlCommand ,
			ref string errormsg
			)
		{
			ObservableCollection<GenericClass>objGclass = new ObservableCollection<GenericClass>();
			string ConString = Flags . CurrentConnectionString;
			if ( ConString == "" )
			{
				GenericDbUtilities . CheckDbDomain ( "IAN1" );
				ConString = Flags . CurrentConnectionString;
			}
			using ( IDbConnection db = new SqlConnection ( ConString ) )
			{
				//returns a Dictionary <str,str> for each field in the object
				try
				{
					var reslt = db . Query ( SqlCommand, CommandType.Text);
					foreach ( var item in reslt )
					{
						int columnCount  =1;
						gclass = new GenericClass ( );
						foreach ( var pair in item )
						{
							AddDictToGeneric ( ref gclass , pair , columnCount++ );
						}
						objGclass . Add ( gclass );
					}
				}
				catch ( Exception ex )
				{
					Debug. WriteLine ( $"SQL Db.Query error : [{ex . Message}]" );
					errormsg = ex . Message;
				}
			}
			return objGclass;
		}

		/// <summary>
		///	Called recursively to create and return a new GenericClass record from a single dictionary entry
		/// </summary>
		/// <param name="gc"></param>
		/// <param name="dict"></param>
		/// <param name="dictcount"></param>
		/// <returns></returns>
		public static GenericClass AddDictToGeneric ( ref GenericClass gc , KeyValuePair<string , object> dict , int dictcount )
		{
			switch ( dictcount )
			{
				case 1:
					gc . field1 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 2:
					gc . field2 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 3:
					gc . field3 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 4:
					gc . field4 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 5:
					gc . field5 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 6:
					gc . field6 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 7:
					gc . field7 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 8:
					gc . field8 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 9:
					gc . field9 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 10:
					gc . field10 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 11:
					gc . field10 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 12:
					gc . field12 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 13:
					gc . field13 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 14:
					gc . field14 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 15:
					gc . field15 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 16:
					gc . field16 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 17:
					gc . field17 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 18:
					gc . field18 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 19:
					gc . field19 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
				case 20:
					gc . field20 = dict . Value == null ? "" : dict . Value . ToString ( );
					break;
			}
			return gc;
		}
		#endregion support methods

	}
}
