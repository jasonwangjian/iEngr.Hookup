using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TreeViewMoveExample
{
    public class TreeItemViewModel : INotifyPropertyChanged
    {
        private string _name;
        private bool _isExpanded;
        private bool _isSelected;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TreeItemViewModel> Children { get; set; }
        public TreeItemViewModel Parent { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public TreeItemViewModel()
        {
            Children = new ObservableCollection<TreeItemViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TreeItemViewModel> TreeItems { get; set; }
        public ICommand MoveToParentCommand { get; set; }
        public ICommand MoveToChildCommand { get; set; }

        private TreeItemViewModel _selectedItem;
        public TreeItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public MainViewModel()
        {
            TreeItems = new ObservableCollection<TreeItemViewModel>();
            MoveToParentCommand = new RelayCommand(MoveToParent, CanMoveToParent);
            MoveToChildCommand = new RelayCommand(MoveToChild, CanMoveToChild);

            InitializeTreeData();
        }

        private void InitializeTreeData()
        {
            // 创建层次结构数据
            for (int i = 1; i <= 3; i++)
            {
                var parent = new TreeItemViewModel { Name = $"父节点 {i}" };

                for (int j = 1; j <= 3; j++)
                {
                    var child = new TreeItemViewModel
                    {
                        Name = $"子节点 {i}-{j}",
                        Parent = parent
                    };

                    for (int k = 1; k <= 2; k++)
                    {
                        child.Children.Add(new TreeItemViewModel
                        {
                            Name = $"孙节点 {i}-{j}-{k}",
                            Parent = child
                        });
                    }

                    parent.Children.Add(child);
                }

                TreeItems.Add(parent);
            }
        }

        private bool CanMoveToParent(object parameter)
        {
            // 只有有父节点且父节点不是根节点时才能移动
            return SelectedItem != null &&
                   SelectedItem.Parent != null &&
                   SelectedItem.Parent.Parent != null; // 确保父节点不是根节点
        }

        private bool CanMoveToChild(object parameter)
        {
            // 可以选择一个兄弟节点作为新的父节点
            return SelectedItem != null &&
                   SelectedItem.Parent != null &&
                   SelectedItem.Parent.Children.Count > 1;
        }

        private void MoveToParent(object parameter)
        {
            if (SelectedItem == null || SelectedItem.Parent == null || SelectedItem.Parent.Parent == null)
                return;

            var currentItem = SelectedItem;
            var oldParent = currentItem.Parent;
            var newParent = oldParent.Parent; // 祖父节点成为新父节点

            // 保存状态
            bool wasExpanded = currentItem.IsExpanded;
            bool wasSelected = currentItem.IsSelected;

            // 从原父节点移除
            oldParent.Children.Remove(currentItem);

            // 更新父级引用
            currentItem.Parent = newParent;

            // 添加到新父节点
            newParent.Children.Add(currentItem);

            // 恢复状态
            currentItem.IsExpanded = wasExpanded;
            currentItem.IsSelected = wasSelected;

            // 确保新父节点展开
            newParent.IsExpanded = true;
        }

        private void MoveToChild(object parameter)
        {
            if (SelectedItem == null || SelectedItem.Parent == null || SelectedItem.Parent.Children.Count <= 1)
                return;

            // 选择兄弟节点作为新父节点（这里选择第一个兄弟节点）
            var sibling = SelectedItem.Parent.Children
                .FirstOrDefault(x => x != SelectedItem);

            if (sibling == null) return;

            var currentItem = SelectedItem;
            var oldParent = currentItem.Parent;

            // 保存状态
            bool wasExpanded = currentItem.IsExpanded;
            bool wasSelected = currentItem.IsSelected;

            // 从原父节点移除
            oldParent.Children.Remove(currentItem);

            // 更新父级引用
            currentItem.Parent = sibling;

            // 添加到兄弟节点的子节点
            sibling.Children.Add(currentItem);

            // 恢复状态
            currentItem.IsExpanded = wasExpanded;
            currentItem.IsSelected = wasSelected;

            // 确保新父节点展开
            sibling.IsExpanded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
