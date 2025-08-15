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
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class MatListViewModel : INotifyPropertyChanged
    {
        public MatListViewModel()
        {
            HK_General = new HK_General();
            MatList = new ObservableCollection<HKMatGenLib>();
            MouseDoubleClickCommand = new RelayCommand<KeyEventArgs>(HandleMouseDoubleClick);
        }
        private HK_General HK_General;

        private ObservableCollection<HKMatGenLib> _matList;
        private HKMatGenLib _selectedMat;
        private ObservableCollection<HKMatGenLib> _matItemsSelected;

        public ObservableCollection<HKMatGenLib> MatList
        {
            get => _matList;
            set => SetField(ref _matList, value);
        }
        public HKMatGenLib SelectedMat
        {
            get => _selectedMat;
            set => SetField(ref _selectedMat, value);
        }

        private string _queryString;
        public string QueryString
        {
            get => _queryString;
            set => SetField(ref _queryString, value);
        }
        public ObservableCollection<HKMatGenLib> MatItemsSelected
        {
            get => _matItemsSelected;
            set => SetField(ref _matItemsSelected, value);
        }
        private string _matDataString;
        public string MatDataString
        {
            get => _matDataString;
            set => SetField(ref _matDataString, value);
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
        public ICommand MouseDoubleClickCommand { get; }
        private void HandleMouseDoubleClick(KeyEventArgs e)
        {
        }
    }
}
