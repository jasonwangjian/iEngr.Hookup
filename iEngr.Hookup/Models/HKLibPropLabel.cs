using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.Models
{
    public class HKLibPropLabel : IIdentifiable
    {
        public string ID { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string RemarksCn { get; set; }
        public string RemarksEn { get; set; }
        public byte Status { get; set; }
        public int SortNum { get; set; }
        public string Name
        {
            get => (HK_General.ProjLanguage == 2) ? NameEn : NameCn;
        }
        public string Remarks
        {
            get => (HK_General.ProjLanguage == 2) ? RemarksEn : RemarksCn;
        }
        public string NestedName { get; set; }
        public string StandardTable { get; set; }
        public string AppliedDevice { get; set; }
        public string PropertyType{ get; set; }
        public string DicApplied { get; set; }
        public string PropCode { get; set; }
        public string PrefixCn { get; set; }
        public string PropNameCn { get; set; }
        public string SuffixCn { get; set; }
        public string PrefixEn { get; set; }
        public string PropNameEn { get; set; }
        public string SuffixEn { get; set; }
    }
}
