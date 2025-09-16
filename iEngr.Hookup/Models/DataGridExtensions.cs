using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace iEngr.Hookup.Models
{
    public static class ScrollSyncBehavior
    {
        // 同步组名称属性
        public static readonly DependencyProperty SyncGroupProperty =
            DependencyProperty.RegisterAttached("SyncGroup", typeof(string), typeof(ScrollSyncBehavior),
                new PropertyMetadata(null, OnSyncGroupChanged));

        // 是否启用同步属性
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ScrollSyncBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        private static readonly Dictionary<string, List<DataGrid>> _syncGroups = new Dictionary<string, List<DataGrid>>();

        public static string GetSyncGroup(DependencyObject obj) => (string)obj.GetValue(SyncGroupProperty);
        public static void SetSyncGroup(DependencyObject obj, string value) => obj.SetValue(SyncGroupProperty, value);

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void OnSyncGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                // 从旧组中移除
                if (e.OldValue is string oldGroup && _syncGroups.ContainsKey(oldGroup))
                {
                    _syncGroups[oldGroup].Remove(dataGrid);
                    dataGrid.Loaded -= DataGrid_Loaded;
                }

                // 添加到新组
                if (e.NewValue is string newGroup && !string.IsNullOrEmpty(newGroup))
                {
                    if (!_syncGroups.ContainsKey(newGroup))
                    {
                        _syncGroups[newGroup] = new List<DataGrid>();
                    }

                    if (!_syncGroups[newGroup].Contains(dataGrid))
                    {
                        _syncGroups[newGroup].Add(dataGrid);
                        dataGrid.Loaded += DataGrid_Loaded;

                        // 如果已经加载，立即设置监听
                        if (dataGrid.IsLoaded)
                        {
                            SetupScrollSyncForDataGrid(dataGrid);
                        }
                    }
                }
            }
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if ((bool)e.NewValue)
                {
                    dataGrid.Loaded += DataGrid_Loaded;
                    if (dataGrid.IsLoaded)
                    {
                        SetupScrollSyncForDataGrid(dataGrid);
                    }
                }
                else
                {
                    dataGrid.Loaded -= DataGrid_Loaded;
                    // 从所有组中移除
                    foreach (var group in _syncGroups.Values)
                    {
                        group.Remove(dataGrid);
                    }
                }
            }
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                SetupScrollSyncForDataGrid(dataGrid);
            }
        }

        private static void SetupScrollSyncForDataGrid(DataGrid dataGrid)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(dataGrid);
            if (scrollViewer != null)
            {
                // 移除旧的事件处理程序
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer sourceScrollViewer)
            {
                var sourceDataGrid = FindParentDataGrid(sourceScrollViewer);
                if (sourceDataGrid == null) return;

                var groupName = GetSyncGroup(sourceDataGrid);
                if (string.IsNullOrEmpty(groupName) || !_syncGroups.ContainsKey(groupName)) return;

                foreach (var targetDataGrid in _syncGroups[groupName].Where(dg => dg != sourceDataGrid))
                {
                    var targetScrollViewer = FindVisualChild<ScrollViewer>(targetDataGrid);
                    if (targetScrollViewer != null)
                    {
                        targetScrollViewer.ScrollToHorizontalOffset(sourceScrollViewer.HorizontalOffset);
                        targetScrollViewer.ScrollToVerticalOffset(sourceScrollViewer.VerticalOffset);
                    }
                }
            }
        }

        private static DataGrid FindParentDataGrid(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is DataGrid dataGrid)
                    return dataGrid;

                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
    }
    public static class DataGridColumnSyncBehavior
    {
        private static readonly Dictionary<DataGrid, List<DataGrid>> _syncGroups = new Dictionary<DataGrid, List<DataGrid>>();

        public static readonly DependencyProperty SyncWithProperty =
            DependencyProperty.RegisterAttached("SyncWith", typeof(DataGrid), typeof(DataGridColumnSyncBehavior),
                new PropertyMetadata(null, OnSyncWithChanged));

        public static DataGrid GetSyncWith(DependencyObject obj) => (DataGrid)obj.GetValue(SyncWithProperty);
        public static void SetSyncWith(DependencyObject obj, DataGrid value) => obj.SetValue(SyncWithProperty, value);

        private static void OnSyncWithChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid targetGrid)
            {
                if (e.OldValue is DataGrid oldSourceGrid)
                {
                    UnregisterSync(oldSourceGrid, targetGrid);
                }

                if (e.NewValue is DataGrid newSourceGrid)
                {
                    RegisterSync(newSourceGrid, targetGrid);
                }
            }
        }

        private static void RegisterSync(DataGrid sourceGrid, DataGrid targetGrid)
        {
            if (!_syncGroups.ContainsKey(sourceGrid))
            {
                _syncGroups[sourceGrid] = new List<DataGrid>();
                // 监听源DataGrid的列宽变化
                sourceGrid.Loaded += SourceGrid_Loaded;
                foreach (var column in sourceGrid.Columns)
                {
                    column.Width = column.Width; // 触发一次同步
                }
            }

            if (!_syncGroups[sourceGrid].Contains(targetGrid))
            {
                _syncGroups[sourceGrid].Add(targetGrid);
            }

            // 初始同步
            SyncColumns(sourceGrid, targetGrid);
        }

        private static void UnregisterSync(DataGrid sourceGrid, DataGrid targetGrid)
        {
            if (_syncGroups.ContainsKey(sourceGrid))
            {
                _syncGroups[sourceGrid].Remove(targetGrid);
                if (_syncGroups[sourceGrid].Count == 0)
                {
                    _syncGroups.Remove(sourceGrid);
                    sourceGrid.Loaded -= SourceGrid_Loaded;
                }
            }
        }

        private static void SourceGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid sourceGrid && _syncGroups.ContainsKey(sourceGrid))
            {
                // 为每一列添加监听
                foreach (var column in sourceGrid.Columns)
                {
                    var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
                    descriptor.RemoveValueChanged(column, Column_WidthChanged);
                    descriptor.AddValueChanged(column, Column_WidthChanged);
                }
            }
        }

        private static void Column_WidthChanged(object sender, EventArgs e)
        {
            if (sender is DataGridColumn changedColumn)
            {
                var sourceGrid = FindParentDataGrid(changedColumn);
                if (sourceGrid != null && _syncGroups.ContainsKey(sourceGrid))
                {
                    int columnIndex = sourceGrid.Columns.IndexOf(changedColumn);
                    foreach (var targetGrid in _syncGroups[sourceGrid])
                    {
                        if (targetGrid.Columns.Count > columnIndex)
                        {
                            targetGrid.Columns[columnIndex].Width = changedColumn.Width;
                        }
                    }
                }
            }
        }

        private static DataGrid FindParentDataGrid(DataGridColumn column)
        {
            foreach (var entry in _syncGroups)
            {
                if (entry.Key.Columns.Contains(column))
                {
                    return entry.Key;
                }
            }
            return null;
        }

        private static void SyncColumns(DataGrid source, DataGrid target)
        {
            if (source.Columns.Count != target.Columns.Count) return;

            for (int i = 0; i < source.Columns.Count; i++)
            {
                target.Columns[i].Width = source.Columns[i].Width;
            }
        }
    }
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
