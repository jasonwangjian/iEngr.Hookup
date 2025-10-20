using iEngr.Hookup.Models;
using Plt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.ViewModels
{
    public class DiagramItem : INotifyPropertyChanged, IIntIdentifiable
    {
        public bool IsComosItem { get; set; }
        public bool IsLibItem { get; set; }
        public int ID { get; set; }
        public string RefID { get; set; }
        public string IdLabels { get; set; }
        IComosBaseObject _objComosDiag;
        public IComosBaseObject ObjComosDiag
        {
            get => _objComosDiag;
            set 
            {
                if (SetField(ref _objComosDiag, value) && value != null)
                {
                    RefID = value.spec("Y00T00103.RefIdInLib").value;
                    IdLabels = value.spec("Y00T00103.IdLabels").value;
                    NameCn = value.GetInternationalDescription(4);
                    NameEn = value.GetInternationalDescription(2);
                    RemarksCn = value.spec("Y00T00103.Remarks").GetInternationalDisplayValue(4);
                    RemarksEn = value.spec("Y00T00103.Remarks").GetInternationalDisplayValue(2);
                }
            }
        }
        public bool _isOwned;
        public bool IsOwned
        {
            get => _isOwned;
            set=> SetField(ref _isOwned, value);
        }
        public int _bomQty;
        public int BomQty
        {
            get => _bomQty;
            set => SetField(ref _bomQty, value);
        }
        public bool _isInherit;
        public bool IsInherit
        {
            get => _isInherit;
            set => SetField(ref _isInherit, value);
        }
        public byte Status { get; set; }
        public DateTime LastOn { get; set; }
        public string LastBy { get; set; }
        private string _picturePath;
        public string PicturePath
        {
            get => _picturePath;
            set
            {
                SetField(ref _picturePath, value);
            }
        }

        private string _nameCn;
        public string NameCn
        {
            get => _nameCn;
            set
            {
                SetField(ref _nameCn, value);
            }
        }
        private string _nameEn;
        public string NameEn
        {
            get => _nameEn;
            set
            {
                SetField(ref _nameEn, value);
            }
        }
        private string _descCn;
        public string DescCn
        {
            get => _descCn;
            set
            {
                SetField(ref _descCn, value);
            }
        }
        private string _descEn;
        public string DescEn
        {
            get => _descEn;
            set
            {
                SetField(ref _descEn, value);
            }
        }
        private string _remarksCn;
        public string RemarksCn
        {
            get => _remarksCn;
            set
            {
                SetField(ref _remarksCn, value);
            }
        }
        private string _remarksEn;
        public string RemarksEn
        {
            get => _remarksEn;
            set
            {
                SetField(ref _remarksEn, value);
            }
        }
        public string Name
        {
            get => (HK_General.ProjLanguage == 2) ? NameEn : NameCn;
        }
        public string Desc
        {
            get => (HK_General.ProjLanguage == 2) ? DescEn : DescCn;
        }
        public string Remarks
        {
            get => (HK_General.ProjLanguage == 2) ? RemarksEn : RemarksCn;
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
