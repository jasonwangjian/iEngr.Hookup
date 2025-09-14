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
    public partial class UcDiagGrid : UserControl
    {
        public UcDiagGrid()
        {
            InitializeComponent();
        }
        private void ClearAllSorting_click(object sender, RoutedEventArgs e)
        {
            foreach (var column in dgDiag.Columns)
            {
                column.SortDirection = null;
            }
            dgDiag.Items.SortDescriptions.Clear();
        }
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var dataItem = e.Row.Item as DiagramItem;
            if (dataItem != null && !dataItem.IsOwned)
            {
                e.Cancel = true;
            }
        }
    }
}
