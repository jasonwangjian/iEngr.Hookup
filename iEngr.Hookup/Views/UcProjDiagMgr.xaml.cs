using iEngr.Hookup.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UcDiagMgr.xaml 的交互逻辑
    /// </summary>
    public partial class UcProjDiagMgr : UserControl
    {
        public UcProjDiagMgr()
        {
            InitializeComponent();
            (ucTree.DataContext as HkTreeViewModel).TreeItemChanged += OnTreeItemChanged;
            (ucTree.DataContext as HkTreeViewModel).DiagramIDsChanged += OnDiagramIDsChanged;
            (ucDiagLib.DataContext as DiagGrid2ViewModel).IsLangCtrlShown = false;
            (ucDiagLib.DataContext as DiagGrid2ViewModel).LibDiagramItems = HK_General.GetDiagramItems();
            (ucDiagLib.DataContext as DiagGrid2ViewModel).NodeDiagramItems = new ObservableCollection<DiagramItem>();
            (ucDiagComos.DataContext as DiagGridViewModel).DiagramItems = HK_General.GetDiagramItems();
        }
        //更新UcHkPicture
        private void OnPicturePathChanged(object sender, string value)
        {
            (ucPic.DataContext as HkPictureViewModel).PicturePath = value;
        }
        //更新UcPropLabel
        private void OnPropLabelItemsChanged(object sender, HkTreeItem value)
        {
            (ucProp.DataContext as PropLabelViewModel).PropLabelItems = HK_General.GetPropLabelItems(value);
        }

        private void OnTreeItemChanged(object sender, HkTreeItem value)
        {
            (ucDiagLib.DataContext as DiagGrid2ViewModel).FocusedNode = value;
            OnPicturePathChanged(sender, value?.PicturePath);
            OnPropLabelItemsChanged(sender, value);
            OnDiagramIDsChanged(sender, value);
        }
        private void OnDiagramIDsChanged(object sender, HkTreeItem value)
        {
            (ucDiagLib.DataContext as DiagGrid2ViewModel).NodeDiagramItems.Clear();
            bool isInherit = false;
            string diagIds = value.DiagID;
            if (string.IsNullOrEmpty(diagIds))
            {
                diagIds = value.InheritDiagID;
                isInherit = true;
            }
            List<int> ids = diagIds?.Split(',')
                                   .Select(s => s.Trim())  // 去除空格
                                   .Where(s => int.TryParse(s, out _))
                                   .Select(int.Parse)
                                   .ToList();
            //修正LibDiagramItems.IsOwned
            foreach (var item in (ucDiagLib.DataContext as DiagGrid2ViewModel).LibDiagramItems)
            {
                if (ids != null && ids.Contains(item.ID))
                {
                    item.IsOwned = true;
                    item.IsInherit = isInherit;
                    (ucDiagLib.DataContext as DiagGrid2ViewModel).NodeDiagramItems.Add(item);
                }
                else
                {
                    item.IsOwned = false;
                }
            }
            if ((ucDiagLib.DataContext as DiagGrid2ViewModel).NodeDiagramItems.Count > 0)
            { (ucDiagLib.DataContext as DiagGrid2ViewModel).NodeSelectedItem = (ucDiagLib.DataContext as DiagGrid2ViewModel).NodeDiagramItems.FirstOrDefault(); }
        }
    }
}
