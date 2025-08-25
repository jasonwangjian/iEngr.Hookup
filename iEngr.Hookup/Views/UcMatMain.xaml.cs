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
            var mainVM = new MatMainViewModel();
            DataContext = mainVM;
            // 获取子控件的 ViewModel
            mainVM.VmMatData = ucMD.DataContext as MatDataViewModel;
            mainVM.VmMatList = ucML.DataContext as MatListViewModel;
        }
        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            (ucML.DataContext as MatListViewModel).BtnCommand = "Query";
        }

        private string getMainSpec(MatDataViewModel matData)
        {
            List<string> specs = new List<string>();
            if (matData.MainSpecT1All?.Count > 0)
            {
                specs.Add($"{matData.MainSpecT1?.ID}:{matData.MainSpecV1?.ID}");
                if (matData.MainSpecT2All?.Count > 0)
                {
                    specs.Add($"{matData.MainSpecT2?.ID}:{matData.MainSpecV2?.ID}");
                    if (matData.MainSpecT3All?.Count > 0)
                    {
                        specs.Add($"{matData.MainSpecT3?.ID}:{matData.MainSpecV3?.ID}");
                    }
                }
            }
            return string.Join(",", specs);
        }
        private string getAuxSpec(MatDataViewModel matData)
        {
            List<string> specs = new List<string>();
            if (matData.AuxSpecT1All?.Count > 0)
            {
                specs.Add($"{matData.AuxSpecT1?.ID}:{matData.AuxSpecV1?.ID}");
                if (matData.AuxSpecT2All?.Count > 0)
                {
                    specs.Add($"{matData.AuxSpecT2?.ID}:{matData.AuxSpecV2?.ID}");
                    if (matData.AuxSpecT3All?.Count > 0)
                    {
                        specs.Add($"{matData.AuxSpecT3?.ID}:{matData.AuxSpecV3?.ID}");
                    }
                }
            }
            return string.Join(",", specs);
        }
        private string getPortType1(MatDataViewModel matData)
        {
            return string.Join("|", matData.TypeAllP1.Select(x => x.ID).Where(x => !string.IsNullOrEmpty(x)).ToList());
        }
        private string getSpecExp(string field, List<string> input)
        {
            if (input == null || !input.Any())
                return string.Empty;

            var conditions = input
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => $"{field} LIKE '%{s}%'");

            return string.Join(" AND ", conditions);
        }
    }
}
