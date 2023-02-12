using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . IO;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;

using Wpfmain . UserControls;
using Wpfmain . ViewModels;

namespace Wpfmain . UtilWindows
{
	/// <summary>
	/// Interaction logic for InfoWindow.xaml
	/// </summary>
	public partial class InfoWindow : Window
	{
		#region PropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged ( string propertyName )
		{
			if ( this . PropertyChanged != null )
			{
				var e = new PropertyChangedEventArgs ( propertyName );
				this . PropertyChanged ( this, e );
			}
		}
		#endregion PropertyChanged

		#region full properties

		private ObservableCollection<string> _HelpTopics;
		public ObservableCollection<string> HelpTopics
		{
			get { return _HelpTopics; }
			set { _HelpTopics = value; OnPropertyChanged ( "HelpTopics" ); }
		}

		#endregion full properties

		private FlowDocument myFlowDocument = new FlowDocument ( );
		private string CurrentFile { get; set; } = "";
		private int helpfileindex { get; set; } = 1;
		private string CurrentTextColor { get; set; } = "PureWhite";
		private string CurrentBackColor { get; set; } = "DarkBlack";
		private int CurrentFontsize { get; set; } = 16;
		private bool IsLoading { get; set; } = true;
		public PopupUControl upopup { get; set; }

		private Dictionary<string, string> Brushcolors = new ( );
		public ObservableCollection<string> FontSizes { get; set; }
		public InfoWindow infoWin { get; set; }
		public string popuptext { get; set; } = "";

		public InfoWindow ( int fileindex )
		{
			InitializeComponent ( );
			this . DataContext = this;
			HelpTopics = new ( );
			FontSizes = new ( );
			helpfileindex = fileindex;
			infoWin = this;
			this . DataContext = this;
		}

		private void InformationWindow_Loaded ( object sender, RoutedEventArgs e )
		{
			CreateListboxTopics ( );
			LoadColorsCombo ( );
			CreateFontSizes ( );

			FontsizeCombo . SelectedIndex = 5;
			FontsizeCombo . SelectedItem = 5;
			TextColorsCombo . SelectedIndex = 18;

			//FontsizeCombo . UpdateLayout ( );
			//IsLoading = false;
			TopicsListbox . SelectedIndex = helpfileindex - 1;  // 16
			TopicsListbox . SelectedItem = helpfileindex - 1;  // 16
			TopicsListbox . UpdateLayout ( );
			popuptext = $"To change the font size of this viewer, either use the dropdown in this window, or Ctrl + Mouse Scroll Wheel.\nLeft Click inside the viewer pane or hit ESC or use  button to close this popup.";
		}

		private void CreateListboxTopics ( )
		{
			HelpTopics . Add ( "Overview of this windows functionality" );
			HelpTopics . Add ( "Using the SQL Table Access System" );
			HelpTopics . Add ( "Using the S.Procedures Access System" );
		}
		private void CreateFontSizes ( )
		{
			for ( int x = 11 ; x < 24 ; x++ )
			{
				if ( x < 16 && x % 2 == 0 )
					FontSizes . Add ( $"{x}" );
				else
					FontSizes . Add ( $"{x}" );
			}
		}

		private void CloseBtn_Click ( object sender, RoutedEventArgs e )
		{
			this . Close ( );
		}

		private void DoHandled ( object sender, MouseButtonEventArgs e )
		{
		}

		private void HelpListbox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
			{
				IsLoading = false;
				TopicsListbox . SelectedIndex = helpfileindex - 1;
			}
			if ( TopicsListbox . SelectedIndex != -1 )
			{
				CurrentFile = "No existing document found !!";
				string selection = TopicsListbox . SelectedItem . ToString ( );

				if ( selection . Contains ( "Overview of this windows functionality" ) )
				{
					string filename = @$"C:\MyDocumentation\OverviewHelp.txt";
					CurrentFile = File . ReadAllText ( filename );
				}
				else if ( selection . Contains ( "Using the SQL Table Access System" ) )
				{
					string filename = @$"C:\MyDocumentation\SqlAccessHelp.txt";
					CurrentFile = File . ReadAllText ( filename );
				}
				else if ( selection . Contains ( "Using the S.Procedures Access System" ) )
				{
					string filename = @$"C:\MyDocumentation\StoredProcsHelp.txt";
					CurrentFile = File . ReadAllText ( filename );
				}
				string newcolor = "";
				string newbackcolor = "";
				Brushcolors . TryGetValue ( CurrentTextColor, out newcolor );
				Brushcolors . TryGetValue ( CurrentBackColor, out newbackcolor );
				myFlowDocument = CreateFlowDocument ( CurrentFile, "Arial", newbackcolor, newcolor, CurrentFontsize );
				InfoPane . Document = myFlowDocument;
				//InfoPane . Refresh ( );
				InfoPane . UpdateLayout ( );
				e . Handled = true;
			}
			IsLoading = false;
		}
		public FlowDocument CreateFlowDocument ( string data, string family = "Nirmala UI", string bcolor = "Black0", string fcolor = "White0", int fsize = 16 )
		{
			myFlowDocument = new FlowDocument ( );
			myFlowDocument . Blocks . Clear ( );
			GenericFlowDocumentSupport sps = new ( );
			myFlowDocument = sps . CreateTextString ( myFlowDocument, data, family, bcolor, fcolor, fsize );
			return myFlowDocument;
		}
		private void FsizeCombo_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			ComboBox cb = sender as ComboBox;
			int size = Convert . ToInt32 ( cb . SelectedItem );
			CurrentFontsize = size;
			myFlowDocument = CreateFlowDocument (
				 CurrentFile,
				"Nirmala UI",
			   GetValidColorString ( CurrentBackColor ),
				GetValidColorString ( CurrentTextColor ),
				CurrentFontsize );
			InfoPane . Document = myFlowDocument;
			InfoPane . Refresh ( );
			InfoPane . UpdateLayout ( );
		}
		public void LoadColorsCombo ( )
		{
			//"" . Track ( 0 );
			Brushcolors . Clear ( );
			SProcsHandling sp = SProcsHandling . spviewer;
			List<string> colors = new ( );
			if ( colors . Count > 0 )
				colors . Clear ( );
			colors . Add ( ( "LightBlack" ) );
			Brushcolors . Add ( "LightBlack", "Black4" );
			colors . Add ( ( "DarkBlack" ) );
			Brushcolors . Add ( "DarkBlack", "Black0" );
			colors . Add ( ( "LightGray" ) );
			Brushcolors . Add ( "LightGray", "Gray4" );
			colors . Add ( ( "DarkGray" ) );
			Brushcolors . Add ( "DarkGray", "Gray0" );
			colors . Add ( ( "LightBlue" ) );
			Brushcolors . Add ( "LightBlue", "Blue4" );
			colors . Add ( ( "DarkBlue" ) );
			Brushcolors . Add ( "DarkBlue", "Blue1" );
			colors . Add ( ( "LightCyan" ) );
			Brushcolors . Add ( "LightCyan", "Cyan0" );
			colors . Add ( ( "DarkCyan" ) );
			Brushcolors . Add ( "DarkCyan", "Cyan4" );
			colors . Add ( ( "LightRed" ) );
			Brushcolors . Add ( "LightRed", "Red4" );
			colors . Add ( ( "DarkRed" ) );
			Brushcolors . Add ( "DarkRed", "Red1" );
			colors . Add ( ( "LightGreen" ) );
			Brushcolors . Add ( "LightGreen", "Green6" );
			colors . Add ( ( "DarkGreen" ) );
			Brushcolors . Add ( "DarkGreen", "Green1" );
			colors . Add ( ( "LightPurple" ) );
			Brushcolors . Add ( "LightPurple", "Purple6" );
			colors . Add ( ( "DarkPurple" ) );
			Brushcolors . Add ( "DarkPurple", "Purple1" );
			colors . Add ( ( "LightOrange" ) );
			Brushcolors . Add ( "LightOrange", "Orange4" );
			colors . Add ( ( "DarkOrange" ) );
			Brushcolors . Add ( "DarkOrange", "Orange2" );
			colors . Add ( ( "LightYellow" ) );
			Brushcolors . Add ( "LightYellow", "Yellow0" );
			colors . Add ( ( "DarkYellow" ) );
			Brushcolors . Add ( "DarkYellow", "Yellow4" );
			colors . Add ( ( "PureWhite" ) );
			Brushcolors . Add ( "PureWhite", "White0" );
			colors . Add ( ( "DarkWhite" ) );
			Brushcolors . Add ( "DarkWhite", "White4" );
			colors . Add ( ( "LightMagenta" ) );
			Brushcolors . Add ( "LightMagenta", "Magenta4" );
			colors . Add ( ( "DarkMagenta" ) );
			Brushcolors . Add ( "DarkMagenta", "Magenta1" );
			TextColorsCombo . ItemsSource = colors;
			TextColorsCombo . SelectedIndex = 8;
			backgroundCombo . ItemsSource = colors;
			backgroundCombo . SelectedIndex = 1;
			//"" . Track ( 1 );
		}

		private void TextColorsCombo_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			ComboBox cb = sender as ComboBox;
			if ( IsLoading )
				return;
			int color = cb . SelectedIndex;

			string fcolor = cb . SelectedItem . ToString ( );
			string newcolor = "";
			Brushcolors . TryGetValue ( fcolor, out newcolor );
			if ( newcolor != null )
				CurrentTextColor = fcolor;
			else
			{
				Utils . DoErrorBeep ( );
				return;
			}
			myFlowDocument = CreateFlowDocument (
				 CurrentFile,
				fcolor: GetValidColorString ( fcolor ),
				bcolor: GetValidColorString ( CurrentBackColor ),
				fsize: CurrentFontsize );
			InfoPane . Document = myFlowDocument;
			InfoPane . Background = FindResource ( GetValidColorString ( CurrentBackColor ) ) as SolidColorBrush;
			//			InfoPane . Refresh ( );
			InfoPane . UpdateLayout ( );
		}

		private void bgoundCombo_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			ComboBox cb = sender as ComboBox;
			if ( IsLoading )
				return;
			int color = cb . SelectedIndex;

			string fcolor = cb . SelectedItem . ToString ( );
			string newcolor = "";
			Brushcolors . TryGetValue ( fcolor, out newcolor );
			if ( newcolor != null )
			{
				CurrentBackColor = fcolor;
				myFlowDocument = CreateFlowDocument ( CurrentFile,
					bcolor: GetValidColorString ( CurrentBackColor ),
					fcolor: GetValidColorString ( CurrentTextColor ),
					fsize: CurrentFontsize );
				ViiewerContainerGrid . Background = FindResource ( GetValidColorString ( CurrentBackColor ) ) as SolidColorBrush;

			}
			else
				Utils . DoErrorBeep ( );
			InfoPane . Document = myFlowDocument;
			InfoPane . Refresh ( );
			InfoPane . UpdateLayout ( );
		}

		private string GetValidColorString ( string color )
		{
			// Get new SolidColorBrush for coloring text/background
			string newcolor = "";
			Brushcolors . TryGetValue ( color, out newcolor );
			return newcolor;
		}

		private void InfoPane_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			SolidColorBrush scb1 = new ( );
			SolidColorBrush scb2 = new ( );
			SolidColorBrush scb3 = new ( );
			SolidColorBrush scb4 = new ( );
			upopup = new ( );
			scb1 = Application . Current . FindResource (  "Blue0" ) as SolidColorBrush;
			scb2 = Application . Current . FindResource (  "White0" ) as SolidColorBrush;
			scb3 = Application . Current . FindResource (  "Red0" ) as SolidColorBrush;
			//scb4 = Application . Current . FindResource ( ( "Blue0" ) as SolidColorBrush;
			upopup . LoadPopupUControl ( InfoPane,
				popuptext,
				scb1,
				  scb2,
					 scb3,
					  scb1);
			upopup . Visibility = Visibility . Visible;
		}

		private void Button_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void Closepopup_Click ( object sender, RoutedEventArgs e )
		{
			//popup1 . IsOpen = false;
			upopup . Visibility -= Visibility . Hidden;
		}

		private void popup1_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . Escape )
				//				popup1 . IsOpen = false;
				upopup . Visibility -= Visibility . Hidden;
		}

		private void InfoPane_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			//			popup1 . IsOpen = false;
			if(upopup  != null)
				upopup . Visibility -= Visibility . Hidden;
		}

        private void InformationWindow_SizeChanged ( object sender, SizeChangedEventArgs e )
        {

        }
    }
}
