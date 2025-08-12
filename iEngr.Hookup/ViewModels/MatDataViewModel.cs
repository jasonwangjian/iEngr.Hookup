using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Odbc;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace iEngr.Hookup.ViewModels
{
    public class MatDataViewModel : INotifyPropertyChanged
    {
        private HKMatMainCat _mainCat;
        private HKMatSubCat _subCat;
        private ObservableCollection<HKMatMainCat> _mainCats;
        private ObservableCollection<HKMatSubCat> _subCats;
        public HKMatMainCat MainCat
        {
            get => _mainCat;
            set
            {
                if (_mainCat != value)
                {
                    _mainCat = value;
                    OnPropertyChanged(nameof(MainCat));
                    SubCats = GetHKMatSubCats(_mainCat);
                }
            }
        }
        public HKMatSubCat SubCat
        {
            get => _subCat;
            set
            {
                if (_subCat != value)
                {
                    _subCat = value;
                    AlterCode = value?.TypeP2;
                    if (value?.ID == string.Empty)
                    {
                        StrTypeAllP1 = GetAllP1StringDistinct(SubCats);
                        StrTypeAllP2 = GetAllP2StringDistinct(SubCats);
                    }
                    else if (value != null)
                    {
                        StrTypeAllP1 = GetAllPortStringDistinct(value.TypeP1);
                        if (!HK_General.portDef.Contains(AlterCode))
                            StrTypeAllP2 = GetAllPortStringDistinct(value.TypeP2);
                        else
                            StrTypeAllP2 = StrTypeAllP1;
                    }
                    OnPropertyChanged(nameof(SubCat));
                }
            }
        }
        public ObservableCollection<HKMatMainCat> MainCats
        {
            get => _mainCats;
            set
            {
                if (_mainCats != value)
                {
                    _mainCats = value;
                    OnPropertyChanged(nameof(MainCats));
                }
            }
        }
        public ObservableCollection<HKMatSubCat> SubCats
        {
            get => _subCats;
            set
            {
                if (_subCats != value)
                {
                    _subCats = value;
                    OnPropertyChanged(nameof(SubCats));
                    SubCat = _subCats?[0];
                }
            }
        }

        private string _typeP1ID;
        private string _typeP2ID;
        private CmbItem _typeP1;
        private CmbItem _typeP2;
        private string _strTypeAllP1;
        private string _strTypeAllP2;
        private ObservableCollection<CmbItem> _typeAllP1;
        private ObservableCollection<CmbItem> _typeAllP2;
        public string StrTypeAllP1
        {
            get => _strTypeAllP1;
            set
            {
                if (_strTypeAllP1 != value)
                {
                    _strTypeAllP1 = value;
                    TypeAllP1 = GetAllPortType(value);
                }
            }
        }
        public string StrTypeAllP2
        {
            get => _strTypeAllP2;
            set
            {
                if (_strTypeAllP2 != value)
                {
                    _strTypeAllP2 = value;
                    TypeAllP2 = GetAllPortType(value);
                }
            }
        }
        public ObservableCollection<CmbItem> TypeAllP1
        {
            get => _typeAllP1;
            set
            {
                if (_typeAllP1 != value)
                {
                    string _id = TypeP1?.ID ?? string.Empty;
                    _typeAllP1 = value;
                    OnPropertyChanged(nameof(TypeAllP1));
                    if (value != null && value.Count > 0 && _id != null)
                    {
                        CmbItem _replacement = value.FirstOrDefault(x => x.ID == _id);
                        _replacement = _replacement ?? value[0];
                        TypeP1 = _replacement;
                    }
                }
            }
        }
        public ObservableCollection<CmbItem> TypeAllP2
        {
            get => _typeAllP2;
            set
            {
                if (_typeAllP2 != value)
                {
                    string _id = (AlterCode == "AS1") ? TypeP1?.ID ?? string.Empty : TypeP2?.ID ?? string.Empty;
                    _typeAllP2 = value;
                    OnPropertyChanged(nameof(TypeAllP2));
                    if (value != null && value.Count > 0 && _id != null)
                    {
                        CmbItem _replacement = value.FirstOrDefault(x => x.ID == _id);
                        _replacement = _replacement ?? value[0];
                        TypeP2 = _replacement;
                    }
                }
            }
        }
        public string TypeP1ID
        {
            get => _typeP1ID;
            set
            {
                value = value ?? string.Empty;
                if (TypeAllP1 != null && TypeAllP1.Count > 0)
                {
                    CmbItem _replacement = TypeAllP1.FirstOrDefault(x => x.ID == value);
                    _replacement = _replacement ?? TypeAllP1[0];
                    TypeP1 = _replacement;
                }
            }
        }
        public string TypeP2ID
        {
            get => _typeP2ID;
            set
            {
                value = value ?? string.Empty;
                if (TypeAllP2 != null && TypeAllP2.Count > 0)
                {
                    CmbItem _replacement = TypeAllP2.FirstOrDefault(x => x.ID == value);
                    _replacement = _replacement ?? TypeAllP2[0];
                    TypeP2 = _replacement;
                }
            }
        }

        public CmbItem TypeP1
        {
            get => _typeP1;
            set
            {
                if (_typeP1 != value)
                {
                    SizeAllP1 = GetAllSizeOrSpec(value);
                    _typeP1 = value;
                    if (AlterCode == "AS1" && TypeAllP2?.Count >0)
                    {
                        string _id = TypeP1?.ID ?? string.Empty;
                        CmbItem _replacement = TypeAllP2.FirstOrDefault(x => x.ID == _id);
                        _replacement = _replacement ?? TypeAllP2?[0];
                        TypeP2 = _replacement;
                        // _id = SizeP1?.ID ?? string.Empty;
                        // _replacement = SizeAllP2?.FirstOrDefault(x => x.ID == _id);
                        //_replacement = _replacement ?? SizeAllP2?[0];
                        //SizeP2 = _replacement;
                    }
                    OnPropertyChanged(nameof(TypeP1));
                }
            }
        }
        public CmbItem TypeP2
        {
            get => _typeP2;
            set
            {
                if (_typeP2 != value)
                {
                    SizeAllP2 = GetAllSizeOrSpec(value);
                    _typeP2 = value;
                    OnPropertyChanged(nameof(TypeP2));
                }
            }
        }

        private string _sizeP1ID;
        private string _sizeP2ID;
        private CmbItem _sizeP1;
        private CmbItem _sizeP2;
        private ObservableCollection<CmbItem> _sizeAllP1;
        private ObservableCollection<CmbItem> _sizeAllP2;
        public ObservableCollection<CmbItem> SizeAllP1
        {
            get => _sizeAllP1;
            set
            {
                if (_sizeAllP1 != value)
                {
                    string _id = SizeP1?.ID ?? string.Empty;
                    _sizeAllP1 = value;
                    OnPropertyChanged(nameof(SizeAllP1));
                    if (value != null && value.Count > 0 && _id != null)
                    {
                        CmbItem _replacement = value.FirstOrDefault(x => x.ID == _id);
                        _replacement = _replacement ?? value[0];
                        SizeP1 = _replacement;
                    }
                }
            }
        }
        public ObservableCollection<CmbItem> SizeAllP2
        {
            get => _sizeAllP2;
            set
            {
                if (_sizeAllP2 != value)
                {
                    string _id = SizeP2?.ID ?? string.Empty;
                    _sizeAllP2 = value;
                    OnPropertyChanged(nameof(SizeAllP2));
                    if (value != null && value.Count > 0 && _id != null)
                    {
                        CmbItem _replacement = value.FirstOrDefault(x => x.ID == _id);
                        _replacement = _replacement ?? value[0];
                        SizeP2 = _replacement;
                    }
                }
            }
        }
        public CmbItem SizeP1
        {
            get => _sizeP1;
            set
            {
                if (_sizeP1 != value)
                {
                    _sizeP1 = value;
                    if (AlterCode == "AS1")
                    {
                        string _id = SizeP1?.ID ?? string.Empty;
                        CmbItem _replacement = SizeAllP2?.FirstOrDefault(x => x.ID == _id);
                        _replacement = _replacement ?? SizeAllP2?[0];
                        SizeP2 = _replacement;
                    }
                    OnPropertyChanged(nameof(SizeP1));
                }
            }
        }
        public CmbItem SizeP2
        {
            get => _sizeP2;
            set
            {
                if (_sizeP2 != value)
                {
                    _sizeP2 = value;
                    OnPropertyChanged(nameof(SizeP2));
                }
            }
        }
        private string _techSpecMain;
        private string _techSpecAux;
        private string _matMatAll;
        private string _moreSpecCn;
        private string _moreSpecEn;
        private string _remarksEn;
        private string _remarksCn;
        private string _alterCode;
        public string TechSpecMain
        {
            get => _techSpecMain;
            set
            {
                if (_techSpecMain != value)
                {
                    _techSpecMain = value;
                    OnPropertyChanged(nameof(TechSpecMain));
                }
            }
        }
        public string TechSpecAux
        {
            get => _techSpecAux;
            set
            {
                if (_techSpecAux != value)
                {
                    _techSpecAux = value;
                    OnPropertyChanged(nameof(TechSpecAux));
                }
            }
        }

        public string MatMatAll
        {
            get => _matMatAll;
            set
            {
                if (_matMatAll != value)
                {
                    _matMatAll = value;
                    OnPropertyChanged(nameof(MatMatAll));
                }
            }
        }
        public string MoreSpecCn
        {
            get => _moreSpecCn;
            set
            {
                if (_moreSpecCn != value)
                {
                    _moreSpecCn = value;
                    OnPropertyChanged(nameof(MoreSpecCn));
                }
            }
        }
        public string MoreSpecEn
        {
            get => _moreSpecEn;
            set
            {
                if (_moreSpecEn != value)
                {
                    _moreSpecEn = value;
                    OnPropertyChanged(nameof(MoreSpecEn));
                }
            }
        }
        public string RemarksCn
        {
            get => _remarksCn;
            set
            {
                if (_remarksCn != value)
                {
                    _remarksCn = value;
                    OnPropertyChanged(nameof(RemarksCn));
                }
            }
        }
        public string RemarksEn
        {
            get => _remarksEn;
            set
            {
                if (_remarksEn != value)
                {
                    _remarksEn = value;
                    OnPropertyChanged(nameof(RemarksEn));
                }
            }
        }
        public string AlterCode
        {
            get => _alterCode;
            set
            {
                if (_alterCode != value)
                {
                    _alterCode = value;
                    OnPropertyChanged(nameof(AlterCode));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //protected bool SetField<T>(ref T field, T value, [CallerMemberName] string name = null)
        //{
        //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        //    field = value;
        //    OnPropertyChanged(name);
        //    return true;
        //}

        public MatDataViewModel()
        {
            MainCats = GetHKMatMainCats();
            MainCat = MainCats?[0];
        }
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
        private string GetAllP1StringDistinct(ObservableCollection<HKMatSubCat> subCats)
        {
            List<string> lst = SubCats.OrderBy(x => x.SortNum)
                           .Select(x => x.TypeP1)
                           .Where(x => !string.IsNullOrWhiteSpace(x))
                           .Select(x => x.Trim())
                           .Where(x => !HK_General.portNA.Contains(x))
                           .Distinct(StringComparer.OrdinalIgnoreCase)
                           .ToList();
            lst = string.Join(",", lst)
                           .Split(',')
                           .Where(x => !string.IsNullOrWhiteSpace(x))
                           .Select(x => x.Trim())
                           .Where(x => !HK_General.portNA.Contains(x))
                           .Where(x => !HK_General.portDef.Contains(x))
                           .Distinct(StringComparer.OrdinalIgnoreCase)
                           .ToList();
            return string.Join(",", lst);
        }
        private string GetAllP2StringDistinct(ObservableCollection<HKMatSubCat> subCats)
        {
            List<string> lst = SubCats.OrderBy(x => x.SortNum)
                           .Select(x => x.TypeP2)
                           .Where(x => !string.IsNullOrWhiteSpace(x))
                           .Select(x => x.Trim())
                           .Where(x => !HK_General.portNA.Contains(x))
                           .Distinct(StringComparer.OrdinalIgnoreCase)
                           .ToList();
            lst = string.Join(",", lst)
                           .Split(',')
                           .Where(x => !string.IsNullOrWhiteSpace(x))
                           .Select(x => x.Trim())
                           .Where(x => !HK_General.portNA.Contains(x))
                           .Where(x => !HK_General.portDef.Contains(x))
                           .Distinct(StringComparer.OrdinalIgnoreCase)
                           .ToList();
            return string.Join(",", lst);
        }
        private string GetAllPortStringDistinct(string strType)
        {
            return string.Join(",", strType.Split(',')
                           .Where(x => !string.IsNullOrWhiteSpace(x))
                           .Select(x => x.Trim())
                           .Where(x => !HK_General.portNA.Contains(x))
                           .Where(x => !HK_General.portDef.Contains(x))
                           .Distinct(StringComparer.OrdinalIgnoreCase)
                           .ToList());
        }
        private ObservableCollection<CmbItem> GetAllPortType(string input)
        {
            List<CmbItem> lst =  HK_General.dicPortType.Select(x => x.Value).Where(x => input.Split(',').Contains(x.ID)).OrderBy(x=>x.SortNum).Select(x=> new CmbItem
            {
                ID = x.ID,
                NameCn = x.NameCn,
                NameEn = x.NameEn,
                Comp = x.ID,
                Class = string.IsNullOrEmpty(x.Link?.Trim())? string.Empty:"Link",
                Link = x.Link,

            }).ToList();
            ObservableCollection<CmbItem> allPortTypes = new ObservableCollection<CmbItem>(lst);
            if (allPortTypes.Count > 0)
                allPortTypes.Insert(0, new CmbItem
                {
                    ID = string.Empty,
                    NameCn = "选择连接类型",
                    NameEn = "Select Conn.Type"
                });
            return allPortTypes;
        }

        private ObservableCollection<CmbItem> GetAllSizeOrSpec(CmbItem title)
        {
            List<CmbItem> lst = new List<CmbItem>();
            if (title == null || string.IsNullOrEmpty(title.ID)) return null;
            if (title.Class.StartsWith("Link") && !string.IsNullOrEmpty(title.Link.Trim()))
            {
                string[] _segParts = title.Link.Split(',').Select(item => item.Trim()).ToArray(); 
                string[] segParts = new string[3] { "", "", "" };
                Array.Copy(_segParts, 0, segParts, 0, Math.Min(_segParts.Length, 3));
                switch (segParts[0])
                {
                    case "LibPipeOD":
                        lst = getCmbItemsPipeODAll(HK_General.dicPipeOD, segParts[1].Trim(), segParts[2]);
                        break;
                    case "LibTubeOD":
                        lst = getCmbItemsTubeODAll(HK_General.dicTubeOD, segParts[1].Trim(), segParts[2]);
                        break;
                    case "LibThread":
                        lst = getCmbItemsThreadAll(HK_General.dicThread, segParts[1].Trim(), segParts[2]);
                        break;


                }
                return getSizeOrSpecLinked(lst, segParts[2].Trim());
            }
            return null;
        }


        private List<CmbItem> getCmbItemsPipeODAll(Dictionary<string, HKLibPipeOD> dic, string name, string cond)
        {
            string comp = cond.Split(':')[0];
            return dic.Select(x => x.Value).OrderBy(x => x.SortNum).Select(x => new CmbItem
            {
                ID = x.ID,
                NameCn = name == "DN" ? $"DN {x.DN} - NPS {x.NPS}"
                                             : name == "NPS" ? $"NPS {x.NPS} - DN {x.DN}"
                                             : $"DN {x.DN} - NPS {x.NPS}",
                NameEn = name == "DN" ? $"DN {x.DN} - NPS {x.NPS}"
                                             : name == "NPS" ? $"NPS {x.NPS} - DN {x.DN}"
                                             : $"DN {x.DN} - NPS {x.NPS}",
                Comp = comp == "DN" ? x.DN
                                             : comp == "NPS" ? x.NPS
                                             : comp == "HGIa" ? x.HGIa.ToString()
                                             : comp == "HGIb" ? x.HGIb.ToString()
                                             : comp == "HGII" ? x.HGII.ToString()
                                             : comp == "GBI" ? x.GBI.ToString()
                                             : comp == "GBII" ? x.GBII.ToString()
                                             : comp == "SpecRem" ? x.SpecRem
                                             : comp == "ISO" ? x.ISO.ToString()
                                             : comp == "ASME" ? x.ASME.ToString()
                                             : x.ID,
            }).ToList();
        }
        private List<CmbItem> getCmbItemsTubeODAll(Dictionary<string, HKLibTubeOD> dic, string name, string cond)
        {
            string comp = cond.Split(':')[0];
            return dic.Select(x => x.Value).OrderBy(x => x.SortNum).Select(x => new CmbItem
            {
                ID = x.ID,
                NameCn =  $"{x.SpecCn}mm D.D.",
                NameEn = $"{x.SpecEn}mm D.D.",
                Comp = comp == "Class" ? x.Class
                                             : comp == "SpecCn" ? x.SpecCn
                                             : comp == "SpecEn" ? x.SpecEn
                                             : comp == "ValueM" ? x.ValueM.ToString()
                                             : comp == "ClassEx" ? x.ClassEx.ToString()
                                             : x.ID,
            }).ToList();
        }
        private List<CmbItem> getCmbItemsThreadAll(Dictionary<string, HKLibThread> dic, string name, string cond)
        {
            string comp = cond.Split(':')[0];
            return dic.Select(x => x.Value).OrderBy(x => x.SortNum).Select(x => new CmbItem
            {
                ID = x.ID,
                NameCn = x.SpecCn,
                NameEn = x.SpecEn,
                Comp = comp == "Class" ? x.Class
                                             : comp == "SubClass" ? x.SubClass
                                             : comp == "SpecCn" ? x.SpecCn
                                             : comp == "SpecEn" ? x.SpecEn
                                             : comp == "Value" ? x.Value.ToString()
                                             : comp == "Pitch" ? x.Pitch.ToString()
                                             : comp == "Qty" ? x.Qty.ToString()
                                             : comp == "ClassEx" ? x.ClassEx
                                             : x.ID,
            }).ToList();
        }
        private ObservableCollection<CmbItem> getSizeOrSpecLinked(List<CmbItem> lst, string cond)
        {
            string[] _segParts = cond.Split(':').Select(item => item.Trim()).ToArray();
            string[] segParts = new string[3] { "", "", "" };
            Array.Copy(_segParts, 0, segParts, 0, Math.Min(_segParts.Length, 3));

            switch (segParts[1].ToLower())
            {
                case "nonull":
                    lst = lst.Where(x => !string.IsNullOrEmpty(x.Comp)).ToList();
                    break;
                case "null":
                    lst = lst.Where(x => string.IsNullOrEmpty(x.Comp)).ToList();
                    break;
                case "in":
                    //segParts[2].Split('|').Select(y => y.Trim())
                    lst = lst.Where(x => segParts[2].Split('|').Select(y=>y.Trim()).Contains(x.Comp)).ToList();
                    break;
                default:
                    break;
            }
            if (lst.Count > 0) lst.Insert(0, new CmbItem
            {
                ID = string.Empty,
                NameCn = "请选择规格或尺寸",
                NameEn = "Select Spec/Size",
            });
            return new ObservableCollection<CmbItem>(lst);
        }


        //public string parseConditions(string input)
        //{
        //    if (string.IsNullOrWhiteSpace(input))
        //        return null;

        //    // 分割输入为三部分 (字段:操作符:值)
        //    var segments = input.Split(':')
        //                        .Select(item => item.Trim())
        //                        .ToArray();

        //    // 验证基本格式
        //    if (segments.Length <= 2)
        //        return null;

        //    string field = segments[0];
        //    string op = segments[1].ToLowerInvariant(); // 使用不区分大小写的比较
        //    string value = segments[2];

        //    // 根据操作符类型路由处理逻辑
        //    switch (op)
        //    {
        //        case "in":
        //            return $"{field} IN {ConvertToStringScope(value, '|')}";

        //        case "out":
        //            return $"{field} NOT IN {ConvertToStringScope(value, '|')}";

        //        case "=":
        //            return $"{field} IN {ConvertToNumScope(value, '|')}";

        //        case "!=":
        //            return $"{field} NOT IN {ConvertToNumScope(value, '|')}";

        //        case "<":
        //        case "<=":
        //        case ">":
        //        case ">=":
        //            return HandleSingleValueComparison(field, op, value);

        //        case "<>":
        //        case "<=>":
        //        case "<>=":
        //        case "<=>=":
        //            return HandleRangeComparison(field, op, value);

        //        case "isnull":
        //            return $"{field} IS NULL OR {field} = ''";
        //        case "nonull":
        //            return $"{field} IS NOT NULL AND {field} <> ''";
        //        case "like":
        //            return $"{field} LIKE '%{value}%'";
        //        default:
        //            return null; // 不支持的操作符
        //    }
        //}

    }
}
