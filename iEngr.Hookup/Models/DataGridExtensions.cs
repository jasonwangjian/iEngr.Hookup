using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace iEngr.Hookup.Models
{
    public static class DataGridComparisonHelper
    {
        #region ComparisonTarget 附加属性 - 使用x:Reference
        public static readonly DependencyProperty ComparisonTargetProperty =
            DependencyProperty.RegisterAttached(
                "ComparisonTarget",
                typeof(DataGrid),
                typeof(DataGridComparisonHelper),
                new PropertyMetadata(null, OnComparisonTargetChanged));

        public static DataGrid GetComparisonTarget(DependencyObject obj)
        {
            return (DataGrid)obj.GetValue(ComparisonTargetProperty);
        }

        public static void SetComparisonTarget(DependencyObject obj, DataGrid value)
        {
            obj.SetValue(ComparisonTargetProperty, value);
        }
        #endregion

        #region ComparisonSourceName 附加属性 - 通过名称查找
        public static readonly DependencyProperty ComparisonSourceNameProperty =
            DependencyProperty.RegisterAttached(
                "ComparisonSourceName",
                typeof(string),
                typeof(DataGridComparisonHelper),
                new PropertyMetadata(null, OnComparisonSourceNameChanged));

        public static string GetComparisonSourceName(DependencyObject obj)
        {
            return (string)obj.GetValue(ComparisonSourceNameProperty);
        }

        public static void SetComparisonSourceName(DependencyObject obj, string value)
        {
            obj.SetValue(ComparisonSourceNameProperty, value);
        }
        #endregion

        #region IsComparisonEnabled 附加属性
        public static readonly DependencyProperty IsComparisonEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsComparisonEnabled",
                typeof(bool),
                typeof(DataGridComparisonHelper),
                new PropertyMetadata(false, OnIsComparisonEnabledChanged));

        public static bool GetIsComparisonEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsComparisonEnabledProperty);
        }

        public static void SetIsComparisonEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsComparisonEnabledProperty, value);
        }
        #endregion

        #region IsComparisonById 附加属性
        public static readonly DependencyProperty IsComparisonByIdProperty =
            DependencyProperty.RegisterAttached(
                "IsComparisonById",
                typeof(bool),
                typeof(DataGridComparisonHelper),
                new PropertyMetadata(false, OnIsComparisonByIdChanged));

        public static bool GetIsComparisonById(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsComparisonByIdProperty);
        }

        public static void SetIsComparisonById(DependencyObject obj, bool value)
        {
            obj.SetValue(IsComparisonByIdProperty, value);
        }
        #endregion
        #region HighlightBrush 附加属性
        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.RegisterAttached(
                "HighlightBrush",
                typeof(Brush),
                typeof(DataGridComparisonHelper),
                new PropertyMetadata(Brushes.Red));

        public static Brush GetHighlightBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightBrushProperty);
        }

        public static void SetHighlightBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightBrushProperty, value);
        }
        #endregion

        private static readonly Dictionary<DataGrid, string> _dataGridNames = new Dictionary<DataGrid, string>();

        // 注册DataGrid名称的方法
        public static void RegisterDataGridName(DataGrid dataGrid, string name)
        {
            if (dataGrid != null && !string.IsNullOrEmpty(name))
            {
                _dataGridNames[dataGrid] = name;
            }
        }

        // 注销DataGrid名称的方法
        public static void UnregisterDataGridName(DataGrid dataGrid)
        {
            if (dataGrid != null)
            {
                _dataGridNames.Remove(dataGrid);
            }
        }

        private static void OnComparisonTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                UpdateComparison(dataGrid);
            }
        }

        private static void OnComparisonSourceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                FindAndSetComparisonTarget(dataGrid);
            }
        }

        private static void OnIsComparisonEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid && dataGrid.Items.Count > 0)
            {
                if ((bool)e.NewValue)
                {
                    // 如果使用名称查找，先尝试查找目标
                    if (GetComparisonTarget(dataGrid) == null)
                    {
                        FindAndSetComparisonTarget(dataGrid);
                    }
                    UpdateComparison(dataGrid);
                }
                else
                {
                    ClearHighlights(dataGrid);
                }
            }
        }
        private static void OnIsComparisonByIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid && dataGrid.Items.Count > 0)
            {
                // 如果使用名称查找，先尝试查找目标
                if (GetComparisonTarget(dataGrid) == null)
                {
                    FindAndSetComparisonTarget(dataGrid);
                }
                UpdateComparison(dataGrid);
            }
        }
        public static void ExecComparison(DataGrid sourceDataGrid, DataGrid targetDataGrid)
        {
            if (sourceDataGrid == null || targetDataGrid == null) return;
            if (GetIsComparisonById(sourceDataGrid))
                CompareDataGridsById(sourceDataGrid, targetDataGrid);
            else
                CompareDataGrids(sourceDataGrid, targetDataGrid);
        }
        private static void FindAndSetComparisonTarget(DataGrid sourceDataGrid)
        {
            string targetName = GetComparisonSourceName(sourceDataGrid);
            if (string.IsNullOrEmpty(targetName)) return;

            // 方法1: 从注册的字典中查找
            var targetDataGrid = _dataGridNames.FirstOrDefault(x => x.Value == targetName).Key;
            if (targetDataGrid != null)
            {
                SetComparisonTarget(sourceDataGrid, targetDataGrid);
                return;
            }

            // 方法2: 在应用程序范围内查找
            targetDataGrid = FindDataGridInApplication(targetName);
            if (targetDataGrid != null)
            {
                SetComparisonTarget(sourceDataGrid, targetDataGrid);
                return;
            }

            // 方法3: 延迟查找（当目标DataGrid尚未加载时）
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    var delayedTarget = FindDataGridInApplication(targetName);
                    if (delayedTarget != null)
                    {
                        SetComparisonTarget(sourceDataGrid, delayedTarget);
                        UpdateComparison(sourceDataGrid);
                    }
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        private static DataGrid FindDataGridInApplication(string dataGridName)
        {
            if (Application.Current == null) return null;

            foreach (Window window in Application.Current.Windows)
            {
                var dataGrid = FindVisualChild<DataGrid>(window, dataGridName);
                if (dataGrid != null) return dataGrid;
            }

            return null;
        }

        private static T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result && result.Name == name)
                    return result;

                var descendant = FindVisualChild<T>(child, name);
                if (descendant != null)
                    return descendant;
            }

            return null;
        }

        private static void UpdateComparison(DataGrid sourceDataGrid)
        {
            if (!GetIsComparisonEnabled(sourceDataGrid)) return;

            var targetDataGrid = GetComparisonTarget(sourceDataGrid);
            if (targetDataGrid == null) return;

            // 清除之前的高亮
            ClearHighlights(sourceDataGrid);
            ClearHighlights(targetDataGrid);

            // 执行比较
            if (GetIsComparisonById(sourceDataGrid))
                CompareDataGridsById(sourceDataGrid, targetDataGrid);
            else
                CompareDataGrids(sourceDataGrid, targetDataGrid);
        }

        // 比较逻辑（与之前相同，但为了完整性保留核心部分）
        private static void CompareDataGrids(DataGrid dataGrid1, DataGrid dataGrid2)
        {
            var items1 = dataGrid1.ItemsSource as IEnumerable;
            var items2 = dataGrid2.ItemsSource as IEnumerable;

            if (items1 == null || items2 == null) return;

            var list1 = items1.Cast<object>().ToList();
            var list2 = items2.Cast<object>().ToList();

            int minRowCount = System.Math.Min(list1.Count, list2.Count);
            var highlightBrush = GetHighlightBrush(dataGrid1);

            for (int rowIndex = 0; rowIndex < minRowCount; rowIndex++)
            {
                var item1 = list1[rowIndex];
                var item2 = list2[rowIndex];

                foreach (DataGridColumn column in dataGrid1.Columns)
                {
                    if (column.Visibility != Visibility.Visible) continue;

                    var correspondingColumn = FindCorrespondingColumn(dataGrid2, column);
                    if (correspondingColumn == null) continue;

                    string value1 = GetCellValue(item1, column);
                    string value2 = GetCellValue(item2, correspondingColumn);

                    if (!string.Equals(value1, value2))
                    {
                        HighlightCell(dataGrid1, rowIndex, column, highlightBrush);
                        HighlightCell(dataGrid2, rowIndex, correspondingColumn, highlightBrush);
                    }
                }
            }
            // 处理行数不同的情况
            if (list1.Count != list2.Count)
            {
                HighlightExtraRows(dataGrid1, list1.Count, list2.Count, highlightBrush);
                HighlightExtraRows(dataGrid2, list2.Count, list1.Count, highlightBrush);
            }
        }
        private static void CompareDataGridsById(DataGrid dataGrid1, DataGrid dataGrid2)
        {
            var items1 = dataGrid1.ItemsSource as IEnumerable;
            var items2 = dataGrid2.ItemsSource as IEnumerable;

            if (items1 == null || items2 == null) return;

            var list1 = items1.Cast<object>().ToList();
            var list2 = items2.Cast<object>().ToList();

            //首列必须是ID
            if (dataGrid1.Columns[0].Header.ToString().ToUpper() != "ID") return;
            if (dataGrid2.Columns[0].Header.ToString().ToUpper() != "ID") return;
            //if (list1.Any() && list1.First().GetType().GetProperty("ID") != null) return;
            //if (list2.Any() && list2.First().GetType().GetProperty("ID") != null) return;

            Dictionary<string, object> dic1 = new Dictionary<string, object>();
            for (int rowIndex = 0; rowIndex < list1.Count; rowIndex++)
            {
                var item1 = list1[rowIndex];
                string id = GetCellValue(item1, dataGrid1.Columns[0]);
                if (!string.IsNullOrEmpty(id) && !dic1.ContainsKey(id))
                    dic1.Add(id, list1[rowIndex]);
                else
                    HighlightExtraRow(dataGrid1, rowIndex, Brushes.Red);
            }
            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            Dictionary<string, int> dic2Index = new Dictionary<string, int>();
            for (int rowIndex = 0; rowIndex < list2.Count; rowIndex++)
            {
                var item2 = list2[rowIndex];
                string id = GetCellValue(item2, dataGrid2.Columns[0]);
                if (!string.IsNullOrEmpty(id) && !dic2.ContainsKey(id))
                {
                    dic2.Add(id, list2[rowIndex]);
                    dic2Index.Add(id, rowIndex); 
                }
                else
                    HighlightExtraRow(dataGrid2, rowIndex, Brushes.Red);
            }

            var highlightBrush = GetHighlightBrush(dataGrid1);

            for (int rowIndex = 0; rowIndex < list1.Count; rowIndex++)
            {
                var item1 = list1[rowIndex];
                string id = GetCellValue(item1, dataGrid1.Columns[0]);
                item1 = dic1.TryGetValue(id, out var value1) ? value1 : null;
                if (item1 == null) continue;
                var item2 = dic2.TryGetValue(id, out var value2) ? value2 : null;
                if (item2 == null)
                {
                    HighlightExtraRow(dataGrid1, rowIndex, highlightBrush);
                    dic1.Remove(id);
                    continue;
                }
                foreach (DataGridColumn column in dataGrid1.Columns)
                {
                    if (column.Visibility != Visibility.Visible) continue;

                    var correspondingColumn = FindCorrespondingColumn(dataGrid2, column);
                    if (correspondingColumn == null) continue;

                    string cell1 = GetCellValue(item1, column);
                    string cell2 = GetCellValue(item2, correspondingColumn);

                    if (!string.Equals(cell1, cell2))
                    {
                        HighlightCell(dataGrid1, rowIndex, column, highlightBrush);
                        HighlightCell(dataGrid2, dic2Index[id], correspondingColumn, highlightBrush);
                    }
                }
                dic2.Remove(id);
                dic1.Remove(id);
            }
            // 处理DataGrid2中DataGrid1中没有的内容
            for (int rowIndex = 0; rowIndex < list2.Count; rowIndex++)
            {
                var item2 = list2[rowIndex];
                string id = GetCellValue(item2, dataGrid2.Columns[0]);
                if (!string.IsNullOrEmpty(id) && dic2.ContainsKey(id))
                    HighlightExtraRow(dataGrid2, rowIndex, highlightBrush);
            }
        }
        private static void HighlightExtraRows(DataGrid dataGrid, int actualCount, int expectedCount, Brush highlightBrush)
        {
            if (actualCount <= expectedCount) return;

            for (int i = expectedCount; i < actualCount; i++)
            {
                var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                if (row != null)
                {
                    row.Background = highlightBrush;
                }
            }
        }

        private static void HighlightExtraRow(DataGrid dataGrid, int rowIndex, Brush highlightBrush)
        {
            var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            if (row != null)
            {
                row.Background = highlightBrush;
            }
        }

        private static DataGridColumn FindCorrespondingColumn(DataGrid dataGrid, DataGridColumn sourceColumn)
        {
            if (sourceColumn.Header is string headerText)
            {
                return dataGrid.Columns.FirstOrDefault(col =>
                    col.Header is string colHeader && colHeader == headerText && col.Visibility == Visibility.Visible);
            }

            if (sourceColumn is DataGridBoundColumn sourceBoundColumn)
            {
                var sourceBinding = (sourceBoundColumn.Binding as System.Windows.Data.Binding)?.Path.Path;
                if (sourceBinding != null)
                {
                    return dataGrid.Columns.OfType<DataGridBoundColumn>()
                        .FirstOrDefault(col =>
                        {
                            var binding = (col.Binding as System.Windows.Data.Binding)?.Path.Path;
                            return binding == sourceBinding && col.Visibility == Visibility.Visible;
                        });
                }
            }

            return null;
        }

        private static string GetCellValue(object item, DataGridColumn column)
        {
            if (column is DataGridBoundColumn boundColumn)
            {
                var binding = boundColumn.Binding as System.Windows.Data.Binding;
                if (binding?.Path != null)
                {
                    string propertyPath = binding.Path.Path;
                    return GetPropertyValue(item, propertyPath)?.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private static object GetPropertyValue(object obj, string propertyPath)
        {
            if (obj == null || string.IsNullOrEmpty(propertyPath)) return null;

            var properties = propertyPath.Split('.');
            object current = obj;

            foreach (var property in properties)
            {
                if (current == null) return null;

                var propInfo = current.GetType().GetProperty(property,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (propInfo == null) return null;

                current = propInfo.GetValue(current);
            }

            return current;
        }


        private static void HighlightCell(DataGrid dataGrid, int rowIndex, DataGridColumn column, Brush highlightBrush)
        {
            // 实现与之前相同
            if (rowIndex < 0 || rowIndex >= dataGrid.Items.Count) return;

            var item = dataGrid.Items[rowIndex];
            var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;

            if (row != null)
            {
                var cell = GetCell(dataGrid, row, column);
                if (cell != null)
                {
                    cell.Background = highlightBrush;
                }
            }
        }

        private static DataGridCell GetCell(DataGrid dataGrid, DataGridRow row, DataGridColumn column)
        {
            if (row == null || column == null) return null;

            var presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null) return null;

            var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column.DisplayIndex);
            return cell;
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

        private static void ClearHighlights(DataGrid dataGrid)
        {
            foreach (var item in dataGrid.Items)
            {
                var row = dataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    foreach (DataGridColumn column in dataGrid.Columns)
                    {
                        var cell = GetCell(dataGrid, row, column);
                        if (cell != null)
                        {
                            cell.ClearValue(DataGridCell.BackgroundProperty);
                        }
                    }
                    row.ClearValue(DataGridCell.BackgroundProperty);
                }
            }
        }

        public static void RefreshComparison(DataGrid dataGrid)
        {
            UpdateComparison(dataGrid);
        }
    }
    public static class DataGridColumnSyncBehavior
    {
        private static readonly Dictionary<string, SyncGroup> _syncGroups = new Dictionary<string, SyncGroup>();
        private static readonly object _syncLock = new object();

        private class SyncGroup
        {
            public List<DataGrid> SyncedGrids { get; } = new List<DataGrid>();
            public bool IsProcessing { get; set; }
            public Dictionary<DataGrid, List<DataGridColumn>> MonitoredColumns { get; } = new Dictionary<DataGrid, List<DataGridColumn>>();
        }

        public static readonly DependencyProperty SyncGroupNameProperty =
            DependencyProperty.RegisterAttached(
                "SyncGroupName",
                typeof(string),
                typeof(DataGridColumnSyncBehavior),
                new PropertyMetadata(null, OnSyncGroupNameChanged));

        public static readonly DependencyProperty SyncDirectionProperty =
            DependencyProperty.RegisterAttached(
                "SyncDirection",
                typeof(SyncDirection),
                typeof(DataGridColumnSyncBehavior),
                new PropertyMetadata(SyncDirection.Bidirectional));

        public static string GetSyncGroupName(DependencyObject obj) => (string)obj.GetValue(SyncGroupNameProperty);
        public static void SetSyncGroupName(DependencyObject obj, string value) => obj.SetValue(SyncGroupNameProperty, value);

        public static SyncDirection GetSyncDirection(DependencyObject obj) => (SyncDirection)obj.GetValue(SyncDirectionProperty);
        public static void SetSyncDirection(DependencyObject obj, SyncDirection value) => obj.SetValue(SyncDirectionProperty, value);

        public enum SyncDirection
        {
            /// <summary>
            /// 双向同步：任何DataGrid的列宽变化都会同步到组内其他DataGrid
            /// </summary>
            Bidirectional,

            /// <summary>
            /// 单向输出：此DataGrid的列宽变化会同步到其他DataGrid，但其他DataGrid的变化不会同步到此DataGrid
            /// </summary>
            OneWayToOthers,

            /// <summary>
            /// 单向输入：其他DataGrid的列宽变化会同步到此DataGrid，但此DataGrid的变化不会同步到其他DataGrid
            /// </summary>
            OneWayFromOthers
        }

        private static void OnSyncGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.OldValue is string oldGroupName)
                    {
                        UnregisterFromGroup(oldGroupName, dataGrid);
                    }

                    if (e.NewValue is string newGroupName && !string.IsNullOrEmpty(newGroupName))
                    {
                        RegisterToGroup(newGroupName, dataGrid);
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private static void RegisterToGroup(string groupName, DataGrid dataGrid)
        {
            lock (_syncLock)
            {
                CleanupInvalidGroups();

                if (!_syncGroups.ContainsKey(groupName))
                {
                    _syncGroups[groupName] = new SyncGroup();
                }

                var syncGroup = _syncGroups[groupName];

                if (!syncGroup.SyncedGrids.Contains(dataGrid))
                {
                    syncGroup.SyncedGrids.Add(dataGrid);
                    syncGroup.MonitoredColumns[dataGrid] = new List<DataGridColumn>();

                    // 设置列监听
                    SetupColumnListeners(dataGrid);

                    // 监听卸载事件
                    dataGrid.Unloaded += (s, e) => UnregisterFromGroup(groupName, dataGrid);
                    dataGrid.Loaded += (s, e) => OnDataGridLoaded(groupName, dataGrid);

                    // 如果组内已有其他DataGrid，进行初始同步
                    if (syncGroup.SyncedGrids.Count > 1)
                    {
                        // 使用第一个DataGrid作为初始同步源（或者可以选择其他策略）
                        var sourceGrid = syncGroup.SyncedGrids[0];
                        if (sourceGrid != dataGrid && sourceGrid.IsLoaded && dataGrid.IsLoaded)
                        {
                            SyncColumns(sourceGrid, dataGrid, syncGroup);
                        }
                    }
                }
            }
        }

        private static void UnregisterFromGroup(string groupName, DataGrid dataGrid)
        {
            lock (_syncLock)
            {
                if (_syncGroups.ContainsKey(groupName))
                {
                    var syncGroup = _syncGroups[groupName];

                    syncGroup.SyncedGrids.Remove(dataGrid);
                    syncGroup.MonitoredColumns.Remove(dataGrid);

                    RemoveColumnListeners(dataGrid);

                    // 如果组为空，清理
                    if (syncGroup.SyncedGrids.Count == 0)
                    {
                        _syncGroups.Remove(groupName);
                    }
                }
            }
        }

        private static void OnDataGridLoaded(string groupName, DataGrid dataGrid)
        {
            lock (_syncLock)
            {
                if (_syncGroups.ContainsKey(groupName))
                {
                    var syncGroup = _syncGroups[groupName];

                    SetupColumnListeners(dataGrid);

                    // 找到另一个已加载的DataGrid进行同步
                    var otherGrid = syncGroup.SyncedGrids.FirstOrDefault(g =>
                        g != dataGrid && g.IsLoaded && g.Columns.Count == dataGrid.Columns.Count);

                    if (otherGrid != null)
                    {
                        SyncColumns(otherGrid, dataGrid, syncGroup);
                    }
                }
            }
        }

        private static void SetupColumnListeners(DataGrid dataGrid)
        {
            if (dataGrid?.Columns == null) return;

            var groupName = GetSyncGroupName(dataGrid);
            if (string.IsNullOrEmpty(groupName) || !_syncGroups.ContainsKey(groupName)) return;

            var syncGroup = _syncGroups[groupName];
            var monitoredColumns = syncGroup.MonitoredColumns[dataGrid];

            // 清理旧的监听
            foreach (var column in monitoredColumns.ToList())
            {
                if (column != null)
                {
                    var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
                    descriptor.RemoveValueChanged(column, OnColumnWidthChanged);
                }
            }
            monitoredColumns.Clear();

            // 添加新的监听
            foreach (var column in dataGrid.Columns)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
                descriptor.RemoveValueChanged(column, OnColumnWidthChanged);
                descriptor.AddValueChanged(column, OnColumnWidthChanged);

                monitoredColumns.Add(column);
            }

            // 监听SizeChanged来检测列变化
            dataGrid.SizeChanged -= OnDataGridSizeChanged;
            dataGrid.SizeChanged += OnDataGridSizeChanged;
        }

        private static void RemoveColumnListeners(DataGrid dataGrid)
        {
            if (dataGrid?.Columns == null) return;

            var groupName = GetSyncGroupName(dataGrid);
            if (!string.IsNullOrEmpty(groupName) && _syncGroups.ContainsKey(groupName))
            {
                var syncGroup = _syncGroups[groupName];
                if (syncGroup.MonitoredColumns.ContainsKey(dataGrid))
                {
                    foreach (var column in syncGroup.MonitoredColumns[dataGrid])
                    {
                        if (column != null)
                        {
                            var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
                            descriptor.RemoveValueChanged(column, OnColumnWidthChanged);
                        }
                    }
                    syncGroup.MonitoredColumns[dataGrid].Clear();
                }
            }

            dataGrid.SizeChanged -= OnDataGridSizeChanged;
        }

        private static void OnDataGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 当DataGrid尺寸变化时，重新设置列监听（列可能已变化）
            if (sender is DataGrid dataGrid)
            {
                var groupName = GetSyncGroupName(dataGrid);
                if (!string.IsNullOrEmpty(groupName) && _syncGroups.ContainsKey(groupName))
                {
                    SetupColumnListeners(dataGrid);
                }
            }
        }

        private static void OnColumnWidthChanged(object sender, EventArgs e)
        {
            if (sender is DataGridColumn changedColumn)
            {
                // 找到所有包含此列的DataGrid和它们的组
                foreach (var group in _syncGroups.Values)
                {
                    if (group.IsProcessing) continue;

                    // 找到哪个DataGrid的列发生了变化
                    var sourceGrid = group.SyncedGrids.FirstOrDefault(grid =>
                        grid?.Columns != null && grid.Columns.Contains(changedColumn));

                    if (sourceGrid != null)
                    {
                        int columnIndex = sourceGrid.Columns.IndexOf(changedColumn);
                        if (columnIndex >= 0)
                        {
                            group.IsProcessing = true;
                            try
                            {
                                // 同步到组内其他DataGrid
                                foreach (var targetGrid in group.SyncedGrids.ToList())
                                {
                                    if (targetGrid != null &&
                                        targetGrid != sourceGrid &&
                                        targetGrid.IsLoaded &&
                                        targetGrid.Columns.Count > columnIndex &&
                                        ShouldSync(sourceGrid, targetGrid))
                                    {
                                        targetGrid.Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            try
                                            {
                                                if (targetGrid.Columns.Count > columnIndex &&
                                                    targetGrid.Columns[columnIndex].Width != changedColumn.Width)
                                                {
                                                    targetGrid.Columns[columnIndex].Width = changedColumn.Width;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"同步列宽时出错: {ex.Message}");
                                            }
                                        }), System.Windows.Threading.DispatcherPriority.Background);
                                    }
                                }
                            }
                            finally
                            {
                                group.IsProcessing = false;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private static bool ShouldSync(DataGrid source, DataGrid target)
        {
            var sourceDirection = GetSyncDirection(source);
            var targetDirection = GetSyncDirection(target);

            // 双向同步：总是同步
            if (sourceDirection == SyncDirection.Bidirectional && targetDirection == SyncDirection.Bidirectional)
                return true;

            // 源是单向输出，目标是双向或单向输入
            if (sourceDirection == SyncDirection.OneWayToOthers &&
                (targetDirection == SyncDirection.Bidirectional || targetDirection == SyncDirection.OneWayFromOthers))
                return true;

            // 源是双向，目标是单向输入
            if (sourceDirection == SyncDirection.Bidirectional && targetDirection == SyncDirection.OneWayFromOthers)
                return true;

            // 源是单向输出，目标是单向输入
            if (sourceDirection == SyncDirection.OneWayToOthers && targetDirection == SyncDirection.OneWayFromOthers)
                return true;

            return false;
        }

        private static void SyncColumns(DataGrid source, DataGrid target, SyncGroup syncGroup)
        {
            if (source?.Columns == null || target?.Columns == null) return;
            if (!ShouldSync(source, target)) return;

            int minColumnCount = Math.Min(source.Columns.Count, target.Columns.Count);

            for (int i = 0; i < minColumnCount; i++)
            {
                if (target.Columns[i].Width != source.Columns[i].Width)
                {
                    target.Columns[i].Width = source.Columns[i].Width;
                }
            }
        }

        private static void CleanupInvalidGroups()
        {
            var groupsToRemove = new List<string>();

            foreach (var entry in _syncGroups)
            {
                var group = entry.Value;

                // 清理无效的DataGrid
                group.SyncedGrids.RemoveAll(grid => grid == null || !grid.IsLoaded || grid.CheckAccess() == false);

                // 清理MonitoredColumns中的无效条目
                var invalidGrids = group.MonitoredColumns.Keys
                    .Where(grid => !group.SyncedGrids.Contains(grid))
                    .ToList();

                foreach (var invalidGrid in invalidGrids)
                {
                    group.MonitoredColumns.Remove(invalidGrid);
                }

                if (group.SyncedGrids.Count == 0)
                {
                    groupsToRemove.Add(entry.Key);
                }
            }

            foreach (var groupName in groupsToRemove)
            {
                _syncGroups.Remove(groupName);
            }
        }
    }
    
    //// 跨控件单向同步
    //public static class DataGridColumnSyncBehavior
    //{
    //    private static readonly Dictionary<string, SyncGroup> _syncGroups = new Dictionary<string, SyncGroup>();
    //    private static readonly object _syncLock = new object();

    //    private class SyncGroup
    //    {
    //        public DataGrid SourceGrid { get; set; }
    //        public List<DataGrid> TargetGrids { get; } = new List<DataGrid>();
    //        public bool IsProcessing { get; set; }
    //    }

    //    public static readonly DependencyProperty SyncGroupNameProperty =
    //        DependencyProperty.RegisterAttached(
    //            "SyncGroupName",
    //            typeof(string),
    //            typeof(DataGridColumnSyncBehavior),
    //            new PropertyMetadata(null, OnSyncGroupNameChanged));

    //    public static readonly DependencyProperty IsSourceProperty =
    //        DependencyProperty.RegisterAttached(
    //            "IsSource",
    //            typeof(bool),
    //            typeof(DataGridColumnSyncBehavior),
    //            new PropertyMetadata(false, OnIsSourceChanged));

    //    public static string GetSyncGroupName(DependencyObject obj) => (string)obj.GetValue(SyncGroupNameProperty);
    //    public static void SetSyncGroupName(DependencyObject obj, string value) => obj.SetValue(SyncGroupNameProperty, value);

    //    public static bool GetIsSource(DependencyObject obj) => (bool)obj.GetValue(IsSourceProperty);
    //    public static void SetIsSource(DependencyObject obj, bool value) => obj.SetValue(IsSourceProperty, value);

    //    private static void OnSyncGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (d is DataGrid dataGrid)
    //        {
    //            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
    //            {
    //                if (e.OldValue is string oldGroupName)
    //                {
    //                    UnregisterFromGroup(oldGroupName, dataGrid);
    //                }

    //                if (e.NewValue is string newGroupName && !string.IsNullOrEmpty(newGroupName))
    //                {
    //                    RegisterToGroup(newGroupName, dataGrid);
    //                }
    //            }), System.Windows.Threading.DispatcherPriority.Loaded);
    //        }
    //    }

    //    private static void OnIsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (d is DataGrid dataGrid)
    //        {
    //            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
    //            {
    //                var groupName = GetSyncGroupName(dataGrid);
    //                if (!string.IsNullOrEmpty(groupName))
    //                {
    //                    if ((bool)e.NewValue)
    //                    {
    //                        SetAsSourceGroup(groupName, dataGrid);
    //                    }
    //                    else
    //                    {
    //                        RemoveAsSourceGroup(groupName, dataGrid);
    //                    }
    //                }
    //            }), System.Windows.Threading.DispatcherPriority.Loaded);
    //        }
    //    }

    //    private static void RegisterToGroup(string groupName, DataGrid dataGrid)
    //    {
    //        lock (_syncLock)
    //        {
    //            CleanupInvalidGroups();

    //            if (!_syncGroups.ContainsKey(groupName))
    //            {
    //                _syncGroups[groupName] = new SyncGroup();
    //            }

    //            var syncGroup = _syncGroups[groupName];

    //            // 如果这个DataGrid被标记为源，则设置为源
    //            if (GetIsSource(dataGrid))
    //            {
    //                if (syncGroup.SourceGrid != null && syncGroup.SourceGrid != dataGrid)
    //                {
    //                    // 如果已经有源，移除旧的监听
    //                    RemoveColumnListeners(syncGroup.SourceGrid);
    //                }
    //                syncGroup.SourceGrid = dataGrid;
    //                SetupColumnListeners(dataGrid);
    //            }
    //            else
    //            {
    //                // 作为目标添加到组中
    //                if (!syncGroup.TargetGrids.Contains(dataGrid))
    //                {
    //                    syncGroup.TargetGrids.Add(dataGrid);

    //                    // 如果已经有源，立即同步
    //                    if (syncGroup.SourceGrid != null && syncGroup.SourceGrid.IsLoaded && dataGrid.IsLoaded)
    //                    {
    //                        SyncColumns(syncGroup.SourceGrid, dataGrid);
    //                    }
    //                }
    //            }

    //            // 监听卸载事件
    //            dataGrid.Unloaded += (s, e) => UnregisterFromGroup(groupName, dataGrid);
    //            dataGrid.Loaded += (s, e) => OnDataGridLoaded(groupName, dataGrid);
    //        }
    //    }

    //    private static void UnregisterFromGroup(string groupName, DataGrid dataGrid)
    //    {
    //        lock (_syncLock)
    //        {
    //            if (_syncGroups.ContainsKey(groupName))
    //            {
    //                var syncGroup = _syncGroups[groupName];

    //                if (syncGroup.SourceGrid == dataGrid)
    //                {
    //                    RemoveColumnListeners(dataGrid);
    //                    syncGroup.SourceGrid = null;
    //                }
    //                else
    //                {
    //                    syncGroup.TargetGrids.Remove(dataGrid);
    //                }

    //                // 如果组为空，清理
    //                if (syncGroup.SourceGrid == null && syncGroup.TargetGrids.Count == 0)
    //                {
    //                    _syncGroups.Remove(groupName);
    //                }
    //            }
    //        }
    //    }

    //    private static void SetAsSourceGroup(string groupName, DataGrid dataGrid)
    //    {
    //        lock (_syncLock)
    //        {
    //            if (_syncGroups.ContainsKey(groupName))
    //            {
    //                var syncGroup = _syncGroups[groupName];

    //                if (syncGroup.SourceGrid != null && syncGroup.SourceGrid != dataGrid)
    //                {
    //                    RemoveColumnListeners(syncGroup.SourceGrid);
    //                }

    //                syncGroup.SourceGrid = dataGrid;
    //                SetupColumnListeners(dataGrid);

    //                // 同步所有目标
    //                foreach (var targetGrid in syncGroup.TargetGrids.ToList())
    //                {
    //                    if (targetGrid?.IsLoaded == true)
    //                    {
    //                        SyncColumns(dataGrid, targetGrid);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private static void RemoveAsSourceGroup(string groupName, DataGrid dataGrid)
    //    {
    //        lock (_syncLock)
    //        {
    //            if (_syncGroups.ContainsKey(groupName) && _syncGroups[groupName].SourceGrid == dataGrid)
    //            {
    //                RemoveColumnListeners(dataGrid);
    //                _syncGroups[groupName].SourceGrid = null;
    //            }
    //        }
    //    }

    //    private static void OnDataGridLoaded(string groupName, DataGrid dataGrid)
    //    {
    //        lock (_syncLock)
    //        {
    //            if (_syncGroups.ContainsKey(groupName))
    //            {
    //                var syncGroup = _syncGroups[groupName];

    //                if (GetIsSource(dataGrid) && syncGroup.SourceGrid == dataGrid)
    //                {
    //                    SetupColumnListeners(dataGrid);

    //                    // 同步所有目标
    //                    foreach (var targetGrid in syncGroup.TargetGrids.ToList())
    //                    {
    //                        if (targetGrid?.IsLoaded == true)
    //                        {
    //                            SyncColumns(dataGrid, targetGrid);
    //                        }
    //                    }
    //                }
    //                else if (syncGroup.SourceGrid != null && syncGroup.SourceGrid.IsLoaded)
    //                {
    //                    // 目标加载时，从源同步
    //                    SyncColumns(syncGroup.SourceGrid, dataGrid);
    //                }
    //            }
    //        }
    //    }

    //    private static void SetupColumnListeners(DataGrid dataGrid)
    //    {
    //        if (dataGrid?.Columns == null) return;

    //        foreach (var column in dataGrid.Columns)
    //        {
    //            var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
    //            descriptor.RemoveValueChanged(column, OnColumnWidthChanged);
    //            descriptor.AddValueChanged(column, OnColumnWidthChanged);
    //        }

    //        // 监听SizeChanged来检测列变化
    //        dataGrid.SizeChanged += OnDataGridSizeChanged;
    //    }

    //    private static void RemoveColumnListeners(DataGrid dataGrid)
    //    {
    //        if (dataGrid?.Columns == null) return;

    //        foreach (var column in dataGrid.Columns)
    //        {
    //            var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
    //            descriptor.RemoveValueChanged(column, OnColumnWidthChanged);
    //        }

    //        dataGrid.SizeChanged -= OnDataGridSizeChanged;
    //    }

    //    private static void OnDataGridSizeChanged(object sender, SizeChangedEventArgs e)
    //    {
    //        // 当DataGrid尺寸变化时，重新设置列监听（列可能已变化）
    //        if (sender is DataGrid dataGrid && GetIsSource(dataGrid))
    //        {
    //            var groupName = GetSyncGroupName(dataGrid);
    //            if (!string.IsNullOrEmpty(groupName) && _syncGroups.ContainsKey(groupName))
    //            {
    //                RemoveColumnListeners(dataGrid);
    //                SetupColumnListeners(dataGrid);
    //            }
    //        }
    //    }

    //    private static void OnColumnWidthChanged(object sender, EventArgs e)
    //    {
    //        if (sender is DataGridColumn changedColumn)
    //        {
    //            // 找到所有包含此列的源DataGrid
    //            foreach (var group in _syncGroups.Values)
    //            {
    //                if (group.SourceGrid?.Columns != null &&
    //                    group.SourceGrid.Columns.Contains(changedColumn) &&
    //                    !group.IsProcessing)
    //                {
    //                    int columnIndex = group.SourceGrid.Columns.IndexOf(changedColumn);
    //                    if (columnIndex >= 0)
    //                    {
    //                        group.IsProcessing = true;
    //                        try
    //                        {
    //                            foreach (var targetGrid in group.TargetGrids.ToList())
    //                            {
    //                                if (targetGrid != null &&
    //                                    targetGrid.IsLoaded &&
    //                                    targetGrid.Columns.Count > columnIndex &&
    //                                    targetGrid.Columns[columnIndex].Width != changedColumn.Width)
    //                                {
    //                                    targetGrid.Dispatcher.BeginInvoke(new Action(() =>
    //                                    {
    //                                        try
    //                                        {
    //                                            if (targetGrid.Columns.Count > columnIndex)
    //                                            {
    //                                                targetGrid.Columns[columnIndex].Width = changedColumn.Width;
    //                                            }
    //                                        }
    //                                        catch (Exception ex)
    //                                        {
    //                                            System.Diagnostics.Debug.WriteLine($"同步列宽时出错: {ex.Message}");
    //                                        }
    //                                    }), System.Windows.Threading.DispatcherPriority.Background);
    //                                }
    //                            }
    //                        }
    //                        finally
    //                        {
    //                            group.IsProcessing = false;
    //                        }
    //                    }
    //                    break;
    //                }
    //            }
    //        }
    //    }

    //    private static void SyncColumns(DataGrid source, DataGrid target)
    //    {
    //        if (source?.Columns == null || target?.Columns == null) return;

    //        int minColumnCount = Math.Min(source.Columns.Count, target.Columns.Count);

    //        for (int i = 0; i < minColumnCount; i++)
    //        {
    //            if (target.Columns[i].Width != source.Columns[i].Width)
    //            {
    //                target.Columns[i].Width = source.Columns[i].Width;
    //            }
    //        }
    //    }

    //    private static void CleanupInvalidGroups()
    //    {
    //        var groupsToRemove = new List<string>();

    //        foreach (var entry in _syncGroups)
    //        {
    //            var group = entry.Value;

    //            // 清理无效的源
    //            if (group.SourceGrid != null && (!group.SourceGrid.IsLoaded || group.SourceGrid.CheckAccess() == false))
    //            {
    //                group.SourceGrid = null;
    //            }

    //            // 清理无效的目标
    //            group.TargetGrids.RemoveAll(grid => grid == null || !grid.IsLoaded || grid.CheckAccess() == false);

    //            if (group.SourceGrid == null && group.TargetGrids.Count == 0)
    //            {
    //                groupsToRemove.Add(entry.Key);
    //            }
    //        }

    //        foreach (var groupName in groupsToRemove)
    //        {
    //            _syncGroups.Remove(groupName);
    //        }
    //    }
    //}
    //// 同一控件中同步，可双向
    //public static class DataGridColumnSyncBehavior
    //{
    //    private static readonly Dictionary<DataGrid, SyncGroup> _syncGroups = new Dictionary<DataGrid, SyncGroup>();
    //    private static readonly object _syncLock = new object();

    //    private class SyncGroup
    //    {
    //        public HashSet<DataGrid> TargetGrids { get; } = new HashSet<DataGrid>();
    //        public bool IsProcessing { get; set; }
    //        public List<DataGridColumn> MonitoredColumns { get; } = new List<DataGridColumn>();
    //    }

    //    public static readonly DependencyProperty SyncWithProperty =
    //        DependencyProperty.RegisterAttached(
    //            "SyncWith",
    //            typeof(DataGrid),
    //            typeof(DataGridColumnSyncBehavior),
    //            new PropertyMetadata(null, OnSyncWithChanged));

    //    public static DataGrid GetSyncWith(DependencyObject obj) => (DataGrid)obj.GetValue(SyncWithProperty);
    //    public static void SetSyncWith(DependencyObject obj, DataGrid value) => obj.SetValue(SyncWithProperty, value);

    //    private static void OnSyncWithChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (d is DataGrid targetGrid)
    //        {
    //            // 延迟执行以避免加载期间的问题
    //            targetGrid.Dispatcher.BeginInvoke(new Action(() =>
    //            {
    //                if (e.OldValue is DataGrid oldSourceGrid)
    //                {
    //                    UnregisterSync(oldSourceGrid, targetGrid);
    //                }

    //                if (e.NewValue is DataGrid newSourceGrid && newSourceGrid != targetGrid)
    //                {
    //                    RegisterSync(newSourceGrid, targetGrid);
    //                }
    //            }), System.Windows.Threading.DispatcherPriority.Loaded);
    //        }
    //    }

    //    private static void RegisterSync(DataGrid sourceGrid, DataGrid targetGrid)
    //    {
    //        lock (_syncLock)
    //        {
    //            CleanupInvalidReferences();

    //            if (sourceGrid == null || targetGrid == null) return;

    //            if (!_syncGroups.ContainsKey(sourceGrid))
    //            {
    //                _syncGroups[sourceGrid] = new SyncGroup();

    //                // 监听源DataGrid的加载和卸载事件
    //                sourceGrid.Loaded += OnSourceGridLoaded;
    //                sourceGrid.Unloaded += OnSourceGridUnloaded;

    //                // 使用定时器定期检查列变化（简单但有效的方法）
    //                SetupColumnChangeDetection(sourceGrid);

    //                // 如果已经加载，立即设置监听
    //                if (sourceGrid.IsLoaded)
    //                {
    //                    SetupColumnListeners(sourceGrid);
    //                }
    //            }

    //            var syncGroup = _syncGroups[sourceGrid];

    //            if (!syncGroup.TargetGrids.Contains(targetGrid))
    //            {
    //                syncGroup.TargetGrids.Add(targetGrid);

    //                // 监听目标网格的卸载事件
    //                targetGrid.Unloaded += OnTargetGridUnloaded;

    //                // 初始同步
    //                if (sourceGrid.IsLoaded && targetGrid.IsLoaded)
    //                {
    //                    SyncColumns(sourceGrid, targetGrid);
    //                }
    //            }
    //        }
    //    }

    //    private static void UnregisterSync(DataGrid sourceGrid, DataGrid targetGrid)
    //    {
    //        lock (_syncLock)
    //        {
    //            if (sourceGrid != null && _syncGroups.ContainsKey(sourceGrid))
    //            {
    //                var syncGroup = _syncGroups[sourceGrid];
    //                syncGroup.TargetGrids.Remove(targetGrid);

    //                if (syncGroup.TargetGrids.Count == 0)
    //                {
    //                    CleanupSourceGrid(sourceGrid);
    //                    _syncGroups.Remove(sourceGrid);
    //                }
    //            }
    //        }
    //    }

    //    private static void OnSourceGridLoaded(object sender, RoutedEventArgs e)
    //    {
    //        if (sender is DataGrid sourceGrid && _syncGroups.ContainsKey(sourceGrid))
    //        {
    //            SetupColumnListeners(sourceGrid);

    //            // 同步所有目标网格
    //            var syncGroup = _syncGroups[sourceGrid];
    //            foreach (var targetGrid in syncGroup.TargetGrids.ToList())
    //            {
    //                if (targetGrid?.IsLoaded == true)
    //                {
    //                    SyncColumns(sourceGrid, targetGrid);
    //                }
    //            }
    //        }
    //    }

    //    private static void OnSourceGridUnloaded(object sender, RoutedEventArgs e)
    //    {
    //        if (sender is DataGrid sourceGrid && _syncGroups.ContainsKey(sourceGrid))
    //        {
    //            RemoveColumnListeners(sourceGrid);
    //        }
    //    }

    //    private static void OnTargetGridUnloaded(object sender, RoutedEventArgs e)
    //    {
    //        if (sender is DataGrid targetGrid)
    //        {
    //            // 找到对应的源网格并取消注册
    //            lock (_syncLock)
    //            {
    //                foreach (var entry in _syncGroups)
    //                {
    //                    if (entry.Value.TargetGrids.Contains(targetGrid))
    //                    {
    //                        UnregisterSync(entry.Key, targetGrid);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    // 简单的列变化检测方法
    //    private static void SetupColumnChangeDetection(DataGrid sourceGrid)
    //    {
    //        // 使用SizeChanged事件作为列可能变化的触发器
    //        sourceGrid.SizeChanged += OnSourceGridSizeChanged;

    //        // 也可以使用LayoutUpdated事件，但要注意性能
    //        sourceGrid.LayoutUpdated += OnSourceGridLayoutUpdated;
    //    }

    //    private static void OnSourceGridSizeChanged(object sender, SizeChangedEventArgs e)
    //    {
    //        // 尺寸变化时可能列也发生了变化，重新检查
    //        if (sender is DataGrid sourceGrid && _syncGroups.ContainsKey(sourceGrid))
    //        {
    //            CheckAndUpdateColumnListeners(sourceGrid);
    //        }
    //    }

    //    private static void OnSourceGridLayoutUpdated(object sender, EventArgs e)
    //    {
    //        // 布局更新时检查列监听
    //        if (sender is DataGrid sourceGrid && _syncGroups.ContainsKey(sourceGrid))
    //        {
    //            CheckAndUpdateColumnListeners(sourceGrid);
    //        }
    //    }

    //    private static void CheckAndUpdateColumnListeners(DataGrid sourceGrid)
    //    {
    //        var syncGroup = _syncGroups[sourceGrid];

    //        // 检查列数量或引用是否发生变化
    //        bool needsUpdate = syncGroup.MonitoredColumns.Count != sourceGrid.Columns.Count;

    //        if (!needsUpdate)
    //        {
    //            // 检查列引用是否相同
    //            for (int i = 0; i < sourceGrid.Columns.Count; i++)
    //            {
    //                if (i >= syncGroup.MonitoredColumns.Count ||
    //                    syncGroup.MonitoredColumns[i] != sourceGrid.Columns[i])
    //                {
    //                    needsUpdate = true;
    //                    break;
    //                }
    //            }
    //        }

    //        if (needsUpdate)
    //        {
    //            RemoveColumnListeners(sourceGrid);
    //            SetupColumnListeners(sourceGrid);

    //            // 重新同步所有目标网格
    //            foreach (var targetGrid in syncGroup.TargetGrids.ToList())
    //            {
    //                if (targetGrid?.IsLoaded == true)
    //                {
    //                    SyncColumns(sourceGrid, targetGrid);
    //                }
    //            }
    //        }
    //    }

    //    private static void SetupColumnListeners(DataGrid dataGrid)
    //    {
    //        if (!_syncGroups.ContainsKey(dataGrid)) return;

    //        var syncGroup = _syncGroups[dataGrid];
    //        syncGroup.MonitoredColumns.Clear();

    //        foreach (var column in dataGrid.Columns)
    //        {
    //            // 检查是否已经添加了监听
    //            var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
    //            descriptor.RemoveValueChanged(column, OnColumnWidthChanged);
    //            descriptor.AddValueChanged(column, OnColumnWidthChanged);

    //            syncGroup.MonitoredColumns.Add(column);
    //        }
    //    }

    //    private static void RemoveColumnListeners(DataGrid dataGrid)
    //    {
    //        if (!_syncGroups.ContainsKey(dataGrid)) return;

    //        var syncGroup = _syncGroups[dataGrid];

    //        foreach (var column in syncGroup.MonitoredColumns)
    //        {
    //            if (column != null)
    //            {
    //                var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
    //                descriptor.RemoveValueChanged(column, OnColumnWidthChanged);
    //            }
    //        }

    //        syncGroup.MonitoredColumns.Clear();
    //    }

    //    private static void OnColumnWidthChanged(object sender, EventArgs e)
    //    {
    //        if (sender is DataGridColumn changedColumn)
    //        {
    //            // 找到源DataGrid
    //            foreach (var entry in _syncGroups)
    //            {
    //                var sourceGrid = entry.Key;
    //                var syncGroup = entry.Value;

    //                if (syncGroup.IsProcessing) return;

    //                if (sourceGrid.Columns.Contains(changedColumn))
    //                {
    //                    int columnIndex = sourceGrid.Columns.IndexOf(changedColumn);
    //                    if (columnIndex >= 0)
    //                    {
    //                        syncGroup.IsProcessing = true;
    //                        try
    //                        {
    //                            foreach (var targetGrid in syncGroup.TargetGrids.ToList())
    //                            {
    //                                if (targetGrid != null &&
    //                                    targetGrid.IsLoaded &&
    //                                    targetGrid.Columns.Count > columnIndex &&
    //                                    targetGrid.Columns[columnIndex].Width != changedColumn.Width)
    //                                {
    //                                    // 使用Dispatcher避免重入
    //                                    targetGrid.Dispatcher.BeginInvoke(new Action(() =>
    //                                    {
    //                                        try
    //                                        {
    //                                            // 再次检查，避免竞态条件
    //                                            if (targetGrid.Columns.Count > columnIndex)
    //                                            {
    //                                                targetGrid.Columns[columnIndex].Width = changedColumn.Width;
    //                                            }
    //                                        }
    //                                        catch (Exception ex)
    //                                        {
    //                                            System.Diagnostics.Debug.WriteLine($"同步列宽时出错: {ex.Message}");
    //                                        }
    //                                    }), System.Windows.Threading.DispatcherPriority.Background);
    //                                }
    //                            }
    //                        }
    //                        finally
    //                        {
    //                            syncGroup.IsProcessing = false;
    //                        }
    //                    }
    //                    break;
    //                }
    //            }
    //        }
    //    }

    //    private static void SyncColumns(DataGrid source, DataGrid target)
    //    {
    //        if (source?.Columns == null || target?.Columns == null) return;

    //        int minColumnCount = Math.Min(source.Columns.Count, target.Columns.Count);

    //        for (int i = 0; i < minColumnCount; i++)
    //        {
    //            if (target.Columns[i].Width != source.Columns[i].Width)
    //            {
    //                target.Columns[i].Width = source.Columns[i].Width;
    //            }
    //        }
    //    }

    //    private static void CleanupSourceGrid(DataGrid sourceGrid)
    //    {
    //        if (sourceGrid != null)
    //        {
    //            sourceGrid.Loaded -= OnSourceGridLoaded;
    //            sourceGrid.Unloaded -= OnSourceGridUnloaded;
    //            sourceGrid.SizeChanged -= OnSourceGridSizeChanged;
    //            sourceGrid.LayoutUpdated -= OnSourceGridLayoutUpdated;
    //            RemoveColumnListeners(sourceGrid);
    //        }
    //    }

    //    private static void CleanupInvalidReferences()
    //    {
    //        var gridsToRemove = new List<DataGrid>();

    //        foreach (var entry in _syncGroups)
    //        {
    //            var sourceGrid = entry.Key;
    //            var syncGroup = entry.Value;

    //            // 清理无效的目标网格
    //            syncGroup.TargetGrids.RemoveWhere(grid => grid == null);

    //            if (syncGroup.TargetGrids.Count == 0 || sourceGrid == null)
    //            {
    //                gridsToRemove.Add(sourceGrid);
    //            }
    //        }

    //        foreach (var grid in gridsToRemove)
    //        {
    //            if (grid != null)
    //            {
    //                CleanupSourceGrid(grid);
    //            }
    //            _syncGroups.Remove(grid);
    //        }
    //    }
    //}
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
    //public static class DataGridColumnSyncBehavior
    //{
    //    private static readonly Dictionary<DataGrid, List<DataGrid>> _syncGroups = new Dictionary<DataGrid, List<DataGrid>>();

    //    public static readonly DependencyProperty SyncWithProperty =
    //        DependencyProperty.RegisterAttached("SyncWith", typeof(DataGrid), typeof(DataGridColumnSyncBehavior),
    //            new PropertyMetadata(null, OnSyncWithChanged));

    //    public static DataGrid GetSyncWith(DependencyObject obj) => (DataGrid)obj.GetValue(SyncWithProperty);
    //    public static void SetSyncWith(DependencyObject obj, DataGrid value) => obj.SetValue(SyncWithProperty, value);

    //    private static void OnSyncWithChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (d is DataGrid targetGrid)
    //        {
    //            if (e.OldValue is DataGrid oldSourceGrid)
    //            {
    //                UnregisterSync(oldSourceGrid, targetGrid);
    //            }

    //            if (e.NewValue is DataGrid newSourceGrid)
    //            {
    //                RegisterSync(newSourceGrid, targetGrid);
    //            }
    //        }
    //    }

    //    private static void RegisterSync(DataGrid sourceGrid, DataGrid targetGrid)
    //    {
    //        if (!_syncGroups.ContainsKey(sourceGrid))
    //        {
    //            _syncGroups[sourceGrid] = new List<DataGrid>();
    //            // 监听源DataGrid的列宽变化
    //            sourceGrid.Loaded += SourceGrid_Loaded;
    //            foreach (var column in sourceGrid.Columns)
    //            {
    //                column.Width = column.Width; // 触发一次同步
    //            }
    //        }

    //        if (!_syncGroups[sourceGrid].Contains(targetGrid))
    //        {
    //            _syncGroups[sourceGrid].Add(targetGrid);
    //        }

    //        // 初始同步
    //        SyncColumns(sourceGrid, targetGrid);
    //    }

    //    private static void UnregisterSync(DataGrid sourceGrid, DataGrid targetGrid)
    //    {
    //        if (_syncGroups.ContainsKey(sourceGrid))
    //        {
    //            _syncGroups[sourceGrid].Remove(targetGrid);
    //            if (_syncGroups[sourceGrid].Count == 0)
    //            {
    //                _syncGroups.Remove(sourceGrid);
    //                sourceGrid.Loaded -= SourceGrid_Loaded;
    //            }
    //        }
    //    }

    //    private static void SourceGrid_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        if (sender is DataGrid sourceGrid && _syncGroups.ContainsKey(sourceGrid))
    //        {
    //            // 为每一列添加监听
    //            foreach (var column in sourceGrid.Columns)
    //            {
    //                var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
    //                descriptor.RemoveValueChanged(column, Column_WidthChanged);
    //                descriptor.AddValueChanged(column, Column_WidthChanged);
    //            }
    //        }
    //    }

    //    private static void Column_WidthChanged(object sender, EventArgs e)
    //    {
    //        if (sender is DataGridColumn changedColumn)
    //        {
    //            var sourceGrid = FindParentDataGrid(changedColumn);
    //            if (sourceGrid != null && _syncGroups.ContainsKey(sourceGrid))
    //            {
    //                int columnIndex = sourceGrid.Columns.IndexOf(changedColumn);
    //                foreach (var targetGrid in _syncGroups[sourceGrid])
    //                {
    //                    if (targetGrid.Columns.Count > columnIndex)
    //                    {
    //                        targetGrid.Columns[columnIndex].Width = changedColumn.Width;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private static DataGrid FindParentDataGrid(DataGridColumn column)
    //    {
    //        foreach (var entry in _syncGroups)
    //        {
    //            if (entry.Key.Columns.Contains(column))
    //            {
    //                return entry.Key;
    //            }
    //        }
    //        return null;
    //    }

    //    private static void SyncColumns(DataGrid source, DataGrid target)
    //    {
    //        if (source.Columns.Count != target.Columns.Count) return;

    //        for (int i = 0; i < source.Columns.Count; i++)
    //        {
    //            target.Columns[i].Width = source.Columns[i].Width;
    //        }
    //    }
    //}
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
