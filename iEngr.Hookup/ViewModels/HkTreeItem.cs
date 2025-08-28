using iEngr.Hookup.Models;
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
    public class HkTreeItem : BasicNotifyPropertyChanged
    {
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
                        EditCountry = Country;
                        EditPopulation = Population;
                        EditDescription = Description;
                    }
                    OnPropertyChanged();
                }
            }
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        private string _functionCode;
        public string FunctionCode
        {
            get => _functionCode;
            set => SetField(ref _functionCode, value);
        }
        private string _device;
        public string Device
        {
            get => _device;
            set => SetField(ref _device, value);
        }
        private string _country;
        public string Country
        {
            get => _country;
            set
            {
                _country = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasValidationErrors)); // 添加这一行
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
                OnPropertyChanged(nameof(HasValidationErrors)); // 添加这一行
            }
        }

        private int _population;
        public int Population
        {
            get => _population;
            set
            {
                _population = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasValidationErrors)); // 添加这一行
            }
        }

        // 编辑时的临时属性
        private string _editName;
        public string EditName
        {
            get => _editName;
            set
            {
                if (_editName != value)
                {
                    _editName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasValidationErrors)); // 添加这一行
                }
            }
        }

        private string _editCountry;
        public string EditCountry
        {
            get => _editCountry;
            set
            {
                if (_editCountry != value)
                {
                    _editCountry = value;
                    OnPropertyChanged();
                }
            }
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

        public ObservableCollection<HkTreeItem> Children { get; set; }
        public HkTreeItem Parent { get; set; }

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
        public HkTreeItem()
        {
            Children = new ObservableCollection<HkTreeItem>();
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
            Country = EditCountry;
            Population = EditPopulation;
            Description = EditDescription;
            IsEditing = false;
            return true;
        }
        // 取消编辑方法
        public void CancelEdit()
        {
            // 恢复原始值
            EditName = Name;
            EditCountry = Country;
            EditPopulation = Population;
            EditDescription = Description;
            IsEditing = false;
        }
    }
}