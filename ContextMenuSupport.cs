using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;
using System . Windows . Media;

using System . Windows;

namespace Wpfmain
{
	class ContextMenuSupport
	{
		/// <summary>
		/// Collapses 1 or more Context menu entries and returns a ContextMenu pointer
		/// </summary>
		/// <param name="menuname"></param>
		/// <param name="singleton"></param>
		/// <param name="delItems"></param>
		/// <returns></returns>
		public static ContextMenu RemoveMenuItems ( ContextMenu menuname , string singleton = "" , List<string> delItems = null )
		{
			// Collapse visibility on one or more context menu items
			int listcount = 0;
			if ( delItems != null )
				listcount = delItems . Count;
			MenuItem mi = new  ( );

			//List<MenuItem> items = new List<MenuItem> ( );

			if ( singleton != "" )
			{
				// Hide specified item
				foreach ( var item in menuname . Items )
				{
					mi = item as MenuItem;
					if ( mi . Name == singleton )
					{
						mi . Visibility = Visibility . Collapsed;
						break;
					}
				}
			}
			else
			{
				foreach ( var menuitem in delItems )
				{
					foreach ( var item in menuname . Items )
					{
						mi = item as MenuItem;
						if ( mi == null )
							continue;
						if ( mi . Name == menuitem )
						{
							mi . Visibility = Visibility . Collapsed;
							//items . Add ( mi );
						}
					}
				}
			}
			return menuname;
		}

		public static ContextMenu AddMenuItem ( ContextMenu menuname , string entry , string MenuEntry )
		{
			MenuItem mi = new MenuItem ( );
			foreach ( MenuItem item in menuname . Items )
			{
				if ( item . Name == entry )
				{
					mi = item;
					mi . Header = MenuEntry;
					mi . Visibility = Visibility . Visible;
					break;
				}
			}
			return menuname;
		}

		public static ContextMenu RemoveMainMenuItems ( ContextMenu menuname, string singleton = "", List<string> delItems = null )
		{
			// Collapse visibility on one or more context menu items
			int listcount = 0;
			if ( delItems != null )
				listcount = delItems . Count;
			MenuItem mi = new ( );

			//List<MenuItem> items = new List<MenuItem> ( );

			if ( singleton != "" )
			{
				// Hide specified item
				foreach ( var item in menuname . Items )
				{
					mi = item as MenuItem;
					if ( mi . Name == singleton )
					{
						mi . Visibility = Visibility . Collapsed;
						break;
					}
				}
			}
			else
			{
				foreach ( var menuitem in delItems )
				{
					foreach ( var item in menuname . Items )
					{
						mi = item as MenuItem;
						if ( mi == null )
							continue;
						if ( mi . Name == menuitem )
						{
							mi . Visibility = Visibility . Collapsed;
							//items . Add ( mi );
						}
					}
				}
			}
			return menuname;
		}

		public static Menu AddMainMenuItem ( Menu menuname, string entry, string MenuEntry )
		{
			MenuItem mi = new MenuItem ( );
			foreach ( MenuItem item in menuname . Items )
			{
				if ( item . Name == entry )
				{
					mi = item;
					mi . Header = MenuEntry;
					mi . Visibility = Visibility . Visible;
					break;
				}
			}
			return menuname;
		}



		/// <summary>
		/// Resets Visibility of 1 or more Context menu entries and returns a ContextMenu pointer
		/// </summary>
		/// <param name="menuname"></param>
		/// <param name="singleton"></param>
		/// <param name="delItems"></param>
		/// <returns></returns>
		public static Menu ResetMainMenuItems ( string menuname , string singleton = "" , List<string> delItems = null )
		{
			int listcount = 0;
			// reset visibility on one or more previously collapsed context menu items
			if ( delItems != null )
				listcount = delItems . Count;

			var menu = Application.Current .FindResource ( menuname ) as Menu;

			if ( singleton != "" )
			{
				// Show specified menu item
				foreach ( MenuItem item in menu . Items )
				{
					if ( item . Name == singleton )
					{
						item . Visibility = Visibility . Visible;
						break;
					}
				}
			}
			else
			{
				//  specific entry  specified to be Hidden
				foreach ( var delitem in delItems )
				{
					// Hide specified menu item

					foreach ( MenuItem menuitem in menu . Items )
					{
						//var v = mi . Items;
						if ( menuitem . Name == delitem )
							menuitem . Visibility = Visibility . Hidden;
					}
				}
			}
			return menu;
		}
		public static bool  CheckForMenuItem ( string menuname , string singleton = "")
		{
			bool result = false;
			//// reset visibility on one or more previously collapsed context menu items
			//if ( delItems != null )
			//	listcount = delItems . Count;
			SProcsHandling sph = SProcsHandling.GetSProcsHandling();
			var menu = sph.FindResource ( menuname ) as ContextMenu;

			if ( singleton != "" )
			{
				// Show specified menu item
				foreach ( MenuItem item in menu . Items )
				{
					if ( item . Name == singleton )
					{
						result = true;
						break;
					}
				}
			}
	
			return result;
		}
	}
}
