using iEngr.Hookup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace iEngr.Hookup.ViewModels
{
    public class MatListViewModel : INotifyPropertyChanged
    {
        public MatListViewModel()
        {
            HK_General = new HK_General();
            MatList = new ObservableCollection<HKMatGenLib>();
            MouseDoubleClickCommand = new RelayCommand<KeyEventArgs>(HandleMouseDoubleClick);
        }
        private HK_General HK_General;

        private ObservableCollection<HKMatGenLib> _matList;
        private HKMatGenLib _selectedMat;
        private ObservableCollection<HKMatGenLib> _matItemsSelected;

        public ObservableCollection<HKMatGenLib> MatList
        {
            get => _matList;
            set => SetField(ref _matList, value);
        }
        public HKMatGenLib SelectedMat
        {
            get => _selectedMat;
            set => SetField(ref _selectedMat, value);
        }
        public ObservableCollection<HKMatGenLib> MatItemsSelected
        {
            get => _matItemsSelected;
            set => SetField(ref _matItemsSelected, value);
        }

        private string _matDataFromQuery;
        public string MatDataFromQuery
        {
            get => _matDataFromQuery;
            set => SetField(ref _matDataFromQuery, value);
        }
        private string _matDataToQuery;
        public string MatDataToQuery
        {
            get => _matDataToQuery;
            set
            {
                SetField(ref _matDataToQuery, value);
                Debug.WriteLine($"Receive: {value}");
                Debug.WriteLine($"Pause Receive: {getConditionString(value)}");
            }
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public ICommand MouseDoubleClickCommand { get; }
        private void HandleMouseDoubleClick(KeyEventArgs e)
        {
        }

        // 0:CatID, 1:SubCatID, 2:TechSpecMain, 3:TechSpecAux, 4:TypeP1, 5:SizeP1, 6:TypeP2, 7:SizeP2, 8:NoreSpecCn, 9:NoreSpecEn, 10:RemarksCn, 11: RemarksEn, 12, PClass, 13:MatSpec, 14,Status

        private string getConditionString(string matData) //大类和小类都不确定时，不予查询
        {
            var arrMatData = matData.Split(',').ToArray<string>();
            List<string> segs = new List<string>();
            if (!string.IsNullOrEmpty(arrMatData[0]) && !string.IsNullOrEmpty(arrMatData[1])) return null;
            if (!string.IsNullOrEmpty(arrMatData[0]))
                segs.Add($"mgl.CatID='{arrMatData[0]}'");
            if (!string.IsNullOrEmpty(arrMatData[1]))
                segs.Add($"mgl.SubCatID='{arrMatData[1]}'");
            var specMainValid = getUsefulSpec(arrMatData[2]);
            if (!string.IsNullOrEmpty(specMainValid))
                segs.Add($"mgl.TechSpecMain Like '%{specMainValid}%'");
            var specAuxValid = getUsefulSpec(arrMatData[3]);
            if (!string.IsNullOrEmpty(specAuxValid))
                segs.Add($"mgl.TechSpecMain Like '%{specAuxValid}%'");
            if (!string.IsNullOrEmpty(arrMatData[4]))
                segs.Add($"mgl.SubCatID='{arrMatData[4]}'");
            return segs.Count>0? $"WHERE {string.Join(" AND ", segs)}":null;
        }

        private string getUsefulSpec(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            return string.Join(",", input.Split('|')?.Where(x => !string.IsNullOrEmpty(x.Split(':')[1])).ToList());
        }
    }
}
