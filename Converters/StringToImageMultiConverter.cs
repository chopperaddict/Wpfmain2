using System;
using System . Globalization;
using System . IO;
using System . Windows . Data;
using System . Windows . Media . Imaging;

namespace Wpfmain . Converters
{

    public class StringToImageMultiConverter : IMultiValueConverter
    {
        public static StringToImageMultiConverter Instance = new StringToImageMultiConverter ( );

        public object Convert ( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            if ( values[0] == null )
                return values[0];
            var path = ( string ) values[0];
            if ( path == null )
                return null;
            //var name = TreeViews. GetFileFolderName ( path );
            var image = "/icons/new.ico";

            if ( parameter != null )
            {
                if ( ( bool ) parameter == true )
                    image = "/icons/folder.gif";
            }
            FileInfo fi = new FileInfo ( path );
            FileAttributes fa = fi. Attributes;
            string attr = fa. ToString ( );
            if ( path. Length == 3 && path. Contains ( "\\" ) )
            {
                image = "/icons/folder.gif";
            }
            else if ( attr == "-1" )
                image = "/icons/folder-open.png";
            else if ( attr. Contains ( "Directory" ) )
            {
                image = "/icons/folder-open.png";
            }
            else
            {
                image = "/icons/templateicon.ico";    // its a fille
            }
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


        public object[] ConvertBack ( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}
