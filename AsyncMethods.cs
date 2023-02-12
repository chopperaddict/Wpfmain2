using System;
using System . Collections . Generic;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;

using Dapper;

using Microsoft . Data . SqlClient;

using Wpfmain . Dapper;

using static Azure . Core . HttpHeader;

namespace Wpfmain
{
	public  class AsyncMethods
	{
		private async Task DoLoadDbTablesAsync ( )
		{   //  load list of Db Tables asynchronously
			List<string> TablesList = await Task . Run ( ( ) =>
			{
				return GetDbTablesListAsync ( "IAN1" );
			} );
//			return Task.FromResult;
		}
		#region Trigger methods  for Stored Procedures (string, Int, Double, Decimal) that return a List<xxxxx>
		// These all return just a single column from any table by calling a Stored Procedure  in MSSQL Server
		public static async Task<List<string>> CallStoredProcedureWithSizes ( List<string> list , string sqlcommand )
		{
			//This call returns us a DataTable
			DataTable dt = await GetDataTable(sqlcommand);
			if ( dt != null )
				list = await GetDataDridRowsWithSizes ( dt );
			//list = Utils . GetDataDridRowsAsListOfStrings ( dt );
			return list;
		}
		public static async Task<List<string>> CallStoredProcedure ( List<string> list , string sqlcommand )
		{
			//This call returns us a DataTable
			DataTable dt = await GetDataTable(sqlcommand);
			if ( dt != null )
				//				list = GenericDbHandlers.GetDataDridRowsWithSizes ( dt );
				list = await GetDataGridRowsAsListOfStrings ( dt );
			return list;
		}
		public static async Task<List<int>> CallStoredProcedure ( List<int> list , string sqlcommand )
		{
			//This call returns us a DataTable
			DataTable dt = await GetDataTable(sqlcommand);
			list = await GetDataDridRowsAsListOfInts ( dt );
			return list;
		}
		public static async Task<List<double>> CallStoredProcedure ( List<double> list , string sqlcommand )
		{
			//This call returns us a DataTable
			DataTable dt = await GetDataTable(sqlcommand);
			list = await GetDataDridRowsAsListOfDoubles ( dt );
			return list;
		}

		//***********************************************************//
		// Generic async Method that returns a DataTable  - Fast !!
		//***********************************************************//
		public static async Task<DataTable> GetDataTableUsingSqlCommand(string sqlcommand )
		{
			//This call returns us a DataTable
			DataTable dt = await GetDataTable(sqlcommand);
			return dt;
		}

		//***********************************************************//
		//***********************************************************//
		public static async Task<List<decimal>> CallStoredProcedure ( List<decimal> list , string sqlcommand )
		{
			//This call returns us a DataTable
			DataTable dt = await GetDataTable(sqlcommand);
			list = await GetDataDridRowsAsListOfDecimals ( dt );
			return list;
		}
		//***********************************************************//
		//***********************************************************//
		public static async Task<List<DateTime>> CallStoredProcedure ( List<DateTime> list , string sqlcommand )
		{
			//This call returns us a DataTable
			DataTable dt = await GetDataTable(sqlcommand);
			list = await GetDataDridRowsAsListOfDateTime ( dt );
			return list;
		}
		#endregion Trigger methods  for Stored Procedures

		#region Generic Sql Execute method - all data types

		//****************************//
		// DIRECT access Method
		//****************************//
		// Simplest data load method - return DataTable of any type
		//Accepts just a fully qualified SQL command string

		public static async Task<DataTable> GetDataTable ( string commandline )
		{
			DataTable dt = new DataTable();
			try
			{
				SqlConnection con;
				string ConString = Flags . CurrentConnectionString;
				con = new SqlConnection ( ConString );
				using ( con )
				{
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Db - {ex . Message}, {ex . Data}" );
				return null;
			}
			//Utils . trace ( "RunSqlCommand" );

			return dt;
		}
		
		#endregion Generic Sql Execute method - all data types

		#region ASYNC methods returning List<??> of various types from Inputs of Datatable
		static public async Task <List<string>> GetDbTablesListAsync ( string DbName )
		{
			List<string> TablesList = new List<string> ( );
			string SqlCommand = "";
			List<string> list = new List<string> ( );
			DbName = DbName . ToUpper ( );
			if ( DapperSupport . CheckResetDbConnection ( DbName , out string constr ) == false )
			{
				Debug . WriteLine ( $"Failed to set connection string for {DbName} Db" );
				return TablesList;
			}
			// All Db's have their own version of this SP.....
			SqlCommand = "spGetTablesList";

			CallStoredProcedure ( list , SqlCommand );
			//This call returns us a DataTable
			DataTable dt = await GetDataTable ( SqlCommand );
			// This how to access Row data from  a grid the easiest way.... parsed into a List <xxxxx>
			if ( dt != null )
			{
				TablesList = Utils . GetDataDridRowsAsListOfStrings ( dt );
			}
			return TablesList;
		}
		public static async Task<List<string>> GetDataDridRowsWithSizes ( DataTable dt )
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
			}
			return list;
		}
		public static async Task<List<DateTime>> GetDataDridRowsAsListOfDateTime ( DataTable dt )
		{
			List<DateTime> list = new List<DateTime>();
			foreach ( DataRow row in dt . Rows )
			{
				// ... Write value of first field as integer.
				list . Add ( row . Field<DateTime> ( 0 ) );
			}
			return list;
		}
		public static async Task<List<decimal>> GetDataDridRowsAsListOfDecimals ( DataTable dt )
		{
			List<decimal> list = new List<decimal>();
			foreach ( DataRow row in dt . Rows )
			{
				// ... Write value of first field as integer.
				list . Add ( row . Field<decimal> ( 0 ) );
			}
			return list;
		}
		public static async Task<List<int>> GetDataDridRowsAsListOfInts ( DataTable dt )
		{
			List<int> list = new List<int>();
			foreach ( DataRow row in dt . Rows )
			{
				// ... Write value of first field as integer.
				list . Add ( row . Field<int> ( 0 ) );
			}
			return list;
		}
		public static async Task<List<double>> GetDataDridRowsAsListOfDoubles ( DataTable dt )
		{
			List<double> list = new List<double>();
			foreach ( DataRow row in dt . Rows )
			{
				// ... Write value of first field as integer.
				list . Add ( row . Field<double> ( 0 ) );
			}
			return list;
		}
		public static async Task<List<string>> GetDataGridRowsAsListOfStrings ( DataTable dt )
		{
			List<string> list = new List<string>();
			foreach ( DataRow row in dt . Rows )
			{
				var txt = row.Field<string>(0);
				list . Add ( txt );
			}
			return list;
		}
		
		#endregion ASYNC methods returning List<??> of various types

	}
}
