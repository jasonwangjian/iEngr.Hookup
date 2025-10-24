using iEngr.Hookup.ViewModels;
using Plt;
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
using Xceed.Wpf.Toolkit.Primitives;

namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UcLabelList.xaml 的交互逻辑
    /// </summary>
    public partial class UcAppliedComos : UserControl
    {
        public event EventHandler<AppliedComosItem> ComosItemContext;
        public event EventHandler<AppliedComosItem> ComosItemDoubleClick;

        public UcAppliedComos()
        {
            InitializeComponent();
        }
        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustColumnWidth(sender as ListView);
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustColumnWidth(sender as ListView);
        }

        private void AdjustColumnWidth(ListView listView)
        {
            if (listView?.View is GridView gridView && gridView.Columns.Count > 0 && listView.ActualWidth >10)
            {
                // 设置列宽为自动（根据内容）或固定值
                gridView.Columns[0].Width = listView.ActualWidth - 10; // 减去边距
            }
        }

        private void ListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

            AppliedComosItem item = ((sender as ListView).DataContext as AppliedComosViewModel).SelectedItem;
            ComosItemContext?.Invoke(this, item);
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AppliedComosItem item = ((sender as ListView).DataContext as AppliedComosViewModel).SelectedItem;
            ComosItemDoubleClick?.Invoke(this, item);
        }

        private void ListView_ContextMenuOpening(object sender, MouseButtonEventArgs e)
        {
            AppliedComosItem item = ((sender as ListView).DataContext as AppliedComosViewModel).SelectedItem;
            ComosItemContext?.Invoke(this, item);
        }
    }
}
