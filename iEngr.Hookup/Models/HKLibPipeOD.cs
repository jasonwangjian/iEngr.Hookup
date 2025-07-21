using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    public class HKLibPipeOD
    {
        public string ID { get; set; }
        public string DN { get; set; }
        public string NPS { get; set; }
        public decimal? HGIa { get; set; }
        public decimal? HGIb { get; set; }
        public decimal? HGII { get; set; }
        public decimal? GBI { get; set; }
        public decimal? GBII { get; set; }
        public decimal? ISO { get; set; }
        public decimal? ASME { get; set; }
        public string SpecRem { get; set; }
        public decimal? SWDiaGB { get; set; }
        public int SortNum { get; set; }    
        public string Name { get; set; }
    }
}
