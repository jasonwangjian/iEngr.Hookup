using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.Models
{
    public class HKLibDevValue : IIdentifiable
    {
        public string ID { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string RemarksCn { get; set; }
        public string RemarksEn { get; set; }
        public string DevTag { get; set; }
        public string TagType { get; set; }
        public string FullName { get; set; }
        public string DevName { get; set; }
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
    }
}
