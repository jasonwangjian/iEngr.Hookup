using iEngr.Hookup.Models;
using System;
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
    /// Binding Data Handle
    /// </summary>
    public partial class HK_LibMat
    {
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
                        : strTypeP2All + ((portDef.Contains(subCat.TypeP2)) ? subCat.TypeP1 : subCat.TypeP2) + ",";
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
                        Class = string.IsNullOrEmpty(Convert.ToString(reader["Link"])) ?
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
            string prefix = (HK_LibMat.intLan == 0) ? libSpecDic.PrefixCn : libSpecDic.PrefixEn;
            string suffix = (HK_LibMat.intLan == 0) ? libSpecDic.SuffixCn : libSpecDic.SuffixEn;
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
    }
}
