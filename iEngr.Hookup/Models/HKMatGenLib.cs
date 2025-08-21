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

namespace iEngr.Hookup
{
    public class HKMatGenLib : INotifyPropertyChanged
    {
        public void Update(HKMatGenLib newData)
        {
            if (ID == newData.ID)
            {
                CatID = newData.CatID;
                SubCatID = newData.SubCatID;
                NameCn = newData.NameCn;
                NameEn = newData.NameEn;
                TechSpecMain = newData.TechSpecMain;
                TechSpecAux = newData.TechSpecAux;
                TypeP1 = newData.TypeP1;
                TypeP2 = newData.TypeP2;
                SizeP1 = newData.SizeP1;
                SizeP2 = newData.SizeP2;
                MatMatID = newData.MatMatID;
                PClass = newData.PClass;
                MoreSpecCn = newData.MoreSpecCn;
                MoreSpecEn = newData.MoreSpecEn;
                RemarksCn = newData.RemarksCn;
                RemarksEn = newData.RemarksEn;
                SpecCombMain = newData.SpecCombMain;
                SpecCombAux = newData.SpecCombAux;
                SpecCombPort = newData.SpecCombPort;
                SpecMat= newData.SpecMat;
                SpecPClass = newData.SpecPClass;
            }
        }
        public int ID { get; set; }
        public string CatID { get; set; }
        public string SubCatID { get; set; }
        //public string NameCn { get; set; }  
        //public string NameEn { get; set; }
        private string _NameCn;
        public string NameCn
        {
            get => _NameCn;
            set
            {
                if (_NameCn != value)
                {
                    _NameCn = value;
                    OnPropertyChanged(nameof(NameCn));
                }
            }
        }
        private string _NameEn;
        public string NameEn
        {
            get => _NameEn;
            set
            {
                if (_NameEn != value)
                {
                    _NameEn = value;
                    OnPropertyChanged(nameof(NameEn));
                }
            }
        }
        public string Name
        {
            get
            {
                if (HK_General.intLan == 0)
                    return NameCn;
                else
                    return NameEn;
            }
        }
        public string TechSpecMain { get; set; }
        private string _SpecCombMain;
        public string SpecCombMain
        {
            get => _SpecCombMain;
            set
            {
                if (_SpecCombMain != value)
                {
                    _SpecCombMain = value;
                    OnPropertyChanged(nameof(SpecCombMain));
                }
            }
        }
        public string TechSpecAux { get; set; }
        //public string SpecCombAux { get; set; }
        private string _SpecCombAux;
        public string SpecCombAux
        {
            get => _SpecCombAux;
            set
            {
                if (_SpecCombAux != value)
                {
                    _SpecCombAux = value;
                    OnPropertyChanged(nameof(SpecCombAux));
                }
            }
        }
        public string TypeP1 { get; set; }
        public string SizeP1 { get; set; }
        public string TypeP2 { get; set; }
        public string SizeP2 { get; set; }
        public string MatMatID { get; set; }
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

        //public string SpecCombPort { get; set; }
        private string _SpecCombPort;
        public string SpecCombPort
        {
            get => _SpecCombPort;
            set
            {
                if (_SpecCombPort != value)
                {
                    _SpecCombPort = value;
                    OnPropertyChanged(nameof(SpecCombPort));
                }
            }
        }
        //public string SpecMat { get; set; }
        private string _SpecMat;
        public string SpecMat
        {
            get => _SpecMat;
            set
            {
                if (_SpecMat != value)
                {
                    _SpecMat = value;
                    OnPropertyChanged(nameof(SpecMat));
                }
            }
        }
        public string PClass { get; set; }
        //public string SpecPClass { get; set; }
        private string _SpecPClass;
        public string SpecPClass
        {
            get => _SpecPClass;
            set
            {
                if (_SpecPClass != value)
                {
                    _SpecPClass = value;
                    OnPropertyChanged(nameof(SpecPClass));
                }
            }
        }
        //public string MoreSpecCn { get; set; }
        //public string MoreSpecEn { get; set; }
        private string _MoreSpecCn;
        public string MoreSpecCn
        {
            get => _MoreSpecCn;
            set
            {
                if (_MoreSpecCn != value)
                {
                    _MoreSpecCn = value;
                    OnPropertyChanged(nameof(MoreSpecCn));
                }
            }
        }
        private string _MoreSpecEn;
        public string MoreSpecEn
        {
            get => _MoreSpecEn;
            set
            {
                if (_MoreSpecEn != value)
                {
                    _MoreSpecEn = value;
                    OnPropertyChanged(nameof(MoreSpecEn));
                }
            }
        }
        public string MoreSpec
        {
            get
            {
                if (HK_General.intLan == 0)
                    return MoreSpecCn;
                else
                    return MoreSpecEn;
            }
        }
        public string AppStd { get; set; }
        //public string RemarksCn { get; set; }
        //public string RemarksEn { get; set; }
        private string _RemarksCn;
        public string RemarksCn
        {
            get => _RemarksCn;
            set
            {
                if (_RemarksCn != value)
                {
                    _RemarksCn = value;
                    OnPropertyChanged(nameof(RemarksCn));
                }
            }
        }
        private string _RemarksEn;
        public string RemarksEn
        {
            get => _RemarksEn;
            set
            {
                if (_RemarksEn != value)
                {
                    _RemarksEn = value;
                    OnPropertyChanged(nameof(RemarksEn));
                }
            }
        }
        public string Comments { get; set; }
        public string SpecCombAll
        {
            get
            {
                return string.Join("; ", new List<string> { SpecCombMain, SpecCombPort, SpecCombAux, SpecMat, MoreSpec }
                                                 .Where(item => !string.IsNullOrWhiteSpace(item))
                                                 .Select(item => item.Trim())
                                                 .ToList());
            }
        }
        public string Remarks
        {
            get
            {
                if (HK_General.intLan == 0)
                    return RemarksCn;
                else
                    return RemarksEn;
            }
        }
        public string AlterCode { get; set; }
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
