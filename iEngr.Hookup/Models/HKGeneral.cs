using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iEngr.Hookup.ViewModels.MatDataViewModel;

namespace iEngr.Hookup
{
    public class CmbItem : IIdentifiable
    {
        public string ID { get; set; }
        public string Comp { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string Class { get; set; }
        public string Link { get; set; }
        //public string PrefixCn { get; set; }
        //public string PrefixEn { get; set; }
        //public string SuffixCn { get; set; }
        //public string SuffixEn { get; set; }
        public string Name
        {
            get
            {
                if (HK_General.intLan == 0)
                    return NameCn;
                else
                    return NameEn;
            }
        }
        //public string Prefix
        //{
        //    get
        //    {
        //        if (HK_LibMat.intLan == 0)
        //            return PrefixCn;
        //        else
        //            return PrefixEn;
        //    }
        //}
        //public string Suffix
        //{
        //    get
        //    {
        //        if (HK_LibMat.intLan == 0)
        //            return SuffixCn;
        //        else
        //            return SuffixEn;
        //    }
        //}
    }
    public class GeneralItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
