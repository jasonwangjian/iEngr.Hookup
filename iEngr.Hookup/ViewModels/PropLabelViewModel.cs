using iEngr.Hookup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace iEngr.Hookup.ViewModels
{
    public class PropLabelViewModel : INotifyPropertyChanged
    {
        private  ObservableCollection<LabelDisplay> _labelItems= new ObservableCollection<LabelDisplay>();
        public ObservableCollection<LabelDisplay> LabelItems
        {
            get => _labelItems;
            set => SetField(ref _labelItems, value);
        }
        private LabelDisplay _selectedItem;
        public LabelDisplay SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetField(ref _selectedItem, value);
            }
        }
        private bool _isCompared;
        public bool IsCompared
        {
            get => _isCompared;
            set
            {
                if (SetField(ref _isCompared, value))
                {
                    OnPropertyChanged(nameof(IsNoCompared));
                }
            }
        }
        public bool IsNoCompared
        {
            get
            {
                return !IsCompared;
            }
        }
        public void Clear(string cat)
        {
            if (LabelItems == null) return;
            if (cat.ToLower() == "node")
            {
                foreach (var item in LabelItems)
                {
                    item.DisplayValue1 = null;
                    item.IsNodeLabel = false;
                }
            }
            else if (cat.ToLower() == "diagram")
            {
                foreach (var item in LabelItems)
                {
                    item.DisplayValue2 = null;
                    item.IsComosLabel = false;  
                }
            }
        }

        public void Update(string cat, Dictionary<string,object> properties)
        {
            if (cat.ToLower() == "node")
            {
                foreach (var item in LabelItems)
                {
                    if (properties.ContainsKey(item.Key))
                    {
                        item.DisplayValue1 = null;
                    }
                }
            }
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
    public class LabelDisplay
    {
        public string Key { get; set; }
        public int SortNum { get; set; }
        public string DisplayName { get; set; }
        public string DisplayValue1 { get; set; } //节点上的标签
        public string DisplayValue2 { get; set; } //模板上的标签
        public bool IsInherit {  get; set; }
        public bool IsDiff
        {
            get
            {
                return DisplayValue1 != DisplayValue2;
            }
        }
        public bool IsComosLabel { get; set; }
        public bool IsNodeLabel { get; set; }
    }
}
