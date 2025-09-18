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
        public BomListItem() 
        {
            AutoComosUpdate = HK_General.IsAutoComosUpdate;
        }
        public void SetDataFromComosObject()
        {
            No = ObjMatBomItem.Label;
            ID = ObjMatBomItem.spec("Z00T00003.ID").value;
            NameCn = ObjMatBomItem.spec("Z00T00003.Name").GetInternationalDisplayValue(4);
            NameEn = ObjMatBomItem.spec("Z00T00003.Name").GetInternationalDisplayValue(2);
            SpecAllCn = ObjMatBomItem.spec("Z00T00003.SpecAll").GetInternationalDisplayValue(4);
            SpecAllEn = ObjMatBomItem.spec("Z00T00003.SpecAll").GetInternationalDisplayValue(2);
            RemarksCn = ObjMatBomItem.spec("Z00T00003.Remarks").GetInternationalDisplayValue(4);
            RemarksEn = ObjMatBomItem.spec("Z00T00003.Remarks").GetInternationalDisplayValue(2);
            MatMatCode = ObjMatBomItem.spec("Z00T00003.Mat").value;
            Qty = ObjMatBomItem.spec("Z00T00003.Qty").value;
            Unit = ObjMatBomItem.spec("Z00T00003.Unit").value;
            SupplyDiscipline = ObjMatBomItem.spec("Z00T00003.SD").value;
            SupplyResponsible = ObjMatBomItem.spec("Z00T00003.SR").value;
            ErectionDiscipline = ObjMatBomItem.spec("Z00T00003.ED").value;
            ErectionResponsible = ObjMatBomItem.spec("Z00T00003.ER").value;
        }
        public void SetComosObjectFromData()
        {
            if (ObjMatBomItem == null) return;
            ObjMatBomItem.Label = No;
            ObjMatBomItem.spec("Z00T00003.ID").value = ID;
            ObjMatBomItem.spec("Z00T00003.Qty").value = Qty;
            ObjMatBomItem.spec("Z00T00003.Unit").value = Unit;
            ObjMatBomItem.spec("Z00T00003.SD").value = SupplyDiscipline;
            ObjMatBomItem.spec("Z00T00003.SR").value = SupplyResponsible;
            ObjMatBomItem.spec("Z00T00003.ED").value = ErectionDiscipline;
            ObjMatBomItem.spec("Z00T00003.ER").value = ErectionResponsible;
            ObjMatBomItem.spec("Z00T00003.Mat").value = MatMatCode;
            ObjMatBomItem.SetInternationalDescription(4, NameCn);
            ObjMatBomItem.SetInternationalDescription(2, NameEn);
            ObjMatBomItem.spec("Z00T00003.Name").SetInternationalValue(4, NameCn);
            ObjMatBomItem.spec("Z00T00003.Name").SetInternationalValue(2, NameEn);
            ObjMatBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(4, SpecAllCn);
            ObjMatBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(2, SpecAllEn);
            ObjMatBomItem.spec("Z00T00003.Remarks").SetInternationalValue(4, RemarksCn);
            ObjMatBomItem.spec("Z00T00003.Remarks").SetInternationalValue(2, RemarksEn);
        }
        //public void SetComosObjectFromData(MatListItem matItem = null)
        //{
        //    ObjMatBomItem.Label = No;
        //    ObjMatBomItem.spec("Z00T00003.Qty").value = Qty;
        //    ObjMatBomItem.spec("Z00T00003.Unit").value = Unit;
        //    ObjMatBomItem.spec("Z00T00003.SD").value = SupplyDiscipline;
        //    ObjMatBomItem.spec("Z00T00003.SR").value = SupplyResponsible;
        //    ObjMatBomItem.spec("Z00T00003.ED").value = ErectionDiscipline;
        //    ObjMatBomItem.spec("Z00T00003.ER").value = ErectionResponsible;
        //    if (matItem == null)
        //    {
        //        ObjMatBomItem.spec("Z00T00003.ID").value = ID;
        //        ObjMatBomItem.spec("Z00T00003.Mat").value = MatMatCode;
        //        ObjMatBomItem.SetInternationalDescription(4, NameCn);
        //        ObjMatBomItem.SetInternationalDescription(2, NameEn);
        //        ObjMatBomItem.spec("Z00T00003.Name").SetInternationalValue(4, NameCn);
        //        ObjMatBomItem.spec("Z00T00003.Name").SetInternationalValue(2, NameEn);
        //        ObjMatBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(4, SpecAllCn);
        //        ObjMatBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(2, SpecAllEn);
        //        ObjMatBomItem.spec("Z00T00003.Remarks").SetInternationalValue(4, RemarksCn);
        //        ObjMatBomItem.spec("Z00T00003.Remarks").SetInternationalValue(2, RemarksEn);

        //    }
        //    else
        //    {
        //        ObjMatBomItem.spec("Z00T00003.ID").value = (matItem.MatLibItem.ID).ToString("D4");
        //        ObjMatBomItem.spec("Z00T00003.Mat").value = matItem.MatMatCode;
        //        ObjMatBomItem.SetInternationalDescription(4, matItem.NameCn);
        //        ObjMatBomItem.SetInternationalDescription(2, matItem.NameEn);
        //        ObjMatBomItem.spec("Z00T00003.Name").SetInternationalValue(4, matItem.NameCn);
        //        ObjMatBomItem.spec("Z00T00003.Name").SetInternationalValue(2, matItem.NameEn);
        //        ObjMatBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(4, matItem.SpecAllCn);
        //        ObjMatBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(2, matItem.SpecAllEn);
        //        ObjMatBomItem.spec("Z00T00003.Remarks").SetInternationalValue(4, matItem.RemarksCn);
        //        ObjMatBomItem.spec("Z00T00003.Remarks").SetInternationalValue(2, matItem.RemarksEn);
        //    }
        //}
        public void SetBomListItemFromMatListItem()
        {
            if (ObjMatListItem == null) { return; }
            //No = ObjMatListItem.No;
            ID = ObjMatListItem.ID;
            NameCn = ObjMatListItem.NameCn;
            NameEn = ObjMatListItem.NameEn;
            SpecAllCn = ObjMatListItem.SpecAllCn;
            SpecAllEn = ObjMatListItem.SpecAllEn;
            //SpecMainCn = ObjMatListItem.SpecMainCn;
            //SpecMainEn = ObjMatListItem.SpecMainEn;
            //SpecPortCn = ObjMatListItem.SpecPortCn;
            //SpecPortEn = ObjMatListItem.SpecPortEn;
            //SpecAuxCn = ObjMatListItem.SpecAuxCn;
            //SpecAuxEn = ObjMatListItem.SpecAuxEn;
            //SpecMoreCn = ObjMatListItem.SpecMoreCn;
            //SpecMoreEn = ObjMatListItem.SpecMoreEn;
            RemarksCn = ObjMatListItem.RemarksCn;
            RemarksEn = ObjMatListItem.RemarksEn;
            MatMatCode = ObjMatListItem.MatMatCode;
            Qty = ObjMatListItem.Qty;
            Unit = ObjMatListItem.Unit;
            SupplyDiscipline = ObjMatListItem.SupplyDiscipline;
            SupplyResponsible = ObjMatListItem.SupplyResponsible;
            ErectionDiscipline = ObjMatListItem.ErectionDiscipline;
            ErectionResponsible = ObjMatListItem.ErectionResponsible;
        }
        public bool AutoComosUpdate { get; set; }
        private string _no;
        public new string No
        {
            get => _no;
            set// => SetField(ref _no, value);
            {
                if (SetField(ref _no, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.Label = value;
            }
        }
        private string _id;
        public new string ID
        {
            get => _id;
            set
            {
                if (SetField(ref _id, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.ID").value = value;
            }
        }
        private string _qty;
        public new string Qty
        {
            get => _qty;
            set
            {
                if (SetField(ref _qty, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.Qty").value = value;
            }
        }
        private string _unit;
        public new string Unit
        {
            get => _unit;
            set
            {
                if (SetField(ref _unit, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.Unit").value = value;
            }
        }
        private string _supplyDiscipline;
        public new string SupplyDiscipline
        {
            get => _supplyDiscipline;
            set
            {
                if (SetField(ref _supplyDiscipline, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.SD").value = value;
            }
        }
        private string _supplyResponsible;
        public new string SupplyResponsible
        {
            get => _supplyResponsible;
            set
            {
                if (SetField(ref _supplyResponsible, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.SR").value = value;
            }
        }
        private string _erectionDiscipline;
        public new string ErectionDiscipline
        {
            get => _erectionDiscipline;
            set
            {
                if (SetField(ref _erectionDiscipline, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.ED").value = value;
            }
        }
        private string _erectionResponsible;
        public new string ErectionResponsible
        {
            get => _erectionResponsible;
            set
            {
                if (SetField(ref _erectionResponsible, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.ER").value = value;
            }
        }
        private string _remarksCn;
        public new string RemarksCn
        {
            get => _remarksCn;
            set
            {
                if (SetField(ref _remarksCn, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.Remarks").SetInternationalValue(4, value);
            }
        }
        private string _remarksEn;
        public new string RemarksEn
        {
            get => _remarksEn;
            set
            {
                if (SetField(ref _remarksEn, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.Remarks").SetInternationalValue(2, value);
            }
        }
        private string _specAllCn;
        public new string SpecAllCn
        {
            get => _specAllCn;
            set
            {
                if (SetField(ref _specAllCn, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(4, value);
            }
        }
        private string _specAllEn;
        public new string SpecAllEn
        {
            get => _specAllEn;
            set
            {
                if (SetField(ref _specAllEn, value) && ObjMatBomItem != null && AutoComosUpdate)
                    ObjMatBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(2, value);
            }
        }

        IComosBaseObject _objMatBomItem;
        public IComosBaseObject ObjMatBomItem
        {
            get => _objMatBomItem;
            set => SetField(ref _objMatBomItem, value);
        }
        MatListItem _objMatListItem;
        public MatListItem ObjMatListItem
        {
            get => _objMatListItem;
            set => SetField(ref _objMatListItem, value);
        }
    }
}
