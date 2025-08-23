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
using iEngr.Hookup.Models;

namespace iEngr.Hookup.ViewModels
{
    public class MatListItem : INotifyPropertyChanged
    {
        public void Update(MatListItem newData)
        {
            //if (ID == newData.ID)
            //{
            //    CatID = newData.CatID;
            //    NameCn = newData.NameCn;
            //    NameEn = newData.NameEn;
            //    TechSpecMain = newData.TechSpecMain;
            //    TechSpecAux = newData.TechSpecAux;
            //    TypeP1 = newData.TypeP1;
            //    TypeP2 = newData.TypeP2;
            //    SizeP1 = newData.SizeP1;
            //    SizeP2 = newData.SizeP2;
            //    MatMatID = newData.MatMatID;
            //    PClass = newData.PClass;
            //    MoreSpecCn = newData.MoreSpecCn;
            //    MoreSpecEn = newData.MoreSpecEn;
            //    RemarksCn = newData.RemarksCn;
            //    RemarksEn = newData.RemarksEn;
            //    SpecCombMainCn = newData.SpecCombMainCn;
            //    SpecCombAuxCn = newData.SpecCombAuxCn;
            //    SpecCombPort = newData.SpecCombPort;
            //    SpecMat = newData.SpecMat;
            //    SpecPClass = newData.SpecPClass;
            //}
        }
        private string _nameCn;
        public string NameCn
        {
            get => _nameCn;
            set => SetField(ref _nameCn, value);
        }
        private string _nameEn;
        public string NameEn
        {
            get => _nameEn;
            set => SetField(ref _nameEn, value);
        }
        private string _specCombMainCn;
        public string SpecCombMainCn
        {
            get => _specCombMainCn;
            set => SetField(ref _specCombMainCn, value);
        }
        private string _specCombAuxCn;
        public string SpecCombAuxCn
        {
            get => _specCombAuxCn;
            set => SetField(ref _specCombAuxCn, value);
        }
        private string _specMatMatCn;
        public string SpecMatMatCn
        {
            get => _specMatMatCn;
            set => SetField(ref _specMatMatCn, value);
        }
        private string _specMatMatEn;
        public string SpecMatMatEn
        {
            get => _specMatMatEn;
            set => SetField(ref _specMatMatEn, value);
        }
        private string _specCombPortCn;
        public string SpecCombPortCn
        {
            get => _specCombPortCn;
            set => SetField(ref _specCombPortCn, value);
        }
        private string _specCombPortEn;
        public string SpecCombPortEn
        {
            get => _specCombPortEn;
            set => SetField(ref _specCombPortEn, value);
        }
        private string _specPClass;
        public string SpecPClass
        {
            get => _specPClass;
            set => SetField(ref _specPClass, value);
        }
        private string _MoreSpecCn;
        public string MoreSpecCn
        {
            get => _MoreSpecCn;
            set => SetField(ref _MoreSpecCn, value);
        }
        private string _MoreSpecEn;
        public string MoreSpecEn
        {
            get => _MoreSpecEn;
            set => SetField(ref _MoreSpecEn, value);
        }
        private string _remarksCn;
        public string RemarksCn
        {
            get => _remarksCn;
            set => SetField(ref _remarksCn, value);
        }
        private string _remarksEn;
        public string RemarksEn
        {
            get => _remarksEn;
            set => SetField(ref _remarksEn, value);
        }
        public Models.HKMatGenLib MatLibItem { get; set; }
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
