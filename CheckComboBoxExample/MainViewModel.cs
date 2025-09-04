using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
namespace CheckComboBoxExample
{

    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Item> _allItems;
        private ObservableCollection<Item> _selectedItems;
        private string _selectedItemsText;
        private string _selectedIdsText;

        public ObservableCollection<Item> AllItems
        {
            get => _allItems;
            set { _allItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Item> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;
                OnPropertyChanged();
                UpdateSelectedText();
            }
        }

        public string SelectedItemsText
        {
            get => _selectedItemsText;
            set { _selectedItemsText = value; OnPropertyChanged(); }
        }

        public string SelectedIdsText
        {
            get => _selectedIdsText;
            set { _selectedIdsText = value; OnPropertyChanged(); }
        }

        // 获取选中ID的只读属性
        public IEnumerable<int> SelectedIds => SelectedItems?.Select(item => item.Id) ?? Enumerable.Empty<int>();

        public MainViewModel()
        {
            InitializeData();
            SelectedItems = new ObservableCollection<Item>();
        }

        private void InitializeData()
        {
            AllItems = new ObservableCollection<Item>
        {
            new Item { Id = 1, Name = "苹果", Category = "水果" },
            new Item { Id = 2, Name = "香蕉", Category = "水果" },
            new Item { Id = 3, Name = "橙子", Category = "水果" },
            new Item { Id = 4, Name = "胡萝卜", Category = "蔬菜" },
            new Item { Id = 5, Name = "西红柿", Category = "蔬菜" },
            new Item { Id = 6, Name = "黄瓜", Category = "蔬菜" },
            new Item { Id = 7, Name = "牛肉", Category = "肉类" },
            new Item { Id = 8, Name = "鸡肉", Category = "肉类" },
            new Item { Id = 9, Name = "鱼肉", Category = "肉类" }
    };
        }

        private void UpdateSelectedText()
        {
            SelectedItemsText = SelectedItems.Any()
                ? string.Join(", ", SelectedItems.Select(i => i.Name))
                : "无选中项";

            SelectedIdsText = SelectedItems.Any()
                ? string.Join(", ", SelectedItems.Select(i => i.Id))
                : "无选中ID";
        }

        public void SelectItemsByIds(params int[] ids)
        {
            var itemsToSelect = AllItems.Where(item => ids.Contains(item.Id)).ToList();
            foreach (var item in itemsToSelect)
            {
                if (!SelectedItems.Contains(item))
                {
                    SelectedItems.Add(item);
                }
            }
            UpdateSelectedText();
        }

        public void ClearSelection()
        {
            SelectedItems.Clear();
            UpdateSelectedText();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}