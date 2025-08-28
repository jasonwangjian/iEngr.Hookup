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
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetField(ref _isEditing, value);
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
            set
            {
                _population = value;
                OnPropertyChanged();
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

    }
}