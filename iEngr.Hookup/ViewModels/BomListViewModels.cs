using Comos.Controls;
using Comos.UIF;
using ComosQueryInterface;
using ComosQueryXObj;
using iEngr.Hookup.Models;
using Plt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class BomListViewModels: INotifyPropertyChanged
    {
        public BomListViewModels()
        {
            HKBOMs = new ObservableCollection<HKBOM>();
        }
        IComosBaseObject _objMat;
        private string _matID;
        public string MatID
        {
            get => _matID;
            set => SetField(ref _matID, value);
        }
        public IComosBaseObject ObjMat
        {
            get => _objMat;
            set => SetField(ref _objMat, value);
        }
        ObservableCollection<HKBOM> _hKBOMs;
        public ObservableCollection<HKBOM> HKBOMs
        {
            get => _hKBOMs;
            set => SetField(ref _hKBOMs, value);
        }
        private HKBOM _selectedItem;
        public HKBOM SelectedItem
        {
            get => _selectedItem;
            set => SetField(ref _selectedItem, value);
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
