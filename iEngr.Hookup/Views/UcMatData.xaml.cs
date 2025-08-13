using iEngr.Hookup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Expression = System.Linq.Expressions.Expression;


namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UnMatData.xaml 的交互逻辑
    /// </summary>
    public partial class UcMatData : UserControl
    {
        public UcMatData()
        {
            InitializeComponent();
            HK_General HK_General = new HK_General();
            int intLan = HK_General.intLan;
            string[] portDef = HK_General.portDef;
        }
        private void cbSpec_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    string cmbName = (sender as ComboBox).Name;
            //    string value = (sender as ComboBox).Text.Trim();
            //    string key = (cbMainSpecT1.SelectedItem as HKLibSpecDic).ID;
            //    if (cmbName == "cbMainSpec2")
            //        key = (cbMainSpecT2.SelectedItem as HKLibSpecDic).ID;
            //    else if (cmbName == "cbMainSpec3")
            //        key = (cbMainSpecT3.SelectedItem as HKLibSpecDic).ID;
            //    else if (cmbName == "cbAuxSpec1")
            //        key = (cbAuxSpecT1.SelectedItem as HKLibSpecDic).ID;
            //    else if (cmbName == "cbAuxSpec2")
            //        key = (cbAuxSpecT2.SelectedItem as HKLibSpecDic).ID;
            //    else if (cmbName == "cbAuxSpec3")
            //        key = (cbAuxSpecT3.SelectedItem as HKLibSpecDic).ID;
            //    else if (cmbName == "cbSizeP1")
            //        key = (cbTypeP1.SelectedItem as HKLibSpecDic).ID;
            //    else if (cmbName == "cbSizeP2")
            //        key = (cbTypeP2.SelectedItem as HKLibSpecDic).ID;
            //    if (!dicNoLinkSpecStr[key].Contains(value))
            //    {
            //        HKLibGenOption newSpec = new HKLibGenOption
            //        {
            //            ID = value,
            //            NameCn = (intLan == 0) ? value : null,
            //            NameEn = (intLan != 0) ? value : null,
            //        };
            //        dicNoLinkSpec[key].Add(newSpec);
            //        dicNoLinkSpecStr[key].Add(value);
            //    }
            //    if (string.IsNullOrEmpty(value) || (sender as ComboBox).SelectedIndex != -1) return;
            //    for (int i = 0; i < dicNoLinkSpec[key].Count(); i++)
            //    {
            //        if (dicNoLinkSpec[key][i].ID == value)
            //        {
            //            (sender as ComboBox).SelectedIndex = i;
            //            break;
            //        }

            //    }

            //}
        }
    }
}
