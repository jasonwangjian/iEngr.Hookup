//using static iEngr.Hookup.ViewModels.MatDataViewModel;

namespace iEngr.Hookup.Models
{
    public class CmbItem// : IIdentifiable
    {
        public string ID { get; set; }
        public string Comp { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string Class { get; set; }
        public string Link { get; set; }

    }
    public class GeneralItem
    {
        public string Code { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
    }
}
