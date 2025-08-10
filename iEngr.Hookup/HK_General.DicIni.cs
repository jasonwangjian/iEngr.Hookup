using System;
using System.Collections;
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
    public static partial class HK_General
    {
        static int? nullInt = null;
        static decimal? nullDecimal = null;
        static internal Dictionary<string, HKMatSubCat> dicSubCat = new Dictionary<string, HKMatSubCat>();
        static internal Dictionary<string, HKLibPortType> dicPortType = new Dictionary<string, HKLibPortType>();
        static internal Dictionary<string, HKLibGenOption> dicGenOption = new Dictionary<string, HKLibGenOption>();
        static internal Dictionary<string, HKLibGland> dicGland = new Dictionary<string, HKLibGland>();
        static internal Dictionary<string, HKLibPipeOD> dicPipeOD = new Dictionary<string, HKLibPipeOD>();
        static internal Dictionary<string, HKLibPN> dicPN = new Dictionary<string, HKLibPN>();
        static internal Dictionary<string, HKLibSpecDic> dicSpecDic = new Dictionary<string, HKLibSpecDic>();
        static internal Dictionary<string, HKLibSteel> dicSteel = new Dictionary<string, HKLibSteel>();
        static internal Dictionary<string, HKLibThread> dicThread = new Dictionary<string, HKLibThread>();
        static internal Dictionary<string, HKLibTubeOD> dicTubeOD = new Dictionary<string, HKLibTubeOD>();
        static internal void SetDicMatGen()
        {
            SetDicSubCat();
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
        static private void SetDicSubCat()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicSubCat)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        static private void SetDicPortType()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicPortType)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        static private void SetDicSpecDic()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicSpecDic)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }

        static private void SetDicPipeOD()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicPipeOD)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        static private void SetDicPN()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicPN)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        static private void SetDicSteel()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicSteel)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        static private void SetDicThread()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicThread)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }

        static private void SetDicTubeOD()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicTubeOD)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        static private void SetDicGland()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicGland)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
        static private void SetDicGenOption()
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
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(SetDicGenOption)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
        }
    }
}
