using iEngr.Hookup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
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
using System.Xml.Linq;

namespace iEngr.Hookup
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class HK_LibMat : UserControl
    {
        public static int intLan;
        private ObservableCollection<HKMatGenLib> result = new ObservableCollection<HKMatGenLib>();

        private ObservableCollection<HKMatSubCat> hKMatSubCats = new ObservableCollection<HKMatSubCat>
            {
                new HKMatSubCat
                {
                ID = string.Empty,
                SpecCn = "所有小类",
                SpecEn = "All Sub Categories"
                }
            };
        private string[] portDef = { "EQ1", "DF1", "AS1", "NEQ" };
        private Dictionary<string, ObservableCollection<string>> dicNoLinkSpecStr = new Dictionary<string, ObservableCollection<string>>();
        private Dictionary<string, ObservableCollection<HKLibGenOption>> dicNoLinkSpec = new Dictionary<string, ObservableCollection<HKLibGenOption>>();
        private string strTypeP1All, strTypeP2All, strTypeP1, strTypeP2, strCondP1, strCondP2, strCondSubCat;
        private string strSpecMainAll, strSpecMain, strSpecAuxAll, strSpecAux;
        private List<string> lstMainSpec = new List<string>();
        private List<string> lstAuxSpec = new List<string>();


        HKMatGenLib currMat = new HKMatGenLib();

        public HK_LibMat()
        {
            InitializeComponent();
            intLan = 0; // 0: 中文； 其它为英文
            conn = GetConnection();
            dgResult.ItemsSource = result;
            cbMainCat.ItemsSource = GetHKMatMainCats();
            cbSubCat.ItemsSource = hKMatSubCats;
            cbMainCat.SelectedIndex = 0;
            SetDicMatGen();
            dcNameCn.Visibility = (intLan == 0)? Visibility.Visible: Visibility.Collapsed;
            dcNameEn.Visibility = (intLan == 0) ? Visibility.Collapsed : Visibility.Visible;
            dcMoreCn.Visibility = (intLan == 0) ? Visibility.Visible : Visibility.Collapsed;
            dcMoreEn.Visibility = (intLan == 0) ? Visibility.Collapsed : Visibility.Visible;
            dcRemCn.Visibility = (intLan == 0) ? Visibility.Visible : Visibility.Collapsed;
            dcRemEn.Visibility = (intLan == 0) ? Visibility.Collapsed : Visibility.Visible;

        }



        private void cbMainCat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            hKMatSubCats = GetHKMatSubCats(cbMainCat.SelectedItem as HKMatMainCat);
            cbSubCat.SelectedIndex = 0;
            UpdateQueryResult();

        }
        private void cbSubCat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HKMatSubCat selectedItem = cbSubCat.SelectedItem as HKMatSubCat;
            if (selectedItem == null)
            {
                strTypeP1 = string.Empty;
                strTypeP2 = string.Empty;
                strSpecMain = string.Empty;
                strSpecAux = string.Empty;
                return;
            }
            else if ((cbSubCat.SelectedItem as HKMatSubCat).ID == string.Empty)
            {
                strTypeP1 = strTypeP1All;
                strTypeP2 = strTypeP2All;
                strSpecMain = string.Empty; // strSpecMainAll;
                strSpecAux = string.Empty; // strSpecAuxAll;
                strCondSubCat = string.Empty;
            }
            else
            {
                strTypeP1 = selectedItem.TypeP1;
                strTypeP1 = (strTypeP1 == "NA" || strTypeP1 == "IS") ? string.Empty : strTypeP1;
                currMat.AlterCode = selectedItem.TypeP2;
                if (currMat.AlterCode == "AS1")
                {
                    cbTypeP2.IsEnabled = false;
                    cbSizeP2.IsEnabled = false;
                }
                else
                {
                    cbTypeP2.IsEnabled = true;
                    cbSizeP2.IsEnabled = true;
                }
                strTypeP2 = (portDef.Contains(currMat.AlterCode)) ? strTypeP1 : currMat.AlterCode;
                strTypeP2 = (currMat.AlterCode == "NA" || currMat.AlterCode == "IS") ? string.Empty : strTypeP2;
                //strTypeP1All = strTypeP1;
                //strTypeP2All = strTypeP2;

                strSpecMain = selectedItem.TechSpecMain?.Trim();
                strSpecAux = selectedItem.TechSpecAux?.Trim();
                strCondP1 = string.IsNullOrEmpty(strTypeP1All)? $"mgl.TypeP1 in {GeneralFun.ConvertToStringScope(strTypeP1All, ',')}": string.Empty;
                strCondP2 = string.IsNullOrEmpty(strTypeP2All) ? $"mgl.TypeP2 in {GeneralFun.ConvertToStringScope(strTypeP2All, ',')}":string.Empty;
                strCondSubCat = $"mgl.SubCatID in {GeneralFun.ConvertToStringScope(selectedItem.ID, ',')}";

                

            }

            // 处理主参数 TechSpecMain
            var lstSpecMain = strSpecMain.Split(',')
                         .Select(item => item.Trim())
                         .Where(item => !string.IsNullOrWhiteSpace(item))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToList();
            // 最多处理3个规格
            for (int i = 0; i < 3; i++)
            {
                string searchKey = lstSpecMain.Count > i ? lstSpecMain[i] : null;
                string seg = null;
                // 查找匹配的规格段
                if (!string.IsNullOrEmpty(searchKey))
                {
                    bool isFound = false;
                    seg = searchKey;
                    if (lstMainSpec.Count > i && searchKey.Contains(lstMainSpec[i].Split(':')[0])) //ItemSource不变
                    {
                        isFound = true;
                        seg = lstMainSpec[i];
                        lstSpecMain[i] = seg;
                        // 解析规格段
                        string[] segParts = seg.Split(':');
                        string specT = segParts[0];
                        string specV = segParts[1];
                        // 设置组合框选择
                        SetCmbMainSpecSelection(i, specT, specV);
                    }
                    else
                    {
                        foreach (string part in lstMainSpec)
                        {
                            string[] partSegments = part.Split(':');
                            if (partSegments.Length > 0 && searchKey.Contains(partSegments[0]))
                            {
                                isFound = true;
                                SetCmbMainSpecTSource(i, GetLibSpecDic(seg));//设定新的ItemSource
                                seg = part;
                                lstSpecMain[i] = seg;

                                // 解析规格段
                                string[] segParts = seg.Split(':');
                                string specT = segParts[0];
                                string specV = segParts[1];
                                // 设置组合框选择
                                SetCmbMainSpecSelection(i, specT, specV);
                                break;
                            }
                        }
                    }
                    if (!isFound ) SetCmbMainSpecTSource(i, GetLibSpecDic(seg));//设定ItemSource
                }
                // 处理未找到的情况
                else
                {
                    SetCmbMainSpecTLastIndex(i);
                    SetCmbMainSpecTSource(i, null);
                }
            }



            //cbMainSpecT1.ItemsSource = null;
            //cbMainSpec1.ItemsSource = null;
            //cbMainSpecT2.ItemsSource = null;
            //cbMainSpec2.ItemsSource = null;
            //cbMainSpecT3.ItemsSource = null;
            //cbMainSpec3.ItemsSource = null;
            //cbMainSpecT1.Visibility = Visibility.Collapsed;
            ////cbMainSpec1.Visibility = Visibility.Collapsed;
            //cbMainSpecT2.Visibility = Visibility.Collapsed;
            ////cbMainSpec2.Visibility = Visibility.Collapsed;
            //cbMainSpecT3.Visibility = Visibility.Collapsed;
            ////cbMainSpec3.Visibility = Visibility.Collapsed;
            //lbMainSpec.Visibility = Visibility.Collapsed;
            //if (lstSpecMain?.Count() > 0 && !string.IsNullOrEmpty(lstSpecMain[0]))
            //{
            //    cbMainSpecT1.ItemsSource = GetLibSpecDic(lstSpecMain[0]);
            //    cbMainSpecT1.SelectedIndex = 0;
            //    cbMainSpecT1.Visibility = Visibility.Visible;
            //    //cbMainSpec1.Visibility = Visibility.Visible;
            //    lbMainSpec.Visibility = Visibility.Visible;
            //    if (lstSpecMain?.Count() > 1 && !string.IsNullOrEmpty(lstSpecMain[1]))
            //    {
            //        cbMainSpecT2.ItemsSource = GetLibSpecDic(lstSpecMain[1]);
            //        cbMainSpecT2.SelectedIndex = 0;
            //        cbMainSpecT2.Visibility = Visibility.Visible;
            //        //cbMainSpec2.Visibility = Visibility.Visible;
            //        if (lstSpecMain?.Count() > 2 && !string.IsNullOrEmpty(lstSpecMain[2]))
            //        {
            //            cbMainSpecT3.ItemsSource = GetLibSpecDic(lstSpecMain[2]);
            //            cbMainSpecT3.SelectedIndex = 0;
            //            cbMainSpecT3.Visibility = Visibility.Visible;
            //            //cbMainSpec3.Visibility = Visibility.Visible;

            //        }
            //    }
            //}



            // 处理主参数 TechSpecAux
            var lstSpecAux = strSpecAux.Split(',')
                         .Select(item => item.Trim())
                         .Where(item => !string.IsNullOrWhiteSpace(item))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToList();
            // 最多处理3个规格
            for (int i = 0; i < 3; i++)
            {
                string searchKey = lstSpecAux.Count > i ? lstSpecAux[i] : null;
                string seg = null;
                // 查找匹配的规格段
                if (!string.IsNullOrEmpty(searchKey))
                {
                    bool isFound = false;
                    seg = searchKey;
                    if (lstAuxSpec.Count > i && searchKey.Contains(lstAuxSpec[i].Split(':')[0])) //ItemSource不变
                    {
                        isFound = true;
                        seg = lstAuxSpec[i];
                        lstSpecAux[i] = seg;
                        // 解析规格段
                        string[] segParts = seg.Split(':');
                        string specT = segParts[0];
                        string specV = segParts[1];
                        // 设置组合框选择
                        SetCmbAuxSpecSelection(i, specT, specV);
                    }
                    else
                    {
                        foreach (string part in lstAuxSpec)
                        {
                            string[] partSegments = part.Split(':');
                            if (partSegments.Length > 0 && searchKey.Contains(partSegments[0]))
                            {
                                isFound = true;
                                SetCmbAuxSpecTSource(i, GetLibSpecDic(seg));//设定新的ItemSource
                                seg = part;
                                lstSpecAux[i] = seg;

                                // 解析规格段
                                string[] segParts = seg.Split(':');
                                string specT = segParts[0];
                                string specV = segParts[1];
                                // 设置组合框选择
                                SetCmbAuxSpecSelection(i, specT, specV);
                                break;
                            }
                        }
                    }
                    if (!isFound) SetCmbAuxSpecTSource(i, GetLibSpecDic(seg));//设定ItemSource
                }
                // 处理未找到的情况
                else
                {
                    SetCmbAuxSpecTLastIndex(i);
                    SetCmbAuxSpecTSource(i, null);
                }
            }
            //cbAuxSpecT1.ItemsSource = null;
            //cbAuxSpec1.ItemsSource = null;
            //cbAuxSpecT2.ItemsSource = null;
            //cbAuxSpec2.ItemsSource = null;
            //cbAuxSpecT3.ItemsSource = null;
            //cbAuxSpec3.ItemsSource = null;
            //cbAuxSpecT1.Visibility = Visibility.Collapsed;
            ////cbAuxSpec1.Visibility = Visibility.Collapsed;
            //cbAuxSpecT2.Visibility = Visibility.Collapsed;
            ////cbAuxSpec2.Visibility = Visibility.Collapsed;
            //cbAuxSpecT3.Visibility = Visibility.Collapsed;
            ////cbAuxSpec3.Visibility = Visibility.Collapsed;
            //lbAuxSpec.Visibility = Visibility.Collapsed;
            //if (lstSpecAux?.Count() > 0 && !string.IsNullOrEmpty(lstSpecAux[0]))
            //{
            //    cbAuxSpecT1.ItemsSource = GetLibSpecDic(lstSpecAux[0]);
            //    cbAuxSpecT1.SelectedIndex = 0;
            //    cbAuxSpecT1.Visibility = Visibility.Visible;
            //    //cbAuxSpec1.Visibility = Visibility.Visible;
            //    lbAuxSpec.Visibility = Visibility.Visible;
            //    if (lstSpecAux?.Count() > 1 && !string.IsNullOrEmpty(lstSpecAux[1]))
            //    {
            //        cbAuxSpecT2.ItemsSource = GetLibSpecDic(lstSpecAux[1]);
            //        cbAuxSpecT2.SelectedIndex = 0;
            //        cbAuxSpecT2.Visibility = Visibility.Visible;
            //        //cbAuxSpec2.Visibility = Visibility.Visible;
            //        if (lstSpecAux?.Count() > 2 && !string.IsNullOrEmpty(lstSpecAux[2]))
            //        {
            //            cbAuxSpecT3.ItemsSource = GetLibSpecDic(lstSpecAux[2]);
            //            cbAuxSpecT3.SelectedIndex = 0;
            //            cbAuxSpecT3.Visibility = Visibility.Visible;
            //            //cbAuxSpec3.Visibility = Visibility.Visible;
            //        }
            //    }
            //}

            // 处理端口一、二
            HKLibSpecDic selectedType = new HKLibSpecDic();
            if (string.IsNullOrEmpty(strTypeP1) || strTypeP1 == "NA" || strTypeP1 == "IS" || strTypeP1 == "NA," || strTypeP1 == "IS,")
            {
                cbTypeP1.ItemsSource = null;
            }
            else
            {
                cbTypeP1.ItemsSource = GetHKPortTypes(strTypeP1);
                SetCmbPortTypeSelection(0, currMat.TypeP1);
                selectedType = cbTypeP1.SelectedItem as HKLibSpecDic;
                cbSizeP1.IsEditable = selectedType?.Class != null && selectedType?.Class?.ToUpper() != "LINK";
            }
            if (string.IsNullOrEmpty(strTypeP2) || strTypeP2 == "NA" || strTypeP2 == "IS" || strTypeP2 == "NA," || strTypeP2 == "IS,")
            {
                cbTypeP2.ItemsSource = null;
            }
            else
            {
                if (portDef.Contains(strTypeP2))
                    cbTypeP2.ItemsSource = GetHKPortTypes(strTypeP1);
                else
                    cbTypeP2.ItemsSource = GetHKPortTypes(strTypeP2);
                SetCmbPortTypeSelection(1, currMat.TypeP2);
                selectedType = cbTypeP2.SelectedItem as HKLibSpecDic;
                cbSizeP2.IsEditable = selectedType?.Class != null && selectedType?.Class?.ToUpper() != "LINK";
            }
            UpdateQueryResult();
        }


        private void cbSpecT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HKLibSpecDic selectedItem = ((sender as ComboBox).SelectedItem as HKLibSpecDic);
            if (selectedItem == null) return;
            if ((sender as ComboBox).Name == "cbMainSpecT1")
            {
                cbMainSpec1.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbMainSpec1.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbMainSpec1.IsEditable = selectedItem.Class?.ToUpper() != "LINK";
                cbMainSpec1.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbMainSpecT1.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : Visibility.Visible;
                //cbMainSpec1.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbMainSpec1.Visibility;
                if (cbMainSpec1.IsEditable) ResetLstMainSpec();
            }
            else if ((sender as ComboBox).Name == "cbMainSpecT2")
            {
                cbMainSpec2.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbMainSpec2.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbMainSpec2.IsEditable = selectedItem.Class?.ToUpper() != "LINK";
                cbMainSpec2.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbMainSpecT2.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : Visibility.Visible;
                //cbMainSpec2.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbMainSpec2.Visibility;
                if (cbMainSpec2.IsEditable) ResetLstMainSpec();
            }
            else if ((sender as ComboBox).Name == "cbMainSpecT3")
            {
                cbMainSpec3.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbMainSpec3.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbMainSpec3.IsEditable = selectedItem.Class?.ToUpper() != "LINK";
                cbMainSpec3.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbMainSpecT3.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : Visibility.Visible;
                //cbMainSpec3.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbMainSpec3.Visibility;
                if (cbMainSpec3.IsEditable) ResetLstMainSpec();
            }
            else if ((sender as ComboBox).Name == "cbAuxSpecT1")
            {
                cbAuxSpec1.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbAuxSpec1.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbAuxSpec1.IsEditable = selectedItem.Class?.ToUpper() != "LINK";
                cbAuxSpec1.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbAuxSpecT1.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpecT1.Visibility;
                //cbAuxSpec1.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpec1.Visibility;
                if (cbAuxSpec1.IsEditable) ResetLstAuxSpec();
            }
            else if ((sender as ComboBox).Name == "cbAuxSpecT2")
            {
                cbAuxSpec2.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbAuxSpec2.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbAuxSpec2.IsEditable = (selectedItem.Class?.ToUpper() == "LINK") ? false : true;
                cbAuxSpec2.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbAuxSpecT2.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpecT2.Visibility;
                //cbAuxSpec2.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpec2.Visibility;
                if (cbAuxSpec2.IsEditable) ResetLstAuxSpec();
            }
            else if ((sender as ComboBox).Name == "cbAuxSpecT3")
            {
                cbAuxSpec3.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbAuxSpec3.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbAuxSpec3.IsEditable = (selectedItem.Class?.ToUpper() == "LINK") ? false : true;
                cbAuxSpec3.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbAuxSpecT3.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpecT3.Visibility;
                //cbAuxSpec3.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpec3.Visibility;
                if (cbAuxSpec3.IsEditable) ResetLstAuxSpec();
            }
            UpdateQueryResult();
        }



        private void cbMainSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsKeyboardFocusWithin)
            {


            }
            ResetLstMainSpec();
            //if ((cbMainSpec1.SelectedItem as HKLibGenOption) != null && (cbMainSpec1.SelectedItem as HKLibGenOption).ID != string.Empty
            //   || (cbMainSpec2.SelectedItem as HKLibGenOption) != null && (cbMainSpec2.SelectedItem as HKLibGenOption).ID != string.Empty
            //   || (cbMainSpec3.SelectedItem as HKLibGenOption) != null && (cbMainSpec3.SelectedItem as HKLibGenOption).ID != string.Empty)
                UpdateQueryResult();
        }
        private void cbAuxSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetLstAuxSpec();
           //if ((cbAuxSpec1.SelectedItem as HKLibGenOption) != null && (cbAuxSpec1.SelectedItem as HKLibGenOption).ID != string.Empty
            //    || (cbAuxSpec2.SelectedItem as HKLibGenOption) != null && (cbAuxSpec2.SelectedItem as HKLibGenOption).ID != string.Empty
            //    || (cbAuxSpec3.SelectedItem as HKLibGenOption) != null && (cbAuxSpec3.SelectedItem as HKLibGenOption).ID != string.Empty)
                UpdateQueryResult();
        }

        private void ResetLstMainSpec()
        {
            lstMainSpec.Clear();
            if ((cbMainSpecT1.SelectedItem as HKLibSpecDic) != null && (cbMainSpecT1.SelectedItem as HKLibSpecDic).ID != "-")
                lstMainSpec.Add($"{(cbMainSpecT1.SelectedItem as HKLibSpecDic).ID}:{(cbMainSpec1.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbMainSpecT2.SelectedItem as HKLibSpecDic) != null && (cbMainSpecT2.SelectedItem as HKLibSpecDic).ID != "-")
                lstMainSpec.Add($"{(cbMainSpecT2.SelectedItem as HKLibSpecDic).ID}:{(cbMainSpec2.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbMainSpecT3.SelectedItem as HKLibSpecDic) != null && (cbMainSpecT3.SelectedItem as HKLibSpecDic).ID != "-")
                lstMainSpec.Add($"{(cbMainSpecT3.SelectedItem as HKLibSpecDic).ID}:{(cbMainSpec3.SelectedItem as HKLibGenOption)?.ID}");
        }
        private void ResetLstAuxSpec()
        {
            lstAuxSpec.Clear();
            if ((cbAuxSpecT1.SelectedItem as HKLibSpecDic) != null && (cbAuxSpecT1.SelectedItem as HKLibSpecDic).ID != "-")
                lstAuxSpec.Add($"{(cbAuxSpecT1.SelectedItem as HKLibSpecDic).ID}:{(cbAuxSpec1.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbAuxSpecT2.SelectedItem as HKLibSpecDic) != null && (cbAuxSpecT2.SelectedItem as HKLibSpecDic).ID != "-")
                lstAuxSpec.Add($"{(cbAuxSpecT2.SelectedItem as HKLibSpecDic).ID}:{(cbAuxSpec2.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbAuxSpecT3.SelectedItem as HKLibSpecDic) != null && (cbAuxSpecT3.SelectedItem as HKLibSpecDic).ID != "-")
                lstAuxSpec.Add($"{(cbAuxSpecT3.SelectedItem as HKLibSpecDic).ID}:{(cbAuxSpec3.SelectedItem as HKLibGenOption)?.ID}");
        }

        private void cbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HKLibSpecDic selectedItem = ((sender as ComboBox).SelectedItem as HKLibSpecDic);
            if (selectedItem == null) return;
            if ((sender as ComboBox).IsKeyboardFocusWithin)
            {
                currMat.TypeP1 = selectedItem.ID;

            }
            //if ((sender as ComboBox).Name == "cbTypeP1")
            //{

            //    strTypeP1 = selectedItem.ID;
            //    if (string.IsNullOrEmpty(strTypeP1) || strTypeP1 == "NA" || strTypeP1 == "IS" || strTypeP1 == "NA," || strTypeP1 == "IS,")
            //        strTypeP1 = string.Empty;
            //    if (selectedItem.ID == string.Empty)
            //        strTypeP1 = strTypeP1All;
            //    if (portDef.Contains((cbSubCat.SelectedItem as HKMatSubCat).TypeP2))
            //    {
            //        cbTypeP2.ItemsSource = cbTypeP1.ItemsSource;
            //        cbTypeP2.SelectedIndex = cbTypeP1.SelectedIndex;
            //        //cbSizeP2.ItemsSource = cbSizeP1.ItemsSource;
            //        //cbSizeP2.SelectedIndex = cbSizeP1.SelectedIndex;
            //    }
            //    cbSizeP1.ItemsSource = GetGeneralSpecOptions(selectedItem);
            //    cbSizeP1.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
            //    cbSizeP1.IsEditable = selectedItem.Class != null && selectedItem.Class?.ToUpper() != "LINK";
            //}
            //else if ((sender as ComboBox).Name == "cbTypeP2")
            //{
            //    strTypeP2 = selectedItem.ID;
            //    if (string.IsNullOrEmpty(strTypeP2) || strTypeP2 == "NA" || strTypeP2 == "IS" || strTypeP2 == "NA," || strTypeP2 == "IS,")
            //        strTypeP2 = string.Empty;
            //    else if (portDef.Contains(strTypeP2))
            //        strTypeP2 = strTypeP1;
            //    if (selectedItem.ID == string.Empty)
            //        strTypeP2 = strTypeP2All;
            //    cbSizeP2.ItemsSource = GetGeneralSpecOptions(selectedItem);
            //    cbSizeP2.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
            //    cbSizeP2.IsEditable = selectedItem.Class != null && selectedItem.Class?.ToUpper() != "LINK";
            //}
            UpdateQueryResult();
        }

        private void cbSpec_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string cmbName = (sender as ComboBox).Name;
                string value = (sender as ComboBox).Text.Trim();
                string key = (cbMainSpecT1.SelectedItem as HKLibSpecDic).ID;
                if (cmbName == "cbMainSpec2")
                    key = (cbMainSpecT2.SelectedItem as HKLibSpecDic).ID;
                else if (cmbName == "cbMainSpec3")
                    key = (cbMainSpecT3.SelectedItem as HKLibSpecDic).ID;
                else if (cmbName == "cbAuxSpec1")
                    key = (cbAuxSpecT1.SelectedItem as HKLibSpecDic).ID;
                else if (cmbName == "cbAuxSpec2")
                    key = (cbAuxSpecT2.SelectedItem as HKLibSpecDic).ID;
                else if (cmbName == "cbAuxSpec3")
                    key = (cbAuxSpecT3.SelectedItem as HKLibSpecDic).ID;
                else if (cmbName == "cbSizeP1")
                    key = (cbTypeP1.SelectedItem as HKLibSpecDic).ID;
                else if (cmbName == "cbSizeP2")
                    key = (cbTypeP2.SelectedItem as HKLibSpecDic).ID;
                if (!dicNoLinkSpecStr[key].Contains(value))
                {
                    HKLibGenOption newSpec = new HKLibGenOption
                    {
                        ID = value,
                        NameCn = (intLan == 0) ? value : null,
                        NameEn = (intLan != 0) ? value : null,
                    };
                    dicNoLinkSpec[key].Add(newSpec);
                    dicNoLinkSpecStr[key].Add(value);
                }
                if (string.IsNullOrEmpty(value) || (sender as ComboBox).SelectedIndex != -1) return;
                for (int i = 0; i < dicNoLinkSpec[key].Count(); i++)
                {
                    if (dicNoLinkSpec[key][i].ID == value)
                    {
                        (sender as ComboBox).SelectedIndex = i;
                        break;
                    }

                }

            }
        }


        private void tbMoreSpec_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UpdateQueryResult();
        }

        private void cbMatMat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbMatMat.SelectedIndex >= 0)
                UpdateQueryResult();
        }

        //private void cbMainSpec_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    string strSpec = (sender as ComboBox).Text.Trim();
        //    if (string.IsNullOrEmpty(strSpec) || (sender as ComboBox).SelectedIndex != -1) return;
        //    string key = (cbMainSpecT1.SelectedItem as HKLibSpecDic).ID;
        //    if ((sender as ComboBox).Name == "cbMainSpec2")
        //        key = (cbMainSpecT2.SelectedItem as HKLibSpecDic).ID;
        //    else if ((sender as ComboBox).Name == "cbMainSpec3")
        //        key = (cbMainSpecT3.SelectedItem as HKLibSpecDic).ID;
        //    if (!dicNoLinkSpecStr[key].Contains(strSpec))
        //    {
        //        HKLibGenOption newSpec = new HKLibGenOption
        //        {
        //            ID = strSpec,
        //            NameCn = (intLan == 0) ? strSpec : null,
        //            NameEn = (intLan != 0) ? strSpec : null,
        //        };
        //        dicNoLinkSpec[key].Add(newSpec);
        //        dicNoLinkSpecStr[key].Add(strSpec);

        //    }


        //}

        private void cbSizeP1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2 == "AS1" ||
                (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2 == "DF1") && cbSizeP2.ItemsSource != null)
            {
                cbSizeP2.SelectedIndex = cbSizeP1.SelectedIndex;
            }
            if (cbSizeP1.SelectedIndex >= 0 && (cbSizeP1.SelectedItem as HKLibGenOption)?.ID != string.Empty)
            {
                strCondP1 = $"mgl.SizeP1 = '{(cbSizeP1.SelectedItem as HKLibGenOption).ID}'";
                if (!string.IsNullOrEmpty(strTypeP1))
                    strCondP1 = $"{strCondP1} AND mgl.TypeP1 in {GeneralFun.ConvertToStringScope(strTypeP1, ',')}";
                UpdateQueryResult();
            }
            else if (cbSizeP1.SelectedIndex == 0)
            {
                //strCondP1 = $"mgl.SizeP1 IS NOT NULL AND LTRIM(RTRIM(mgl.SizeP1)) <> ''";
                strCondP1 = string.Empty;
                if (!string.IsNullOrEmpty(strTypeP1))
                    strCondP1 = string.IsNullOrEmpty(strCondP1) ?
                        $"mgl.TypeP1 in {GeneralFun.ConvertToStringScope(strTypeP1, ',')}" :
                        $"{strCondP1} AND mgl.TypeP1 in {GeneralFun.ConvertToStringScope(strTypeP1, ',')}";
                UpdateQueryResult();
            }
            else
                strCondP1 = string.Empty;
        }
        private void cbSizeP2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSizeP2.SelectedIndex >= 0 && (cbSizeP2.SelectedItem as HKLibGenOption)?.ID != string.Empty)
            {
                strCondP2 = $"mgl.SizeP2 = '{(cbSizeP2.SelectedItem as HKLibGenOption).ID}'";
                if (!string.IsNullOrEmpty(strTypeP2))
                    strCondP2 = $"{strCondP2} AND mgl.TypeP2 in {GeneralFun.ConvertToStringScope(strTypeP2, ',')}";
                UpdateQueryResult();
            }
            else if (cbSizeP2.SelectedIndex == 0)
            {
                //strCondP2 = $"mgl.SizeP2 IS NOT NULL AND LTRIM(RTRIM(mgl.SizeP2)) <> ''";
                strCondP2 = string.Empty;
                if (!string.IsNullOrEmpty(strTypeP2))
                    strCondP2 = string.IsNullOrEmpty(strCondP2) ?
                        $"mgl.TypeP2 in {GeneralFun.ConvertToStringScope(strTypeP2, ',')}" :
                        $"{strCondP2} AND mgl.TypeP2 in {GeneralFun.ConvertToStringScope(strTypeP2, ',')}";
                UpdateQueryResult();
            }
            else
                strCondP2 = string.Empty;
        }







        private void UpdateResultItem(HKMatGenLib item)
        {
            HKMatGenLib newItem = UpdateQueryResult(item.ID);
            if (newItem != null)
            {
                item.NameCn =  newItem.NameCn;
                item.NameEn = newItem.NameEn;
                item.SpecCombMain = newItem.SpecCombMain;
                item.SpecCombPort = newItem.SpecCombPort;
                item.SpecCombAux = newItem.SpecCombAux;
                item.MoreSpecCn = newItem.MoreSpecCn;
                item.MoreSpecEn = newItem.MoreSpecEn;
                item.SpecMat = newItem.SpecMat;
                item.SpecPClass = newItem.SpecPClass;
                item.RemarksCn = newItem.RemarksCn;
                item.RemarksEn = newItem.RemarksEn;
                currMat.TypeP1 = newItem.TypeP1;
                currMat.SizeP1 = newItem.SizeP1;
                currMat.TypeP2 = newItem.TypeP2;
                currMat.SizeP2 = newItem.SizeP2;
                currMat.MatSpec = newItem.MatSpec;
                currMat.MoreSpecCn = newItem.MoreSpecCn;
                currMat.MoreSpecEn = newItem.MoreSpecEn;
                currMat.RemarksCn = newItem.RemarksCn;
                currMat.RemarksEn = newItem.RemarksEn;
                currMat.TechSpecMain = newItem.TechSpecMain;
                currMat.TechSpecAux = newItem.TechSpecAux;
                lstMainSpec = currMat.TechSpecMain.Split(',').ToList();
                lstAuxSpec = currMat.TechSpecAux.Split(',').ToList();
                currMat.AlterCode = newItem.AlterCode;
            }
        }
        private void ckAutoRef_Click(object sender, RoutedEventArgs e)
        {
            if (ckAutoRef.IsChecked == true)
                UpdateQueryResult();    
        }
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            for (int i = dgResult.SelectedItems.Count - 1; i >= 0; i--)
            {
                HKMatGenLib item = dgResult.SelectedItems[i] as HKMatGenLib;
                DataDel(item.ID);
                result.Remove(item);
            }
        }
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            int count = NewDataAdd();
            if (count > 0)
            {
                MessageBox.Show($"{count} 条记录已添加至材料库。");
                UpdateQueryResult();
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgResult.SelectedItems.Count != 1)
                MessageBox.Show($"必须在材料库查询结果中选中唯一的材料。");

            else
            {
                HKMatGenLib item = dgResult.SelectedItems[0] as HKMatGenLib;
                int count = DataUpdate(item.ID);
                if (count > 0)
                    MessageBox.Show($"{count} 条记录已更新至材料库。");
                //(dgResult.SelectedItems[0] as HKMatGenLib).NameCn = "测试1";
                UpdateResultItem((dgResult.SelectedItems[0] as HKMatGenLib));
            }
        }
        private void dgResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgResult.SelectedItems.Count != 1) return;
            HKMatGenLib item = dgResult.SelectedItems[0] as HKMatGenLib;
            currMat = item;
            ResetMatGUI(item);
        }
        private void ResetMatGUI(HKMatGenLib itemMat)
        {
            if (itemMat == null) return;
            //设定大类
            ObservableCollection<HKMatMainCat> mainCats = cbMainCat.ItemsSource as ObservableCollection<HKMatMainCat>;
            if (mainCats != null && mainCats.Count > 0)
            {
                for (int i = 0; i < mainCats.Count; i++)
                {
                    if (mainCats[i].ID == itemMat.CatID)
                    {
                        cbMainCat.SelectedIndex = i;
                        break;
                    }
                }
            }
            // 设定小类
            ObservableCollection<HKMatSubCat> subCats = cbSubCat.ItemsSource as ObservableCollection<HKMatSubCat>;
            if (subCats != null && subCats.Count > 0)
            {
                for (int i = 0; i < subCats.Count; i++)
                {
                    if (subCats[i].ID == itemMat.SubCatID)
                    {
                        cbSubCat.SelectedIndex = i;
                        break;
                    }
                }
            }
            //设定主参数
            cbMainSpecT1.SelectedIndex = (cbMainSpecT1.Items.Count > 0) ? 0 : -1;
            cbMainSpecT2.SelectedIndex = (cbMainSpecT2.Items.Count > 0) ? 0 : -1;
            cbMainSpecT3.SelectedIndex = (cbMainSpecT3.Items.Count > 0) ? 0 : -1;
            if (!string.IsNullOrEmpty(itemMat.TechSpecMain))
            {
                // 预先分割规格字符串
                string[] techSpecParts = itemMat.TechSpecMain.Split(',');

                // 最多处理3个规格
                int maxSpecs = Math.Min(lstMainSpec.Count, 3);
                for (int i = 0; i < maxSpecs; i++)
                {
                    //string searchKey = lstMainSpec[i].Split(':')[0];
                    string searchKey = GetCmbMainSpecTComb(i);   
                    string seg = null;

                    // 查找匹配的规格段
                    foreach (string part in techSpecParts)
                    {
                        string[] partSegments = part.Split(':');
                        if (partSegments.Length > 0 && searchKey.Contains(partSegments[0]))
                        {
                            seg = part;
                            // 解析规格段
                            string[] segParts = seg.Split(':');
                            string specT = segParts[0];
                            string specV = segParts[1];

                            // 设置组合框选择
                            SetCmbMainSpecSelection(i, specT, specV);
                            break;
                        }
                    }

                    // 处理未找到的情况
                    if (seg == null)
                    {
                        SetCmbMainSpecTLastIndex(i);
                        break;
                    }
                }
            }
            //设定副参数
            cbAuxSpecT1.SelectedIndex = (cbAuxSpecT1.Items.Count > 0) ? 0 : -1;
            cbAuxSpecT2.SelectedIndex = (cbAuxSpecT2.Items.Count > 0) ? 0 : -1;
            cbAuxSpecT3.SelectedIndex = (cbAuxSpecT3.Items.Count > 0) ? 0 : -1;
            if (!string.IsNullOrEmpty(itemMat.TechSpecAux))
            {
                // 预先分割规格字符串
                string[] techSpecParts = itemMat.TechSpecAux.Split(',');

                // 最多处理3个规格
                int maxSpecs = Math.Min(lstAuxSpec.Count, 3);
                for (int i = 0; i < maxSpecs; i++)
                {
                    //string searchKey = lstAuxSpec[i].Split(':')[0];
                    string searchKey = GetCmbAuxSpecTComb(i);
                    string seg = null;

                    // 查找匹配的规格段
                    foreach (string part in techSpecParts)
                    {
                        string[] partSegments = part.Split(':');
                        if (partSegments.Length > 0 && searchKey.Contains(partSegments[0]))
                        {
                            seg = part;
                            // 解析规格段
                            string[] segParts = seg.Split(':');
                            string specT = segParts[0];
                            string specV = segParts[1];

                            // 设置组合框选择
                            SetCmbAuxSpecSelection(i, specT, specV);
                            break;
                        }
                    }

                    // 处理未找到的情况
                    if (seg == null)
                    {
                        SetCmbAuxSpecTLastIndex(i);
                        break;
                    }
                }
            }
            // 设定端口
            ObservableCollection<HKLibSpecDic> typeP1 = cbTypeP1.ItemsSource as ObservableCollection<HKLibSpecDic>;
            if (typeP1 != null && typeP1.Count > 0)
            {
                for (int i = 0; i < typeP1.Count; i++)
                {
                    if (typeP1[i].ID == itemMat.TypeP1)
                    {
                        cbTypeP1.SelectedIndex = i;
                        break;
                    }
                }
            }
            ObservableCollection<HKLibGenOption> sizeP1 = cbSizeP1.ItemsSource as ObservableCollection<HKLibGenOption>;
            if (sizeP1 != null && sizeP1.Count > 0)
            {
                for (int i = 0; i < sizeP1.Count; i++)
                {
                    if (sizeP1[i].ID == itemMat.SizeP1)
                    {
                        cbSizeP1.SelectedIndex = i;
                        break;
                    }
                }
            }
            ObservableCollection<HKLibSpecDic> typeP2 = cbTypeP2.ItemsSource as ObservableCollection<HKLibSpecDic>;
            if (typeP2 != null && typeP2.Count > 0)
            {
                for (int i = 0; i < typeP2.Count; i++)
                {
                    if (typeP2[i].ID == itemMat.TypeP2)
                    {
                        cbTypeP2.SelectedIndex = i;
                        break;
                    }
                }
            }
            ObservableCollection<HKLibGenOption> sizeP2 = cbSizeP2.ItemsSource as ObservableCollection<HKLibGenOption>;
            if (sizeP2 != null && sizeP2.Count > 0)
            {
                for (int i = 0; i < sizeP2.Count; i++)
                {
                    if (sizeP2[i].ID == itemMat.SizeP2)
                    {
                        cbSizeP2.SelectedIndex = i;
                        break;
                    }
                }
            }
            // 设定TextBox
            tbMoreSpecCn.Text = itemMat.MoreSpecCn;
            tbMoreSpecEn.Text = itemMat.MoreSpecEn;
            tbRemarksCn.Text = itemMat.RemarksCn;
            tbRemarksEn.Text = itemMat.RemarksEn;
        }
        private string GetCmbMainSpecTComb(int index)
        {
            switch (index)
            {
                case 0:
                    return string.Join("|", (cbMainSpecT1.ItemsSource as ObservableCollection<HKLibSpecDic>).Select(x => x.ID).Where(x => x != "-"));
                case 1:
                    return string.Join("|", (cbMainSpecT2.ItemsSource as ObservableCollection<HKLibSpecDic>).Select(x => x.ID).Where(x => x != "-"));
                case 2:
                    return string.Join("|", (cbMainSpecT3.ItemsSource as ObservableCollection<HKLibSpecDic>).Select(x => x.ID).Where(x => x != "-"));
            }
            return null;
        }
        private string GetCmbAuxSpecTComb(int index)
        {
            switch (index)
            {
                case 0:
                    return string.Join("|", (cbAuxSpecT1.ItemsSource as ObservableCollection<HKLibSpecDic>).Select(x => x.ID).Where(x => x != "-"));
                case 1:
                    return string.Join("|", (cbAuxSpecT2.ItemsSource as ObservableCollection<HKLibSpecDic>).Select(x => x.ID).Where(x => x != "-"));
                case 2:
                    return string.Join("|", (cbAuxSpecT3.ItemsSource as ObservableCollection<HKLibSpecDic>).Select(x => x.ID).Where(x => x != "-"));
            }
            return null;
        }
        // 设置组合框到最后一项("-" 隐藏该下拉框）
        private void SetCmbMainSpecTLastIndex(int index)
        {
            switch (index)
            {
                case 0:
                    cbMainSpecT1.SelectedIndex = cbMainSpecT1.Items.Count - 1;
                    break;
                case 1:
                    cbMainSpecT2.SelectedIndex = cbMainSpecT2.Items.Count - 1;
                    break;
                case 2:
                    cbMainSpecT3.SelectedIndex = cbMainSpecT3.Items.Count - 1;
                    break;
            }
        }
        private void SetCmbAuxSpecTLastIndex(int index)
        {
            switch (index)
            {
                case 0:
                    cbAuxSpecT1.SelectedIndex = cbAuxSpecT1.Items.Count - 1;
                    break;
                case 1:
                    cbAuxSpecT2.SelectedIndex = cbAuxSpecT2.Items.Count - 1;
                    break;
                case 2:
                    cbAuxSpecT3.SelectedIndex = cbAuxSpecT3.Items.Count - 1;
                    break;
            }
        }
        // 设置ItemSource
        private void SetCmbMainSpecTSource(int index, ObservableCollection<HKLibSpecDic> items)
        {
            switch (index)
            {
                case 0:
                    cbMainSpecT1.ItemsSource = items;
                    if (items != null) cbMainSpecT1.SelectedIndex = 0;
                    break;
                case 1:
                    cbMainSpecT2.ItemsSource = items;
                    if (items != null) cbMainSpecT2.SelectedIndex = 0;
                    break;
                case 2:
                    cbMainSpecT3.ItemsSource = items;
                    if (items != null) cbMainSpecT2.SelectedIndex = 0;
                    break;
            }
        }
        private void SetCmbAuxSpecTSource(int index, ObservableCollection<HKLibSpecDic> items)
        {
            switch (index)
            {
                case 0:
                    cbAuxSpecT1.ItemsSource = items;
                    if (items != null) cbAuxSpecT1.SelectedIndex = 0;
                    break;
                case 1:
                    cbAuxSpecT2.ItemsSource = items;
                    if (items != null) cbAuxSpecT2.SelectedIndex = 0;
                    break;
                case 2:
                    cbAuxSpecT3.ItemsSource = items;
                    if (items != null) cbAuxSpecT2.SelectedIndex = 0;
                    break;
            }
        }
        private void SetCmbMainSpecSelection(int index, string specT, string specV)
        {
            ObservableCollection<HKLibSpecDic> specTCol = null;
            ObservableCollection<HKLibGenOption> specVCol = null;
            ComboBox cmbT = null;
            ComboBox cmbV = null;

            switch (index)
            {
                case 0:
                    cmbT = cbMainSpecT1;
                    cmbV = cbMainSpec1;
                    break;
                case 1:
                    cmbT = cbMainSpecT2;
                    cmbV = cbMainSpec2;
                    break;
                case 2:
                    cmbT = cbMainSpecT3;
                    cmbV = cbMainSpec3;
                    break;
            }
            specTCol = cmbT.ItemsSource as ObservableCollection<HKLibSpecDic>;
            specVCol = cmbV.ItemsSource as ObservableCollection<HKLibGenOption>;

            // 设置类型组合框
            for (int i = 0; i < specTCol.Count; i++)
            {
                if (specTCol[i].ID == specT)
                {
                    cmbT.SelectedIndex = i;
                    // 设置值组合框
                    // 处理可编辑下拉框，将现有选项加入字典源
                    if (cmbV != null && cmbV.IsEditable == true)
                    {
                        if (!dicNoLinkSpecStr[specT].Contains(specV))
                        {
                            HKLibGenOption newSpec = new HKLibGenOption
                            {
                                ID = specV,
                                NameCn = (intLan == 0) ? specV : null,
                                NameEn = (intLan != 0) ? specV : null,
                            };
                            dicNoLinkSpec[specT].Add(newSpec);
                            dicNoLinkSpecStr[specT].Add(specV);
                        }

                    }
                    for (int j = 0; j < specVCol.Count; j++)
                    {
                        if (specVCol[j].ID == specV)
                        {
                            cmbV.SelectedIndex = j;
                            break;
                        }
                    }
                    break;
                }
            }
        }
        private void SetCmbAuxSpecSelection(int index, string specT, string specV)
        {
            ObservableCollection<HKLibSpecDic> specTCol = null;
            ObservableCollection<HKLibGenOption> specVCol = null;
            ComboBox cmbT = null;
            ComboBox cmbV = null;

            switch (index)
            {
                case 0:
                    cmbT = cbAuxSpecT1;
                    cmbV = cbAuxSpec1;
                    break;
                case 1:
                    cmbT = cbAuxSpecT2;
                    cmbV = cbAuxSpec2;
                    break;
                case 2:
                    cmbT = cbAuxSpecT3;
                    cmbV = cbAuxSpec3;
                    break;
            }
            specTCol = cmbT.ItemsSource as ObservableCollection<HKLibSpecDic>;
            specVCol = cmbV.ItemsSource as ObservableCollection<HKLibGenOption>;

            // 设置类型组合框
            for (int i = 0; i < specTCol.Count; i++)
            {
                if (specTCol[i].ID == specT)
                {
                    cmbT.SelectedIndex = i;
                    // 设置值组合框
                    // 处理可编辑下拉框，将现有选项加入字典源
                    if (cmbV != null && cmbV.IsEditable == true)
                    {
                        if (!dicNoLinkSpecStr[specT].Contains(specV))
                        {
                            HKLibGenOption newSpec = new HKLibGenOption
                            {
                                ID = specV,
                                NameCn = (intLan == 0) ? specV : null,
                                NameEn = (intLan != 0) ? specV : null,
                            };
                            dicNoLinkSpec[specT].Add(newSpec);
                            dicNoLinkSpecStr[specT].Add(specV);
                        }

                    }
                    for (int j = 0; j < specVCol.Count; j++)
                    {
                        if (specVCol[j].ID == specV)
                        {
                            cmbV.SelectedIndex = j;
                            break;
                        }
                    }
                    break;
                }
            }
        }
        private void SetCmbPortTypeSelection(int index, string type)
        {
            ObservableCollection<HKLibSpecDic> col = null;
            ComboBox cmb = null;
            switch (index)
            {
                case 0:
                    cmb = cbTypeP1;
                    break;
                case 1:
                    cmb = cbTypeP2;
                    break;
            }
            if (string.IsNullOrEmpty(type))
            {
                cmb.SelectedIndex = 0;
                return;
            }
            col = cmb.ItemsSource as ObservableCollection<HKLibSpecDic>;
            for (int i = 0; i < col.Count; i++)
            {
                if (col[i].ID == type)
                {
                    cmb.SelectedIndex = i;
                    return;
                }
            }
            cmb.SelectedIndex = 0;
        }
        private void SetCmbPortSizeSelection(int index, string size)
        {
            ObservableCollection<HKLibSpecDic> col = null;
            ComboBox cmb = null;
            switch (index)
            {
                case 0:
                    cmb = cbSizeP1;
                    break;
                case 1:
                    cmb = cbSizeP2;
                    break;
            }
            if (string.IsNullOrEmpty(size))
            {
                cmb.SelectedIndex = 0;
                return;
            }
            col = cmb.ItemsSource as ObservableCollection<HKLibSpecDic>;
            for (int i = 0; i < col.Count; i++)
            {
                if (col[i].ID == size)
                {
                    cmb.SelectedIndex = i;
                    return;
                }
            }
            cmb.SelectedIndex = 0;
        }
        //private string SetCmbAuxSpecSelection(int index, string specT, string specV)
        //{
        //    string segment = null;
        //    ComboBox cbType = null;
        //    ComboBox cbValue = null;
        //    ObservableCollection<HKLibSpecDic> typeList = null;
        //    ObservableCollection<HKLibGenOption> valueList = null;
        //    ComboBox cmbT = null;
        //    ComboBox cmbV = null;

        //    switch (index)
        //    {
        //        case 0:
        //            cbType = cbAuxSpecT1;
        //            cbValue = cbAuxSpec1;
        //            cmbT = cbAuxSpecT1;
        //            cmbV = cbAuxSpec1;
        //            break;
        //        case 1:
        //            cbType = cbAuxSpecT2;
        //            cbValue = cbAuxSpec2;
        //            cmbT = cbAuxSpecT2;
        //            cmbV = cbAuxSpec2;
        //            break;
        //        case 2:
        //            cbType = cbAuxSpecT3;
        //            cbValue = cbAuxSpec3;
        //            cmbT = cbAuxSpecT3;
        //            cmbV = cbAuxSpec3;
        //            break;
        //    }
        //    typeList = cmbT.ItemsSource as ObservableCollection<HKLibSpecDic>;
        //    valueList = cmbV.ItemsSource as ObservableCollection<HKLibGenOption>;

        //    // 设置类型组合框
        //    for (int j = 0; j < typeList.Count; j++)
        //    {
        //        if (typeList[j].ID == specT)
        //        {
        //            cbType.SelectedIndex = j;
        //            segment = specT;
        //            break;
        //        }
        //    }

        //    // 设置值组合框
        //    if (segment != null)
        //    {
        //        if (cmbV != null && cmbV.IsEditable == true)
        //        {
        //            if (!dicNoLinkSpecStr[specT].Contains(specV))
        //            {
        //                HKLibGenOption newSpec = new HKLibGenOption
        //                {
        //                    ID = specV,
        //                    NameCn = (intLan == 0) ? specV : null,
        //                    NameEn = (intLan != 0) ? specV : null,
        //                };
        //                dicNoLinkSpec[specT].Add(newSpec);
        //                dicNoLinkSpecStr[specT].Add(specV);
        //            }

        //        }
        //        for (int j = 0; j < valueList.Count; j++)
        //        {
        //            if (valueList[j].ID == specV)
        //            {
        //                cbValue.SelectedIndex = j;
        //                break;
        //            }
        //        }
        //    }
        //    return segment;
        //}
        private void btnToBeChk_Click(object sender, RoutedEventArgs e)
        {
            //string test = GeneralFun.ConvertToSqlString("Class:<: NPTM | NPTF");
            //string test = GeneralFun.ParseLinkExp("LibThread,,Class:IN:NPTM|NPSC");
            //var t1 = GeneralFun.ParseNumber("12345.0");
            string test1 = getSpecMainAux("DN:,TTT:PN16");
            UpdateQueryResult(true);

            //int test = GetNewID();
            //NewDataAdd();

            //SetDicMatGen();
            //string test1 = SetSpecMainAux("DN:0080,PN:PN16");
            //string test2 = SetSpecPort("W", "", "W", "0050", "DF1");

        }

    }
}
