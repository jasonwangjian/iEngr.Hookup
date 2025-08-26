using iEngr.Hookup.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.ViewModels
{
    public class BomMgrViewModel : INotifyPropertyChanged
    {
        public BomListViewModel VmBomList { get; set; }
        private MatListViewModel _vmMatList;
        public MatListViewModel VmMatList
        {
            get => _vmMatList;
            set
            {
                if (_vmMatList == value) return;
                // 取消旧订阅
                if (_vmMatList != null)
                {
                    _vmMatList.PropertyChanged -= VmMatListPropertyChanged;
                }
                _vmMatList = value;
                // 建立新订阅
                if (_vmMatList != null)
                {
                    _vmMatList.PropertyChanged += VmMatListPropertyChanged;
                }
            }
        }
        private void VmMatListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MatListViewModel.SelectedItem))
            {
                if (sender is MatListViewModel vm)
                {
                    VmBomList.SelectedMatListItem = vm.SelectedItem;
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
}
