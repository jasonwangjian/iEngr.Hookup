using iEngr.Hookup.Models;
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
    /// UcDiagList.xaml 的交互逻辑
    /// </summary>
    public partial class UcDiagItems : UserControl
    {
        public event EventHandler<ActiveArea> ActiveAreaChange;

        public UcDiagItems()
        {
            InitializeComponent();
            // 鼠标悬停处理
            //dgDiagAvailable.LoadingRow += (s, e) => {
            //    e.Row.MouseEnter += (sender, args) => {
            //        // 处理鼠标进入
            //        var viewModel = DataContext as DiagItemsViewModel;
            //        viewModel?.ItemMouseEnterCommand?.Execute(e.Row.DataContext);
            //    };

            //    e.Row.MouseLeave += (sender, args) => {
            //        // 处理鼠标离开
            //        var viewModel = DataContext as DiagItemsViewModel;
            //        viewModel?.ItemMouseLeaveCommand?.Execute(e.Row.DataContext);
            //    };

            //    e.Row.MouseUp += (sender, args) => {
            //        // 处理鼠标点击
            //        var viewModel = DataContext as DiagItemsViewModel;
            //        viewModel?.ItemMouseClickCommand?.Execute(e.Row.DataContext);
            //    };
            //};
        }
        private void ClearAllSorting_click(object sender, RoutedEventArgs e)
        {
            foreach (var column in dgDiagAssigned.Columns)
            {
                column.SortDirection = null;
            }
            dgDiagAssigned.Items.SortDescriptions.Clear();
            foreach (var column in dgDiagAvailable.Columns)
            {
                column.SortDirection = null;
            }
            dgDiagAvailable.Items.SortDescriptions.Clear();
        }
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var dataItem = e.Row.Item as DiagramItem;
            if (dataItem == null) { e.Cancel = true; return; }
            if (dataItem.IsComosItem && ((HK_General.RoleRE + HK_General.RoleDL + HK_General.RoleAdmin) & HK_General.UserComos.Roles) == 0)
            {
                e.Cancel = true;
                return;
            }
            if (dataItem.IsLibItem && (HK_General.RoleAdmin & HK_General.UserComos.Roles) == 0)
            {
                e.Cancel = true;
                return;
            }
            if (dataItem.IsLibItem && (!dataItem.IsOwned || dataItem.IsInherit) && dataItem.IsInherit)
            {
                e.Cancel = true;
            }
        }

        private void dgDiagAssigned_GotFocus(object sender, RoutedEventArgs e)
        {
            ActiveAreaChange?.Invoke(this, ActiveArea.Assigned);
            //通过赋值触发事件
            dgDiagAssigned.SelectedItem = dgDiagAssigned.SelectedItem;
        }

        private void dgDiagAvailable_GotFocus(object sender, RoutedEventArgs e)
        {
            ActiveAreaChange?.Invoke(this, ActiveArea.Available);
            //通过赋值触发事件
            dgDiagAvailable.SelectedItem = dgDiagAvailable.SelectedItem;
        }

        private void dgDiag_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true; // 阻止Delete键的功能
            }
        }
    }
}
