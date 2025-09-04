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

namespace CheckComboBoxExample
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void ShowSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedInfo = new StringBuilder();
            selectedInfo.AppendLine($"选中了 {_viewModel.SelectedItems.Count} 个项目:");

            foreach (var item in _viewModel.SelectedItems)
            {
                selectedInfo.AppendLine($"ID: {item.Id}, 名称: {item.Name}, 类别: {item.Category}");
            }

            MessageBox.Show(selectedInfo.ToString(), "选中项目信息", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SelectFirstThree_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectItemsByIds(1, 2, 3);
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearSelection();
        }

        private void AddNewItem_Click(object sender, RoutedEventArgs e)
        {
            var newId = _viewModel.AllItems.Max(item => item.Id) + 1;
            var newItem = new Item
            {
                Id = newId,
                Name = $"新项目{newId}",
                Category = "其他"
            };

            _viewModel.AllItems.Add(newItem);
            MessageBox.Show($"已添加新项目: {newItem.Name}", "添加成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GetSelectedIds_Click(object sender, RoutedEventArgs e)
        {
            var selectedIds = _viewModel.SelectedIds.ToList();
            if (selectedIds.Any())
            {
                MessageBox.Show($"选中的ID: {string.Join(", ", selectedIds)}", "选中ID",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("没有选中任何项目", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
