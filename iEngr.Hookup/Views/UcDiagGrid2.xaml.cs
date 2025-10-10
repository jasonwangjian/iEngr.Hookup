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
    public partial class UcDiagGrid2 : UserControl
    {
        public UcDiagGrid2()
        {
            InitializeComponent();
        }
        private void ClearAllSorting_click(object sender, RoutedEventArgs e)
        {
            foreach (var column in dgDiagNode.Columns)
            {
                column.SortDirection = null;
            }
            dgDiagNode.Items.SortDescriptions.Clear();
            foreach (var column in dgDiagLib.Columns)
            {
                column.SortDirection = null;
            }
            dgDiagLib.Items.SortDescriptions.Clear();
        }
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var dataItem = e.Row.Item as DiagramItem;
            if (dataItem != null && (!dataItem.IsOwned || dataItem.IsInherit))
            {
                e.Cancel = true;
            }
        }

    }
}
