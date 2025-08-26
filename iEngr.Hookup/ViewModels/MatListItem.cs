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
            NameCn = newData.NameCn;
            NameEn = newData.NameEn;
            SpecMainCn = newData.SpecMainCn;
            SpecMainEn = newData.SpecMainEn;
            SpecPortCn = newData.SpecPortCn;
            SpecPortEn = newData.SpecPortEn;
            SpecAuxCn = newData.SpecAuxCn;
            SpecAuxEn = newData.SpecAuxCn;
            SpecMoreCn = newData.SpecMoreCn;
            SpecMoreEn = newData.SpecMoreEn;
            MatMatCode = newData.MatMatCode;
            MatMatCn = newData.MatMatCn;
            MatMatEn = newData.MatMatEn;
            SpecAllCn = newData.SpecAllCn; //触发OnPropertyChanged
            SpecAllEn = newData.SpecAllEn;//触发OnPropertyChanged
            RemarksCn = newData.RemarksCn;
            RemarksEn = newData.RemarksEn;
            SpecPClass = newData.SpecPClass;
            MatLibItem = newData.MatLibItem;
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
        public string SpecMainCn
        {
            get => _specCombMainCn;
            set => SetField(ref _specCombMainCn, value);
        }
        private string _specCombMainEn;
        public string SpecMainEn
        {
            get => _specCombMainEn;
            set => SetField(ref _specCombMainEn, value);
        }
        private string _specCombAuxCn;
        public string SpecAuxCn
        {
            get => _specCombAuxCn;
            set => SetField(ref _specCombAuxCn, value);
        }
        private string _specCombAuxEn;
        public string SpecAuxEn
        {
            get => _specCombAuxEn;
            set => SetField(ref _specCombAuxEn, value);
        }
        private string _matMatCode;
        public string MatMatCode
        {
            get => _matMatCode;
            set => SetField(ref _matMatCode, value);
        }
        private string _specMatMatCn;
        public string MatMatCn
        {
            get => _specMatMatCn;
            set => SetField(ref _specMatMatCn, value);
        }
        private string _specMatMatEn;
        public string MatMatEn
        {
            get => _specMatMatEn;
            set => SetField(ref _specMatMatEn, value);
        }
        private string _specCombPortCn;
        public string SpecPortCn
        {
            get => _specCombPortCn;
            set => SetField(ref _specCombPortCn, value);
        }
        private string _specCombPortEn;
        public string SpecPortEn
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
        public string SpecMoreCn
        {
            get => _MoreSpecCn;
            set => SetField(ref _MoreSpecCn, value);
        }
        private string _MoreSpecEn;
        public string SpecMoreEn
        {
            get => _MoreSpecEn;
            set => SetField(ref _MoreSpecEn, value);
        }
        public string SpecAllCn
        {
            get => string.Join("; ", new List<string> { SpecMainCn, SpecPortCn, SpecAuxCn, SpecMoreCn}
                                                 .Where(item => !string.IsNullOrWhiteSpace(item))
                                                 .Select(item => item.Trim())
                                                 .ToList());
            set => OnPropertyChanged();
        }
        public string SpecAllEn
        {
            get => string.Join("; ", new List<string> { SpecMainEn, SpecPortEn, SpecAuxEn, SpecMoreEn }
                                                 .Where(item => !string.IsNullOrWhiteSpace(item))
                                                 .Select(item => item.Trim())
                                                 .ToList());
            set => OnPropertyChanged();
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
        private string _no;
        public string No
        {
            get => _no;
            set => SetField(ref _no, value);
        }
        private string _id;
        public string ID
        {
            get => _id;
            set=>SetField(ref _id, value);
        }
        private string _qty;
        public string Qty
        {
            get => _qty;
            set=>SetField(ref _qty, value);
        }
        private string _unit;
        public string Unit
        {
            get => _unit;
            set=>SetField(ref _unit, value);
        }
        private string _supplyDiscipline;
        public string SupplyDiscipline
        {
            get => _supplyDiscipline;
            set => SetField(ref _supplyDiscipline, value);
        }
        private string _supplyResponsible;
        public string SupplyResponsible
        {
            get => _supplyResponsible;
            set=>SetField(ref _supplyResponsible, value);
        }
        private string _erectionDiscipline;
        public string ErectionDiscipline
        {
            get => _erectionDiscipline;
            set=>SetField(ref _erectionDiscipline, value);
        }
        private string _erectionResponsible;
        public string ErectionResponsible
        {
            get => _erectionResponsible;
            set=>SetField(ref _erectionResponsible, value);
        }
        public string AlterCode { get;set; }
        public HKMatGenLib MatLibItem { get; set; }
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
