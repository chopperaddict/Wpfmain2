using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Diagnostics . Eventing . Reader;
using System . Reflection;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;

using ViewModels;

using Wpfmain . Models;

namespace Wpfmain . UtilWindows
{
	/// <summary>
	/// Interaction logic for DataEditWin.xaml
	/// </summary>
	public partial class DataEditWin : Window
	{
		public List<string> listdata { get; set; } = new ( );
		public List<string> newdata { get; set; } = new ( );
		public string CurrentDbName { set; get; } = "";

		public bool bdirty { get; set; } = false;
		public int reccount { get; set; } = 0;
		public int currselection { get; set; } = 0;
		public List<DataGridLayout> dglayoutlist { get; set; }
		public SProcsHandling sphandling { get; set; }
		GenericClass gclass = new ( );
		GenericClass newgclass = new ( );
		public static string SqlCommand { get; set; }
		private int colcount { get; set; } = 0;


		#region methods

		public DataEditWin ( SProcsHandling sph, string SqlTable, List<string> datalist, List<DataGridLayout> Dglayoutlist, int currentindex )
		{
			int newweight = 0;
			InitializeComponent ( );
			listdata =ObjectCopier.Clone( datalist);
			dglayoutlist = ObjectCopier . Clone ( Dglayoutlist);
			this . Topmost = true;
			sphandling = sph;
			CurrentDbName = SqlTable;
			reccount = datalist . Count;
			// create cloned copy of original data so it doesn't get updated automatically
			newdata = ObjectCopier . Clone ( datalist );
			populatedatafields ( datalist );
			currselection = currentindex;
			Data2 . Focus ( );
			editprompt . Text = "Edit content of current record and click \"Update Table\" to save any changes,  or ESC to exit";
			newweight = newweight = MainWindow . GetfontWeight ( "Normal" );
			editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
			editprompt . Background = FindResource ( "Yellow2" ) as SolidColorBrush;
			editprompt . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
			int indx = 0;
			Data2 . SelectionLength = Data2.Text.Length;
			Data2 . Select ( 0, Data2 . Text . Length );
			Data2 . Focus ( );
			this . Show( );
		}
		private void populatedatafields ( List<string> datalist )
		{
			int newweight = 0;
			// put data into edit fields and fill GenericClass with it as well
			for ( int x = 0 ; x < reccount ; x++ )
			{
				switch ( x )
				{
					case 0:
						label1 . Content = sphandling . dglayoutlist [ 0 ] . Fieldname;
						Data1 . Text = datalist [ x ];
						gclass . field1 = datalist [ x ];
						if ( dglayoutlist [0].fieldname .ToUpper() == "ID")
						{
							label1 . Content = "(Automatic value)";
							label1 . Foreground = FindResource ( "Orange2" ) as SolidColorBrush;
							//newweight = Convert . ToInt32 ( MainWindow . fontWeight . DemiBold );
							newweight = MainWindow . GetfontWeight ( "DemiBold" );
							label1 . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
						}
						break;
					case 1:
						label2 . Content = sphandling . dglayoutlist [ 1 ] . Fieldname;
						Data2 . Text = datalist [ x ];
						gclass . field2 = datalist [ x ];
						break;
					case 2:
						label3 . Content = sphandling . dglayoutlist [ 2 ] . Fieldname;
						Data3 . Text = datalist [ x ];
						gclass . field3 = datalist [ x ];
						break;
					case 3:
						label4 . Content = sphandling . dglayoutlist [ 3 ] . Fieldname;
						Data4 . Text = datalist [ x ];
						gclass . field4 = datalist [ x ];
						break;
					case 4:
						label5 . Content = sphandling . dglayoutlist [ 4 ] . Fieldname;
						Data5 . Text = datalist [ x ];
						gclass . field5 = datalist [ x ];
						break;
					case 5:
						label6 . Content = sphandling . dglayoutlist [ 5 ] . Fieldname;
						Data6 . Text = datalist [ x ];
						gclass . field6 = datalist [ x ];
						break;
					case 6:
						label7 . Content = sphandling . dglayoutlist [ 6 ] . Fieldname;
						Data7 . Text = datalist [ x ];
						gclass . field7 = datalist [ x ];
						break;
					case 7:
						label8 . Content = sphandling . dglayoutlist [ 7 ] . Fieldname;
						Data8 . Text = datalist [ x ];
						gclass . field8 = datalist [ x ];
						break;
					case 8:
						label9 . Content = sphandling . dglayoutlist [ 8 ] . Fieldname;
						Data9 . Text = datalist [ x ];
						gclass . field9 = datalist [ x ];
						break;
					case 9:
						label10 . Content = sphandling . dglayoutlist [ 9 ] . Fieldname;
						Data10 . Text = datalist [ x ];
						gclass . field10 = datalist [ x ];
						break;
					case 10:
						label11 . Content = sphandling . dglayoutlist [ 10 ] . Fieldname;
						Data11 . Text = datalist [ x ];
						gclass . field11 = datalist [ x ];
						break;
					case 11:
						label12 . Content = sphandling . dglayoutlist [ 11 ] . Fieldname;
						Data12 . Text = datalist [ x ];
						gclass . field12 = datalist [ x ];
						break;
					case 12:
						label13 . Content = sphandling . dglayoutlist [ 12 ] . Fieldname;
						Data13 . Text = datalist [ x ];
						gclass . field13 = datalist [ x ];
						break;
					case 13:
						label14 . Content = sphandling . dglayoutlist [ 13 ] . Fieldname;
						Data14 . Text = datalist [ x ];
						gclass . field14 = datalist [ x ];
						break;
					case 14:
						label15 . Content = sphandling . dglayoutlist [ 14 ] . Fieldname;
						Data15 . Text = datalist [ x ];
						gclass . field15 = datalist [ x ];
						break;
					case 15:
						label16 . Content = sphandling . dglayoutlist [ 15 ] . Fieldname;
						Data16 . Text = datalist [ x ];
						gclass . field16 = datalist [ x ];
						break;
					case 16:
						label17 . Content = sphandling . dglayoutlist [ 16 ] . Fieldname;
						Data17 . Text = datalist [ x ];
						gclass . field17 = datalist [ x ];
						break;
					case 17:
						label18 . Content = sphandling . dglayoutlist [ 17 ] . Fieldname;
						Data18 . Text = datalist [ x ];
						gclass . field18 = datalist [ x ];
						break;
					case 18:
						label19 . Content = sphandling . dglayoutlist [ 18 ] . Fieldname;
						Data19 . Text = datalist [ x ];
						gclass . field19 = datalist [ x ];
						break;
					case 19:
						label20 . Content = sphandling . dglayoutlist [ 19 ] . Fieldname;
						Data20 . Text = datalist [ x ];
						gclass . field20 = datalist [ x ];
						break;
				}
			}
			for ( int y = reccount + 1 ; y <= 20 ; y++ )
			{
				switch ( y )
				{
					case 1:
						label1 . IsEnabled = false;
						break;
					case 2:
						label2 . IsEnabled = false;
						break;
					case 3:
						label3 . IsEnabled = false;
						break;
					case 4:
						label4 . IsEnabled = false;
						break;
					case 5:
						label5 . IsEnabled = false;
						break;
					case 6:
						label6 . IsEnabled = false;
						break;
					case 7:
						label7 . IsEnabled = false;
						break;
					case 8:
						label8 . IsEnabled = false;
						break;
					case 9:
						label9 . IsEnabled = false;
						break;
					case 10:
						label10 . IsEnabled = false;
						break;

					case 11:
						label11 . IsEnabled = false;
						break;
					case 12:
						label12 . IsEnabled = false;
						break;
					case 13:
						label13 . IsEnabled = false;
						break;
					case 14:
						label14 . IsEnabled = false;
						break;
					case 15:
						label15 . IsEnabled = false;
						break;
					case 16:
						label16 . IsEnabled = false;
						break;
					case 17:
						label17 . IsEnabled = false;
						break;
					case 18:
						label18 . IsEnabled = false;
						break;
					case 19:
						label19 . IsEnabled = false;
						break;
					case 20:
						label20 . IsEnabled = false;
						break;
				}
				bdirty = false;
			}
			editprompt . Text = "Edit content of current record and click \"Update Table\" to save any changes,  or ESC to exit";
			//newweight = Convert . ToInt32 ( MainWindow . fontWeight . Normal );
			newweight = MainWindow . GetfontWeight ( "Normal" );
			editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
			editprompt . Background = FindResource ( "Yellow2" ) as SolidColorBrush;
			editprompt . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
			Data2 . Focus ( );
		}
		private void Closewin ( object sender, RoutedEventArgs e )
		{
			int newweight = 0;
			Mouse . OverrideCursor = Cursors . Wait;
			editprompt . Text = "Just checking for unsaved  data changes ...";
			editprompt . UpdateLayout ( );
			GenericClass gc = new GenericClass ( );
			if (bdirty && CheckForChanges ( "", -1, true ) == false )
			{
				Mouse . OverrideCursor = Cursors . Arrow;
				MessageBoxResult mbr = MessageBox . Show ( "There are changes in the current data being edited.\n\nDo you want to save them ?", "Unsaved changes ?", MessageBoxButton . YesNo );
				if ( mbr == MessageBoxResult . Yes )
				{
					// create new list of data in newlist
					GetNewData ( );
					// Update table itself
					UpdateDb ( );
					listdata = ObjectCopier . Clone ( newdata );
				}
				else
				{
					sphandling . ExecResult . Text = "Changes were made to data but they have NOT been saved !";
//					newweight = Convert . ToInt32 ( MainWindow . fontWeight . Bold );
					newweight = MainWindow . GetfontWeight ( "Bold" );
					if ( newweight != -1 )
					{
						editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
						editprompt . Background = FindResource ( "Red5" ) as SolidColorBrush;
						editprompt . Foreground = FindResource ( "White0" ) as SolidColorBrush;
					}
				}
				sphandling . RefreshDatagrid ( CurrentDbName );
			}
			this . Close ( );
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private bool UpdateDb ( )
		{
			int currsel = sphandling . SPDatagrid . SelectedIndex;
			Mouse . OverrideCursor = Cursors . Wait;
			GetNewData ( );
			sphandling . CreateNewSqlUpdateCommand ( newdata, newgclass );
			//if ( sphandling . dglayoutlist [ 0 ] . Fieldname . ToUpper ( ) == "ID" )
			//	SProcsHandling . SqlCommand += $" where {sphandling . dglayoutlist [ 1 ] . Fieldname}={newdata [ 0 ]}";
			//else
			SProcsHandling . SqlCommand += $" where {sphandling . dglayoutlist [ 0 ] . Fieldname}={newdata [ 0 ]}";
			Debug . WriteLine ( $"{SProcsHandling . SqlCommand}	" );
			if ( sphandling . UpdateSqlTable ( SProcsHandling . SqlCommand ) == true )
			{
				editprompt . Text = $"The Sql Table [{sphandling . GridCombo . SelectedItem . ToString ( ) . ToUpper ( )}] has been updated successfuly....";
				editprompt . Background = FindResource ( "Green5" ) as SolidColorBrush;
				editprompt . Foreground = FindResource ( "White0" ) as SolidColorBrush;
				sphandling . RefreshDatagrid ( CurrentDbName );
			}
			else
			{
				editprompt . Text = $"The Sql Table [{sphandling . GridCombo . SelectedItem . ToString ( ) . ToUpper ( )}] was NOT updated ....";
				editprompt . Background = FindResource ( "Red4" ) as SolidColorBrush;
				editprompt . Foreground = FindResource ( "White0" ) as SolidColorBrush;
			}
			Mouse . OverrideCursor = Cursors . Arrow;
			return true;
		}
		public string Validatechars ( string dat )
		{
			string newstring = "";
			string validchars = "0123456789";
			if ( dat . Contains ( "'" ) )
				for ( int x = 0 ; x < dat . Length ; x++ )
				{
					char dc = ( char ) dat [ x ];
					if ( validchars . Contains ( dc ) == true )
						newstring += dc;
				}
			else
				newstring = dat;
			dat = newstring;
			return newstring;
		}
		private void GetNewData ( )
		{
			string dat = "";
			List<string> Newdata = new List<string> ( );

			// Create newdata list and genericclass with updated data
			for ( int x = 0 ; x < reccount ; x++ )
			{
				switch ( x )
				{
					case 0:
						dat = Data1 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 1:
						dat = Data2 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 2:
						dat = Data3 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 3:
						dat = Data4 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 4:
						dat = Data5 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 5:
						dat = Data6 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 6:
						dat = Data7 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 7:
						dat = Data8 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 8:
						dat = Data9 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 9:
						dat = Data10 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 10:
						dat = Data11 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 11:
						dat = Data12 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 12:
						dat = Data13 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 13:
						dat = Data14 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 14:
						dat = Data15 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field14 = dat;
						break;
					case 15:
						dat = Data16 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field16 = dat;
						break;
					case 16:
						dat = Data17 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field17 = dat;
						break;
					case 17:
						dat = Data18 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field18 = dat;
						break;
					case 18:
						dat = Data19 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( dat );
						newgclass . field19 = dat;
						break;
					case 19:
						dat = Data20 . Text;
						dat = Validatechars ( dat );
						if ( dat == "" )
							return;
						if ( dat . Contains ( "/" ) )
						{
							dat = ConvertDataToSql ( dat );
							Newdata . Add ( dat );
						}
						else
							Newdata . Add ( Data20 . Text );
						newgclass . field20 = Data20 . Text;
						break;
				}
			}
			// create cloned copy of original data so it doesn't get updated automatically
			newdata = ObjectCopier . Clone ( Newdata );
			Newdata . Clear ( );
		}
		private void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			DisableAllfields ( );
		}
		private void DisableAllfields ( )
		{
			for ( int x = reccount ; x < 20 ; x++ )
			{
				switch ( x )
				{
					case 0:
						Data1 . IsEnabled = false;
						break;
					case 1:
						Data2 . IsEnabled = false;
						break;
					case 2:
						Data3 . IsEnabled = false;
						break;
					case 3:
						Data4 . IsEnabled = false;
						break;
					case 4:
						Data5 . IsEnabled = false;
						break;
					case 5:
						Data6 . IsEnabled = false;
						break;
					case 6:
						Data7 . IsEnabled = false;
						break;
					case 7:
						Data8 . IsEnabled = false;
						break;
					case 8:
						Data9 . IsEnabled = false;
						break;
					case 9:
						Data10 . IsEnabled = false;
						break;
					case 10:
						Data11 . IsEnabled = false;
						break;
					case 11:
						Data12 . IsEnabled = false;
						break;
					case 12:
						Data13 . IsEnabled = false;
						break;
					case 13:
						Data14 . IsEnabled = false;
						break;
					case 14:
						Data15 . IsEnabled = false;
						break;
					case 15:
						Data16 . IsEnabled = false;
						break;
					case 16:
						Data17 . IsEnabled = false;
						break;
					case 17:
						Data18 . IsEnabled = false;
						break;
					case 18:
						Data19 . IsEnabled = false;
						break;
					case 19:
						Data20 . IsEnabled = false;
						break;
				}
			}
		}
		public void CreateEditList ( int currselection )
		{
			List<string> list = new ( );
			sphandling . SPDatagrid . SelectedItem = currselection;
			GenericClass gc = sphandling . SPDatagrid . Items [ currselection ] as GenericClass;
			for ( int x = 0 ; x < 20 ; x++ )
			{
				if ( gc . field1 != null )
				{
					list . Add ( gc . field1 );
					label1 . IsEnabled = false;
				}
				else
					break;
				if ( gc . field2 != null )
					list . Add ( gc . field2 );
				else
				{
					label2 . IsEnabled = false;
				}
				if ( gc . field3 != null )
					list . Add ( gc . field3 );
				else
				{
					label3 . IsEnabled = false;
				}
				if ( gc . field4 != null )
					list . Add ( gc . field4 );
				else
				{
					label4 . IsEnabled = false;
				}
				if ( gc . field5 != null )
					list . Add ( gc . field5 );
				else
				{
					label5 . IsEnabled = false;
				}
				if ( gc . field6 != null )
					list . Add ( gc . field6 );
				else
				{
					label6 . IsEnabled = false;
				}
				if ( gc . field7 != null )
					list . Add ( gc . field7 );
				else
				{
					label7 . IsEnabled = false;
				}
				if ( gc . field8 != null )
					list . Add ( gc . field8 );
				else
				{
					label8 . IsEnabled = false;
				}
				if ( gc . field9 != null )
					list . Add ( gc . field9 );
				else
				{
					label9 . IsEnabled = false;
				}
				if ( gc . field10 != null )
					list . Add ( gc . field10 );
				else
				{
					label10 . IsEnabled = false;
				}


				if ( gc . field11 != null )
					list . Add ( gc . field11 );
				else
				{
					label11 . IsEnabled = false;
				}
				if ( gc . field12 != null )
					list . Add ( gc . field12 );
				else
				{
					label12 . IsEnabled = false;
				}
				if ( gc . field13 != null )
					list . Add ( gc . field13 );
				else
				{
					label13 . IsEnabled = false;
				}
				if ( gc . field14 != null )
					list . Add ( gc . field14 );
				else
				{
					label14 . IsEnabled = false;
				}
				if ( gc . field15 != null )
					list . Add ( gc . field15 );
				else
				{
					label15 . IsEnabled = false;
				}
				if ( gc . field16 != null )
					list . Add ( gc . field16 );
				else
				{
					label16 . IsEnabled = false;
				}
				if ( gc . field17 != null )
					list . Add ( gc . field17 );
				else
				{
					label17 . IsEnabled = false;
				}
				if ( gc . field18 != null )
					list . Add ( gc . field18 );
				else
				{
					label18 . IsEnabled = false;
				}
				if ( gc . field19 != null )
					list . Add ( gc . field19 );
				else
				{
					label19 . IsEnabled = false;
				}
				if ( gc . field20 != null )
					list . Add ( gc . field20 );
				else
				{
					label20 . IsEnabled = false;
				}
			}
			populatedatafields ( list );
			listdata = list;
		}
		private void Savedata ( object sender, RoutedEventArgs e )
		{
			// save data to sql table
			UpdateDb ( );
			bdirty = false;
		}
		public string ConvertDataToSql ( string datestring )
		{
			string [ ] parts = datestring . Split ( " " );
			string [ ] dateparts = parts [ 0 ] . Split ( "/" );
			string newdate = $"'{dateparts [ 2 ]}/{dateparts [ 1 ]}/{dateparts [ 0 ]}'";// {parts[1]}'";
			return newdate;
		}
		private void NextRecord ( object sender, RoutedEventArgs e )
		{
			if ( currselection < sphandling . SPDatagrid . Items . Count )
			{
				currselection++;
				CreateEditList ( currselection );
			}
			editprompt . Text = "Next record shown ... ";
			editprompt . Background = FindResource ( "Purple3" ) as SolidColorBrush;
			editprompt . Foreground = Brushes . White;
			bdirty = false;
		}
		private void PreviousRecord ( object sender, RoutedEventArgs e )
		{
			if ( currselection > 0 )
			{
				currselection--;
				CreateEditList ( currselection );
			}
			editprompt . Text = "Previous record shown ... ";
			editprompt . Background = FindResource ( "Green3" ) as SolidColorBrush;
			editprompt . Foreground = Brushes . White;
			bdirty = false;
		}
		private void AddRecord ( object sender, RoutedEventArgs e )
		{
			GenericClass gc = new ( );
			for ( int x = 0 ; x < listdata . Count ; x++ )
			{
				Data1 . IsEnabled = false;
				Data1 . Text = "Automatic";
				if ( x >= 1 )
				{
					Data2 . Text = "";
					Data2 . IsEnabled = true;
				}
				if ( x >= 2 )
				{
					Data3 . Text = "";
					Data3 . IsEnabled = true;
				}
				if ( x >= 3 )
				{
					Data4 . Text = "";
					Data4 . IsEnabled = true;
				}
				if ( x >= 4 )
				{
					Data5 . Text = "";
					Data5 . IsEnabled = true;
				}
				if ( x >= 5 )
				{
					Data6 . Text = "";
					Data6 . IsEnabled = true;
				}
				if ( x >= 6 )
				{
					Data7 . Text = "";
					Data7 . IsEnabled = true;
				}
				if ( x >= 7 )
				{
					Data8 . Text = "";
					Data8 . IsEnabled = true;
				}
				if ( x >= 8 )
				{
					Data9 . Text = "";
					Data9 . IsEnabled = true;
				}
				if ( x >= 19 )
				{
					Data10 . Text = "";
					Data10 . IsEnabled = true;
				}
				if ( x >= 10 )
				{
					Data11 . Text = "";
					Data11 . IsEnabled = true;
				}
				if ( x >= 11 )
				{
					Data12 . Text = "";
					Data12 . IsEnabled = true;
				}
				if ( x >= 12 )
				{
					Data13 . Text = "";
					Data13 . IsEnabled = true;
				}
				if ( x >= 13 )
				{
					Data14 . Text = "";
					Data14 . IsEnabled = true;
				}
				if ( x >= 14 )
				{
					Data15 . Text = "";
					Data15 . IsEnabled = true;
				}
				if ( x >= 15 )
				{
					Data16 . Text = "";
					Data16 . IsEnabled = true;
				}
				if ( x >= 16 )
				{
					Data17 . Text = "";
					Data17 . IsEnabled = true;
				}
				if ( x >= 17 )
				{
					Data18 . Text = "";
					Data18 . IsEnabled = true;
				}
				if ( x >= 18 )
				{
					Data19 . Text = "";
					Data19 . IsEnabled = true;
				}
				if ( x >= 19 )
				{
					Data20 . Text = "";
					Data20 . IsEnabled = true;
				}
				Data2 . Focus ( );

			}
			editprompt . Text = "Enter data for this new record";
			editprompt . Background = Brushes . LightYellow;
		}

		#endregion methods

		private void Window_KeyDown ( object sender, System . Windows . Input . KeyEventArgs e )
		{
			if ( e . Key == Key . Escape )
				this . Close ( );
		}

		#region focushandllers

		private void Data_GotFocus ( object sender, RoutedEventArgs e )
		{
			TextBox tb = sender as TextBox;
			string [ ] parts = new string [ 2 ];

			if ( tb == null )
				return;
			tb . SelectionLength = tb . Text . Length;
			string fldname = tb . Name;
			string text = tb . Text;
			parts = fldname . Split ( "Data" );
	//		string s = dglayoutlist [ fieldindex - 1] . fieldname;
			int fieldindex = Convert.ToInt32(parts [1]);

			if ( CheckForChanges ( text, fieldindex) == false )
			{
				editprompt . Text = "Data now contains 1 or more changed fields... ";
				editprompt . Background = FindResource ( "Red4" ) as SolidColorBrush;
				editprompt . Foreground = Brushes . White;
				bdirty = true;
			}
		}
		#endregion focushandllers
		private bool CheckForChanges ( string arg, int index, bool CheckAll = false )
		{
			//for (int x = 0 ; x < listdata.Count ; x++)
			//{
			if ( CheckAll == false )
			{
				if ( listdata [ (index - 1)] != arg )
					return false;
			}
			else
			{
				for ( int x = 0 ; x < listdata . Count ; x++ )
				{
					if ( listdata [ x ] != newdata [ x ] )
					{
						// ignore date felds
						if ( listdata [ x ] . Contains ( "/" ) == false )
							return false;
					}
				}
			}
			return true;
		}
		private void Resetdata ( object sender, RoutedEventArgs e )
		{
			int newweight = 0;

			populatedatafields ( listdata );
			editprompt . Text = $"All entries have been reset to original values for you...";
			editprompt . Background = FindResource ( "Orange3" ) as SolidColorBrush;
			editprompt . Foreground = FindResource ( "Red3" ) as SolidColorBrush;
//			newweight = Convert . ToInt32 ( MainWindow . fontWeight . Bold );
			newweight = MainWindow . GetfontWeight ( "Bold" );
			editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
		}

		private void CloseBtn_GotFocus ( object sender, RoutedEventArgs e )
		{

		}

		private void UpdateNewDataList ( object sender, TextChangedEventArgs e )
		{
			TextBox tb = sender as TextBox;
			string name = tb . Name;
			string [ ] parts = new string [ 2 ];
			parts = name . Split ( "Data" );
			switch ( Convert . ToInt32 ( parts [ 1 ] ) )
			{
				case 1:
					newdata [ 0 ] = tb . Text;
					break;
				case 2:
					newdata [ 1 ] = tb . Text;
					break;
				case 3:
					newdata [ 2 ] = tb . Text;
					break;
				case 4:
					newdata [ 3 ] = tb . Text;
					break;
				case 5:
					newdata [ 4 ] = tb . Text;
					break;
				case 6:
					newdata [ 5 ] = tb . Text;
					break;
				case 7:
					newdata [ 6 ] = tb . Text;
					break;
				case 8:
					newdata [ 7 ] = tb . Text;
					break;
				case 9:
					newdata [ 8 ] = tb . Text;
					break;
				case 10:
					newdata [ 9 ] = tb . Text;
					break;
				case 11:
					newdata [ 10 ] = tb . Text;
					break;
				case 12:
					newdata [ 11 ] = tb . Text;
					break;
				case 13:
					newdata [ 12 ] = tb . Text;
					break;
				case 14:
					newdata [ 13 ] = tb . Text;
					break;
				case 15:
					newdata [ 14 ] = tb . Text;
					break;
				case 16:
					newdata [ 15 ] = tb . Text;
					break;
				case 17:
					newdata [ 16 ] = tb . Text;
					break;
				case 18:
					newdata [ 17 ] = tb . Text;
					break;
				case 19:
					newdata [ 18 ] = tb . Text;
					break;
				case 20:
					newdata [ 19 ] = tb . Text;
					break;
			}
			bdirty = true;
		}

		private void Data1_KeyDown ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . Enter )
				UpdateNewDataList ( sender, null );
		}
	}
}
