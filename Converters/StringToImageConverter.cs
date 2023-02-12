using System;
using System . Globalization;
using System . Windows . Data;
using System . Windows . Media . Imaging;

using Wpfmain . Models;

namespace Wpfmain . Converters
{
	/// <summary>
	/// Special converter for my Viewmodel Treeview ONLY
	/// </summary>
	[ValueConversion ( typeof ( DirectoryItemType ) , typeof ( BitmapImage ) )]
	public class StringToImageConverter : IValueConverter
	{
		public static StringToImageConverter Instance = new StringToImageConverter();
		public object Convert ( object value , Type targetType , object parameter , CultureInfo culture )
		{
			//if ( value == null )
			//	return value;
			//var path = (string)value;
			//if ( path == null )
			//	return null;
//			var name = TreeViews.GetFileFolderName(path);
			var image = "/icons/new.ico";
			switch ((DirectoryItemType)value)
			{
				case DirectoryItemType . Drive:
					image = "/icons/folder.gif";
					break;
				case DirectoryItemType . Folder:
					image = "/icons/folder-open.png";
					break;
				case DirectoryItemType . File:
					 image = "/icons/docs.ico";
					break;

			}
			//if((bool)parameter == true)
			//	image = "/icons/folder.gif";
			//FileInfo fi = new FileInfo(path);
			//FileAttributes fa =  fi . Attributes;
			//string attr = fa . ToString ( );
			//if (path.Length == 3  && path.Contains("\\" ))
			//{
			//}
			//else if ( attr . Contains ( "Directory" ) )
			//{
			//}
			//else
			//{
			//}
			//if ( string . IsNullOrEmpty ( name ) ) // must be a drive
			//	image = "/icons/new.ico";
			//else if ( name . Contains ( "." ) )
			//	image = "/icons/templateicon.ico";    // its a fille
			//else
			//	image = "/icons/folder-open.png";
			//File alone
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
