﻿using System;
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

namespace HeroVirtualTabletop.Movement
{
    /// <summary>
    /// Interaction logic for MovementEditorView.xaml
    /// </summary>
    public partial class MovementEditorView : UserControl
    {
        private MovementEditorViewModel viewModel;
        public MovementEditorView()
        {
            InitializeComponent();
        }
        private void MovementEditorView_Loaded(object sender, RoutedEventArgs e)
        {
            this.viewModel = this.DataContext as MovementEditorViewModel;

            this.viewModel.EditModeEnter -= viewModel_EditModeEnter;
            this.viewModel.EditModeLeave -= viewModel_EditModeLeave;
            this.viewModel.MovementAdded -= viewModel_MovementAdded;

            this.viewModel.EditModeEnter += viewModel_EditModeEnter;
            this.viewModel.EditModeLeave += viewModel_EditModeLeave;
            this.viewModel.MovementAdded += viewModel_MovementAdded;
        }
        private void viewModel_EditModeEnter(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            Grid grid = comboBox.Parent as Grid;
            TextBox textBox = grid.Children[2] as TextBox;
            textBox.Visibility = Visibility.Visible;
            textBox.Focus();
            textBox.SelectAll();
        }

        private void viewModel_EditModeLeave(object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            txtBox.Visibility = Visibility.Hidden;
            BindingExpression expression = txtBox.GetBindingExpression(TextBox.TextProperty);
            expression.UpdateSource();
        }

        private void viewModel_MovementAdded(object sender, EventArgs e)
        {
            this.viewModel.EnterMovementEditMode(this.comboBoxMovements);
        }
    }
}
