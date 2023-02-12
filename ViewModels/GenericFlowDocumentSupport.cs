using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Documents;
using System . Windows . Media;
using System . Windows;
using System . Windows . Controls;

namespace Wpfmain . ViewModels
{
	public class GenericFlowDocumentSupport
	{
		public FlowDocument CreateTextString ( FlowDocument myFlowDocument, string SpText, string family = "Arial", string bground = "Black3", string fground = "White0", int fontsize = 16 )
		{
			string original = SpText;
			bool UsenewVerson = true;
			Brush brush;
			FontWeight fontweight = new ( );
			original = Utils2 . CopyCollection ( SpText, original );
			// Now create a (formatted) list of lines from all  paragraphs identified previously
			// using temp paragraph so I can access it from my public para variable
			Paragraph tmppara = new Paragraph ( );
			Run run1 = AddStdNewDocumentParagraph ( SpText, family, bground, fground, fontsize );
			tmppara . Inlines . Add ( run1 );
			Paragraph para1 = new ( );
			para1 = tmppara;
			para1 . Background = GetSolidColorBrush ( para1 . Background ,bground );
			para1 . Foreground = GetSolidColorBrush ( para1 . Foreground , fground );
			para1 . FontSize = fontsize;
			para1 . FontWeight = Utils2 . GetfontWeight ( "SemiBold" );
			para1 . FontFamily = new FontFamily ( family );
			para1 . FontWeight = FontWeights . Normal;
			// build  document by adding all blocks to Document
			myFlowDocument . Blocks . Clear ( );
			myFlowDocument . Blocks . Add ( para1 );
			myFlowDocument . Background = para1 . Background;
			//myFlowDocument . Background = Application . Current . FindResource ( bground ) as SolidColorBrush;
			//myFlowDocument . Foreground = Application . Current . FindResource ( fground ) as SolidColorBrush;
			return myFlowDocument;
		}
		public Run AddStdNewDocumentParagraph ( string textstring, string family, string bground, string fground, int fontsize )
		{
			// Main text
			SolidColorBrush scb = null;
			Run run1 = new Run ( textstring );
			run1 . FontSize = fontsize;
			run1 . FontFamily = new FontFamily ( family );
			run1 . FontWeight = FontWeights . Normal;
			run1 . Background = GetSolidColorBrush ( run1 . Background ,bground );
			run1 . Foreground = GetSolidColorBrush ( run1 . Background , fground );
			return run1;
		}
		public FlowDocument CreateBoldString ( FlowDocument myFlowDocument, string SpText, string SrchTerm )
		{
			string original = SpText;
			string originalSearchterm = "";
			original = Utils2 . CopyCollection ( SpText, original );
			string input = SpText . ToUpper ( );
			string [ ] NonSearchText;
			List<int> NonSearchTextlength = new List<int> ( );
			List<string> NonCapitlisedString = new List<string> ( );
			originalSearchterm = SrchTerm;
			int newpos = 0;
			if ( SrchTerm == null )
				SrchTerm = "";
			SrchTerm = SrchTerm . ToUpper ( );

			// split source down based on searchterm (using non capitalised version
			// // Only searchterm is capitalised !!!!))
			NonSearchText = input . Split ( $"{SrchTerm}" );
			foreach ( var item in NonSearchText )
			{
				NonSearchTextlength . Add ( item . Length );
			}
			for ( int x = 0 ; x < NonSearchTextlength . Count ; x++ )
			{
				string temp = original . Substring ( newpos, NonSearchTextlength [ x ] );
				NonCapitlisedString . Add ( temp );
				newpos += NonSearchTextlength [ x ] + SrchTerm . Length;
			}
			// Now create a (formatted) list of lines from all  paragraphs identified previously
			// using temp paragraph so I can access it from my public para variable
			Paragraph tmppara = new Paragraph ( );

			for ( int x = 0 ; x < NonCapitlisedString . Count ; x++ )
			{
				//Run run1 = AddStdNewDocumentParagraph ( NonCapitlisedString [ x ], SrchTerm );
				//tmppara . Inlines . Add ( run1 );
				//			Run run2 = AddDecoratedNewDocumentParagraph ( NonCapitlisedString [ x ], SrchTerm );

				//if ( x < NonCapitlisedString . Count - 1 )
				//	tmppara . Inlines . Add ( run2 );
			}
			Paragraph para1 = tmppara;
			SProcsHandling sp = SProcsHandling . spviewer;
			para1 . Background = sp . ScrollViewerBground;
			//para1 . Foreground = sp . ScrollViewerFground;
			// build  document by adding all blocks to Document
			myFlowDocument . Blocks . Add ( para1 );
			return myFlowDocument;
		}

		private Brush GetSolidColorBrush ( Brush run, string color )
		{
			// Get new SolidColorBrush for coloring text/background
			SolidColorBrush scb = null;
			string valid = "0123456789";
			string number = color . Substring ( color . Length - 1 );
			if ( valid . Contains ( number ) )
				run = Application.Current.FindResource ( color ) as SolidColorBrush;
			else{
				run = ( SolidColorBrush ) new BrushConverter ( ) . ConvertFromString ( color );
			}
			return run;
		}
	}
}
