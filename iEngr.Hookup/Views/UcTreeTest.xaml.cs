using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UcHkTree.xaml 的交互逻辑
    /// </summary>
    public partial class UcTreeTest : UserControl
    {
        public UcTreeTest()
        {
            InitializeComponent();
            currentFileText.Text = "未选择文件";
            itemCountText.Text = "0 个节点";
            keyCheckText.Text = "";
        }
        private string currentFilePath;
        private TreeViewItem selectedTreeViewItem;
        private TreeViewItem editingItem;
        private TreeViewItem rightClickedItem;
        private bool isEditing = false;

        // 剪贴板相关
        private TreeNodeData clipboardNode;
        private bool isCutOperation = false;
        private TreeViewItem sourceItemForCut;

        // 节点数据类
        public class TreeNodeData
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string PicturePath { get; set; }
            public string Gender { get; set; } = "未知";
            public List<TreeNodeData> Children { get; set; } = new List<TreeNodeData>();

            public TreeNodeData Clone()
            {
                return new TreeNodeData
                {
                    Name = this.Name,
                    Value = this.Value,
                    PicturePath = this.PicturePath,
                    Gender = this.Gender,
                    Children = this.Children.Select(c => c.Clone()).ToList()
                };
            }
        }

        public class EditData
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        private TreeViewItem CreateTreeViewItem(string name, string value, string picturePath = "", string gender = "未知")
        {
            // 创建图标和文本的StackPanel
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            // 性别图标
            var genderIcon = new Image
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 5, 0),
                Source = new BitmapImage(new Uri("pack://application:,,,/iEngr.Hookup;component/Resources/Icon1.ico")),
                ToolTip = "性别"
            };

            // 图片图标
            var pictureIcon = new Image
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 5, 0),
                Source = (ImageSource)FindResource("PictureIcon"),
                ToolTip = "包含图片",
                Visibility = string.IsNullOrEmpty(picturePath) ? Visibility.Collapsed : Visibility.Visible
            };

            // 节点文本
            string displayText = $"{name}: {value}";
            List<string> attributes = new List<string>();
            if (!string.IsNullOrEmpty(picturePath))
            {
                attributes.Add($"图片: {System.IO.Path.GetFileName(picturePath)}");
            }
            if (!string.IsNullOrEmpty(gender) && gender != "未知")
            {
                attributes.Add($"性别: {gender}");
            }
            if (attributes.Count > 0)
            {
                displayText += $" [{string.Join(", ", attributes)}]";
            }

            var textBlock = new TextBlock
            {
                Text = displayText,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 添加控件到StackPanel
            stackPanel.Children.Add(genderIcon);
            stackPanel.Children.Add(pictureIcon);
            stackPanel.Children.Add(textBlock);

            var item = new TreeViewItem
            {
                Header = stackPanel, // 直接设置Header为包含图标的StackPanel
                Tag = new TreeNodeData { Name = name, Value = value, PicturePath = picturePath, Gender = gender },
                ContextMenu = (ContextMenu)FindResource("SimpleContextMenu")
            };

            // 根据性别设置图标
            if (gender == "男性")
            {
                genderIcon.Source = (ImageSource)FindResource("MaleIcon");
            }
            else if (gender == "女性")
            {
                genderIcon.Source = (ImageSource)FindResource("FemaleIcon");
            }

            return item;
        }

        #region 事件处理
        private void TreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditing) return;

            // 找到点击的TreeViewItem
            var originalSource = e.OriginalSource as DependencyObject;
            var treeViewItem = FindVisualParent<TreeViewItem>(originalSource);

            if (treeViewItem != null)
            {
                // 设置选中状态
                treeViewItem.IsSelected = true;
                selectedTreeViewItem = treeViewItem;
                rightClickedItem = treeViewItem;

                // 阻止事件继续冒泡，确保右键菜单在正确的位置显示
                e.Handled = true;
            }
        }

        private TreeViewItem FindVisualParent<TreeViewItem>(DependencyObject child) where TreeViewItem : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is TreeViewItem))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            selectedTreeViewItem = e.NewValue as TreeViewItem;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.C:
                        ContextMenu_Copy_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.X:
                        ContextMenu_Cut_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.V:
                        if (clipboardNode != null)
                        {
                            ContextMenu_Paste_Click(null, null);
                            e.Handled = true;
                        }
                        break;
                }
            }
        }
        private void TreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // 确保右键点击的节点被正确记录
            var originalSource = e.OriginalSource as DependencyObject;
            rightClickedItem = FindVisualParent<TreeViewItem>(originalSource);

            // 更新粘贴菜单状态
            UpdatePasteMenuStatus();

            // 如果没有选中节点，但右键点击了节点，则选中它
            if (rightClickedItem != null && selectedTreeViewItem != rightClickedItem)
            {
                rightClickedItem.IsSelected = true;
                selectedTreeViewItem = rightClickedItem;
            }
        }
        private void ExpanderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // 找到父TreeViewItem
                var treeViewItem = FindVisualParent<TreeViewItem>(button);
                if (treeViewItem != null)
                {
                    treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
                }
            }
        }

        //private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        //{
        //    var parent = VisualTreeHelper.GetParent(child);
        //    while (parent != null && !(parent is T))
        //    {
        //        parent = VisualTreeHelper.GetParent(parent);
        //    }
        //    return parent as T;
        //}        
        #endregion

        #region 文件操作 - XML
        private void LoadXml_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "XML文件 (*.xml)|*.xml|所有文件 (*.*)|*.*",
                DefaultExt = ".xml",
                Title = "选择XML文件"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    currentFilePath = dialog.FileName;
                    string xmlContent = File.ReadAllText(currentFilePath, Encoding.UTF8);

                    treeView.Items.Clear();
                    XDocument xmlDoc = XDocument.Parse(xmlContent);
                    BuildTreeViewFromXml(xmlDoc.Root, treeView.Items);

                    currentFileText.Text = System.IO.Path.GetFileName(currentFilePath);
                    statusText.Text = $"已加载XML: {System.IO.Path.GetFileName(currentFilePath)}";
                    UpdateItemCount();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载XML文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveXml_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                MessageBox.Show("请先完成当前编辑操作。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(currentFilePath))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "XML文件 (*.xml)|*.xml|所有文件 (*.*)|*.*",
                    DefaultExt = ".xml",
                    AddExtension = true,
                    Title = "保存XML文件"
                };

                if (dialog.ShowDialog() == true)
                {
                    currentFilePath = dialog.FileName;
                }
                else
                {
                    return;
                }
            }

            try
            {
                XDocument xmlDoc = ConvertTreeViewToXml(treeView.Items);
                File.WriteAllText(currentFilePath, xmlDoc.ToString(), Encoding.UTF8);

                currentFileText.Text = System.IO.Path.GetFileName(currentFilePath);
                statusText.Text = $"已保存XML: {System.IO.Path.GetFileName(currentFilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存XML文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region XML处理功能
        private void BuildTreeViewFromXml(XElement element, ItemCollection items)
        {
            if (element == null) return;

            foreach (var childElement in element.Elements())
            {
                string nodeName = childElement.Name.LocalName;
                string nodeValue = GetElementValueWithoutChildren(childElement);
                string picturePath = childElement.Attribute("PicturePath")?.Value ?? "";
                string gender = childElement.Attribute("Gender")?.Value ?? "未知";

                var treeViewItem = CreateTreeViewItem(nodeName, nodeValue, picturePath, gender);
                items.Add(treeViewItem);

                // 递归处理子元素
                if (childElement.HasElements)
                {
                    BuildTreeViewFromXml(childElement, treeViewItem.Items);
                }
            }
        }

        private XDocument ConvertTreeViewToXml(ItemCollection items)
        {
            var rootElement = new XElement("TreeViewData");
            var xmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rootElement);

            foreach (TreeViewItem item in items)
            {
                var element = ConvertTreeViewItemToXElement(item);
                rootElement.Add(element);
            }

            return xmlDoc;
        }

        private XElement ConvertTreeViewItemToXElement(TreeViewItem item)
        {
            if (item.Tag is TreeNodeData nodeData)
            {
                string elementName = CleanXmlName(nodeData.Name);

                var element = new XElement(elementName);

                // 设置节点值
                if (!string.IsNullOrEmpty(nodeData.Value))
                {
                    element.Value = nodeData.Value;
                }

                // 添加附加属性
                if (!string.IsNullOrEmpty(nodeData.PicturePath))
                {
                    element.SetAttributeValue("PicturePath", nodeData.PicturePath);
                }
                if (!string.IsNullOrEmpty(nodeData.Gender) && nodeData.Gender != "未知")
                {
                    element.SetAttributeValue("Gender", nodeData.Gender);
                }

                // 递归处理子节点
                foreach (TreeViewItem childItem in item.Items)
                {
                    element.Add(ConvertTreeViewItemToXElement(childItem));
                }

                return element;
            }

            return new XElement("Unknown");
        }

        private string GetElementValueWithoutChildren(XElement element)
        {
            var textNodes = element.Nodes().OfType<XText>();
            return string.Concat(textNodes.Select(t => t.Value)).Trim();
        }

        private string CleanXmlName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "未命名";

            string cleaned = new string(name.Where(c =>
                char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.').ToArray());

            return string.IsNullOrEmpty(cleaned) ? "未命名" : cleaned;
        }
        #endregion

        #region 节点操作功能
        //private TreeViewItem CreateTreeViewItem(string name, string value, string picturePath = "", string gender = "未知")
        //{
        //    string displayText = $"{name}: {value}";

        //    // 添加附加属性显示
        //    List<string> attributes = new List<string>();
        //    if (!string.IsNullOrEmpty(picturePath))
        //    {
        //        attributes.Add($"图片: {System.IO.Path.GetFileName(picturePath)}");
        //    }
        //    if (!string.IsNullOrEmpty(gender) && gender != "未知")
        //    {
        //        attributes.Add($"性别: {gender}");
        //    }

        //    if (attributes.Count > 0)
        //    {
        //        displayText += $" [{string.Join(", ", attributes)}]";
        //    }

        //    return new TreeViewItem
        //    {
        //        Header = displayText,
        //        Tag = new TreeNodeData { Name = name, Value = value, PicturePath = picturePath, Gender = gender },
        //        ContextMenu = (ContextMenu)FindResource("SimpleContextMenu")
        //    };
        //}

        private void AddNewNode(ItemCollection items, string name, string value, string picturePath = "", string gender = "未知")
        {
            items.Add(CreateTreeViewItem(name, value, picturePath, gender));
        }

        private void ContextMenu_AddNode_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing) return;

            var targetItem = selectedTreeViewItem;
            string baseName = "新节点";
            string newName = baseName;
            int counter = 1;

            ItemCollection targetCollection = targetItem == null ?
                treeView.Items : targetItem.Items;

            while (CheckKeyExistsInSiblings(targetCollection, newName))
            {
                newName = $"{baseName}_{counter++}";
            }

            AddNewNode(targetCollection, newName, "新值");
            if (targetItem != null) targetItem.IsExpanded = true;

            statusText.Text = targetItem == null ? "新节点已添加到根节点" : "新子节点已添加";
            UpdateItemCount();
        }

        private void ContextMenu_EditNode_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing) return;

            if (selectedTreeViewItem == null)
            {
                MessageBox.Show("请选择一个要编辑的节点。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StartEditing(selectedTreeViewItem);
        }

        private void StartEditing(TreeViewItem item)
        {
            if (item.Tag is TreeNodeData nodeData)
            {
                isEditing = true;
                editingItem = item;

                var editData = new EditData { Name = nodeData.Name, Value = nodeData.Value };
                item.HeaderTemplate = (DataTemplate)Resources["EditTemplate"];
                item.Header = editData;

                item.Loaded += (s, e) =>
                {
                    var textBox = FindVisualChild<TextBox>(item, "editNameTextBox");
                    textBox?.Focus();
                    textBox?.SelectAll();
                };

                statusText.Text = "正在编辑节点...";
            }
        }

        private void EditConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (editingItem != null && editingItem.Header is EditData editData)
            {
                if (string.IsNullOrWhiteSpace(editData.Name))
                {
                    MessageBox.Show("节点名称不能为空。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (editingItem.Tag is TreeNodeData nodeData)
                {
                    string originalKey = nodeData.Name;

                    if (editData.Name != originalKey && CheckKeyExistsInCurrentLevel(editingItem, editData.Name))
                    {
                        MessageBox.Show($"Key '{editData.Name}' 在同一层级已存在，请使用不同的Key值。", "Key重复",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // 更新节点数据
                    nodeData.Name = editData.Name;
                    nodeData.Value = editData.Value;
                    UpdateTreeViewItemDisplay(editingItem);

                    editingItem.HeaderTemplate = null;
                    isEditing = false;
                    editingItem = null;
                    statusText.Text = "节点编辑完成";
                }
            }
        }

        private void UpdateTreeViewItemDisplay(TreeViewItem item)
        {
            if (item.Tag is TreeNodeData nodeData)
            {
                string displayText = $"{nodeData.Name}: {nodeData.Value}";

                List<string> attributes = new List<string>();
                if (!string.IsNullOrEmpty(nodeData.PicturePath))
                {
                    attributes.Add($"图片: {System.IO.Path.GetFileName(nodeData.PicturePath)}");
                }
                if (!string.IsNullOrEmpty(nodeData.Gender) && nodeData.Gender != "未知")
                {
                    attributes.Add($"性别: {nodeData.Gender}");
                }

                if (attributes.Count > 0)
                {
                    displayText += $" [{string.Join(", ", attributes)}]";
                }

                item.Header = displayText;
            }
        }

        private void EditCancel_Click(object sender, RoutedEventArgs e)
        {
            if (editingItem != null && editingItem.Tag is TreeNodeData nodeData)
            {
                UpdateTreeViewItemDisplay(editingItem);
                editingItem.HeaderTemplate = null;
                isEditing = false;
                editingItem = null;
                statusText.Text = "编辑已取消";
            }
        }

        private void EditTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EditConfirm_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                EditCancel_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ContextMenu_DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing) return;

            if (selectedTreeViewItem == null)
            {
                MessageBox.Show("请选择一个要删除的节点。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("确定要删除这个节点吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DeleteNode(selectedTreeViewItem);
                statusText.Text = "节点已删除";
                UpdateItemCount();
            }
        }

        private void DeleteNode(TreeViewItem item)
        {
            if (item.Parent is TreeViewItem parentItem)
            {
                parentItem.Items.Remove(item);
            }
            else
            {
                treeView.Items.Remove(item);
            }

            selectedTreeViewItem = null;
        }

        private void ContextMenu_SetPicturePath_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTreeViewItem == null)
            {
                MessageBox.Show("请选择一个节点。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Filter = "图片文件 (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件 (*.*)|*.*",
                Title = "选择图片文件"
            };

            if (dialog.ShowDialog() == true)
            {
                if (selectedTreeViewItem.Tag is TreeNodeData nodeData)
                {
                    nodeData.PicturePath = dialog.FileName;
                    UpdateTreeViewItemDisplay(selectedTreeViewItem);
                    statusText.Text = "图片路径已设置";
                }
            }
        }

        private void ContextMenu_SetGender_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTreeViewItem == null) return;

            var popup = new Popup
            {
                Placement = PlacementMode.Bottom,
                PlacementTarget = selectedTreeViewItem,
                StaysOpen = false,
                AllowsTransparency = true,
                Width = 150,
                IsOpen = true
            };

            var listBox = new ListBox
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0),
                ItemsSource = new[]
                {
            new { Display = "🚹 男性", Value = "男性" },
            new { Display = "🚺 女性", Value = "女性" },
            new { Display = "❓ 未知", Value = "未知" }
        },
                DisplayMemberPath = "Display"
            };

            listBox.SelectionChanged += (s, args) =>
            {
                if (listBox.SelectedItem != null)
                {
                    dynamic selected = listBox.SelectedItem;
                    SetGender(selected.Value, popup);
                }
            };

            popup.Child = listBox;
        }
        private void SetGender(string gender, Popup popup)
        {
            popup.IsOpen = false;

            if (selectedTreeViewItem?.Tag is TreeNodeData nodeData)
            {
                nodeData.Gender = gender;
                UpdateTreeViewItemDisplay(selectedTreeViewItem);
                statusText.Text = $"性别已设置为: {gender}";
            }
        }
        //private void ContextMenu_SetGender_Click(object sender, RoutedEventArgs e)
        //{
        //    if (selectedTreeViewItem == null) return;

        //    // 如果是通过ContextMenu触发的，可以直接使用ContextMenu的位置
        //    if (sender is MenuItem menuItem && menuItem.CommandParameter is ContextMenu contextMenu)
        //    {
        //        var dialog = new GenderSelectionDialog();
        //        dialog.WindowStartupLocation = WindowStartupLocation.Manual;
        //        dialog.Left = contextMenu.ActualWidth; // 或者使用其他逻辑
        //        dialog.Top = contextMenu.ActualHeight;

        //        if (dialog.ShowDialog() == true)
        //        {
        //            // 处理结果
        //        }
        //    }
        //    else
        //    {
        //        // 使用鼠标位置
        //        Point mousePosition = GetCurrentMousePosition();
        //        var dialog = new GenderSelectionDialog();
        //        dialog.WindowStartupLocation = WindowStartupLocation.Manual;
        //        dialog.Left = mousePosition.X;
        //        dialog.Top = mousePosition.Y;

        //        if (dialog.ShowDialog() == true)
        //        {
        //            // 处理结果
        //        }
        //    }
        //}
        //// 获取当前鼠标位置
        //private Point GetCurrentMousePosition()
        //{
        //    // 获取鼠标在屏幕上的位置
        //    System.Windows.Point mousePoint = Mouse.GetPosition(this);

        //    // 转换为屏幕坐标
        //    Point screenPoint = this.PointToScreen(mousePoint);

        //    return screenPoint;
        //}
        //private void ContextMenu_SetGender_Click(object sender, RoutedEventArgs e)
        //{
        //    if (selectedTreeViewItem == null) return;

        //    // 直接使用Mouse.GetPosition获取屏幕坐标
        //    System.Windows.Point mousePoint = Mouse.GetPosition(this);
        //    Point screenPoint = this.PointToScreen(mousePoint);

        //    var dialog = new GenderSelectionDialog();
        //    dialog.WindowStartupLocation = WindowStartupLocation.Manual;
        //    dialog.Left = screenPoint.X;
        //    dialog.Top = screenPoint.Y;

        //    // 简单的边界检查
        //    if (dialog.Left + dialog.Width > SystemParameters.WorkArea.Width)
        //    {
        //        dialog.Left = SystemParameters.WorkArea.Width - dialog.Width - 10;
        //    }
        //    if (dialog.Top + dialog.Height > SystemParameters.WorkArea.Height)
        //    {
        //        dialog.Top = SystemParameters.WorkArea.Height - dialog.Height - 10;
        //    }

        //    if (dialog.ShowDialog() == true)
        //    {
        //        if (selectedTreeViewItem.Tag is TreeNodeData nodeData)
        //        {
        //            nodeData.Gender = dialog.SelectedGender;
        //            UpdateTreeViewItemDisplay(selectedTreeViewItem);
        //            statusText.Text = "性别已设置";
        //        }
        //    }
        //}
        //private void ContextMenu_SetGender_Click(object sender, RoutedEventArgs e)
        //{
        //    if (selectedTreeViewItem == null) return;

        //    // 创建Popup
        //    var popup = new Popup
        //    {
        //        Placement = PlacementMode.Bottom,
        //        PlacementTarget = selectedTreeViewItem,
        //        StaysOpen = false,
        //        Width = 150,
        //        Height = 100
        //    };

        //    // 创建内容
        //    var stackPanel = new StackPanel { Background = Brushes.White };

        //    var maleButton = new Button { Content = "男性", Margin = new Thickness(5) };
        //    var femaleButton = new Button { Content = "女性", Margin = new Thickness(5) };
        //    var unknownButton = new Button { Content = "未知", Margin = new Thickness(5) };

        //    maleButton.Click += (s, args) => SetGenderAndClose("男性", popup);
        //    femaleButton.Click += (s, args) => SetGenderAndClose("女性", popup);
        //    unknownButton.Click += (s, args) => SetGenderAndClose("未知", popup);

        //    stackPanel.Children.Add(maleButton);
        //    stackPanel.Children.Add(femaleButton);
        //    stackPanel.Children.Add(unknownButton);

        //    popup.Child = stackPanel;
        //    popup.IsOpen = true;
        //}

        //private void SetGenderAndClose(string gender, Popup popup)
        //{
        //    if (selectedTreeViewItem?.Tag is TreeNodeData nodeData)
        //    {
        //        nodeData.Gender = gender;
        //        UpdateTreeViewItemDisplay(selectedTreeViewItem);
        //        statusText.Text = "性别已设置";
        //    }
        //    popup.IsOpen = false;
        //}        

        #endregion

        // 其他辅助方法（复制粘贴、Key检查等）保持不变
        // ...
        #region 复制粘贴功能
        private void ContextMenu_Copy_Click(object sender, RoutedEventArgs e)
        {
            var targetItem = rightClickedItem ?? selectedTreeViewItem;
            if (targetItem == null)
            {
                MessageBox.Show("请先选择一个节点。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            clipboardNode = ConvertTreeViewItemToData(targetItem);
            isCutOperation = false;
            sourceItemForCut = null;
            statusText.Text = "已复制节点";
            UpdatePasteMenuStatus();
        }

        private void ContextMenu_Cut_Click(object sender, RoutedEventArgs e)
        {
            var targetItem = rightClickedItem ?? selectedTreeViewItem;
            if (targetItem == null)
            {
                MessageBox.Show("请先选择一个节点。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            clipboardNode = ConvertTreeViewItemToData(targetItem);
            isCutOperation = true;
            sourceItemForCut = targetItem;
            statusText.Text = "已剪切节点";
            UpdatePasteMenuStatus();
        }

        private void ContextMenu_Paste_Click(object sender, RoutedEventArgs e)
        {
            if (clipboardNode == null) return;

            var targetItem = rightClickedItem ?? selectedTreeViewItem;
            ItemCollection targetCollection = targetItem == null ?
                treeView.Items : targetItem.Items;

            if (CheckKeyExistsInSiblings(targetCollection, clipboardNode.Name))
            {
                MessageBox.Show($"Key '{clipboardNode.Name}' 在同一层级已存在，请修改Key值后再粘贴。", "Key重复",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newItem = CreateTreeViewItemFromData(clipboardNode);
            targetCollection.Add(newItem);
            if (targetItem != null) targetItem.IsExpanded = true;

            if (isCutOperation && sourceItemForCut != null)
            {
                DeleteNode(sourceItemForCut);
                sourceItemForCut = null;
            }

            statusText.Text = "已粘贴节点";
            UpdateItemCount();
            CheckDuplicateKeys();
        }

        private TreeNodeData ConvertTreeViewItemToData(TreeViewItem item)
        {
            if (item.Tag is TreeNodeData nodeData)
            {
                var clonedData = nodeData.Clone();

                // 递归处理子节点
                foreach (TreeViewItem childItem in item.Items)
                {
                    clonedData.Children.Add(ConvertTreeViewItemToData(childItem));
                }

                return clonedData;
            }

            return new TreeNodeData();
        }

        private TreeViewItem CreateTreeViewItemFromData(TreeNodeData data)
        {
            var item = CreateTreeViewItem(data.Name, data.Value, data.PicturePath);

            // 递归创建子节点
            foreach (var childData in data.Children)
            {
                item.Items.Add(CreateTreeViewItemFromData(childData));
            }

            return item;
        }

        private void UpdatePasteMenuStatus()
        {
            var contextMenu = treeView.ContextMenu;
            if (contextMenu != null)
            {
                foreach (var item in contextMenu.Items)
                {
                    if (item is MenuItem menuItem && menuItem.Name == "pasteMenuItem")
                    {
                        menuItem.IsEnabled = clipboardNode != null;
                        break;
                    }
                }
            }
        }
        #endregion

        #region 移动功能
        private void ContextMenu_MoveUp_Click(object sender, RoutedEventArgs e)
        {
            var targetItem = rightClickedItem ?? selectedTreeViewItem;
            if (targetItem == null) return;
            MoveNode(targetItem, -1);
        }

        private void ContextMenu_MoveDown_Click(object sender, RoutedEventArgs e)
        {
            var targetItem = rightClickedItem ?? selectedTreeViewItem;
            if (targetItem == null) return;
            MoveNode(targetItem, 1);
        }

        private void MoveNode(TreeViewItem item, int direction)
        {
            var parentCollection = GetParentItemCollection(item);
            if (parentCollection == null) return;

            int currentIndex = parentCollection.IndexOf(item);
            int newIndex = currentIndex + direction;

            if (newIndex >= 0 && newIndex < parentCollection.Count)
            {
                parentCollection.RemoveAt(currentIndex);
                parentCollection.Insert(newIndex, item);
                statusText.Text = direction < 0 ? "已向上移动" : "已向下移动";
            }
        }

        private ItemCollection GetParentItemCollection(TreeViewItem item)
        {
            if (item.Parent is TreeViewItem parentItem)
                return parentItem.Items;
            else if (item.Parent is TreeView)
                return treeView.Items;
            return null;
        }
        #endregion

        #region Key检查功能
        private void CheckDuplicateKeys_Click(object sender, RoutedEventArgs e)
        {
            CheckDuplicateKeys();
        }

        private void CheckDuplicateKeys()
        {
            ClearDuplicateStyles();
            var duplicates = FindDuplicateKeysInSiblings(treeView.Items);

            if (duplicates.Count > 0)
            {
                keyCheckText.Text = $"发现 {duplicates.Count} 个同级重复Key";
                statusText.Text = "发现同级重复Key，已用红色背景标记";

                foreach (var duplicate in duplicates)
                {
                    var style = (Style)FindResource("DuplicateKeyStyle");
                    if (style != null) duplicate.Style = style;
                }
            }
            else
            {
                keyCheckText.Text = "无同级重复Key";
                statusText.Text = "Key值检查完成，无同级重复";
            }
        }

        private List<TreeViewItem> FindDuplicateKeysInSiblings(ItemCollection items)
        {
            var duplicates = new List<TreeViewItem>();
            var keyCount = new Dictionary<string, List<TreeViewItem>>();

            foreach (TreeViewItem item in items)
            {
                string key = GetItemKey(item);
                if (!string.IsNullOrEmpty(key))
                {
                    if (!keyCount.ContainsKey(key)) keyCount[key] = new List<TreeViewItem>();
                    keyCount[key].Add(item);
                }

                if (item.Items.Count > 0)
                {
                    duplicates.AddRange(FindDuplicateKeysInSiblings(item.Items));
                }
            }

            foreach (var kvp in keyCount.Where(k => k.Value.Count > 1))
            {
                duplicates.AddRange(kvp.Value);
            }

            return duplicates;
        }

        private bool CheckKeyExistsInSiblings(ItemCollection collection, string key, TreeViewItem excludeItem = null)
        {
            foreach (TreeViewItem item in collection)
            {
                if (excludeItem != null && item == excludeItem) continue;
                if (GetItemKey(item) == key) return true;
            }
            return false;
        }

        private bool CheckKeyExistsInCurrentLevel(TreeViewItem item, string newKey)
        {
            var parentCollection = GetParentItemCollection(item);
            return parentCollection != null && CheckKeyExistsInSiblings(parentCollection, newKey, item);
        }

        private void CheckKeyDuringEditing(string newKey, string originalKey)
        {
            if (editingItem == null || newKey == originalKey) return;

            if (CheckKeyExistsInCurrentLevel(editingItem, newKey))
            {
                editingItem.Background = Brushes.LightPink;
                statusText.Text = "Key在同一层级已存在，请修改!";
            }
            else
            {
                editingItem.ClearValue(TreeViewItem.BackgroundProperty);
                statusText.Text = "正在编辑节点...";
            }
        }

        private string GetItemKey(TreeViewItem item)
        {
            if (item.Tag is TreeNodeData nodeData)
                return nodeData.Name;
            return null;
        }

        private void ClearDuplicateStyles()
        {
            ClearStylesRecursive(treeView.Items);
        }

        private void ClearStylesRecursive(ItemCollection items)
        {
            foreach (TreeViewItem item in items)
            {
                item.ClearValue(TreeViewItem.StyleProperty);
                if (item.Items.Count > 0) ClearStylesRecursive(item.Items);
            }
        }
        #endregion

        #region 辅助功能
        private void ExpandAllNodes(ItemCollection items, bool expand)
        {
            foreach (var item in items)
            {
                if (item is TreeViewItem treeViewItem)
                {
                    treeViewItem.IsExpanded = expand;
                    ExpandAllNodes(treeViewItem.Items, expand);
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string name = null) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result && (name == null || (child is FrameworkElement fe && fe.Name == name)))
                    return result;

                var descendant = FindVisualChild<T>(child, name);
                if (descendant != null) return descendant;
            }
            return null;
        }

        private void UpdateItemCount()
        {
            int count = CountTreeViewItems(treeView.Items);
            itemCountText.Text = $"{count} 个节点";
        }

        private int CountTreeViewItems(ItemCollection items)
        {
            int count = 0;
            foreach (var item in items)
            {
                count++;
                if (item is TreeViewItem treeViewItem)
                    count += CountTreeViewItems(treeViewItem.Items);
            }
            return count;
        }
        #endregion

        #region 右键菜单功能
        private void ContextMenu_ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            var targetItem = rightClickedItem ?? selectedTreeViewItem;

            if (targetItem != null)
            {
                // 展开当前节点和所有子节点
                ExpandNodeAndChildren(targetItem, true);
                statusText.Text = $"已展开节点: {GetNodeDisplayText(targetItem)} 及其所有子节点";
            }
            else if (treeView.Items.Count > 0)
            {
                // 展开所有根节点
                ExpandAllNodes(treeView.Items, true);
                statusText.Text = "已展开所有根节点";
            }
        }

        private void ContextMenu_CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            var targetItem = rightClickedItem ?? selectedTreeViewItem;

            if (targetItem != null)
            {
                // 折叠当前节点和所有子节点
                ExpandNodeAndChildren(targetItem, false);
                statusText.Text = $"已折叠节点: {GetNodeDisplayText(targetItem)} 及其所有子节点";
            }
            else if (treeView.Items.Count > 0)
            {
                // 折叠所有根节点
                ExpandAllNodes(treeView.Items, false);
                statusText.Text = "已折叠所有根节点";
            }
        }

        private void ExpandNodeAndChildren(TreeViewItem node, bool expand)
        {
            node.IsExpanded = expand;
            if (node.HasItems)
            {
                ExpandAllNodes(node.Items, expand);
            }
        }

        private string GetNodeDisplayText(TreeViewItem item)
        {
            if (item.Header is string headerText)
            {
                // 提取节点名称（去掉值和属性信息）
                int colonIndex = headerText.IndexOf(':');
                if (colonIndex > 0)
                {
                    return headerText.Substring(0, colonIndex).Trim();
                }
                return headerText;
            }
            return "节点";
        }

        #endregion


    }
}
