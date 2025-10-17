using iEngr.Hookup.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.Primitives;

namespace iEngr.Hookup.ViewModels
{
    public class ProjDiagMgrViewModel : INotifyPropertyChanged
    {
        public event EventHandler<bool> LangInChineseChanged;
        public event EventHandler<bool> LangInEnglishChanged;
        public event EventHandler<bool> ComparisonEnabledChanged;
        public event EventHandler<bool> ComparisonByIdChanged;

        public RelayCommand<object> LibDiagMgrCommand { get; set; }
        public ProjDiagMgrViewModel()
        {
            LibDiagMgrCommand = new RelayCommand<object>(LibDiagMgr, CanLibDiagMgr);
        }
        private bool CanLibDiagMgr(object parameter)
        {
            return true;
        }
        private void LibDiagMgr(object parameter)
        {
            var dialog = new GenLibDiagMgrDialog();
            dialog.ShowDialog();
        }



        private bool _langInChinese;
        public bool LangInChinese
        {
            get => _langInChinese;
            set
            {
                SetField(ref _langInChinese, value);
                LangInChineseChanged?.Invoke(this, value);
            }
        }
        private bool _langInEnglish;
        public bool LangInEnglish
        {
            get => _langInEnglish;
            set 
            { 
                SetField(ref _langInEnglish, value);
                LangInEnglishChanged?.Invoke(this, value);
            }
        }
        private bool _isComparisonEnabled;
        public bool IsComparisonEnabled
        {
            get => _isComparisonEnabled;
            set
            {
                SetField(ref _isComparisonEnabled, value);
                ComparisonEnabledChanged?.Invoke(this, value);
            }
        }
        private bool _isComparisonById;
        public bool IsComparisonById
        {
            get => _isComparisonById;
            set
            {
                SetField(ref _isComparisonById, value);
                ComparisonByIdChanged?.Invoke(this, value);
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
