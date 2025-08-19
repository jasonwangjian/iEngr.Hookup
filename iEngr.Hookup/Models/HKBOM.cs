using Plt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace iEngr.Hookup.Models
{
    public class HKBOM : HKMatGenLib
    {
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
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
