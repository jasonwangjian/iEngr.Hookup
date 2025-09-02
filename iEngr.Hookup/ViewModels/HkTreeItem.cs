using iEngr.Hookup.Models;
using iEngr.Hookup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.ViewModels
{
    public class HkTreeItem : INotifyPropertyChanged
    {
        public HkTreeItem()
        {
            Children = new ObservableCollection<HkTreeItem>();
            SelectedPropertyKeys = new ObservableCollection<string>();
            //FunctionItems = new ObservableCollection<GeneralItem>(HK_General.dicPortType.Select(x => x.Value).Select(x => new GeneralItem
            //{
            //    Code = x.ID,
            //    NameCn = x.NameCn,
            //    NameEn = x.NameEn

            //}).ToList());
        }
        public ObservableCollection<GeneralItem> NodeItems
        {
            get
            {
                List<GeneralItem> list = HK_General.dicTreeNode
                    .Where(x => x.Key == Parent?.NodeName)
                    .Select(x => x.Value).FirstOrDefault()?
                    .Select(x => new GeneralItem
                    {
                        Code = x.ID,
                        NameCn = x.NameCn,
                        NameEn = x.NameEn
                    })?.ToList();
                if (list == null) return null;
               return new ObservableCollection<GeneralItem>(list);

            }
        }

        private RelayCommand<string> _removePropertyCommand;
        public RelayCommand<string> RemovePropertyCommand
        {
            get
            {
                return _removePropertyCommand ??= new RelayCommand<string>(
                    execute: RemoveProperty,
                    canExecute: key => !string.IsNullOrEmpty(key) && HasProperty(key)
                );
            }
        }
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetField(ref _isExpanded, value);
        }
        // 编辑状态属性
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    if (_isEditing)
                    {
                        // 进入编辑模式时保存原始值
                        EditName = Name;
                        EditNodeName = NodeName;
                        EditPopulation = Population;
                        EditDescription = Description;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private string _id;
        public string ID
        {
            get => _id;
            set => SetField(ref _id, value);
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        private string _nodeName;
        public string NodeName
        {
            get => _nodeName;
            set => SetField(ref _nodeName, value);
        }
        private string _editNodeName;
        public string EditNodeName
        {
            get => _editNodeName;
            set => SetField(ref _editNodeName, value);
        }
        private string _nodeValue;
        public string NodeValue
        {
            get => _nodeValue;
            set => SetField(ref _nodeValue, value);
        }

        private string _functionCode;
        public string FunctionCode
        {
            get => _functionCode;
            set => SetField(ref _functionCode, value);
        }
        private string _editDeviceCode;
        public string EditDeviceCode
        {
            get => _editDeviceCode;
            set => SetField(ref _editDeviceCode, value);
        }
        private string _deviceCode;
        public string DeviceCode
        {
            get => _deviceCode;
            set => SetField(ref _deviceCode, value);
        }
        private string _country;
        public string Country
        {
            get => _country;
            set
            {
                _country = value;
                OnPropertyChanged();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private int _population;
        public int Population
        {
            get => _population;
            set =>SetField(ref _population, value); 
        }

        // 编辑时的临时属性
        private string _editName;
        public string EditName
        {
            get => _editName;
            set=> SetField(ref _editName, value);
        }



        private int _editPopulation;
        public int EditPopulation
        {
            get => _editPopulation;
            set
            {
                if (_editPopulation != value)
                {
                    _editPopulation = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _editDescription;
        public string EditDescription
        {
            get => _editDescription;
            set
            {
                if (_editDescription != value)
                {
                    _editDescription = value;
                    OnPropertyChanged();
                }
            }
        }
        // 添加一个用于显示的属性字符串
        public string _displayProperties;
        public string DisplayProperties
        {
            //get
            //{
            //    if (SelectedPropertyKeys == null || SelectedPropertyKeys.Count == 0)
            //        return string.Empty;

            //    var displayText = new StringBuilder();
            //    foreach (var prop in Properties)
            //    {
            //            var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
            //            if (propDef != null)
            //            {
            //                displayText.Append($"{propDef.DisplayName}: {prop.Value}");
            //            }
            //    }                
            //    //foreach (var key in SelectedPropertyKeys)
            //    //{
            //    //    if (Properties.ContainsKey(key))
            //    //    {
            //    //        var prop = PropertyLibrary.GetPropertyDefinition(key);
            //    //        if (prop != null)
            //    //        {
            //    //            displayText.Append($"{prop.DisplayName}: {prop.Value}  ");
            //    //        }
            //    //    }
            //    //}
            //    return displayText.ToString().Trim();
            //}
            get => _displayProperties;
            set => SetField(ref _displayProperties, value);
        }
        public void RefreshDisplayProperties()
        {
            if (SelectedPropertyKeys == null || SelectedPropertyKeys.Count == 0)
                DisplayProperties = string.Empty;

            var displayText = new StringBuilder();
            foreach (var prop in Properties)
            {
                var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
                if (propDef != null)
                {
                    displayText.Append($"{propDef.DisplayName}: {(propDef.GeneralItems != null? propDef.GeneralItems.FirstOrDefault(x => x.Code == prop.Value.ToString())?.Name : prop.Value)}");
                }
            }
            DisplayProperties = displayText.ToString().Trim();
        }
        // 动态属性字典
        private Dictionary<string, object> _properties = new Dictionary<string, object>();
        public Dictionary<string, object> Properties
        {
            get => _properties;
            set
            {
                if (SetField(ref _properties, value))
                    RefreshDisplayProperties(); 
            }
        }

        // 用户选择的属性键（最多5个）
        private ObservableCollection<string> _selectedPropertyKeys = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedPropertyKeys
        {
            get => _selectedPropertyKeys;
            set
            {
                _selectedPropertyKeys = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<HkTreeItem> Children { get; set; }
        public HkTreeItem Parent { get; set; }

        // 获取属性值
        public object GetProperty(string key)
        {
            return _properties.ContainsKey(key) ? _properties[key] : null;
        }

        // 设置属性值
        public void SetProperty(string key, object value)
        {
            if (_properties.ContainsKey(key))
            {
                _properties[key] = value;
            }
            else
            {
                _properties.Add(key, value);
            }
            OnPropertyChanged(nameof(Properties));
            OnPropertyChanged(nameof(DisplayProperties)); // 通知显示更新
        }

        // 检查是否已选择某个属性
        public bool HasProperty(string key)
        {
            return _selectedPropertyKeys.Contains(key);
        }

        // 添加属性
        public bool AddProperty(string key)
        {
            if (_selectedPropertyKeys.Count >= 5)
                return false;

            if (!_selectedPropertyKeys.Contains(key))
            {
                _selectedPropertyKeys.Add(key);
                OnPropertyChanged(nameof(SelectedPropertyKeys));
                return true;
            }
            return false;
        }

        // 移除属性
        public void RemoveProperty(string key)
        {
            if (_selectedPropertyKeys.Contains(key))
            {
                _selectedPropertyKeys.Remove(key);
                if (_properties.ContainsKey(key))
                {
                    _properties.Remove(key);
                }
                OnPropertyChanged(nameof(SelectedPropertyKeys));
                OnPropertyChanged(nameof(Properties));
                OnPropertyChanged(nameof(DisplayProperties)); // 通知显示更新
            }
        }

        public HkTreeItem Clone()
        {
            var clone = new HkTreeItem
            {
                Name = this.Name,
                IsExpanded = this.IsExpanded
            };

            foreach (var child in this.Children)
            {
                var childClone = child.Clone();
                childClone.Parent = clone;
                clone.Children.Add(childClone);
            }

            return clone;
        }

        // 在HkTreeItem中添加验证
        public bool HasValidationErrors
        {
            get
            {
                return string.IsNullOrWhiteSpace(EditName) ||
                       EditPopulation < 0;
            }
        }

        // 在ConfirmEdit方法中添加验证
        public bool ConfirmEdit()
        {
            if (HasValidationErrors)
            {
                return false;
            }

            Name = EditName;
            NodeName = EditNodeName;
            Population = EditPopulation;
            Description = EditDescription;
            IsEditing = false;
            return true;
        }
        //// 取消编辑方法
        //public void CancelEdit()
        //{
        //    // 恢复原始值
        //    EditName = Name;
        //    EditFunctionCode = FunctionCode;
        //    EditPopulation = Population;
        //    EditDescription = Description;
        //    IsEditing = false;
        //}
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