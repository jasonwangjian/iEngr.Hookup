using iEngr.Hookup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace xlsLibHookup
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        int? nullInt = null;
        private static OdbcConnection xlsConn;
        private static OdbcConnection conn;
        private ObservableCollection<HKLibThread> hKLibThreads;
        public MainWindow()
        {
            InitializeComponent();
            xlsConn = GetXlsConnection();
            conn = GetConnection();
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
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            string id;
            string sqlString;
            foreach (Object result in dgResult.ItemsSource)
            {
                switch (result.GetType()?.Name)
                {
                    case "HKMatMainCat":
                        if (isDataExisting("HK_MatMainCat", (result as HKMatMainCat).ID))
                        {
                            sqlString = $"UPDATE HK_MatMainCat SET " +
                                $"NameCn='{(result as HKMatMainCat).NameCn}'," +
                                $"NameEn='{(result as HKMatMainCat).NameEn}'," +
                                $"Remarks='{(result as HKMatMainCat).Remarks}'," +
                                $"SortNum={(result as HKMatMainCat).SortNum} " +
                                $"WHERE ID='{(result as HKMatMainCat).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_MatMainCat (ID, NameCn, NameEn, Remarks, SortNum) VALUES (" +
                                $"'{(result as HKMatMainCat).ID}'," +
                                $"'{(result as HKMatMainCat).NameCn}'," +
                                $"'{(result as HKMatMainCat).NameEn}'," +
                                $"'{(result as HKMatMainCat).Remarks}'," +
                                $"{(result as HKMatMainCat).SortNum}" +
                                $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKLibThread":
                            string qty = ((result as HKLibThread).Qty == null) ? "Null" : (result as HKLibThread).Qty.ToString();
                        if (isDataExisting("HK_LibThread", (result as HKLibThread).ID))
                        {
                            sqlString = $"UPDATE HK_LibThread SET " +
                                $"Class='{(result as HKLibThread).Class}'," +
                                $"SubClass='{(result as HKLibThread).SubClass}'," +
                                $"ClassEx='{(result as HKLibThread).ClassEx}'," +
                                $"SpecCn=N'{(result as HKLibThread).SpecCn}'," +
                                $"SpecEn=N'{(result as HKLibThread).SpecEn}'," +
                                $"Value={(result as HKLibThread).Value}," +
                                $"Pitch={(result as HKLibThread).Pitch}," +
                                $"Qty={qty}," +
                                $"SortNum={(result as HKLibThread).SortNum} " +
                                $"WHERE ID='{(result as HKLibThread).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_LibThread (ID, Class, SubClass, ClassEx, SpecCn, SpecEn, Value, Pitch, Qty, SortNum) VALUES (" +
                                $"'{(result as HKLibThread).ID}'," +
                                $"'{(result as HKLibThread).Class}'," +
                                $"'{(result as HKLibThread).SubClass}'," +
                                $"'{(result as HKLibThread).ClassEx}'," +
                                $"N'{(result as HKLibThread).SpecCn}'," +
                                $"N'{(result as HKLibThread).SpecEn}'," +
                                $"{(result as HKLibThread).Value}," +
                                $"{(result as HKLibThread).Pitch}," +
                                $"{qty}," +
                                $"{(result as HKLibThread).SortNum}" +
                              $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                }
                //Type type = result.GetType();

                //foreach (PropertyInfo property in type.GetProperties())
                //{
                //    if (!property.CanRead || !property.CanWrite || property.GetIndexParameters().Length > 0 ||
                //        !property.PropertyType.IsPrimitive && property.PropertyType != typeof(string) && property.PropertyType != typeof(DateTime))
                //        continue;

                //    try
                //    {
                //        string field = property.Name;
                //        object value = property.GetValue(result);

                //        Debug.Print(field);
                //    }
                //    catch
                //    {
                //        // 忽略错误
                //    }
                //}
            }
            //return count;
        }
        private int updateData(string query)
        {
             try
            {
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
                Console.WriteLine($"Error occurred: {ex.Message}");
                return 0;
            }
        }
        private bool isDataExisting(string tableName, string id)
        {
            string query = $"SELECT COUNT(*) FROM {tableName} WHERE ID = '{id}'";
            try
            {
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
                Console.WriteLine($"Error occurred: {ex.Message}");
                return false;
            }
        }
        private ObservableCollection<HKMatMainCat> GetXlsMatMainCat(string id = null)
        {
            ObservableCollection<HKMatMainCat> data = new ObservableCollection<HKMatMainCat>();
            // 构建 SQL 查询语句
            string query = (id == null)? "select * from [MatMainCat$]"
                                       : $"select * from [MatMainCat$] where ID = '{id}'";
            try
            {
                if (xlsConn == null || xlsConn.State != ConnectionState.Open)
                    xlsConn = GetXlsConnection();
                OdbcCommand command = new OdbcCommand(query, xlsConn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (string.IsNullOrEmpty(Convert.ToString(reader["ID"]).Trim()))
                        break;
                    HKMatMainCat item = new HKMatMainCat
                    {
                        ID = Convert.ToString(reader["ID"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        Remarks = Convert.ToString(reader["Remarks"]),
                        SortNum = Convert.ToInt32(reader["SortNum"]),
                    };
                    data.Add(item);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return data;
        }
        private ObservableCollection<HKMatSubCat> GetXlsMatSubCat(string id = null)
        {
            ObservableCollection<HKMatSubCat> data = new ObservableCollection<HKMatSubCat>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [MatSubCat$]"
                                      : $"select * from [MatSubCat$] where ID = '{id}'";
            try
            {
                if (xlsConn == null || xlsConn.State != ConnectionState.Open)
                    xlsConn = GetXlsConnection();
                OdbcCommand command = new OdbcCommand(query, xlsConn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (string.IsNullOrEmpty(Convert.ToString(reader["ID"]).Trim()))
                        break;
                    HKMatSubCat item = new HKMatSubCat
                    {
                        ID = Convert.ToString(reader["ID"]),
                        CatID = Convert.ToString(reader["CatID"]),
                        SubCatID = Convert.ToString(reader["SubCatID"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Remarks = Convert.ToString(reader["Remarks"]),
                        SortNum = Convert.ToInt32(reader["SortNum"]),
                        TypeP1 = Convert.ToString(reader["TypeP1"]),
                        TypeP2 = Convert.ToString(reader["TypeP2"]),
                        TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                        TechSpecAux = Convert.ToString(reader["TechSpecAux"]),
                    };
                    data.Add(item);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return data;
        }
        private ObservableCollection<HKLibPortType> GetXlsLibPortType(string id = null)
        {
            ObservableCollection<HKLibPortType> data = new ObservableCollection<HKLibPortType>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [LibPortType$]"
                                      : $"select * from [LibPortType$] where ID = '{id}'";
            try
            {
                if (xlsConn == null || xlsConn.State != ConnectionState.Open)
                    xlsConn = GetXlsConnection();
                OdbcCommand command = new OdbcCommand(query, xlsConn);
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (string.IsNullOrEmpty(Convert.ToString(reader["ID"]).Trim()))
                        break;
                    HKLibPortType item = new HKLibPortType
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Class = Convert.ToString(reader["Class"]),
                        SubClass = Convert.ToString(reader["SubClass"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Link = Convert.ToString(reader["Link"]),
                         SortNum = Convert.ToInt32(reader["SortNum"]),
                        Remarks = Convert.ToString(reader["Remarks"]),
                   };
                    data.Add(item);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return data;
        }
        private ObservableCollection<HKLibThread> GetXlsLibThread(string id = null)
        {
            ObservableCollection<HKLibThread> libThreads = new ObservableCollection<HKLibThread>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [LibThread$]"
                                     : $"select * from [LibThread$] where ID = '{id}'";
            try
            {
                if (xlsConn == null || xlsConn.State != ConnectionState.Open)
                    xlsConn = GetXlsConnection();
                OdbcCommand command = new OdbcCommand(query, xlsConn);
                OdbcDataReader reader = command.ExecuteReader();
                 while (reader.Read())
                {
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

 
        private void btnMainCat_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsMatMainCat();
        }
        private void btnSubCat_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsMatSubCat();
        }
        private void btnLibPortType_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsLibPortType();
        }
        private void btnLibThread_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsLibThread();
        }
    }
}
