using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Controls . Primitives;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Navigation;
using System . Windows . Shapes;

namespace Wpfmain . UserControls
{
	/// <summary>
	/// Interaction logic for PopupUControl.xaml
	/// </summary>
	public partial class PopupUControl : UserControl
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

		private string _Popuptext;
		public string Popuptext
		{
			get { return _Popuptext; }
			set { _Popuptext = value; OnPropertyChanged("Popuptext");} 
		}
		private Brush _Bcolor;
		public Brush Bcolor
		{
			get { return _Bcolor; }
			set { _Bcolor = value; OnPropertyChanged ( "Bcolor" ); }
		}
		private Brush _Fcolor;
		public Brush Fcolor
		{
			get { return _Fcolor; }
			set { _Fcolor = value; OnPropertyChanged ( "Fcolor" ); }
		}
		private Brush _BtnBcolor;
		public Brush BtnBcolor {
			get { return _BtnBcolor; }
			set { _BtnBcolor = value; OnPropertyChanged ( "BtnBcolor" ); }
		}
		private Brush _BtnFcolor;
		public Brush BtnFcolor {
			get { return _BtnFcolor; }
			set { _BtnFcolor = value; OnPropertyChanged ( "BtnFcolor" ); }
		}
		private Control _Parent;
		new public Control Parent{
			get { return _Parent; }
			set { _Parent = value; OnPropertyChanged ( "Parent" ); }
		}
		public PopupUControl ( )
		{
			InitializeComponent ( );
			this.DataContext= this;
		}
		public void LoadPopupUControl ( Control parent, string text, Brush bcolor, Brush fcolor, Brush btnbcolor, Brush btnfcolor )
		{
			Popuptext = text;
			Bcolor =bcolor;
			Fcolor = fcolor;
			BtnBcolor = btnbcolor;
			BtnFcolor = btnfcolor;
			Parent = parent;
			//popup1.Visibility= Visibility.Visible;
			PlacementMode pm = new ( );
			popup1 . PlacementTarget = Parent;
			pm = PlacementMode . MousePoint;
			popup1 . Placement = pm;
			popup1 . IsOpen = true;
            Debug.WriteLine("Popup loaded");
        }
		private void UserControl_Loaded ( object sender, RoutedEventArgs e )
		{
		}
		private void popup1_PreviewKeyDown ( object sender, KeyEventArgs e )
		{

		}

		private void Closepopup_Click ( object sender, RoutedEventArgs e )
		{

		}


	}
}
