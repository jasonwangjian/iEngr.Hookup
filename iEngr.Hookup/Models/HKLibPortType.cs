using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    public class HKLibPortType
    {
        public string ID { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public string Link { get; set; }
        public string Remarks { get; set; }
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
    }
}
