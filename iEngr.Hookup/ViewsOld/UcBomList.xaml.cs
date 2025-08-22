using Comos.Controls;
using Comos.UIF;
using ComosQueryInterface;
using ComosQueryXObj;
using Plt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using IContainer = Comos.Controls.IContainer;

namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UcBomData.xaml 的交互逻辑
    /// </summary>
    public partial class UcBomList : UserControl
    {
        public UcBomList()
        {
            InitializeComponent();
            DataGridComboBoxColumnBindingIni();
            //dcNameCn.Binding = new Binding("")
        }
        private void DataGridComboBoxColumnBindingIni()
        {
            var columnsDiscipline = dgBOM.Columns.OfType<DataGridComboBoxColumn>()
                                                .Where(c => c.Header.ToString().Contains("专业"));
            foreach (DataGridComboBoxColumn column in columnsDiscipline)
            {
                var binding = new Binding("Disciplines");
                binding.Source = DataContext;
                BindingOperations.SetBinding(column, DataGridComboBoxColumn.ItemsSourceProperty, binding);
            }
            var columnsResponsible = dgBOM.Columns.OfType<DataGridComboBoxColumn>()
                                                .Where(c => c.Header.ToString().Contains("范围"));
            foreach (DataGridComboBoxColumn column in columnsResponsible)
            {
                var binding = new Binding("Responsibles");
                binding.Source = DataContext;
                BindingOperations.SetBinding(column, DataGridComboBoxColumn.ItemsSourceProperty, binding);
            }

        }

        // 禁止回车换行的事件处理
        private void dgBOM_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    e.Handled = true; // 阻止默认行为

            //    //// 可选：移动到下一列而不是下一行
            //    //var dataGrid = sender as DataGrid;
            //    //if (dataGrid != null)
            //    //{
            //    //    dataGrid.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            //    //}
            //}
        }
    }
}
