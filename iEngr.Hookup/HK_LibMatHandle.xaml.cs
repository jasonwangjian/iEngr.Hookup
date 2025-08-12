using iEngr.Hookup.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace iEngr.Hookup
{
    /// <summary>
    /// Hk_LibMatHandle.xaml 的交互逻辑
    /// </summary>
    public partial class HK_LibMatHandle : UserControl
    {
        public HK_LibMatHandle()
        {
            InitializeComponent();
        }

        private void text_Click(object sender, RoutedEventArgs e)
        {
            (ucMD.DataContext as MatDataViewModel).TypeP1ID = "FLNPS";
        }
    }
}
