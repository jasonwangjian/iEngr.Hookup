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

namespace iEngr.Hookup
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class HK_Mat_Main : UserControl
    {
        public static int intLan;
        private static OdbcConnection conn;
        private ObservableCollection<HKMatMainCat> hKMatMainCats;
        private ObservableCollection<HKLibThread> hKLibThreads;
        private ObservableCollection<HKMatSubCat> hKMatSubCats = new ObservableCollection<HKMatSubCat>
            {
                new HKMatSubCat
                {
                ID = "%",
                SpecCn = "所有小类",
                SpecEn = "All Sub Categories"
                }
            };
        private ObservableCollection<HKLibPortType> hKPortTypesP1 = new ObservableCollection<HKLibPortType>
            {
                new HKLibPortType
                {
                ID = "%",
                NameCn = "所有连接类型",
                NameEn = "All Connections"
                }
            };
        private ObservableCollection<HKLibPortType> hKPortTypesP2 = new ObservableCollection<HKLibPortType>
            {
                new HKLibPortType
                {
                ID = "%",
                NameCn = "所有连接类型",
                NameEn = "All Connections"
                }
            };
        private object hKPortSizeP1, hKPortSizeP2;
        //private ObservableCollection<HKLibThread> hKPortSizeP2 = new ObservableCollection<HKLibThread>
        //    {
        //        new HKLibThread
        //        {
        //        ID = "%",
        //        SpecCn = "所有规格",
        //        SpecEn = "All Specification"
        //        }
        //    };
        private string[] portDef = { "EQ1", "DF1", "AS1", "NEQ" };
        private Dictionary <string, ObservableCollection<string>> dicNoLinkSpecStr = new Dictionary<string, ObservableCollection<string>>();
        private Dictionary<string, ObservableCollection<HKLibGenOption>> dicNoLinkSpec = new Dictionary<string, ObservableCollection<HKLibGenOption>>();
        private string strTypeP1, strTypeP1S, strSizeP1S;
        private string strTypeP2, strTypeP2S, strSizeP2S;
        public HK_Mat_Main()
        {
            InitializeComponent();
            intLan = 0; // 0: 中文； 其它为英文
            conn = GetConnection();
            hKMatMainCats = GetHKMatMainCats();
            cbMainCat.ItemsSource = hKMatMainCats;
            cbSubCat.ItemsSource = hKMatSubCats;
            cbMainCat.SelectedIndex = 0;


            hKLibThreads = GetLibThread();
            dgResult.ItemsSource = hKLibThreads;
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
                ID = "%",
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
            //ObservableCollection<HKMatSubCat> subCats = new ObservableCollection<HKMatSubCat>
            //{
            //    new HKMatSubCat
            //    {
            //    ID = "%",
            //    NameCn = mainCat.ID=="%"? "所有小类" : "所有" + mainCat.Name,
            //    NameEn = mainCat.ID=="%"? "All Sub Categories" : "All " + mainCat.Name
            //    }
            //};
            ObservableCollection<HKMatSubCat> subCats = hKMatSubCats;
            strTypeP1 = "";
            subCats.Clear();
            subCats.Add(
                new HKMatSubCat
                {
                    ID = "%",
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
                        TypeP1 = Convert.ToString(reader["TypeP1"]),
                        TypeP2 = Convert.ToString(reader["TypeP2"]),
                        TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                        TechSpecAux = Convert.ToString(reader["TechSpecAux"])
                    };
                    strTypeP1 += Convert.ToString(reader["TypeP1"]);
                    strTypeP1 += Convert.ToString(reader["TypeP2"]);
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
            strTypeP2 = strTypeP1;
            return subCats;
        }
        private ObservableCollection<HKLibPortType> GetHKPortTypes(string strTypes, int intPort)
        {
            ObservableCollection<HKLibPortType> portTypes = (intPort == 1) ? hKPortTypesP1 : hKPortTypesP2;
            portTypes.Clear();
            portTypes.Add(
                new HKLibPortType
                {
                    ID = "%",
                    NameCn = "选择连接类型",
                    NameEn = "Select Conn.Type"
                }
            );

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
                    HKLibPortType portType = new HKLibPortType
                    {
                        ID = Convert.ToString(reader["ID"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Remarks = Convert.ToString(reader["Remarks"]),
                        Link = Convert.ToString(reader["Link"])
                    };
                    portTypes.Add(portType);
                }

                reader.Close();
                if (portTypes.Count == 1 || portTypes.Count == 2 && (portTypes.ElementAt(1).ID == "NA" || portTypes.ElementAt(1).ID == "IS"))
                {
                    portTypes.Clear();
                    portTypes.Add(
                        new HKLibPortType
                        {
                            ID = "%",
                            NameCn = "无",
                            NameEn = "NA"
                        }
                    );
                    if (intPort == 1)
                    {
                        lbPort1.Visibility = Visibility.Collapsed;
                        wpPort1.Visibility = Visibility.Collapsed;

                    }
                    else if (intPort == 2)
                    {
                        lbPort2.Visibility = Visibility.Collapsed;
                        wpPort2.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (intPort == 1)
                    {
                        lbPort1.Visibility = Visibility.Visible;
                        wpPort1.Visibility = Visibility.Visible;

                    }
                    else if (intPort == 2)
                    {
                        lbPort2.Visibility = Visibility.Visible;
                        wpPort2.Visibility = Visibility.Visible;
                    }
                }

                //if (portTypes.Count == 2 && portTypes.ElementAt(1).ID == "IS")
                //{
                //    portTypes.RemoveAt(0);
                //}
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return portTypes;
        }
        private Object GetHKPortSpecs(int intPort)
        {
            HKLibPortType typePS = cbTypeP1.SelectedItem as HKLibPortType;
            if (intPort == 2)
                typePS = cbTypeP2.SelectedItem as HKLibPortType;
            if ((typePS != null) && (typePS.ID != "%"))
            {
                if (typePS.Link.StartsWith("LibThread"))
                {
                    ObservableCollection<HKLibThread> hKLibThreads = new ObservableCollection<HKLibThread>()
                    {
                        new HKLibThread
                        {
                            ID="%",
                            SpecCn = "选择螺纹规格",
                            SpecEn = "All Specification"
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
                            ID="%",
                            SpecCn = "选择Tube管外径",
                            SpecEn = "All Specification"
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
                            ID="%",
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
                                NameCn = $"DN {Convert.ToString(reader["DN"])} / NPS {Convert.ToString(reader["NPS"])}",
                                NameEn = $"DN {Convert.ToString(reader["NPS"])} / NPS {Convert.ToString(reader["DN"])}"
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

        private void btnToBeChk_Click(object sender, RoutedEventArgs e)
        {
            //string test = GeneralFun.ConvertToSqlString("Class:<: NPTM | NPTF");
            string test = GeneralFun.ParseLinkExp("LibThread,,Class:IN:NPTM|NPSC");
            var t1 = GeneralFun.ParseNumber("12345.0");

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
            // 处理主参数 TechSpecMain
            var lstSpecMain = (cbSubCat.SelectedItem as HKMatSubCat)?.TechSpecMain?.Split(',')
                         .Select(item => item.Trim())
                         .Where(item => !string.IsNullOrWhiteSpace(item))
                         .ToList();
            if (lstSpecMain?.Count() > 0)
            {
                if (!string.IsNullOrEmpty(lstSpecMain[0]))
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
                        else
                        {
                            cbMainSpecT3.Visibility = Visibility.Collapsed;
                            cbMainSpec3.Visibility = Visibility.Collapsed;
                        }

                    }
                    else
                    {
                        cbMainSpecT2.Visibility = Visibility.Collapsed;
                        cbMainSpec2.Visibility = Visibility.Collapsed;
                        cbMainSpecT3.Visibility = Visibility.Collapsed;
                        cbMainSpec3.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    cbMainSpecT1.Visibility = Visibility.Collapsed;
                    cbMainSpec1.Visibility = Visibility.Collapsed;
                    cbMainSpecT2.Visibility = Visibility.Collapsed;
                    cbMainSpec2.Visibility = Visibility.Collapsed;
                    cbMainSpecT3.Visibility = Visibility.Collapsed;
                    cbMainSpec3.Visibility = Visibility.Collapsed;
                    lbMainSpec.Visibility = Visibility.Collapsed;
                }

            }
            else
            {
                cbMainSpecT1.Visibility = Visibility.Collapsed;
                cbMainSpec1.Visibility = Visibility.Collapsed;
                cbMainSpecT2.Visibility = Visibility.Collapsed;
                cbMainSpec2.Visibility = Visibility.Collapsed;
                cbMainSpecT3.Visibility = Visibility.Collapsed;
                cbMainSpec3.Visibility = Visibility.Collapsed;
                lbMainSpec.Visibility = Visibility.Collapsed;
            }

            // 处理端口一、二
            if (cbSubCat.SelectedItem != null && cbSubCat.SelectedIndex != 0)
            {
                strTypeP1 = (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP1;
                strTypeP2 = (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2;
            }
            hKPortTypesP1 = GetHKPortTypes(strTypeP1, 1);
            cbTypeP1.ItemsSource = hKPortTypesP1;
            if (portDef.Contains(strTypeP2))
                hKPortTypesP2 = GetHKPortTypes(strTypeP1, 2);
            else
                hKPortTypesP2 = GetHKPortTypes(strTypeP2, 2);

            cbTypeP2.ItemsSource = hKPortTypesP2;
            //cbTypeP1.DisplayMemberPath = "Name";
            cbTypeP1.SelectedIndex = 0;
            //cbTypeP2.DisplayMemberPath = "Name";
            cbTypeP2.SelectedIndex = 0;

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

        private void cbMainSpecT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HKLibSpecDic selectedItem = ((sender as ComboBox).SelectedItem as HKLibSpecDic);
            if (selectedItem == null) return;
            if ((sender as ComboBox).Name == "cbMainSpecT1")
            {
                cbMainSpec1.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbMainSpec1.SelectedIndex = 0;
                cbMainSpec1.IsEditable = (selectedItem.Class.ToUpper() == "LINK") ? false : true;
                cbMainSpec1.MinWidth = (selectedItem.Class.ToUpper() == "LINK") ? 40 : 120;
            }
            else if ((sender as ComboBox).Name == "cbMainSpecT2")
            {
                cbMainSpec2.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbMainSpec2.SelectedIndex = 0;
                cbMainSpec2.IsEditable = (selectedItem.Class.ToUpper() == "LINK") ? false : true;
            }
            else if ((sender as ComboBox).Name == "cbMainSpecT3")
            {
                cbMainSpec3.ItemsSource = GetGeneralSpecOptions(selectedItem);
                cbMainSpec3.SelectedIndex = 0;
                cbMainSpec3.IsEditable = (selectedItem.Class.ToUpper() == "LINK") ? false : true;
            }
        }

        private void cbMainSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cbTypeP1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTypeP1.SelectedItem != null && cbTypeP1.SelectedIndex != 0)
            {
                string link = (cbTypeP1.SelectedItem as HKLibPortType)?.Link;
                if (link.StartsWith("LibThread"))
                {
                    hKPortSizeP1 = GetHKPortSpecs(1);
                    cbSizeP1.ItemsSource = hKPortSizeP1 as ObservableCollection<HKLibThread>;
                    //cbSizeP1.DisplayMemberPath = "Name";
                    cbSizeP1.SelectedIndex = 0;
                }
                else if (link.StartsWith("LibTubeOD"))
                {
                    hKPortSizeP1 = GetHKPortSpecs(1);
                    cbSizeP1.ItemsSource = hKPortSizeP1 as ObservableCollection<HKLibTubeOD>;
                    //cbSizeP1.DisplayMemberPath = "Name";
                    cbSizeP1.SelectedIndex = 0;
                }
                else if (link.StartsWith("LibPipeOD"))
                {
                    hKPortSizeP1 = GetHKPortSpecs(1);
                    cbSizeP1.ItemsSource = hKPortSizeP1 as ObservableCollection<HKLibPipeOD>;
                    //cbSizeP1.DisplayMemberPath = "Name";
                    cbSizeP1.SelectedIndex = 0;
                }
                else
                {
                    cbSizeP1.ItemsSource = null;
                }
            }
            else
            {
                cbSizeP1.ItemsSource = null;
            }

            if ((cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2 == "AS1" || (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2 == "DF1")
            {
                cbTypeP2.ItemsSource = hKPortTypesP2;
                cbTypeP2.SelectedIndex = cbTypeP1.SelectedIndex;
            }
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
                    //cbSizeP2.DisplayMemberPath = "Name";
                    cbSizeP2.SelectedIndex = 0;
                }
                else if (link.StartsWith("LibTubeOD"))
                {
                    hKPortSizeP2 = GetHKPortSpecs(2);
                    cbSizeP2.ItemsSource = hKPortSizeP2 as ObservableCollection<HKLibTubeOD>;
                    //cbSizeP2.DisplayMemberPath = "Name";
                    cbSizeP2.SelectedIndex = 0;
                }
                else if (link.StartsWith("LibPipeOD"))
                {
                    hKPortSizeP2 = GetHKPortSpecs(2);
                    cbSizeP2.ItemsSource = hKPortSizeP2 as ObservableCollection<HKLibPipeOD>;
                    //cbSizeP2.DisplayMemberPath = "Name";
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

        private void cbMainSpec_LostFocus(object sender, RoutedEventArgs e)
        {
            string strSpec = (sender as ComboBox).Text.Trim();
            if (string.IsNullOrEmpty(strSpec)) return;
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
            if (((cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2 == "AS1" || (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP2 == "DF1") && cbSizeP2.ItemsSource != null)
            {
                cbSizeP2.SelectedIndex = cbSizeP1.SelectedIndex;
            }
        }
        private void cbSizeP2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
            return libSpecDics;
        }

        private ObservableCollection<HKLibGenOption> GetGeneralSpecOptions(HKLibSpecDic libSpecDic)
        {
            if (libSpecDic == null) return null;
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
                            if (libSpecDic.ID.StartsWith("NPS"))
                            {
                                while (reader.Read())
                                {
                                    hkGeneralSpec = new HKLibGenOption
                                    {
                                        ID = Convert.ToString(reader["ID"]),
                                        NameCn = $"{prefix}{Convert.ToString(reader["NPS"])}{suffix}",
                                        NameEn = $"{prefix}{Convert.ToString(reader["NPS"])}{suffix}",
                                        SpecCn = $"{prefix}{Convert.ToString(reader["NPS"])}{suffix}",
                                        SpecEn = $"{prefix}{Convert.ToString(reader["NPS"])}{suffix}"
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
                                        NameCn = $"{prefix}{Convert.ToString(reader["DN"])}{suffix}",
                                        NameEn = $"{prefix}{Convert.ToString(reader["DN"])}{suffix}",
                                        SpecCn = $"{prefix}{Convert.ToString(reader["DN"])}{suffix}",
                                        SpecEn = $"{prefix}{Convert.ToString(reader["DN"])}{suffix}"
                                    };
                                    hKGeneralSpecs.Add(hkGeneralSpec);
                                }
                            }
                        }
                        if (libSpecDic.Link.StartsWith("LibSteel"))
                        {
                            if (libSpecDic.ID.StartsWith("CS"))
                            {
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
                              || libSpecDic.Link.StartsWith("LibYubeOD")
                              || libSpecDic.Link.StartsWith("LibThread"))
                        {
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

        private ObservableCollection<HKLibThread> GetLibThread()
        {
            ObservableCollection<HKLibThread> libThreads = new ObservableCollection<HKLibThread>();
            // 构建 SQL 查询语句
            string query = "select * from HK_LibThread";
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                int? nullInt = null;
                int i = 0;
                while (reader.Read())
                {
                    i++;
                    Debug.Print(i.ToString());
                    if (string.IsNullOrEmpty(Convert.ToString(reader["ID"]).Trim()))
                        break;
                    HKLibThread libThread = new HKLibThread
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Class = Convert.ToString(reader["Class"]),
                        SubClass = Convert.ToString(reader["SubClass"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Value = Convert.ToDecimal(reader["Value"]),
                        Pitch = Convert.ToDecimal(reader["Pitch"]),
                        Qty = !string.IsNullOrEmpty(Convert.ToString(reader["Qty"])) ? Convert.ToInt32(reader["Qty"]) : nullInt,
                        ClassEx = Convert.ToString(reader["ClassEx"]),
                        SortNum = Convert.ToInt32(reader["SortNum"]),
                    };
                    libThreads.Add(libThread);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return libThreads;
        }
    }
}
