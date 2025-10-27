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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class AppliedComosViewModel : INotifyPropertyChanged
    {
        public event EventHandler<AppliedComosItem> ComosItemSelected;
        public event EventHandler<AppliedComosItem> ComosDiagAppDelCmd;
        public event EventHandler<AppliedComosItem> ComosItemContextMenu;
        public ICommand RemoveCommand { get; }
        public ICommand ItemMouseEnterCommand { get; }
        public ICommand ItemMouseLeaveCommand { get; }
        public ICommand ItemMouseClickCommand { get; }
        private CancellationTokenSource _currentTokenSource;
        private object _currentHoveredItem;
        public AppliedComosViewModel() 
        {
            RemoveCommand = new RelayCommand<AppliedComosItem>(DeleteDiagObj, _=>true);
            ItemMouseEnterCommand = new RelayCommand<object>(OnItemMouseEnter);
            ItemMouseLeaveCommand = new RelayCommand<object>(OnItemMouseLeave);
            ItemMouseClickCommand = new RelayCommand<object>(OnItemMouseClick);
        }
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

        private void DeleteDiagObj(AppliedComosItem item)
        {
            ComosDiagAppDelCmd?.Invoke(this, item);
        }

        private async void OnItemMouseEnter(object item)
        {
            // 如果已经在悬停其他项，先取消
            if (_currentHoveredItem != null && _currentHoveredItem != item)
            {
                _currentTokenSource?.Cancel();
            }

            _currentHoveredItem = item;
            _currentTokenSource = new CancellationTokenSource();

            try
            {
                // 等待1秒
                await Task.Delay(500, _currentTokenSource.Token);

                // 如果计时完成且仍然是当前悬停的项
                if (!_currentTokenSource.Token.IsCancellationRequested && _currentHoveredItem == item)
                {
                    ComosItemContextMenu?.Invoke(this, item as AppliedComosItem);
                }
            }
            catch (TaskCanceledException)
            {
                // 计时被取消是正常情况
            }
        }
        private void OnItemMouseLeave(object item)
        {
            // 只有当离开的是当前悬停项时才取消
            if (_currentHoveredItem == item)
            {
                CancelCurrentHover();
            }
        }
        private void OnItemMouseClick(object item)
        {
            // 任何鼠标点击都取消当前的悬停计时
            CancelCurrentHover();
            ComosItemContextMenu?.Invoke(this, item as AppliedComosItem);
        }
        private void CancelCurrentHover()
        {
            _currentTokenSource?.Cancel();
            _currentTokenSource = null;
            _currentHoveredItem = null;
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
        public IComosBaseObject ComosObj { get; set; } //仪控设备
        private string _comosUID;
        public string ComosUID //DiagObj的SystemUID
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
        private string _assignMode;
        public string AssignMode
        {
            get => _assignMode;
            set => SetField(ref _assignMode, value);
        }
        private string _isLocked;
        public string IsLocked
        {
            get => _isLocked;
            set => SetField(ref _isLocked, value);
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