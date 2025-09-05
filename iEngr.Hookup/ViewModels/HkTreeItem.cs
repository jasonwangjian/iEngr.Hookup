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
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class HkTreeItem : INotifyPropertyChanged
    {
        public HkTreeItem()
        {
            Children = new ObservableCollection<HkTreeItem>();
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetField(ref _isExpanded, value);
        }
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
                    }
                    OnPropertyChanged();
                }
            }
        }
        private string _operationStatus;
        public string OperationStatus
        {
            get => _operationStatus;
            set
            {
                if (SetField(ref _operationStatus, value))
                {
                    OnPropertyChanged(nameof(IsCutOperation));
                    OnPropertyChanged(nameof(IsCopyOperation));
                }
            }
        }
        public bool IsCutOperation
        {
            get
            {
                return OperationStatus?.ToLower() == "cut";
            }
        }
        public bool IsCopyOperation
        {
            get
            {
                return OperationStatus?.ToLower() == "copy";
            }
        }
        public bool IsDuplicatedName
        {
            get
            {
                return NodeItem != null && NodeItem.IsPropNode == true && Siblings.Any(x=>x.Name == Name) ;
            }
            set => OnPropertyChanged(nameof(IsDuplicatedName));
        }
        public string ID { get; set; }
        public HKLibTreeNode NodeItem { get; set; }
        private string _diagID;
        public string DiagID
        {
            get => _diagID;
            set => SetField(ref _diagID, value);
        }
        public string InheritDiagID
        {
            get
            {
                if (Parent?.NodeItem?.IsPropNode == true)
                    return Parent.DiagID ?? Parent.InheritDiagID;
                return null;
            }
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
            set { if(SetField(ref _nodeName, value) && value != null)
                {
                    NodeItem = HK_General.dicTreeNode.TryGetValue(value, out HKLibTreeNode nodeItem) ? nodeItem : new HKLibTreeNode();
                }
            }

        }
        private string _nodeValue;
        public string NodeValue
        {
            get => _nodeValue;
            set => SetField(ref _nodeValue, value);
        }

        private string _picturePath;
        public string PicturePath
        {
            get => _picturePath;
            set
            {
                if (SetField(ref _picturePath, value))
                    OnPropertyChanged(nameof(IsInheritPictureActive));
            }
        }
        public string InheritPicturePath
        {
            get
            {
                if (Parent?.NodeItem?.IsPropNode == true)
                    return Parent.PicturePath ?? Parent.InheritPicturePath;
                return null;

            }
        }
        public bool IsInheritPictureActive
        {
            get
            {
                return PicturePath == null && InheritPicturePath != null;
            }
        }

        private string _editNodeName;
        public string EditNodeName
        {
            get => _editNodeName;
            set => SetField(ref _editNodeName, value);
        }

        private string _editName;
        public string EditName
        {
            get => _editName;
            set
            {
                if (SetField(ref _editName, value))
                {
                    if (NodeName == "SpecNode")
                    {
                        if (string.IsNullOrEmpty(value)) ErrMsgName = "不能为空";
                        else if (Siblings.Any(x => x.Name == value)) ErrMsgName = $"重名：{value}";
                        else ErrMsgName = string.Empty;
                        return;
                    }
                }
            }
        }
        private string _editNodeValue;
        public string EditNodeValue
        {
            get => _editNodeValue;
            set
            {
                if (_editNodeValue != value)
                {
                    _editNodeValue = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _errMsgName;
        public string ErrMsgName
        {
            get => _errMsgName;
            set => SetField(ref _errMsgName, value);
        }

        // 添加一个用于显示的属性字符串
        //public string _displayKCProperties;
        public string DisplayProperties
        {

            //get => _displayProperties;
            //set => SetField(ref _displayProperties, value);
            get
            {
                if (Properties == null || Properties.Count == 0)
                    return string.Empty;

                var displayText = new StringBuilder();
                foreach (var prop in Properties)
                {
                    var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
                    if (propDef != null)
                    {
                        string displayValue = prop.Value?.ToString();
                        if (prop.Value is ObservableCollection<GeneralItem> items)
                        {
                            displayValue = string.Join(", ", items.Select(x => x.Name).ToList());

                        }
                        else if (prop.Value is GeneralItem item)
                        {
                            displayValue = item.Name;
                            //displayValue = propDef.Items.FirstOrDefault(x => x.Code == prop.Value?.ToString())?.Name;
                        }
                        displayText.Append($"{propDef.DisplayName}:{displayValue}; ");
                    }
                }
                return displayText.ToString().Trim();
            }
            set => OnPropertyChanged();
        }
        //public void RefreshDisplayProperties()
        //{
        //    if (Properties == null || Properties.Count == 0)
        //        DisplayProperties = string.Empty;

        //    var displayText = new StringBuilder();
        //    foreach (var prop in Properties)
        //    {
        //        var propDef = PropertyLibrary.GetPropertyDefinition(prop.Key);
        //        if (propDef != null)
        //        {
        //            string displayValue = prop.Value?.ToString();
        //            if (prop.Value is ObservableCollection<GeneralItem> items)
        //            {
        //                displayValue = string.Join(", ", items.Select(x => x.Name).ToList());

        //            }
        //            else if (prop.Value is GeneralItem item)
        //            {
        //                displayValue =  item.Name;
        //                //displayValue = propDef.Items.FirstOrDefault(x => x.Code == prop.Value?.ToString())?.Name;
        //            }
        //            displayText.Append($"{propDef.DisplayName}:{displayValue}; ");
        //        }
        //    }
        //    DisplayProperties = displayText.ToString().Trim();
        //}
        // 动态属性字典
        private Dictionary<string, object> _properties = new Dictionary<string, object>();
        public Dictionary<string, object> Properties
        {
            get => _properties;
            set
            {
                if (SetField(ref _properties, value))
                    OnPropertyChanged(DisplayProperties); 
            }
        }

        // 用户选择的属性键（最多5个）
        //private ObservableCollection<string> _selectedPropertyKeys = new ObservableCollection<string>();
        //public ObservableCollection<string> SelectedPropertyKeys
        //{
        //    get => _selectedPropertyKeys;
        //    set
        //    {
        //        _selectedPropertyKeys = value;
        //        OnPropertyChanged();
        //    }
        //}

        public ObservableCollection<HkTreeItem> Children { get; set; }
        public HkTreeItem Parent { get; set; }
        public List<HkTreeItem> Siblings 
        {
            get
            {
                return Parent.Children.Where(x=> x != this).ToList();
            }
        }
        public ObservableCollection<GeneralItem> NodeItems //可用于选择的节点
        {
            get
            {
                //List<GeneralItem> list = HK_General.dicParentTreeNode
                //    .Where(x => x.Key == Parent?.NodeName)
                //    .Select(x => x.Value).FirstOrDefault()?
                //    .Select(x => new GeneralItem
                //    {
                //        Code = x.ID,
                //        NameCn = x.NameCn,
                //        NameEn = x.NameEn
                //    })?
                //    .Where(x => !Siblings.Select(s => s.NodeName).Contains(x.Code)).ToList();
                List<GeneralItem> list = HK_General.dicTreeNode
                    .Select(x=>x.Value)
                    .Where(x => x.Parent == Parent?.NodeName)
                    .Select(x => new GeneralItem
                    {
                        Code = x.ID,
                        NameCn = x.NameCn,
                        NameEn = x.NameEn
                    })?
                    .Where(x => !Siblings.Select(s => s.NodeName).Contains(x.Code)).ToList(); if (list == null) return null;
                return new ObservableCollection<GeneralItem>(list);

            }
        }

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

        //// 检查是否已选择某个属性
        //public bool HasProperty(string key)
        //{
        //    //return _selectedPropertyKeys.Contains(key);
        //    return Properties.ContainsKey(key);
        //}

        // 添加属性
        //public bool AddProperty(string key)
        //{
        //    if (_selectedPropertyKeys.Count >= 5)
        //        return false;

        //    if (!_selectedPropertyKeys.Contains(key))
        //    {
        //        _selectedPropertyKeys.Add(key);
        //        OnPropertyChanged(nameof(SelectedPropertyKeys));
        //        return true;
        //    }
        //    return false;
        //}

        // 移除属性
        public void RemoveProperty(string key)
        {
            //if (_selectedPropertyKeys.Contains(key))
            //{
            //    _selectedPropertyKeys.Remove(key);
            //    if (_properties.ContainsKey(key))
            //    {
            //        _properties.Remove(key);
            //    }
            //    OnPropertyChanged(nameof(SelectedPropertyKeys));
            //    OnPropertyChanged(nameof(Properties));
            //    OnPropertyChanged(nameof(DisplayProperties)); // 通知显示更新
            //}
            if (_properties.ContainsKey(key))
            {
                _properties.Remove(key);
            }
            OnPropertyChanged(nameof(Properties));
            OnPropertyChanged(nameof(DisplayProperties)); // 通知显示更新
        }

        public HkTreeItem Clone()
        {
            var clone = new HkTreeItem
            {
                IsExpanded = IsExpanded,
                NodeName = NodeName,
                NodeValue = NodeValue,
                Name = Name,
                PicturePath = PicturePath,
                DiagID = DiagID,
                Properties = new Dictionary<string, object>(Properties)
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
                return string.IsNullOrWhiteSpace(EditName) && NodeName == "SpecNode" ||
                       !string.IsNullOrEmpty(ErrMsgName);
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
            NodeValue = EditNodeValue;
            IsEditing = false;
            return true;
        }

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