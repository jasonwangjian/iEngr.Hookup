using System;

namespace iEngr.Hookup.Models
{
    public class HKMatGenLib
    {
        public int ID { get; set; }
        public string CatID { get; set; }
        public string NameID { get; set; }
        public string TechSpecMain { get; set; }
        public string TechSpecAux { get; set; }
        public string TypeP1 { get; set; }
        public string SizeP1 { get; set; }
        public string TypeP2 { get; set; }
        public string SizeP2 { get; set; }
        public string MatMatID { get; set; }
        public string PClass { get; set; }
        public string MoreSpecCn { get; set; }
        public string MoreSpecEn { get; set; }
        public string AppStd { get; set; }
        public string RemarksCn { get; set; }
        public string RemarksEn { get; set; }
        public string Comments { get; set; }
        public byte Status { get; set; }
        public string LastBy { get; set; }
        public DateTime LastOn { get; set; }
    }
}
