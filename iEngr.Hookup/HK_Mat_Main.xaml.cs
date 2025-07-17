using iEngr.Hookup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Linq;
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
        private static OdbcConnection xlsConn;
        private ObservableCollection<HKMatMainCat> hKMatMainCats;
        private ObservableCollection<HKLibThread> hKLibThreads;
        private ObservableCollection<HKMatSubCat> hKMatSubCats = new ObservableCollection<HKMatSubCat>
            {
                new HKMatSubCat
                {
                ID = "%",
                NameCn = "所有小类",
                NameEn = "All Sub Categories"
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
        private string strTypeP1;
        private string strTypeP2;   
        public HK_Mat_Main()
        {
            InitializeComponent();
            intLan = 0; // 0: 中文； 其它为英文
            conn = GetConnection();
            xlsConn = GetXlsConnection();
            hKMatMainCats = GetHKMatMainCats();
            cbMainCat.ItemsSource = hKMatMainCats;
            cbMainCat.DisplayMemberPath = "Name";
            cbSubCat.ItemsSource = hKMatSubCats;
            cbSubCat.DisplayMemberPath = "Name";
            cbMainCat.SelectedIndex = 0;

            hKLibThreads = GetXlsLibThread();
            dgResult.ItemsSource = hKLibThreads;
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
            if (cbSubCat.SelectedItem != null && cbSubCat.SelectedIndex != 0)
            {
                strTypeP1 = (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP1;
            }
            hKPortTypesP1 = GetHKPortTypes(strTypeP1, 1);
            cbTypeP1.ItemsSource = hKPortTypesP1;
            cbTypeP1.DisplayMemberPath = "Name";
            cbTypeP1.SelectedIndex = 0;

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
                    NameCn = mainCat.ID == "%" ? "所有小类" : "所有" + mainCat.Name,
                    NameEn = mainCat.ID == "%" ? "All Sub Categories" : "All " + mainCat.Name
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
                        NameCn = Convert.ToString(reader["SpecCn"]),
                        NameEn = Convert.ToString(reader["SpecEn"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Remarks = Convert.ToString(reader["Remarks"]),
                        TypeP1 = Convert.ToString(reader["TypeP1"]),
                        TypeP2 = Convert.ToString(reader["TypeP2"]),
                        TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                        TechSpecAux = Convert.ToString(reader["TechSpecAux"])
                    };
                    strTypeP1 += Convert.ToString(reader["TypeP1"]);
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
        private ObservableCollection<HKLibPortType> GetHKPortTypes(string strType, int intPort)
        {
            ObservableCollection<HKLibPortType> portTypes = (intPort == 1) ? hKPortTypesP1 : hKPortTypesP2;
            portTypes.Clear();
            portTypes.Add(
                new HKLibPortType
                {
                ID = "%",
                NameCn = "所有连接类型",
                NameEn = "All Connections"
                }
            );

            // 构建 SQL 查询语句
            string query = "select * from HK_LibPortType where OrderNum < 101 and ID in " + GeneralFun.ConvertToStringScope(strType, ',') + " order by OrderNum";
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
                if (portTypes.Count == 1 || portTypes.Count == 2 && portTypes.ElementAt(1).ID == "NA")
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
                }
                 if (portTypes.Count == 2 && portTypes.ElementAt(1).ID == "IS")
                {
                    portTypes.RemoveAt(0);
                }
           }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return portTypes;
        }

        private void cbTypeP1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private static OdbcConnection GetXlsConnection()
        {
            try
            {
                // 定义 DSN 名称
                string dsnName = "LibHookup"; //"ComosExt";

                // 创建 OdbcConnection 对象并传入 DSN 连接字符串
                OdbcConnection connection = new OdbcConnection($"DSN={dsnName}");

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
        private ObservableCollection<HKLibThread> GetXlsLibThread()
        {
            ObservableCollection<HKLibThread> libThreads = new ObservableCollection<HKLibThread>();
            // 构建 SQL 查询语句
            string query = "select * from [LibThread$]";
            try
            {
                if (xlsConn == null || xlsConn.State != ConnectionState.Open)
                    xlsConn = GetXlsConnection();
                OdbcCommand command = new OdbcCommand(query, xlsConn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HKLibThread libThread = new HKLibThread
                    {
                        ID = Convert.ToString(reader["ID"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
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
