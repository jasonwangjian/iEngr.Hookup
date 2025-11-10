namespace iEngr.Hookup.Models
{
    public class HKLibThread
    {
        public string ID { get; set; }
        public string Class { get; set; }
        public string SubClass { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public decimal Value { get; set; }
        public decimal Pitch { get; set; }
        public int? Qty { get; set; }
        public string ClassEx { get; set; }
        public int SortNum { get; set; }
        public string SizeCode { get; set; }
    }
    public class HKLibThreadSize
    {
        public string ID { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public int SortNum { get; set; }
        public int Status { get; set; }
    }
}
