using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup
{
    //public class DN
    //{
    //    public string ID { get; set; }
    //    public string Name { get; set; }
    //}
    public class Prompt
    {
        public string ID { get; set; }
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
    }
}
