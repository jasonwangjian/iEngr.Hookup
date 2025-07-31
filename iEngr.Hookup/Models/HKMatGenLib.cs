using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    public class HKMatGenLib
    {
        public int ID { get; set; }
        public string CatID { get; set; }
        public string SubCatID { get; set; }
        public string NameCn { get; set; }  
        public string NameEn { get; set; }
        public string Name
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return NameCn;
                else
                    return NameEn;
            }
        }
        public string TechSpecMain { get; set; }
        public string SpecCombMainCn { get; set; }
        public string SpecCombMainEn { get; set; }
        public string SpecCombMain
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return SpecCombMainCn;
                else
                    return SpecCombMainEn;
            }
        }
        public string TechSpecAux { get; set; }
        public string SpecCombAuxCn { get; set; }
        public string SpecCombAuxEn { get; set; }
        public string SpecCombAux
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return SpecCombAuxCn;
                else
                    return SpecCombAuxEn;
            }
        }
        public string TypeP1 { get; set; }
        public string SizeP1 { get; set; }
        public string SpecCombP1Cn { get; set; }
        public string SpecCombP1En { get; set; }
        public string SpecCombP1
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return SpecCombP1Cn;
                else
                    return SpecCombP1En;
            }
        }
        public string TypeP2 { get; set; }
        public string SizeP2 { get; set; }
        public string SpecCombP2Cn { get; set; }
        public string SpecCombP2En { get; set; }
        public string SpecCombP2
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return SpecCombP2Cn;
                else
                    return SpecCombP2En;
            }
        }
        public string MatSpec { get; set; }
        public string SpecCombMatCn { get; set; }
        public string SpecCombMatEn { get; set; }
        public string SpecCombMat
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return SpecCombMatCn;
                else
                    return SpecCombMatEn;
            }
        }
        public string PClass { get; set; }
        public string SpecCombPCCn { get; set; }
        public string SpecCombPCEn { get; set; }
        public string SpecCombPC
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return SpecCombPCCn;
                else
                    return SpecCombPCEn;
            }
        }
        public string MoreSpecCn { get; set; }
        public string MoreSpecEn { get; set; }
        public string MoreSpec
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return MoreSpecCn;
                else
                    return MoreSpecEn;
            }
        }
        public string AppStd { get; set; }
        public string RemarksCn { get; set; }
        public string RemarksEn { get; set; }
        public string Comments { get; set; }
        public string SpecCombAllCn { get; set; }
        public string SpecCombAllEn { get; set; }
        public string SpecCombAll
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return SpecCombAllCn;
                else
                    return SpecCombAllEn;
            }
        }
        public string Remarks
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return RemarksCn;
                else
                    return RemarksEn;
            }
        }
    }
}
