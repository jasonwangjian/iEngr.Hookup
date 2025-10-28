using Comos.Controls;
using Comos.UIF;
using ComosQueryInterface;
using ComosQueryXObj;
using iEngr.Hookup.Models;
using iEngr.Hookup.Views;
using Plt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class BomItemsViewModel : INotifyPropertyChanged
    {
        public event EventHandler<string> SelectedBomIDChanged;

        public BomItemsViewModel()
        {
            DataSource = new ObservableCollection<BomItem>();
            SetDisciplineSource();
            SetResponsibleSource();
            SetUnitSource();
            MatMats = HK_General.dicMatMat.Select(x => x.Value).ToList();
            CellEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(HandleCellEditEnding);
            SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(HandleSelectionChanged);
            MoveUpCommand = new RelayCommand<object>(_ => MoveUp(), _ => CanMoveUp());
            MoveDownCommand = new RelayCommand<object>(_ => MoveDown(), _ => CanMoveDown());
            MoveToFirstCommand = new RelayCommand<object>(_ => MoveToFirst(), _ => CanMoveUp());
            MoveToLastCommand = new RelayCommand<object>(_ => MoveToLast(), _ => CanMoveDown());
            NewAddCommand = new RelayCommand<object>(_ => NewAdd(), _ => SelectedMatListItem != null && (CurrentObject != null || SelectedDiagramItem != null));
            UpdateCommand = new RelayCommand<object>(_ => Update(), _ => !string.IsNullOrEmpty(SelectedItem?.ID));
            AlterCommand = new RelayCommand<object>(_ => Alter(), _ => SelectedItem != null && SelectedMatListItem != null);
            DeleteCommand = new RelayCommand<object>(_ => Delete(), _ => SelectedItems?.Count > 0);
            AutoComosUpdate = HK_General.IsAutoComosUpdate;
            IsLangCtrlShown = true;
            IsButtonShown = true;
            LangInChinese = true;
        }
        public List<GeneralItem> Disciplines { get; set; }
        public List<GeneralItem> Responsibles { get; set; }
        public List<GeneralItem> Units { get; set; }
        private void SetDisciplineSource()
        {
            Disciplines = new List<GeneralItem>
            {
                new GeneralItem
                {
                    Code = "I",
                    NameCn = "仪表",
                    NameEn = "Instrument"
                },
                new GeneralItem
                {
                    Code = "F",
                    NameCn = "工艺",
                    NameEn="Process"
                },
                new GeneralItem
                {
                    Code = "E",
                    NameCn = "电气",
                    NameEn = "Electric"
                }
            };
        }
        private void SetResponsibleSource()
        {
            Responsibles = new List<GeneralItem>
            {
                new GeneralItem
                {
                    Code = "B",
                    NameCn = "业主方",
                    NameEn="Buyer"
                },
                new GeneralItem
                {
                    Code = "S",
                    NameCn = "供货商",
                    NameEn="Supplier"
                },
                new GeneralItem
                {
                    Code = "M",
                    NameCn = "制造商",
                    NameEn="Maker"
                },
                new GeneralItem
                {
                    Code = "E",
                    NameCn = "安装方",
                    NameEn="Erector"
                }
            };
        }
        private void SetUnitSource()
        {
            Units = new List<GeneralItem>
            {
                new GeneralItem
                {
                    Code = "pcs",
                    NameCn = "只",
                    NameEn="pcs"
                },
                new GeneralItem
                {
                    Code = "set",
                    NameCn = "套",
                    NameEn="set"
                },
                new GeneralItem
                {
                    Code = "pkg",
                    NameCn = "包",
                    NameEn="pkg"
                },
                new GeneralItem
                {
                    Code = "m",
                    NameCn = "米",
                    NameEn="m"
                },
                new GeneralItem
                {
                    Code = "m2",
                    NameCn = "平方米",
                    NameEn="m2"
                },
                new GeneralItem
                {
                    Code = "kg",
                    NameCn = "公斤",
                    NameEn="kg"
                }
            };
        }
        private bool _isComparisonEnabled;
        public bool IsComparisonEnabled
        {
            get => _isComparisonEnabled;
            set { _isComparisonEnabled = value; OnPropertyChanged(); }
        }
        private bool _isComparisonById;
        public bool IsComparisonById
        {
            get => _isComparisonById;
            set { _isComparisonById = value; OnPropertyChanged(); }
        }

        public List<HKLibMatMat> MatMats { get; set; }
        private string _diagramNameCn;
        public string DiagramNameCn
        {
            get => _diagramNameCn;
            set => SetField(ref _diagramNameCn, value);
        }
        private string _diagramNameEn;
        public string DiagramNameEn
        {
            get => _diagramNameEn;
            set => SetField(ref _diagramNameEn, value);
        }
        IComosBaseObject _objDiagram;
        public IComosBaseObject ObjDiagram
        {
            get => _objDiagram;
            set => SetField(ref _objDiagram, value);
        }
        public IComosDProject Project { set; get; }
        public IComosBaseObject _currentObject;
        public IComosBaseObject CurrentObject
        {
            get => _currentObject;
            set => SetField(ref _currentObject, value);
        }
        //public IComosDProject _project;
        //public IComosDProject Project
        //{
        //    get => _project;
        //    set => SetField(ref _project, value);
        //}
        private bool _autoComosUpdate;
        public bool AutoComosUpdate
        {
            get => _autoComosUpdate;
            set
            {
                SetField(ref _autoComosUpdate, value);
                HK_General.IsAutoComosUpdate = value;
                for (int i = 0; i < DataSource.Count; i++)
                {
                    DataSource[i].AutoComosUpdate = value;
                }
            }
        }
        private bool _isLangCtrlShown;
        public bool IsLangCtrlShown
        {
            get => _isLangCtrlShown;
            set => SetField(ref _isLangCtrlShown, value);
        }
        private bool _isButtonShown;
        public bool IsButtonShown
        {
            get => _isButtonShown;
            set => SetField(ref _isButtonShown, value);
        }
        private bool _langInChinese;
        public bool LangInChinese
        {
            get => _langInChinese;
            set => SetField(ref _langInChinese, value);
        }
        private bool _langInEnglish;
        public bool LangInEnglish
        {
            get => _langInEnglish;
            set => SetField(ref _langInEnglish, value);
        }
        ObservableCollection<BomItem> _dataSource;
        public ObservableCollection<BomItem> DataSource
        {
            get => _dataSource;
            set => SetField(ref _dataSource, value);
        }
        private BomItem _selectedItem;
        public BomItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if(SetField(ref _selectedItem, value) && value !=null)
                {
                    SelectedBomIDChanged?.Invoke(this, value.ID);
                }
            }
        }
        public ObservableCollection<BomItem> SelectedItems { get; set; }

        public MatListItem SelectedMatListItem { get; set; }
        public DiagramItem SelectedDiagramItem { get; set; }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public RelayCommand<object> MoveUpCommand { get; }
        public RelayCommand<object> MoveDownCommand { get; }
        public RelayCommand<object> MoveToFirstCommand { get; }
        public RelayCommand<object> MoveToLastCommand { get; }
        private void MoveUp() => DataSource.MoveUp(SelectedItem);
        private void MoveDown() => DataSource.MoveDown(SelectedItem);
        private void MoveToFirst() => DataSource.MoveToFirst(SelectedItem);
        private void MoveToLast() => DataSource.MoveToLast(SelectedItem);

        private bool CanMoveUp() => SelectedItem != null && DataSource.IndexOf(SelectedItem) > 0;
        private bool CanMoveDown() => SelectedItem != null && DataSource.IndexOf(SelectedItem) < DataSource.Count - 1;
        public RelayCommand<object> UpdateCommand { get; }
        public RelayCommand<object> NewAddCommand { get; }
        public RelayCommand<object> AlterCommand { get; }
        public RelayCommand<object> DeleteCommand { get; }

        private void Update()
        {
            if (SelectedItem != null && int.TryParse(SelectedItem.ID, out int id))
            {
                MatListItem objSelected = HK_General.UpdateQueryResult(id);
                SelectedItem.LibBomItem = objSelected;
                //SelectedItem.SetBomListItemFromMatListItem();
                SelectedItem.SetComosObjectFromData();
            }
        }
        private void NewAdd()
        {
            if (SelectedMatListItem == null || CurrentObject == null && SelectedDiagramItem == null || DataSource == null) { return; }
            int index = 0;
            string newNo = null;
            if (SelectedItem != null)
            {
                newNo = SelectedItem.No;
                index = DataSource.IndexOf(SelectedItem);
            }
            else
                index = DataSource.Count-1;
            newNo =string.IsNullOrEmpty(newNo)? DataSource.LastOrDefault()?.No ?? "0": newNo;
            newNo = ((int.TryParse(newNo, out int result) ? result : 998) + 1).ToString();
            if (CurrentObject != null)
            {
                IComosBaseObject cdev = Project.GetCDeviceBySystemFullname("@30|M41|A50|A10Z|A10|A10|A60|A30|Z10", 1);
                IComosBaseObject newMat = Project.Workset().CreateDeviceObject(CurrentObject, cdev);
                BomItem newBomItem = new BomItem() { ObjComosBomItem = newMat, No = newNo, LibBomItem = SelectedMatListItem };
                //newBomItem.SetBomListItemFromMatListItem();
                newBomItem.SetComosObjectFromData();
                //newBomItem.SetDataFromComosObject();
                DataSource.Insert(index, newBomItem);
            }
            else if (SelectedDiagramItem != null)
            {
                BomItem newBomItem = new BomItem() { ObjComosBomItem = null, No = newNo, LibBomItem = SelectedMatListItem };
                //newBomItem.SetBomListItemFromMatListItem();
                HK_General.NewDiagBomAdd(SelectedDiagramItem.ID, newNo, SelectedMatListItem);
                SelectedDiagramItem.BomQty = HK_General.GetDiagBomCount(SelectedDiagramItem.ID).ToString();
                DataSource.Insert(index+1, newBomItem);
            }
        }

        private void Alter()
        {
            if (SelectedMatListItem == null || SelectedItem == null) { return; }
            SelectedItem.LibBomItem = SelectedMatListItem;
            //SelectedItem.SetBomListItemFromMatListItem();
            SelectedItem.SetComosObjectFromData();
        }
        private void Delete()
        {
            foreach (var item in SelectedItems)
            {
                if (CurrentObject != null)
                    item.ObjComosBomItem.DeleteAll();
                else if (SelectedDiagramItem != null)
                {
                    HK_General.DiagBomDelete(item.BomID);
                }
                DataSource.Remove(item);
            }
            if (SelectedDiagramItem != null)
            {
                SelectedDiagramItem.BomQty = HK_General.GetDiagBomCount(SelectedDiagramItem.ID).ToString();
            }
        }
        public ICommand CellEditEndingCommand { get; }
        private void HandleCellEditEnding(DataGridCellEditEndingEventArgs e)
        {

            //if (e.EditAction == DataGridEditAction.Commit && AutoComosUpdate)
            //{
            //    // 获取行数据项
            //    var item = e.Row.Item;
            //    //(iteKm as BomListItem)?.SetComosObjectFromData();

            //    // 获取列信息
            //    var column = e.Column as DataGridBoundColumn;

            //    // 获取编辑后的值
            //    if (e.EditingElement is TextBox textBox)
            //    {
            //        string newValue = textBox.Text;
            //        Debug.WriteLine($"编辑完成: 项目={item}, 列={column?.Header}, 新值={newValue}");
            //    }
            //    else if (e.EditingElement is ComboBox comboBox)
            //    {
            //        object selectedValue = comboBox.SelectedValue;
            //        Debug.WriteLine($"编辑完成: 项目={item}, 列={column?.Header}, 新值={selectedValue}");
            //    }

            //    // 获取绑定路径（属性名）
            //    if (column != null)
            //    {
            //        var binding = column.Binding as Binding;
            //        string propertyName = binding?.Path.Path;
            //        Debug.WriteLine($"属性名: {propertyName}");
            //    }
            //}
        }
        public RelayCommand<SelectionChangedEventArgs> SelectionChangedCommand { get; }
        private void HandleSelectionChanged(SelectionChangedEventArgs e)
        {
            var selectedItems = (e.Source as DataGrid)?.SelectedItems;
            if (selectedItems != null)
            {
                ObservableCollection<BomItem> _selectedItems = new ObservableCollection<BomItem>();
                foreach (var item in selectedItems)
                {
                    _selectedItems.Add(item as BomItem);
                }
                SelectedItems = _selectedItems;
            }
        }
    }
}
