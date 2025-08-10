using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Odbc;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace iEngr.Hookup.ViewModels
{
    public class MatDataViewModel : INotifyPropertyChanged
    {
        public HKLibSpecDic _typeP1;
        public HKLibSpecDic _typeP2;
        public HKLibGenOption _sizeP1;
        public HKLibGenOption _sizeP2;
        private string _mainCatID;
        private string _subCatID;
        private HKMatMainCat _mainCat;
        private HKMatSubCat _subCat;
        private ObservableCollection<HKMatMainCat> _mainCats;
        private ObservableCollection<HKMatSubCat> _subCats;
        private string _techSpecMain;
        private string _techSpecAux;
        private string _strTypeAllP1;
        private string _strTypeAllP2;
        private ObservableCollection<HKLibSpecDic> _typeAllP1;
        private ObservableCollection<HKLibSpecDic> _typeAllP2;
        private ObservableCollection<HKLibGenOption> _sizeAllP1;
        private ObservableCollection<HKLibGenOption> _sizeAllP2;
        private string _matMatAll;
        private string _moreSpecCn;
        private string _moreSpecEn;
        private string _remarksEn;
        private string _remarksCn;
        private string _alterCode;
        public string MainCatID
        {
            get => _mainCatID;
            set
            {
                if (_mainCatID != value)
                {
                    _mainCatID = value;
                    OnPropertyChanged(nameof(MainCatID));
                }
            }
        }
        public string SubCatID
        {
            get => _subCatID;
            set
            {
                if (_subCatID != value)
                {
                    _subCatID = value;
                    OnPropertyChanged(nameof(SubCatID));
                }
            }
        }
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
                    OnPropertyChanged(nameof(SubCat));
                    AlterCode = value?.TypeP2;
                    if (value?.ID == string.Empty)
                    {
                        StrTypeAllP1=GetAllP1StringDistinct(SubCats);
                        StrTypeAllP2 = GetAllP2StringDistinct(SubCats);
                    }
                    else if (value != null)
                    {
                        StrTypeAllP1 = GetAllPortStringDistinct(value.TypeP1);
                        if  (!HK_General.portDef.Contains(AlterCode))
                            StrTypeAllP2=GetAllPortStringDistinct(value.TypeP2);
                    }
                    //if (value?.ID == string.Empty)
                    // {
                    //     TypeAllP1 = GetAllPortType(GetAllPortType(GetAllP1StringDistinct(SubCats)));
                    //     TypeAllP2 = HK_General.portDef.Contains(AlterCode)? TypeAllP1: GetAllPortType(GetAllPortType(GetAllP2StringDistinct(SubCats)));
                    // }
                    // else if (value != null)
                    // {
                    //     TypeAllP1 = GetAllPortType(GetAllPortType(GetAllPortStringDistinct(value.TypeP1)));
                    //     TypeAllP2 = HK_General.portDef.Contains(AlterCode) ? TypeAllP1 : GetAllPortType(GetAllPortType(GetAllPortStringDistinct(value.TypeP2)));
                    // }
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
                    //TypeAllP1 = GetAllPortType(GetAllPortType(GetAllP1StringDistinct(value)));
                    //TypeAllP2 = GetAllPortType(GetAllPortType(GetAllP2StringDistinct(value)));
                    SubCat = _subCats?[0];
                }
            }
        }
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
        public string StrTypeAllP1
        {
            get => _strTypeAllP1;
            set
            {
                if (_strTypeAllP1 != value)
                {
                    _strTypeAllP1 = value;
                    OnPropertyChanged(nameof(StrTypeAllP1));
                    TypeAllP1 = GetAllPortType(GetAllPortType(value));
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
                    OnPropertyChanged(nameof(StrTypeAllP2));
                    TypeAllP2 = GetAllPortType(GetAllPortType(value));
                }
            }
        }
        public ObservableCollection<HKLibSpecDic> TypeAllP1
        {
            get => _typeAllP1;
            set
            {
                if (_typeAllP1 != value)
                {
                    _typeAllP1 = value;
                    OnPropertyChanged(nameof(TypeAllP1));
                }
            }
        }
        public ObservableCollection<HKLibSpecDic> TypeAllP2
        {
            get => _typeAllP2;
            set
            {
                if (_typeAllP2 != value)
                {
                    _typeAllP2 = value;
                    OnPropertyChanged(nameof(TypeAllP2));
                }
            }
        }
        public HKLibSpecDic TypeP1
        {
            get => _typeP1;
            set
            {
                if (_typeP1 != value)
                {
                    _typeP1 = value;
                    OnPropertyChanged(nameof(TypeP1));
                    if (AlterCode == "AS1") TypeP2 = value;
                }
            }
        }
        public HKLibSpecDic TypeP2
        {
            get => _typeP2;
            set
            {
                if (_typeP2 != value)
                {
                    _typeP2 = value;
                    OnPropertyChanged(nameof(TypeP2));
                }
            }
        }
        public HKLibGenOption SizeP1
        {
            get => _sizeP1;
            set
            {
                if (_sizeP1 != value)
                {
                    _sizeP1 = value;
                    OnPropertyChanged(nameof(SizeP1));
                }
            }
        }
        public HKLibGenOption SizeP2
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
        public ObservableCollection<HKLibGenOption> SizeAllP1
        {
            get => _sizeAllP1;
            set
            {
                if (_sizeAllP1 != value)
                {
                    _sizeAllP1 = value;
                    OnPropertyChanged(nameof(SizeAllP1));
                }
            }
        }
        public ObservableCollection<HKLibGenOption> SizeAllP2
        {
            get => _sizeAllP2;
            set
            {
                if (_sizeAllP2 != value)
                {
                    _sizeAllP2 = value;
                    OnPropertyChanged(nameof(SizeAllP2));
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
                    if (HK_General.portDef.Contains(value))
                    {
                        TypeAllP2 = TypeAllP1;
                        if (value == "AS1")
                            TypeP2 = TypeP1;
                    }
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
                           .Select(x=>x.TypeP1)
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
        private List<HKLibPortType> GetAllPortType(string input)
        {
            return HK_General.dicPortType.Select(x => x.Value).Where(x => input.Split(',').Contains(x.ID)).ToList();
        }

        private ObservableCollection<HKLibSpecDic> GetAllPortType(List<HKLibPortType> lst)
        {

            ObservableCollection<HKLibSpecDic> allPortTypes = new ObservableCollection<HKLibSpecDic>(lst.Select(x => new HKLibSpecDic
            {
                ID = x.ID,
                NameCn = x.NameCn,
                NameEn = x.NameEn,
                PrefixCn = x.PrefixCn,
                PrefixEn = x.PrefixEn,
                SuffixCn = x.SuffixCn,
                SuffixEn = x.SuffixEn,
                Link = x.Link,
                Class = string.IsNullOrEmpty(x.Link) ? x.Remarks : "Link"
            })
                .OrderBy(x => x.SortNum).ToList());
            if (allPortTypes.Count>0)
            allPortTypes.Insert(0, new HKLibSpecDic
            {
                ID = string.Empty,
                NameCn = "选择连接类型",
                NameEn = "Select Conn.Type"
            });
            return allPortTypes;
        }
    }
}
