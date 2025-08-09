using System;
using System.Collections.Generic;
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
using System.Xml.Linq;

namespace iEngr.Hookup
{
    /// <summary>
    /// Database Handle
    /// </summary>
    public partial class HK_LibMat
    {
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
                        Class = Convert.ToString(reader["Class"]),
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
    }
}
