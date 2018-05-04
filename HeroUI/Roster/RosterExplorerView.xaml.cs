using HeroUI;
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

namespace HeroVirtualTabletop.Roster
{
    /// <summary>
    /// Interaction logic for RosterExplorerView.xaml
    /// </summary>
    public partial class RosterExplorerView : UserControl
    {
        private RosterExplorerViewModelImpl viewModel;
        private const string CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY = "CrowdMemberDragFromCrowdExplorer";
        Dictionary<string, bool> rosterGroupExpansionStates = new Dictionary<string, bool>();

        public RosterExplorerView()
        {
            InitializeComponent();
        }

        private void RosterExplorerView_Loaded(object sender, RoutedEventArgs e)
        {
            this.viewModel = this.DataContext as RosterExplorerViewModelImpl;

            this.viewModel.RosterUpdated -= this.viewModel_RosterUpdated;
            this.viewModel.RosterUpdated += this.viewModel_RosterUpdated;
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.LeftButton == MouseButtonState.Pressed)
            {
                var groupbox = ControlUtilities.GetTemplateAncestorByType(e.OriginalSource as TextBlock, typeof(GroupBox));
                var itemsPres = ControlUtilities.GetDescendantByType(groupbox, typeof(ItemsPresenter)) as ItemsPresenter;
                var vStackPanel = VisualTreeHelper.GetChild(itemsPres as DependencyObject, 0) as VirtualizingStackPanel;
                foreach (ListBoxItem item in vStackPanel.Children)
                {
                    item.IsSelected = true;
                }
                e.Handled = true;
            }
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                
            }
            // This is the second mouse click.
            else if (e.ClickCount == 2)
            {
                
            }
            else if (e.ClickCount == 3)
            {
                
            }
            else if (e.ClickCount == 4)
            {
                
            }
        }

        private void RosterViewListBox_Drop(object sender, DragEventArgs e)
        {
            this.viewModel.ImportRosterMemberFromCrowdExplorer();
        }

        private void RosterViewListBox_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void viewModel_RosterUpdated(object sender, EventArgs e)
        {
            CollectionViewSource source = (CollectionViewSource)(this.Resources["ParticipantsView"]);
            ListCollectionView view = (ListCollectionView)source.View;
            if (view != null && view.Groups != null && view.Groups.Count > 1)
            {
                view.Refresh();
                foreach (CollectionViewGroup cvg in view.Groups)
                {
                    if (rosterGroupExpansionStates.ContainsKey(cvg.Name.ToString()))
                    {
                        bool isExpanded = rosterGroupExpansionStates[cvg.Name.ToString()];
                        if (isExpanded)
                        {
                            GroupItem groupItem = this.RosterViewListBox.ItemContainerGenerator.ContainerFromItem(cvg) as GroupItem;
                            if (groupItem != null)
                            {
                                groupItem.UpdateLayout();
                                Expander expander = ControlUtilities.GetDescendantByType(groupItem, typeof(Expander)) as Expander;
                                if (expander != null)
                                {
                                    expander.IsExpanded = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ExpanderOptionGroup_ExpansionChanged(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            CollectionViewGroup cvg = expander.DataContext as CollectionViewGroup;
            if (rosterGroupExpansionStates.ContainsKey(cvg.Name.ToString()))
                rosterGroupExpansionStates[cvg.Name.ToString()] = expander.IsExpanded;
            else
                rosterGroupExpansionStates.Add(cvg.Name.ToString(), expander.IsExpanded);
        }  
    }
}
