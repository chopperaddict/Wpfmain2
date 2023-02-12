using System;
using System . Diagnostics;

namespace Wpfmain . SqlBasicSupport
{
	public class SqlSupport
	{
		public static string LoadConnectionStrings ( )
		{
			string cstr ="";
			try
			{
				if ( Flags . ConnectionStringsDict . Count > 0 )
				{
					string str = (string)Utils.ReadConfigSetting("ConnectionString");
					return str;
				}
				//                cstr = Utils . ReadConfigSetting ( "ConnectionString" );
				cstr = Utils . ReadConfigSetting ( "ConnectionString" );
				Flags . ConnectionStringsDict . Add ( "IAN1" , cstr );
				//ConnectionStringsDict . Add ( "IAN1" , Utils . ReadConfigSetting ( "ConnectionString" ) );
				//                ConnectionStringsDict . Add("IAN2", Utils.ReadConfigSetting("Constring"));
				//ConnectionStringsDict.Add("PUBS", Utils.ReadConfigSetting("PubsConnectionString"));
				//Utils . WriteSerializedCollectionJSON ( ConnectionStringsDict , @"C:\users\ianch\DbConnectionstrings.dat" );
				// string connstr = $@"Data Source=DESKTOP-BEQTQ1J; Initial Catalog="IAN1"; Integrated Security=True; Connect Timeout=30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False"
			}
			catch ( NullReferenceException ex )
			{
				string s = Utils.ReadConfigSetting("ConnectionString");
				Debug . WriteLine ( $"Dictionary  entrry [{s}] already exists" );
			}
			finally
			{
			}
			return cstr;
		}

	}
}


