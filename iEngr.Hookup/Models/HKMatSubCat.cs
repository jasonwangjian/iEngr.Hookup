using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    public class HKMatSubCat
    {
        public string ID { get; set; }
        public string CatID { get; set; }
        public string SubCatID { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public string Remarks { get; set; }
        public int  SortNum { get; set; }
        public string TypeP1 { get; set; }
        public string TypeP2 { get; set; }
        public string TechSpecMain{ get; set; }
        public string TechSpecAux { get; set; }

        public string Name
        {
            get
            {
                if (HK_LibMat.intLan == 0)
                    return SpecCn;
                else
                    return SpecEn;
            }
        }
    }
}
