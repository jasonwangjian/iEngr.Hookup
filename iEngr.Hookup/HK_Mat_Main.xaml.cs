using iEngr.Hookup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
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
        private ObservableCollection<HKMatSubCat> hKMatSubCats = new ObservableCollection<HKMatSubCat>
            {
                new HKMatSubCat
                {
                ID = "%",
                NameCn = "所有小类",
                NameEn = "All Sub Categories"
                }
            };
        private ObservableCollection<HKPortType> hKPortTypesP1 = new ObservableCollection<HKPortType>
            {
                new HKPortType
                {
                ID = "%",
                NameCn = "所有连接类型",
                NameEn = "All Connections"
                }
            };
        public HK_Mat_Main()
        {
            InitializeComponent();
            intLan = 1;
            conn = GetConnection();
            hKMatMainCats = GetHKMatMainCats();
            cbMainCat.ItemsSource = hKMatMainCats;
            cbMainCat.DisplayMemberPath = "Name";
            cbSubCat.ItemsSource = hKMatSubCats;
            cbSubCat.DisplayMemberPath = "Name";
            cbTypeP1.ItemsSource = hKPortTypesP1;
            cbTypeP1.DisplayMemberPath = "Name";
            cbMainCat.SelectedIndex = 0;
       }

 
        private void btnToBeChk_Click(object sender, RoutedEventArgs e)
        {
            string test = GeneralFun.ConvertToSqlInString("  FL, BW, SW , ");

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
            string strTypeP1 = (cbSubCat.SelectedItem as HKMatSubCat)?.TypeP1;


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
        private ObservableCollection<HKPortType> GetHKPortTypes(string strType)
        {
            ObservableCollection<HKPortType> portTypes = new ObservableCollection<HKPortType>
            {
                new HKPortType
                {
                ID = "%",
                NameCn = "所有连接类型",
                NameEn = "All Connections"
                }
            };

            // 构建 SQL 查询语句
            string query = "select * from HK_LibPortType where OrderNum < 101 and " + GeneralFun.ConvertToSqlInString(strType) + " order by OrderNum ID";
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
                    conn = GetConnection();
                OdbcCommand command = new OdbcCommand(query, conn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HKPortType portType = new HKPortType
                    {
                        ID = Convert.ToString(reader["ID"]),
                        NameCn = Convert.ToString(reader["SpecCn"]),
                        NameEn = Convert.ToString(reader["SpecEn"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Remarks = Convert.ToString(reader["Remarks"]),
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

        private void cbTypeP1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
