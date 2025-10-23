using Plt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class AppliedComosViewModel : INotifyPropertyChanged
    {
        public event EventHandler<AppliedComosItem> ComosItemSelected;

        private ObservableCollection<AppliedComosItem> _appliedItems = new ObservableCollection<AppliedComosItem>();
        public ObservableCollection<AppliedComosItem> AppliedItems
        {
            get => _appliedItems;
            set => SetField(ref _appliedItems, value);
        }
        private AppliedComosItem _selectedItem;
        public AppliedComosItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetField(ref _selectedItem, value) && value != null)
                {

                    ComosItemSelected?.Invoke(this, value);
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
    public class AppliedComosItem : INotifyPropertyChanged
    {
        public IComosBaseObject ComosObj { get; set; }
        private string _comosUID;
        public string ComosUID
        {
            get => _comosUID;
            set => SetField(ref _comosUID, value);
        }
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => SetField(ref _displayName, value);
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