using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . Common;
using System . Diagnostics;
using System . DirectoryServices . ActiveDirectory;
using System . Linq;
using System . Security . Cryptography . Xml;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Controls . Primitives;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Media;
using System . Xml . Linq;

using Dapper;

using Microsoft . Data . SqlClient;

using ViewModels;

using Views;

using Wpfmain;
using Wpfmain . Models;

namespace SqlMethods
{
	static public class SqlDataMethods
	{
		static SProcsHandling  sph = SProcsHandling . GetSProcsHandling ( );

		#region Data loading to DataTable via Sql
		public static DataTable LoadBankData ( string Sqlcommand , int max = 0 , bool isMultiMode = false )
		//Load data from Sql Server
		{
			DataTable dtBank = new DataTable ( );
			try
			{
				SqlConnection con;
				string commandline = "";
				string ConString = Flags . CurrentConnectionString;
				ConString = ( string ) Wpfmain . Properties . Settings . Default [ "BankSysConnectionString" ];
				con = new SqlConnection ( ConString );
				using ( con )
				{
					if ( Flags . IsMultiMode )
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM BANKACCOUNT WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM BANKACCOUNT "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";
					}
					else if ( Flags . FilterCommand != "" )
					{
						commandline = Flags . FilterCommand;
					}
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = Sqlcommand;
					}
					SqlCommand cmd = new SqlCommand ( commandline , con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					if ( dtBank == null )
						dtBank = new DataTable ( );
					sda . Fill ( dtBank );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" );
				return null;
			}
			return dtBank;
		}

		public static DataTable LoadCustData ( string Sqlcommand , int max = 0 , bool isMultiMode = false )
		//Load data from Sql Server
		{
			SqlConnection con;
			DataTable dtCust = new DataTable ( );
			string ConString = Flags . CurrentConnectionString;
			ConString = ( string ) Wpfmain . Properties . Settings . Default [ "BankSysConnectionString" ];
			con = new SqlConnection ( ConString );
			try
			{
				using ( con )
				{
					string commandline = "";

					if ( Flags . IsMultiMode )
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM CUSTOMER WHERE CUSTNO IN "
							  + $"(SELECT CUSTNO FROM CUSTOMER  "
							  + $" GROUP BY CUSTNO"
							  + $" HAVING COUNT(*) > 1) ORDER BY ";
					}
					else if ( Flags . FilterCommand != "" )
					{
						commandline = Flags . FilterCommand;
					}
					else
					{
						// Create a valid Query Command string including any active sort ordering
						if ( max == 0 ) //&& bottomrec == 0 && toprec == 0 )
						{
							commandline = Sqlcommand;
							//							commandline = WpfLib1 . Utils .GetDataSortOrder ( commandline );
						}
					}
					SqlCommand cmd = new SqlCommand ( commandline , con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dtCust );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Customer Details - {ex . Message}, {ex . Data}" );
				return dtCust;
			}
			finally
			{
				con . Close ( );
			}

			return dtCust;
		}

		public static DataTable LoadDetailsData ( string Sqlcommand , int max = 0 , bool isMultiMode = false )
		{
			SqlConnection con;
			string filterline = "";
			DataTable dtDetails = new DataTable ( );
			string ConString = Flags . CurrentConnectionString;
			ConString = ( string ) Wpfmain . Properties . Settings . Default [ "BankSysConnectionString" ];

			con = new SqlConnection ( ConString );
			try
			{
				Debug . WriteLine ( $"Using new SQL connection in DETAILSCOLLECTION" );
				using ( con )
				{
					if ( Flags . IsMultiMode )
					{
						// Create a valid Query Command string including any active sort ordering
						filterline = $"SELECT * FROM SECACCOUNTS WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM SECACCOUNTS  "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";
						//						filterline = WpfLib1 . Utils .GetDataSortOrder ( filterline );
					}
					else if ( Flags . FilterCommand != "" )
					{
						filterline = Flags . FilterCommand;
					}
					else
					{
						// Create a valid Query Command string including any active sort ordering
						filterline = Sqlcommand;
						//						filterline = WpfLib1 . Utils .GetDataSortOrder ( filterline );
					}
					SqlCommand cmd = new SqlCommand ( filterline , con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dtDetails );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex . Message}, {ex . Data}" );
			}
			finally
			{
				con . Close ( );
			}
			return dtDetails;
		}

		public static DataTable LoadGenericData ( string Sqlcommand , string DbName = "" , int max = 0 , bool isMultiMode = false )
		{
			SqlConnection con;
			string filterline = "";
			DataTable dtGeneric = new DataTable ( );

			// This resets the current database connection - should be used anywhere that We switch between databases in Sql Server
			if ( DapperSupport . CheckResetDbConnection ( "IAN1" , out string constring ) == false )
			{
				Debug . WriteLine ( $"Failed to set connection string for {DbName . ToUpper ( )} Db" );
				return null;
			}
			filterline = Sqlcommand;
			string ConString = Flags . CurrentConnectionString;

			con = new SqlConnection ( ConString );
			try
			{
				Debug . WriteLine ( $"Using new SQL connection in LOADGENERICDATA" );
				using ( con )
				{
					SqlCommand cmd = new SqlCommand ( filterline , con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dtGeneric );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"GENERIC : ERROR in LoadGenericData(): Failed to load Generic Data :  {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"GENERIC: ERROR in LoadGenericData(): Failed to load Generic Data : {ex . Message}, {ex . Data}" );
			}
			finally
			{
				con . Close ( );
			}
			return dtGeneric;
		}
		#endregion Data loading to DataTable
		//********************************************************************************************************************************************************************************//
		#region Datatable loading from Datatables to collections

		public static ObservableCollection<BankAccountViewModel> LoadBankCollection ( DataTable dtBank , bool Notify = false )
		{
			int count = 0;
			ObservableCollection<BankAccountViewModel> bvm = new ObservableCollection<BankAccountViewModel> ( );
			try
			{
				for ( int i = 0 ; i < dtBank . Rows . Count ; i++ )
				{
					bvm . Add ( new BankAccountViewModel
					{
						Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ) ,
						BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ) ,
						CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ) ,
						AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ) ,
						Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ) ,
						IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ) ,
						ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ) ,
						CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ) ,
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"BANK : SQL Error in BankCollection(351) load function : {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANK : SQL Error in BankCollection (351) load function : {ex . Message}, {ex . Data}" );
			}
			finally
			{
				// This is ONLY called  if a requestor specifies the argument as TRUE
				if ( Notify )
				{
					Application . Current . Dispatcher . Invoke ( ( ) =>
					EventControl . TriggerBankDataLoaded ( null ,
						new LoadedEventArgs
						{
							CallerType = "SQLSUPPORT" ,
							DataSource = bvm ,
							RowCount = bvm . Count
						} )
					);
				}
			}
			return bvm;
		}
	
		public static ObservableCollection<CustomerViewModel> LoadCustomerCollection ( DataTable dtCust , bool Notify = false )
		{
			int count = 0;
			ObservableCollection<CustomerViewModel> cvm = new ObservableCollection<CustomerViewModel> ( );
			try
			{
				for ( int i = 0 ; i < dtCust . Rows . Count ; i++ )
				{
					cvm . Add ( new CustomerViewModel
					{
						Id = Convert . ToInt32 ( dtCust . Rows [ i ] [ 0 ] ) ,
						CustNo = dtCust . Rows [ i ] [ 1 ] . ToString ( ) ,
						BankNo = dtCust . Rows [ i ] [ 2 ] . ToString ( ) ,
						AcType = Convert . ToInt32 ( dtCust . Rows [ i ] [ 3 ] ) ,
						FName = dtCust . Rows [ i ] [ 4 ] . ToString ( ) ,
						LName = dtCust . Rows [ i ] [ 5 ] . ToString ( ) ,
						Addr1 = dtCust . Rows [ i ] [ 6 ] . ToString ( ) ,
						Addr2 = dtCust . Rows [ i ] [ 7 ] . ToString ( ) ,
						Town = dtCust . Rows [ i ] [ 8 ] . ToString ( ) ,
						County = dtCust . Rows [ i ] [ 9 ] . ToString ( ) ,
						PCode = dtCust . Rows [ i ] [ 10 ] . ToString ( ) ,
						Phone = dtCust . Rows [ i ] [ 11 ] . ToString ( ) ,
						Mobile = dtCust . Rows [ i ] [ 12 ] . ToString ( ) ,
						Dob = Convert . ToDateTime ( dtCust . Rows [ i ] [ 13 ] ) ,
						ODate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 14 ] ) ,
						CDate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 15 ] )
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"CUSTOMERS : ERROR {ex . Message} + {ex . Data} ...." );
				cvm = null;
			}
			finally
			{
				if ( Notify && count > 0 )
				{
					Debug . WriteLine ( $"Triggering event CustDataLoaded with {cvm . Count}" );
					Application . Current . Dispatcher . Invoke ( ( ) =>
					EventControl . TriggerCustDataLoaded ( null ,
						  new LoadedEventArgs
						  {
							  CallerType = "SQLSUPPPORT" ,
							  DataSource = cvm ,
							  RowCount = cvm . Count
						  } )
					);
				}
			}
			Debug . WriteLine ( $"Customers Db Total = {cvm?.Count}" );
			return cvm;
		}
	
		public static ObservableCollection<DetailsViewModel> LoadDetailsCollection ( DataTable dtDetails , bool Notify = false )
		{
			int count = 0;
			ObservableCollection<DetailsViewModel> dvm = new ObservableCollection<DetailsViewModel> ( );
			try
			{
				Debug . WriteLine ( $" Loading Datable with {dtDetails . Rows . Count} records" );
				dvm . Clear ( );
				for ( int i = 0 ; i < dtDetails . Rows . Count ; i++ )
				{
					dvm . Add ( new DetailsViewModel
					{
						Id = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 0 ] ) ,
						BankNo = dtDetails . Rows [ i ] [ 1 ] . ToString ( ) ,
						CustNo = dtDetails . Rows [ i ] [ 2 ] . ToString ( ) ,
						AcType = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 3 ] ) ,
						Balance = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 4 ] ) ,
						IntRate = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 5 ] ) ,
						ODate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 6 ] ) ,
						CDate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 7 ] ) ,
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				MessageBox . Show ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				return null;
			}
			finally
			{
				if ( Notify )
				{
					EventControl . TriggerDetDataLoaded ( null ,
						new LoadedEventArgs
						{
							CallerType = "SQLSERVER" ,
							DataSource = ( object ) dvm ,
							RowCount = dvm . Count
						} );
				}
			}
			Debug . WriteLine ( $" DETAILS DB Loading () ALL FINISHED :  Records = [{dvm . Count}]" );
			return dvm;
		}

		public static ObservableCollection<GenericClass> LoadGenericCollection ( DataTable dtgeneric , bool Notify = false )
		{
			int count = 0;
			ObservableCollection<GenericClass> gvm = new ObservableCollection<GenericClass> ( );
			try
			{
				Debug . WriteLine ( $" Loading Datable with {dtgeneric . Rows . Count} records" );
				gvm . Clear ( );
				int colcount = dtgeneric . Columns . Count;
				if ( colcount > 20 )
					colcount = 20;
				for ( int i = 0 ; i < dtgeneric . Rows . Count ; i++ )
				{
					switch ( colcount )
					{
						case 20:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
								field12 = dtgeneric?.Rows [ i ] [ 11 ] . ToString ( ) ,
								field13 = dtgeneric?.Rows [ i ] [ 12 ] . ToString ( ) ,
								field14 = dtgeneric?.Rows [ i ] [ 13 ] . ToString ( ) ,
								field15 = dtgeneric?.Rows [ i ] [ 14 ] . ToString ( ) ,
								field16 = dtgeneric?.Rows [ i ] [ 15 ] . ToString ( ) ,
								field17 = dtgeneric?.Rows [ i ] [ 16 ] . ToString ( ) ,
								field18 = dtgeneric?.Rows [ i ] [ 17 ] . ToString ( ) ,
								field19 = dtgeneric?.Rows [ i ] [ 18 ] . ToString ( ) ,
								field20 = dtgeneric?.Rows [ i ] [ 19 ] . ToString ( )
							} );
							break;
						case 19:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
								field12 = dtgeneric?.Rows [ i ] [ 11 ] . ToString ( ) ,
								field13 = dtgeneric?.Rows [ i ] [ 12 ] . ToString ( ) ,
								field14 = dtgeneric?.Rows [ i ] [ 13 ] . ToString ( ) ,
								field15 = dtgeneric?.Rows [ i ] [ 14 ] . ToString ( ) ,
								field16 = dtgeneric?.Rows [ i ] [ 15 ] . ToString ( ) ,
								field17 = dtgeneric?.Rows [ i ] [ 16 ] . ToString ( ) ,
								field18 = dtgeneric?.Rows [ i ] [ 17 ] . ToString ( ) ,
								field19 = dtgeneric?.Rows [ i ] [ 18 ] . ToString ( ) ,
							} );
							break;
						case 18:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
								field12 = dtgeneric?.Rows [ i ] [ 11 ] . ToString ( ) ,
								field13 = dtgeneric?.Rows [ i ] [ 12 ] . ToString ( ) ,
								field14 = dtgeneric?.Rows [ i ] [ 13 ] . ToString ( ) ,
								field15 = dtgeneric?.Rows [ i ] [ 14 ] . ToString ( ) ,
								field16 = dtgeneric?.Rows [ i ] [ 15 ] . ToString ( ) ,
								field17 = dtgeneric?.Rows [ i ] [ 16 ] . ToString ( ) ,
								field18 = dtgeneric?.Rows [ i ] [ 17 ] . ToString ( ) ,
							} );
							break;
						case 17:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
								field12 = dtgeneric?.Rows [ i ] [ 11 ] . ToString ( ) ,
								field13 = dtgeneric?.Rows [ i ] [ 12 ] . ToString ( ) ,
								field14 = dtgeneric?.Rows [ i ] [ 13 ] . ToString ( ) ,
								field15 = dtgeneric?.Rows [ i ] [ 14 ] . ToString ( ) ,
								field16 = dtgeneric?.Rows [ i ] [ 15 ] . ToString ( ) ,
								field17 = dtgeneric?.Rows [ i ] [ 16 ] . ToString ( ) ,
							} );
							break;
						case 16:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
								field12 = dtgeneric?.Rows [ i ] [ 11 ] . ToString ( ) ,
								field13 = dtgeneric?.Rows [ i ] [ 12 ] . ToString ( ) ,
								field14 = dtgeneric?.Rows [ i ] [ 13 ] . ToString ( ) ,
								field15 = dtgeneric?.Rows [ i ] [ 14 ] . ToString ( ) ,
								field16 = dtgeneric?.Rows [ i ] [ 15 ] . ToString ( ) ,
							} );
							break;
						case 15:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
								field12 = dtgeneric?.Rows [ i ] [ 11 ] . ToString ( ) ,
								field13 = dtgeneric?.Rows [ i ] [ 12 ] . ToString ( ) ,
								field14 = dtgeneric?.Rows [ i ] [ 13 ] . ToString ( ) ,
								field15 = dtgeneric?.Rows [ i ] [ 14 ] . ToString ( ) ,
							} );
							break;
						case 14:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
								field12 = dtgeneric?.Rows [ i ] [ 11 ] . ToString ( ) ,
								field13 = dtgeneric?.Rows [ i ] [ 12 ] . ToString ( ) ,
								field14 = dtgeneric?.Rows [ i ] [ 13 ] . ToString ( ) ,
							} );
							break;
						case 13:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
								field12 = dtgeneric?.Rows [ i ] [ 11 ] . ToString ( ) ,
								field13 = dtgeneric?.Rows [ i ] [ 12 ] . ToString ( ) ,
							} );
							break;
						case 12:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
								field12 = dtgeneric?.Rows [ i ] [ 11 ] . ToString ( ) ,
							} );
							break;
						case 11:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
								field11 = dtgeneric?.Rows [ i ] [ 10 ] . ToString ( ) ,
							} );
							break;
						case 10:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
								field10 = dtgeneric?.Rows [ i ] [ 9 ] . ToString ( ) ,
							} );
							break;
						case 9:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
								field9 = dtgeneric?.Rows [ i ] [ 8 ] . ToString ( ) ,
							} );
							break;
						case 8:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
								field8 = dtgeneric?.Rows [ i ] [ 7 ] . ToString ( ) ,
							} );
							break;
						case 7:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
								field7 = dtgeneric?.Rows [ i ] [ 6 ] . ToString ( ) ,
							} );
							break;
						case 6:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
								field6 = dtgeneric?.Rows [ i ] [ 5 ] . ToString ( ) ,
							} );
							break;
						case 5:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
								field5 = dtgeneric?.Rows [ i ] [ 4 ] . ToString ( ) ,
							} );
							break;
						case 4:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
								field4 = dtgeneric?.Rows [ i ] [ 3 ] . ToString ( ) ,
							} );
							break;
						case 3:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
								field3 = dtgeneric?.Rows [ i ] [ 2 ] . ToString ( ) ,
							} );
							break;
						case 2:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
								field2 = dtgeneric?.Rows [ i ] [ 1 ] . ToString ( ) ,
							} );
							break;
						case 1:
							gvm . Add ( new GenericClass
							{
								field1 = dtgeneric?.Rows [ i ] [ 0 ] . ToString ( ) ,
							} );
							break;
					}
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"GENERICS : ERROR in  LoadGenCollection() : loading Generic into ObservableCollection \"GenCollection\" : [{ex . Message}] : {ex . Data} ...." );
				//MessageBox . Show ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				//return null;
			}
			finally
			{
				if ( Notify )
				{
					EventControl . TriggerGenDataLoaded ( null ,
						new LoadedEventArgs
						{
							CallerType = "SQLSERVER" ,
							DataSource = ( object ) gvm ,
							RowCount = gvm . Count
						} );
				}
			}
			Debug . WriteLine ( $" DETAILS DB Loading () ALL FINISHED :  Records = [{gvm . Count}]" );
			return gvm;
		}

		#endregion Datatable loading from tables via SQL
		//********************************************************************************************************************************************************************************//
		/*
				SqlTable, 
				tablename , 
				ref SPDatagrid , 
				ref dglayoutlist ,
				ref VarCharLength , 
				"IAN1" );
*/
		public static List<Dictionary<string , string>> ReplaceDataGridFldNames (
			ObservableCollection<GenericClass> sqltable ,
			string tablename ,
			ref DataGrid Grid1 ,
			ref List<DataGridLayout> dglayoutlist ,
			ref List<int> VarCharLength ,
			int reccount ,
			string domain )
		{
			List<string> list = new List<string> ( );
			List<Dictionary<string , string>> ColumntypesList = new List<Dictionary<string , string>> ( );

			// This following code returns a Dictionary<sting,string> PLUS a collection  and a List<string> passed by ref....
			string currdomain = Flags . DbDomain;
			if ( currdomain != domain )
				currdomain = "IAN1";
			// clear reference sturcture first off
			dglayoutlist . Clear ( );

			ColumntypesList = GetFullSqlTableInfo ( sqltable , Grid1 , tablename , currdomain , ref list , ref VarCharLength , ref dglayoutlist );
			return ColumntypesList;
		}

		static public List<Dictionary<string , string>> GetFullSqlTableInfo (
			System . Collections . ObjectModel . ObservableCollection<ViewModels .
			GenericClass> sqltable ,
			DataGrid Grid1 ,
			string tablename ,
			string domain ,
			ref List<string> list ,
			ref List<int> VarCharLength ,
			ref List<DataGridLayout> dglayoutlist )
		{

			Dictionary<string , string> dict = new Dictionary<string , string> ( );
			List<Dictionary<string , string>> ColumntypesList = new List<Dictionary<string , string>> ( );
			// Make sure we are accessing the correct Db Domain
			DapperSupport . CheckDbDomain ( domain );
			"" . Track ( 0 );

			//		ALL ref arguments should now be filled after this method has returned
			dict = GetSpArgs ( sqltable , ref list , tablename , domain , ref VarCharLength , ref dglayoutlist );

			// GenClass & dglayoutlist are now fully populated        
			int index = 0;
			// Add header detail held in GenClass retrieved above
			if ( sqltable . Count > 0 )
			{
				index = 0;
				GenericClass gc = new GenericClass ( );
				foreach ( var item in list )
				{
					try
					{
						gc = sqltable [ index ];
						DataGridTextColumn dgtcol = new ( );
						dgtcol . Header = dglayoutlist [ index ] . Fieldname;
						dgtcol . Binding = new Binding ( dglayoutlist [ index ] . Fieldname );
						Grid1 . Columns . Add ( dgtcol );
					}
					catch ( ArgumentOutOfRangeException ex ) { Debug . WriteLine ( $"TODO - BAD Columns - 300 GenericDbHandlers.cs" ); }
					index++;
				}
				ColumntypesList . Add ( dict );
			}
			"" . Track ( 1 );
			return ColumntypesList;
		}
		public static DataGridColumnHeader GetHeader ( DataGridColumn column , DependencyObject reference )
		{
			for ( int i = 0 ; i < VisualTreeHelper . GetChildrenCount ( reference ) ; i++ )
			{
				DependencyObject child = VisualTreeHelper . GetChild ( reference , i );

				DataGridColumnHeader colHeader = child as DataGridColumnHeader;
				if ( ( colHeader != null ) && ( colHeader . Column == column ) )
				{
					return colHeader;
				}

				colHeader = GetHeader ( column , child );
				if ( colHeader != null )
				{
					return colHeader;
				}
			}
			return null;
		}

		public static Dictionary<string , string> GetDbTableColumns<GenericClass> (
			 System . Collections . ObjectModel . ObservableCollection<ViewModels . GenericClass> Gencollection ,
			ref List<string> list ,
			string dbName ,
			string DbDomain ,
			ref List<int> VarCharLength ,
			ref List<DataGridLayout> dglayoutlist )
		{
			"" . Track ( 0 );
			Dictionary<string , string> dict = new ( );
			// Make sure we are accessing the correct Db Domain
			DapperSupport . CheckDbDomain ( DbDomain );
			// on return, list contains all column names, VarCharLength contains total column count for VARCHARs's (only)
			//dglayoutlist will contain ALL NEEDED column information
			dict = GetSpArgs ( Gencollection , ref list , dbName , DbDomain , ref VarCharLength , ref dglayoutlist );
			"" . Track ( 1 );
			return dict;
		}

		public static Dictionary<string , string> GetSpArgs (
			ObservableCollection<GenericClass> Gencollection ,
			ref List<string> list ,
			string dbName ,
			string DbDomain ,
			ref List<int> VarCharLength ,
			ref List<DataGridLayout> dglayoutlist )
		{
			"" . Track ( 0 );
			DataTable dt = new DataTable ( );
			Dictionary<string , string> dict = new Dictionary<string , string> ( );
			try
			{
				// Make sure we are accessing the correct Db Domain
				DapperSupport . CheckDbDomain ( DbDomain );
				// get a GenericClass structure (Sprocs) holding table name, ALL field names, sizes, decimal parts, types etc
				SProcsHandling . Sprocs = LoadDbAsGenericData ( ref list , "spGetTableColumnWithSizes" , dbName , DbDomain , ref VarCharLength , false );
			}
			catch ( Exception ex )
			{
				MessageBox . Show ( $"SQL ERROR 1125 - {ex . Message}" );
				dict . Clear ( );
				"" . Track ( 1 );
				return dict;
			}
			// We do have all column info, but Datagrid is NOT populated, AND it has NO column header YET
			// so lets add them now !
			dict = CreateNewColumnHeaders ( SProcsHandling . Sprocs , list , dglayoutlist );
			"" . Track ( 1 );
			return dict;
		}

		static public Dictionary<string , string> CreateNewColumnHeaders (
			ObservableCollection<GenericClass> Gencollection ,
			List<string> list ,
			List<DataGridLayout> dglayoutlist )
		{
			Dictionary<string , string> dict = new ( );
			"" . Track ( 0 );

			dict . Clear ( );
			list . Clear ( );
			dglayoutlist . Clear ( );
			try
			{
				// GenCollection contains all te column information - NOT SQL DATA
				foreach ( var item in Gencollection )
				{
					DataGridLayout dglayout = new ( );

					GenericClass gc = new GenericClass ( );
					gc = item as GenericClass;
					//	save field name and type to dictionary and list
					dict . Add ( gc . field2 , gc . field3 );
					// create storage in dglayout & add to dglayoutlist for all column header info from data recieved from SP: spGetTableColumnWithSizes
					dglayout . Fieldname = gc . field2;
					dglayout . Fieldtype = gc . field3;
					dglayout . Fieldlength = Convert . ToInt32 ( gc . field4 );
					dglayout . Fielddec = Convert . ToInt32 ( gc . field5 );
					dglayout . Fieldpart = Convert . ToInt32 ( gc . field6 );
					dglayoutlist . Add ( dglayout );
					// store fldnames into list
					list . Add ( gc . field2 );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( ex . Message );
			}
			"" . Track ( 1 );
			return dict;
		}

		/// <summary>
		/// Returns  a GENERIC collection, plus a List<string> using an SP
		/// </summary>
		/// <param name="GenClass"></param>
		/// <param name="list"></param>
		/// <param name="SqlCommand"></param>
		/// <param name="Arguments"></param>
		/// <param name="DbDomain"></param>
		/// <returns></returns>
		public static ObservableCollection<GenericClass> LoadDbAsGenericData (
					ref List<string> list ,
					string SqlCommand ,
					string Arguments ,
					string DbDomain ,
					ref List<int> VarCharLength ,
					bool GetLengths = false )
		{
			string result = "";
			string arg1 = "", arg2 = "", arg3 = "", arg4 = "";
			// provide a default connection string
			string ConString = "ConnectionString";
			Dictionary<string , object> dict = new Dictionary<string , object> ( );
			ObservableCollection<GenericClass> GenClass = new ObservableCollection<GenericClass> ( );

			"" . Track ( 0 );

			// Ensure we have the correct connection string for the current Db Doman
			DapperSupport . CheckResetDbConnection ( DbDomain , out string constr );
			Flags . CurrentConnectionString = constr;
			ConString = constr;

			using ( IDbConnection db = new SqlConnection ( ConString ) )
			{
				try
				{
					// Use DAPPER to run  Stored Procedure
					// One or No arguments
					arg1 = Arguments;
					if ( arg1 . Contains ( "," ) )              // trim comma off
						arg1 = arg1 . Substring ( 0 , arg1 . Length - 1 );
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

					//***************************************************************************************************************//
					// This returns the data from SP commands (only) in a GenericClass Structured format
					var reslt = db . Query ( SqlCommand , Params , commandType: CommandType . StoredProcedure );
					//***************************************************************************************************************//

					if ( reslt != null )
					{
						//Although this is duplicated  with the one above we CANNOT make it a method()
						int dictcount = 0;
						bool IsSuccess = false;

						dict . Clear ( );
						long zero = reslt . LongCount ( );
						try
						{
							int colcount = 0, fldcount = 0;
							foreach ( var item in reslt )
							{
								GenericClass gc = new GenericClass ( );
								try
								{
									//	Create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
									gc = DapperSupport . ParseDapperRow ( item , dict , out colcount , ref VarCharLength , GetLengths );
									//VarcharList . Add ( VarCharLength );
									dictcount = 1;
									fldcount = dict . Count;
									if ( fldcount == 0 )
									{
										//no problem, we will get a Datatable anyway
										return GenClass;
									}
									string buffer = "", tmp = "";
									foreach ( var pair in dict )
									{
										try
										{
											if ( pair . Key != null && pair . Value != null )
											{
												DapperSupport . AddDictPairToGeneric ( gc , pair , dictcount++ );
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
									//string s = buffer . Substring (0, buffer . Length - 1 );
									//buffer = s;
									//genericlist . Add ( buffer );
								}
								catch ( Exception ex )
								{
									result = $"SQLERROR : {ex . Message}";
									Debug . WriteLine ( result );
									"" . Track ( 1 );
									return GenClass;
								}
								//										gc . ActiveColumns = dict . Count;
								//ParseListToDbRecord ( genericlist , out gc );
								GenClass . Add ( gc as GenericClass );
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
								"" . Track ( 1 );
								return GenClass;
							}
							else
							{
								long x = reslt . LongCount ( );
								if ( x == ( long ) 0 )
								{
									result = $"ERROR : [{SqlCommand}] returned ZERO records... ";
									"" . Track ( 1 );
									return GenClass;
								}
								else
								{
									result = ex . Message;
									//									errormsg = $"UNKNOWN :{ex . Message}";
								}
								"" . Track ( 1 );
								return GenClass;
							}
						}
					}
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}" );
				}
			}
			"" . Track ( 1 );
			return GenClass;
		}

		/// <summary>
		/// CLEVER METHOD
		/// This Method recieves a datagrid prefilled with GENERIC STYLE data (field1, field2, etc)
		/// and then uses the spGetTableNames S.Proc toi get the real field names
		/// and replaces the column headers with them instead, assuming they are retrieved successfully
		/// </summary>
		/// <param name="CurrentType"></param>
		/// <param name="Grid1"></param>
		public static void ReplaceDataGridFldNames ( 
			string CurrentType , 
			ref DataGrid Grid1 , 
			ref List<int> VarCharLength , 
			ref List<DataGridLayout> dglayoutlist , 
			string Domain = "IAN1" )
		{
			List<string> list = new List<string> ( );
			ObservableCollection<GenericClass> Genericclass = new ObservableCollection<GenericClass> ( );
			Dictionary<string , string> dict = new Dictionary<string , string> ( );

			"" . Track ( 0 );
			// ???????????????????????????????????????????????????????????
			// This returns a Dictionary<sting,string> PLUS a collection  and a List<string> passed by ref....
			dict = GetDbTableColumns<GenericClass> ( Genericclass , ref list , CurrentType , Domain , ref VarCharLength , ref dglayoutlist );
			int index = 0;
			// Add data  for field size
			foreach ( var item in Genericclass )
			{
				//save column width to column 3 ????
				item . field3 = VarCharLength [ index++ ] . ToString ( );
			}
			if ( list . Count > 0 )
			{
				index = 0;
				// use the list to get the correct column header info
				foreach ( var item in Grid1 . Columns )
				{
					DataGridColumn dgc = item;
					try
					{
						dgc . Header = list [ index++ ];
						//dgc . Width = Convert.ToInt32(GenericClass [ 3 ] . field3);
						if ( index >= dict . Count )
						{
							break;
						}
					}
					catch ( ArgumentOutOfRangeException ex ) { Debug . WriteLine ( $"TODO - BAD Columns - 300 GenericDbHandlers.cs" ); }
				}
			}
			"" . Track ( 1 );
		}

		public static List<string> GetDataDridRowsWithSizes ( DataTable dt )
		{
			List<string> list = new List<string> ( );
			foreach ( DataRow row in dt . Rows )
			{
				var txt = row . Field<string> ( 0 );
				list . Add ( txt );
				txt = row . Field<string> ( 1 );
				list . Add ( txt );
				if ( row . Field<object> ( 2 ) != null )
				{
					txt = row . Field<object> ( 2 ) . ToString ( );
					list . Add ( txt );
				}
				else
					list . Add ( "---" );
				//txt = row . Field<string> ( 1 );
				//list . Add ( txt);
				//object obj = row . Field<object>(0);
				//if( obj == typeof ( Int16 ) || obj == typeof ( Int32 ) || obj == typeof ( int ) )
				//	list . Add ( obj.ToString() );
			}
			return list;
		}

	}
}
