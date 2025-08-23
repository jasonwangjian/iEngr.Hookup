using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static iEngr.Hookup.ViewModels.MatDataViewModel;

namespace iEngr.Hookup.Models
{
    public class HKLibMatCat : IIdentifiable
    {
        public string ID { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string Remarks { get; set; }
        public int SortNum { get; set; }
        public string Name
        {
            get=> (HK_General.intLan == 2)? NameEn:NameCn;
        }
    }
}
