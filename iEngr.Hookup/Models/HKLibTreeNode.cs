
namespace iEngr.Hookup.Models
{
    public class HKLibTreeNode : IIdentifiable
    {
        public string ID { get; set; }
        public string Parent { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string RemarksCn { get; set; }
        public string RemarksEn { get; set; }
        public string NodeType { get; set; }
        public string IdentType { get; set; }
        public string FullName { get; set; }
        public string NestedName { get; set; }
        public string SpecValue { get; set; }
        public string IconName {  get; set; }   
        public bool IsPropNode { get; set; }
        public bool IsPropHolder { get; set; }  
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
