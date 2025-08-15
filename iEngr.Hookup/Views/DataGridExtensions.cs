using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace iEngr.Hookup.Views
{
    public static class DataGridExtensions
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(DataGridExtensions),
                new PropertyMetadata(null, OnSelectedItemsChanged)
            );

        public static IList GetSelectedItems(DependencyObject obj)
            => (IList)obj.GetValue(SelectedItemsProperty);

        public static void SetSelectedItems(DependencyObject obj, IList value)
            => obj.SetValue(SelectedItemsProperty, value);

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            }
        }

        private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            var selectedItems = GetSelectedItems(dataGrid);

            if (selectedItems != null)
            {
                // 同步选中项到绑定的集合
                foreach (var item in e.AddedItems)
                    if (!selectedItems.Contains(item)) selectedItems.Add(item);

                foreach (var item in e.RemovedItems)
                    selectedItems.Remove(item);
            }
        }
    }
}
