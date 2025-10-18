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
        private  ObservableCollection<LabelDisplay> _propLabelItems= new ObservableCollection<LabelDisplay>();
        public ObservableCollection<LabelDisplay> PropLabelItems
        {
            get => _propLabelItems;
            set => SetField(ref _propLabelItems, value);
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
        public string DisplayName { get; set; }
        public string DisplayValue1 { get; set; }
        public string DisplayValue2 { get; set; }
        public bool IsInherit {  get; set; }
        public bool IsDiff
        {
            get
            {
                return DisplayValue1 == DisplayValue2;
            }
        }
    }
}
