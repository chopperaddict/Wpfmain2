using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Newtonsoft.Json.Linq;
using Views;

namespace Wpfmain
{
    public class MenuAttachedProperties : DependencyObject
    {
        #region UseAttProperties
        public static readonly DependencyProperty UseAttPropertiesProperty
              = DependencyProperty.RegisterAttached(
              "UseAttProperties",
              typeof(bool),
              typeof(MenuAttachedProperties),
              new PropertyMetadata(false), OnUseAttPropertiesChanged);
        public static bool GetUseAttProperties(DependencyObject d)
        {
            return (bool)d.GetValue(UseAttPropertiesProperty);
        }
        public static void SetUseAttProperties(DependencyObject d, bool value)
        {
            d.SetValue(UseAttPropertiesProperty, value);
        }
        private static bool OnUseAttPropertiesChanged(object value)
        {
            return true;
        }
        #endregion UseAttProperties

        #region NormalBackground 
        public static Brush NormalBackground(DependencyObject obj)
        {
            if (GetUseAttProperties(obj))
                return (Brush)obj.GetValue(NormalBackgroundProperty);
            return null;
        }
        public static void SetNormalBackground(DependencyObject obj, Brush value)
        {
            if (GetUseAttProperties(obj))
                obj.SetValue(NormalBackgroundProperty, value);
        }
        public static readonly DependencyProperty NormalBackgroundProperty =
              DependencyProperty.RegisterAttached("NormalBackground", typeof(Brush), typeof(MenuAttachedProperties), new PropertyMetadata(Brushes.LightGray));
        #endregion HeaderBackground 

        #region NormallForeground 
        public static Brush NormalForeground(DependencyObject obj)
        {
            if (GetUseAttProperties(obj))
                return (Brush)obj.GetValue(NormalForegroundProperty);
            return null;
        }
        public static void SetNormalForeground(DependencyObject obj, Brush value)
        {
            if (GetUseAttProperties(obj))
                obj.SetValue(NormalForegroundProperty, value);
        }
        public static readonly DependencyProperty NormalForegroundProperty =
                DependencyProperty.RegisterAttached("NormalForeground", typeof(Brush), typeof(MenuAttachedProperties), new PropertyMetadata(Brushes.Black));
        #endregion NormallForeground 

        #region MouseoverBackground 
        public static Brush GetMouseoverBackground(DependencyObject obj)
        {
            if (GetUseAttProperties(obj))
                return (Brush)obj.GetValue(MouseoverBackgroundProperty);
            return null;
        }

        public static void SetMouseoverBackground(DependencyObject obj, Brush value)
        {
            if (GetUseAttProperties(obj))
                obj.SetValue(MouseoverBackgroundProperty, value);
        }
        public static readonly DependencyProperty MouseoverBackgroundProperty =
                DependencyProperty.RegisterAttached("MouseoverBackground", typeof(Brush), typeof(MenuAttachedProperties), new PropertyMetadata(Brushes.DarkGray));
        #endregion MouseoverBackground 

        #region MouseoverForeground 
        public static Brush MousoverForeground(DependencyObject obj)
        {
            if (GetUseAttProperties(obj))
                return (Brush)obj.GetValue(MousoverForegroundProperty);
            return null;
        }
        public static void SetMousoverForeground(DependencyObject obj, Brush value)
        {
            if (GetUseAttProperties(obj))
                obj.SetValue(MousoverForegroundProperty, value);
        }
        public static readonly DependencyProperty MousoverForegroundProperty =
            DependencyProperty.RegisterAttached("MousoverForeground", typeof(Brush), typeof(MenuAttachedProperties), new PropertyMetadata(Brushes.White));
        #endregion MouseoverForeground 



    }
}
