using iEngr.Hookup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace iEngr.Hookup.ViewModels
{
    public class MatDataViewModel : INotifyPropertyChanged
    {
        // 添加自定义事件
        public event EventHandler<string> DataChanged;

        public MatDataViewModel()
        {
            HK_General = new HK_General();
            MainCats = GetHKMatMainCats();
            MainCat = MainCats?[0];
            KeyDownCommand = new RelayCommand<KeyEventArgs>(HandleKeyDownSpec);
            ResetAllCommand = new RelayCommand<object>(_ => DataResetAll());
            ResetSpecCommand = new RelayCommand<object>(_ => DataResetSpec());
            ResetMoreCommand = new RelayCommand<object>(_ => DataResetMore());
        }

        private HK_General HK_General;
        private HKMatMainCat _mainCat;
        private HKMatSubCat _subCat;
        private ObservableCollection<HKMatMainCat> _mainCats;
        private ObservableCollection<HKMatSubCat> _subCats;
        public HKMatMainCat MainCat
        {
            get => _mainCat;
            set
            {
                SetField(ref _mainCat, value);
                SubCats = GetHKMatSubCats(_mainCat);
            }
                //set
                //{
                //    if (_mainCat != value)
                //    {
                //        _mainCat = value;
                //        OnPropertyChanged(nameof(MainCat));
                //        SubCats = GetHKMatSubCats(_mainCat);
                //    }
                //}
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
                        //StrTypeAllP2 = GetAllP2StringDistinct(SubCats);
                        StrTypeAllP2 = StrTypeAllP1;
                        StrMainSpecT1All = string.Empty;
                        StrMainSpecT2All = string.Empty;
                        StrMainSpecT3All = string.Empty;
                        StrAuxSpecT1All = string.Empty;
                        StrAuxSpecT2All = string.Empty;
                        StrAuxSpecT3All = string.Empty;
                    }
                    else if (value != null)
                    {
                        StrTypeAllP1 = GetAllPortStringDistinct(value.TypeP1);
                        if (!HK_General.portDef.Contains(AlterCode))
                            StrTypeAllP2 = GetAllPortStringDistinct(value.TypeP2);
                        else
                            StrTypeAllP2 = StrTypeAllP1;
                        string[] _mainSpecTitle = value.TechSpecMain.Split(',').Select(item => item.Trim()).ToArray();
                        string[] mainSpecTitle = new string[3] { "", "", "" }; 
                        Array.Copy(_mainSpecTitle, 0, mainSpecTitle, 0, Math.Min(_mainSpecTitle.Length, 3));
                        StrMainSpecT1All = mainSpecTitle[0];
                        StrMainSpecT2All = mainSpecTitle[1];
                        StrMainSpecT3All = mainSpecTitle[2];
                        string[] _auxSpecTitle = value.TechSpecAux.Split(',').Select(item => item.Trim()).ToArray();
                        string[] auxSpecTitle = new string[3] { "", "", "" };
                        Array.Copy(_auxSpecTitle, 0, auxSpecTitle, 0, Math.Min(_auxSpecTitle.Length, 3));
                        StrAuxSpecT1All = auxSpecTitle[0];
                        StrAuxSpecT2All = auxSpecTitle[1];
                        StrAuxSpecT3All = auxSpecTitle[2];
                    }
                    OnPropertyChanged();
                    MatDataToQuery = getMatDataString();
                }
            }
        }
        public ObservableCollection<HKMatMainCat> MainCats
        {
            get => _mainCats;
            set => SetField(ref _mainCats, value);
        }
        public ObservableCollection<HKMatSubCat> SubCats
        {
            get => _subCats;
            set
            {
                string _id = SubCat?.ID ?? string.Empty;
                if (SetField(ref _subCats, value))
                    SubCat = SetCurrSelectedItem(value, _id, 0, SubCat);
           }
        }

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
                    TypeAllP1 = GetPortType(value);
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
                    TypeAllP2 = GetPortType(value);
                }
            }
        }
        public ObservableCollection<CmbItem> TypeAllP1
        {
            get => _typeAllP1;
            set
            {
                string _id = TypeP1?.ID ?? string.Empty;
                if (SetField(ref _typeAllP1, value))
                    TypeP1 = SetCurrSelectedItem(value, _id, 0, TypeP1);
            }
        }
        public ObservableCollection<CmbItem> TypeAllP2
        {
            get => _typeAllP2;
            set
            {
                string _id = (AlterCode == "AS1") ? TypeP1?.ID ?? string.Empty : TypeP2?.ID ?? string.Empty;
                if (SetField(ref _typeAllP2, value))
                    TypeP2 = SetCurrSelectedItem(value, _id, 0, TypeP2);
            }
            //    set
            //    {
            //        if (_typeAllP2 != value)
            //        {
            //            string _id = (AlterCode == "AS1") ? TypeP1?.ID ?? string.Empty : TypeP2?.ID ?? string.Empty;
            //            _typeAllP2 = value;
            //            OnPropertyChanged(nameof(TypeAllP2));
            //            TypeP2 = SetCurrSelectedItem(value, _id, 0, TypeP2);
            //        }
            //    }
        }
        public string TypeP1ID
        {
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
                if (_typeP1 != value && value !=null)
                {
                    SizeAllP1 = GetAllSizeOrSpec(value);
                    _typeP1 = value;
                    if (AlterCode == "AS1" && TypeAllP2?.Count >0)
                    {
                        string _id = TypeP1?.ID ?? string.Empty;
                        TypeP2 = SetCurrSelectedItem(TypeAllP2, _id, 0, TypeP2);
                    }
                    OnPropertyChanged(nameof(TypeP1));
                    MatDataToQuery = getMatDataString();
                }
            }
        }
        public CmbItem TypeP2
        {
            get => _typeP2;
            set
            {
                if (_typeP2 != value && value != null)
                {
                    SizeAllP2 = GetAllSizeOrSpec(value);
                    _typeP2 = value;
                    OnPropertyChanged(nameof(TypeP2));
                    MatDataToQuery = getMatDataString();
                }
            }
        }

        private CmbItem _sizeP1;
        private CmbItem _sizeP2;
        private ObservableCollection<CmbItem> _sizeAllP1;
        private ObservableCollection<CmbItem> _sizeAllP2;
        public ObservableCollection<CmbItem> SizeAllP1
        {
            get => _sizeAllP1;
            set
            {
                string _id = SizeP1?.ID ?? string.Empty;
                if (SetField(ref _sizeAllP1, value))
                    SizeP1 = SetCurrSelectedItem(value, _id, 0, SizeP1);
            }
        }
        public ObservableCollection<CmbItem> SizeAllP2
        {
            get => _sizeAllP2;
            set
            {
                string _id = (AlterCode == "AS1") ? SizeP1?.ID ?? string.Empty : SizeP2?.ID ?? string.Empty;
                if (SetField(ref _sizeAllP2, value))
                    SizeP2 = SetCurrSelectedItem(value, _id, 0, SizeP2);
            }
            //set
            //{
            //    if (_sizeAllP2 != value)
            //    {
            //        string _id = (AlterCode == "AS1") ? SizeP1?.ID ?? string.Empty : SizeP2?.ID ?? string.Empty;
            //        _sizeAllP2 = value;
            //        OnPropertyChanged();
            //        SizeP2 = SetCurrSelectedItem(value, _id, 0, SizeP2);
            //    }
            //}
        }
        public CmbItem SizeP1
        {
            get => _sizeP1;
            set
            {
                //string _id = SizeP1?.ID ?? string.Empty;
                if (SetField(ref _sizeP1, value)  && AlterCode == "AS1")
                    SizeP2 = SetCurrSelectedItem(SizeAllP2, SizeP1?.ID, 0, SizeP2);
            }
            //set
            //{
            //    if (_sizeP1 != value)
            //    {
            //        _sizeP1 = value;
            //        if (AlterCode == "AS1")
            //        {
            //            string _id = SizeP1?.ID ?? string.Empty;
            //            CmbItem _replacement = SizeAllP2?.FirstOrDefault(x => x.ID == _id);
            //            _replacement = _replacement ?? SizeAllP2?[0];
            //            SizeP2 = _replacement;
            //        }
            //        OnPropertyChanged(nameof(SizeP1));
            //    }
            //}
        }
        public CmbItem SizeP2
        {
            get => _sizeP2;
            set => SetField(ref _sizeP2, value);
        }
        public string SizeP1ID
        {
            set
            {
                value = value ?? string.Empty;
                if (SizeAllP1 != null && SizeAllP1.Count > 0)
                {
                    CmbItem _replacement = SizeAllP1.FirstOrDefault(x => x.ID == value);
                    _replacement = _replacement ?? SizeAllP1[0];
                    SizeP1 = _replacement;
                }
            }
        }
        public string SizeP2ID
        {
            set
            {
                value = value ?? string.Empty;
                if (SizeAllP2 != null && SizeAllP2.Count > 0)
                {
                    CmbItem _replacement = SizeAllP2.FirstOrDefault(x => x.ID == value);
                    _replacement = _replacement ?? SizeAllP2[0];
                    SizeP2 = _replacement;
                }
            }
        }

        private string _noLinkSpecTrigger;
        public string NoLinkSpecTrigger
        {
            get => _noLinkSpecTrigger;
            set
            {
                if (string.IsNullOrEmpty(value) && _noLinkSpecTrigger != value)
                {
                    var seg = value.Split('|');
                    if (!HK_General.dicNoLinkSpec[seg[0]].Any(x=>x.ID == seg[1]))
                    {
                        HKLibGenOption newSpec = new HKLibGenOption
                        {
                            ID = seg[1],
                            NameCn = seg[1],
                            NameEn = seg[1],
                        };
                        HK_General.dicNoLinkSpec[seg[0]].Add(newSpec);
                        var lst = HK_General.dicNoLinkSpec[seg[0]].Select(x => new CmbItem
                        {
                            ID = x.ID,
                            NameCn = x.NameCn,
                            NameEn = x.NameEn
                        }).ToList();
                         new ObservableCollection<CmbItem>(lst);
                    }
                    if (SizeAllP2 != null && SizeAllP2.Count > 0)
                    {
                        CmbItem _replacement = SizeAllP2.FirstOrDefault(x => x.ID == value);
                        _replacement = _replacement ?? SizeAllP2[0];
                        SizeP2 = _replacement;
                    }
                }
            }
        }


        private CmbItem _mainSpecT1;
        private string _strMainSpecT1All;
        private ObservableCollection<CmbItem> _mainSpecT1All;
        private CmbItem _mainSpecV1;
        private ObservableCollection<CmbItem> _mainSpecV1All;
        public CmbItem MainSpecT1
        {
            get => _mainSpecT1;
            set
            {
                if (_mainSpecT1 != value && value != null)
                {
                    MainSpecV1All = GetAllSizeOrSpec(value);
                    _mainSpecT1 = value;
                    OnPropertyChanged();
                    MatDataToQuery = getMatDataString();
                }
            }
        }
        public string StrMainSpecT1All
        {
            get => _strMainSpecT1All;
            set
            {
                if (_strMainSpecT1All != value)
                {
                   MainSpecT1All = GetSpecTitle(value);
                    _strMainSpecT1All = value;
                }
            }
        }
        public ObservableCollection<CmbItem> MainSpecT1All
        {
            get => _mainSpecT1All;
            set
            {
                string _id = MainSpecT1?.ID ?? string.Empty;
                if (SetField(ref _mainSpecT1All, value))
                    MainSpecT1 = SetCurrSelectedItem(value,_id, 0, MainSpecT1);
            }
        }
        public CmbItem MainSpecV1
        {
            get => _mainSpecV1;
            set => SetField(ref _mainSpecV1, value);
        }
        public ObservableCollection<CmbItem> MainSpecV1All
        {
            get => _mainSpecV1All;
            set
            {
                string _id = MainSpecV1?.ID ?? string.Empty;
                if (SetField(ref _mainSpecV1All, value))
                    MainSpecV1 = SetCurrSelectedItem(value,_id, 0, MainSpecV1);
            }
        }

        private CmbItem _mainSpecT2;
        private string _strMainSpecT2All;
        private ObservableCollection<CmbItem> _mainSpecT2All;
        private CmbItem _mainSpecV2;
        private ObservableCollection<CmbItem> _mainSpecV2All;
        public CmbItem MainSpecT2
        {
            get => _mainSpecT2;
            set
            {
                if (_mainSpecT2 != value && value != null)
                {
                    MainSpecV2All = GetAllSizeOrSpec(value);
                    _mainSpecT2 = value;
                    OnPropertyChanged();
                    MatDataToQuery = getMatDataString();
                }
            }
        }
        public string StrMainSpecT2All
        {
            get => _strMainSpecT2All;
            set
            {
                if (_strMainSpecT2All != value)
                {
                    MainSpecT2All = GetSpecTitle(value);
                    _strMainSpecT2All = value;
                }
            }
        }
        public ObservableCollection<CmbItem> MainSpecT2All
        {
            get => _mainSpecT2All;
            set
            {
                string _id = MainSpecT2?.ID ?? string.Empty;
                if (SetField(ref _mainSpecT2All, value))
                    MainSpecT2 = SetCurrSelectedItem(value, _id, 0, MainSpecT2);
            }
        }
        public CmbItem MainSpecV2
        {
            get => _mainSpecV2;
            set => SetField(ref _mainSpecV2, value);
        }
        public ObservableCollection<CmbItem> MainSpecV2All
        {
            get => _mainSpecV2All;
            set
            {
                string _id = MainSpecV2?.ID ?? string.Empty;
                if (SetField(ref _mainSpecV2All, value))
                    MainSpecV2 = SetCurrSelectedItem(value, _id, 0, MainSpecV2);
            }
        }

        private CmbItem _mainSpecT3;
        private string _strMainSpecT3All;
        private ObservableCollection<CmbItem> _mainSpecT3All;
        private CmbItem _mainSpecV3;
        private ObservableCollection<CmbItem> _mainSpecV3All;
        public CmbItem MainSpecT3
        {
            get => _mainSpecT3;
            set
            {
                if (_mainSpecT3 != value && value != null)
                {
                    MainSpecV3All = GetAllSizeOrSpec(value);
                    _mainSpecT3 = value;
                    OnPropertyChanged();
                    MatDataToQuery = getMatDataString();
                }
            }
        }
        public string StrMainSpecT3All
        {
            get => _strMainSpecT3All;
            set
            {
                if (_strMainSpecT3All != value)
                {
                    MainSpecT3All = GetSpecTitle(value);
                    _strMainSpecT3All = value;
                }
            }
        }
        public ObservableCollection<CmbItem> MainSpecT3All
        {
            get => _mainSpecT3All;
            set
            {
                string _id = MainSpecT3?.ID ?? string.Empty;
                if (SetField(ref _mainSpecT3All, value))
                    MainSpecT3 = SetCurrSelectedItem(value, _id, 0, MainSpecT3);
            }
        }
        public CmbItem MainSpecV3
        {
            get => _mainSpecV3;
            set => SetField(ref _mainSpecV3, value);
        }
        public ObservableCollection<CmbItem> MainSpecV3All
        {
            get => _mainSpecV3All;
            set
            {
                string _id = MainSpecV3?.ID ?? string.Empty;
                if (SetField(ref _mainSpecV3All, value))
                    MainSpecV3 = SetCurrSelectedItem(value, _id, 0, MainSpecV3);
            }
        }

        private CmbItem _auxSpecT1;
        private string _strAuxSpecT1All;
        private ObservableCollection<CmbItem> _auxSpecT1All;
        private CmbItem _auxSpecV1;
        private ObservableCollection<CmbItem> _auxSpecV1All;
        public CmbItem AuxSpecT1
        {
            get => _auxSpecT1;
            set
            {
                if (_auxSpecT1 != value && value != null)
                {
                    AuxSpecV1All = GetAllSizeOrSpec(value);
                    _auxSpecT1 = value;
                    OnPropertyChanged();
                    MatDataToQuery = getMatDataString();
                }
            }
        }
        public string StrAuxSpecT1All
        {
            get => _strAuxSpecT1All;
            set
            {
                if (_strAuxSpecT1All != value)
                {
                    AuxSpecT1All = GetSpecTitle(value);
                    _strAuxSpecT1All = value;
                }
            }
        }
        public ObservableCollection<CmbItem> AuxSpecT1All
        {
            get => _auxSpecT1All;
            set
            {
                string _id = AuxSpecT1?.ID ?? string.Empty;
                if (SetField(ref _auxSpecT1All, value))
                    AuxSpecT1 = SetCurrSelectedItem(value, _id, 0, AuxSpecT1);
             }
        }
        public CmbItem AuxSpecV1
        {
            get => _auxSpecV1;
            set => SetField(ref _auxSpecV1, value);
        }
        public ObservableCollection<CmbItem> AuxSpecV1All
        {
            get => _auxSpecV1All;
            set
            {
                string _id = AuxSpecV1?.ID ?? string.Empty;
                if (SetField(ref _auxSpecV1All, value))
                    AuxSpecV1 = SetCurrSelectedItem(value, _id, 0, AuxSpecV1);
            }
        }

        private CmbItem _auxSpecT2;
        private string _strAuxSpecT2All;
        private ObservableCollection<CmbItem> _auxSpecT2All;
        private CmbItem _auxSpecV2;
        private ObservableCollection<CmbItem> _auxSpecV2All;
        public CmbItem AuxSpecT2
        {
            get => _auxSpecT2;
            set
            {
                if (_auxSpecT2 != value && value != null)
                {
                    AuxSpecV2All = GetAllSizeOrSpec(value);
                    _auxSpecT2 = value;
                    OnPropertyChanged();
                    MatDataToQuery = getMatDataString();
                }
            }
        }
        public string StrAuxSpecT2All
        {
            get => _strAuxSpecT2All;
            set
            {
                if (_strAuxSpecT2All != value)
                {
                    AuxSpecT2All = GetSpecTitle(value);
                    _strAuxSpecT2All = value;
                }
            }
        }
        public ObservableCollection<CmbItem> AuxSpecT2All
        {
            get => _auxSpecT2All;
            set
            {
                string _id = AuxSpecT2?.ID ?? string.Empty;
                if (SetField(ref _auxSpecT2All, value))
                    AuxSpecT2 = SetCurrSelectedItem(value, _id, 0, AuxSpecT2);
            }
        }
        public CmbItem AuxSpecV2
        {
            get => _auxSpecV2;
            set => SetField(ref _auxSpecV2, value);
        }
        public ObservableCollection<CmbItem> AuxSpecV2All
        {
            get => _auxSpecV2All;
            set
            {
                string _id = AuxSpecV2?.ID ?? string.Empty;
                if (SetField(ref _auxSpecV2All, value))
                    AuxSpecV2 = SetCurrSelectedItem(value, _id, 0, AuxSpecV2);
            }
        }

        private CmbItem _auxSpecT3;
        private string _strAuxSpecT3All;
        private ObservableCollection<CmbItem> _auxSpecT3All;
        private CmbItem _auxSpecV3;
        private ObservableCollection<CmbItem> _auxSpecV3All;
        public CmbItem AuxSpecT3
        {
            get => _auxSpecT3;
            set
            {
                if (_auxSpecT3 != value && value != null)
                {
                    AuxSpecV3All = GetAllSizeOrSpec(value);
                    _auxSpecT3 = value;
                    OnPropertyChanged();
                    MatDataToQuery = getMatDataString();
                }
            }
        }
        public string StrAuxSpecT3All
        {
            get => _strAuxSpecT3All;
            set
            {
                if (_strAuxSpecT3All != value)
                {
                    AuxSpecT3All = GetSpecTitle(value);
                    _strAuxSpecT3All = value;
                }
            }
        }
        public ObservableCollection<CmbItem> AuxSpecT3All
        {
            get => _auxSpecT3All;
            set
            {
                string _id = AuxSpecT3?.ID ?? string.Empty;
                if (SetField(ref _auxSpecT3All, value))
                    AuxSpecT3 = SetCurrSelectedItem(value, _id, 0, AuxSpecT3);
            }
        }
        public CmbItem AuxSpecV3
        {
            get => _auxSpecV3;
            set => SetField(ref _auxSpecV3, value);
        }
        public ObservableCollection<CmbItem> AuxSpecV3All
        {
            get => _auxSpecV3All;
            set
            {
                string _id = AuxSpecV3?.ID ?? string.Empty;
                if (SetField(ref _auxSpecV3All, value))
                    AuxSpecV3 = SetCurrSelectedItem(value, _id, 0, AuxSpecV3);
            }
        }


        public string MainSpecT2ID
        {
            set
            {
                value = value ?? string.Empty;
                if (MainSpecT2All != null && MainSpecT2All.Count > 0)
                {
                    CmbItem _replacement = MainSpecT2All.FirstOrDefault(x => x.ID == value);
                    _replacement = _replacement ?? MainSpecT2All[0];
                    MainSpecT2 = _replacement;
                }
            }
        }
        public string MainSpecV2ID
        {
            set
            {
                value = value ?? string.Empty;
                if (MainSpecV2All != null && MainSpecV2All.Count > 0)
                {
                    CmbItem _replacement = MainSpecV2All.FirstOrDefault(x => x.ID == value);
                    _replacement = _replacement ?? MainSpecV2All[0];
                    MainSpecV2 = _replacement;
                }
            }
        }


        private string _matMatAll;
        private string _moreSpecCn;
        private string _moreSpecEn;
        private string _remarksEn;
        private string _remarksCn;
        private string _alterCode;


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
            set => SetField(ref _moreSpecCn, value);
        }
        public string MoreSpecEn
        {
            get => _moreSpecEn;
            set => SetField(ref _moreSpecEn, value);
        }
        public string RemarksCn
        {
            get => _remarksCn;
            set => SetField(ref _remarksCn, value);
        }
        public string RemarksEn
        {
            get => _remarksEn;
            set => SetField(ref _remarksEn, value);
        }
        public string AlterCode
        {
            get => _alterCode;
            set => SetField(ref _alterCode, value);
        }

        private string _matDataToQuery;
        private DispatcherTimer _debounceTimer;
        public string MatDataToQuery
        {
            get => _matDataToQuery;
            set
            {
                if (_matDataToQuery != value)
                {
                    // 重置去抖动计时器
                    _debounceTimer?.Stop();
                    _debounceTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(100) // 设置适当的延迟时间
                    };
                    _debounceTimer.Tick += (s, e) =>
                    {
                        _debounceTimer.Stop();
                        _matDataToQuery = value;
                        OnPropertyChanged();
                    };
                    _debounceTimer.Start();
                    // 触发自定义事件
                    DataChanged?.Invoke(this, value);
                    //Debug.WriteLine($"子控件数据已更新: {value}");
                }
            }
        }
        private string _matDataFromQuery;
        public string MatDataFromQuery
        {
            set
            {
                Debug.WriteLine($"收到Query结果: {value}");
                // 0:CatID, 1:SubCatID, 2:TechSpecMain, 3:TechSpecAux, 4:TypeP1, 5:SizeP1, 6:TypeP2, 7:SizeP2, 8:MoreSpecCn, 9:MoreSpecEn, 10:RemarksCn, 11: RemarksEn, 12, PClass, 13:MatSpec, 14,Status
                var arrMatData = value.Split(',').ToArray<string>();
                string[] seg;
                MainCat = SetCurrSelectedItem<HKMatMainCat>(MainCats, arrMatData[0], 0);
                SubCat = SetCurrSelectedItem<HKMatSubCat>(SubCats, arrMatData[1], 0);
                var arrMainSpec = arrMatData[2].Split('|').ToArray<string>();
                if (arrMainSpec.Length > 0 && arrMainSpec[0].Contains(":"))
                {
                    MainSpecT1 = SetCurrSelectedItem<CmbItem>(MainSpecT1All, arrMainSpec[0].Split(';')[0], 0);
                    seg = arrMainSpec[0].Split(':');
                    if (HK_General.dicNoLinkSpec.ContainsKey(seg[0]))
                    {
                        SetNoLinkDic(seg[0], seg[1]);
                        SetCmbItems(MainSpecV1All, seg[0]);
                    }
                    MainSpecV1 = SetCurrSelectedItem<CmbItem>(MainSpecV1All, seg[1], 0);
                }
                if (arrMainSpec.Length > 1 && arrMainSpec[1].Contains(":"))
                {
                    MainSpecT2 = SetCurrSelectedItem<CmbItem>(MainSpecT2All, arrMainSpec[0].Split(';')[0], 0);
                    seg = arrMainSpec[1].Split(':');
                    if (HK_General.dicNoLinkSpec.ContainsKey(seg[0]))
                    {
                        SetNoLinkDic(seg[0], seg[1]);
                        SetCmbItems(MainSpecV2All, seg[0]);
                    }
                    MainSpecV2 = SetCurrSelectedItem<CmbItem>(MainSpecV2All, seg[1], 0);
                }
                if (arrMainSpec.Length > 2 && arrMainSpec[2].Contains(":"))
                {
                    MainSpecT3 = SetCurrSelectedItem<CmbItem>(MainSpecT3All, arrMainSpec[0].Split(';')[0], 0);
                    seg = arrMainSpec[2].Split(':');
                    if (HK_General.dicNoLinkSpec.ContainsKey(seg[0]))
                    {
                        SetNoLinkDic(seg[0], seg[1]);
                        SetCmbItems(MainSpecV3All, seg[0]);
                    }
                    MainSpecV3 = SetCurrSelectedItem<CmbItem>(MainSpecV3All, seg[1], 0);
                }
                var arrAuxSpec = arrMatData[3].Split('|').ToArray<string>();
                if (arrAuxSpec.Length > 0 && arrAuxSpec[0].Contains(":"))
                {
                    AuxSpecT1 = SetCurrSelectedItem<CmbItem>(AuxSpecT1All, arrAuxSpec[0].Split(';')[0], 0);
                    seg = arrAuxSpec[0].Split(':');
                    if (HK_General.dicNoLinkSpec.ContainsKey(seg[0]))
                    {
                        SetNoLinkDic(seg[0], seg[1]);
                        SetCmbItems(AuxSpecV1All, seg[0]);
                    }
                    AuxSpecV1 = SetCurrSelectedItem<CmbItem>(AuxSpecV1All, seg[1], 0);
                }
                if (arrAuxSpec.Length > 1 && arrAuxSpec[1].Contains(":"))
                {
                    AuxSpecT2 = SetCurrSelectedItem<CmbItem>(AuxSpecT2All, arrAuxSpec[0].Split(';')[0], 0);
                    seg = arrAuxSpec[1].Split(':');
                    if (HK_General.dicNoLinkSpec.ContainsKey(seg[0]))
                    {
                        SetNoLinkDic(seg[0], seg[1]);
                        SetCmbItems(AuxSpecV2All, seg[0]);
                    }
                    AuxSpecV2 = SetCurrSelectedItem<CmbItem>(AuxSpecV2All, seg[1], 0);
                }
                if (arrAuxSpec.Length > 2 && arrAuxSpec[2].Contains(":"))
                {
                    AuxSpecT3 = SetCurrSelectedItem<CmbItem>(AuxSpecT3All, arrAuxSpec[0].Split(';')[0], 0);
                    seg = arrAuxSpec[2].Split(':');
                    if (HK_General.dicNoLinkSpec.ContainsKey(seg[0]))
                    {
                        SetNoLinkDic(seg[0], seg[1]);
                        SetCmbItems(AuxSpecV3All, seg[0]);
                    }
                    AuxSpecV3 = SetCurrSelectedItem<CmbItem>(AuxSpecV3All, seg[1], 0);
                }
                TypeP1 = SetCurrSelectedItem<CmbItem>(TypeAllP1, arrMatData[4], 0);
                SizeP1 = SetCurrSelectedItem<CmbItem>(SizeAllP1, arrMatData[5], 0);
                TypeP2 = SetCurrSelectedItem<CmbItem>(TypeAllP2, arrMatData[6], 0);
                SizeP2 = SetCurrSelectedItem<CmbItem>(SizeAllP2, arrMatData[7], 0);
                MoreSpecCn = arrMatData[8];
                MoreSpecEn = arrMatData[9];
                RemarksCn = arrMatData[10];
                RemarksEn = arrMatData[11];
            }
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            MatDataToQuery = getMatDataString();
            return true;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //protected virtual void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        //protected bool SetField<T>(ref T field, T value, [CallerMemberName] string name = null)
        //{
        //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        //    field = value;
        //    OnPropertyChanged(name);
        //    return true;
        //}

        // 按键处理命令
        public ICommand ResetAllCommand { get; }
        public ICommand ResetSpecCommand { get; }
        public ICommand ResetMoreCommand { get; }
        private void DataResetAll()
        {
            MainCat = MainCats[0];
            SubCat = SubCats[0];
            MainSpecT1 = null;
            MainSpecT2 = null;
            MainSpecT3 = null;
            MainSpecV1 = null;
            MainSpecV2 = null;  
            MainSpecV3 = null;
            AuxSpecT1 = null;
            AuxSpecT2 = null;
            AuxSpecT3 = null;
            AuxSpecV1 = null;
            AuxSpecV2 = null;
            AuxSpecV3 = null;
            TypeP1 = TypeAllP1[0];
            TypeP2 = TypeAllP2[0];
            SizeP1 = null;
            SizeP2 = null;
            MoreSpecCn = null;
            MoreSpecEn = null;
            RemarksCn = null;
            RemarksEn= null;
        }
        private void DataResetSpec()
        {
            MainSpecT1 = null;
            MainSpecT2 = null;
            MainSpecT3 = null;
            MainSpecV1 = null;
            MainSpecV2 = null;
            MainSpecV3 = null;
            AuxSpecT1 = null;
            AuxSpecT2 = null;
            AuxSpecT3 = null;
            AuxSpecV1 = null;
            AuxSpecV2 = null;
            AuxSpecV3 = null;
            TypeP1 = TypeAllP1[0];
            TypeP2 = TypeAllP2[0];
            SizeP1 = null;
            SizeP2 = null;
        }
        private void DataResetMore()
        {
            MoreSpecCn = null;
            MoreSpecEn = null;
            RemarksCn = null;
            RemarksEn = null;
        }
        public ICommand KeyDownCommand { get; }
        private void HandleKeyDownSpec(KeyEventArgs e)
        {

            if (e.Key == Key.Enter) // 示例：按Enter键时处理
            {
                // 更安全的方式获取 ComboBox
                var comboBox = e.Source as ComboBox ?? e.OriginalSource as ComboBox;

                if (comboBox != null)
                {
                    // 确保绑定更新
                    //var binding = comboBox.GetBindingExpression(ComboBox.TextProperty);
                    //binding?.UpdateSource();
                    bool isValid = true;
                    string value = comboBox.Text?.Trim();
                    var titleItem = (comboBox.DataContext as CmbItem);
                    string key = titleItem.ID;
                    var cmbItems = comboBox.ItemsSource as ObservableCollection<CmbItem>;
                    
                    
                    
                    switch(titleItem.Class)
                    {
                        case "NumItems":
                            isValid = GeneralFun.ValidateNumberItemsFormat(value, 'x', 2);
                            Debug.WriteLine($"错误格式: {value}");
                            break;
                    }
                    SetNoLinkDic(key, value);
                    SetCmbItems(cmbItems, key);
                    comboBox.SelectedItem  = SetCurrSelectedItem(cmbItems, value, -1);
                }
                e.Handled = true; // 标记事件已处理
            }
        }

        private void SetNoLinkDic(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrEmpty(value)) return;
            if (HK_General.dicNoLinkSpec[key].Any(x=>x.ID ==  value)) return;
            HK_General.dicNoLinkSpec[key].Add(new HKLibGenOption
            {
                ID = value,
                NameCn = value,
                NameEn = value,
            });
        }

        private void SetCmbItems(ObservableCollection<CmbItem> items, string key)
        {
            if (string.IsNullOrWhiteSpace(key) || items == null) return;
            items.Clear();
            foreach (var item in HK_General.dicNoLinkSpec[key])
            {
                items.Add(new CmbItem
                {
                    ID = item.ID,
                    NameCn = item.NameCn,
                    NameEn = item.NameEn,
                });
            }
        }

        private CmbItem SetCurrSelectedItem(ObservableCollection<CmbItem> sourceCollection,
                                          string targetId,
                                          int defIndex = 0,
                                          CmbItem defaultValue = null)
        {
            // 验证输入
            if (sourceCollection == null || sourceCollection.Count == 0 || targetId == null)
                return defaultValue;

            // 尝试通过ID查找
            var matchedItem = sourceCollection.FirstOrDefault(item => item.ID == targetId);

            if (matchedItem != null)
                return matchedItem;

            // 后备索引查找
            if (defIndex >= 0 && defIndex < sourceCollection.Count)
                return sourceCollection[defIndex];
            else
                return null;
        }
        private HKMatSubCat SetCurrSelectedItem(ObservableCollection<HKMatSubCat> sourceCollection,
                                          string targetId,
                                          int defIndex = 0,
                                          HKMatSubCat defaultValue = null)
        {
            // 验证输入
            if (sourceCollection == null || sourceCollection.Count == 0 || targetId == null)
                return defaultValue;

            // 尝试通过ID查找
            var matchedItem = sourceCollection.FirstOrDefault(item => item.ID == targetId);

            if (matchedItem != null)
                return matchedItem;

            // 后备索引查找
            if (defIndex >= 0 && defIndex < sourceCollection.Count)
                return sourceCollection[defIndex];
            else
                return null;
        }
        public interface IIdentifiable
        {
            string ID { get; }
        }
        
        private T SetCurrSelectedItem<T>(ObservableCollection<T> sourceCollection,
                                        string targetId,
                                        int defIndex = 0,
                                        T defaultValue = default) where T : class, IIdentifiable
        {
            // 验证输入
            if (sourceCollection == null || sourceCollection.Count == 0 || targetId == null)
                return defaultValue;

            // 尝试通过ID查找
            var matchedItem = sourceCollection.FirstOrDefault(item => item.ID == targetId);

            if (matchedItem != null)
                return matchedItem;

            // 后备索引查找
            if (defIndex >= 0 && defIndex < sourceCollection.Count)
                return sourceCollection[defIndex];

            return null;
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
                    MessageBox.Show($"{nameof(MatDataViewModel)}.{nameof(GetHKMatMainCats)}{Environment.NewLine}Error: {ex.Message}");
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
         private ObservableCollection<CmbItem> GetPortType(string input)
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
        private ObservableCollection<CmbItem> GetSpecTitle(string input)
        {
            List<CmbItem> lst = HK_General.dicSpecDic.Select(x => x.Value).Where(x => input.Split('|').Contains(x.ID)).OrderBy(x => x.SortNum).Select(x => new CmbItem
            {
                ID = x.ID,
                NameCn = x.NameCn,
                NameEn = x.NameEn,
                Comp = x.ID,
                Class = x.Class,
                Link = x.Link,

            }).ToList();
            return new ObservableCollection<CmbItem>(lst);
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
                    case "LibPN":
                        lst = getCmbItemsPNAll(HK_General.dicPN, segParts[1].Trim(), segParts[2]);
                        break;
                    case "LibGland":
                        lst = getCmbItemsGlandAll(HK_General.dicGland, segParts[1].Trim(), segParts[2]);
                        break;
                    case "LibSteel":
                        lst = getCmbItemsSteelAll(HK_General.dicSteel, segParts[1].Trim(), segParts[2]);
                        break;
                    case "LibGenOption":
                        lst = getCmbItemsGenOptionAll(HK_General.dicGenOption, segParts[1].Trim(), segParts[2]);
                        break;


                }
                return getSizeOrSpecLinked(lst, segParts[2].Trim());
            }
            else
            {
                lst = HK_General.dicNoLinkSpec[title.ID].Select(x=> new CmbItem
                {
                    ID = x.ID,
                    NameCn= x.NameCn,
                    NameEn= x.NameEn
                }).ToList();
                return new ObservableCollection<CmbItem>(lst);
            }
        }


        private List<CmbItem> getCmbItemsPipeODAll(Dictionary<string, HKLibPipeOD> dic, string name, string cond)
        {
            string comp = cond.Split(':')[0];
            return dic.Select(x => x.Value).OrderBy(x => x.SortNum).Select(x => new CmbItem
            {
                ID = x.ID,
                NameCn = name == "DN" ? $"DN {x.DN} - NPS {x.NPS}"
                                             : name == "NPS" ? $"NPS {x.NPS} - DN {x.DN}"
                                             : name == "HGIa" ? $"DN {x.DN} - Φ{x.HGIa}"
                                             : name == "HGIb" ? $"DN {x.DN} - Φ{x.HGIb}"
                                             : name == "HGII" ? $"DN {x.DN} - Φ{x.HGII}"
                                             : name == "GBI" ? $"DN {x.DN} - Φ{x.GBI}"
                                             : name == "GBII" ? $"DN {x.DN} - Φ{x.GBII}"
                                             : name == "ISO" ? $"DN {x.DN} - Φ{x.ISO}"
                                             : $"DN {x.DN} - NPS {x.NPS}",
                NameEn = name == "DN" ? $"DN {x.DN} - NPS {x.NPS}"
                                             : name == "NPS" ? $"NPS {x.NPS} - DN {x.DN}"
                                             : name == "HGIa" ? $"DN {x.DN} - Φ{x.HGIa}"
                                             : name == "HGIb" ? $"DN {x.DN} - Φ{x.HGIb}"
                                             : name == "HGII" ? $"DN {x.DN} - Φ{x.HGII}"
                                             : name == "GBI" ? $"DN {x.DN} - Φ{x.GBI}"
                                             : name == "GBII" ? $"DN {x.DN} - Φ{x.GBII}"
                                             : name == "ISO" ? $"DN {x.DN} - Φ{x.ISO}"
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
        private List<CmbItem> getCmbItemsPNAll(Dictionary<string, HKLibPN> dic, string name, string cond)
        {
            string comp = cond.Split(':')[0];
            return dic.Select(x => x.Value).OrderBy(x => x.SortNum).Select(x => new CmbItem
            {
                ID = x.ID,
                NameCn = x.SpecCn,
                NameEn = x.SpecEn,
                Comp = comp == "Class" ? x.Class
                                             : comp == "SpecCn" ? x.SpecCn
                                             : comp == "SpecEn" ? x.SpecEn
                                             : comp == "ISOS1" ? x.ISOS1
                                             : comp == "ISOS2" ? x.ISOS2
                                             : comp == "GBDIN" ? x.GBDIN
                                             : comp == "GBANSI" ? x.GBANSI
                                             : comp == "ASME" ? x.ASME
                                             : x.ID,
            }).ToList();
        }
        private List<CmbItem> getCmbItemsGlandAll(Dictionary<string, HKLibGland> dic, string name, string cond)
        {
            string comp = cond.Split(':')[0];
            return dic.Select(x => x.Value).OrderBy(x => x.SortNum).Select(x => new CmbItem
            {
                ID = x.ID,
                NameCn = x.SpecCn,
                NameEn = x.SpecEn,
                Comp = comp == "Class" ? x.Class
                                             : comp == "SpecCn" ? x.SpecCn
                                             : comp == "SpecEn" ? x.SpecEn
                                             : comp == "Value" ? x.CabODMin.ToString()
                                             : comp == "Pitch" ? x.CabODMax.ToString()
                                             : comp == "ClassEx" ? x.ClassEx
                                             : x.ID,
            }).ToList();
        }
        private List<CmbItem> getCmbItemsSteelAll(Dictionary<string, HKLibSteel> dic, string name, string cond)
        {
            string comp = cond.Split(':')[0];
            return dic.Select(x => x.Value).OrderBy(x => x.SortNum).Select(x => new CmbItem
            {
                ID = x.ID,
                NameCn = (name == "CSSpec" || name == "CS") ? x.CSSpecCn
                                             : (name == "IBSpec" || name == "IB") ? x.IBSpecCn
                                             : x.CSSpecCn,
                NameEn = (name == "CSSpec" || name == "CS") ? x.CSSpecEn
                                             : (name == "IBSpec" || name == "IB") ? x.IBSpecEn
                                             : x.CSSpecEn,
                Comp = comp == "Width" ? x.Width.ToString()
                                             : (comp == "CSSpec" || comp == "CS" || comp == "CSSpecCn") ? x.CSSpecCn
                                             : comp == "CSSpecEn" ? x.CSSpecEn
                                             : (comp == "IBSpec" || comp == "IB" || comp == "IBSpecCn") ? x.IBSpecCn
                                             : comp == "IBSpecEn" ? x.IBSpecEn
                                             : comp == "CSb" ? x.CSb.ToString()
                                             : comp == "CSd" ? x.CSd.ToString()
                                             : comp == "IBb" ? x.IBb.ToString()
                                             : comp == "IBd" ? x.IBd.ToString()
                                             : x.ID,
            }).ToList();
        }
        private List<CmbItem> getCmbItemsGenOptionAll(Dictionary<string, HKLibGenOption> dic, string name, string cond)
        {
            string comp = cond.Split(':')[0];
            return dic.Select(x => x.Value).OrderBy(x => x.SortNum).Select(x => new CmbItem
            {
                ID = x.ID,
                NameCn = x.NameCn,
                NameEn = x.NameEn,
                Comp = comp == "Cat" ? x.Cat
                                             : comp == "NameCn" ? x.NameCn
                                             : comp == "NameEn" ? x.NameEn
                                             : comp == "SpecCn" ? x.SpecCn
                                             : comp == "SpecEn" ? x.SpecEn
                                             : comp == "Inact" ? x.Inact.ToString()
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
                NameCn = "规格或尺寸",
                NameEn = "Spec.or Size",
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
        private string getMatDataString()
        {
            if (SubCat == null) return null;
            // 0:CatID, 1:SubCatID, 2:TechSpecMain, 3:TechSpecAux, 4:TypeP1, 5:SizeP1, 6:TypeP2, 7:SizeP2, 8:NoreSpecCn, 9:NoreSpecEn, 10:RemarksCn, 11: RemarksEn, 12, PClass, 13:MatSpec, 14,Status
            string techSpecMain = getMainSpec();
            string techSpecAux = getAuxSpec();
            string pClass = getPClass($"{techSpecMain}|{techSpecAux}");
            return $"{MainCat?.ID},{SubCat.ID}," +
                   $"{techSpecMain}," +
                   $"{techSpecAux}," +
                   //$"{((TypeAllP1 == null) ? null : string.Join("|", TypeAllP1.Select(x => x.ID).Where(x => !string.IsNullOrEmpty(x)).ToList()))}," +
                   $"{TypeP1?.ID}," +
                   $"{SizeP1?.ID}," +
                   //$"{((TypeAllP2 == null) ? null : string.Join("|", TypeAllP2.Select(x => x.ID).Where(x => !string.IsNullOrEmpty(x)).ToList()))}," +
                   $"{TypeP2?.ID}," +
                   $"{SizeP2?.ID}," +
                   $"{MoreSpecCn}," +
                   $"{MoreSpecEn}," +
                   $"{RemarksCn}," +
                   $"{RemarksEn}," +
                   $"{pClass}," +
                   $"{MatMatAll}," +
                   $"," +
                   $"";
        }
        private string getMainSpec()
        {
            List<string> specs = new List<string>();
            if (MainSpecT1All?.Count > 0)
            {
                specs.Add($"{MainSpecT1?.ID}:{MainSpecV1?.ID}");
                if (MainSpecT2All?.Count > 0)
                {
                    specs.Add($"{MainSpecT2?.ID}:{MainSpecV2?.ID}");
                    if (MainSpecT3All?.Count > 0)
                    {
                        specs.Add($"{MainSpecT3?.ID}:{MainSpecV3?.ID}");
                    }
                }
            }
            return string.Join("|", specs);
        }
        private string getAuxSpec()
        {
            List<string> specs = new List<string>();
            if (AuxSpecT1All?.Count > 0)
            {
                specs.Add($"{AuxSpecT1?.ID}:{AuxSpecV1?.ID}");
                if (AuxSpecT2All?.Count > 0)
                {
                    specs.Add($"{AuxSpecT2?.ID}:{AuxSpecV2?.ID}");
                    if (AuxSpecT3All?.Count > 0)
                    {
                        specs.Add($"{AuxSpecT3?.ID}:{AuxSpecV3?.ID}");
                    }
                }
            }
            return string.Join("|", specs);
        }
        private string getPClass(string input)
        {
            return input.Split('|').FirstOrDefault(x => x.StartsWith("PN"))?.Split(':')[1];
        }
    }
}
