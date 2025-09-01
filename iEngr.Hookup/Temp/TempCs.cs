using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml.Linq;
using System.IO;
using System;

namespace TreeViewMoveExample
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TreeItemViewModel> TreeItems { get; set; }
        
        // ... 其他命令和属性保持不变 ...

        public MainViewModel()
        {
            TreeItems = new ObservableCollection<TreeItemViewModel>();
            
            // 初始化命令
            MoveToParentCommand = new RelayCommand(MoveToParent, _ => CanMoveToParent);
            MoveToChildCommand = new RelayCommand(MoveToChild, _ => CanMoveToChild);
            MoveUpCommand = new RelayCommand(MoveUp, _ => CanMoveUp);
            MoveDownCommand = new RelayCommand(MoveDown, _ => CanMoveDown);
            ExpandAllCommand = new RelayCommand(ExpandAll);
            CollapseAllCommand = new RelayCommand(CollapseAll);
            CopyCommand = new RelayCommand(Copy, _ => SelectedItem != null);
            CutCommand = new RelayCommand(Cut, _ => SelectedItem != null);
            PasteCommand = new RelayCommand(Paste, _ => CanPaste);
            DeleteCommand = new RelayCommand(Delete, _ => CanDelete);
            
            // 从XML文件加载数据
            LoadTreeDataFromXml();
        }

        // 从XML文件加载树形数据
        private void LoadTreeDataFromXml()
        {
            try
            {
                string xmlFilePath = GetXmlFilePath();
                
                if (File.Exists(xmlFilePath))
                {
                    XDocument doc = XDocument.Load(xmlFilePath);
                    var rootNodes = doc.Root.Elements("Node");
                    
                    foreach (var node in rootNodes)
                    {
                        var treeItem = ParseXmlNode(node, null);
                        TreeItems.Add(treeItem);
                    }
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
        private TreeItemViewModel ParseXmlNode(XElement xmlNode, TreeItemViewModel parent)
        {
            var treeItem = new TreeItemViewModel
            {
                Name = xmlNode.Attribute("Name")?.Value ?? "未命名节点",
                Parent = parent
            };

            // 递归处理子节点
            foreach (var childNode in xmlNode.Elements("Node"))
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
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TreeData.xml");
        }

        // 保存树形数据到XML文件（可选功能）
        public void SaveTreeDataToXml()
        {
            try
            {
                XDocument doc = new XDocument(
                    new XElement("TreeNodes",
                        from item in TreeItems
                        select ConvertToXmlNode(item)
                    )
                );

                doc.Save(GetXmlFilePath());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存XML数据失败: {ex.Message}");
            }
        }

        // 将TreeItem转换为XML节点（递归方法）
        private XElement ConvertToXmlNode(TreeItemViewModel treeItem)
        {
            return new XElement("Node",
                new XAttribute("Name", treeItem.Name),
                from child in treeItem.Children
                select ConvertToXmlNode(child)
            );
        }

        // 默认测试数据（备用）
        private void InitializeDefaultData()
        {
            TreeItems.Clear();

            // 创建简单的默认数据结构
            var parent1 = new TreeItemViewModel { Name = "默认节点1" };
            var parent2 = new TreeItemViewModel { Name = "默认节点2" };
            
            for (int j = 1; j <= 2; j++)
            {
                var child = new TreeItemViewModel { 
                    Name = $"子节点 {j}", 
                    Parent = parent1 
                };
                parent1.Children.Add(child);
            }

            for (int j = 1; j <= 2; j++)
            {
                var child = new TreeItemViewModel { 
                    Name = $"子节点 {j}", 
                    Parent = parent2 
                };
                parent2.Children.Add(child);
            }

            TreeItems.Add(parent1);
            TreeItems.Add(parent2);
        }

        // ... 其他方法保持不变 ...
    }
}