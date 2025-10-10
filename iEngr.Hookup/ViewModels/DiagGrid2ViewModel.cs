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
    public class DiagGrid2ViewModel : INotifyPropertyChanged
    {
        public event EventHandler<string> PicturePathChanged;
        public event EventHandler<string> DiagramIDChanged;
        public ICommand CellEditEndingCommand { get; }
        public ICommand PictureSetCommand { get; }
        public ICommand DiagramAddCommand { get; }
        public ICommand DiagramRemoveCommand { get; }

        public DiagGrid2ViewModel()
        {
            CellEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(HandleCellEditEnding);
            PictureSetCommand = new RelayCommand<DiagramItem>(SetPicture, CanSetPicture);
            DiagramAddCommand = new RelayCommand<DiagramItem>(AddDiagram, CanAddDiagram);
            DiagramRemoveCommand = new RelayCommand<DiagramItem>(RemoveDiagram, CanRemoveDiagram);
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
                       FocusedNode?.NodeItem?.IsPropNode == true;
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
        private bool CanAddDiagram(object parameter)
        {
            if (parameter is DiagramItem item)
            {
                return item != null &&
                       FocusedNode.NodeItem?.IsPropNode == true &&
                       item.IsOwned == false;
            }
            return false;
        }
        private void AddDiagram(DiagramItem item)
        {
            //FocusedNode.DiagID = string.IsNullOrEmpty(FocusedNode.DiagID)? item.ID.ToString(): FocusedNode.DiagID +"," + item.ID.ToString();
            FocusedNode.DiagID = string.Join(",", (FocusedNode.DiagID + "," + item.ID.ToString()).Split(',').Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList());
            
            string diagIDs = FocusedNode.DiagID;
            NodeDiagramItems = HK_General.GetDiagramItems(diagIDs, true, false);
            HK_General.UpdateLibData("HK_TreeNOde",int.Parse(FocusedNode.ID), "DiagID", diagIDs);
            if (string.IsNullOrEmpty(diagIDs)) return;
            List<string> ids = diagIDs.Split(',').ToList();
            foreach (var itemLib in LibDiagramItems)
            {
                itemLib.IsOwned = ids.Contains(itemLib.ID.ToString());
            }
        }
        private bool CanRemoveDiagram(object parameter)
        {
            if (parameter is DiagramItem item)
            {
                return item != null &&
                       item.IsOwned == true &&
                       item.IsInherit == false;
            }
            return false;
        }
        private void RemoveDiagram(DiagramItem item)
        {
            List<string> ids = FocusedNode.DiagID.Split(',').ToList();
            if (ids.Remove(item.ID.ToString()))
            {
                FocusedNode.DiagID = string.Join(",", ids);
                NodeDiagramItems = HK_General.GetDiagramItems(FocusedNode.DiagID, true, false);
                HK_General.UpdateLibData("HK_TreeNOde", int.Parse(FocusedNode.ID), "DiagID", FocusedNode.DiagID);
                if (string.IsNullOrEmpty(FocusedNode.DiagID)) return;
                foreach (var itemLib in LibDiagramItems)
                {
                    itemLib.IsOwned = ids.Contains(itemLib.ID.ToString());
                }
            }
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
        private HkTreeItem _focusedNode;
        public HkTreeItem FocusedNode
        {
            get => _focusedNode;
            set
            {
                if (SetField(ref _focusedNode, value))
                {
                    //LibSelectedItem.FocusedNode = value;
                    //NodeSelectedItem.FocusedNode = value;
                }
            }
        }
        private ObservableCollection<DiagramItem> _nodeDiagramItems = new ObservableCollection<DiagramItem>();
        public ObservableCollection<DiagramItem> NodeDiagramItems
        {
            get => _nodeDiagramItems;
            set => SetField(ref _nodeDiagramItems, value);
        }
        private DiagramItem _nodeSelectedItem;
        public DiagramItem NodeSelectedItem
        {
            get => _nodeSelectedItem;
            set
            {
                if(SetField(ref _nodeSelectedItem, value))// && value != null)
                {
                    PicturePathChanged?.Invoke(this, value?.PicturePath);
                    DiagramIDChanged?.Invoke(this, value?.ID.ToString());
                    SelectedItem = value;
                }
            }
        }
        private ObservableCollection<DiagramItem> _libDiagramItems = new ObservableCollection<DiagramItem>();
        public ObservableCollection<DiagramItem> LibDiagramItems
        {
            get => _libDiagramItems;
            set => SetField(ref _libDiagramItems, value);
        }
        private DiagramItem _libSelectedItem;
        public DiagramItem LibSelectedItem
        {
            get => _libSelectedItem;
            set
            {
                if (SetField(ref _libSelectedItem, value))// && value != null)
                {
                    PicturePathChanged?.Invoke(this, value?.PicturePath);
                    DiagramIDChanged?.Invoke(this, value?.ID.ToString());
                    SelectedItem = value;
                }
            }
        }
        public DiagramItem SelectedItem { set; get; }

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