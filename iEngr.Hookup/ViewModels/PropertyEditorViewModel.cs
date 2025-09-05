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
        public PropertyEditorViewModel(HkTreeItem treeItem)
        {
            _treeItem = treeItem;

            AvailableProperties = new ObservableCollection<PropertyDefinition>(PropertyLibrary.AllProperties);
            SelectedAvailableProperties = new ObservableCollection<PropertyDefinition>();
            SelectedProperties = new ObservableCollection<PropertyDefinition>();

            // 初始化已选属性
            foreach (var prop in treeItem.Properties)
            {
                var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
                if (propDef != null)
                {
                    if (propDef.Type == PropertyType.EnumItems)
                    {
                        propDef.SelectedItems = prop.Value as ObservableCollection<GeneralItem>;
                        if (propDef.SelectedItems != null)
                            propDef.Value = string.Join(", ", propDef.SelectedItems.Select(x => x.Code)?.ToList());
                    }
                    else if (propDef.Type == PropertyType.EnumItem)
                    {
                        propDef.SelectedItem = prop.Value as GeneralItem;
                        propDef.Value = propDef.SelectedItem?.Code;
                    }
                    else
                        propDef.Value = prop.Value;
                    SelectedProperties.Add(propDef);
                }
            }
            //foreach (var key in treeItem.SelectedPropertyKeys)
            //{
            //    var prop = PropertyLibrary.GetPropertyDefinition(key);
            //    if (prop != null)
            //    {
            //        SelectedProperties.Add(prop);
            //    }
            //}
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
        public void UpdateSelectedAvailableProperties(IList selectedItems)
        {
            var newSelection = new ObservableCollection<PropertyDefinition>();
            foreach (var item in selectedItems)
            {
                if (item is PropertyDefinition property)
                {
                    newSelection.Add(property);
                }
            }
            SelectedAvailableProperties = newSelection;
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

        public bool CanAddMoreProperties => SelectedProperties.Count < 5 && SelectedAvailableProperties.Any();

        public RelayCommand<object> AddSelectedPropertiesCommand { get; }
        public RelayCommand<string> RemovePropertyCommand { get; }
        public event EventHandler<bool?> CloseRequested;

        public RelayCommand<object> OKCommand { get; }
        public RelayCommand<object> CancelCommand { get; }

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

        private void AddSelectedProperties()
        {
            foreach (var prop in SelectedAvailableProperties.ToList())
            {
                if (SelectedProperties.Count >= 5) break;
                if (!SelectedProperties.Any(p => p.Key == prop.Key))
                {
                    SelectedProperties.Add(prop);
                    //// 设置默认值
                    //if (!_treeItem.Properties.ContainsKey(prop.Key) && prop.DefaultValue != null)
                    //{
                    //    _treeItem.SetProperty(prop.Key, prop.DefaultValue);
                    //}
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

        private void RemoveProperty(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            // 使用循环而不是LINQ，避免并发修改问题
            PropertyDefinition propertyToRemove = null;
            foreach (var prop in SelectedProperties)
            {
                if (prop.Key == key)
                {
                    propertyToRemove = prop;
                    break;
                }
            }

            if (propertyToRemove != null)
            {
                SelectedProperties.Remove(propertyToRemove);
                _treeItem.RemoveProperty(key);

                // 通知UI更新
                OnPropertyChanged(nameof(SelectedProperties));
                OnPropertyChanged(nameof(CanAddMoreProperties));
            }
        }
        private void OK()
        {
            // 更新树节点的选择属性
            //_treeItem.SelectedPropertyKeys = new ObservableCollection<string>(
            //    SelectedProperties.Select(p => p.Key));
            SetItemProperties(SelectedProperties);

            // 请求关闭对话框
            CloseRequested?.Invoke(this, true);
        }
        private void SetItemProperties(ObservableCollection<PropertyDefinition> selectedProperties)
        {
            //_treeItem.SelectedPropertyKeys.Clear();
            //_treeItem.Properties.Clear();
            //for (int i=0;i<selectedProperties?.Count;i++)
            //{
            //    var prop = selectedProperties[i];
            //    _treeItem.SelectedPropertyKeys.Add(prop.Key);
            //    _treeItem.SetProperty(prop.Key, prop.Type == PropertyType.EnumItems ? prop.SelectedItems :
            //                                    prop.Type == PropertyType.EnumItem ? prop.SelectedItem: prop.Value);
            //}
            _treeItem.Properties.Clear();
            for (int i = 0; i < selectedProperties?.Count; i++)
            {
                var prop = selectedProperties[i];
                //_treeItem.SelectedPropertyKeys.Add(prop.Key);
                _treeItem.SetProperty(prop.Key, prop.Type == PropertyType.EnumItems ? prop.SelectedItems :
                                                prop.Type == PropertyType.EnumItem ? prop.SelectedItem : prop.Value);
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