using iEngr.Hookup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
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
    public partial class HK_Mat_Main : UserControl
    {
        public static int intLan;
        private static OdbcConnection conn;
        private ObservableCollection<HKMatGenLib> result = new ObservableCollection<HKMatGenLib>();

        //private ObservableCollection<HKMatMainCat> hKMatMainCats;
        //private ObservableCollection<HKLibThread> hKLibThreads;
        private ObservableCollection<HKMatSubCat> hKMatSubCats = new ObservableCollection<HKMatSubCat>
            {
                new HKMatSubCat
                {
                ID = string.Empty,
                SpecCn = "所有小类",
                SpecEn = "All Sub Categories"
                }
            };
        private ObservableCollection<HKLibPortType> hKPortTypesP1 = new ObservableCollection<HKLibPortType>
            {
                new HKLibPortType
                {
                ID =  string.Empty,
                NameCn = "所有连接类型",
                NameEn = "All Connections"
                }
            };
        private ObservableCollection<HKLibPortType> hKPortTypesP2 = new ObservableCollection<HKLibPortType>
            {
                new HKLibPortType
                {
                ID =  string.Empty,
                NameCn = "所有连接类型",
                NameEn = "All Connections"
                }
            };
        private object hKPortSizeP1, hKPortSizeP2;
        private string[] portDef = { "EQ1", "DF1", "AS1", "NEQ" };
        private Dictionary<string, ObservableCollection<string>> dicNoLinkSpecStr = new Dictionary<string, ObservableCollection<string>>();
        private Dictionary<string, ObservableCollection<HKLibGenOption>> dicNoLinkSpec = new Dictionary<string, ObservableCollection<HKLibGenOption>>();
        private string strTypeP1All, strTypeP2All, strTypeP1, strTypeP2, strCondP1, strCondP2, strCondSubCat;
        private string strSpecMainAll, strSpecMain, strSpecAuxAll, strSpecAux;
        private List<string> lstMainSpec = new List<string>();
        private List<string> lstAuxSpec = new List<string>();
        //private Dictionary<string, Dictionary<string, HKLibSpecDic>> dicMatGen1 = new Dictionary<string, Dictionary<string, HKLibSpecDic>>();
        //private Dictionary<string, Dictionary<string, HKLibGenOption>> dicMatGen2 = new Dictionary<string, Dictionary<string, HKLibGenOption>>();
        int? nullInt = null;
        decimal? nullDecimal = null;
        Dictionary<string, HKLibPortType> dicPortType = new Dictionary<string, HKLibPortType>();
        Dictionary<string, HKLibGenOption> dicGenOption = new Dictionary<string, HKLibGenOption>();
        Dictionary<string, HKLibGland> dicGland = new Dictionary<string, HKLibGland>();
        Dictionary<string, HKLibPipeOD> dicPipeOD = new Dictionary<string, HKLibPipeOD>();
        Dictionary<string, HKLibPN> dicPN = new Dictionary<string, HKLibPN>();
        Dictionary<string, HKLibSpecDic> dicSpecDic = new Dictionary<string, HKLibSpecDic>();
        Dictionary<string, HKLibSteel> dicSteel = new Dictionary<string, HKLibSteel>();
        Dictionary<string, HKLibThread> dicThread = new Dictionary<string, HKLibThread>();
        Dictionary<string, HKLibTubeOD> dicTubeOD = new Dictionary<string, HKLibTubeOD>();

        public HK_Mat_Main()
        {
            InitializeComponent();
            intLan = 0; // 0: 中文； 其它为英文
            conn = GetConnection();
            dgResult.ItemsSource = result;
            //hKMatMainCats = GetHKMatMainCats();
            cbMainCat.ItemsSource = GetHKMatMainCats();
            cbSubCat.ItemsSource = hKMatSubCats;
            cbMainCat.SelectedIndex = 0;
            SetDicMatGen();

            //hKLibThreads = GetLibThread();
            //dgResult.ItemsSource = hKLibThreads;
        }


        private static OdbcConnection GetConnection()
        {
            try
            {
                // 定义 DSN 名称
                string dsnName = "ComosExt";

                // 创建 OdbcConnection 对象并传入 DSN 连接字符串
                OdbcConnection connection = new OdbcConnection($"DSN={dsnName};UID=COMOSSH;Pwd=comos#321");

                // 打开数据库连接
                connection.Open();

                // 返回连接对象
                return connection;
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                return null;
            }
        }
        private ObservableCollection<HKMatMainCat> GetHKMatMainCats()
        {
            ObservableCollection<HKMatMainCat> mainCats = new ObservableCollection<HKMatMainCat>
            {
                new HKMatMainCat
                {
                ID =  "%",
                NameCn = "所有大类",
                NameEn = "All Main Categories"
                }
            };
            // 构建 SQL 查询语句
            string query = "select * from HK_MatMainCat order by SortNum ";
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HKMatMainCat mainCat = new HKMatMainCat
                    {
                        ID = Convert.ToString(reader["ID"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        Remarks = Convert.ToString(reader["Remarks"])
                    };
                    mainCats.Add(mainCat);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return mainCats;
        }
        private ObservableCollection<HKMatSubCat> GetHKMatSubCats(HKMatMainCat mainCat)
        {
            ObservableCollection<HKMatSubCat> subCats = hKMatSubCats;
            strTypeP1All = string.Empty;
            strTypeP2All = string.Empty;
            strSpecMainAll = string.Empty;
            strSpecAuxAll = string.Empty;
            subCats.Clear();
            subCats.Add(
                new HKMatSubCat
                {
                    ID = string.Empty,
                    SpecCn = mainCat.ID == "%" ? "所有小类" : "所有" + mainCat.Name,
                    SpecEn = mainCat.ID == "%" ? "All Sub Categories" : "All " + mainCat.Name
                });

            // 构建 SQL 查询语句
            string query = "select * from HK_MatSubCat where CatID like '" + mainCat.ID + "' order by SortNum ";
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HKMatSubCat subCat = new HKMatSubCat
                    {
                        ID = Convert.ToString(reader["ID"]),
                        CatID = Convert.ToString(reader["CatID"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Remarks = Convert.ToString(reader["Remarks"]),
                        TypeP1 = Convert.ToString(reader["TypeP1"])?.Trim(),
                        TypeP2 = Convert.ToString(reader["TypeP2"])?.Trim(),
                        TechSpecMain = Convert.ToString(reader["TechSpecMain"])?.Trim(),
                        TechSpecAux = Convert.ToString(reader["TechSpecAux"])?.Trim()
                    };
                    strTypeP1All = (strTypeP1All.Contains(subCat.TypeP1 + ",") || subCat.TypeP1 == "NA" || subCat.TypeP1 == "IS") ? strTypeP1All 
                        : strTypeP1All + subCat.TypeP1 + ",";
                    strTypeP2All = (strTypeP2All.Contains(((portDef.Contains(subCat.TypeP2)) ? subCat.TypeP1 : subCat.TypeP2) + ",") || subCat.TypeP2 == "NA" || subCat.TypeP2 == "IS") ? strTypeP2All 
                        : strTypeP2All + ((portDef.Contains(subCat.TypeP2)) ? subCat.TypeP1: subCat.TypeP2) + ",";
                    strSpecMainAll = strSpecMainAll.Contains(subCat.TechSpecMain + ",") ? strSpecMainAll : strSpecMainAll + subCat.TechSpecMain + ",";
                    strSpecAuxAll = strSpecAuxAll.Contains(subCat.TechSpecAux + ",") ? strSpecAuxAll : strSpecAuxAll + subCat.TechSpecAux + ",";
                    subCats.Add(subCat);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
             return subCats;
        }
        private ObservableCollection<HKLibSpecDic> GetHKPortTypes(string strTypes)
        {
            ObservableCollection<HKLibSpecDic> portTypes = new ObservableCollection<HKLibSpecDic>
            {
                new HKLibSpecDic
                {
                    ID =  string.Empty,
                    NameCn = "选择连接类型",
                    NameEn = "Select Conn.Type"
                }
            };

            // 构建 SQL 查询语句
            string query = "select * from HK_LibPortType where SortNum < 101 and ID in " + GeneralFun.ConvertToStringScope(strTypes, ',') + " order by SortNum";
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HKLibSpecDic portType = new HKLibSpecDic
                    {
                        ID = Convert.ToString(reader["ID"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        PrefixCn = Convert.ToString(reader["PrefixCn"]),
                        PrefixEn = Convert.ToString(reader["PrefixEn"]),
                        SuffixCn = Convert.ToString(reader["SuffixCn"]),
                        SuffixEn = Convert.ToString(reader["SuffixEn"]),
                        Class = string.IsNullOrEmpty(Convert.ToString(reader["Link"]))?
                                Convert.ToString(reader["Remarks"]) :
                                "Link",
                        Link = Convert.ToString(reader["Link"])
                    };
                    portTypes.Add(portType);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return portTypes;
        }

        //private ObservableCollection<HKLibPortType> GetHKPortTypes(string strTypes, int intPort)
        //{
        //    ObservableCollection<HKLibPortType> portTypes = (intPort == 1) ? hKPortTypesP1 : hKPortTypesP2;
        //    portTypes.Clear();
        //    portTypes.Add(
        //        new HKLibPortType
        //        {
        //            ID = "%",
        //            NameCn = "选择连接类型",
        //            NameEn = "Select Conn.Type"
        //        }
        //    );

        //    // 构建 SQL 查询语句
        //    string query = "select * from HK_LibPortType where SortNum < 101 and ID in " + GeneralFun.ConvertToStringScope(strTypes, ',') + " order by SortNum";
        //    try
        //    {
        //        if (conn == null || conn.State != ConnectionState.Open)
        //            conn = GetConnection();
        //        OdbcCommand command = new OdbcCommand(query, conn);
        //        OdbcDataReader reader = command.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            HKLibPortType portType = new HKLibPortType
        //            {
        //                ID = Convert.ToString(reader["ID"]),
        //                NameCn = Convert.ToString(reader["NameCn"]),
        //                NameEn = Convert.ToString(reader["NameEn"]),
        //                PrefixCn = Convert.ToString(reader["PrefixCn"]),
        //                PrefixEn = Convert.ToString(reader["PrefixEn"]),
        //                SuffixCn = Convert.ToString(reader["SuffixCn"]),
        //                SuffixEn = Convert.ToString(reader["SuffixEn"]),
        //                Remarks = Convert.ToString(reader["Remarks"]),
        //                Link = Convert.ToString(reader["Link"])
        //            };
        //            portTypes.Add(portType);
        //        }

        //        reader.Close();
        //        if (portTypes.Count == 1 || portTypes.Count == 2 && (portTypes.ElementAt(1).ID == "NA" || portTypes.ElementAt(1).ID == "IS"))
        //        {
        //            portTypes.Clear();
        //            portTypes.Add(
        //                new HKLibPortType
        //                {
        //                    ID = "%",
        //                    NameCn = "无",
        //                    NameEn = "NA"
        //                }
        //            );
        //            if (intPort == 1)
        //            {
        //                lbPort1.Visibility = Visibility.Collapsed;
        //                wpPort1.Visibility = Visibility.Collapsed;

        //            }
        //            else if (intPort == 2)
        //            {
        //                lbPort2.Visibility = Visibility.Collapsed;
        //                wpPort2.Visibility = Visibility.Collapsed;
        //            }
        //        }
        //        else
        //        {
        //            if (intPort == 1)
        //            {
        //                lbPort1.Visibility = Visibility.Visible;
        //                wpPort1.Visibility = Visibility.Visible;

        //            }
        //            else if (intPort == 2)
        //            {
        //                lbPort2.Visibility = Visibility.Visible;
        //                wpPort2.Visibility = Visibility.Visible;
        //            }
        //        }

        //        //if (portTypes.Count == 2 && portTypes.ElementAt(1).ID == "IS")
        //        //{
        //        //    portTypes.RemoveAt(0);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        // 处理异常
        //        MessageBox.Show($"Error: {ex.Message}");
        //        // 可以选择返回空列表或者其他适当的处理
        //    }
        //    return portTypes;
        //}
        private Object GetHKPortSpecs(int intPort)
        {
            HKLibPortType typePS = cbTypeP1.SelectedItem as HKLibPortType;
            if (intPort == 2)
                typePS = cbTypeP2.SelectedItem as HKLibPortType;
            if ((typePS != null) && (typePS.ID != string.Empty))
            {
                if (typePS.Link.StartsWith("LibThread"))
                {
                    ObservableCollection<HKLibThread> hKLibThreads = new ObservableCollection<HKLibThread>()
                    {
                        new HKLibThread
                        {
                            ID= string.Empty,
                            SpecCn = "选择螺纹规格",
                            SpecEn = "All Thread Typies"
                        }
                    };
                    // 构建 SQL 查询语句
                    string query = GeneralFun.ParseLinkExp(typePS.Link);
                    try
                    {
                        if (conn == null || conn.State != ConnectionState.Open)
                            conn = GetConnection();
                        OdbcCommand command = new OdbcCommand(query, conn);
                        OdbcDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            HKLibThread hKLibThread = new HKLibThread
                            {
                                ID = Convert.ToString(reader["ID"]),
                                SpecCn = Convert.ToString(reader["SpecCn"]),
                                SpecEn = Convert.ToString(reader["SpecEn"])
                            };
                            hKLibThreads.Add(hKLibThread);
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        MessageBox.Show($"Error: {ex.Message}");
                        // 可以选择返回空列表或者其他适当的处理
                    }
                    return hKLibThreads;
                }
                else if (typePS.Link.StartsWith("LibTubeOD"))
                {
                    ObservableCollection<HKLibTubeOD> hKLibTubeODs = new ObservableCollection<HKLibTubeOD>()
                    {
                        new HKLibTubeOD
                        {
                            ID= string.Empty,
                            SpecCn = "选择Tube管外径",
                            SpecEn = "All Tube O.D."
                        }
                    };
                    // 构建 SQL 查询语句
                    string query = GeneralFun.ParseLinkExp(typePS.Link);
                    try
                    {
                        if (conn == null || conn.State != ConnectionState.Open)
                            conn = GetConnection();
                        OdbcCommand command = new OdbcCommand(query, conn);
                        OdbcDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            HKLibTubeOD hKLibTubeOD = new HKLibTubeOD
                            {
                                ID = Convert.ToString(reader["ID"]),
                                SpecCn = Convert.ToString(reader["SpecCn"]),
                                SpecEn = Convert.ToString(reader["SpecEn"])
                            };
                            hKLibTubeODs.Add(hKLibTubeOD);
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        MessageBox.Show($"Error: {ex.Message}");
                        // 可以选择返回空列表或者其他适当的处理
                    }
                    return hKLibTubeODs;
                }
                else if (typePS.Link.StartsWith("LibPipeOD"))
                {
                    ObservableCollection<HKLibPipeOD> hKLibPipeODs = new ObservableCollection<HKLibPipeOD>()
                    {
                        new HKLibPipeOD
                        {
                            ID= string.Empty,
                            NameCn = "选择公称直径",
                            NameEn = "All DN/NPS"
                        }
                    };
                    // 构建 SQL 查询语句
                    string query = GeneralFun.ParseLinkExp(typePS.Link);
                    try
                    {
                        if (conn == null || conn.State != ConnectionState.Open)
                            conn = GetConnection();
                        OdbcCommand command = new OdbcCommand(query, conn);
                        OdbcDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            HKLibPipeOD hKLibPipeOD = new HKLibPipeOD
                            {
                                ID = Convert.ToString(reader["ID"]),
                                NameCn = typePS.ID.Contains("NPS")?
                                         $"NPS {Convert.ToString(reader["NPS"])} / DN {Convert.ToString(reader["DN"])}" :
                                         $"DN {Convert.ToString(reader["DN"])} / NPS {Convert.ToString(reader["NPS"])}",
                                NameEn = typePS.ID.Contains("NPS") ?
                                         $"NPS {Convert.ToString(reader["NPS"])} / DN {Convert.ToString(reader["DN"])}" :
                                         $"DN {Convert.ToString(reader["DN"])} / NPS {Convert.ToString(reader["NPS"])}",
                            };
                            hKLibPipeODs.Add(hKLibPipeOD);
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        MessageBox.Show($"Error: {ex.Message}");
                        // 可以选择返回空列表或者其他适当的处理
                    }
                    return hKLibPipeODs;
                }

            }

            return null;

        }


        private void cbMainCat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string catID = (cbMainCat.SelectedItem as HKMatMainCat).ID == "--" ? "%" : (cbMainCat.SelectedItem as HKMatMainCat).ID;
            //if (catID == null) { return; }
            //catID = catID?.Length < 2 ? catID : catID?.Substring(0, 2);
            //catID = catID=="--"? "%" : catID;
            hKMatSubCats = GetHKMatSubCats(cbMainCat.SelectedItem as HKMatMainCat);
            //HMIMain.clsSubCat = hKMatSubCats.ElementAt(3);
            cbSubCat.SelectedIndex = 0;
            //cbSubCat.SelectedItem = HMIMain.clsSubCat;
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
            }
            else if ((cbSubCat.SelectedItem as HKMatSubCat).ID == string.Empty)
            {
                strTypeP1 = strTypeP1All;
                strTypeP2 = strTypeP2All;
                strSpecMain = strSpecMainAll;
                strSpecAux = strSpecAuxAll;
            }
            else
            {
                strTypeP1 = selectedItem.TypeP1?.Trim();
                strTypeP1 = (strTypeP1 == "NA" || strTypeP1 == "IS") ? string.Empty : strTypeP1;
                strTypeP2 = selectedItem.TypeP2?.Trim();
                strTypeP2 = (strTypeP2 == "NA" || strTypeP2 == "IS") ? string.Empty : strTypeP2;
                strTypeP2 = (portDef.Contains(strTypeP2)) ? strTypeP1 : strTypeP2;
                strTypeP1All = strTypeP1;
                strTypeP2All = strTypeP2;

                strSpecMain = selectedItem.TechSpecMain?.Trim();
                strSpecAux = selectedItem.TechSpecAux?.Trim();
                strCondP1 = $"mgl.TypeP1 in {GeneralFun.ConvertToStringScope(strTypeP1All, ',')}";
                strCondP2 = $"mgl.TypeP2 in {GeneralFun.ConvertToStringScope(strTypeP2All, ',')}";
                strCondSubCat = $"mgl.SubCatID in {GeneralFun.ConvertToStringScope(selectedItem.ID, ',')}";



            }

            // 处理主参数 TechSpecMain
            var lstSpecMain = strSpecMain.Split(',')
                         .Select(item => item.Trim())
                         .Where(item => !string.IsNullOrWhiteSpace(item))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToList();
            cbMainSpecT1.ItemsSource = null;
            cbMainSpec1.ItemsSource = null;
            cbMainSpecT2.ItemsSource = null;
            cbMainSpec2.ItemsSource = null;
            cbMainSpecT3.ItemsSource = null;
            cbMainSpec3.ItemsSource = null;
            cbMainSpecT1.Visibility = Visibility.Collapsed;
            cbMainSpec1.Visibility = Visibility.Collapsed;
            cbMainSpecT2.Visibility = Visibility.Collapsed;
            cbMainSpec2.Visibility = Visibility.Collapsed;
            cbMainSpecT3.Visibility = Visibility.Collapsed;
            cbMainSpec3.Visibility = Visibility.Collapsed;
            lbMainSpec.Visibility = Visibility.Collapsed;
            if (lstSpecMain?.Count() > 0 && !string.IsNullOrEmpty(lstSpecMain[0]))
            {
                cbMainSpecT1.ItemsSource = GetLibSpecDic(lstSpecMain[0]);
                cbMainSpecT1.SelectedIndex = 0;
                cbMainSpecT1.Visibility = Visibility.Visible;
                cbMainSpec1.Visibility = Visibility.Visible;
                lbMainSpec.Visibility = Visibility.Visible;
                if (lstSpecMain?.Count() > 1 && !string.IsNullOrEmpty(lstSpecMain[1]))
                {
                    cbMainSpecT2.ItemsSource = GetLibSpecDic(lstSpecMain[1]);
                    cbMainSpecT2.SelectedIndex = 0;
                    cbMainSpecT2.Visibility = Visibility.Visible;
                    cbMainSpec2.Visibility = Visibility.Visible;
                    if (lstSpecMain?.Count() > 2 && !string.IsNullOrEmpty(lstSpecMain[2]))
                    {
                        cbMainSpecT3.ItemsSource = GetLibSpecDic(lstSpecMain[2]);
                        cbMainSpecT3.SelectedIndex = 0;
                        cbMainSpecT3.Visibility = Visibility.Visible;
                        cbMainSpec3.Visibility = Visibility.Visible;

                    }
                }
            }



            // 处理主参数 TechSpecAux
            var lstSpecAux = strSpecAux.Split(',')
                         .Select(item => item.Trim())
                         .Where(item => !string.IsNullOrWhiteSpace(item))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToList();
            cbAuxSpecT1.ItemsSource = null;
            cbAuxSpec1.ItemsSource = null;
            cbAuxSpecT2.ItemsSource = null;
            cbAuxSpec2.ItemsSource = null;
            cbAuxSpecT3.ItemsSource = null;
            cbAuxSpec3.ItemsSource = null;
            cbAuxSpecT1.Visibility = Visibility.Collapsed;
            cbAuxSpec1.Visibility = Visibility.Collapsed;
            cbAuxSpecT2.Visibility = Visibility.Collapsed;
            cbAuxSpec2.Visibility = Visibility.Collapsed;
            cbAuxSpecT3.Visibility = Visibility.Collapsed;
            cbAuxSpec3.Visibility = Visibility.Collapsed;
            lbAuxSpec.Visibility = Visibility.Collapsed;
            if (lstSpecAux?.Count() > 0 && !string.IsNullOrEmpty(lstSpecAux[0]))
            {
                cbAuxSpecT1.ItemsSource = GetLibSpecDic(lstSpecAux[0]);
                cbAuxSpecT1.SelectedIndex = 0;
                cbAuxSpecT1.Visibility = Visibility.Visible;
                cbAuxSpec1.Visibility = Visibility.Visible;
                lbAuxSpec.Visibility = Visibility.Visible;
                if (lstSpecAux?.Count() > 1 && !string.IsNullOrEmpty(lstSpecAux[1]))
                {
                    cbAuxSpecT2.ItemsSource = GetLibSpecDic(lstSpecAux[1]);
                    cbAuxSpecT2.SelectedIndex = 0;
                    cbAuxSpecT2.Visibility = Visibility.Visible;
                    cbAuxSpec2.Visibility = Visibility.Visible;
                    if (lstSpecAux?.Count() > 2 && !string.IsNullOrEmpty(lstSpecAux[2]))
                    {
                        cbAuxSpecT3.ItemsSource = GetLibSpecDic(lstSpecAux[2]);
                        cbAuxSpecT3.SelectedIndex = 0;
                        cbAuxSpecT3.Visibility = Visibility.Visible;
                        cbAuxSpec3.Visibility = Visibility.Visible;
                    }
                }
            }

            // 处理端口一、二
            if (string.IsNullOrEmpty(strTypeP1) || strTypeP1 == "NA" || strTypeP1 == "IS" || strTypeP1 == "NA," || strTypeP1 == "IS,")
            {
                cbTypeP1.ItemsSource = null;
                lbPort1.Visibility = Visibility.Collapsed;
                wpPort1.Visibility = Visibility.Collapsed;
            }
            else
            {
                cbTypeP1.ItemsSource = GetHKPortTypes(strTypeP1);
                cbTypeP1.SelectedIndex = 0;
                lbPort1.Visibility = Visibility.Visible;
                wpPort1.Visibility = Visibility.Visible;
            }
            if (string.IsNullOrEmpty(strTypeP2) || strTypeP2 == "NA" || strTypeP2 == "IS" || strTypeP2 == "NA," || strTypeP2 == "IS,")
            {
                cbTypeP2.ItemsSource = null;
                lbPort2.Visibility = Visibility.Collapsed;
                wpPort2.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (portDef.Contains(strTypeP2))
                    cbTypeP2.ItemsSource = GetHKPortTypes(strTypeP1);
                else
                    cbTypeP2.ItemsSource = GetHKPortTypes(strTypeP2);
                cbTypeP2.SelectedIndex = 0;
                lbPort2.Visibility = Visibility.Visible;
                wpPort2.Visibility = Visibility.Visible;
            }
            if (strTypeP2 == "AS1")
            {
                cbTypeP2.IsEnabled = false;
                cbSizeP2.IsEnabled = false;
            }
            else
            {
                cbTypeP2.IsEnabled = true;
                cbSizeP2.IsEnabled = true;
            }
        }
        //if (cbSubCat.SelectedItem != null && cbSubCat.SelectedIndex != 0)
        //{
        //    strTypeP1 = (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP1;
        //    strTypeP2 = (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2;
        //}
        //hKPortTypesP1 = GetHKPortTypes(strTypeP1, 1);
        //cbTypeP1.ItemsSource = hKPortTypesP1;
        //if (portDef.Contains(strTypeP2))
        //    hKPortTypesP2 = GetHKPortTypes(strTypeP1, 2);
        //else
        //    hKPortTypesP2 = GetHKPortTypes(strTypeP2, 2);

        //cbTypeP2.ItemsSource = hKPortTypesP2;
        //cbTypeP1.SelectedIndex = 0;
        //cbTypeP2.SelectedIndex = 0;

        //if (strTypeP2 == "AS1")
        //{
        //    cbTypeP2.IsEnabled = false;
        //    cbSizeP2.IsEnabled = false;
        //}
        //else
        //{
        //    cbTypeP2.IsEnabled = true;
        //    cbSizeP2.IsEnabled = true;
        //}

 
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
                cbMainSpecT1.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbMainSpecT1.Visibility;
                cbMainSpec1.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbMainSpec1.Visibility;
            }
            else if ((sender as ComboBox).Name == "cbMainSpecT2")
            {
                cbMainSpec2.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbMainSpec2.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbMainSpec2.IsEditable = selectedItem.Class?.ToUpper() != "LINK";
                cbMainSpec2.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbMainSpecT2.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbMainSpecT2.Visibility;
                cbMainSpec2.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbMainSpec2.Visibility;
            }
            else if ((sender as ComboBox).Name == "cbMainSpecT3")
            {
                cbMainSpec3.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbMainSpec3.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbMainSpec3.IsEditable = selectedItem.Class?.ToUpper() != "LINK";
                cbMainSpec3.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbMainSpecT3.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbMainSpecT3.Visibility;
                cbMainSpec3.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbMainSpec3.Visibility;
            }
            else if ((sender as ComboBox).Name == "cbAuxSpecT1")
            {
                cbAuxSpec1.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbAuxSpec1.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbAuxSpec1.IsEditable = selectedItem.Class?.ToUpper() != "LINK";
                cbAuxSpec1.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbAuxSpecT1.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpecT1.Visibility;
                cbAuxSpec1.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpec1.Visibility;
            }
            else if ((sender as ComboBox).Name == "cbAuxSpecT2")
            {
                cbAuxSpec2.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbAuxSpec2.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbAuxSpec2.IsEditable = (selectedItem.Class?.ToUpper() == "LINK") ? false : true;
                cbAuxSpec2.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbAuxSpecT2.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpecT2.Visibility;
                cbAuxSpec2.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpec2.Visibility;
            }
            else if ((sender as ComboBox).Name == "cbAuxSpecT3")
            {
                cbAuxSpec3.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbAuxSpec3.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbAuxSpec3.IsEditable = (selectedItem.Class?.ToUpper() == "LINK") ? false : true;
                cbAuxSpec3.MinWidth = (selectedItem.Class?.ToUpper() == "LINK") ? 40 : 120;
                cbAuxSpecT3.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpecT3.Visibility;
                cbAuxSpec3.Visibility = (selectedItem.ID == "-") ? Visibility.Collapsed : cbAuxSpec3.Visibility;
            }
        }



         private void cbMainSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstMainSpec.Clear();
            if ((cbMainSpecT1.SelectedItem as HKLibSpecDic) != null && (cbMainSpecT1.SelectedItem as HKLibSpecDic).ID != "-")
                lstMainSpec.Add($"{(cbMainSpecT1.SelectedItem as HKLibSpecDic).ID}:{(cbMainSpec1.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbMainSpecT2.SelectedItem as HKLibSpecDic) != null && (cbMainSpecT2.SelectedItem as HKLibSpecDic).ID != "-")
                lstMainSpec.Add($"{(cbMainSpecT2.SelectedItem as HKLibSpecDic).ID}:{(cbMainSpec2.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbMainSpecT3.SelectedItem as HKLibSpecDic) != null && (cbMainSpecT3.SelectedItem as HKLibSpecDic).ID != "-")
                lstMainSpec.Add($"{(cbMainSpecT3.SelectedItem as HKLibSpecDic).ID}:{(cbMainSpec3.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbMainSpec1.SelectedItem as HKLibGenOption) != null && (cbMainSpec1.SelectedItem as HKLibGenOption).ID != string.Empty
               || (cbMainSpec2.SelectedItem as HKLibGenOption) != null && (cbMainSpec2.SelectedItem as HKLibGenOption).ID != string.Empty
               || (cbMainSpec3.SelectedItem as HKLibGenOption) != null && (cbMainSpec3.SelectedItem as HKLibGenOption).ID != string.Empty )
                UpdateQueryResult();
        }
        private void cbAuxSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstAuxSpec.Clear();
            if ((cbAuxSpecT1.SelectedItem as HKLibSpecDic) != null && (cbAuxSpecT1.SelectedItem as HKLibSpecDic).ID != "-")
                lstAuxSpec.Add($"{(cbAuxSpecT1.SelectedItem as HKLibSpecDic).ID}:{(cbAuxSpec1.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbAuxSpecT2.SelectedItem as HKLibSpecDic) != null && (cbAuxSpecT2.SelectedItem as HKLibSpecDic).ID != "-")
                lstAuxSpec.Add($"{(cbAuxSpecT2.SelectedItem as HKLibSpecDic).ID}:{(cbAuxSpec2.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbAuxSpecT3.SelectedItem as HKLibSpecDic) != null && (cbAuxSpecT3.SelectedItem as HKLibSpecDic).ID != "-" )
                lstAuxSpec.Add($"{(cbAuxSpecT3.SelectedItem as HKLibSpecDic).ID}:{(cbAuxSpec3.SelectedItem as HKLibGenOption)?.ID}");
            if ((cbAuxSpec1.SelectedItem as HKLibGenOption) != null && (cbAuxSpec1.SelectedItem as HKLibGenOption).ID != string.Empty 
                || (cbAuxSpec2.SelectedItem as HKLibGenOption) != null && (cbAuxSpec2.SelectedItem as HKLibGenOption).ID != string.Empty 
                || (cbAuxSpec3.SelectedItem as HKLibGenOption) != null && (cbAuxSpec3.SelectedItem as HKLibGenOption).ID != string.Empty)
                UpdateQueryResult();
                
        }

        private void cbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HKLibSpecDic selectedItem = ((sender as ComboBox).SelectedItem as HKLibSpecDic);
            if (selectedItem == null) return;

            if ((sender as ComboBox).Name == "cbTypeP1")
            {

                strTypeP1 = selectedItem.ID;
                if (string.IsNullOrEmpty(strTypeP1) || strTypeP1 == "NA" || strTypeP1 == "IS" || strTypeP1 == "NA," || strTypeP1 == "IS,")
                    strTypeP1 = string.Empty;
                if (selectedItem.ID == string.Empty)
                    strTypeP1 = strTypeP1All;
                if (portDef.Contains((cbSubCat.SelectedItem as HKMatSubCat).TypeP2))
                {
                    cbTypeP2.ItemsSource = cbTypeP1.ItemsSource;
                    cbTypeP2.SelectedIndex = cbTypeP1.SelectedIndex;
                    //cbSizeP2.ItemsSource = cbSizeP1.ItemsSource;
                    //cbSizeP2.SelectedIndex = cbSizeP1.SelectedIndex;
                }
                cbSizeP1.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbSizeP1.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbSizeP1.IsEditable = selectedItem.Class != null && selectedItem.Class?.ToUpper() != "LINK";
            }
            else if ((sender as ComboBox).Name == "cbTypeP2")
            {
                strTypeP2 = selectedItem.ID;
                if (string.IsNullOrEmpty(strTypeP2) || strTypeP2 == "NA" || strTypeP2 == "IS" || strTypeP2 == "NA," || strTypeP2 == "IS,")
                    strTypeP2 = string.Empty;
                else if (portDef.Contains(strTypeP2))
                    strTypeP2 = strTypeP1;
                if (selectedItem.ID == string.Empty)
                    strTypeP2 = strTypeP2All;
                cbSizeP2.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbSizeP2.SelectedIndex = (selectedItem.Class?.ToUpper() == "LINK") ? 0 : -1;
                cbSizeP2.IsEditable = selectedItem.Class != null && selectedItem.Class?.ToUpper() != "LINK";
            }

            //if (cbTypeP1.SelectedItem != null && cbTypeP1.SelectedIndex != 0)
            //{
            //    string link = (cbTypeP1.SelectedItem as HKLibPortType)?.Link;
            //    if (link.StartsWith("LibThread"))
            //    {
            //        hKPortSizeP1 = GetHKPortSpecs(1);
            //        cbSizeP1.ItemsSource = hKPortSizeP1 as ObservableCollection<HKLibThread>;
            //        cbSizeP1.SelectedIndex = 0;
            //    }
            //    else if (link.StartsWith("LibTubeOD"))
            //    {
            //        hKPortSizeP1 = GetHKPortSpecs(1);
            //        cbSizeP1.ItemsSource = hKPortSizeP1 as ObservableCollection<HKLibTubeOD>;
            //        cbSizeP1.SelectedIndex = 0;
            //    }
            //    else if (link.StartsWith("LibPipeOD"))
            //    {
            //        hKPortSizeP1 = GetHKPortSpecs(1);
            //        cbSizeP1.ItemsSource = hKPortSizeP1 as ObservableCollection<HKLibPipeOD>;
            //        cbSizeP1.SelectedIndex = 0;
            //    }
            //    else
            //    {
            //        cbSizeP1.ItemsSource = null;
            //    }
            //}
            //else
            //{
            //    cbSizeP1.ItemsSource = null;
            //}

            //if ((cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2 == "AS1" || (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2 == "DF1")
            //{
            //    cbTypeP2.ItemsSource = hKPortTypesP2;
            //    cbTypeP2.SelectedIndex = cbTypeP1.SelectedIndex;
            //}
        }
        private void cbTypeP2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTypeP2.SelectedItem != null && cbTypeP2.SelectedIndex != 0)
            {
                string link = (cbTypeP2.SelectedItem as HKLibPortType)?.Link;
                if (link.StartsWith("LibThread"))
                {
                    hKPortSizeP2 = GetHKPortSpecs(2);
                    cbSizeP2.ItemsSource = hKPortSizeP2 as ObservableCollection<HKLibThread>;
                     cbSizeP2.SelectedIndex = 0;
                }
                else if (link.StartsWith("LibTubeOD"))
                {
                    hKPortSizeP2 = GetHKPortSpecs(2);
                    cbSizeP2.ItemsSource = hKPortSizeP2 as ObservableCollection<HKLibTubeOD>;
                   cbSizeP2.SelectedIndex = 0;
                }
                else if (link.StartsWith("LibPipeOD"))
                {
                    hKPortSizeP2 = GetHKPortSpecs(2);
                    cbSizeP2.ItemsSource = hKPortSizeP2 as ObservableCollection<HKLibPipeOD>;
                    cbSizeP2.SelectedIndex = 0;
                }
                else
                {
                    cbSizeP2.ItemsSource = null;
                }
            }
            else
            {
                cbSizeP2.ItemsSource = null;
            }

        }


        private void cbMainSpec_TextChanged(object sender, KeyEventArgs e)
        {
        }


        private void cbSpec_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string strSpec = (sender as ComboBox).Text.Trim();
                if (string.IsNullOrEmpty(strSpec) || (sender as ComboBox).SelectedIndex != -1) return;
                string key = (cbMainSpecT1.SelectedItem as HKLibSpecDic).ID;
                if ((sender as ComboBox).Name == "cbMainSpec2")
                    key = (cbMainSpecT2.SelectedItem as HKLibSpecDic).ID;
                else if ((sender as ComboBox).Name == "cbMainSpec3")
                    key = (cbMainSpecT3.SelectedItem as HKLibSpecDic).ID;
                else if ((sender as ComboBox).Name == "cbAuxSpec1")
                    key = (cbAuxSpecT1.SelectedItem as HKLibSpecDic).ID;
                else if ((sender as ComboBox).Name == "cbAuxSpec2")
                    key = (cbAuxSpecT2.SelectedItem as HKLibSpecDic).ID;
                else if ((sender as ComboBox).Name == "cbAuxSpec3")
                    key = (cbAuxSpecT3.SelectedItem as HKLibSpecDic).ID;
                else if ((sender as ComboBox).Name == "cbSizeP1")
                    key = (cbTypeP1.SelectedItem as HKLibSpecDic).ID;
                else if ((sender as ComboBox).Name == "cbSizeP2")
                    key = (cbTypeP2.SelectedItem as HKLibSpecDic).ID;
                if (!dicNoLinkSpecStr[key].Contains(strSpec))
                {
                    HKLibGenOption newSpec = new HKLibGenOption
                    {
                        ID = strSpec,
                        NameCn = (intLan == 0) ? strSpec : null,
                        NameEn = (intLan != 0) ? strSpec : null,
                    };
                    dicNoLinkSpec[key].Add(newSpec);
                    dicNoLinkSpecStr[key].Add(strSpec);
                }
                for (int i = 0; i < dicNoLinkSpec[key].Count(); i++)
                {
                    if (dicNoLinkSpec[key][i].ID == strSpec)
                    {
                        (sender as ComboBox).SelectedIndex = i;
                        break;
                    }

                }

            }
        }

        private void tbMoreSpecCn_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void cbMatMat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cbMainSpec_LostFocus(object sender, RoutedEventArgs e)
        {
            string strSpec = (sender as ComboBox).Text.Trim();
            if (string.IsNullOrEmpty(strSpec) || (sender as ComboBox).SelectedIndex != -1) return;
            string key = (cbMainSpecT1.SelectedItem as HKLibSpecDic).ID;
            if ((sender as ComboBox).Name == "cbMainSpec2")
                key = (cbMainSpecT2.SelectedItem as HKLibSpecDic).ID;
            else if ((sender as ComboBox).Name == "cbMainSpec3")
                key = (cbMainSpecT3.SelectedItem as HKLibSpecDic).ID;
            if (!dicNoLinkSpecStr[key].Contains(strSpec))
            {
                HKLibGenOption newSpec = new HKLibGenOption
                {
                    ID = strSpec,
                    NameCn = (intLan == 0) ? strSpec : null,
                    NameEn = (intLan != 0) ? strSpec : null,
                };
                dicNoLinkSpec[key].Add(newSpec);
                dicNoLinkSpecStr[key].Add(strSpec);

            }


        }

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
                     strCondP1 = $"{strCondP1} AND mgl.TypeP1 in {GeneralFun.ConvertToStringScope(strTypeP1,',')}";
               UpdateQueryResult();
            }
            else if (cbSizeP1.SelectedIndex == 0)
            {
                //strCondP1 = $"mgl.SizeP1 IS NOT NULL AND LTRIM(RTRIM(mgl.SizeP1)) <> ''";
                strCondP1 = string.Empty;
                if (!string.IsNullOrEmpty(strTypeP1))
                    strCondP1 = string.IsNullOrEmpty(strCondP1) ?
                        $"mgl.TypeP1 in {GeneralFun.ConvertToStringScope(strTypeP1, ',')}":
                        $"{strCondP1} AND mgl.TypeP1 in {GeneralFun.ConvertToStringScope(strTypeP1, ',')}";
                UpdateQueryResult();
            }
            else
                strCondP1 =string.Empty ;
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
                    strCondP2 = string.IsNullOrEmpty(strCondP2)?
                        $"mgl.TypeP2 in {GeneralFun.ConvertToStringScope(strTypeP2, ',')}":
                        $"{strCondP2} AND mgl.TypeP2 in {GeneralFun.ConvertToStringScope(strTypeP2, ',')}";
                UpdateQueryResult();
            }
            else
                strCondP2 = string.Empty;
        }

        private ObservableCollection<HKLibSpecDic> GetLibSpecDic(string strIDs)
        {
            ObservableCollection<HKLibSpecDic> libSpecDics = new ObservableCollection<HKLibSpecDic>();
            // 构建 SQL 查询语句
            string query = "select * from HK_LibSpecDic where ID in " + GeneralFun.ConvertToStringScope(strIDs, '|') + " order by SortNum";
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HKLibSpecDic libSpecDic = new HKLibSpecDic
                    {
                        ID = Convert.ToString(reader["ID"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        PrefixCn = Convert.ToString(reader["PrefixCn"]),
                        PrefixEn = Convert.ToString(reader["PrefixEn"]),
                        Class = Convert.ToString(reader["Class"]),
                        Link = Convert.ToString(reader["Link"]),
                        SuffixCn = Convert.ToString(reader["SuffixCn"]),
                        SuffixEn = Convert.ToString(reader["SuffixEn"]),
                        SortNum = Convert.ToInt32(reader["SortNum"])
                    };
                    libSpecDics.Add(libSpecDic);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            libSpecDics.Add(new HKLibSpecDic
            {
                ID = "-",
                NameCn = "移除此项",
                NameEn = "Remove it",
                SortNum = 9999
            });
            return libSpecDics;
        }

        private ObservableCollection<HKLibGenOption> GetGeneralSpecOptions(HKLibSpecDic libSpecDic)
        {
            if (libSpecDic == null || libSpecDic.ID == string.Empty || libSpecDic.ID == "-") return null;
            ObservableCollection<HKLibGenOption> hKGeneralSpecs = new ObservableCollection<HKLibGenOption>();
            HKLibGenOption hkGeneralSpec = new HKLibGenOption();
            string prefix = (HK_Mat_Main.intLan == 0) ? libSpecDic.PrefixCn : libSpecDic.PrefixEn;
            string suffix = (HK_Mat_Main.intLan == 0) ? libSpecDic.SuffixCn : libSpecDic.SuffixEn;
            switch (libSpecDic?.Class.ToUpper())
            {
                case "LINK":
                    // 构建 SQL 查询语句
                    string query = GeneralFun.ParseLinkExp(libSpecDic.Link);
                    try
                    {
                        if (conn == null || conn.State != ConnectionState.Open)
                            conn = GetConnection();
                        OdbcCommand command = new OdbcCommand(query, conn);
                        OdbcDataReader reader = command.ExecuteReader();
                        if (libSpecDic.Link.StartsWith("LibPipeOD"))
                        {
                            hkGeneralSpec = new HKLibGenOption
                            {
                                ID = string.Empty,
                                NameCn = "选择公称直径",
                                NameEn = "All DN/NPS"
                            };
                            hKGeneralSpecs.Add(hkGeneralSpec);
                            if (libSpecDic.ID.Contains("NPS") || libSpecDic.ID.Contains("ASME") || libSpecDic.ID.Contains("ANSI"))
                            {
                                while (reader.Read())
                                {
                                    hkGeneralSpec = new HKLibGenOption
                                    {
                                        ID = Convert.ToString(reader["ID"]),
                                        NameCn = libSpecDic.ID.StartsWith("OD") ?
                                                 $"NPS {Convert.ToString(reader["NPS"])} - {prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}" :
                                                 $"NPS {Convert.ToString(reader["NPS"])}",
                                        NameEn = libSpecDic.ID.StartsWith("OD") ?
                                                 $"NPS {Convert.ToString(reader["NPS"])} - {prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}" :
                                                 $"NPS {Convert.ToString(reader["NPS"])}",
                                        SpecCn = $"{prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}",
                                        SpecEn = $"{prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}"
                                    };
                                    hKGeneralSpecs.Add(hkGeneralSpec);
                                }
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    hkGeneralSpec = new HKLibGenOption
                                    {
                                        ID = Convert.ToString(reader["ID"]),
                                        NameCn = libSpecDic.ID.StartsWith("OD") ?
                                                 $"DN {Convert.ToString(reader["DN"])} - {prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}" :
                                                 $"DN {Convert.ToString(reader["DN"])}",
                                        NameEn = libSpecDic.ID.StartsWith("OD") ?
                                                 $"DN {Convert.ToString(reader["DN"])} - {prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}" :
                                                 $"DN {Convert.ToString(reader["DN"])}",
                                        SpecCn = $"{prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}",
                                        SpecEn = $"{prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}"
                                    };
                                    hKGeneralSpecs.Add(hkGeneralSpec);
                                }
                            }
                        }
                        else if (libSpecDic.Link.StartsWith("LibSteel"))
                        {
                            if (libSpecDic.ID.StartsWith("CS"))
                            {
                                hkGeneralSpec = new HKLibGenOption
                                {
                                    ID = string.Empty,
                                    NameCn = "选择槽钢规格",
                                    NameEn = "All Channel Steels"
                                };
                                hKGeneralSpecs.Add(hkGeneralSpec);
                                while (reader.Read())
                                {
                                    hkGeneralSpec = new HKLibGenOption
                                    {
                                        ID = Convert.ToString(reader["ID"]),
                                        NameCn = $"{prefix}{Convert.ToString(reader["CSSpecCn"])}{suffix}",
                                        NameEn = $"{prefix}{Convert.ToString(reader["CSSpecEn"])}{suffix}",
                                        SpecCn = $"{prefix}{Convert.ToString(reader["CSSpecCn"])}{suffix}",
                                        SpecEn = $"{prefix}{Convert.ToString(reader["CSSpecEn"])}{suffix}"
                                    };
                                    hKGeneralSpecs.Add(hkGeneralSpec);
                                }
                            }
                            else if (libSpecDic.ID.StartsWith("IB"))
                            {
                                hkGeneralSpec = new HKLibGenOption
                                {
                                    ID = string.Empty,
                                    NameCn = "选择工字钢规格",
                                    NameEn = "All I-Beams"
                                };
                                hKGeneralSpecs.Add(hkGeneralSpec);
                                while (reader.Read())
                                {
                                    hkGeneralSpec = new HKLibGenOption
                                    {
                                        ID = Convert.ToString(reader["ID"]),
                                        NameCn = $"{prefix}{Convert.ToString(reader["IBSpecCn"])}{suffix}",
                                        NameEn = $"{prefix}{Convert.ToString(reader["IBSpecEn"])}{suffix}",
                                        SpecCn = $"{prefix}{Convert.ToString(reader["IBSpecCn"])}{suffix}",
                                        SpecEn = $"{prefix}{Convert.ToString(reader["IBSpecEn"])}{suffix}"
                                    };
                                    hKGeneralSpecs.Add(hkGeneralSpec);
                                }
                            }
                        }
                        else if (libSpecDic.Link.StartsWith("LibPN")
                              || libSpecDic.Link.StartsWith("LibGland")
                              || libSpecDic.Link.StartsWith("LibTubeOD")
                              || libSpecDic.Link.StartsWith("LibThread"))
                        {
                            if (libSpecDic.Link.StartsWith("LibPN"))
                            {
                                hkGeneralSpec = new HKLibGenOption
                                {
                                    ID = string.Empty,
                                    NameCn = "选择公称压力",
                                    NameEn = "All PN/CLS"
                                };
                            }
                            else if (libSpecDic.Link.StartsWith("LibGland"))
                            {
                                hkGeneralSpec = new HKLibGenOption
                                {
                                    ID = string.Empty,
                                    NameCn = "选择电缆索头",
                                    NameEn = "All Cable Glands"
                                };
                            }
                            else if (libSpecDic.Link.StartsWith("LibTubeOD"))
                            {
                                hkGeneralSpec = new HKLibGenOption
                                {
                                    ID = string.Empty,
                                    NameCn = "选择Tube管外径",
                                    NameEn = "All Tube O.D."
                                };
                            }
                            else if (libSpecDic.Link.StartsWith("LibThread"))
                            {
                                hkGeneralSpec = new HKLibGenOption
                                {
                                    ID = string.Empty,
                                    NameCn = "选择螺纹规格",
                                    NameEn = "All Thread Typies"
                                };
                            }
                            hKGeneralSpecs.Add(hkGeneralSpec);
                            while (reader.Read())
                            {
                                hkGeneralSpec = new HKLibGenOption
                                {
                                    ID = Convert.ToString(reader["ID"]),
                                    NameCn = $"{prefix}{Convert.ToString(reader["SpecCn"])}{suffix}",
                                    NameEn = $"{prefix}{Convert.ToString(reader["SpecEn"])}{suffix}",
                                    SpecCn = $"{prefix}{Convert.ToString(reader["SpecCn"])}{suffix}",
                                    SpecEn = $"{prefix}{Convert.ToString(reader["SpecEn"])}{suffix}"
                                };
                                hKGeneralSpecs.Add(hkGeneralSpec);
                            }
                        }
                        else if (libSpecDic.Link.StartsWith("LibGenOption"))
                        {
                            hkGeneralSpec = new HKLibGenOption
                            {
                                ID = string.Empty,
                                NameCn = "选择相关规格",
                                NameEn = "All Specifications"
                            };
                            hKGeneralSpecs.Add(hkGeneralSpec);
                            while (reader.Read())
                            {
                                hkGeneralSpec = new HKLibGenOption
                                {
                                    ID = Convert.ToString(reader["ID"]),
                                    NameCn = $"{prefix}{Convert.ToString(reader["NameCn"])}{suffix}",
                                    NameEn = $"{prefix}{Convert.ToString(reader["NameEn"])}{suffix}",
                                    SpecCn = $"{prefix}{Convert.ToString(reader["SpecCn"])}{suffix}",
                                    SpecEn = $"{prefix}{Convert.ToString(reader["SpecEn"])}{suffix}"
                                };
                                hKGeneralSpecs.Add(hkGeneralSpec);
                            }
                        }
                        reader.Close();
                    }

                    catch (Exception ex)
                    {
                        // 处理异常
                        MessageBox.Show($"Error: {ex.Message}");
                        // 可以选择返回空列表或者其他适当的处理
                    }
                    break;
                default:
                    if (!dicNoLinkSpec.ContainsKey(libSpecDic.ID))
                    {
                        dicNoLinkSpec.Add(libSpecDic.ID, new ObservableCollection<HKLibGenOption>());
                        dicNoLinkSpecStr.Add(libSpecDic.ID, new ObservableCollection<string>());
                    }
                    hKGeneralSpecs = dicNoLinkSpec[libSpecDic.ID];
                    break;
            }
            return hKGeneralSpecs;
        }

        private void UpdateQueryResult()
        {
            // 构建 SQL 查询语句
            //string query = GeneralFun.ParseLinkExp(libSpecDic.Link);
            result.Clear();
            string conditions = GetConditionExp(new List<string> 
            {
                strCondSubCat,
                strCondP1,
                strCondP2,
                GetSpecExp("mgl.TechSpecMain", lstMainSpec),
                GetSpecExp("mgl.TechSpecAux", lstAuxSpec)
            });
            string query = $"select " +
                $"mgl.ID as ID, " +
                $"sc.SpecCn as NameCn, " +
                $"sc.SpecEn as NameEn, " +
                $"sc.TypeP2 as AlterCode, " +
                $"mgl.TypeP1 as TypeP1, " +
                $"mgl.TypeP2 as TypeP2, " +
                $"mgl.SizeP1 as SizeP1, " +
                $"mgl.SizeP2 as SizeP2, " +
                $"mgl.TechSpecMain as TechSpecMain, " +
                $"mgl.TechSpecAux as TechSpecAux " +
                $"from HK_MatGenLib mgl" +
                $" inner join HK_MatSubCat sc on mgl.SubCatID = sc.ID" +
                $" WhERE {conditions}";
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HKMatGenLib item = new HKMatGenLib
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        AlterCode = Convert.ToString(reader["AlterCode"]),
                        TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                        TechSpecAux = Convert.ToString(reader["TechSpecAux"]),
                        TypeP1 = Convert.ToString(reader["TypeP1"]),
                        TypeP2 = Convert.ToString(reader["TypeP2"]),
                        SizeP1 = Convert.ToString(reader["SizeP1"]),
                        SizeP2 = Convert.ToString(reader["SizeP2"]),
                    };
                    item.SpecCombMain = SetSpecMainAux(item.TechSpecMain);
                    item.SpecCombAux = SetSpecMainAux(item.TechSpecAux);
                   result.Add(item);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }

        private int GetNewID()
        {
            string query = $"SELECT  MAX(ID) FROM HK_MatGenLib";
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                // 创建并配置 OdbcCommand 对象
                using (OdbcCommand command = new OdbcCommand(query, conn))
                {
                    // 执行查询，获取记录数
                    return (int)command.ExecuteScalar() + 1; 
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
                return 0;
            }
        }

        private bool IsDataExisting()
        {
            try
            {
                string pn = lstMainSpec.Where(x => x.StartsWith("PN:")).FirstOrDefault()?.Split(':')[1];
                if (string.IsNullOrEmpty(pn)) pn = lstAuxSpec.Where(x => x.StartsWith("PN:")).FirstOrDefault()?.Split(':')[1];
                string query = $"SELECT  COUNT(*) FROM HK_MatGenLib WHERE " +
                                $"CatID = '{((cbMainCat.SelectedIndex > 0) ? (cbMainCat.SelectedItem as HKMatMainCat)?.ID : (cbSubCat.SelectedItem as HKMatSubCat)?.ID?.Substring(0, 2))}' AND " +
                                $"SubCatID = '{(cbSubCat.SelectedItem as HKMatSubCat)?.ID}' AND " +
                                $"TechSpecMain = '{string.Join(",", lstMainSpec)}' AND " +
                                $"TechSpecAux = '{string.Join(",", lstAuxSpec)}' AND " +
                                $"TypeP1 = '{(cbTypeP1.SelectedItem as HKLibPortType)?.ID}' AND " +
                                $"SizeP1 = '{(cbSizeP1.SelectedItem as HKLibGenOption)?.ID}' AND " +
                                $"TypeP2 = '{(cbTypeP2.SelectedItem as HKLibPortType)?.ID}' AND " +
                                $"SizeP2 = '{(cbSizeP1.SelectedItem as HKLibGenOption)?.ID}' AND " +
                                $"MatSpec = '{(cbMatMat.SelectedItem as HKLibGenOption)?.ID}' AND " +
                                $"PClass = '{pn}' AND " +
                                $"MoreSpecCn = '{tbMoreSpecCn.Text}' AND " +
                                $"MoreSpecEn = '{tbMoreSpecEn.Text}'";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                // 创建并配置 OdbcCommand 对象
                using (OdbcCommand command = new OdbcCommand(query, conn))
                {
                    // 执行查询，获取记录数
                    return (int)command.ExecuteScalar() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
                return false;
            }

        }
        private int NewDataAdd()
        {
            if (cbSubCat.SelectedItem == null || cbSubCat.SelectedIndex == 0)
            {
                MessageBox.Show($"数据未记录！{Environment.NewLine}必须选择材料小类");
                cbSubCat.Focus();
                return 0;
            }
            try
            {
                if (IsDataExisting()) return 0;    
                string pn = lstMainSpec.Where(x => x.StartsWith("PN:")).FirstOrDefault()?.Split(':')[1];
                if (string.IsNullOrEmpty(pn)) pn = lstAuxSpec.Where(x => x.StartsWith("PN:")).FirstOrDefault()?.Split(':')[1];
                string query = $"INSERT INTO HK_MatGenLib (ID, CatID, SubCatID, TechSpecMain, TechSpecAux, TypeP1, SizeP1, TypeP2, SizeP2, MatSpec, PClass, MoreSpecCn, MoreSpecEn, Status) VALUES (" +
                                    $"'{GetNewID()}'," +
                                    $"'{((cbMainCat.SelectedIndex > 0) ? (cbMainCat.SelectedItem as HKMatMainCat)?.ID : (cbSubCat.SelectedItem as HKMatSubCat)?.ID?.Substring(0, 2))}'," +
                                    $"'{(cbSubCat.SelectedItem as HKMatSubCat)?.ID}'," +
                                    $"'{string.Join(",", lstMainSpec)}'," +
                                    $"'{string.Join(",", lstAuxSpec)}'," +
                                    $"'{(cbTypeP1.SelectedItem as HKLibPortType)?.ID}'," +
                                    $"'{(cbSizeP1.SelectedItem as HKLibGenOption)?.ID}'," +
                                    $"'{(cbTypeP2.SelectedItem as HKLibPortType)?.ID}'," +
                                    $"'{(cbSizeP1.SelectedItem as HKLibGenOption)?.ID}'," +
                                    $"'{(cbMatMat.SelectedItem as HKLibGenOption)?.ID}'," +
                                    $"'{pn}'," +
                                    $"'{tbMoreSpecCn.Text}'," +
                                    $"'{tbMoreSpecEn.Text}'," +
                                    $"1" +
                                    $")";
                //string query = "";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                // 创建并配置 OdbcCommand 对象
                using (OdbcCommand command = new OdbcCommand(query, conn))
                {
                    // 执行查询，获取记录数
                    return command.ExecuteNonQuery(); ;
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"数据未记录！{Environment.NewLine}Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
                return 0;
            }
        }

        private string GetConditionExp(List<string> input)
        {
            if (input == null || !input.Any())
                return string.Empty;

            return string.Join(" AND ", input.Where(s => !string.IsNullOrEmpty(s)));
        }

        private string GetSpecExp(string field, List<string> input)
        {
            if (input == null || !input.Any())
                return string.Empty;

            var conditions = input
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => $"{field} LIKE '%{s}%'");

            return string.Join(" AND ", conditions);
        }

        private void SetDicMatGen()
        {
            SetDicPortType();
            SetDicSpecDic();
            SetDicPipeOD();
            SetDicPN();
            SetDicSteel();
            SetDicThread();
            SetDicTubeOD();
            SetDicGland();
            SetDicGenOption();
        }
        private void SetDicPortType()
        {
            dicPortType.Clear();
            try
            {
                string query = "select * from HK_LibPortType where SortNum < 101 order by SortNum";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dicPortType.Add(Convert.ToString(reader["ID"]), new HKLibPortType
                    {
                        ID = Convert.ToString(reader["ID"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        PrefixCn = Convert.ToString(reader["PrefixCn"]),
                        PrefixEn = Convert.ToString(reader["PrefixEn"]),
                        SuffixCn = Convert.ToString(reader["SuffixCn"]),
                        SuffixEn = Convert.ToString(reader["SuffixEn"]),
                        Class =  Convert.ToString(reader["Class"]),
                        SubClass = Convert.ToString(reader["SubClass"]),
                        Remarks = Convert.ToString(reader["Remarks"]),
                        Link = Convert.ToString(reader["Link"]),
                        SortNum = Convert.ToInt32(reader["SortNum"])
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }
        private void SetDicSpecDic()
        {
            dicSpecDic.Clear();
            try
            {
                string query = "select * from HK_LibSpecDic order by SortNum";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dicSpecDic.Add(Convert.ToString(reader["ID"]), new HKLibSpecDic
                    {
                        ID = Convert.ToString(reader["ID"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        PrefixCn = Convert.ToString(reader["PrefixCn"]),
                        PrefixEn = Convert.ToString(reader["PrefixEn"]),
                        SuffixCn = Convert.ToString(reader["SuffixCn"]),
                        SuffixEn = Convert.ToString(reader["SuffixEn"]),
                        Class = Convert.ToString(reader["Class"]),
                        Link = Convert.ToString(reader["Link"]),
                        SortNum = Convert.ToInt32(reader["SortNum"])
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }
        private void SetDicPipeOD()
        {
            dicPipeOD.Clear();
            try
            {
                string query = "select * from HK_LibPipeOD order by SortNum";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dicPipeOD.Add(Convert.ToString(reader["ID"]), new HKLibPipeOD
                    {
                        ID = Convert.ToString(reader["ID"]),
                        DN = Convert.ToString(reader["DN"]),
                        NPS = Convert.ToString(reader["NPS"]),
                        HGIa = !string.IsNullOrEmpty(Convert.ToString(reader["HGIa"])) ? Convert.ToDecimal(reader["HGIa"]) : nullDecimal,
                        HGIb = !string.IsNullOrEmpty(Convert.ToString(reader["HGIb"])) ? Convert.ToDecimal(reader["HGIb"]) : nullDecimal,
                        HGII = !string.IsNullOrEmpty(Convert.ToString(reader["HGII"])) ? Convert.ToDecimal(reader["HGII"]) : nullDecimal,
                        GBI = !string.IsNullOrEmpty(Convert.ToString(reader["GBI"])) ? Convert.ToDecimal(reader["GBI"]) : nullDecimal,
                        GBII = !string.IsNullOrEmpty(Convert.ToString(reader["GBII"])) ? Convert.ToDecimal(reader["GBII"]) : nullDecimal,
                        ISO = !string.IsNullOrEmpty(Convert.ToString(reader["ISO"])) ? Convert.ToDecimal(reader["ISO"]) : nullDecimal,
                        ASME = !string.IsNullOrEmpty(Convert.ToString(reader["ASME"])) ? Convert.ToDecimal(reader["ASME"]) : nullDecimal,
                        SWDiaGB = !string.IsNullOrEmpty(Convert.ToString(reader["SWDiaGB"])) ? Convert.ToDecimal(reader["SWDiaGB"]) : nullDecimal,
                        SpecRem = Convert.ToString(reader["SpecRem"]),
                        SortNum = Convert.ToInt32(reader["SortNum"]),
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }
        private void SetDicPN()
        {
            dicPN.Clear();
            try
            {
                string query = "select * from HK_LibPN order by SortNum";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dicPN.Add(Convert.ToString(reader["ID"]), new HKLibPN
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Class = Convert.ToString(reader["Class"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        ISOS1 = Convert.ToString(reader["ISOS1"]),
                        ISOS2 = Convert.ToString(reader["ISOS2"]),
                        GBDIN = Convert.ToString(reader["GBDIN"]),
                        GBANSI = Convert.ToString(reader["GBANSI"]),
                        ASME = Convert.ToString(reader["ASME"]),
                        SortNum = Convert.ToInt32(reader["SortNum"])
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }
        private void SetDicSteel()
        {
            dicSteel.Clear();
            try
            {
                string query = "select * from HK_LibSteel order by SortNum";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dicSteel.Add(Convert.ToString(reader["ID"]), new HKLibSteel
                    {
                        CSSpecCn = Convert.ToString(reader["CSSpecCn"]),
                        CSSpecEn = Convert.ToString(reader["CSSpecEn"]),
                        IBSpecCn = Convert.ToString(reader["IBSpecCn"]),
                        IBSpecEn = Convert.ToString(reader["IBSpecEn"]),
                        Width = Convert.ToDecimal(reader["Width"]),
                        CSb = !string.IsNullOrEmpty(Convert.ToString(reader["CSb"])) ? Convert.ToDecimal(reader["CSb"]) : nullDecimal,
                        CSd = !string.IsNullOrEmpty(Convert.ToString(reader["CSd"])) ? Convert.ToDecimal(reader["CSd"]) : nullDecimal,
                        IBb = !string.IsNullOrEmpty(Convert.ToString(reader["IBb"])) ? Convert.ToDecimal(reader["IBb"]) : nullDecimal,
                        IBd = !string.IsNullOrEmpty(Convert.ToString(reader["IBd"])) ? Convert.ToDecimal(reader["IBd"]) : nullDecimal,
                        SortNum = Convert.ToInt32(reader["SortNum"]),
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }
        private void SetDicThread()
        {
            dicThread.Clear();
            try
            {
                string query = "select * from HK_LibThread order by SortNum";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dicThread.Add(Convert.ToString(reader["ID"]), new HKLibThread
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Class = Convert.ToString(reader["Class"]),
                        SubClass = Convert.ToString(reader["SubClass"]),
                        ClassEx = Convert.ToString(reader["ClassEx"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Value = Convert.ToDecimal(reader["Value"]),
                        Pitch = Convert.ToDecimal(reader["Pitch"]),
                        Qty = !string.IsNullOrEmpty(Convert.ToString(reader["Qty"])) ? Convert.ToInt32(reader["Qty"]) : nullInt,
                        SortNum = Convert.ToInt32(reader["SortNum"])
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }
        private void SetDicTubeOD()
        {
            dicTubeOD.Clear();
            try
            {
                string query = "select * from HK_LibTubeOD order by SortNum";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dicTubeOD.Add(Convert.ToString(reader["ID"]), new HKLibTubeOD
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Class = Convert.ToString(reader["Class"]),
                        ClassEx = Convert.ToString(reader["ClassEx"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        ValueM = Convert.ToDecimal(reader["ValueM"]),
                        SortNum = Convert.ToInt32(reader["SortNum"])
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }
        private void SetDicGland()
        {
            dicGland.Clear();
            try
            {
                string query = "select * from HK_LibGland order by SortNum";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dicGland.Add(Convert.ToString(reader["ID"]), new HKLibGland
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Class = Convert.ToString(reader["Class"]),
                        ClassEx = Convert.ToString(reader["ClassEx"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        CabODMin = Convert.ToDecimal(reader["CabODMin"]),
                        CabODMax = Convert.ToDecimal(reader["CabODMax"]),
                        SortNum = Convert.ToInt32(reader["SortNum"])
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }
        private void SetDicGenOption()
        {
            dicGenOption.Clear();
            try
            {
                string query = "select * from HK_LibGenOption order by SortNum";
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    dicGenOption.Add(Convert.ToString(reader["ID"]), new HKLibGenOption
                    {
                        ID = Convert.ToString(reader["ID"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Cat = Convert.ToString(reader["Cat"]),
                        Inact = !string.IsNullOrEmpty(Convert.ToString(reader["Inact"])) ? Convert.ToInt32(reader["Inact"]) : nullInt,
                        SortNum = Convert.ToInt32(reader["SortNum"])
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
        }
        private string SetSpecMainAux(string input)
        {
            string result = string.Empty;
            string results = string.Empty;
            if (string.IsNullOrEmpty(input)) return string.Empty;
            List<string> segments = input.Split(',').ToList();
            try
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    string name = segments[i].Trim().Split(':')?[0];
                    string value = segments[i].Trim().Split(':')?[1];
                    if (string.IsNullOrEmpty(value))
                        result = string.Empty;
                    else
                    {
                        if (dicSpecDic.ContainsKey(name))
                        {
                            if (dicSpecDic[name].Link.StartsWith("LibPipeOD"))
                                result = $"{dicSpecDic[name].Prefix}{GetPipeData(value, dicSpecDic[name].Link.Split(',')[1])}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibPN"))
                                result = $"{dicSpecDic[name].Prefix}{dicPN[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibGenOption"))
                                result = $"{dicSpecDic[name].Prefix}{dicGenOption[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibGland"))
                                result = $"{dicSpecDic[name].Prefix}{dicGland[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibThread"))
                                result = $"{dicSpecDic[name].Prefix}{dicThread[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibTubeOD"))
                                result = $"{dicSpecDic[name].Prefix}{dicTubeOD[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibSteel"))
                                result = $"{dicSpecDic[name].Prefix}{GetSteelData(value, dicSpecDic[name].Link.Split(',')[1])}{dicSpecDic[name].Suffix}";
                            else
                                result = $"{dicSpecDic[name].Prefix}{dicPN[value].Spec}{dicSpecDic[name].Suffix}";
                        }
                    }
                    results = string.IsNullOrEmpty(results) ? result : results + ", " + result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
                return string.Empty;
            }
            return results;
        }
        private string SetSpecPort(string typeP1, string sizeP1, string typeP2, string sizeP2, string alterCode = "")
        {
            string result1 = string.Empty;
            string result2 = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(typeP1))
                {
                    if (string.IsNullOrEmpty(sizeP1))
                        result1 = dicPortType[typeP1].Name;
                    else if (dicPortType.ContainsKey(typeP1))
                    {
                        if (dicPortType[typeP1].Link.StartsWith("LibPipeOD"))
                            result1 = $"{dicPortType[typeP1].Prefix}{GetPipeData(sizeP1, dicPortType[typeP1].Link.Split(',')[1])}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibPN"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicPN[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibGenOption"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicGenOption[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibGland"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicGland[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibThread"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicThread[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibTubeOD"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicTubeOD[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibSteel"))
                            result1 = $"{dicPortType[typeP1].Prefix}{GetSteelData(sizeP1, dicPortType[typeP1].Link.Split(',')[1])}{dicPortType[typeP1].Suffix}";
                        else
                            result1 = $"{dicPortType[typeP1].Prefix}{dicPN[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                    }
                }
                if (!string.IsNullOrEmpty(typeP2))
                {
                    if (string.IsNullOrEmpty(sizeP2))
                        result2 = dicPortType[typeP2].Name;
                    else if (dicPortType.ContainsKey(typeP2))
                    {
                        if (dicPortType[typeP2].Link.StartsWith("LibPipeOD"))
                            result2 = $"{dicPortType[typeP2].Prefix}{GetPipeData(sizeP2, dicPortType[typeP2].Link.Split(',')[1])}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibPN"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicPN[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibGenOption"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicGenOption[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibGland"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicGland[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibThread"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicThread[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibTubeOD"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicTubeOD[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibSteel"))
                            result2 = $"{dicPortType[typeP2].Prefix}{GetSteelData(sizeP2, dicPortType[typeP2].Link.Split(',')[1])}{dicPortType[typeP2].Suffix}";
                        else
                            result2 = $"{dicPortType[typeP2].Prefix}{dicPN[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                    }
                }
                if(alterCode == "AS1" || alterCode == "DF1" && result1 == result2) 
                    result2=string.Empty;  
                return string.IsNullOrEmpty(result2) ? result1 : result1 + " / " + result2;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
                return string.Empty;
            }
        }
        private string GetPipeData(string input, string key = "")
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (key == "DN")
                return dicPipeOD[input]?.DN;
            else if (key == "NPS")
                return dicPipeOD[input]?.NPS;
            else if (key == "HGIa")
                return dicPipeOD[input]?.HGIa?.ToString();
            else if (key == "HGIb")
                return dicPipeOD[input]?.HGIb?.ToString();
            else if (key == "HGII")
                return dicPipeOD[input]?.HGII?.ToString();
            else if (key == "GBI")
                return dicPipeOD[input]?.GBI?.ToString();
            else if (key == "GBII")
                return dicPipeOD[input]?.GBII?.ToString();
            else if (key == "ISO")
                return dicPipeOD[input]?.ISO?.ToString();
            else if (key == "ASME")
                return dicPipeOD[input]?.ASME?.ToString();
            else if (key == "SWDiaGB")
                return dicPipeOD[input]?.SWDiaGB?.ToString();
            else
                return dicPipeOD[input]?.DN;
        }
        private string GetSteelData(string input, string key = "")
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (key == "CSSpec")
                return dicSteel[input]?.CSSpec;
            else if (key == "IBSpec")
                return dicSteel[input]?.IBSpec;
            else if (key == "CSSpecCN")
                return dicSteel[input]?.CSSpecCn;
            else if (key == "CSSpecEn")
                return dicSteel[input]?.CSSpecEn;
            else if (key == "IBSpecCn")
                return dicSteel[input]?.IBSpecCn;
            else if (key == "IBSpecEn")
                return dicSteel[input]?.IBSpecEn;
            else if (key == "CSb")
                return dicSteel[input]?.CSb?.ToString();
            else if (key == "CSd")
                return dicSteel[input]?.CSd?.ToString();
            else if (key == "IBb")
                return dicSteel[input]?.IBb?.ToString();
            else if (key == "IBd")
                return dicSteel[input]?.IBd?.ToString();
            else
                return string.Empty;
        }
        private void btnToBeChk_Click(object sender, RoutedEventArgs e)
        {
            //string test = GeneralFun.ConvertToSqlString("Class:<: NPTM | NPTF");
            //string test = GeneralFun.ParseLinkExp("LibThread,,Class:IN:NPTM|NPSC");
            //var t1 = GeneralFun.ParseNumber("12345.0");
            UpdateQueryResult();

            //int test = GetNewID();
            //NewDataAdd();

            //SetDicMatGen();
            //string test1 = SetSpecMainAux("DN:0080,PN:PN16");
            //string test2 = SetSpecPort("W", "", "W", "0050", "DF1");

        }

    }
}
