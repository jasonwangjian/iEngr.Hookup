using iEngr.Hookup.Models;
using Microsoft.Win32;
using Plt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace iEngr.Hookup.ViewModels
{
    public class DiagItemsViewModel : INotifyPropertyChanged
    {
        public event EventHandler<DiagramItem> PicturePathChanged;
        public event EventHandler<string> DiagramIDChanged;
        public event EventHandler<DiagramItem> ComosPicturePathSet;
        public event EventHandler<IComosBaseObject> ComosDiagChanged;

        public event EventHandler<DiagramItem> ComosDiagModAppCmd;
        public event EventHandler<DiagramItem> ComosDiagObjDelCmd;
        public ICommand CellEditEndingCommand { get; }
        public ICommand PictureSetCommand { get; }
        public ICommand DiagramAddCommand { get; }
        public ICommand DiagramAppCommand { get; }
        public ICommand ComosDiagDelCommand { get; }
        public ICommand DiagramRemoveCommand { get; }
        public ICommand DiagramDeleteCommand { get; }

        public DiagItemsViewModel()
        {
            CellEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(HandleCellEditEnding);
            PictureSetCommand = new RelayCommand<DiagramItem>(SetPicture, CanSetPicture);
            DiagramAddCommand = new RelayCommand<DiagramItem>(AddDiagram, CanAddDiagram);
            DiagramAppCommand = new RelayCommand<DiagramItem>(AppDiagram, CanAppDiagram);
            ComosDiagDelCommand = new RelayCommand<DiagramItem>(ComosDiagObjDel, _=>true);
            DiagramRemoveCommand = new RelayCommand<DiagramItem>(RemoveDiagram, CanRemoveDiagram);
            DiagramDeleteCommand = new RelayCommand<DiagramItem>(DeleteDiagram, CanDeleteDiagram);
            SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(HandleSelectionChanged);
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
                return item != null;
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
                PicturePathChanged?.Invoke(this, item);
                if (item.IsLibItem)
                    HK_General.UpdateDiagram(item.ID, "PicturePath", item.PicturePath);
                else if (item.IsComosItem)
                    ComosPicturePathSet?.Invoke(this, item);
            }
        }
        private bool CanAddDiagram(object parameter)
        {
            if (parameter is DiagramItem item)
            {
                return item != null &&
                       item.IsLibItem &&
                       FocusedNode != null &&
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
            //NodeDiagramItems = HK_General.GetDiagramItems(diagIDs, true, false);
            HK_General.UpdateLibData("HK_TreeNOde",int.Parse(FocusedNode.ID), "DiagID", diagIDs);
            //if (string.IsNullOrEmpty(diagIDs)) return;
            //List<string> ids = diagIDs.Split(',').ToList();
            AssignedDiagramItems.Clear();
            List<int> ids = diagIDs?.Split(',')
                                        .Select(s => s.Trim())  // 去除空格
                                        .Where(s => int.TryParse(s, out _))
                                        .Select(int.Parse)
                                        .ToList();
            foreach (var itemLib in AvailableDiagramItems)
            {
                //itemLib.IsOwned = ids.Contains(itemLib.ID.ToString());
                if (ids != null && ids.Contains(itemLib.ID))
                {
                    itemLib.IsOwned = true;
                    itemLib.IsInherit = false;
                    AssignedDiagramItems.Add(itemLib);
                }
                else
                {
                    itemLib.IsOwned = false;
                }
            }
            DiagramIDChanged?.Invoke(this, item.ID.ToString());
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
            //List<string> ids = FocusedNode.DiagID.Split(',').ToList();
            //if (ids.Remove(item.ID.ToString()))
            //{
            //    FocusedNode.DiagID = string.Join(",", ids);
            //    NodeDiagramItems = HK_General.GetDiagramItems(FocusedNode.DiagID, true, false);
            //    HK_General.UpdateLibData("HK_TreeNOde", int.Parse(FocusedNode.ID), "DiagID", FocusedNode.DiagID);
            //    if (string.IsNullOrEmpty(FocusedNode.DiagID)) return;
            //    foreach (var itemLib in LibDiagramItems)
            //    {
            //        itemLib.IsOwned = ids.Contains(itemLib.ID.ToString());
            //    }
            //}
            List<int> ids = FocusedNode.DiagID.Split(',')
                                        .Select(s => s.Trim())  // 去除空格
                                        .Where(s => int.TryParse(s, out _))
                                        .Select(int.Parse)
                                        .ToList(); 
            if (ids.Remove(item.ID))
            {
                FocusedNode.DiagID = string.Join(",", ids);
                HK_General.UpdateLibData("HK_TreeNOde", int.Parse(FocusedNode.ID), "DiagID", FocusedNode.DiagID);
                AssignedDiagramItems.Clear();
                foreach (var itemLib in AvailableDiagramItems)
                {
                    if (ids != null && ids.Contains(itemLib.ID))
                    {
                        itemLib.IsOwned = true;
                        itemLib.IsInherit = false;
                        AssignedDiagramItems.Add(itemLib);
                    }
                    else
                    {
                        itemLib.IsOwned = false;
                    }
                }
            }
        }
        public ObservableCollection<DiagramItem> SelectedItems { get; set; }
        public RelayCommand<SelectionChangedEventArgs> SelectionChangedCommand { get; }
        private void HandleSelectionChanged(SelectionChangedEventArgs e)
        {
            var selectedItems = (e.Source as DataGrid)?.SelectedItems;
            if (selectedItems != null)
            {
                ObservableCollection<DiagramItem> _selectedItems = new ObservableCollection<DiagramItem>();
                foreach (var item in selectedItems)
                {
                    _selectedItems.Add(item as DiagramItem);
                }
                SelectedItems = _selectedItems;
            }
        }

        private bool CanDeleteDiagram(object parameter)
        {
            if (parameter is DiagramItem item)
            {
                return item != null &&
                       !HK_General.IsIDAssigned(item.ID);
            }
            return false;
        }
        private void DeleteDiagram(DiagramItem item)
        {
            foreach(var itemS in SelectedItems)
            {
                if (!HK_General.IsIDAssigned(itemS.ID))
                {
                    AvailableDiagramItems.Remove(itemS);
                    HK_General.DeleteByID("HK_Diagram", itemS.ID);
                }
            }
        }
        #region Properties

        //public bool IsComosItem { get; set; }
        //public bool IsLibItem { get; set; }
        public bool _isAssignedDiagramItemsShown;
        public bool IsAssignedDiagramItemsShown
        {
            get => _isAssignedDiagramItemsShown;
            set => SetField(ref _isAssignedDiagramItemsShown, value);
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
        private ObservableCollection<DiagramItem> _assignedDiagramItems = new ObservableCollection<DiagramItem>();
        public ObservableCollection<DiagramItem> AssignedDiagramItems
        {
            get => _assignedDiagramItems;
            set => SetField(ref _assignedDiagramItems, value);
        }
        private DiagramItem _assignedDiagramsSelectedItem;
        public DiagramItem AssignedDiagramsSelectedItem
        {
            get => _assignedDiagramsSelectedItem;
            set
            {
                SetField(ref _assignedDiagramsSelectedItem, value);
                SelectedItem = value;
                PicturePathChanged?.Invoke(this, value);
                if (SelectedItem?.IsLibItem == true)
                {
                    DiagramIDChanged?.Invoke(this, value?.ID.ToString());
                }
                if (SelectedItem?.IsComosItem == true)
                {
                    ComosDiagChanged?.Invoke(this, SelectedItem.ObjComosDiagMod) ;
                }
            }
        }
        private ObservableCollection<DiagramItem> _availableDiagramItems = new ObservableCollection<DiagramItem>();
        public ObservableCollection<DiagramItem> AvailableDiagramItems
        {
            get => _availableDiagramItems;
            set => SetField(ref _availableDiagramItems, value);
        }
        private DiagramItem _availableDiagramsSelectedItem;
        public DiagramItem AvailableDiagramsSelectedItem
        {
            get => _availableDiagramsSelectedItem;
            set
            {
                SetField(ref _availableDiagramsSelectedItem, value);
                SelectedItem = value;
                PicturePathChanged?.Invoke(this, value);
                if (SelectedItem?.IsLibItem == true)
                {
                    DiagramIDChanged?.Invoke(this, value?.ID.ToString());
                }
                if (SelectedItem?.IsComosItem == true)
                {
                    ComosDiagChanged?.Invoke(this, SelectedItem.ObjComosDiagMod);
                }
            }
        }
        public DiagramItem SelectedItem { set; get; }
        #endregion

        #region CommandEvent
        private bool CanAppDiagram(object parameter)
        {
            if (parameter is DiagramItem item)
            {
                return item != null &&
                       item.IsLibItem;
            }
            return false;
        }
        private void AppDiagram(DiagramItem item)
        {
            ComosDiagModAppCmd?.Invoke(this, item);
        }
        private void ComosDiagObjDel(DiagramItem item)
        {
            ComosDiagObjDelCmd?.Invoke(this, item);
        }
        #endregion 

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