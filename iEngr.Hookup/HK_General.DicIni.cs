using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class HK_General
    {
        int? nullInt = null;
        decimal? nullDecimal = null;
        internal Dictionary<string, HKMatSubCat> dicSubCat = new Dictionary<string, HKMatSubCat>();
        internal Dictionary<string, HKLibPortType> dicPortType = new Dictionary<string, HKLibPortType>();
        internal Dictionary<string, HKLibGenOption> dicGenOption = new Dictionary<string, HKLibGenOption>();
        internal Dictionary<string, HKLibGland> dicGland = new Dictionary<string, HKLibGland>();
        internal Dictionary<string, HKLibPipeOD> dicPipeOD = new Dictionary<string, HKLibPipeOD>();
        internal Dictionary<string, HKLibPN> dicPN = new Dictionary<string, HKLibPN>();
        internal Dictionary<string, HKLibSpecDic> dicSpecDic = new Dictionary<string, HKLibSpecDic>();
        internal Dictionary<string, HKLibSteel> dicSteel = new Dictionary<string, HKLibSteel>();
        internal Dictionary<string, HKLibThread> dicThread = new Dictionary<string, HKLibThread>();
        internal Dictionary<string, HKLibTubeOD> dicTubeOD = new Dictionary<string, HKLibTubeOD>();
        internal Dictionary<string, ObservableCollection<HKLibGenOption>> dicNoLinkSpec = new Dictionary<string, ObservableCollection<HKLibGenOption>>();
        private void dicSubCatIni()
        {
            dicSubCat.Clear();
            string query = "select * from HK_MatSubCat order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dicSubCat.Add(Convert.ToString(reader["ID"]), new HKMatSubCat
                            {
                                ID = Convert.ToString(reader["ID"]),
                                SpecCn = Convert.ToString(reader["SpecCn"]),
                                SpecEn = Convert.ToString(reader["SpecEn"]),
                                TypeP1 = Convert.ToString(reader["TypeP1"]),
                                TypeP2 = Convert.ToString(reader["TypeP2"]),
                                TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                                TechSpecAux = Convert.ToString(reader["TechSpecAux"]),
                                CatID = Convert.ToString(reader["CatID"]),
                                Remarks = Convert.ToString(reader["Remarks"]),
                                SortNum = Convert.ToInt32(reader["SortNum"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicSubCatIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicPortTypeIni()
        {
            dicPortType.Clear();
            string query = "select * from HK_LibPortType where SortNum < 101 order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicPortTypeIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicSpecDicIni()
        {
            dicSpecDic.Clear();
            string query = "select * from HK_LibSpecDic order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicSpecDicIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicPipeODIni()
        {
            dicPipeOD.Clear();
            string query = "select * from HK_LibPipeOD order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicPipeODIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicPNIni()
        {
            dicPN.Clear();
            string query = "select * from HK_LibPN order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicPNIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicSteelIni()
        {
            dicSteel.Clear();
            string query = "select * from HK_LibSteel order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dicSteel.Add(Convert.ToString(reader["ID"]), new HKLibSteel
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
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicSteelIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicThreadIni()
        {
            dicThread.Clear();
            string query = "select * from HK_LibThread order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicThreadIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicTubeODIni()
        {
            dicTubeOD.Clear();
            string query = "select * from HK_LibTubeOD order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicTubeODIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicGlandIni()
        {
            dicGland.Clear();
            string query = "select * from HK_LibGland order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicGlandIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicGenOptionIni()
        {
            dicGenOption.Clear();
            string query = "select * from HK_LibGenOption order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicGenOptionIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        private void dicNoLinkSpecIni()
        {
            dicNoLinkSpec.Clear();

            //List<string> lst = dicSubCat.Values
            //    .SelectMany(v => new[] { v.TechSpecMain, v.TechSpecAux }) // 同时处理两个属性
            //    .SelectMany(s => s.Split(','))                           // 统一分割字符串
            //    .SelectMany(s => s.Split('|'))                          
            //    .Where(x => !string.IsNullOrWhiteSpace(x))               // 过滤空值
            //    .Select(x => x.Trim())                                   // 统一去除空格
            //    .Distinct(StringComparer.OrdinalIgnoreCase)              // 统一去重
            //    .ToList();
            List<string> lst = dicSpecDic.Values.Where(x=>x.Class != "Link").Select(x => x.ID).ToList();
            for (int i = 0; i < lst.Count; i++)
            {
                dicNoLinkSpec.Add(lst[i], new ObservableCollection<HKLibGenOption>());
            }
        }
    }
}
