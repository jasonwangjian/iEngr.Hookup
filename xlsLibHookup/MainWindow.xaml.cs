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
        decimal? nullDecimal = null;
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
            string qty;
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
                    case "HKMatSubCat":
                        if (isDataExisting("HK_MatSubCat", (result as HKMatSubCat).ID))
                        {
                            sqlString = $"UPDATE HK_MatSubCat SET " +
                                $"CatID='{(result as HKMatSubCat).CatID}'," +
                                $"SubCatID='{(result as HKMatSubCat).SubCatID}'," +
                                $"SpecCn='{(result as HKMatSubCat).SpecCn}'," +
                                $"SpecEn='{(result as HKMatSubCat).SpecEn}'," +
                                $"Remarks='{(result as HKMatSubCat).Remarks}'," +
                                $"TypeP1='{(result as HKMatSubCat).TypeP1}'," +
                                $"TypeP2='{(result as HKMatSubCat).TypeP2}'," +
                                $"TechSpecMain='{(result as HKMatSubCat).TechSpecMain}'," +
                                $"TechSpecAux='{(result as HKMatSubCat).TechSpecAux}'," +
                                $"SortNum={(result as HKMatSubCat).SortNum} " +
                                $"WHERE ID='{(result as HKMatSubCat).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_MatSubCat (ID, CatID, SubCatID, SpecCn, SpecEn, Remarks, TypeP1, TypeP2, TechSpecMain, TechSpecAux, SortNum) VALUES (" +
                                $"'{(result as HKMatSubCat).ID}'," +
                                $"'{(result as HKMatSubCat).CatID}'," +
                                $"'{(result as HKMatSubCat).SubCatID}'," +
                                $"'{(result as HKMatSubCat).SpecCn}'," +
                                $"'{(result as HKMatSubCat).SpecEn}'," +
                                $"'{(result as HKMatSubCat).Remarks}'," +
                                $"'{(result as HKMatSubCat).TypeP1}'," +
                                $"'{(result as HKMatSubCat).TypeP2}'," +
                                $"'{(result as HKMatSubCat).TechSpecMain}'," +
                                $"'{(result as HKMatSubCat).TechSpecAux}'," +
                                $"{(result as HKMatSubCat).SortNum}" +
                                $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKLibPortType":
                        if (isDataExisting("HK_LibPortType", (result as HKLibPortType).ID))
                        {
                            sqlString = $"UPDATE HK_LibPortType SET " +
                                $"Class='{(result as HKLibPortType).Class}'," +
                                $"SubClass='{(result as HKLibPortType).SubClass}'," +
                                $"NameCn='{(result as HKLibPortType).NameCn}'," +
                                $"NameEn='{(result as HKLibPortType).NameEn}'," +
                                $"PrefixCn=N'{(result as HKLibPortType).PrefixCn}'," +
                                $"PrefixEn=N'{(result as HKLibPortType).PrefixEn}'," +
                                $"SuffixCn=N'{(result as HKLibPortType).SuffixCn}'," +
                                $"SuffixEn=N'{(result as HKLibPortType).SuffixEn}'," +
                                $"Link='{(result as HKLibPortType).Link}'," +
                                $"Remarks='{(result as HKLibPortType).Remarks}'," +
                                $"SortNum={(result as HKLibPortType).SortNum} " +
                                $"WHERE ID='{(result as HKLibPortType).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_LibPortType (ID, Class, SubClass, NameCn, NameEn, PrefixCn, PrefixEn, SuffixCn, SuffixEn, Link, Remarks, SortNum) VALUES (" +
                                $"'{(result as HKLibPortType).ID}'," +
                                $"'{(result as HKLibPortType).Class}'," +
                                $"'{(result as HKLibPortType).SubClass}'," +
                                $"'{(result as HKLibPortType).NameCn}'," +
                                $"'{(result as HKLibPortType).NameEn}'," +
                                $"N'{(result as HKLibPortType).PrefixCn}'," +
                                $"N'{(result as HKLibPortType).PrefixEn}'," +
                                $"N'{(result as HKLibPortType).SuffixCn}'," +
                                $"N'{(result as HKLibPortType).SuffixEn}'," +
                                $"'{(result as HKLibPortType).Link}'," +
                                $"'{(result as HKLibPortType).Remarks}'," +
                                $"{(result as HKLibPortType).SortNum} " +
                                $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKLibThread":
                         qty = ((result as HKLibThread).Qty == null) ? "Null" : (result as HKLibThread).Qty.ToString();
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
                    case "HKLibGland":
                        if (isDataExisting("HK_LibGland", (result as HKLibGland).ID))
                        {
                            sqlString = $"UPDATE HK_LibGland SET " +
                                $"Class='{(result as HKLibGland).Class}'," +
                                $"ClassEx='{(result as HKLibGland).ClassEx}'," +
                                $"SpecCn=N'{(result as HKLibGland).SpecCn}'," +
                                $"SpecEn=N'{(result as HKLibGland).SpecEn}'," +
                                $"CabODMin={(result as HKLibGland).CabODMin}," +
                                $"CabODMax={(result as HKLibGland).CabODMax}," +
                                $"SortNum={(result as HKLibGland).SortNum} " +
                                $"WHERE ID='{(result as HKLibGland).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_LibGland (ID, Class, ClassEx, SpecCn, SpecEn, CabODMin, CabODMax, SortNum) VALUES (" +
                                $"'{(result as HKLibGland).ID}'," +
                                $"'{(result as HKLibGland).Class}'," +
                                $"'{(result as HKLibGland).ClassEx}'," +
                                $"N'{(result as HKLibGland).SpecCn}'," +
                                $"N'{(result as HKLibGland).SpecEn}'," +
                                $"{(result as HKLibGland).CabODMin}," +
                                $"{(result as HKLibGland).CabODMax}," +
                                $"{(result as HKLibGland).SortNum} " +
                              $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKLibTubeOD":
                        if (isDataExisting("HK_LibTubeOD", (result as HKLibTubeOD).ID))
                        {
                            sqlString = $"UPDATE HK_LibTubeOD SET " +
                                $"Class='{(result as HKLibTubeOD).Class}'," +
                                $"SpecCn=N'{(result as HKLibTubeOD).SpecCn}'," +
                                $"SpecEn=N'{(result as HKLibTubeOD).SpecEn}'," +
                                $"ValueM={(result as HKLibTubeOD).ValueM}," +
                                $"ClassEx='{(result as HKLibTubeOD).ClassEx}'," +
                                $"SortNum={(result as HKLibTubeOD).SortNum} " +
                                $"WHERE ID='{(result as HKLibTubeOD).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_LibTubeOD (ID, Class, SpecCn, SpecEn, ValueM, ClassEx, SortNum) VALUES (" +
                                $"'{(result as HKLibTubeOD).ID}'," +
                                $"'{(result as HKLibTubeOD).Class}'," +
                                $"N'{(result as HKLibTubeOD).SpecCn}'," +
                                $"N'{(result as HKLibTubeOD).SpecEn}'," +
                                $"{(result as HKLibTubeOD).ValueM}," +
                                $"'{(result as HKLibTubeOD).ClassEx}'," +
                                $"{(result as HKLibTubeOD).SortNum} " +
                              $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKLibPipeOD":
                        string hGIa = ((result as HKLibPipeOD).HGIa == null) ? "Null" : (result as HKLibPipeOD).HGIa.ToString();
                        string hGIb = ((result as HKLibPipeOD).HGIb == null) ? "Null" : (result as HKLibPipeOD).HGIb.ToString();
                        string hGII = ((result as HKLibPipeOD).HGII == null) ? "Null" : (result as HKLibPipeOD).HGII.ToString();
                        string gBI = ((result as HKLibPipeOD).GBI == null) ? "Null" : (result as HKLibPipeOD).GBI.ToString();
                        string gBII = ((result as HKLibPipeOD).GBII == null) ? "Null" : (result as HKLibPipeOD).GBII.ToString();
                        string iSO = ((result as HKLibPipeOD).ISO == null) ? "Null" : (result as HKLibPipeOD).ISO.ToString();
                        string aSME = ((result as HKLibPipeOD).ASME == null) ? "Null" : (result as HKLibPipeOD).ASME.ToString();
                        string sWDiaGB = ((result as HKLibPipeOD).SWDiaGB == null) ? "Null" : (result as HKLibPipeOD).SWDiaGB.ToString();
                        if (isDataExisting("HK_LibPipeOD", (result as HKLibPipeOD).ID))
                        {
                            sqlString = $"UPDATE HK_LibPipeOD SET " +
                                $"DN='{(result as HKLibPipeOD).DN}'," +
                                $"NPS=N'{(result as HKLibPipeOD).NPS}'," +
                                $"HGIa={hGIa}," +
                                $"HGIb={hGIb}," +
                                $"HGII={hGII}," +
                                $"GBI={gBI}," +
                                $"GBII={gBII}," +
                                $"ISO={iSO}," +
                                $"ASME={aSME}," +
                                $"SWDiaGB={sWDiaGB}," +
                                $"SpecRem='{(result as HKLibPipeOD).SpecRem}'," +
                                $"SortNum={(result as HKLibPipeOD).SortNum} " +
                                $"WHERE ID='{(result as HKLibPipeOD).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_LibPipeOD (ID, DN, NPS, HGIa, HGIb, HGII, GBI, GBII, ISO, ASME, SWDiaGB, SpecRem, SortNum) VALUES (" +
                                $"'{(result as HKLibPipeOD).ID}'," +
                                $"'{(result as HKLibPipeOD).DN}'," +
                                $"N'{(result as HKLibPipeOD).NPS}'," +
                                $"{hGIa}," +
                                $"{hGIb}," +
                                $"{hGII}," +
                                $"{gBI}," +
                                $"{gBII}," +
                                $"{iSO}," +
                                $"{aSME}," +
                                $"{sWDiaGB}," +
                                $"'{(result as HKLibPipeOD).SpecRem}'," +
                                $"{(result as HKLibPipeOD).SortNum}" +
                                $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKLibSpecDic":
                        if (isDataExisting("HK_LibSpecDic", (result as HKLibSpecDic).ID))
                        {
                            sqlString = $"UPDATE HK_LibSpecDic SET " +
                                $"Class='{(result as HKLibSpecDic).Class}'," +
                                $"NameCn='{(result as HKLibSpecDic).NameCn}'," +
                                $"NameEn='{(result as HKLibSpecDic).NameEn}'," +
                                $"PrefixCn=N'{(result as HKLibSpecDic).PrefixCn}'," +
                                $"PrefixEn=N'{(result as HKLibSpecDic).PrefixEn}'," +
                                $"SuffixCn=N'{(result as HKLibSpecDic).SuffixCn}'," +
                                $"SuffixEn=N'{(result as HKLibSpecDic).SuffixEn}'," +
                                $"Link='{(result as HKLibSpecDic).Link}'," +
                                $"SortNum={(result as HKLibSpecDic).SortNum} " +
                                $"WHERE ID='{(result as HKLibSpecDic).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_LibSpecDic (ID, Class, NameCn, NameEn, PrefixCn, PrefixEn, SuffixCn, SuffixEn, Link, SortNum) VALUES (" +
                                $"'{(result as HKLibSpecDic).ID}'," +
                                $"'{(result as HKLibSpecDic).Class}'," +
                                $"'{(result as HKLibSpecDic).NameCn}'," +
                                $"'{(result as HKLibSpecDic).NameEn}'," +
                                $"N'{(result as HKLibSpecDic).PrefixCn}'," +
                                $"N'{(result as HKLibSpecDic).PrefixEn}'," +
                                $"N'{(result as HKLibSpecDic).SuffixCn}'," +
                                $"N'{(result as HKLibSpecDic).SuffixEn}'," +
                                $"'{(result as HKLibSpecDic).Link}'," +
                                $"{(result as HKLibSpecDic).SortNum} " +
                                $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKLibGenOption":
                        string inact = ((result as HKLibGenOption).Inact == null) ? "Null" : (result as HKLibGenOption).Inact.ToString();
                        if (isDataExisting("HK_LibGenOption", (result as HKLibGenOption).ID))
                        {
                            sqlString = $"UPDATE HK_LibGenOption SET " +
                                $"Cat='{(result as HKLibGenOption).Cat}'," +
                                $"NameCn=N'{(result as HKLibGenOption).NameCn}'," +
                                $"NameEn=N'{(result as HKLibGenOption).NameEn}'," +
                                $"SpecCn=N'{(result as HKLibGenOption).SpecCn}'," +
                                $"SpecEn=N'{(result as HKLibGenOption).SpecEn}'," +
                                $"Inact={inact}," +
                                $"SortNum={(result as HKLibGenOption).SortNum} " +
                                $"WHERE ID='{(result as HKLibGenOption).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_LibGenOption (ID, Cat, NameCn, NameEn, SpecCn, SpecEn, Inact, SortNum) VALUES (" +
                                $"'{(result as HKLibGenOption).ID}'," +
                                $"'{(result as HKLibGenOption).Cat}'," +
                                $"N'{(result as HKLibGenOption).NameCn}'," +
                                $"N'{(result as HKLibGenOption).NameEn}'," +
                                $"N'{(result as HKLibGenOption).SpecCn}'," +
                                $"N'{(result as HKLibGenOption).SpecEn}'," +
                                $"{inact}," +
                                $"{(result as HKLibGenOption).SortNum} " +
                                $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKLibSteel":
                        string cSb = ((result as HKLibSteel).CSb == null) ? "Null" : (result as HKLibSteel).CSb.ToString();
                        string cSd = ((result as HKLibSteel).CSd == null) ? "Null" : (result as HKLibSteel).CSd.ToString();
                        string iBb = ((result as HKLibSteel).IBb == null) ? "Null" : (result as HKLibSteel).IBb.ToString();
                        string iBd = ((result as HKLibSteel).IBd == null) ? "Null" : (result as HKLibSteel).IBd.ToString();
                        if (isDataExisting("HK_LibSteel", (result as HKLibSteel).ID))
                        {
                            sqlString = $"UPDATE HK_LibSteel SET " +
                                $"CSSpecCn=N'{(result as HKLibSteel).CSSpecCn}'," +
                                $"CSSpecEn=N'{(result as HKLibSteel).CSSpecEn}'," +
                                $"IBSpecCn=N'{(result as HKLibSteel).IBSpecCn}'," +
                                $"IBSpecEn=N'{(result as HKLibSteel).IBSpecEn}'," +
                                $"Width={(result as HKLibSteel).Width}," +
                                $"CSb={cSb}," +
                                $"CSd={cSd}," +
                                $"IBb={iBb}," +
                                $"IBd={iBd}," +
                                $"SortNum={(result as HKLibSteel).SortNum} " +
                                $"WHERE ID='{(result as HKLibSteel).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_LibSteel (ID, CSSpecCn, CSSpecEn, IBSpecCn, IBSpecEn, Width, CSb, CSd, IBb, IBd, SortNum) VALUES (" +
                                $"'{(result as HKLibSteel).ID}'," +
                                $"N'{(result as HKLibSteel).CSSpecCn}'," +
                                $"N'{(result as HKLibSteel).CSSpecEn}'," +
                                $"N'{(result as HKLibSteel).IBSpecCn}'," +
                                $"N'{(result as HKLibSteel).IBSpecEn}'," +
                                $"{(result as HKLibSteel).Width}," +
                                $"{cSb}," +
                                $"{cSd}," +
                                $"{iBb}," +
                                $"{iBd}," +
                                $"{(result as HKLibSteel).SortNum}" +
                                $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKLibPN":
                        if (isDataExisting("HK_LibPN", (result as HKLibPN).ID))
                        {
                            sqlString = $"UPDATE HK_LibPN SET " +
                                $"Class='{(result as HKLibPN).Class}'," +
                                $"SpecCn='{(result as HKLibPN).SpecCn}'," +
                                $"SpecEn='{(result as HKLibPN).SpecEn}'," +
                                $"ISOS1='{(result as HKLibPN).ISOS1}'," +
                                $"ISOS2='{(result as HKLibPN).ISOS2}'," +
                                $"GBDIN='{(result as HKLibPN).GBDIN}'," +
                                $"GBANSI='{(result as HKLibPN).GBANSI}'," +
                                $"ASME='{(result as HKLibPN).ASME}'," +
                                $"SortNum={(result as HKLibPN).SortNum} " +
                                $"WHERE ID='{(result as HKLibPN).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_LibPN (ID, Class, SpecCn, SpecEn, ISOS1, ISOS2, GBDIN, GBANSI, ASME, SortNum) VALUES (" +
                                $"'{(result as HKLibPN).ID}'," +
                                $"'{(result as HKLibPN).Class}'," +
                                $"'{(result as HKLibPN).SpecCn}'," +
                                $"'{(result as HKLibPN).SpecEn}'," +
                                $"'{(result as HKLibPN).ISOS1}'," +
                                $"'{(result as HKLibPN).ISOS2}'," +
                                $"'{(result as HKLibPN).GBDIN}'," +
                                $"'{(result as HKLibPN).GBANSI}'," +
                                $"'{(result as HKLibPN).ASME}'," +
                                $"{(result as HKLibPN).SortNum} " +
                                $")";
                        }
                        count = count + updateData(sqlString);
                        break;
                    case "HKMatGenLib":
                        if (isDataExisting("HK_MatGenLib", (result as HKMatGenLib).ID))
                        {
                            sqlString = $"UPDATE HK_MatGenLib SET " +
                                $"CatID='{(result as HKMatGenLib).CatID}'," +
                                $"SubCatID='{(result as HKMatGenLib).SubCatID}'," +
                                $"TechSpecMain='{(result as HKMatGenLib).TechSpecMain}'," +
                                $"TechSpecAux='{(result as HKMatGenLib).TechSpecAux}'," +
                                $"TypeP1='{(result as HKMatGenLib).TypeP1}'," +
                                $"SizeP1='{(result as HKMatGenLib).SizeP1}'," +
                                $"TypeP2='{(result as HKMatGenLib).TypeP2}'," +
                                $"SizeP2='{(result as HKMatGenLib).SizeP2}'," +
                                $"MatSpec='{(result as HKMatGenLib).MatSpec}'," +
                                $"PClass='{(result as HKMatGenLib).PClass}'," +
                                $"MoreSpecCn='{(result as HKMatGenLib).MoreSpecCn}'," +
                                $"MoreSpecEn='{(result as HKMatGenLib).MoreSpecEn}'," +
                                $"AppStd='{(result as HKMatGenLib).AppStd}'," +
                                $"RemarksCn='{(result as HKMatGenLib).RemarksCn}'," +
                                $"RemarksEn='{(result as HKMatGenLib).RemarksEn}'," +
                                $"Comments='{(result as HKMatGenLib).Comments}' " +
                                $"WHERE ID='{(result as HKMatGenLib).ID}'";
                        }
                        else
                        {
                            sqlString = $"INSERT INTO HK_MatGenLib (ID, CatID, SubCatID, TechSpecMain, TechSpecAux, TypeP1, SizeP1, TypeP2, SizeP2, MatSpec, PClass, MoreSpecCn, MoreSpecEn, AppStd, RemarksCn, RemarksEn, Comments) VALUES (" +
                                $"'{(result as HKMatGenLib).ID}'," +
                                $"'{(result as HKMatGenLib).CatID}'," +
                                $"'{(result as HKMatGenLib).SubCatID}'," +
                                $"'{(result as HKMatGenLib).TechSpecMain}'," +
                                $"'{(result as HKMatGenLib).TechSpecAux}'," +
                                $"'{(result as HKMatGenLib).TypeP1}'," +
                                $"'{(result as HKMatGenLib).SizeP1}'," +
                                $"'{(result as HKMatGenLib).TypeP2}'," +
                                $"'{(result as HKMatGenLib).SizeP2}'," +
                                $"'{(result as HKMatGenLib).MatSpec}'," +
                                $"'{(result as HKMatGenLib).PClass}'," +
                                $"'{(result as HKMatGenLib).MoreSpecCn}'," +
                                $"'{(result as HKMatGenLib).MoreSpecEn}'," +
                                $"'{(result as HKMatGenLib).AppStd}'," +
                                $"'{(result as HKMatGenLib).RemarksCn}'," +
                                $"'{(result as HKMatGenLib).RemarksEn}'," +
                                $"'{(result as HKMatGenLib).Comments}' " +
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
                        PrefixCn = Convert.ToString(reader["PrefixCn"]),
                        PrefixEn = Convert.ToString(reader["PrefixEn"]),
                        SuffixCn = Convert.ToString(reader["SuffixCn"]),
                        SuffixEn = Convert.ToString(reader["SuffixEn"]),
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
        private ObservableCollection<HKLibGland> GetXlsLibGland(string id = null)
        {
            ObservableCollection<HKLibGland> libGlands = new ObservableCollection<HKLibGland>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [LibGland$]"
                                     : $"select * from [LibGland$] where ID = '{id}'";
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
                    HKLibGland libGland = new HKLibGland
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Class = Convert.ToString(reader["Class"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        CabODMin = Convert.ToDecimal(reader["CabODMin"]),
                        CabODMax = Convert.ToDecimal(reader["CabODMax"]),
                        ClassEx = Convert.ToString(reader["ClassEx"]),
                        SortNum = Convert.ToInt32(reader["SortNum"]),
                    };
                    libGlands.Add(libGland);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return libGlands;
        }
        private ObservableCollection<HKLibTubeOD> GetXlsLibTubeOD(string id = null)
        {
            ObservableCollection<HKLibTubeOD> libTubeODs = new ObservableCollection<HKLibTubeOD>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [LibTubeOD$]"
                                     : $"select * from [LibTubeOD$] where ID = '{id}'";
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
                    HKLibTubeOD libTubeOD = new HKLibTubeOD
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Class = Convert.ToString(reader["Class"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        ValueM = Convert.ToDecimal(reader["ValueM"]),
                        ClassEx = Convert.ToString(reader["ClassEx"]),
                        SortNum = Convert.ToInt32(reader["SortNum"]),
                    };
                    libTubeODs.Add(libTubeOD);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return libTubeODs;
        }
        private ObservableCollection<HKLibPipeOD> GetXlsLibPipeOD(string id = null)
        {
            ObservableCollection<HKLibPipeOD> libPipeODs = new ObservableCollection<HKLibPipeOD>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [LibPipeOD$]"
                                     : $"select * from [LibPipeOD$] where ID = '{id}'";
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
                    HKLibPipeOD libPipeOD = new HKLibPipeOD
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
                    };
                    libPipeODs.Add(libPipeOD);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return libPipeODs;
        }
        private ObservableCollection<HKLibSpecDic> GetXlsLibSpecDic(string id = null)
        {
            ObservableCollection<HKLibSpecDic> data = new ObservableCollection<HKLibSpecDic>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [LibSpecDic$]"
                                      : $"select * from [LibSpecDic$] where ID = '{id}'";
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
                    HKLibSpecDic item = new HKLibSpecDic
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Class = Convert.ToString(reader["Class"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        PrefixCn = Convert.ToString(reader["PrefixCn"]),
                        PrefixEn = Convert.ToString(reader["PrefixCn"]),
                        SuffixCn = Convert.ToString(reader["SuffixCn"]),
                        SuffixEn = Convert.ToString(reader["SuffixEn"]),
                        Link = Convert.ToString(reader["Link"]),
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
        private ObservableCollection<HKLibGenOption> GetXlsLibGenOption(string id = null)
        {
            ObservableCollection<HKLibGenOption> data = new ObservableCollection<HKLibGenOption>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [LibGenOption$]"
                                      : $"select * from [LibGenOption$] where ID = '{id}'";
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
                    HKLibGenOption item = new HKLibGenOption
                    {
                        ID = Convert.ToString(reader["ID"]),
                        Cat = Convert.ToString(reader["Cat"]),
                        NameCn = Convert.ToString(reader["NameCn"]),
                        NameEn = Convert.ToString(reader["NameEn"]),
                        SpecCn = Convert.ToString(reader["SpecCn"]),
                        SpecEn = Convert.ToString(reader["SpecEn"]),
                        Inact = !string.IsNullOrEmpty(Convert.ToString(reader["Inact"])) ? Convert.ToInt32(reader["Inact"]) : nullInt,
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
        private ObservableCollection<HKLibSteel> GetXlsLibSteel(string id = null)
        {
            ObservableCollection<HKLibSteel> libSteels = new ObservableCollection<HKLibSteel>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [LibSteel$]"
                                     : $"select * from [LibSteel$] where ID = '{id}'";
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
                    HKLibSteel libSteel = new HKLibSteel
                    {
                        ID = Convert.ToString(reader["ID"]),
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
                    };
                    libSteels.Add(libSteel);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
            }
            return libSteels;
        }
        private ObservableCollection<HKLibPN> GetXlsLibPN(string id = null)
        {
            ObservableCollection<HKLibPN> data = new ObservableCollection<HKLibPN>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [LibPN$]"
                                      : $"select * from [LibPN$] where ID = '{id}'";
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
                    HKLibPN item = new HKLibPN
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
        private ObservableCollection<HKMatGenLib> GetXlsMatGenLib(string id = null)
        {
            ObservableCollection<HKMatGenLib> data = new ObservableCollection<HKMatGenLib>();
            // 构建 SQL 查询语句
            string query = (id == null) ? "select * from [MatGenLib$]"
                                      : $"select * from [MatGenLib$] where ID = '{id}'";
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
                    HKMatGenLib item = new HKMatGenLib
                    {
                        ID = Convert.ToString(reader["ID"]),
                        CatID = Convert.ToString(reader["CatID"]),
                        SubCatID = Convert.ToString(reader["SubCatID"]),
                        TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                        TechSpecAux = Convert.ToString(reader["TechSpecAux"]),
                        TypeP1 = Convert.ToString(reader["TypeP1"]),
                        SizeP1 = Convert.ToString(reader["SizeP1"]),
                        TypeP2 = Convert.ToString(reader["TypeP2"]),
                        SizeP2 = Convert.ToString(reader["SizeP2"]),
                        MatSpec = Convert.ToString(reader["MatSpec"]),
                        PClass = Convert.ToString(reader["PClass"]),
                        MoreSpecCn = Convert.ToString(reader["MoreSpecCn"]),
                        MoreSpecEn = Convert.ToString(reader["MoreSpecEn"]),
                        AppStd = Convert.ToString(reader["AppStd"]),
                        RemarksCn = Convert.ToString(reader["RemarksCn"]),
                        RemarksEn = Convert.ToString(reader["RemarksEn"]),
                        Comments = Convert.ToString(reader["Comments"])
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
        private void btnLibGland_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsLibGland();
        }
        private void btnLibTubeOD_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsLibTubeOD();
        }
        private void btnLibPipeOD_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsLibPipeOD();
        }
        private void btnLibSpecDic_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsLibSpecDic();
        }
        private void btnLibGenOption_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsLibGenOption();
        }
        private void btnLibSteel_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsLibSteel();
        }
        private void btnLibPN_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsLibPN();
        }
        private void btnMatGenLib_Click(object sender, RoutedEventArgs e)
        {
            dgResult.ItemsSource = GetXlsMatGenLib();
        }
    }
}
