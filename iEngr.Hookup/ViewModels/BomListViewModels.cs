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
            CellEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(HandleCellEditEnding);
            SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(HandleSelectionChanged);
            DeleteCommand = new RelayCommand<object>(_ => Delete(), _ => SelectedItems?.Count > 0);
        }
        private void SetDisciplineSource()
        {
            Disciplines = new List<GeneralItem>
            {
                new GeneralItem
                {
                    Code = "I",
                    Name = "仪表"
                },
                new GeneralItem
                {
                    Code = "F",
                    Name = "工艺"
                },
                new GeneralItem
                {
                    Code = "E",
                    Name = "电气"
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
                    Name = "业主方"
                },
                new GeneralItem
                {
                    Code = "S",
                    Name = "供货商"
                },
                new GeneralItem
                {
                    Code = "M",
                    Name = "制造商"
                },
                new GeneralItem
                {
                    Code = "E",
                    Name = "安装方"
                }
            };
        }
        public List<GeneralItem> Disciplines { get; set; }
        public List<GeneralItem> Responsibles { get; set; }
        IComosBaseObject _objMat;
        private string _matID;
        public string MatID
        {
            get => _matID;
            set => SetField(ref _matID, value);
        }
        public IComosBaseObject ObjMat
        {
            get => _objMat;
            set => SetField(ref _objMat, value);
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
        public RelayCommand<object> DeleteCommand { get; }
        private void Delete()
        {
            (SelectedItem.ObjMat as IComosBaseObject)?.DeleteAll();
            //BtnCommand = "Delete";
        }
    }
}
