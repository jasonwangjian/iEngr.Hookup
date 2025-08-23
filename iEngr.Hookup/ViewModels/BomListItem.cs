using iEngr.Hookup.ViewModels;
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
using System.Windows;

namespace iEngr.Hookup.Models
{
    public class BomListItem : MatListItem
    {
        public void SetDataFromComosObject() {
            No = ObjMat.Label;
            NameCn = ObjMat.spec("Z00T00003.Name").GetInternationalDisplayValue(4);
            NameEn = ObjMat.spec("Z00T00003.Name").GetInternationalDisplayValue(2);
            RemarksCn = ObjMat.spec("Z00T00003.Remarks").GetInternationalDisplayValue(4);
            RemarksEn = ObjMat.spec("Z00T00003.Remarks").GetInternationalDisplayValue(2);
            Qty = ObjMat.spec("Z00T00003.Qty").value;
            Unit = ObjMat.spec("Z00T00003.Unit").value;
            SupplyDiscipline = ObjMat.spec("Z00T00003.SD").value;
            SupplyResponsible = ObjMat.spec("Z00T00003.SR").value;
            ErectionDiscipline = ObjMat.spec("Z00T00003.ED").value;
            ErectionResponsible = ObjMat.spec("Z00T00003.ER").value;
        }
        public void SetComosObjectFromData(MatListItem matItem = null)
        {
            ObjMat.Label = No;
            ObjMat.spec("Z00T00003.Qty").value = Qty;
            ObjMat.spec("Z00T00003.Unit").value = Unit;
            ObjMat.spec("Z00T00003.SD").value = SupplyDiscipline;
            ObjMat.spec("Z00T00003.SR").value = SupplyResponsible;
            ObjMat.spec("Z00T00003.ED").value = ErectionDiscipline;
            ObjMat.spec("Z00T00003.ER").value = ErectionResponsible; if (matItem == null)
            {
                ObjMat.SetInternationalDescription(4, NameCn);
                ObjMat.SetInternationalDescription(2, NameEn);
                ObjMat.spec("Z00T00003.Name").SetInternationalValue(4, NameCn);
                ObjMat.spec("Z00T00003.Name").SetInternationalValue(2, NameEn);
                ObjMat.spec("Z00T00003.Remarks").SetInternationalValue(4, RemarksCn);
                ObjMat.spec("Z00T00003.Remarks").SetInternationalValue(2, RemarksEn);

            }
            else
            {
                ObjMat.SetInternationalDescription(4, matItem.NameCn);
                ObjMat.SetInternationalDescription(2, matItem.NameEn);
                ObjMat.spec("Z00T00003.Name").SetInternationalValue(4, matItem.NameCn);
                ObjMat.spec("Z00T00003.Name").SetInternationalValue(2, matItem.NameEn);
                ObjMat.spec("Z00T00003.Remarks").SetInternationalValue(4, matItem.RemarksCn);
                ObjMat.spec("Z00T00003.Remarks").SetInternationalValue(2, matItem.RemarksEn);
            }
        }
        private string _no;
        public string No
        {
            get => _no;
            set => SetField(ref _no, value);
        }
        private string _qty;
        public string Qty
        {
            get => _qty;
            set => SetField(ref _qty, value);
        }
        private string _unit;
        public string Unit
        {
            get => _unit;
            set => SetField(ref _unit, value);
        }
        private string _supplyDiscipline;
        public string SupplyDiscipline
        {
            get => _supplyDiscipline;
            set
            {
                if (SetField(ref _supplyDiscipline, value))
                {
                }

            }
       }
        private string _supplyResponsible;
        public string SupplyResponsible
        {
            get => _supplyResponsible;
            set => SetField(ref _supplyResponsible, value);
        }
        private string _erectionDiscipline;
        public string ErectionDiscipline
        {
            get => _erectionDiscipline;
            set => SetField(ref _erectionDiscipline, value);
        }
        private string _erectionResponsible;
        public string ErectionResponsible
        {
            get => _erectionResponsible;
            set => SetField(ref _erectionResponsible, value);
        }
        IComosBaseObject _objMat;
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
