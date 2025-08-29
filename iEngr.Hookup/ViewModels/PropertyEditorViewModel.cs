using iEngr.Hookup.Models;
using iEngr.Hookup.Services;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class PropertyEditorViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PropertyDefinition> _selectedAvailableProperties = new ObservableCollection<PropertyDefinition>();

        public ObservableCollection<PropertyDefinition> SelectedAvailableProperties
        {
            get => _selectedAvailableProperties;
            set
            {
                if (_selectedAvailableProperties != value)
                {
                    _selectedAvailableProperties = value;
                    OnPropertyChanged();
                }
            }
        }

        // 修改更新方法
        public void UpdateSelectedAvailableItems(IList selectedItems)
        {
            _selectedAvailableProperties.Clear();
            foreach (var item in selectedItems)
            {
                if (item is PropertyDefinition property)
                {
                    _selectedAvailableProperties.Add(property);
                }
            }
        }
        private readonly HkTreeItem _treeItem;
        private string _filterText;
        private CollectionViewSource _filteredPropertiesSource;

        public ObservableCollection<PropertyDefinition> AvailableProperties { get; }
        public ObservableCollection<PropertyDefinition> SelectedProperties { get; }
        public ICollectionView FilteredProperties => _filteredPropertiesSource.View;

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged();
                _filteredPropertiesSource.View.Refresh();
            }
        }


        public RelayCommand<object> AddSelectedPropertiesCommand { get; }
        public RelayCommand<string> RemovePropertyCommand { get; }

        public RelayCommand<object> OKCommand { get; }
        public RelayCommand<object> CancelCommand { get; }

        public PropertyEditorViewModel(HkTreeItem treeItem)
        {
            _treeItem = treeItem;

            // 初始化编辑属性
            InitializeEditingProperties();

            AvailableProperties = new ObservableCollection<PropertyDefinition>(PropertyLibrary.AllProperties);
            SelectedAvailableProperties = new ObservableCollection<PropertyDefinition>();
            SelectedProperties = new ObservableCollection<PropertyDefinition>();

            // 初始化已选属性
            foreach (var key in treeItem.SelectedPropertyKeys)
            {
                var prop = PropertyLibrary.GetPropertyDefinition(key);
                if (prop != null)
                {
                    SelectedProperties.Add(prop);
                }
            }

            // 设置过滤
            _filteredPropertiesSource = new CollectionViewSource { Source = AvailableProperties };
            _filteredPropertiesSource.Filter += FilterProperties;

            // 命令
            AddSelectedPropertiesCommand = new RelayCommand<object>(
                execute: _ => AddSelectedProperties(),
                canExecute: _ => CanAddMoreProperties
            );

            RemovePropertyCommand = new RelayCommand<string>(
                execute: RemoveProperty,
                canExecute: key => !string.IsNullOrEmpty(key)
            );
            SelectedProperties.CollectionChanged += SelectedProperties_CollectionChanged;

            OKCommand = new RelayCommand<object>(_ => OK());
            CancelCommand = new RelayCommand<object>(_ => Cancel());

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedProperties))
                {
                    ((RelayCommand<object>)AddSelectedPropertiesCommand).RaiseCanExecuteChanged();
                }
            };
        }

        private void FilterProperties(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                e.Accepted = true;
                return;
            }

            var prop = (PropertyDefinition)e.Item;
            e.Accepted = prop.DisplayName.Contains(FilterText) ||
                         prop.Key.Contains(FilterText) ||
                         prop.Category.Contains(FilterText);
        }

        // 修改CanAddMoreProperties
        public bool CanAddMoreProperties => EditingProperties.Count < 5 && SelectedAvailableProperties.Any();
        // 修改添加属性的方法
        private void AddSelectedProperties()
        {
            foreach (var prop in SelectedAvailableProperties.ToList())
            {
                if (EditingProperties.Count >= 5) break;
                if (!EditingProperties.Any(p => p.Definition.Key == prop.Key))
                {
                    var editItem = new PropertyEditItem(prop);
                    EditingProperties.Add(editItem);
                }
            }
            SelectedAvailableProperties.Clear();
        }

        private bool CanExecuteRemoveProperty(string key)
        {
            return !string.IsNullOrEmpty(key) &&
                   SelectedProperties.Any(p => p.Key == key);
        }

        private void SelectedProperties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // 当选择属性变化时，重新评估所有命令
            RemovePropertyCommand.RaiseCanExecuteChanged();
            AddSelectedPropertiesCommand.RaiseCanExecuteChanged();
        }

        // 修改移除属性的方法
        private void RemoveProperty(string key)
        {
            var editItem = EditingProperties.FirstOrDefault(p => p.Definition.Key == key);
            if (editItem != null)
            {
                EditingProperties.Remove(editItem);
            }
        }

        public event EventHandler<bool?> CloseRequested;
        public event EventHandler<HkTreeItem> PropertiesUpdated;
        private ObservableCollection<PropertyEditItem> _editingProperties = new ObservableCollection<PropertyEditItem>();
        public ObservableCollection<PropertyEditItem> EditingProperties => _editingProperties;
        private void OK()
        {
            // 1. 更新选择属性键
            _treeItem.SelectedPropertyKeys = new ObservableCollection<string>(
                EditingProperties.Select(p => p.Definition.Key));

            // 2. 更新属性值
            foreach (var editItem in EditingProperties)
            {
                _treeItem.SetProperty(editItem.Definition.Key, editItem.Value);
            }

            PropertiesUpdated?.Invoke(this, _treeItem);
            CloseRequested?.Invoke(this, true);
        }
        // 初始化编辑属性
        private void InitializeEditingProperties()
        {
            _editingProperties.Clear();

            // 添加已选择的属性
            foreach (var key in _treeItem.SelectedPropertyKeys)
            {
                var propDef = PropertyLibrary.GetPropertyDefinition(key);
                if (propDef != null)
                {
                    var editItem = new PropertyEditItem(propDef);

                    // 设置现有值或默认值
                    if (_treeItem.Properties.ContainsKey(key))
                    {
                        editItem.Value = _treeItem.Properties[key];
                    }

                    _editingProperties.Add(editItem);
                }
            }
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}