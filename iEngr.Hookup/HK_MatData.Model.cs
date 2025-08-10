using iEngr.Hookup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace iEngr.Hookup
{
    public class HKMatData : INotifyPropertyChanged
    {
        public HKMatData()
        {
            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AlterCode))
                {
                    if (s is HKMatData instance)
                    {
                        string currentValue = instance.AlterCode;
                        if (currentValue == "AS1")
                        {
                        }
                    }
                }
            };
        }
        private string _MainCatID;
        public string MainCatID
        {
            get => _MainCatID;
            set
            {
                if (_MainCatID != value)
                {
                    _MainCatID = value;
                    OnPropertyChanged(nameof(MainCatID));
                }
            }
        }
        private string _SubCatID;
        public string SubCatID
        {
            get => _SubCatID;
            set
            {
                if (_SubCatID != value)
                {
                    _SubCatID = value;
                    OnPropertyChanged(nameof(SubCatID));
                }
            }
        }
        private string _TechSpecMain;
        public string TechSpecMain
        {
            get => _TechSpecMain;
            set
            {
                if (_TechSpecMain != value)
                {
                    _TechSpecMain = value;
                    OnPropertyChanged(nameof(TechSpecMain));
                }
            }
        }
        private string _TechSpecAux;
        public string TechSpecAux
        {
            get => _TechSpecAux;
            set
            {
                if (_TechSpecAux != value)
                {
                    _TechSpecAux = value;
                    OnPropertyChanged(nameof(TechSpecAux));
                }
            }
        }
        private string _TypeAllP1;
        public string TypeAllP1
        {
            get => _TypeAllP1;
            set
            {
                if (_TypeAllP1 != value)
                {
                    _TypeAllP1 = value;
                    OnPropertyChanged(nameof(TypeAllP1));
                }
            }
        }
        public string TypeP1 { get; set; }
        public string SizeP1 { get; set; }
        public string TypeP2 { get; set; }
        public string SizeP2 { get; set; }
        private string _TypeAllP2;
        public string TypeAllP2
        {
            get => _TypeAllP2;
            set
            {
                if (_TypeAllP2 != value)
                {
                    _TypeAllP2 = value;
                    OnPropertyChanged(nameof(TypeAllP2));
                }
            }
        }
        private string _MatMatAll;
        public string MatMatAll
        {
            get => _MatMatAll;
            set
            {
                if (_MatMatAll != value)
                {
                    _MatMatAll = value;
                    OnPropertyChanged(nameof(MatMatAll));
                }
            }
        }
        private string _MoreSpecCn;
        public string MoreSpecCn
        {
            get => _MoreSpecCn;
            set
            {
                if (_MoreSpecCn != value)
                {
                    _MoreSpecCn = value;
                    OnPropertyChanged(nameof(MoreSpecCn));
                }
            }
        }
        private string _MoreSpecEn;
        public string MoreSpecEn
        {
            get => _MoreSpecEn;
            set
            {
                if (_MoreSpecEn != value)
                {
                    _MoreSpecEn = value;
                    OnPropertyChanged(nameof(MoreSpecEn));
                }
            }
        }
        private string _RemarksCn;
        public string RemarksCn
        {
            get => _RemarksCn;
            set
            {
                if (_RemarksCn != value)
                {
                    _RemarksCn = value;
                    OnPropertyChanged(nameof(RemarksCn));
                }
            }
        }
        private string _RemarksEn;
        public string RemarksEn
        {
            get => _RemarksEn;
            set
            {
                if (_RemarksEn != value)
                {
                    _RemarksEn = value;
                    OnPropertyChanged(nameof(RemarksEn));
                }
            }
        }
        private string _AlterCode;
        public string AlterCode
        {
            get => _AlterCode;
            set
            {
                if (_AlterCode != value)
                {
                    _AlterCode = value;
                    OnPropertyChanged(nameof(AlterCode));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
