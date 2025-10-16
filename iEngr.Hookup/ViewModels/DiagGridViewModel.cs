using iEngr.Hookup.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace iEngr.Hookup.ViewModels
{
    public class DiagGridViewModel : INotifyPropertyChanged
    {
        public event EventHandler<string> PicturePathChanged;
        public event EventHandler<string> DiagramIDChanged;
        public ICommand CellEditEndingCommand { get; }
        public ICommand PictureSetCommand { get; }

        public DiagGridViewModel()
        {
            CellEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(HandleCellEditEnding);
            PictureSetCommand = new RelayCommand<DiagramItem>(SetPicture, CanSetPicture);
            IsLangCtrlShown = true;
            LangInChinese = true;
        }
        private void HandleCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // 获取行数据项
                DiagramItem item = e.Row.Item as DiagramItem;
                //(iteKm as BomListItem)?.SetComosObjectFromData();

                // 获取列信息
                var column = e.Column as DataGridBoundColumn;

                // 获取编辑后的值
                object value = null;
                if (e.EditingElement is TextBox textBox)
                {
                    string newValue = textBox.Text;
                    value = textBox.Text;
                    Debug.WriteLine($"编辑完成: 项目={item}, 列={column?.Header}, 新值={newValue}");
                }
                else if (e.EditingElement is ComboBox comboBox)
                {
                    object selectedValue = comboBox.SelectedValue;
                    value = comboBox.SelectedValue;
                    Debug.WriteLine($"编辑完成: 项目={item}, 列={column?.Header}, 新值={selectedValue}");
                }

                // 获取绑定路径（属性名）
                if (column != null)
                {
                    var binding = column.Binding as Binding;
                    string propertyName = binding?.Path.Path;
                    HK_General.UpdateDiagram(item.ID, propertyName, value);
                    Debug.WriteLine($"属性名: {propertyName}");

                }
            }
        }
        private bool CanSetPicture(object parameter)
        {
            if (parameter is DiagramItem item)
            {
                return item != null &&
                       item.IsOwned;
            }
            return false;
        }
        private void SetPicture(DiagramItem item)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "所有支持的文件|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif;*.pdf|图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif|PDF文件|*.pdf|所有文件|*.*",
                Title = "选择图片文件"
            };
            if (dialog.ShowDialog() == true)
            {
                item.PicturePath = dialog.FileName;
                PicturePathChanged?.Invoke(this, item.PicturePath);
                HK_General.UpdateDiagram(item.ID, "PicturePath", item.PicturePath);
            }
        }
        private bool _isLangCtrlShown;
        public bool IsLangCtrlShown
        {
            get => _isLangCtrlShown;
            set => SetField(ref _isLangCtrlShown, value);
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
        private ObservableCollection<DiagramItem> _diagramItems = new ObservableCollection<DiagramItem>();
        public ObservableCollection<DiagramItem> DiagramItems
        {
            get => _diagramItems;
            set => SetField(ref _diagramItems, value);
        }
        private DiagramItem _selectedItem;
        public DiagramItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetField(ref _selectedItem, value);
                if (value != null)
                    PicturePathChanged?.Invoke(this, value?.PicturePath);
                DiagramIDChanged?.Invoke(this, value?.ID.ToString());
            }
        }

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
    }
}