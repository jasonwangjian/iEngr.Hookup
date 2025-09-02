using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
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
using iEngr.Hookup.Models;

namespace iEngr.Hookup
{
    /// <summary>
    /// Database Handle
    /// </summary>
    public static partial class HK_General
    {
        internal static int? nullInt = null;
        internal static decimal? nullDecimal = null;
        internal static Dictionary<string, HKLibMatName> dicMatName = dicMatNameIni();
        internal static Dictionary<string, HKLibPortType> dicPortType = dicPortTypeIni();
        internal static Dictionary<string, HKLibSpecDic> dicSpecDic = dicSpecDicIni();
        internal static Dictionary<string, HKLibPipeOD> dicPipeOD = dicPipeODIni();
        internal static Dictionary<string, HKLibPN> dicPN = dicPNIni();
        internal static Dictionary<string, HKLibSteel> dicSteel = dicSteelIni();
        internal static Dictionary<string, HKLibThread> dicThread = dicThreadIni();
        internal static Dictionary<string, HKLibTubeOD> dicTubeOD = dicTubeODIni();
        internal static Dictionary<string, HKLibGland> dicGland = dicGlandIni();
        internal static Dictionary<string, HKLibGenOption> dicGenOption = dicGenOptionIni();
        internal static Dictionary<string, ObservableCollection<HKLibGenOption>> dicNoLinkSpec = dicNoLinkSpecIni();
        internal static Dictionary<string, HKLibMatMat> dicMatMat = dicMatMatIni();
        internal static Dictionary<string, ObservableCollection<HKLibTreeNode>> dicTreeNode = dicTreeNodeIni();
        private static Dictionary<string, HKLibMatName> dicMatNameIni()
        {
            Dictionary<string, HKLibMatName> dicMatName = new Dictionary<string, HKLibMatName>();
            string query = "select * from HK_LibMatName order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dicMatName.Add(Convert.ToString(reader["ID"]), new HKLibMatName
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
                                Qty = Convert.ToString(reader["Qty"]),
                                Unit = Convert.ToString(reader["CatID"]),
                                SupDisc = Convert.ToString(reader["SupDisc"]),
                                SupResp = Convert.ToString(reader["SupResp"]),
                                ErecDisc = Convert.ToString(reader["ErecDisc"]),
                                ErecResp = Convert.ToString(reader["ErecResp"]),
                                SortNum = Convert.ToInt32(reader["SortNum"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(dicMatNameIni)}{Environment.NewLine}Error: {ex.Message}");
                }
            }
            return dicMatName;
        }
        private static Dictionary<string, HKLibPortType> dicPortTypeIni()
        {
            Dictionary<string, HKLibPortType> dicPortType = new Dictionary<string, HKLibPortType>(); ;
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
                    Debug.WriteLine($"___HK_General.dicPortTypeIni, Error: {ex.Message}");
                }
            }
            return dicPortType;
        }
        private static Dictionary<string, HKLibSpecDic> dicSpecDicIni()
        {
            Dictionary<string, HKLibSpecDic> dicSpecDic = new Dictionary<string, HKLibSpecDic>();
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
                    Debug.WriteLine($"___HK_General.dicSpecDicIni, Error: {ex.Message}");
                }
            }
            return dicSpecDic;
        }
        private static Dictionary<string, HKLibPipeOD> dicPipeODIni()
        {
            Dictionary<string, HKLibPipeOD> dicPipeOD = new Dictionary<string, HKLibPipeOD>();
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
                    Debug.WriteLine($"___HK_General.dicPipeODIni, Error: {ex.Message}");
                }
            }
            return dicPipeOD;
        }
        private static Dictionary<string, HKLibPN> dicPNIni()
        {
            Dictionary<string, HKLibPN> dicPN = new Dictionary<string, HKLibPN>();
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
                    Debug.WriteLine($"___HK_General.dicPNIni, Error: {ex.Message}");
                }
            }
            return dicPN;
        }
        private static Dictionary<string, HKLibSteel> dicSteelIni()
        {
            Dictionary<string, HKLibSteel> dicSteel = new Dictionary<string, HKLibSteel>();
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
                    Debug.WriteLine($"___HK_General.dicSteelIni, Error: {ex.Message}");
                }
            }
            return dicSteel;
        }
        private static Dictionary<string, HKLibThread> dicThreadIni()
        {
            Dictionary<string, HKLibThread> dicThread = new Dictionary<string, HKLibThread>();
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
                    Debug.WriteLine($"___HK_General.dicThreadIni, Error: {ex.Message}");
                }
            }
            return dicThread;
        }
        private static Dictionary<string, HKLibTubeOD> dicTubeODIni()
        {
            Dictionary<string, HKLibTubeOD> dicTubeOD = new Dictionary<string, HKLibTubeOD>();
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
                    Debug.WriteLine($"___HK_General.dicTubeODIni, Error: {ex.Message}");
                }
            }
            return dicTubeOD;
        }
        private static Dictionary<string, HKLibGland> dicGlandIni()
        {
            Dictionary<string, HKLibGland> dicGland = new Dictionary<string, HKLibGland>();
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
                    Debug.WriteLine($"___HK_General.dicGlandIni, Error: {ex.Message}");
                }
            }
            return dicGland;
        }
        private static Dictionary<string, HKLibGenOption> dicGenOptionIni()
        {
            Dictionary<string, HKLibGenOption> dicGenOption = new Dictionary<string, HKLibGenOption>();
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
                    Debug.WriteLine($"___HK_General.dicGenOptionIni, Error: {ex.Message}");
                }
            }
            return dicGenOption;
        }
        private static Dictionary<string, ObservableCollection<HKLibGenOption>> dicNoLinkSpecIni()
        {
            Dictionary<string, ObservableCollection<HKLibGenOption>> dicNoLinkSpec = new Dictionary<string, ObservableCollection<HKLibGenOption>>();

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
            return dicNoLinkSpec;
        }
        private static Dictionary<string, HKLibMatMat> dicMatMatIni()
        {
            Dictionary<string, HKLibMatMat> dicMatMat = new Dictionary<string, HKLibMatMat>();
            string query = "select * from HK_LibMatMat order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dicMatMat.Add(Convert.ToString(reader["ID"]), new HKLibMatMat
                            {
                                ID = Convert.ToString(reader["ID"]),
                                SpecCn = Convert.ToString(reader["SpecCn"]),
                                SpecEn = Convert.ToString(reader["SpecEn"]),
                                NameCn = Convert.ToString(reader["NameCn"]),
                                NameEn = Convert.ToString(reader["NameCn"]),
                                ActiveCode = Convert.ToString(reader["ActiveCode"]),
                                SortNum = Convert.ToInt32(reader["SortNum"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Debug.WriteLine($"___HK_General.dicMatMatIni, Error: {ex.Message}");
                }
            }
            return dicMatMat;
        }
        private static Dictionary<string, ObservableCollection<HKLibTreeNode>> dicTreeNodeIni()
        {
            Dictionary<string, ObservableCollection<HKLibTreeNode>> dicTreeNode = new Dictionary<string, ObservableCollection<HKLibTreeNode>>();
            dicTreeNode.Add("SpecNode", new ObservableCollection<HKLibTreeNode>());
            string query = "select * from HK_LibTreeNode order by SortNum";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = Convert.ToString(reader["ID"]);
                            string parent = Convert.ToString(reader["Parent"]);
                            if (dicTreeNode.ContainsKey(parent))
                            {
                                if (!dicTreeNode[parent].Any(x=>x.ID == id))
                                {
                                    dicTreeNode[parent].Add(new HKLibTreeNode
                                    {
                                        ID = id,
                                        Parent = parent,
                                        NameCn = Convert.ToString(reader["NameCn"]),
                                        NameEn = Convert.ToString(reader["NameEn"]),
                                        RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                        RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                        NodeType = Convert.ToString(reader["NodeType"]),
                                        IdentType = Convert.ToString(reader["IdentType"]),
                                        FullName = Convert.ToString(reader["FullName"]),
                                        NestedName = Convert.ToString(reader["NestedName"]),
                                        SpecValue = Convert.ToString(reader["SpecValue"]),
                                        Status = Convert.ToByte(reader["Status"]),
                                        SortNum = Convert.ToInt32(reader["SortNum"]),
                                    });
                                }
                            }
                            else
                            {
                                dicTreeNode.Add(parent, new ObservableCollection<HKLibTreeNode>
                                {
                                    new HKLibTreeNode
                                    {
                                        ID = id,
                                        Parent = parent,
                                        NameCn = Convert.ToString(reader["NameCn"]),
                                        NameEn = Convert.ToString(reader["NameEn"]),
                                        RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                        RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                        NodeType = Convert.ToString(reader["NodeType"]),
                                        IdentType = Convert.ToString(reader["IdentType"]),
                                        FullName = Convert.ToString(reader["FullName"]),
                                        NestedName = Convert.ToString(reader["NestedName"]),
                                        SpecValue = Convert.ToString(reader["SpecValue"]),
                                        Status = Convert.ToByte(reader["Status"]),
                                        SortNum = Convert.ToInt32(reader["SortNum"]),
                                    }
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Debug.WriteLine($"___HK_General.dicMatMatIni, Error: {ex.Message}");
                }
            }
            return dicTreeNode;
        }
    }
}
