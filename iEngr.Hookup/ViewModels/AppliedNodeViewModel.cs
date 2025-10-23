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
    public class AppliedNodeViewModel : INotifyPropertyChanged
    {
        public event EventHandler<AppliedNodeItem> NodeIDHighlighted;

        private ObservableCollection<AppliedNodeItem> _appliedItems = new ObservableCollection<AppliedNodeItem>();
        public ObservableCollection<AppliedNodeItem> AppliedItems
        {
            get => _appliedItems;
            set => SetField(ref _appliedItems, value);
        }
        private AppliedNodeItem _selectedItem;
        public AppliedNodeItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetField(ref _selectedItem, value) && value != null)
                {
                    NodeIDHighlighted?.Invoke(this, value);
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
    public class AppliedNodeItem : INotifyPropertyChanged
    {
        private string _nodeID;
        public string NodeID
        {
            get => _nodeID;
            set => SetField(ref _nodeID, value);
        }
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => SetField(ref _displayName, value);
        }
        private bool _isInherit;
        public bool IsInherit
        {
            get => _isInherit;
            set => SetField(ref _isInherit, value);
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