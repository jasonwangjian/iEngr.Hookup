using System.Collections;
using System.Diagnostics;
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
                new PropertyMetadata(null, OnSelectedItemsChanged));

        public static IList GetSelectedItems(DependencyObject obj)
            => (IList)obj.GetValue(SelectedItemsProperty);

        public static void SetSelectedItems(DependencyObject obj, IList value)
            => obj.SetValue(SelectedItemsProperty, value);

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine($"SelectedItems changed: Old={e.OldValue}, New={e.NewValue}");

            if (d is DataGrid dataGrid)
            {
                dataGrid.SelectionChanged -= DataGrid_SelectionChanged;

                // 如果新的 SelectedItems 有值，强制同步到 DataGrid
                if (e.NewValue is IList newSelectedItems)
                {
                    dataGrid.SelectedItems.Clear();
                    foreach (var item in newSelectedItems)
                        dataGrid.SelectedItems.Add(item);
                }

                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            }
        }
        private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            var selectedItems = GetSelectedItems(dataGrid);

            if (selectedItems == null) return;

            // 避免循环更新
            dataGrid.SelectionChanged -= DataGrid_SelectionChanged;

            try
            {
                foreach (var item in e.RemovedItems)
                    selectedItems.Remove(item);

                foreach (var item in e.AddedItems)
                    if (!selectedItems.Contains(item))
                        selectedItems.Add(item);
            }
            finally
            {
                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            }
        }
    }
}
