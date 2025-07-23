using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    public class HKMatGenLib
    {
        public string ID { get; set; }
        public string CatID { get; set; }
        public string SubCatID { get; set; }
        public string TechSpecMain { get; set; }
        public string TechSpecAux { get; set; }
        public string TypeP1 { get; set; }
        public string SpecP1 { get; set; }
        public string TypeP2 { get; set; }
        public string SpecP2 { get; set; }
        public string MatSpec { get; set; }
        public string PClass { get; set; }
        public string MoreSpec { get; set; }
        public string AppStd { get; set; }
        public string RemarksCn { get; set; }
        public string RemarksEn { get; set; }
        public string Comments { get; set; }
        public string SpecComb { get; set; }
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
