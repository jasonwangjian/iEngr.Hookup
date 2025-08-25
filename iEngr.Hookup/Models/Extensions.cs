using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace iEngr.Hookup.Models
{
    public static class DataGridExtensions
    {
        public static readonly DependencyProperty ClearSortingProperty =
            DependencyProperty.RegisterAttached("ClearSorting", typeof(bool), typeof(DataGridExtensions),
                new PropertyMetadata(false, OnClearSortingChanged));

        public static bool GetClearSorting(DataGrid obj) => (bool)obj.GetValue(ClearSortingProperty);
        public static void SetClearSorting(DataGrid obj, bool value) => obj.SetValue(ClearSortingProperty, value);

        private static void OnClearSortingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid && (bool)e.NewValue)
            {
                ClearDataGridSorting(dataGrid);
                SetClearSorting(dataGrid, false); // 重置
            }
        }

        public static void ClearDataGridSorting(DataGrid dataGrid)
        {
            // 清除列头的排序指示器
            foreach (var column in dataGrid.Columns)
            {
                column.SortDirection = null;
            }

            // 清除集合视图的排序
            var collectionView = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (collectionView != null)
            {
                collectionView.SortDescriptions.Clear();
            }

            // 刷新显示
            dataGrid.Items.Refresh();
        }
    }
}
