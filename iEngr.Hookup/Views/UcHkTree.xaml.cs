using iEngr.Hookup.ViewModels;
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

namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UcHkTree.xaml 的交互逻辑
    /// </summary>
    public partial class UcHkTree : UserControl
    {
        private HkTreeViewModel _viewModel;
        public UcHkTree()
        {
            InitializeComponent();
            _viewModel = new HkTreeViewModel();
            this.DataContext = _viewModel;
            this.PreviewKeyDown += UcHkTree_PreviewKeyDown;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is HkTreeItem newItem && DataContext is HkTreeViewModel viewModel)
            {
                viewModel.SelectedItem = newItem;
                viewModel.LastSelectedItem = newItem;
            }
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView != null)
            {
                var hitTestResult = VisualTreeHelper.HitTest(treeView, e.GetPosition(treeView));
                if (hitTestResult != null)
                {
                    var treeViewItem = FindParent<TreeViewItem>(hitTestResult.VisualHit);
                    if (treeViewItem != null)
                    {
                        treeViewItem.IsSelected = true;
                        e.Handled = true;
                    }
                }
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = sender as TreeViewItem;
            if (treeViewItem != null && treeViewItem.DataContext is HkTreeItem item)
            {
                // 选中该项
                treeViewItem.IsSelected = true;

                if (DataContext is HkTreeViewModel viewModel)
                {
                    viewModel.SelectedItem = item;
                    viewModel.LastSelectedItem = item;
                }

                e.Handled = true;
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
        // 在MainWindow.xaml.cs中添加Delete键支持
        private void UcHkTree_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel == null) return;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.C:
                        _viewModel.CopyCommand.Execute(_viewModel.SelectedItem);
                        e.Handled = true;
                        break;
                    case Key.X:
                        _viewModel.CutCommand.Execute(_viewModel.SelectedItem);
                        e.Handled = true;
                        break;
                    case Key.V:
                        _viewModel.PasteCommand.Execute(_viewModel.SelectedItem);
                        e.Handled = true;
                        break;
                    case Key.E:
                        _viewModel.ExpandAllCommand.Execute(_viewModel.SelectedItem);
                        e.Handled = true;
                        break;
                    case Key.W:
                        _viewModel.CollapseAllCommand.Execute(_viewModel.SelectedItem);
                        e.Handled = true;
                        break;
                }
            }
            else if (e.Key == Key.Delete)
            {
                // 处理Delete键删除
                _viewModel.DeleteCommand.Execute(_viewModel.SelectedItem);
                e.Handled = true;
            }
        }
    }
}