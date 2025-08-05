using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
        public string SpecCombMain { get; set; }
        public string TechSpecAux { get; set; }
        public string SpecCombAux { get; set; }
        public string TypeP1 { get; set; }
        public string SizeP1 { get; set; }
        public string TypeP2 { get; set; }
        public string SizeP2 { get; set; }
         public string MatSpec { get; set; }
        public string SpecCombPort { get; set; }
        public string SpecMat { get; set; }
        public string PClass { get; set; }
        public string SpecPClass { get; set; }
        public string MoreSpecCn { get; set; }
        public string MoreSpecEn { get; set; }
        public string SpecMore
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
        public string SpecCombAll
        {
            get
            {
                return string.Join("; ", new List<string> { SpecCombMain, SpecCombPort, SpecCombAux, SpecMat, SpecMore }
                                                 .Select(item => item.Trim())
                                                 .Where(item => !string.IsNullOrWhiteSpace(item))
                                                 .ToList());
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
        public string AlterCode { get; set; }
    }
}
