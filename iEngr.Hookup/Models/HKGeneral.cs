//using static iEngr.Hookup.ViewModels.MatDataViewModel;

using iEngr.Hookup.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace iEngr.Hookup.Models
{
    public interface IIdentifiable
    {
        string ID { get; }
    }
    public interface IIntIdentifiable
    {
        int ID { get; }
    }
    public class MatDataCmbItem : IIdentifiable
    {
        public string ID { get; set; }
        public string Comp { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string Class { get; set; }
        public string Link { get; set; }
        public string Name
        {
            get => (HK_General.ProjLanguage == 2) ? NameEn : NameCn;
        }
    }
    public class GeneralItem
    {
        public string Code { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public string SpecCn { get; set; }
        public string SpecEn { get; set; }
        public string Name
        {
            get => (HK_General.ProjLanguage == 2) ? NameEn : NameCn;
        }
    }
}
