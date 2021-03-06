﻿using Framework.WPF.Extensions;
using HeroUI;
using HeroVirtualTabletop.Common;
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

namespace HeroVirtualTabletop.Crowd
{
    /// <summary>
    /// Interaction logic for CrowdMemberExplorerView.xaml
    /// </summary>
    public partial class CrowdMemberExplorerView : UserControl
    {
        private CrowdMemberExplorerViewModelImpl viewModel;
        private Crowd selectedCrowdRoot;
        private const string CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY = "CrowdMemberDragFromCrowdExplorer";
        
        public CrowdMemberExplorerView()
        {
            InitializeComponent();
        }
        private async void CrowdMemberExplorerView_Loaded(object sender, RoutedEventArgs e)
        {
            this.viewModel = this.DataContext as CrowdMemberExplorerViewModelImpl;
            this.viewModel.EditModeEnter -= viewModel_EditModeEnter;
            this.viewModel.EditModeLeave -= viewModel_EditModeLeave;
            this.viewModel.EditNeeded -= viewModel_EditNeeded;
            this.viewModel.ExpansionUpdateNeeded -= viewModel_ExpansionUpdateNeeded;
            this.viewModel.FlattenNumberRequired -= viewModel_FlattenNumberRequired;
            this.viewModel.FlattenNumberEntryFinished -= viewModel_FlattenNumberEntryFinished;
            this.viewModel.RefreshViewRequired -= viewModel_RefreshViewRequired;
            this.viewModel.UpdateViewRequired -= viewModel_UpdateViewRequired;
            this.viewModel.EditModeEnter += viewModel_EditModeEnter;
            this.viewModel.EditModeLeave += viewModel_EditModeLeave;
            this.viewModel.EditNeeded += viewModel_EditNeeded;
            this.viewModel.ExpansionUpdateNeeded += viewModel_ExpansionUpdateNeeded;
            this.viewModel.FlattenNumberRequired += viewModel_FlattenNumberRequired;
            this.viewModel.FlattenNumberEntryFinished += viewModel_FlattenNumberEntryFinished;
            this.viewModel.RefreshViewRequired += viewModel_RefreshViewRequired;
            this.viewModel.UpdateViewRequired += viewModel_UpdateViewRequired;
            await this.viewModel.LoadCrowdCollection();
        }

        private void viewModel_EditNeeded(object sender, CustomEventArgs<string> e)
        {
            CrowdMember modelToSelect = sender as CrowdMember;
            if (sender == null) // need to unselect
            {
                UnselectSelectedNodeInTreeview();
            }
            else
            {
                bool itemFound = false;
                TextBox txtBox = null;
                treeViewCrowd.UpdateLayout();
                
                if (sender is Crowd)
                {
                    TreeViewItem item = FindCrowdNodeInTree(sender as Crowd);
                    if(item != null)
                    {
                        itemFound = true;
                        txtBox = FindTextBoxInTemplate(item);
                    }
                }

                if (!itemFound)
                {
                    bool isDragDropInProgress = e != null && e.Value == "EditAfterDragDrop" && this.currentDropItemNodeParent != null;
                    TreeViewItem tvi = GetParentNodeForItemToSelect(isDragDropInProgress);
                    TreeViewItem item = FindNodeThatRepresentsCrowdMember(tvi, modelToSelect);
                    if(item != null)
                    {
                        itemFound = true;
                        txtBox = FindTextBoxInTemplate(item);
                    }
                }
                if (txtBox != null)
                    this.viewModel.EnterEditMode(txtBox);
            }
        }

        private void UnselectSelectedNodeInTreeview()
        {
            DependencyObject dObject = treeViewCrowd.GetItemFromSelectedObject(treeViewCrowd.SelectedItem);
            TreeViewItem tvi = dObject as TreeViewItem; // got the selected treeviewitem
            if (tvi != null)
            {
                tvi.IsSelected = false;
                this.selectedCrowdRoot = null;
            }
        }

        private TreeViewItem FindCrowdNodeInTree(Crowd modelToSelect)
        {
            TreeViewItem item = null;
            if (this.viewModel.SelectedCrowdMember == null) // A new crowd has been added to the collection
            {
                for (int i = 0; i < treeViewCrowd.Items.Count; i++)
                {
                    item = treeViewCrowd.ItemContainerGenerator.ContainerFromItem(treeViewCrowd.Items[i]) as TreeViewItem;
                    if (item != null)
                    {
                        var model = item.DataContext as CrowdMember;
                        if (model.Name == modelToSelect.Name)
                        {
                            item.IsSelected = true;
                            
                            this.viewModel.SelectedCrowdMember = model as Crowd;
                            break;
                        }
                    }
                }
            }

            return item;
        }

        private TreeViewItem FindNodeThatRepresentsCrowdMember(TreeViewItem tviParent, CrowdMember modelToSelect)
        {
            TreeViewItem item = null;
            if (tviParent != null)
            {
                CrowdMember model = tviParent.DataContext as CrowdMember;
                if (tviParent.Items != null)
                {
                    tviParent.IsExpanded = true;
                    tviParent.UpdateLayout();
                    for (int i = 0; i < tviParent.Items.Count; i++)
                    {
                        item = tviParent.ItemContainerGenerator.ContainerFromItem(tviParent.Items[i]) as TreeViewItem;
                        if (item != null)
                        {
                            model = item.DataContext as CrowdMember;
                            if (model.Name == modelToSelect.Name)
                            {
                                item.IsSelected = true;
                                item.UpdateLayout();
                                if (model is Crowd)
                                {
                                    this.viewModel.SelectedCrowdMember = model as Crowd;
                                    this.viewModel.SelectedCrowdParent = tviParent.DataContext as Crowd;
                                    this.viewModel.SelectedCharacterCrowdMember = null;
                                }
                                else
                                {
                                    this.viewModel.SelectedCharacterCrowdMember = model as CharacterCrowdMemberImpl;
                                    this.viewModel.SelectedCrowdMember = tviParent.DataContext as Crowd;
                                }
                                if (this.selectedCrowdRoot == null)
                                    this.selectedCrowdRoot = tviParent.DataContext as CrowdImpl;
                                break;
                            }
                        }
                    }
                }
            }
            return item;
        }

        private TreeViewItem GetParentNodeForItemToSelect(bool isDragDropInProgress)
        {
            DependencyObject dObject = null;
            if (isDragDropInProgress)
            {
                dObject = currentDropItemNodeParent;
            }
            else
            {
                if (this.selectedCrowdRoot != null && this.viewModel.SelectedCrowdMember != null)
                {
                    TreeViewItem itemParent = treeViewCrowd.ItemContainerGenerator.ContainerFromItem(this.selectedCrowdRoot) as TreeViewItem;
                    dObject = FindTreeViewItemUnderTreeViewItemByModelName(itemParent, this.viewModel.SelectedCrowdMember.Name);
                }
                else
                    dObject = treeViewCrowd.GetItemFromSelectedObject(this.viewModel.SelectedCrowdMember);
            }
            TreeViewItem tvi = dObject as TreeViewItem;
            return tvi;
        }

        private TextBox FindTextBoxInTemplate(TreeViewItem item)
        {
            TextBox textBox = ControlUtilities.GetDescendantByType(item, typeof(TextBox)) as TextBox;
            return textBox;
        }

        private void viewModel_EditModeEnter(object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            Grid grid = txtBox.Parent as Grid;
            TextBox textBox = grid.Children[1] as TextBox;
            textBox.Text = txtBox.Text;
            textBox.Visibility = Visibility.Visible;
            textBox.Focus();
            textBox.SelectAll();
        }

        private void viewModel_EditModeLeave(object sender, CustomEventArgs<string> e)
        {
            TextBox txtBox = sender as TextBox;
            Grid grid = txtBox.Parent as Grid;
            TextBox otherTextBox = grid.Children[0] as TextBox;

            txtBox.Visibility = Visibility.Hidden;
            if (e != null && !string.IsNullOrEmpty(e.Value))
                txtBox.Text = e.Value;
            BindingExpression expression = txtBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
            otherTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            // Now recursively update tree because for some reason the character nodes don't update themselves!!!
            UpdateTreeviewRecursively();
        }

        private void UpdateTreeviewRecursively()
        {
            foreach (var item in treeViewCrowd.Items)
            {
                var tvi = treeViewCrowd.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                UpdateItemsRecursivelyToUpdateNodeText(tvi, item);
            }
        }

        private void viewModel_UpdateViewRequired(object sender, EventArgs e)
        {
            UpdateTreeviewRecursively();
        }

        private void viewModel_FlattenNumberRequired(object sender, EventArgs e)
        {
            gridFlattenNumber.Visibility = Visibility.Visible;
            intUpDownFlattenNum.Focus();
        }

        private void viewModel_FlattenNumberEntryFinished(object sender, EventArgs e)
        {
            gridFlattenNumber.Visibility = Visibility.Collapsed;
        }
        private void treeViewCrowd_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);

            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                TreeViewItem item = GetRootTreeViewItemParent(treeViewItem);
                if (item != null)
                    this.selectedCrowdRoot = item.DataContext as HeroVirtualTabletop.Crowd.Crowd;
                else
                    this.selectedCrowdRoot = null;
                if (treeViewItem.DataContext is Crowd)
                {
                    treeViewItem = GetImmediateTreeViewItemParent(treeViewItem);
                    if (treeViewItem != null)
                        this.viewModel.SelectedCrowdParent = treeViewItem.DataContext as Crowd;
                    else
                        this.viewModel.SelectedCrowdParent = null;

                }
                else
                    this.viewModel.SelectedCrowdParent = null;
            }
        }

        private void UpdateItemsRecursivelyToUpdateNodeText(TreeViewItem tvi, object obj)
        {
            tvi.UpdateLayout();
            TextBox textBox = FindTextBoxInTemplate(tvi);
            textBox?.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            foreach (var innerItem in tvi.Items)
            {
                var tviInner = tvi.ItemContainerGenerator.ContainerFromItem(innerItem) as TreeViewItem;
                if(tviInner != null)
                    UpdateItemsRecursivelyToUpdateNodeText(tviInner, innerItem);
            }
        }

        private TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private TreeViewItem GetContainerFromItem(ItemsControl parent, object item)
        {
            var found = parent.ItemContainerGenerator.ContainerFromItem(item);
            if (found == null)
            {
                for (int i = 0; i < parent.Items.Count; i++)
                {
                    var childContainer = parent.ItemContainerGenerator.ContainerFromIndex(i) as ItemsControl;
                    TreeViewItem childFound = null;
                    if (childContainer != null)
                    {
                        bool expanded = (childContainer as TreeViewItem).IsExpanded;
                        (childContainer as TreeViewItem).IsExpanded = true;
                        childFound = GetContainerFromItem(childContainer, item);
                        (childContainer as TreeViewItem).IsExpanded = childFound == null ? expanded : true;
                    }
                    if (childFound != null)
                    {
                        (childContainer as TreeViewItem).IsExpanded = true;
                        return childFound;
                    }

                }
            }
            return found as TreeViewItem;
        }

        private TreeViewItem GetImmediateTreeViewItemParent(TreeViewItem treeViewItem)
        {
            DependencyObject dObject = VisualTreeHelper.GetParent(treeViewItem); // got the immediate parent
            treeViewItem = dObject as TreeViewItem; // now get first treeview item parent
            while (treeViewItem == null)
            {
                dObject = VisualTreeHelper.GetParent(dObject);
                treeViewItem = dObject as TreeViewItem;
                if (dObject is TreeView)
                    break;
            }
            return treeViewItem;
        }

        private TreeViewItem GetRootTreeViewItemParent(TreeViewItem treeViewItem)
        {
            DependencyObject dObject = VisualTreeHelper.GetParent(treeViewItem); // got the immediate parent
            treeViewItem = dObject as TreeViewItem; // now get first treeview item parent
            while (true)
            {
                dObject = VisualTreeHelper.GetParent(dObject);
                if (dObject is TreeViewItem)
                    treeViewItem = dObject as TreeViewItem;
                else if (dObject is TreeView)
                    break;
            }
            return treeViewItem;
        }

        private void treeViewCrowd_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                TreeViewItem item = GetRootTreeViewItemParent(treeViewItem);
                if (item != null)
                    this.selectedCrowdRoot = item.DataContext as Crowd;
                else
                    this.selectedCrowdRoot = null;
                if (treeViewItem.DataContext is Crowd)
                {
                    treeViewItem = GetImmediateTreeViewItemParent(treeViewItem);
                    if (treeViewItem != null)
                        this.viewModel.SelectedCrowdParent = treeViewItem.DataContext as Crowd;
                    else
                        this.viewModel.SelectedCrowdParent = null;

                }
                else
                    this.viewModel.SelectedCrowdParent = null;
            }
        }

        private TreeViewItem FindTreeViewItemUnderTreeViewItemByModelName(TreeViewItem tvi, string modelName)
        {
            TreeViewItem treeViewItemRet = null;
            if (tvi != null)
            {
                CrowdMember model = tvi.DataContext as CrowdMember;
                if (model.Name == modelName)
                {
                    return tvi;
                }
                else if (tvi.Items != null)
                {
                    for (int i = 0; i < tvi.Items.Count; i++)
                    {
                        TreeViewItem item = tvi.ItemContainerGenerator.ContainerFromItem(tvi.Items[i]) as TreeViewItem;
                        var treeViewItem = FindTreeViewItemUnderTreeViewItemByModelName(item, modelName);
                        if (treeViewItem != null)
                        {
                            treeViewItemRet = treeViewItem;
                            break;
                        }
                    }
                }
            }
            return treeViewItemRet;
        }

        #region TreeView Expansion Management

        private void viewModel_ExpansionUpdateNeeded(object sender, CustomEventArgs<ExpansionUpdateEvent> e)
        {
            Crowd crowdModel = sender as Crowd;
            DependencyObject dObject = null;
            ExpansionUpdateEvent updateEvent = e.Value;
            if (updateEvent == ExpansionUpdateEvent.Filter)
            {
                ExpandMatchedNode(sender);
            }
            else if (updateEvent == ExpansionUpdateEvent.DragDrop)
            {
                if (this.currentDropItemNode != null)
                    this.currentDropItemNode.IsExpanded = true;
            }
            else
            {
                if (this.selectedCrowdRoot != null && crowdModel != null)
                {
                    TreeViewItem item = treeViewCrowd.ItemContainerGenerator.ContainerFromItem(this.selectedCrowdRoot) as TreeViewItem;
                    dObject = FindTreeViewItemUnderTreeViewItemByModelName(item, crowdModel.Name);
                    if (dObject == null)
                        dObject = treeViewCrowd.GetItemFromSelectedObject(crowdModel);
                }
                else
                    dObject = treeViewCrowd.GetItemFromSelectedObject(crowdModel);
                TreeViewItem tvi = dObject as TreeViewItem;
                if (tvi != null)
                {
                    CrowdMember model = tvi.DataContext as CrowdMember;
                    if (tvi.Items != null && tvi.Items.Count > 0)
                    {
                        if (updateEvent != ExpansionUpdateEvent.Delete)
                            tvi.IsExpanded = true;
                        else
                            UpdateExpansions(tvi);
                    }
                    else
                        tvi.IsExpanded = false;
                }
            }
        }
        /// <summary>
        /// This will recursively make the nodes unexpanded if there are no children in it. Otherwise it will hold the current state
        /// </summary>
        /// <param name="tvi"></param>
        private void UpdateExpansions(TreeViewItem tvi)
        {
            if (tvi != null)
            {
                if (tvi.Items != null && tvi.Items.Count > 0)
                {
                    for (int i = 0; i < tvi.Items.Count; i++)
                    {
                        TreeViewItem item = tvi.ItemContainerGenerator.ContainerFromItem(tvi.Items[i]) as TreeViewItem;
                        UpdateExpansions(item);
                    }
                }
                else
                    tvi.IsExpanded = false;
            }
        }

        /// <summary>
        /// This will expand a matched item and its matched children
        /// </summary>
        /// <param name="sender"></param>
        private void ExpandMatchedNode(object sender)
        {
            Crowd crowdModel = sender as Crowd;
            if (crowdModel.MatchesFilter)
            {
                DependencyObject dObject = treeViewCrowd.GetItemFromSelectedObject(crowdModel);
                TreeViewItem tvi = dObject as TreeViewItem;
                if (tvi != null)
                {
                    tvi.IsExpanded = true;
                    ExpandMatchedItems(tvi);
                }
            }
        }
        /// <summary>
        /// This will recursively expand a matched item and its matched children
        /// </summary>
        /// <param name="tvi"></param>
        private void ExpandMatchedItems(TreeViewItem tvi)
        {
            if (tvi != null)
            {
                tvi.UpdateLayout();
                if (tvi.Items != null && tvi.Items.Count > 0)
                {
                    for (int i = 0; i < tvi.Items.Count; i++)
                    {
                        TreeViewItem item = tvi.ItemContainerGenerator.ContainerFromItem(tvi.Items[i]) as TreeViewItem;
                        if (item != null)
                        {
                            Crowd model = item.DataContext as Crowd;
                            if (model != null && model.MatchesFilter)
                            {
                                item.IsExpanded = true;
                                ExpandMatchedItems(item);
                            }
                            else
                                item.IsExpanded = false;
                        }
                    }
                }
            }
        }
        #endregion

        #region Refresh Tree View

        private void viewModel_RefreshViewRequired(object sender, EventArgs e)
        {
            this.RefreshTreeView();
        }

        private void RefreshTreeView()
        {
            this.treeViewCrowd.Items.Refresh();
        }

        #endregion

        #region Drag Drop

        bool isDragging = false;
        Point startPoint;
        TreeViewItem currentDropItemNode;
        TreeViewItem currentDropItemNodeParent;

        private void StartDrag(TreeView tv, MouseEventArgs e)
        {
            isDragging = true;
            try
            {
                // Get the dragged ListViewItem
                TreeView treeView = tv as TreeView;
                TreeViewItem treeViewItem =
                    ControlUtilities.FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
                if (treeViewItem != null)
                {
                    // Find the data behind the TreeViewItem
                    //AnimationElement elementBehind = (AnimationElement)treeView.ItemContainerGenerator.ItemFromContainer(treeViewItem);
                    var element = treeView.SelectedItem;
                    // Initialize the drag & drop operation
                    DataObject dragData = new DataObject(CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY, element);
                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                isDragging = false;
            }
        }
        private void treeViewCrowd_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Action action = delegate ()
            {
                if (e.LeftButton == MouseButtonState.Pressed && !isDragging)
                {
                    // Get the current mouse position
                    Point mousePos = e.GetPosition(null);
                    Vector diff = startPoint - mousePos;
                    if (
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        StartDrag(sender as TreeView, e);
                    }
                }
            };
            Application.Current.Dispatcher.BeginInvoke(action);
        }

        private void FindDropTarget(TreeView tv, out TreeViewItem itemNode, DragEventArgs dragEventArgs)
        {
            itemNode = null;

            DependencyObject k = VisualTreeHelper.HitTest(tv, dragEventArgs.GetPosition(tv)).VisualHit;

            while (k != null)
            {
                if (k is TreeViewItem)
                {
                    TreeViewItem treeNode = k as TreeViewItem;
                    if (treeNode.DataContext is Crowd || treeNode.DataContext is CharacterCrowdMember)
                    {
                        itemNode = treeNode;
                        break;
                    }
                }
                else if (k == tv)
                {
                    break;
                }

                k = VisualTreeHelper.GetParent(k);
            }
        }

        private void treeViewCrowd_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void treeViewCrowd_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY))
            {

                FindDropTarget((TreeView)sender, out currentDropItemNode, e);

                if (currentDropItemNode != null)
                {
                    var dropMember = (currentDropItemNode != null && currentDropItemNode.IsVisible ? currentDropItemNode.DataContext : null);
                    var dragMember = e.Data.GetData(CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY);
                    Crowd targetCrowd = null;
                    if (dropMember is Crowd)
                    {
                        targetCrowd = dropMember as Crowd;
                        currentDropItemNodeParent = currentDropItemNode;
                    }
                    else
                    {
                        currentDropItemNodeParent = GetImmediateTreeViewItemParent(currentDropItemNode);
                        targetCrowd = currentDropItemNodeParent != null ? currentDropItemNodeParent.DataContext as Crowd : null;
                    }

                    this.viewModel.DragDropSelectedCrowdMember(targetCrowd);
                }
            }
        }

        private void textBlockCrowdMember_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void textBlockCrowdMember_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY))
            {
                e.Handled = true;
            }
        }

        private void textBlockCrowdMember_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(CROWD_MEMBER_DRAG_FROM_CROWD_XPLORER_KEY))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }
        #endregion
    }
}
