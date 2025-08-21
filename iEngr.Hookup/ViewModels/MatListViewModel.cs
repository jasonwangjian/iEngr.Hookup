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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace iEngr.Hookup.ViewModels
{
    public class MatListViewModel : INotifyPropertyChanged
    {
        public MatListViewModel()
        {
            HK_General = new HK_General();
            MatList = new ObservableCollection<HKMatGenLib>();
            MatItemsSelected = new ObservableCollection<HKMatGenLib>();
            MouseDoubleClickCommand = new RelayCommand<MouseButtonEventArgs>(HandleMouseDoubleClick);
            SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(HandleSelectionChanged);
            QueryCommand = new RelayCommand<object>(_ => Query(),_ => AutoQueryEnable == false);
            NewAddCommand = new RelayCommand<object>(_ => NewAdd(), _ => CountExistingData == 0);
            UpdateCommand = new RelayCommand<object>(_ => Update(), _ => SelectedMat != null && CountExistingData == 0);
            DeleteCommand = new RelayCommand<object>(_ => Delete(), _ => MatItemsSelected?.Count > 0);
            AutoQueryEnable = true;
        }
        private HK_General HK_General;
        // 确保命令支持刷新

        private int _countExistingData;
        public int CountExistingData
        {
            get => _countExistingData;
            set 
            {
                if (SetField(ref _countExistingData, value))
                {
                    // 关键：手动触发命令状态更新
                    NewAddCommand.RaiseCanExecuteChanged();
                    UpdateCommand.RaiseCanExecuteChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<HKMatGenLib> MatItemsSelected;
        private ObservableCollection<HKMatGenLib> _matList;
        private HKMatGenLib _selectedMat;

        private bool _autoQueryEnable;
        public bool AutoQueryEnable
        {
            get => _autoQueryEnable;
            set
            {
                SetField(ref _autoQueryEnable, value);
                if (value)
                {
                    MatList = HK_General.UpdateQueryResult(getConditionLike(MatDataToQuery));
                }
            }
        }
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

        private string _matDataFromQuery;
        public string MatDataFromQuery
        {
            get => _matDataFromQuery;
            set=> SetField(ref _matDataFromQuery, value);
        }
        private string _matDataToQuery;
        public string MatDataToQuery
        {
            get => _matDataToQuery;
            set
            {
                SetField(ref _matDataToQuery, value);
                //Debug.WriteLine($"Receive: {value}");
                //Debug.WriteLine($"Pause Receive: {getConditionString(value)}");
                //Debug.WriteLine($"getConditionEqual: {getConditionEqual(MatDataToQuery)}");

                CountExistingData = HK_General.CountExistingData(getConditionEqual(MatDataToQuery));

                if (AutoQueryEnable)
                {
                    MatList = HK_General.UpdateQueryResult(getConditionLike(value));
                }
            }
        }
        public string BtnCommand
        {
            set
            {
                int countData=0;
                switch(value)
                {
                    case "Query":
                        MatList = HK_General.UpdateQueryResult(getConditionLike(MatDataToQuery));
                        break;
                    case "NewAdd":
                        if (HK_General.CountExistingData(getConditionEqual(MatDataToQuery)) !=0) return;
                        if (HK_General.NewDataAdd(MatDataToQuery, out int newID) == 1)
                        {
                            countData = 1;
                            MessageBox.Show($"{countData} 条数据已增加！");
                            MatList.Add(HK_General.UpdateQueryResult(newID));
                        }
                        break;
                    case "Update":
                        if (HK_General.CountExistingData(getConditionEqual(MatDataToQuery)) != 0) return;
                        if (HK_General.DataUpdate(SelectedMat.ID, MatDataToQuery) == 1)
                        {
                            countData = 1;
                            SelectedMat.Update(HK_General.UpdateQueryResult(SelectedMat.ID));
                            MessageBox.Show($"所选数据已更新！");
                            CountExistingData = 1;
                        }
                        break;
                    case "Delete":
                        for (int i = MatItemsSelected.Count-1; i>=0; i--)
                        {
                            countData += HK_General.DataDel(MatItemsSelected[i].ID);
                            MatList.Remove(MatItemsSelected[i]);
                        }
                        MessageBox.Show($"{countData} 条数据已删除！");
                        break;
                }
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
        public RelayCommand<MouseButtonEventArgs> MouseDoubleClickCommand { get; }
        private void HandleMouseDoubleClick(MouseButtonEventArgs e)
        {
            var selectedMat = (e.Source as DataGrid)?.SelectedItem as HKMatGenLib;
            if (selectedMat == null) return;
            // 0:CatID, 1:SubCatID, 2:TechSpecMain, 3:TechSpecAux, 4:TypeP1, 5:SizeP1, 6:TypeP2, 7:SizeP2, 8:MoreSpecCn, 9:MoreSpecEn, 10:RemarksCn, 11: RemarksEn, 12, PClass, 13:MatSpec, 14,Status
            MatDataFromQuery = $"{selectedMat.CatID}," +
                            $"{selectedMat.SubCatID}," +
                            $"{selectedMat.TechSpecMain.Replace(',','|')}," +
                            $"{selectedMat.TechSpecAux.Replace(',', '|')}," +
                            $"{selectedMat.TypeP1}," +
                            $"{selectedMat.SizeP1}," +
                            $"{selectedMat.TypeP2}," +
                            $"{selectedMat.SizeP2}," +
                            $"{selectedMat.MoreSpecCn}," +
                            $"{selectedMat.MoreSpecEn}," +
                            $"{selectedMat.RemarksCn}," +
                            $"{selectedMat.RemarksEn}," +
                            $",{selectedMat.MatMatID},,";
        }
        public RelayCommand<SelectionChangedEventArgs> SelectionChangedCommand { get; }
        private void HandleSelectionChanged(SelectionChangedEventArgs e)
        {
            var selectedItems = (e.Source as DataGrid)?.SelectedItems;
            if (selectedItems != null)
            {
                ObservableCollection<HKMatGenLib> matItemsSelected = new ObservableCollection<HKMatGenLib>();
                foreach (var item in selectedItems)
                {
                    matItemsSelected.Add(item as HKMatGenLib);
                }
                MatItemsSelected = matItemsSelected;
            }

        }

        // 0:CatID, 1:SubCatID, 2:TechSpecMain, 3:TechSpecAux, 4:TypeP1, 5:SizeP1, 6:TypeP2, 7:SizeP2, 8:MoreSpecCn, 9:MoreSpecEn, 10:RemarksCn, 11: RemarksEn, 12, PClass, 13:MatSpec, 14,Status

        private string getConditionLike(string matData) //大类和小类都不确定时，不予查询
        {
            if (matData == null) return null;
            var arrMatData = matData.Split(',').ToArray<string>();
            List<string> segs = new List<string>();
            if (string.IsNullOrEmpty(arrMatData[0]) && string.IsNullOrEmpty(arrMatData[1])) return null;
            if (!string.IsNullOrEmpty(arrMatData[0]))
                segs.Add($"mgl.CatID='{arrMatData[0]}'");
            if (!string.IsNullOrEmpty(arrMatData[1]))
                segs.Add($"mgl.SubCatID='{arrMatData[1]}'");
            if (!string.IsNullOrEmpty(arrMatData[2]))
                segs.Add(getSpecConditionLike(arrMatData[2], "mgl.TechSpecMain"));
            if (!string.IsNullOrEmpty(arrMatData[3]))
                segs.Add(getSpecConditionLike(arrMatData[3], "mgl.TechSpecAux"));
            //var specMainValid = getUsefulSpec(arrMatData[2]);
            //if (!string.IsNullOrEmpty(specMainValid))
            //    segs.Add($"mgl.TechSpecMain Like '%{specMainValid}%'");
            //var specAuxValid = getUsefulSpec(arrMatData[3]);
            //if (!string.IsNullOrEmpty(specAuxValid))
            //    segs.Add($"mgl.TechSpecMain Like '%{specAuxValid}%'");
            if (!string.IsNullOrEmpty(arrMatData[4]))
                segs.Add($"mgl.TypeP1='{arrMatData[4]}'");
            if (!string.IsNullOrEmpty(arrMatData[5]))
                segs.Add($"mgl.SizeP1='{arrMatData[5]}'");
            if (!string.IsNullOrEmpty(arrMatData[6]))
                segs.Add($"mgl.TypeP2='{arrMatData[6]}'");
            if (!string.IsNullOrEmpty(arrMatData[7]))
                segs.Add($"mgl.SizeP2='{arrMatData[7]}'");
            if (!string.IsNullOrEmpty(arrMatData[8]))
                segs.Add($"mgl.MoreSpecCn Like '%{arrMatData[8]}%'");
            if (!string.IsNullOrEmpty(arrMatData[9]))
                segs.Add($"mgl.MoreSpecEn Like '%{arrMatData[9]}%'");
            if (!string.IsNullOrEmpty(arrMatData[10]))
                segs.Add($"mgl.RemarksCn Like '%{arrMatData[10]}%'");
            if (!string.IsNullOrEmpty(arrMatData[11]))
                segs.Add($"mgl.RemarksEn Like '%{arrMatData[11]}%'");
            if (!string.IsNullOrEmpty(arrMatData[13]))
                segs.Add($"mgl.MatSpec='{arrMatData[13]}'");
            segs.Add($"mgl.Status & {255} > 0");
            return segs.Count>0? $"WHERE {string.Join(" AND ", segs)}":null;
        }
        private string getConditionEqual(string matData) //大类不确定时，不予查询
        {
            if (matData == null) return null;
            var arrMatData = matData.Split(',').ToArray<string>();
            List<string> segs = new List<string>();
            if (string.IsNullOrEmpty(arrMatData[1])) return null;
            segs.Add($"mgl.SubCatID='{arrMatData[1]}'");
            segs.Add(getSpecConditionEqual(arrMatData[2], "mgl.TechSpecMain"));
            segs.Add(getSpecConditionEqual(arrMatData[3], "mgl.TechSpecAux"));
            segs.Add((!string.IsNullOrEmpty(arrMatData[4])) ? $"mgl.TypeP1='{arrMatData[4]}'" : $"(mgl.TypeP1 = '' OR mgl.TypeP1 IS NULL)");
            segs.Add((!string.IsNullOrEmpty(arrMatData[5])) ? $"mgl.SizeP1='{arrMatData[5]}'" : $"(mgl.SizeP1 = '' OR mgl.SizeP1 IS NULL)");
            segs.Add((!string.IsNullOrEmpty(arrMatData[6])) ? $"mgl.TypeP2='{arrMatData[6]}'" : $"(mgl.TypeP2 = '' OR mgl.TypeP2 IS NULL)");
            segs.Add((!string.IsNullOrEmpty(arrMatData[7])) ? $"mgl.SizeP2='{arrMatData[7]}'" : $"(mgl.SizeP2 = '' OR mgl.SizeP2 IS NULL)");
            segs.Add((!string.IsNullOrEmpty(arrMatData[8])) ? $"mgl.MoreSpecCn='{arrMatData[8]}'" : $"(mgl.MoreSpecCn = '' OR mgl.MoreSpecCn IS NULL)");
            segs.Add((!string.IsNullOrEmpty(arrMatData[9])) ? $"mgl.MoreSpecEn='{arrMatData[9]}'" : $"(mgl.MoreSpecEn = '' OR mgl.MoreSpecEn IS NULL)");
            segs.Add((!string.IsNullOrEmpty(arrMatData[10])) ? $"mgl.RemarksCn='{arrMatData[10]}'" : $"(mgl.RemarksCn = '' OR mgl.RemarksCn IS NULL)");
            segs.Add((!string.IsNullOrEmpty(arrMatData[11])) ? $"mgl.RemarksEn='{arrMatData[11]}'" : $"(mgl.RemarksEn = '' OR mgl.RemarksEn IS NULL)");
            segs.Add($"mgl.Status & {255} > 0");
            return segs.Count > 0 ? $"WHERE {string.Join(" AND ", segs)}" : null;
        }

        private string getSpecConditionEqual(string input, string fieldName)
        {
            if (string.IsNullOrEmpty(input)) return $"({fieldName} = '' OR {fieldName} IS NULL)";
            return $"{fieldName} = '{string.Join(",", input.Split('|')?.ToList())}'";
            //return string.Join(",", input.Split('|')?.Where(x => !string.IsNullOrEmpty(x.Split(':')[1])).ToList());
        }

        private string getSpecConditionLike(string input, string fieldName)
        {
            if (string.IsNullOrEmpty(input)) return null;
            return string.Join(" AND ", input.Split('|')?.Select(x=> fieldName + " LIKE '%" + x + "%'").ToList());
            //return string.Join(",", input.Split('|')?.Where(x => !string.IsNullOrEmpty(x.Split(':')[1])).ToList());
        }

        public RelayCommand<object> QueryCommand { get; }
        private void Query()
        {
            BtnCommand = "Query";
        }
        public RelayCommand<object> NewAddCommand { get; }
        private void NewAdd()
        {
            BtnCommand = "NewAdd";
        }
        public RelayCommand<object> UpdateCommand { get; }
        private void Update()
        {
            BtnCommand = "Update";
        }
        public RelayCommand<object> DeleteCommand { get; }
        private void Delete()
        {
            BtnCommand = "Delete";
        }
    }
}
