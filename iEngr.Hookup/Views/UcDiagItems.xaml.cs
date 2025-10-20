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
        public UcDiagItems()
        {
            InitializeComponent();
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
            if (dataItem == null) {e.Cancel = true;return; }
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
            if (dataItem.IsLibItem && (!dataItem.IsOwned || dataItem.IsInherit))
            {
                e.Cancel = true;
            }
        }

        private void dgDiagAssigned_GotFocus(object sender, RoutedEventArgs e)
        {
            //通过赋值触发事件
            dgDiagAssigned.SelectedItem = dgDiagAssigned.SelectedItem;
        }

        private void dgDiagAvailable_GotFocus(object sender, RoutedEventArgs e)
        {
            //通过赋值触发事件
            dgDiagAvailable.SelectedItem = dgDiagAvailable.SelectedItem;
        }
    }
}
