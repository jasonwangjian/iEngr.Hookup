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
    public class BomListViewModels: INotifyPropertyChanged
    {
        public BomListViewModels()
        {
            DataSource = new ObservableCollection<HKBOM>();
            SetDisciplineSource();
            SetResponsibleSource();
            SetUnitSource();
            CellEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(HandleCellEditEnding);
            SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(HandleSelectionChanged);
            AutoComosUpdate = true;
        }
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
        public List<GeneralItem> Disciplines { get; set; }
        public List<GeneralItem> Responsibles { get; set; }
        public List<GeneralItem> Units { get; set; }
        private void SetUnitSource()
        {
            Responsibles = new List<GeneralItem>
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
        public List<GeneralItem> MatMats = new List<GeneralItem>();
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
        private bool _autoComosUpdate;
        public bool AutoComosUpdate
        {
            get => _autoComosUpdate;
            set
            {
                SetField(ref _autoComosUpdate, value);
            }
        }
        ObservableCollection<HKBOM> _dataSource;
        public ObservableCollection<HKBOM> DataSource
        {
            get => _dataSource;
            set => SetField(ref _dataSource, value);
        }
        private HKBOM _selectedItem;
        public HKBOM SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetField(ref _selectedItem, value);
                value.ObjMat.Label = value.No;
                
            }
        }
        public ObservableCollection<HKBOM> SelectedItems { get; set; }



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


        public ICommand CellEditEndingCommand { get; }
        private void HandleCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // 获取行数据项
                var item = e.Row.Item;

                // 获取列信息
                var column = e.Column as DataGridBoundColumn;

                // 获取编辑后的值
                if (e.EditingElement is TextBox textBox)
                {
                    string newValue = textBox.Text;
                    Debug.WriteLine($"编辑完成: 项目={item}, 列={column?.Header}, 新值={newValue}");
                }
                else if (e.EditingElement is ComboBox comboBox)
                {
                    object selectedValue = comboBox.SelectedValue;
                    Debug.WriteLine($"编辑完成: 项目={item}, 列={column?.Header}, 新值={selectedValue}");
                }

                // 获取绑定路径（属性名）
                if (column != null)
                {
                    var binding = column.Binding as Binding;
                    string propertyName = binding?.Path.Path;
                    Debug.WriteLine($"属性名: {propertyName}");
                }
            }
        }
        public RelayCommand<SelectionChangedEventArgs> SelectionChangedCommand { get; }
        private void HandleSelectionChanged(SelectionChangedEventArgs e)
        {
            var selectedItems = (e.Source as DataGrid)?.SelectedItems;
            if (selectedItems != null)
            {
                ObservableCollection<HKBOM> _selectedItems = new ObservableCollection<HKBOM>();
                foreach (var item in selectedItems)
                {
                    _selectedItems.Add(item as HKBOM);
                }
                SelectedItems = _selectedItems;
            }
        }
    }
}
