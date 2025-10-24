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
    public class BomItem1 : MatListItem
    {
        public BomItem1() 
        {
            AutoComosUpdate = HK_General.IsAutoComosUpdate;
        }
        //public void SetDataFromComosObject()
        //{
        //    No = ObjComosBomItem.Label;
        //    ID = ObjComosBomItem.spec("Z00T00003.ID").value;
        //    NameCn = ObjComosBomItem.spec("Z00T00003.Name").GetInternationalDisplayValue(4);
        //    NameEn = ObjComosBomItem.spec("Z00T00003.Name").GetInternationalDisplayValue(2);
        //    SpecAllCn = ObjComosBomItem.spec("Z00T00003.SpecAll").GetInternationalDisplayValue(4);
        //    SpecAllEn = ObjComosBomItem.spec("Z00T00003.SpecAll").GetInternationalDisplayValue(2);
        //    RemarksCn = ObjComosBomItem.spec("Z00T00003.Remarks").GetInternationalDisplayValue(4);
        //    RemarksEn = ObjComosBomItem.spec("Z00T00003.Remarks").GetInternationalDisplayValue(2);
        //    MatMatCode = ObjComosBomItem.spec("Z00T00003.Mat").value;
        //    Qty = ObjComosBomItem.spec("Z00T00003.Qty").value;
        //    Unit = ObjComosBomItem.spec("Z00T00003.Unit").value;
        //    SupplyDiscipline = ObjComosBomItem.spec("Z00T00003.SD").value;
        //    SupplyResponsible = ObjComosBomItem.spec("Z00T00003.SR").value;
        //    ErectionDiscipline = ObjComosBomItem.spec("Z00T00003.ED").value;
        //    ErectionResponsible = ObjComosBomItem.spec("Z00T00003.ER").value;
        //}
        public void SetComosObjectFromData()
        {
            if (ObjComosBomItem == null) return;
            ObjComosBomItem.Label = No;
            ObjComosBomItem.spec("Z00T00003.ID").value = ID;
            ObjComosBomItem.spec("Z00T00003.Qty").value = Qty;
            ObjComosBomItem.spec("Z00T00003.Unit").value = Unit;
            ObjComosBomItem.spec("Z00T00003.SD").value = SupplyDiscipline;
            ObjComosBomItem.spec("Z00T00003.SR").value = SupplyResponsible;
            ObjComosBomItem.spec("Z00T00003.ED").value = ErectionDiscipline;
            ObjComosBomItem.spec("Z00T00003.ER").value = ErectionResponsible;
            ObjComosBomItem.spec("Z00T00003.Mat").value = MatMatCode;
            ObjComosBomItem.SetInternationalDescription(4, NameCn);
            ObjComosBomItem.SetInternationalDescription(2, NameEn);
            ObjComosBomItem.spec("Z00T00003.Name").SetInternationalValue(4, NameCn);
            ObjComosBomItem.spec("Z00T00003.Name").SetInternationalValue(2, NameEn);
            ObjComosBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(4, SpecAllCn);
            ObjComosBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(2, SpecAllEn);
            ObjComosBomItem.spec("Z00T00003.Remarks").SetInternationalValue(4, RemarksCn);
            ObjComosBomItem.spec("Z00T00003.Remarks").SetInternationalValue(2, RemarksEn);
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
            if (LibBomItem == null) { return; }
            //No = ObjMatListItem.No;
            ID = LibBomItem.ID;
            NameCn = LibBomItem.NameCn;
            NameEn = LibBomItem.NameEn;
            SpecAllCn = LibBomItem.SpecAllCn;
            SpecAllEn = LibBomItem.SpecAllEn;
            //SpecMainCn = ObjMatListItem.SpecMainCn;
            //SpecMainEn = ObjMatListItem.SpecMainEn;
            //SpecPortCn = ObjMatListItem.SpecPortCn;
            //SpecPortEn = ObjMatListItem.SpecPortEn;
            //SpecAuxCn = ObjMatListItem.SpecAuxCn;
            //SpecAuxEn = ObjMatListItem.SpecAuxEn;
            //SpecMoreCn = ObjMatListItem.SpecMoreCn;
            //SpecMoreEn = ObjMatListItem.SpecMoreEn;
            RemarksCn = LibBomItem.RemarksCn;
            RemarksEn = LibBomItem.RemarksEn;
            MatMatCode = LibBomItem.MatMatCode;
            Qty = LibBomItem.Qty;
            Unit = LibBomItem.Unit;
            SupplyDiscipline = LibBomItem.SupplyDiscipline;
            SupplyResponsible = LibBomItem.SupplyResponsible;
            ErectionDiscipline = LibBomItem.ErectionDiscipline;
            ErectionResponsible = LibBomItem.ErectionResponsible;
        }
        public bool AutoComosUpdate { get; set; }
        public int BomID { get; set; }
        //private int _bomID;
        //public int BomID
        //{
        //    get => _bomID;
        //    set => SetField(ref _bomID, value);
        //}
        private string _no;
        public new string No
        {
            get => _no;
            set// => SetField(ref _no, value);
            {
                if (SetField(ref _no, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.Label = value;
            }
        }
        private string _id;
        public new string ID
        {
            get => _id;
            set
            {
                if (SetField(ref _id, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.ID").value = value;
            }
        }
        private string _qty;
        public new string Qty
        {
            get => _qty;
            set
            {
                if (SetField(ref _qty, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.Qty").value = value;
            }
        }
        private string _unit;
        public new string Unit
        {
            get => _unit;
            set
            {
                if (SetField(ref _unit, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.Unit").value = value;
            }
        }
        private string _supplyDiscipline;
        public new string SupplyDiscipline
        {
            get => _supplyDiscipline;
            set
            {
                if (SetField(ref _supplyDiscipline, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.SD").value = value;
            }
        }
        private string _supplyResponsible;
        public new string SupplyResponsible
        {
            get => _supplyResponsible;
            set
            {
                if (SetField(ref _supplyResponsible, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.SR").value = value;
            }
        }
        private string _erectionDiscipline;
        public new string ErectionDiscipline
        {
            get => _erectionDiscipline;
            set
            {
                if (SetField(ref _erectionDiscipline, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.ED").value = value;
            }
        }
        private string _erectionResponsible;
        public new string ErectionResponsible
        {
            get => _erectionResponsible;
            set
            {
                if (SetField(ref _erectionResponsible, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.ER").value = value;
            }
        }
        private string _remarksCn;
        public new string RemarksCn
        {
            get => _remarksCn;
            set
            {
                if (SetField(ref _remarksCn, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.Remarks").SetInternationalValue(4, value);
            }
        }
        private string _remarksEn;
        public new string RemarksEn
        {
            get => _remarksEn;
            set
            {
                if (SetField(ref _remarksEn, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.Remarks").SetInternationalValue(2, value);
            }
        }
        private string _specAllCn;
        public new string SpecAllCn
        {
            get => _specAllCn;
            set
            {
                if (SetField(ref _specAllCn, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(4, value);
            }
        }
        private string _specAllEn;
        public new string SpecAllEn
        {
            get => _specAllEn;
            set
            {
                if (SetField(ref _specAllEn, value) && ObjComosBomItem != null && AutoComosUpdate)
                    ObjComosBomItem.spec("Z00T00003.SpecAll").SetInternationalValue(2, value);
            }
        }

        IComosBaseObject _objComosBomItem;
        public IComosBaseObject ObjComosBomItem
        {
            get => _objComosBomItem;
            set
            {
                if (SetField(ref _objComosBomItem, value) && value != null)
                {
                    No = value.Label;
                    ID = value.spec("Z00T00003.ID").value;
                    NameCn = value.spec("Z00T00003.Name").GetInternationalDisplayValue(4);
                    NameEn = value.spec("Z00T00003.Name").GetInternationalDisplayValue(2);
                    SpecAllCn = value.spec("Z00T00003.SpecAll").GetInternationalDisplayValue(4);
                    SpecAllEn = value.spec("Z00T00003.SpecAll").GetInternationalDisplayValue(2);
                    RemarksCn = value.spec("Z00T00003.Remarks").GetInternationalDisplayValue(4);
                    RemarksEn = value.spec("Z00T00003.Remarks").GetInternationalDisplayValue(2);
                    MatMatCode = value.spec("Z00T00003.Mat").value;
                    Qty = value.spec("Z00T00003.Qty").value;
                    Unit = value.spec("Z00T00003.Unit").value;
                    SupplyDiscipline = value.spec("Z00T00003.SD").value;
                    SupplyResponsible = value.spec("Z00T00003.SR").value;
                    ErectionDiscipline = value.spec("Z00T00003.ED").value;
                    ErectionResponsible = value.spec("Z00T00003.ER").value;
                }
            }
        }
        MatListItem _libBomItem;
        public MatListItem LibBomItem
        {
            get => _libBomItem;
            set => SetField(ref _libBomItem, value);
        }
    }
}
