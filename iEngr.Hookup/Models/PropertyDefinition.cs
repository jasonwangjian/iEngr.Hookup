using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace iEngr.Hookup.Models
{
    public class PropertyDefinition : INotifyPropertyChanged
    {
        public PropertyDefinition()
        {
            SelectedItems = new ObservableCollection<GeneralItem>();
        }
        public string Key { get; set; }
        public int SortNum { get; set; }
        public string DisplayNameCn { get; set; }
        public string DisplayNameEn { get; set; }
        public string DisplayName
        {
            get => (HK_General.ProjLanguage == 2) ? DisplayNameEn : DisplayNameCn;
        }
        public string RemarksCn { get; set; }
        public string RemarksEn { get; set; }
        public string Remarks
        {
            get => (HK_General.ProjLanguage == 2) ? RemarksEn : RemarksCn;
        }

        public PropertyType Type { get; set; }
        public string Category { get; set; }
        public object DefaultValue { get; set; }
        public object Value { get; set; }
        public ObservableCollection<object> Options { get; set; }
        public ObservableCollection<GeneralItem> Items { get; set; }
        private GeneralItem _selectedItem;
        public GeneralItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<GeneralItem> _selectedItems;
        public ObservableCollection<GeneralItem> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;
                OnPropertyChanged();
            }
        }
        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public enum PropertyType
    {
        String,
        Integer,
        Double,
        Boolean,
        DateTime,
        Enum,
        EnumItem,
        EnumItems // 多选
    }
}