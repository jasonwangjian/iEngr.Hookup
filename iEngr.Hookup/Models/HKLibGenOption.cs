using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    public class HKLibGenOption
    {
        public string ID { get; set; }
        public string Cat { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public int? Inact { get; set; }
         public int SortNum { get; set; }
        public string Name
        {
            get
            {
                if (HK_LibMat.intLan == 0)
                    return NameCn;
                else
                    return NameEn;
            }
        }
        public string Spec
        {
            get
            {
                if (HK_LibMat.intLan == 0)
                    return NameCn;
                else
                    return NameEn;
            }
        }
    }
}
