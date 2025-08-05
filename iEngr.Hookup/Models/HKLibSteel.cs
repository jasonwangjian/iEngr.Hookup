using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    public class HKLibSteel
    {
        public string ID { get; set; }
        public string CSSpecCn { get; set; }
        public string CSSpecEn { get; set; }
        public string IBSpecCn { get; set; }
        public string IBSpecEn { get; set; }
        public decimal Width { get; set; }
        public decimal? CSb { get; set; }
        public decimal? CSd { get; set; }
        public decimal? IBb { get; set; }
        public decimal? IBd { get; set; }
        public int SortNum { get; set; }
        public string Name { get; set; }
        public string CSSpec
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return CSSpecCn;
                else
                    return CSSpecEn;
            }
        }
        public string IBSpec
        {
            get
            {
                if (HK_Mat_Main.intLan == 0)
                    return IBSpecCn;
                else
                    return IBSpecEn;
            }
        }
    }
}
