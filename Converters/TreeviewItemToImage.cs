using ViewModels;

using System;
using System . Collections . Generic;
using System . Globalization;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;
using System . Windows . Media . Imaging;

namespace Wpfmain . Converters
{
	/// </summary>
	[ValueConversion ( typeof ( string ) , typeof ( BitmapImage ) )]
	public class TreeviewItemToImage : IValueConverter
	{
		public static TreeviewItemToImage Instance = new TreeviewItemToImage ( );
		public object Convert ( object value , Type targetType , object parameter , CultureInfo culture )
		{
			if ( value == null )
				return value;
			string valstr = value . ToString ( );
			var path = valstr;
			if ( path == null )
				return null;
			//			var name = TreeViews.GetFileFolderName(path);
			var image = "/icons/new.ico";
			//switch ( ( DirectoryItemType ) value )
			//{
			//	case DirectoryItemType . Drive:
			//		image = "/icons/folder.gif";
			//		break;
			//	case DirectoryItemType . Folder:
			//		image = "/icons/folder-open.png";
			//		break;
			//	case DirectoryItemType . File:
			//		image = "/icons/new.ico";
			//		break;

			//}
			if ( parameter != null)
			{
				if ( ( bool ) parameter == true )
					image = "/icons/folder.gif";
			}
			FileInfo fi = new FileInfo(path);
			FileAttributes fa =  fi . Attributes;
			string attr = fa . ToString ( );
			if ( path . Length == 3 && path . Contains ( "\\" ) )
			{
				image = "/icons/folder.gif";
			}
			else if ( attr . Contains ( "Directory" ) )
			{
				image = "/icons/blue folder open.png";
			}
			else
			{
				image = "/icons/blank doc.png";
			}
			Uri uri = new Uri ( $"pack://application:,,,{image}" );
			BitmapImage source = new BitmapImage ( uri );
			return source;
		}

		public object ConvertBack ( object value , Type targetType ,
		    object parameter , CultureInfo culture )
		{
			throw new NotSupportedException ( "Cannot convert back" );
		}
	}
}
