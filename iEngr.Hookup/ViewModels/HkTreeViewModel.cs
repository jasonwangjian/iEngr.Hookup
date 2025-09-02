using iEngr.Hookup.Converters;
using iEngr.Hookup.Models;
using iEngr.Hookup.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace iEngr.Hookup.ViewModels
{
    public class HkTreeViewModel : INotifyPropertyChanged
    {
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
            OnPropertyChanged(nameof(CanMoveToParent));
            OnPropertyChanged(nameof(CanMoveToChild));
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
            MoveToParentCommand = new RelayCommand<object>(MoveToParent, _ => CanMoveToParent);
            MoveToChildCommand = new RelayCommand<object>(MoveToChild, _ => CanMoveToChild);
            MoveUpCommand = new RelayCommand<object>(MoveUp, _ => CanMoveUp);
            MoveDownCommand = new RelayCommand<object>(MoveDown, _ => CanMoveDown);
            ExpandAllCommand = new RelayCommand<object>(ExpandAll);
            CollapseAllCommand = new RelayCommand<object>(CollapseAll);
            CopyCommand = new RelayCommand<object>(Copy, _ => SelectedItem != null);
            CutCommand = new RelayCommand<object>(Cut, _ => SelectedItem != null);
            PasteCommand = new RelayCommand<object>(Paste, _ => CanPaste);
            DeleteCommand = new RelayCommand<object>(DeleteWithConfirmation, _ => CanDelete);
            EditPropertiesCommand = new RelayCommand<HkTreeItem>(
                execute: EditProperties,
                canExecute: item => item != null & IsNodeValided
            );
            StartEditNewCommand = new RelayCommand<object>(StartEditNew,_=> IsNodeValided);
            StartEditCommand = new RelayCommand<object>(StartEdit, _ => SelectedItem != null && IsNodeValided);
            ConfirmEditCommand = new RelayCommand<object>(ConfirmEdit, CanExecuteConfirmEdit);
            CancelEditCommand = new RelayCommand<object>(CancelEdit);
            // 从XML文件加载数据
            LoadTreeDataFromXml();
        }
        //节点是否合法
        public bool IsNodeValided
        {
            get
            {
                if (SelectedItem == null) return false;
                if (HK_General.dicTreeNode.ContainsKey(SelectedItem.NodeName))
                    return true;
                return false;
            }
        }
        #region 移动
        public ICommand MoveToParentCommand { get; set; }
        public ICommand MoveToChildCommand { get; set; }
        public ICommand MoveUpCommand { get; set; }
        public ICommand MoveDownCommand { get; set; }
        public bool CanMoveToParent => SelectedItem != null &&
                                      SelectedItem.Parent != null &&
                                      SelectedItem.Parent.Parent != null;

        public bool CanMoveToChild => SelectedItem != null &&
                                     SelectedItem.Parent != null &&
                                     SelectedItem.Parent.Children.Count > 1;

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
        private void MoveToParent(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                SelectedItem = item;
            }

            if (!CanMoveToParent) return;

            var currentItem = SelectedItem;
            var oldParent = currentItem.Parent;
            var newParent = oldParent.Parent;

            // 保存展开状态
            bool wasExpanded = currentItem.IsExpanded;

            // 从原父节点移除
            oldParent.Children.Remove(currentItem);

            // 更新父级引用
            currentItem.Parent = newParent;

            // 添加到新父节点
            newParent.Children.Add(currentItem);

            // 恢复展开状态
            currentItem.IsExpanded = wasExpanded;

            // 确保新父节点展开
            newParent.IsExpanded = true;

            // 保持选中状态
            SelectedItem = currentItem;

            // 更新状态
            OnPropertyChanged(nameof(CanMoveToParent));
            OnPropertyChanged(nameof(CanMoveToChild));
            OnPropertyChanged(nameof(CanMoveUp));
            OnPropertyChanged(nameof(CanMoveDown));
        }

        private void MoveToChild(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                SelectedItem = item;
            }

            if (!CanMoveToChild) return;

            // 选择兄弟节点作为新父节点
            var sibling = SelectedItem.Parent.Children
                .FirstOrDefault(x => x != SelectedItem);

            if (sibling == null) return;

            var currentItem = SelectedItem;
            var oldParent = currentItem.Parent;

            // 保存展开状态
            bool wasExpanded = currentItem.IsExpanded;

            // 从原父节点移除
            oldParent.Children.Remove(currentItem);

            // 更新父级引用
            currentItem.Parent = sibling;

            // 添加到兄弟节点的子节点
            sibling.Children.Add(currentItem);

            // 恢复展开状态
            currentItem.IsExpanded = wasExpanded;

            // 确保新父节点展开
            sibling.IsExpanded = true;

            // 保持选中状态
            SelectedItem = currentItem;

            // 更新状态
            OnPropertyChanged(nameof(CanMoveToParent));
            OnPropertyChanged(nameof(CanMoveToChild));
            OnPropertyChanged(nameof(CanMoveUp));
            OnPropertyChanged(nameof(CanMoveDown));
        }

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
        public bool CanPaste => _clipboardContent != null &&
                               SelectedItem != null &&
                               !IsPasteOperationInvalid();
        // 检查粘贴操作是否无效，设置状态消息
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

        // 检查target是否是source的后代或是source本身
        private bool IsDescendantOrSelf(HkTreeItem target, HkTreeItem source)
        {
            if (target == null || source == null)
                return false;

            // 如果是同一个节点
            if (target == source)
                return true;

            // 检查是否是后代节点
            return IsDescendant(target, source);
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

        #region 文件操作;  Xml - TreeView转换
        // 从XML文件加载树形数据
        private void LoadTreeDataFromXml()
        {
            try
            {
                string xmlFilePath = GetXmlFilePath();

                if (File.Exists(xmlFilePath))
                {
                    XDocument doc = XDocument.Load(xmlFilePath);
                    var treeItem = ParseXmlNode(doc.Root, null);
                    treeItem.IsExpanded = true;
                    TreeItems.Add(treeItem);
                    //var rootNodes = doc.Root.Elements();
                    //foreach (var node in rootNodes)
                    //{
                    //    treeItem = ParseXmlNode(node, null);
                    //    TreeItems.Add(treeItem);
                    //}
                }
                else
                {
                    // 如果XML文件不存在，使用默认测试数据
                    InitializeDefaultData();
                    // 可选：保存默认数据到XML文件
                    SaveTreeDataToXml();
                }
            }
            catch (Exception ex)
            {
                // 加载失败时使用默认数据
                InitializeDefaultData();
                Console.WriteLine($"加载XML数据失败: {ex.Message}");
            }
        }
        // 解析XML节点（递归方法）
        private HkTreeItem ParseXmlNode(XElement xmlNode, HkTreeItem parent)
        {
            var treeItem = new HkTreeItem
            {
                NodeName = xmlNode.Name.LocalName,
                NodeValue = GetElementValue(xmlNode),
                Parent = parent
            };

            // 解析扩展属性
            treeItem.ID = xmlNode.Attribute("ID")?.Value;
            treeItem.FunctionCode = xmlNode.Attribute("FunctionCode")?.Value;
            treeItem.Name = xmlNode.Attribute("Name")?.Value;
            // 解析字典属性
            string[] reservedAttributes = { "ID", "Name"};
            treeItem.Properties = xmlNode.Attributes().Where(x => !reservedAttributes.Contains(x.Name.ToString())).ToDictionary(x => x.Name.ToString(), x => (object)x.Value);
            //treeItem.Properties = new Dictionary<string, object>();
            //foreach (var attribute in xmlNode.Attributes())
            //{
            //    if (!reservedAttributes.Contains(attribute.Name.ToString()))
            //        treeItem.Properties.Add(attribute.Name.ToString(), attribute.Value);
            //}
            //treeItem.RefreshDisplayProperties();

            // 递归处理子节点
            foreach (var childNode in xmlNode.Elements())
            {
                var childItem = ParseXmlNode(childNode, treeItem);
                treeItem.Children.Add(childItem);
            }

            return treeItem;
        }
        // 获取XML文件路径
        private string GetXmlFilePath()
        {
            // 可以根据需要调整文件路径
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HookupConfig.xml");
        }
        private string GetElementValue(XElement element)
        {
            var textNodes = element.Nodes().OfType<XText>();
            return string.Concat(textNodes.Select(t => t.Value)).Trim();
        }

        // 保存树形数据到XML文件（可选功能）
        public void SaveTreeDataToXml()
        {
            try
            {
                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    ConvertToXmlNode(TreeItems[0])
                //new XElement("HookupConfig",
                //    from item in TreeItems
                //    select ConvertToXmlNode(item)
                //)
                );

                doc.Save(GetXmlFilePath());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存XML数据失败: {ex.Message}");
            }
        }

        // 将TreeItem转换为XML节点（递归方法）
        private XElement ConvertToXmlNode(HkTreeItem treeItem)
        {
            string elementName = CleanXmlName(treeItem.NodeName);
            var element = new XElement(elementName);

            // 设置节点值
            if (!string.IsNullOrEmpty(treeItem.NodeValue))
            {
                element.Value = treeItem.NodeValue;
            }

            // 添加附加属性
            // 添加附加属性
            if (!string.IsNullOrEmpty(treeItem.ID))
            {
                element.SetAttributeValue("ID", treeItem.ID);
            }
            if (!string.IsNullOrEmpty(treeItem.FunctionCode))
            {
                element.SetAttributeValue("FunctionCode", treeItem.FunctionCode);
            }
            if (!string.IsNullOrEmpty(treeItem.Name))
            {
                element.SetAttributeValue("Name", treeItem.Name);
            }
            foreach (var prop in treeItem.Properties)
            {
                element.SetAttributeValue(prop.Key, prop.Value);
            }


            // 递归处理子节点
            foreach (HkTreeItem childItem in treeItem.Children)
            {
                element.Add(ConvertToXmlNode(childItem));
            }

            return element;
        }
        private string CleanXmlName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "未命名";

            string cleaned = new string(name.Where(c =>
                char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.').ToArray());

            return string.IsNullOrEmpty(cleaned) ? "未命名" : cleaned;
        }



        // 默认测试数据（备用）
        private void InitializeDefaultData()
        {
            TreeItems.Clear();

            // 创建简单的默认数据结构
            var parent1 = new HkTreeItem { Name = "默认节点1" };
            var parent2 = new HkTreeItem { Name = "默认节点2" };

            for (int j = 1; j <= 2; j++)
            {
                var child = new HkTreeItem
                {
                    Name = $"子节点 {j}",
                    Parent = parent1
                };
                parent1.Children.Add(child);
            }

            for (int j = 1; j <= 2; j++)
            {
                var child = new HkTreeItem
                {
                    Name = $"子节点 {j}",
                    Parent = parent2
                };
                parent2.Children.Add(child);
            }

            TreeItems.Add(parent1);
            TreeItems.Add(parent2);
        }
        #endregion

        #region 编辑节点
        public ICommand StartEditNewCommand { get; set; }
        public ICommand StartEditCommand { get; set; }
        public ICommand ConfirmEditCommand { get; set; }
        public ICommand CancelEditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        private HkTreeItem _editingItem;
        private void StartEditNew(object parameter)
        {
            if (parameter is HkTreeItem item)
            {
                item.IsExpanded = true;
                HkTreeItem newIten = new HkTreeItem
                {
                    Parent = item,
                };
                item.Children.Add(newIten);
                // 取消之前的编辑
                if (_editingItem != null)
                {
                    _editingItem.IsEditing = false;
                }
                IsNewAddedItemEditing = true;
                newIten.IsEditing = true;
                _editingItem = newIten;
                SelectedItem = newIten;
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
                    item.RefreshDisplayProperties();
                    SaveTreeDataToXml();

                    //item.OnPropertyChanged(nameof(item.DisplayProperties));
                    // 编辑成功
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
                    item.IsEditing=false;
                _editingItem = null;
            }
            else if (_editingItem != null)
            {
                _editingItem.IsEditing = false; ;
                _editingItem = null;
            }
        }
        //编辑节点属性
        public RelayCommand<HkTreeItem> EditPropertiesCommand { get; set; }
        private void EditProperties(HkTreeItem item)
        {
            var dialog = new PropertyEditorDialog(item);
            if (dialog.ShowDialog() == true)
            {
                item.RefreshDisplayProperties();
                //item.OnPropertyChanged(nameof(HkTreeItem.DisplayProperties));
                SaveTreeDataToXml();
                //OnPropertyChanged(nameof(TreeItems));
            }
        }
        // 在MainViewModel中添加带确认的删除方法
        private void DeleteWithConfirmation(object parameter)
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
                ExecuteDelete(SelectedItem);
            }
        }

        // 实际的删除执行方法
        private void ExecuteDelete(HkTreeItem itemToDelete)
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