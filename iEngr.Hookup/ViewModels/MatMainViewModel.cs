using iEngr.Hookup.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class MatMainViewModel : INotifyPropertyChanged
    {
        private MatDataViewModel _vmMatData;
        public MatDataViewModel VmMatData
        {
            get => _vmMatData;
            set
            {
                if (_vmMatData == value) return;
                // 取消旧订阅
                if (_vmMatData != null)
                {
                    _vmMatData.PropertyChanged -= VmMatDatePropertyChanged;
                }
                _vmMatData = value;
                // 建立新订阅
                if (_vmMatData != null)
                {
                    _vmMatData.PropertyChanged += VmMatDatePropertyChanged;
                    //_vmMatDate.DataChanged += OnChildDataChanged;
                }
            }
        }
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
        // 添加自定义事件处理
        private void OnChildDataChanged(object sender, string newData)
        {
            Application.Current.Dispatcher.Invoke(() => {
                //MatDataToQuery = newData;
                //Debug.WriteLine($"通过自定义事件收到数据: {newData}");
            });
        }

        private void VmMatDatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MatDataViewModel.MatDataToQuery))
            {
                //Application.Current.Dispatcher.Invoke(() => {
                if (sender is MatDataViewModel vm)
                {
                    VmMatList.MatDataToQuery = vm.MatDataToQuery;
                    //Debug.WriteLine($"通过PropertyChanged收到数据: {vm.MatDataString}");
                }
                //});
            }
        }
        private void VmMatListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MatListViewModel.MatDataFromQuery))
            {
                if (sender is MatListViewModel vm)
                {
                    VmMatData.MatDataFromQuery = vm.MatDataFromQuery;
                    //Debug.WriteLine($"通过PropertyChanged收到数据: {vm.MatDataString}");
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
