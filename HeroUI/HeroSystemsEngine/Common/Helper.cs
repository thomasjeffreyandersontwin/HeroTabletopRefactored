using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HeroUI.HeroSystemsEngine
{
    public class HelperLibrary
    {
        #region Resource Dictionary and Style related
        public static System.Windows.Style GetCustomStyle(string styleName)
        {
            System.Windows.ResourceDictionary resource = new System.Windows.ResourceDictionary
            {
                Source = new Uri(Constants.RESOURCE_DICTIONARY_PATH, UriKind.RelativeOrAbsolute)
            };
            return (System.Windows.Style)resource[styleName];
        }

        public static System.Windows.Style GetCustomWindowStyle()
        {
            return GetCustomStyle(Constants.CUSTOM_MODELESS_TRANSPARENT_WINDOW_STYLENAME);
        }

        #endregion

        #region General Control

        public static Visual GetAncestorByType(DependencyObject element, Type type)
        {
            while (element != null && !(element.GetType() == type))
                element = VisualTreeHelper.GetParent(element);

            return element as Visual;
        }

        public static Visual GetTemplateAncestorByType(DependencyObject element, Type type)
        {
            while (element != null && !(element.GetType() == type))
                element = (element as FrameworkElement).TemplatedParent;

            return element as Visual;
        }

        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
                (element as FrameworkElement).ApplyTemplate();
            for (int i = 0;
                i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

        public static string GetContainerWindowName(object element)
        {
            Window win = null;
            string winName = "";

            if (element is Window)
            {
                win = element as Window;
                winName = win.Name;
            }
            else
            {
                DependencyObject dObj = element as DependencyObject;
                while (win == null)
                {
                    FrameworkElement elem = dObj as FrameworkElement;
                    dObj = elem.Parent;
                    if (dObj is Window)
                    {
                        win = dObj as Window;
                        winName = win.Name;
                        break;
                    }
                }
            }

            return winName;
        }

        // Helper to search up the VisualTree
        public static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        #endregion
    }
}
