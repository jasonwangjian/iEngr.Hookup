using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    public class HKLibPN
    {
        public string ID { get; set; }
        public string Class { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public string ISOS1 { get; set; }
        public string ISOS2 { get; set; }
        public string GBDIN { get; set; }
        public string GBANSI { get; set; }
        public string ASME { get; set; }
        public int SortNum { get; set; }
        public string Name
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return SpecCn;
                else
                    return SpecEn;
            }
        }
        public string Spec
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return SpecCn;
                else
                    return SpecEn;
            }
        }
    }
}
