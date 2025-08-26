using iEngr.Hookup.ViewModels;
using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UcMatMain.xaml 的交互逻辑
    /// </summary>
    public partial class UcMatMain : UserControl
    {
        public UcMatMain()
        {
            InitializeComponent();
            var VmMatMain = new MatMainViewModel();
            DataContext = VmMatMain;
            // 获取子控件的 ViewModel
            VmMatMain.VmMatData = ucMD.DataContext as MatDataViewModel;
            VmMatMain.VmMatList = ucML.DataContext as MatListViewModel;
        }
    }
}
