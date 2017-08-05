using Framework.WPF.Extensions;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HeroUI
{
    #region Converters
    /// <summary>   Boolean to visibility converter. </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>   Converts boolean to visiblity. </summary>
        /// <param name="value">        The value. </param>
        /// <param name="targetType">   Type of the target. </param>
        /// <param name="parameter">    The parameter. </param>
        /// <param name="culture">      The culture. </param>
        /// <returns>   The converted object. </returns>
        public object Convert(object value,
                              Type targetType,
                              object parameter,
                              System.Globalization.CultureInfo culture)
        {
            Visibility vRet = Visibility.Visible;
            if (value is bool)
            {
                if ((bool)value)
                    vRet = Visibility.Visible;
                else
                    vRet = Visibility.Collapsed;
            }

            return vRet;
        }

        /// <summary>   Convert back. </summary>
        /// <exception cref="NotImplementedException">  Thrown when the requested operation is
        ///                                             unimplemented. </exception>
        /// <param name="value">        The value. </param>
        /// <param name="targetType">   Type of the target. </param>
        /// <param name="parameter">    The parameter. </param>
        /// <param name="culture">      The culture. </param>
        /// <returns>   The converted object. </returns>
        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>   Boolean to visibility invert converter. </summary>
    public class BooleanToVisibilityInvertConverter : IValueConverter
    {
        /// <summary>   Converts boolean to inverted visibility. </summary>
        /// <param name="value">        The value. </param>
        /// <param name="targetType">   Type of the target. </param>
        /// <param name="parameter">    The parameter. </param>
        /// <param name="culture">      The culture. </param>
        /// <returns>   The converted object. </returns>
        public object Convert(object value,
                              Type targetType,
                              object parameter,
                              System.Globalization.CultureInfo culture)
        {
            Visibility vRet = Visibility.Visible;
            if (value is bool)
            {
                if ((bool)value)
                {
                    if ((string)parameter != "invert")
                        vRet = Visibility.Collapsed;
                    else
                        vRet = Visibility.Visible;
                }
                else
                {
                    if ((string)parameter != "invert")
                        vRet = Visibility.Visible;
                    else
                        vRet = Visibility.Collapsed;
                }
            }

            return vRet;
        }

        /// <summary>   Convert back. </summary>
        /// <exception cref="NotImplementedException">  Thrown when the requested operation is
        ///                                             unimplemented. </exception>
        /// <param name="value">        The value. </param>
        /// <param name="targetType">   Type of the target. </param>
        /// <param name="parameter">    The parameter. </param>
        /// <param name="culture">      The culture. </param>
        /// <returns>   The converted object. </returns>
        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bRet = false;
            bool bParam = parameter != null ? Boolean.Parse(parameter.ToString()) : true;
            if (value != null)
                bRet = ((bool)value ^ bParam);
            return bRet;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bParam = parameter != null ? Boolean.Parse(parameter.ToString()) : true;
            return ((bool)value ^ bParam);
        }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }

    public class StringComparerToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == parameter.ToString())
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// BorderGapMaskConverter class
    /// </summary>
    public class BorderGapMaskConverter : IMultiValueConverter
        {

            /// <summary>
            /// Convert a value.
            /// </summary>
            /// <param name="values">values as produced by source binding</param>
            /// <param name="targetType">target type</param>
            /// <param name="parameter">converter parameter</param>
            /// <param name="culture">culture information</param>
            /// <returns>
            /// Converted value.
            /// Visual Brush that is used as the opacity mask for the Border
            /// in the style for GroupBox.
            /// </returns>
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                //
                // Parameter Validation
                //

                Type doubleType = typeof(double);

                if (parameter == null ||
                    values == null ||
                    values.Length != 3 ||
                    values[0] == null ||
                    values[1] == null ||
                    values[2] == null ||
                    !doubleType.IsAssignableFrom(values[0].GetType()) ||
                    !doubleType.IsAssignableFrom(values[1].GetType()) ||
                    !doubleType.IsAssignableFrom(values[2].GetType()))
                {
                    return DependencyProperty.UnsetValue;
                }

                Type paramType = parameter.GetType();
                if (!(doubleType.IsAssignableFrom(paramType) || typeof(string).IsAssignableFrom(paramType)))
                {
                    return DependencyProperty.UnsetValue;
                }

                //
                // Conversion
                //

                double headerWidth = (double)values[0];
                double borderWidth = (double)values[1];
                double borderHeight = (double)values[2];

                // Doesn't make sense to have a Grid
                // with 0 as width or height
                if (borderWidth == 0
                    || borderHeight == 0)
                {
                    return null;
                }

                // Width of the line to the left of the header
                // to be used to set the width of the first column of the Grid
                double lineWidth;
                if (parameter is string)
                {
                    lineWidth = Double.Parse(((string)parameter), NumberFormatInfo.InvariantInfo);
                }
                else
                {
                    lineWidth = (double)parameter;
                }

                Grid grid = new Grid();
                grid.Width = borderWidth;
                grid.Height = borderHeight;
                ColumnDefinition colDef1 = new ColumnDefinition();
                ColumnDefinition colDef2 = new ColumnDefinition();
                ColumnDefinition colDef3 = new ColumnDefinition();
                colDef1.Width = new GridLength(lineWidth);
                colDef2.Width = new GridLength(headerWidth);
                colDef3.Width = new GridLength(1, GridUnitType.Star);
                grid.ColumnDefinitions.Add(colDef1);
                grid.ColumnDefinitions.Add(colDef2);
                grid.ColumnDefinitions.Add(colDef3);
                RowDefinition rowDef1 = new RowDefinition();
                RowDefinition rowDef2 = new RowDefinition();
                rowDef1.Height = new GridLength(borderHeight / 2);
                rowDef2.Height = new GridLength(1, GridUnitType.Star);
                grid.RowDefinitions.Add(rowDef1);
                grid.RowDefinitions.Add(rowDef2);

                Rectangle rectColumn1 = new Rectangle();
                Rectangle rectColumn2 = new Rectangle();
                Rectangle rectColumn3 = new Rectangle();
                rectColumn1.Fill = Brushes.Black;
                rectColumn2.Fill = Brushes.Black;
                rectColumn3.Fill = Brushes.Black;

                Grid.SetRowSpan(rectColumn1, 2);
                Grid.SetRow(rectColumn1, 0);
                Grid.SetColumn(rectColumn1, 0);

                Grid.SetRow(rectColumn2, 1);
                Grid.SetColumn(rectColumn2, 1);

                Grid.SetRowSpan(rectColumn3, 2);
                Grid.SetRow(rectColumn3, 0);
                Grid.SetColumn(rectColumn3, 2);

                grid.Children.Add(rectColumn1);
                grid.Children.Add(rectColumn2);
                grid.Children.Add(rectColumn3);

                return (new VisualBrush(grid));
            }

            /// <summary>
            /// Not Supported
            /// </summary>
            /// <param name="value">value, as produced by target</param>
            /// <param name="targetTypes">target types</param>
            /// <param name="parameter">converter parameter</param>
            /// <param name="culture">culture information</param>
            /// <returns>Nothing</returns>
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                return new object[] { Binding.DoNothing };
            }
        }

    public class LeftBorderGapMaskConverter : IMultiValueConverter
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public LeftBorderGapMaskConverter()
        {
            //      base.ctor();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Type type1 = typeof(double);
            if (parameter == null
                || values == null
                || (values.Length != 3 || values[0] == null)
                || (values[1] == null
                    || values[2] == null
                    || (!type1.IsAssignableFrom(values[0].GetType())
                        || !type1.IsAssignableFrom(values[1].GetType())))
                || !type1.IsAssignableFrom(values[2].GetType()))
                return DependencyProperty.UnsetValue;

            Type type2 = parameter.GetType();
            if (!type1.IsAssignableFrom(type2)
                && !typeof(string).IsAssignableFrom(type2))
                return DependencyProperty.UnsetValue;

            double pixels1 = (double)values[0];
            double num1 = (double)values[1];
            double num2 = (double)values[2];
            if (num1 == 0.0 || num2 == 0.0)
                return (object)null;

            double pixels2 = !(parameter is string)
                ? (double)parameter
                : double.Parse((string)parameter, (IFormatProvider)NumberFormatInfo.InvariantInfo);

            Grid grid = new Grid();
            grid.Width = num1;
            grid.Height = num2;
            RowDefinition RowDefinition1 = new RowDefinition();
            RowDefinition RowDefinition2 = new RowDefinition();
            RowDefinition RowDefinition3 = new RowDefinition();
            RowDefinition1.Height = new GridLength(pixels2);
            RowDefinition2.Height = new GridLength(pixels1);
            RowDefinition3.Height = new GridLength(1.0, GridUnitType.Star);
            grid.RowDefinitions.Add(RowDefinition1);
            grid.RowDefinitions.Add(RowDefinition2);
            grid.RowDefinitions.Add(RowDefinition3);
            ColumnDefinition ColumnDefinition1 = new ColumnDefinition();
            ColumnDefinition ColumnDefinition2 = new ColumnDefinition();
            ColumnDefinition1.Width = new GridLength(num2 / 2.0);
            ColumnDefinition2.Width = new GridLength(1.0, GridUnitType.Star);
            grid.ColumnDefinitions.Add(ColumnDefinition1);
            grid.ColumnDefinitions.Add(ColumnDefinition2);
            Rectangle rectangle1 = new Rectangle();
            Rectangle rectangle2 = new Rectangle();
            Rectangle rectangle3 = new Rectangle();
            rectangle1.Fill = (Brush)Brushes.Black;
            rectangle2.Fill = (Brush)Brushes.Black;
            rectangle3.Fill = (Brush)Brushes.Black;

            Grid.SetColumnSpan((UIElement)rectangle1, 2);
            Grid.SetColumn((UIElement)rectangle1, 0);
            Grid.SetRow((UIElement)rectangle1, 0);
            Grid.SetColumn((UIElement)rectangle2, 1);
            Grid.SetRow((UIElement)rectangle2, 1);
            Grid.SetColumnSpan((UIElement)rectangle3, 2);
            Grid.SetColumn((UIElement)rectangle3, 0);
            Grid.SetRow((UIElement)rectangle3, 2);
            grid.Children.Add((UIElement)rectangle1);
            grid.Children.Add((UIElement)rectangle2);
            grid.Children.Add((UIElement)rectangle3);
            return (object)new VisualBrush((Visual)grid);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[1]
          {
            Binding.DoNothing
          };
        }
    }
    
    #endregion

    #region Control Utility Methods

    public class ControlUtilities
    {
        public const string RESOURCE_DICTIONARY_PATH = "/HeroResourceDictionary.xaml";
        public const string CUSTOM_MODELESS_TRANSPARENT_WINDOW_STYLENAME = "CustomModelessTransparentWindow";

        #region TreeView

        public static object GetCurrentSelectedCrowdInCrowdCollectionInTreeView(Object tv, out CrowdMember crowdMember)
        {
            Crowd containingCrowdModel = null;
            crowdMember = null;
            TreeView treeView = tv as TreeView;

            if (treeView != null && treeView.SelectedItem != null)
            {
                if (treeView.SelectedItem is Crowd)
                {
                    containingCrowdModel = treeView.SelectedItem as Crowd;
                }
                else
                {
                    DependencyObject dObject = treeView.GetItemFromSelectedObject(treeView.SelectedItem);
                    TreeViewItem tvi = dObject as TreeViewItem; // got the selected treeviewitem
                    crowdMember = tvi.DataContext as CrowdMember;
                    dObject = VisualTreeHelper.GetParent(tvi); // got the immediate parent
                    tvi = dObject as TreeViewItem; // now get first treeview item parent
                    while (tvi == null)
                    {
                        dObject = VisualTreeHelper.GetParent(dObject);
                        tvi = dObject as TreeViewItem;
                    }
                    containingCrowdModel = tvi.DataContext as Crowd;
                }
            }

            return containingCrowdModel;
        }

        public static object GetCurrentSelectedAnimationInAnimationCollection(Object tv, out AnimationElement animationElement)
        {
            AnimationElement selectedAnimationElement = null;
            animationElement = null;
            TreeView treeView = tv as TreeView;

            if (treeView != null && treeView.SelectedItem != null)
            {
                DependencyObject dObject = treeView.GetItemFromSelectedObject(treeView.SelectedItem);
                TreeViewItem tvi = dObject as TreeViewItem; // got the selected treeviewitem
                if (tvi != null)
                    selectedAnimationElement = tvi.DataContext as AnimationElement;
                dObject = VisualTreeHelper.GetParent(tvi); // got the immediate parent
                tvi = dObject as TreeViewItem; // now get first treeview item parent
                while (tvi == null)
                {
                    dObject = VisualTreeHelper.GetParent(dObject);
                    tvi = dObject as TreeViewItem;
                    if (tvi == null)
                    {
                        var tView = dObject as TreeView;
                        if (tView != null)
                            break;
                    }
                    else
                        animationElement = tvi.DataContext as AnimationElement;
                }
            }

            return selectedAnimationElement;
        }

        public static string GetTextFromControlObject(object control)
        {
            string text = null;
            PropertyInfo propertyInfo = control.GetType().GetProperty("Text");
            if (propertyInfo != null)
            {
                text = propertyInfo.GetValue(control).ToString();
            }
            return text;
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

        #region Resource Dictionary and Style related
        public static System.Windows.Style GetCustomStyle(string styleName)
        {
            System.Windows.ResourceDictionary resource = new System.Windows.ResourceDictionary
            {
                Source = new Uri(RESOURCE_DICTIONARY_PATH, UriKind.RelativeOrAbsolute)
            };
            var windowStyle = (System.Windows.Style)resource[styleName];
            return windowStyle;
        }
        public static System.Windows.Style GetCustomStyle(Type targetType)
        {
            System.Windows.ResourceDictionary resource = new System.Windows.ResourceDictionary
            {
                Source = new Uri(RESOURCE_DICTIONARY_PATH, UriKind.RelativeOrAbsolute)
            };
            var windowStyle = (System.Windows.Style)resource[targetType];
            return windowStyle;
        }

        public static System.Windows.Style GetCustomWindowStyle()
        {
            return GetCustomStyle(typeof(Window));
        }

        #endregion
    }

    #endregion

    #region Input Binding

    public class InputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
    {
        public InputBindingTrigger()
        {

        }
        public InputBinding InputBinding
        {
            get { return (InputBinding)GetValue(InputBindingProperty); }
            set { SetValue(InputBindingProperty, value); }
        }
        public static readonly DependencyProperty InputBindingProperty =
            DependencyProperty.Register("InputBinding", typeof(InputBinding)
            , typeof(InputBindingTrigger)
            , new UIPropertyMetadata(null));
        protected override void OnAttached()
        {
            if (InputBinding != null)
            {
                InputBinding.Command = this;
                AssociatedObject.InputBindings.Add(InputBinding);
            }
            base.OnAttached();
        }

        #region ICommand Members
        public bool CanExecute(object parameter)
        {
            // action is anyway blocked by Caliburn at the invoke level
            return true;
        }
        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            InvokeActions(parameter);
        }

        #endregion
    }

    #endregion
}
