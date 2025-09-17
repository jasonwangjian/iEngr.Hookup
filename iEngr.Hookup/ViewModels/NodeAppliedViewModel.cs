using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class NodeAppliedViewModel : INotifyPropertyChanged
    {
        public event EventHandler<NodeItem> NodeIDHighlighted;

        private ObservableCollection<NodeItem> _appliedNodeItems = new ObservableCollection<NodeItem>();
        public ObservableCollection<NodeItem> AppliedNodeItems
        {
            get => _appliedNodeItems;
            set => SetField(ref _appliedNodeItems, value);
        }
        private NodeItem _selectedItem;
        public NodeItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetField(ref _selectedItem, value);
                //if (value != null)
                    NodeIDHighlighted?.Invoke(this, value);
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
    public class NodeItem : INotifyPropertyChanged
    {
        private string _nodeID;
        public string NodeID
        {
            get=> _nodeID;  
            set=>SetField(ref _nodeID, value);
        }
        private string _disPlayName;
        public string DisPlayName
        {
            get => _disPlayName;
            set => SetField(ref _disPlayName, value);
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