using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    public class HKLibTubeOD
    {
        public string ID { get; set; }
        public string Class { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public decimal ValueM { get; set; }
        public string ClassEx { get; set; }
        public int SortNum { get; set; }
        public string Name
        {
            get
            {
                if (HK_General.intLan == 0)
                    return SpecCn;
                else
                    return SpecEn;
            }
        }
        public string Spec
        {
            get
            {
                if (HK_General.intLan == 0)
                    return SpecCn;
                else
                    return SpecEn;
            }
        }
    }
}
