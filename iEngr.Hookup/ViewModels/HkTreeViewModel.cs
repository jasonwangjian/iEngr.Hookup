using iEngr.Hookup.Converters;
using iEngr.Hookup.Models;
using iEngr.Hookup.Services;
using iEngr.Hookup.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace iEngr.Hookup.ViewModels
{
    public class HkTreeViewModel : INotifyPropertyChanged
    {
        private HkTreeItem _editingItem;
        public ObservableCollection<HkTreeItem> TreeItems { get; set; }
        private HkTreeItem _selectedItem;
        public HkTreeItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    if (_selectedItem != null)
                    {
                        _lastSelectedItem = _selectedItem;
                    }
                    OnPropertyChanged();
                    UpdateCommandStates();
                }
            }
        }
        private HkTreeItem _lastSelectedItem; // 保存上次选中的项目
        public HkTreeItem LastSelectedItem
        {
            get => _lastSelectedItem;
            set => _lastSelectedItem = value;
        }
        public bool IsNewAddedItemEditing { get; set; }


        // 更新命令状态
        private void UpdateCommandStates()
        {
            OnPropertyChanged(nameof(IsNodeValided));
            OnPropertyChanged(nameof(CanMoveUp));
            OnPropertyChanged(nameof(CanMoveDown));
            OnPropertyChanged(nameof(CanPaste));
            OnPropertyChanged(nameof(CanDelete));
            CommandManager.InvalidateRequerySuggested();
        }


        // 剪贴板内容
        private HkTreeItem _clipboardContent;
        private bool _isCutOperation; // 标记是剪切还是复制操作
        public string ClipboardContent => _clipboardContent != null ?
            $"{_clipboardContent.Name} ({(_isCutOperation ? "剪切" : "复制")})" : "空";
        private string _pasteStatusMessage;
        public string PasteStatusMessage
        {
            get => _pasteStatusMessage;
            set => SetField(ref _pasteStatusMessage, value);
        }
        public HkTreeViewModel()
        {
            TreeItems = new ObservableCollection<HkTreeItem>();
            MoveUpCommand = new RelayCommand<object>(MoveUp, _ => CanMoveUp);
            MoveDownCommand = new RelayCommand<object>(MoveDown, _ => CanMoveDown);
            ExpandAllCommand = new RelayCommand<object>(ExpandAll);
            CollapseAllCommand = new RelayCommand<object>(CollapseAll);
            CopyCommand = new RelayCommand<object>(Copy, CanCopyOrCut);
            CutCommand = new RelayCommand<object>(Cut, CanCopyOrCut);
            PasteCommand = new RelayCommand<object>(Paste, _ => CanPaste);
            DeleteCommand = new RelayCommand<object>(DeleteRecursiveWithConfirmation, _ => CanDelete);
            EditPropertiesCommand = new RelayCommand<HkTreeItem>(
                execute: EditProperties,
                canExecute: item => item != null && IsNodeValided && SelectedItem.NodeName == "SpecNode"
            );
            SetPictureCommand = new RelayCommand<HkTreeItem>(SetPicture, CanSetPicture);
            StartEditNewCommand = new RelayCommand<object>(StartEditNew, CanEditNew);
            StartEditCommand = new RelayCommand<object>(StartEdit, _ => SelectedItem != null && IsNodeValided && SelectedItem.NodeName != "HookupConfig");
            ConfirmEditCommand = new RelayCommand<object>(ConfirmEdit, CanExecuteConfirmEdit);
            CancelEditCommand = new RelayCommand<object>(CancelEdit);
            PictureDelCommand = new RelayCommand<object>(PictureDel);
            // 从XML文件加载数据
            LoadTreeNode();
            //LoadTreeDataFromXml();
        }
        //节点是否合法
        public bool IsNodeValided
        {
            get
            {
                if (SelectedItem == null || string.IsNullOrEmpty(SelectedItem.NodeName)) return false;
                if (HK_General.dicTreeNode.ContainsKey(SelectedItem.NodeName))
                    return true;
                return false;
            }
        }
        #region 移动
        public ICommand MoveUpCommand { get; set; }
        public ICommand MoveDownCommand { get; set; }
        public bool CanMoveUp => SelectedItem != null &&
                                ((SelectedItem.Parent != null &&
                                  SelectedItem.Parent.Children.IndexOf(SelectedItem) > 0) ||
                                 (SelectedItem.Parent == null &&
                                  TreeItems.IndexOf(SelectedItem) > 0));

        public bool CanMoveDown => SelectedItem != null &&
                                  ((SelectedItem.Parent != null &&
                                    SelectedItem.Parent.Children.IndexOf(SelectedItem) < SelectedItem.Parent.Children.Count - 1) ||
                                   (SelectedItem.Parent == null &&
                                    TreeItems.IndexOf(SelectedItem) < TreeItems.Count - 1));
        private void MoveUp(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                SelectedItem = item;
            }

            if (!CanMoveUp) return;

            var collection = SelectedItem.Parent != null ?
                SelectedItem.Parent.Children : TreeItems;

            int currentIndex = collection.IndexOf(SelectedItem);
            if (currentIndex > 0)
            {
                collection.Move(currentIndex, currentIndex - 1);
                // 更新状态
                OnPropertyChanged(nameof(CanMoveUp));
                OnPropertyChanged(nameof(CanMoveDown));
            }
        }

        private void MoveDown(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                SelectedItem = item;
            }

            if (!CanMoveDown) return;

            var collection = SelectedItem.Parent != null ?
                SelectedItem.Parent.Children : TreeItems;

            int currentIndex = collection.IndexOf(SelectedItem);
            if (currentIndex < collection.Count - 1)
            {
                collection.Move(currentIndex, currentIndex + 1);
                // 更新状态
                OnPropertyChanged(nameof(CanMoveUp));
                OnPropertyChanged(nameof(CanMoveDown));
            }
        }
        #endregion

        #region 折叠展开
        public ICommand ExpandAllCommand { get; set; }
        public ICommand CollapseAllCommand { get; set; }
        // 展开所有节点
        private void ExpandAll(object parameter)
        {
            if (parameter is HkTreeItem selectedItem)
            {
                // 从选中的节点开始展开
                ExpandAllRecursive(selectedItem);
            }
            else
            {
                // 展开所有根节点及其子节点
                foreach (var item in TreeItems)
                {
                    ExpandAllRecursive(item);
                }
            }
        }

        // 折叠所有节点
        private void CollapseAll(object parameter)
        {
            if (parameter is HkTreeItem selectedItem)
            {
                // 从选中的节点开始折叠
                CollapseAllRecursive(selectedItem);
            }
            else
            {
                // 折叠所有根节点及其子节点
                foreach (var item in TreeItems)
                {
                    CollapseAllRecursive(item);
                }
            }
        }

        // 递归展开所有子节点
        private void ExpandAllRecursive(HkTreeItem item)
        {
            if (item == null) return;

            item.IsExpanded = true;
            foreach (var child in item.Children)
            {
                ExpandAllRecursive(child);
            }
        }

        // 递归折叠所有子节点
        private void CollapseAllRecursive(HkTreeItem item)
        {
            if (item == null) return;

            item.IsExpanded = false;
            foreach (var child in item.Children)
            {
                CollapseAllRecursive(child);
            }
        }
        #endregion

        #region 复制剪切粘贴
        public ICommand CopyCommand { get; set; }
        public ICommand CutCommand { get; set; }
        public ICommand PasteCommand { get; set; }
        private bool CanCopyOrCut(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                return IsNodeValided &&
                       SelectedItem != null &&
                       HK_General.dicTreeNode[SelectedItem.NodeName].IsPropNode;
            }
            return false;
        }
        // 检查粘贴操作是否无效，设置状态消息
        public bool CanPaste => _clipboardContent != null &&
                               SelectedItem != null &&
                               SelectedItem.NodeItem.IsPropHolder == true &&
                               !IsPasteOperationInvalid();
        private bool IsPasteOperationInvalid()
        {
            PasteStatusMessage = string.Empty;

            if (_clipboardContent == null || SelectedItem == null)
                return true;

            if (_isCutOperation)
            {
                // 检查是否粘贴到自己
                if (SelectedItem == _clipboardContent)
                {
                    PasteStatusMessage = " (不能粘贴到自己)";
                    return true;
                }

                // 检查是否粘贴到自己的子节点
                if (IsDescendant(SelectedItem, _clipboardContent))
                {
                    PasteStatusMessage = " (不能粘贴到子节点)";
                    return true;
                }

                // 检查是否粘贴到原位置
                if (SelectedItem == _clipboardContent.Parent)
                {
                    PasteStatusMessage = " (已在目标位置)";
                    return true;
                }
            }

            return false;
        }
        // 检查target是否是source的后代
        private bool IsDescendant(HkTreeItem target, HkTreeItem source)
        {
            var current = target;
            while (current != null)
            {
                if (current == source)
                    return true;
                current = current.Parent;
            }
            return false;
        }

        // 复制功能 - 需要创建新实例
        private void Copy(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                SelectedItem = item;
                SelectedItem.OperationStatus = "Copy";
            }

            if (SelectedItem == null) return;

            // 深拷贝节点（创建新实例）
            _clipboardContent = SelectedItem.Clone();
            _isCutOperation = false;

            UpdateClipboardState();
        }

        // 剪切功能 - 直接使用原有实例
        private void Cut(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                SelectedItem = item;
                SelectedItem.OperationStatus = "Cut";
            }

            if (SelectedItem == null) return;

            // 直接使用原有实例，不创建副本
            _clipboardContent = SelectedItem;
            _isCutOperation = true;

            UpdateClipboardState();
        }

        // 粘贴功能
        private void Paste(object parameter)
        {
            if (parameter is HkTreeItem targetItem)
            {
                SelectedItem = targetItem;
            }

            if (_clipboardContent == null || SelectedItem == null) return;

            if (_isCutOperation)
            {
                // 剪切操作：移动原有实例
                PasteCutOperation();
            }
            else
            {
                // 复制操作：创建新实例
                PasteCopyOperation();
            }
        }

        // 处理剪切粘贴
        private void PasteCutOperation()
        {
            var itemToMove = _clipboardContent;

            // 查找同名节点
            SelectedItem.Children
                .Where(x => x.Name == itemToMove.Name)
                .ToList()
                .ForEach(si => si.IsDuplicatedName = true);

            // 如果是从剪贴板剪切过来的同一个节点，直接移动到新位置
            if (itemToMove.Parent != null)
            {
                itemToMove.Parent.Children.Remove(itemToMove);
            }
            else
            {
                TreeItems.Remove(itemToMove);
            }

            // 添加到新位置
            SelectedItem.Children.Add(itemToMove);
            itemToMove.Parent = SelectedItem;

            // 展开目标节点并选中移动的节点
            SelectedItem.IsExpanded = true;
            SelectedItem = itemToMove;

            // 清空剪贴板
            ClearClipboard();
        }

        // 处理复制粘贴
        private void PasteCopyOperation()
        {
            // 创建新实例
            var newItem = _clipboardContent.Clone();
            // 查找同名节点
            SelectedItem.Children
                .Where(x => x.Name == newItem.Name)
                .ToList()
                .ForEach(si => si.IsDuplicatedName = true);
            // 添加到新位置
            SelectedItem.Children.Add(newItem);
            newItem.Parent = SelectedItem;

            // 展开目标节点并选中新节点
            SelectedItem.IsExpanded = true;
            SelectedItem = newItem;

            // 保持剪贴板内容不变，可以继续粘贴
        }

        private void ClearClipboard()
        {
            _clipboardContent.OperationStatus = null;
            _clipboardContent = null;
            _isCutOperation = false;
            PasteStatusMessage = string.Empty;
            UpdateClipboardState();
        }

        // 更新剪贴板状态
        private void UpdateClipboardState()
        {
            OnPropertyChanged(nameof(ClipboardContent));
            OnPropertyChanged(nameof(CanPaste));
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region 解析TreeNode
        List<HkTreeItem> allItems;
        int AllParsedCount = 0;
        private void LoadTreeNode()
        {
            allItems = HK_General.GetAllTreeNodeItems();
            HkTreeItem rootItem = allItems.FirstOrDefault(x => x.ID == "0");
            if (rootItem != null)
            {
                ParseTreeNode(rootItem);
                if (AllParsedCount < allItems.Count-1)
                    PasteStatusMessage = (allItems.Count-1- AllParsedCount).ToString();
                rootItem.IsExpanded = true;
                TreeItems.Add(rootItem);
            }
            else
            {
                // 加载失败

            }
        }
        private void ParseTreeNode(HkTreeItem item, int count = 0)
        {
            item.Parent = allItems.FirstOrDefault(x => x.ID == item.ParentID);
            item.Properties = new Dictionary<string, object>();
            var dicProp = new Dictionary<string, string>();
            item.PropertiesString.Split(',')
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Contains(':'))
                .Select(x => x.Split(':', (char)2))
                .Where(parts => parts.Length == 2)
                .ToList()
                .ForEach(parts =>
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    if (!dicProp.ContainsKey(key))
                        dicProp.Add(key, value);
                });
            foreach (var prop in dicProp)
            {
                var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
                if (propDef != null)
                {
                    if (propDef.Type == PropertyType.EnumItems)
                    {
                        item.Properties.Add(prop.Key, new ObservableCollection<GeneralItem>(propDef.Items.Where(x => prop.Value.Split(',').Contains(x.Code)).ToList()));
                    }
                    else if (propDef.Type == PropertyType.EnumItem)
                    {
                        item.Properties.Add(prop.Key, propDef.Items.FirstOrDefault(x => x.Code == prop.Value));
                    }
                    else
                        item.Properties.Add(prop.Key, prop.Value);
                }
            }

            // 递归处理子节点
            allItems.Where(x => x.ParentID == item.ID).ToList()
                .ForEach(x =>
                {
                    item.Children.Add(x);
                    AllParsedCount++;
                    ParseTreeNode(x, AllParsedCount);
                });
        }
        #endregion

        #region 编辑节点
        public ICommand StartEditNewCommand { get; set; }
        public ICommand StartEditCommand { get; set; }
        public ICommand ConfirmEditCommand { get; set; }
        public ICommand CancelEditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand PictureDelCommand { get; set; }
        public bool CanDelete
        {
            get
            {
                if (SelectedItem == null) return false;

                // 根节点不能删除
                if (TreeItems.Contains(SelectedItem))
                {
                    PasteStatusMessage = " (根节点不能删除)";
                    return false;
                }

                // 必须有父节点才能删除
                if (SelectedItem.Parent == null)
                {
                    PasteStatusMessage = " (该节点不能删除)";
                    return false;
                }

                PasteStatusMessage = string.Empty;
                return true;
            }
        }
        private bool CanEditNew(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                return IsNodeValided &&
                       SelectedItem != null &&
                       (HK_General.dicTreeNode[SelectedItem.NodeName].IsPropNode ||
                       SelectedItem.Children?.Count < HK_General.dicTreeNode.Select(x=>x.Value).Where(x=>x.Parent == SelectedItem.NodeName).Count());
            }
            return false;
        }
        private void StartEditNew(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                item.IsExpanded = true;
                HkTreeItem newItem = new HkTreeItem
                {
                    Parent = item,
                };

                item.Children.Add(newItem);
                if (HK_General.dicTreeNode.TryGetValue(item.NodeName, out HKLibTreeNode node))
                {
                    if (node.NodeType == "ComboBox")
                        newItem.NodeName = newItem.NodeItems.FirstOrDefault()?.Code;
                    else if (node.NodeType == "TextBox")
                    {
                        newItem.NodeName = "SpecNode";
                        newItem.Name = string.Empty;
                    }
                }
                else
                {
                    item.Children.Remove(newItem);
                    return;
                }
                // 取消之前的编辑
                if (_editingItem != null)
                {
                    _editingItem.IsEditing = false;
                }
                IsNewAddedItemEditing = true;
                newItem.IsEditing = true;
                _editingItem = newItem;
                SelectedItem = newItem;
            }
        }
        private void StartEdit(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                // 取消之前的编辑
                if (_editingItem != null && _editingItem != item)
                {
                    if (IsNewAddedItemEditing)
                    {
                        // 从父节点中移除
                        _editingItem.Parent.Children.Remove(_editingItem);
                        IsNewAddedItemEditing = false;
                    }

                    _editingItem.IsEditing = false; ;
                }

                item.IsEditing = true;
                _editingItem = item;
                SelectedItem = item;
            }
        }

        private bool CanExecuteConfirmEdit(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                return !item.HasValidationErrors;
            }
            return _editingItem != null && !_editingItem.HasValidationErrors;
        }
        // 确认编辑
        private void ConfirmEdit(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                if (item.ConfirmEdit())
                {
                    _editingItem = null;
                    IsNewAddedItemEditing = false;
                    HK_General.UpdateNode(item, 0, true);
                }
                else
                {
                    // 编辑失败，显示错误信息
                    PasteStatusMessage = " (验证错误: 名称不能为空且人口不能为负数)";
                }
            }
        }

        // 取消编辑
        private void CancelEdit(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                if (IsNewAddedItemEditing)
                {

                    // 选择父节点
                    if (item.Parent != null)
                    {
                        SelectedItem = item.Parent;
                    }
                    else
                    {
                        SelectedItem = null;
                    }
                    // 从父节点中移除
                    item.Parent.Children.Remove(item);
                    IsNewAddedItemEditing = false;
                }

                else
                    item.IsEditing = false;
                _editingItem = null;
            }
            else if (_editingItem != null)
            {
                _editingItem.IsEditing = false; ;
                _editingItem = null;
            }
        }
        private void PictureDel(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                item.PicturePath = null;
            }
        }
        //编辑节点属性
        public RelayCommand<HkTreeItem> EditPropertiesCommand { get; set; }
        private void EditProperties(HkTreeItem item)
        {
            var dialog = new PropertyEditorDialog(item);
            if (dialog.ShowDialog() == true)
            {
                //item.RefreshDisplayProperties();
                //item.OnPropertyChanged(nameof(HkTreeItem.DisplayProperties));
                item.DisplayProperties = "Trigger"; // 触发OnPropertyChanged
                HK_General.UpdateNode(item);
            }
        }
        //设置节点安装图图片
        public RelayCommand<HkTreeItem> SetPictureCommand { get; set; }
        private bool CanSetPicture(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                return IsNodeValided &&
                       SelectedItem != null &&
                       HK_General.dicTreeNode[SelectedItem.NodeName].IsPropNode;
            }
            return false;
        }
        private void SetPicture(HkTreeItem item)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "图片文件 (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件 (*.*)|*.*",
                Title = "选择图片文件"
            };
            if (dialog.ShowDialog() == true)
            {
                item.PicturePath = dialog.FileName;
                HK_General.UpdateNode(item);
            }
        }
        // 在MainViewModel中添加带确认的删除方法
        private void DeleteRecursiveWithConfirmation(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                SelectedItem = item;
            }

            if (!CanDelete) return;

            // 显示确认对话框（在实际项目中需要实现对话框服务）
            var message = $"确定要删除节点 '{SelectedItem.Name}' 及其所有子节点吗？";
            var caption = "确认删除";

            // 这里使用MessageBox作为示例，实际项目中建议使用DialogService
            var result = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ExecuteDeleteRecursive(SelectedItem);
            }
        }

        // 实际的删除执行方法
        private void ExecuteDeleteRecursive(HkTreeItem itemToDelete)
        {
            var parent = itemToDelete.Parent;
            var siblingToSelect = FindSiblingToSelect(itemToDelete);

            // 从父节点中移除
            parent.Children.Remove(itemToDelete);

            // 选择兄弟节点或父节点
            if (siblingToSelect != null)
            {
                SelectedItem = siblingToSelect;
            }
            else if (parent != null)
            {
                SelectedItem = parent;
            }
            else
            {
                SelectedItem = null;
            }

            // 清空相关剪贴板内容
            if (_clipboardContent == itemToDelete)
            {
                ClearClipboard();
            }
        }
        // 查找要选中的兄弟节点
        private HkTreeItem FindSiblingToSelect(HkTreeItem deletedItem)
        {
            if (deletedItem.Parent == null) return null;

            var siblings = deletedItem.Parent.Children;
            int deletedIndex = siblings.IndexOf(deletedItem);

            if (siblings.Count > 0)
            {
                // 尝试选择后面的兄弟节点
                if (deletedIndex < siblings.Count - 1)
                {
                    return siblings[deletedIndex + 1];
                }
                // 尝试选择前面的兄弟节点
                else if (deletedIndex > 0)
                {
                    return siblings[deletedIndex - 1];
                }
            }

            return null;
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