//using static iEngr.Hookup.ViewModels.MatDataViewModel;

namespace iEngr.Hookup.Models
{
    public class HKLibMatName: IIdentifiable
    {
        public string ID { get; set; }
        public string CatID { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public string Remarks { get; set; }
        public int  SortNum { get; set; }
        public string TypeP1 { get; set; }
        public string TypeP2 { get; set; }
        public string TechSpecMain{ get; set; }
        public string TechSpecAux { get; set; }
        public string Qty { get; set; }
        public string Unit { get; set; }
        public string SupDisc { get; set; }
        public string SupResp { get; set; }
        public string ErecDisc { get; set; }
        public string ErecResp { get; set; }
        public string Name
        {
            get => (HK_General.intLan == 2) ? SpecEn : SpecCn;
        }
    }
}
