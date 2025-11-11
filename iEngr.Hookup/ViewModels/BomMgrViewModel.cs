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
        public event EventHandler<string> DiagramNameCnChanged;
        public event EventHandler<string> DiagramNameEnChanged;
        public event EventHandler<string> DiagramRemarksCnChanged;
        public event EventHandler<string> DiagramRemarksEnChanged;
        public BomItemsViewModel VmBomList { get; set; }
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
        private string _diagramNameCn;
        public string DiagramNameCn
        {
            get => _diagramNameCn;
            set
            {
                if (SetField(ref _diagramNameCn, value))
                {
                    DiagramNameCnChanged?.Invoke(this, value); 
                }
            }
        }
        private string _diagramNameEn;
        public string DiagramNameEn
        {
            get => _diagramNameEn;
            set
            {
                if (SetField(ref _diagramNameEn, value))
                {
                    DiagramNameEnChanged?.Invoke(this, value);
                }
            }
        }
        private string _diagramRemarksCn;
        public string DiagramRemarksCn
        {
            get => _diagramRemarksCn;
            set
            {
                if (SetField(ref _diagramRemarksCn, value))
                {
                    DiagramRemarksCnChanged?.Invoke(this, value);
                }
            }
        }
        private string _diagramRemarksEn;
        public string DiagramRemarksEn
        {
            get => _diagramRemarksEn;
            set
            {
                if (SetField(ref _diagramRemarksEn, value))
                {
                    DiagramRemarksEnChanged?.Invoke(this, value);
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
