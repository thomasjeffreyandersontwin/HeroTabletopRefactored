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
    /// Interaction logic for CharacterEditorView.xaml
    /// </summary>
    public partial class CharacterEditorView : UserControl
    {
        private CharacterEditorViewModel viewModel;

        private const string OPTION_GROUP_DRAG_KEY = "CharacterOptionGroupDrag";
        private const string OPTION_DRAG_KEY = "CharacterOptionDrag";

        public static Point OptionGroupDragStartPoint;
        public static string DraggingOptionGroupName;

        public CharacterEditorView()
        {
            InitializeComponent();
        }

        private void CharacterEditorView_Loaded(object sender, RoutedEventArgs e)
        {
            this.viewModel = this.DataContext as CharacterEditorViewModel;
        }

        #region Drag Drop Option Group

        bool isDragging = false;
        private void StartDrag(string draggingOptionGroupName, MouseEventArgs e)
        {
            isDragging = true;
            try
            {
                if (draggingOptionGroupName != null)
                {
                    // Initialize the drag & drop operation
                    DataObject dragData = new DataObject(OPTION_GROUP_DRAG_KEY, draggingOptionGroupName);
                    DragDrop.DoDragDrop(this.listViewOptionGroup, dragData, DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                isDragging = false;
                DraggingOptionGroupName = null; // try a class member (static)
            }
        }

        private void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Action action = delegate ()
            {
                if (e.LeftButton == MouseButtonState.Pressed && !isDragging && DraggingOptionGroupName != null)
                {
                    // Get the current mouse position
                    Point mousePos = e.GetPosition(null);
                    Vector diff = OptionGroupDragStartPoint - mousePos;
                    if (
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        StartDrag(DraggingOptionGroupName, e);
                    }
                }
            };
            Application.Current.Dispatcher.BeginInvoke(action);
        }

        private void ListView_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(OPTION_GROUP_DRAG_KEY) && !e.Data.GetDataPresent(OPTION_DRAG_KEY))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void FindDropTarget(ListView listView, out ListViewItem listViewItem, DragEventArgs dragEventArgs)
        {
            listViewItem = null;

            DependencyObject k = VisualTreeHelper.HitTest(listView, dragEventArgs.GetPosition(listView)).VisualHit;

            while (k != null)
            {
                if (k is ListViewItem)
                {
                    ListViewItem lvItem = k as ListViewItem;
                    if (lvItem.DataContext is CharacterActionGroupViewModel)
                    {
                        listViewItem = lvItem;
                        break;
                    }
                }
                else if (k == listView)
                {
                    break;
                }

                k = VisualTreeHelper.GetParent(k);
            }
        }

        private void ListView_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(OPTION_GROUP_DRAG_KEY))
            {
                string draggingOPtionGroupName = e.Data.GetData(OPTION_GROUP_DRAG_KEY) as string;
                ListViewItem listViewItem;
                FindDropTarget(sender as ListView, out listViewItem, e);
                if (listViewItem != null)
                {
                    CharacterActionGroupViewModel targetViewModel = listViewItem.DataContext as CharacterActionGroupViewModel;
                    if (targetViewModel.ActionGroup.Name != draggingOPtionGroupName)
                    {
                        var sourceViewModel = this.viewModel.CharacterActionGroups.FirstOrDefault(vm => vm.ActionGroup.Name == draggingOPtionGroupName);
                        int sourceIndex = this.viewModel.CharacterActionGroups.IndexOf(sourceViewModel);
                        int targetIndex = this.viewModel.CharacterActionGroups.IndexOf(targetViewModel);
                        this.viewModel.ReOrderActionGroups(sourceIndex, targetIndex);
                    }
                }
            }
        }

        #endregion

        private void ListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DraggingOptionGroupName = null;
        }

        private void SaveCharacter(object sender, RoutedEventArgs e)
        {
            //this.viewModel.SaveCharacter(null);
        }
    }
}
