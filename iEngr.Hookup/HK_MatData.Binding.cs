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
    public partial class HK_MatData
    {
        private ObservableCollection<HKMatMainCat> GetHKMatMainCats()
        {
            ObservableCollection<HKMatMainCat> mainCats = new ObservableCollection<HKMatMainCat>
            {
                new HKMatMainCat
                {
                ID =  string.Empty,
                NameCn = "所有大类",
                NameEn = "All Main Categories"
                }
            };
            // 构建 SQL 查询语句
            string query = "select * from HK_MatMainCat order by SortNum ";
            using (OdbcConnection conn = HK_General.GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
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
                        ;
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_MatData)}.{nameof(GetHKMatMainCats)}{Environment.NewLine}Error: {ex.Message}");
                }
                return mainCats;
            }
        }
        private ObservableCollection<HKMatSubCat> GetHKMatSubCats(HKMatMainCat mainCat)
        {
            ObservableCollection<HKMatSubCat> subCats = new ObservableCollection<HKMatSubCat>();
            if (mainCat != null && string.IsNullOrEmpty(mainCat.ID))
                subCats = new ObservableCollection<HKMatSubCat>(HK_General.dicSubCat.Select(x => x.Value).ToList());
            else if (mainCat != null && !string.IsNullOrEmpty(mainCat.ID))
                subCats = new ObservableCollection<HKMatSubCat>(HK_General.dicSubCat.Select(x => x.Value).Where(x => x.CatID == mainCat.ID).ToList());
            subCats.Insert(0, new HKMatSubCat
            {
                ID = string.Empty,
                SpecCn = string.IsNullOrEmpty(mainCat.ID) ? "所有小类" : "所有" + mainCat.Name,
                SpecEn = string.IsNullOrEmpty(mainCat.ID) ? "All Sub Categories" : "All " + mainCat.Name
            });
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
            //string query = "select * from HK_LibPortType where SortNum < 101 and ID in " + GeneralFun.ConvertToStringScope(strTypes, ',') + " order by SortNum";
            //try
            //{
            //    if (conn == null || conn.State != ConnectionState.Open)
            //        conn = GetConnection();
            //    OdbcCommand command = new OdbcCommand(query, conn);
            //    OdbcDataReader reader = command.ExecuteReader();
            //    while (reader.Read())
            //    {
            //        HKLibSpecDic portType = new HKLibSpecDic
            //        {
            //            ID = Convert.ToString(reader["ID"]),
            //            NameCn = Convert.ToString(reader["NameCn"]),
            //            NameEn = Convert.ToString(reader["NameEn"]),
            //            PrefixCn = Convert.ToString(reader["PrefixCn"]),
            //            PrefixEn = Convert.ToString(reader["PrefixEn"]),
            //            SuffixCn = Convert.ToString(reader["SuffixCn"]),
            //            SuffixEn = Convert.ToString(reader["SuffixEn"]),
            //            Class = string.IsNullOrEmpty(Convert.ToString(reader["Link"])) ?
            //                    Convert.ToString(reader["Remarks"]) :
            //                    "Link",
            //            Link = Convert.ToString(reader["Link"])
            //        };
            //        portTypes.Add(portType);
            //    }

            //    reader.Close();
            //}
            //catch (Exception ex)
            //{
            //    // 处理异常
            //    MessageBox.Show($"Error: {ex.Message}");
            //    // 可以选择返回空列表或者其他适当的处理
            //}
            return portTypes;
        }
        private ObservableCollection<HKLibSpecDic> GetLibSpecDic(string strIDs)
        {
            ObservableCollection<HKLibSpecDic> libSpecDics = new ObservableCollection<HKLibSpecDic>();
            // 构建 SQL 查询语句
            //string query = "select * from HK_LibSpecDic where ID in " + GeneralFun.ConvertToStringScope(strIDs, '|') + " order by SortNum";
            //try
            //{
            //    if (conn == null || conn.State != ConnectionState.Open)
            //        conn = GetConnection();
            //    OdbcCommand command = new OdbcCommand(query, conn);
            //    OdbcDataReader reader = command.ExecuteReader();
            //    while (reader.Read())
            //    {
            //        HKLibSpecDic libSpecDic = new HKLibSpecDic
            //        {
            //            ID = Convert.ToString(reader["ID"]),
            //            NameCn = Convert.ToString(reader["NameCn"]),
            //            NameEn = Convert.ToString(reader["NameEn"]),
            //            PrefixCn = Convert.ToString(reader["PrefixCn"]),
            //            PrefixEn = Convert.ToString(reader["PrefixEn"]),
            //            Class = Convert.ToString(reader["Class"]),
            //            Link = Convert.ToString(reader["Link"]),
            //            SuffixCn = Convert.ToString(reader["SuffixCn"]),
            //            SuffixEn = Convert.ToString(reader["SuffixEn"]),
            //            SortNum = Convert.ToInt32(reader["SortNum"])
            //        };
            //        libSpecDics.Add(libSpecDic);
            //    }
            //    reader.Close();
            //}
            //catch (Exception ex)
            //{
            //    // 处理异常
            //    MessageBox.Show($"Error: {ex.Message}");
            //    // 可以选择返回空列表或者其他适当的处理
            //}
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
            //HKLibGenOption hkGeneralSpec = new HKLibGenOption();
            //string prefix = (HK_LibMat.intLan == 0) ? libSpecDic.PrefixCn : libSpecDic.PrefixEn;
            //string suffix = (HK_LibMat.intLan == 0) ? libSpecDic.SuffixCn : libSpecDic.SuffixEn;
            //switch (libSpecDic?.Class.ToUpper())
            //{
            //    case "LINK":
            //        // 构建 SQL 查询语句
            //        string query = GeneralFun.ParseLinkExp(libSpecDic.Link);
            //        try
            //        {
            //            if (conn == null || conn.State != ConnectionState.Open)
            //                conn = GetConnection();
            //            OdbcCommand command = new OdbcCommand(query, conn);
            //            OdbcDataReader reader = command.ExecuteReader();
            //            if (libSpecDic.Link.StartsWith("LibPipeOD"))
            //            {
            //                hkGeneralSpec = new HKLibGenOption
            //                {
            //                    ID = string.Empty,
            //                    NameCn = "选择公称直径",
            //                    NameEn = "All DN/NPS"
            //                };
            //                hKGeneralSpecs.Add(hkGeneralSpec);
            //                if (libSpecDic.ID.Contains("NPS") || libSpecDic.ID.Contains("ASME") || libSpecDic.ID.Contains("ANSI"))
            //                {
            //                    while (reader.Read())
            //                    {
            //                        hkGeneralSpec = new HKLibGenOption
            //                        {
            //                            ID = Convert.ToString(reader["ID"]),
            //                            NameCn = libSpecDic.ID.StartsWith("OD") ?
            //                                     $"NPS {Convert.ToString(reader["NPS"])} - {prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}" :
            //                                     $"NPS {Convert.ToString(reader["NPS"])}",
            //                            NameEn = libSpecDic.ID.StartsWith("OD") ?
            //                                     $"NPS {Convert.ToString(reader["NPS"])} - {prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}" :
            //                                     $"NPS {Convert.ToString(reader["NPS"])}",
            //                            SpecCn = $"{prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}",
            //                            SpecEn = $"{prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}"
            //                        };
            //                        hKGeneralSpecs.Add(hkGeneralSpec);
            //                    }
            //                }
            //                else
            //                {
            //                    while (reader.Read())
            //                    {
            //                        hkGeneralSpec = new HKLibGenOption
            //                        {
            //                            ID = Convert.ToString(reader["ID"]),
            //                            NameCn = libSpecDic.ID.StartsWith("OD") ?
            //                                     $"DN {Convert.ToString(reader["DN"])} - {prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}" :
            //                                     $"DN {Convert.ToString(reader["DN"])}",
            //                            NameEn = libSpecDic.ID.StartsWith("OD") ?
            //                                     $"DN {Convert.ToString(reader["DN"])} - {prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}" :
            //                                     $"DN {Convert.ToString(reader["DN"])}",
            //                            SpecCn = $"{prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}",
            //                            SpecEn = $"{prefix}{Convert.ToString(reader[libSpecDic.Link.Split(',')[1]])}{suffix}"
            //                        };
            //                        hKGeneralSpecs.Add(hkGeneralSpec);
            //                    }
            //                }
            //            }
            //            else if (libSpecDic.Link.StartsWith("LibSteel"))
            //            {
            //                if (libSpecDic.ID.StartsWith("CS"))
            //                {
            //                    hkGeneralSpec = new HKLibGenOption
            //                    {
            //                        ID = string.Empty,
            //                        NameCn = "选择槽钢规格",
            //                        NameEn = "All Channel Steels"
            //                    };
            //                    hKGeneralSpecs.Add(hkGeneralSpec);
            //                    while (reader.Read())
            //                    {
            //                        hkGeneralSpec = new HKLibGenOption
            //                        {
            //                            ID = Convert.ToString(reader["ID"]),
            //                            NameCn = $"{prefix}{Convert.ToString(reader["CSSpecCn"])}{suffix}",
            //                            NameEn = $"{prefix}{Convert.ToString(reader["CSSpecEn"])}{suffix}",
            //                            SpecCn = $"{prefix}{Convert.ToString(reader["CSSpecCn"])}{suffix}",
            //                            SpecEn = $"{prefix}{Convert.ToString(reader["CSSpecEn"])}{suffix}"
            //                        };
            //                        hKGeneralSpecs.Add(hkGeneralSpec);
            //                    }
            //                }
            //                else if (libSpecDic.ID.StartsWith("IB"))
            //                {
            //                    hkGeneralSpec = new HKLibGenOption
            //                    {
            //                        ID = string.Empty,
            //                        NameCn = "选择工字钢规格",
            //                        NameEn = "All I-Beams"
            //                    };
            //                    hKGeneralSpecs.Add(hkGeneralSpec);
            //                    while (reader.Read())
            //                    {
            //                        hkGeneralSpec = new HKLibGenOption
            //                        {
            //                            ID = Convert.ToString(reader["ID"]),
            //                            NameCn = $"{prefix}{Convert.ToString(reader["IBSpecCn"])}{suffix}",
            //                            NameEn = $"{prefix}{Convert.ToString(reader["IBSpecEn"])}{suffix}",
            //                            SpecCn = $"{prefix}{Convert.ToString(reader["IBSpecCn"])}{suffix}",
            //                            SpecEn = $"{prefix}{Convert.ToString(reader["IBSpecEn"])}{suffix}"
            //                        };
            //                        hKGeneralSpecs.Add(hkGeneralSpec);
            //                    }
            //                }
            //            }
            //            else if (libSpecDic.Link.StartsWith("LibPN")
            //                  || libSpecDic.Link.StartsWith("LibGland")
            //                  || libSpecDic.Link.StartsWith("LibTubeOD")
            //                  || libSpecDic.Link.StartsWith("LibThread"))
            //            {
            //                if (libSpecDic.Link.StartsWith("LibPN"))
            //                {
            //                    hkGeneralSpec = new HKLibGenOption
            //                    {
            //                        ID = string.Empty,
            //                        NameCn = "选择公称压力",
            //                        NameEn = "All PN/CLS"
            //                    };
            //                }
            //                else if (libSpecDic.Link.StartsWith("LibGland"))
            //                {
            //                    hkGeneralSpec = new HKLibGenOption
            //                    {
            //                        ID = string.Empty,
            //                        NameCn = "选择电缆索头",
            //                        NameEn = "All Cable Glands"
            //                    };
            //                }
            //                else if (libSpecDic.Link.StartsWith("LibTubeOD"))
            //                {
            //                    hkGeneralSpec = new HKLibGenOption
            //                    {
            //                        ID = string.Empty,
            //                        NameCn = "选择Tube管外径",
            //                        NameEn = "All Tube O.D."
            //                    };
            //                }
            //                else if (libSpecDic.Link.StartsWith("LibThread"))
            //                {
            //                    hkGeneralSpec = new HKLibGenOption
            //                    {
            //                        ID = string.Empty,
            //                        NameCn = "选择螺纹规格",
            //                        NameEn = "All Thread Typies"
            //                    };
            //                }
            //                hKGeneralSpecs.Add(hkGeneralSpec);
            //                while (reader.Read())
            //                {
            //                    hkGeneralSpec = new HKLibGenOption
            //                    {
            //                        ID = Convert.ToString(reader["ID"]),
            //                        NameCn = $"{prefix}{Convert.ToString(reader["SpecCn"])}{suffix}",
            //                        NameEn = $"{prefix}{Convert.ToString(reader["SpecEn"])}{suffix}",
            //                        SpecCn = $"{prefix}{Convert.ToString(reader["SpecCn"])}{suffix}",
            //                        SpecEn = $"{prefix}{Convert.ToString(reader["SpecEn"])}{suffix}"
            //                    };
            //                    hKGeneralSpecs.Add(hkGeneralSpec);
            //                }
            //            }
            //            else if (libSpecDic.Link.StartsWith("LibGenOption"))
            //            {
            //                hkGeneralSpec = new HKLibGenOption
            //                {
            //                    ID = string.Empty,
            //                    NameCn = "选择相关规格",
            //                    NameEn = "All Specifications"
            //                };
            //                hKGeneralSpecs.Add(hkGeneralSpec);
            //                while (reader.Read())
            //                {
            //                    hkGeneralSpec = new HKLibGenOption
            //                    {
            //                        ID = Convert.ToString(reader["ID"]),
            //                        NameCn = $"{prefix}{Convert.ToString(reader["NameCn"])}{suffix}",
            //                        NameEn = $"{prefix}{Convert.ToString(reader["NameEn"])}{suffix}",
            //                        SpecCn = $"{prefix}{Convert.ToString(reader["SpecCn"])}{suffix}",
            //                        SpecEn = $"{prefix}{Convert.ToString(reader["SpecEn"])}{suffix}"
            //                    };
            //                    hKGeneralSpecs.Add(hkGeneralSpec);
            //                }
            //            }
            //            reader.Close();
            //        }

            //        catch (Exception ex)
            //        {
            //            // 处理异常
            //            MessageBox.Show($"Error: {ex.Message}");
            //            // 可以选择返回空列表或者其他适当的处理
            //        }
            //        break;
            //    default:
            //        if (!dicNoLinkSpec.ContainsKey(libSpecDic.ID))
            //        {
            //            dicNoLinkSpec.Add(libSpecDic.ID, new ObservableCollection<HKLibGenOption>());
            //            dicNoLinkSpecStr.Add(libSpecDic.ID, new ObservableCollection<string>());
            //        }
            //        hKGeneralSpecs = dicNoLinkSpec[libSpecDic.ID];
            //        break;
            //}
            return hKGeneralSpecs;
        }
    }
}
