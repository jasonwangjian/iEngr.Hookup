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
        private MatDataViewModel _vmMatDate;
        public MatDataViewModel VmMatDate
        {
            get => _vmMatDate;
            set
            {
                if (_vmMatDate == value) return;

                // 取消旧订阅
                if (_vmMatDate != null)
                {
                    _vmMatDate.PropertyChanged -= VmMatDatePropertyChanged;
                }

                _vmMatDate = value;
                //OnPropertyChanged();

                // 建立新订阅
                if (_vmMatDate != null)
                {
                    _vmMatDate.PropertyChanged += VmMatDatePropertyChanged;
                    _vmMatDate.DataChanged += OnChildDataChanged;

                    // 立即同步当前值
                    MatDataString = _vmMatDate.MatDataString;
                }
            }
        }
        // 添加自定义事件处理
        private void OnChildDataChanged(object sender, string newData)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MatDataString = newData;
                Debug.WriteLine($"通过自定义事件收到数据: {newData}");
            });
        }

        private void VmMatDatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MatDataViewModel.MatDataString))
            {
                Application.Current.Dispatcher.Invoke(() => {
                    if (sender is MatDataViewModel vm)
                    {
                        MatDataString = vm.MatDataString;
                        Debug.WriteLine($"通过PropertyChanged收到数据: {vm.MatDataString}");
                    }
                });
            }
        }
        private string _matDataString;
        public string MatDataString
        {
            get => _matDataString;
            set
            {
                if (SetField(ref _matDataString, value))
                {
                    // 这里可以添加处理接收到的数据的逻辑
                    Debug.WriteLine($"收到新数据: {value}");
                }
            }
        }

        // 修改构造函数确保初始化
        public MatMainViewModel()
        {
 
        }

        // 其余代码不变...

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
