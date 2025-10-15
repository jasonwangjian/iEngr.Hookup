using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.ViewModels
{
    public class ProjDiagMgrViewModel : INotifyPropertyChanged
    {
        public event EventHandler<bool> LangInChineseChanged;
        public event EventHandler<bool> LangInEnglishChanged;
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
