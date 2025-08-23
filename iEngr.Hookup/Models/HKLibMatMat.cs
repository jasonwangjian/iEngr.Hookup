
namespace iEngr.Hookup.Models
{
    public class HKLibMatMat : IIdentifiable
    {
        public string ID { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public string ActiveCode { get; set; }
        public int SortNum { get; set; }
        public string Name
        {
            get => (HK_General.intLan == 2) ? NameEn : NameCn;
        }
    }
}
