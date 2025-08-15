using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class MatMainViewModel : INotifyPropertyChanged
    {
        private string _matDataString;
        public string MatDataString
        {
            get => _matDataString;
            set => SetField(ref _matDataString, value);
        }
        public MatMainViewModel()
        {
            VmMatDate.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MatDataViewModel.MatDataString))
                    MatDataString = VmMatDate.MatDataString;
            };
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MatDataString))
                    VmMatList.MatDataString = MatDataString;
            };
        }
        private HK_General HK_General;

        public MatDataViewModel VmMatDate { get; } = new MatDataViewModel();
        public MatListViewModel VmMatList { get; } = new MatListViewModel();

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
        public ICommand QueryCommand { get; }
        public ICommand TestCommand { get; }
        private void matQuery()
        {

        }
        private void test()
        {
        }
    }
}
