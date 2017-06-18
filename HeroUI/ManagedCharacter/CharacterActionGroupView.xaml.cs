using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HeroVirtualTabletop.ManagedCharacter
{
    /// <summary>
    /// Interaction logic for CharacterActionGroupView.xaml
    /// </summary>
    public partial class CharacterActionGroupView : UserControl
    {
        private CharacterActionGroupViewModel viewModel;

        private const string OPTION_DRAG_KEY = "CharacterOptionDrag";

        public CharacterActionGroupView()
        {
            InitializeComponent();

            this.DataContextChanged += CharacterActionGroupView_DataContextChanged;
            Style itemContainerStyle = this.optionListBox.ItemContainerStyle;
            if (itemContainerStyle != null && itemContainerStyle.Setters != null)
            {
                itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));

                itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ListBoxItem_PreviewMouseLeftButtonDown)));
                itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(ListBoxItem_PreviewMouseLeftButtonUp)));
            }
        }

        private void CharacterActionGroupView_Loaded(object sender, RoutedEventArgs e)
        {
            this.viewModel.RenameActionGroup();
        }

        private void CharacterActionGroupView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.viewModel = this.DataContext as CharacterActionGroupViewModel;
            this.viewModel.EditModeEnter += viewModel_EditModeEnter;
            this.viewModel.EditModeLeave += viewModel_EditModeLeave;
        }

        #region Rename
        private void viewModel_EditModeEnter(object sender, EventArgs e)
        {
            this.grpBoxOptionGroup.ApplyTemplate();
            Border headborder = (Border)this.grpBoxOptionGroup.Template.FindName("Header", this.grpBoxOptionGroup);

            if (headborder != null)
            {
                ContentPresenter headContentPresenter = (ContentPresenter)headborder.Child;
                headContentPresenter.ApplyTemplate();
                var dataTemplate = this.grpBoxOptionGroup.HeaderTemplate;
                TextBlock headerTextBlock = dataTemplate.FindName("textBlockName", headContentPresenter) as TextBlock;
                TextBox headerTextBox = dataTemplate.FindName("textBoxName", headContentPresenter) as TextBox;
                headerTextBox.Text = headerTextBlock.Text;
                headerTextBox.Visibility = Visibility.Visible;
                headerTextBlock.Visibility = Visibility.Collapsed;
                headerTextBox.Focus();
                headerTextBox.SelectAll(); 
            }
        }

        private void viewModel_EditModeLeave(object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            Grid grid = txtBox.Parent as Grid;
            TextBlock otherTextBlock = grid.Children[0] as TextBlock;
            BindingExpression expression = txtBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            txtBox.Visibility = Visibility.Collapsed;
            otherTextBlock.Visibility = Visibility.Visible;
        }

        #endregion

        #region Drag Drop

        bool isDragging = false;
        Point startPoint;
        ListBoxItem dataItem = null;
        private void StartDrag(ListBoxItem listBoxItem, MouseEventArgs e)
        {
            isDragging = true;
            try
            {
                if (listBoxItem != null)
                {
                    // Find the data behind the ListBoxItem
                    CharacterAction option = (CharacterAction)listBoxItem.DataContext;
                    int sourceIndex = optionListBox.Items.IndexOf(option);

                    // Initialize the drag & drop operation
                    DataObject dragData = new DataObject(OPTION_DRAG_KEY, new Tuple<CharacterActionGroupViewModel, int, CharacterAction>(this.viewModel, sourceIndex, option));
                    DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                isDragging = false;
                dataItem = null;
            }
        }
        private void groupbox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Action action = delegate ()
            {
                if (e.LeftButton == MouseButtonState.Pressed && !isDragging && dataItem != null)//&& !Helper.GlobalVariables_IsPlayingAttack && !this.viewModel.IsReadOnlyMode)
                {
                    // Get the current mouse position
                    Point mousePos = e.GetPosition(null);
                    Vector diff = startPoint - mousePos;
                    if (
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        StartDrag(dataItem as ListBoxItem, e);
                    }
                }
            };
            Application.Current.Dispatcher.BeginInvoke(action);
        }

        private void FindDropTarget(GroupBox lb, out ListBoxItem listBoxItem, DragEventArgs dragEventArgs)
        {
            listBoxItem = null;

            DependencyObject k = VisualTreeHelper.HitTest(lb, dragEventArgs.GetPosition(lb)).VisualHit;

            while (k != null)
            {
                if (k is ListBoxItem)
                {
                    ListBoxItem lbItem = k as ListBoxItem;
                    if (lbItem.DataContext is CharacterAction)
                    {
                        listBoxItem = lbItem;
                        break;
                    }
                }
                else if (k == lb)
                {
                    break;
                }

                k = VisualTreeHelper.GetParent(k);
            }
        }
        private bool expanderExpandedForDrop = false;
        private void grpBoxOptionGroup_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(OPTION_DRAG_KEY) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                Tuple<CharacterActionGroupViewModel, int, CharacterAction> dragDropDataTuple = e.Data.GetData(OPTION_DRAG_KEY) as Tuple<CharacterActionGroupViewModel, int, CharacterAction>;
                CharacterActionGroupViewModel sourceViewModel = dragDropDataTuple.Item1;
                if (sourceViewModel != this.viewModel && this.viewModel.ActionGroup.Type != CharacterActionType.Mixed)
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            if (e.Effects != DragDropEffects.None && !this.ExpanderOptionGroup.IsExpanded)
            {
                this.ExpanderOptionGroup.IsExpanded = expanderExpandedForDrop = true;
            }
            e.Handled = true;
        }

        private void GroupBox_PreviewDragLeave(object sender, DragEventArgs e)
        {
            if (expanderExpandedForDrop)
            {
                this.ExpanderOptionGroup.IsExpanded = expanderExpandedForDrop = false;
            }
        }

        private void GroupBox_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(OPTION_DRAG_KEY))
            {
                expanderExpandedForDrop = false;
                GroupBox groupBox = (GroupBox)sender;
                Tuple<CharacterActionGroupViewModel, int, CharacterAction> dragDropDataTuple = e.Data.GetData(OPTION_DRAG_KEY) as Tuple<CharacterActionGroupViewModel, int, CharacterAction>;
                if (dragDropDataTuple != null)
                {
                    CharacterActionGroupViewModel sourceViewModel = dragDropDataTuple.Item1;
                    int sourceIndex = dragDropDataTuple.Item2;
                    CharacterAction option = dragDropDataTuple.Item3;
                    if (this.viewModel.ActionGroup.Type == CharacterActionType.Mixed && sourceViewModel.ActionGroup.Type != CharacterActionType.Mixed) // do a copy paste
                    {
                        sourceIndex = -1;
                    }
                    int targetIndex = 0;

                    ListBoxItem listBoxItem;
                    FindDropTarget(groupBox, out listBoxItem, e);
                    if (listBoxItem != null)
                    {
                        CharacterAction target = listBoxItem.DataContext as CharacterAction;
                        if (dragDropDataTuple != null && target != null)
                        {
                            targetIndex = optionListBox.Items.IndexOf(target);
                        }
                    }
                    else
                    {
                        targetIndex = optionListBox.Items != null ? optionListBox.Items.Count : 0; // append to last of current option group
                        if (sourceIndex >= 0 && this.viewModel == sourceViewModel) // an item will be removed from the current option group, so reduce target index by 1
                            targetIndex -= 1;
                    }
                    if (sourceViewModel != null && sourceIndex >= 0)
                    {
                        sourceViewModel.RemoveAction(sourceIndex);
                    }
                    this.viewModel.InsertAction(option, targetIndex);
                    this.viewModel.SaveActionGroup();
                }
            }
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
            dataItem = sender as ListBoxItem;
        }

        private void ListBoxItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dataItem = null;
        }

        private void textBlockName_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CharacterEditorView.OptionGroupDragStartPoint = e.GetPosition(null);
            CharacterEditorView.DraggingOptionGroupName = this.viewModel.ActionGroup.Name;
        }

        #endregion     

        #region Adjustable Width

        public double ActionGroupWidth
        {
            get
            {
                return (double)GetValue(ActionGroupWidthProperty);
            }
            set
            {
                SetValue(ActionGroupWidthProperty, value);
            }
        }

        public static readonly DependencyProperty
            ActionGroupWidthProperty =
            DependencyProperty.Register("ActionGroupWidth",
            typeof(double), typeof(CharacterActionGroupView),
            new PropertyMetadata(null));


        public double ActionListBoxWidth
        {
            get
            {
                return (double)GetValue(ActionListBoxWidthProperty);
            }
            set
            {
                SetValue(ActionListBoxWidthProperty, value);
            }
        }

        public static readonly DependencyProperty
            ActionListBoxWidthProperty =
            DependencyProperty.Register("ActionListBoxWidth",
            typeof(double), typeof(CharacterActionGroupView),
            new PropertyMetadata(null));

        public int NumberOfActionsPerRow
        {
            get
            {
                return (int)GetValue(NumberOfActionsPerRowProperty);
            }
            set
            {
                SetValue(NumberOfActionsPerRowProperty, value);
            }
        }

        public static readonly DependencyProperty
            NumberOfActionsPerRowProperty =
            DependencyProperty.Register("NumberOfActionsPerRow",
            typeof(int), typeof(CharacterActionGroupView),
            new PropertyMetadata(null));
        #endregion
    }
}
