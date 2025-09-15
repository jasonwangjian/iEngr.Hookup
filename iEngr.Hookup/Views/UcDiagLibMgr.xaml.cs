using iEngr.Hookup.Models;
using iEngr.Hookup.Services;
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
    /// UcDiagLibMgr.xaml 的交互逻辑
    /// </summary>
    public partial class UcDiagLibMgr : UserControl
    {
        public UcDiagLibMgr()
        {
            InitializeComponent();

            (ucTree.DataContext as HkTreeViewModel).PicturePathChanged += OnPicturePathChanged;
            (ucTree.DataContext as HkTreeViewModel).PropLabelItemsChanged += OnPropLabelItemsChanged;
            (ucTree.DataContext as HkTreeViewModel).TreeItemChanged += OnTreeItemChanged;
            (ucDiag.DataContext as DiagGrid2ViewModel).PicturePathChanged += OnPicturePathChanged;
            (ucDiag.DataContext as DiagGrid2ViewModel).LibDiagramItems = HK_General.GetDiagramItems();
        }
        private void OnPicturePathChanged(object sender, string value)
        {
            (ucPic.DataContext as HkPictureViewModel).PicturePath = value;

        }
        private void OnPropLabelItemsChanged(object sender, HkTreeItem value)
        {
            if (value != null)
            {
                (ucProp.DataContext as PropLabelViewModel).PropLabelItems = HK_General.GetPropLabelItems(value);
            }
        }
        private void OnTreeItemChanged(object sender, HkTreeItem value)
        {
            (ucDiag.DataContext as DiagGrid2ViewModel).FocusedNode = value;
            if (value != null)
            {
                string diagIDs = string.IsNullOrEmpty(value.DiagID) ? value.InheritDiagID : value.DiagID;
                if (string.IsNullOrEmpty(diagIDs)) return;
                ObservableCollection<DiagramItem> diagramItems = HK_General.GetDiagramItems(diagIDs, true);
                (ucDiag.DataContext as DiagGrid2ViewModel).NodeDiagramItems = diagramItems;
                List<string> ids = diagIDs.Split(',').ToList();
                foreach( var item in (ucDiag.DataContext as DiagGrid2ViewModel).LibDiagramItems)
                {
                    item.IsOwned = ids.Contains(item.ID.ToString());
                }

            }
        }
        //private void OnTreeItemChanged(object sender, HkTreeItem value)
        //{
        //    if (value != null)
        //    {
        //        string diagIDs = string.IsNullOrEmpty(value.DiagID) ? value.InheritDiagID : value.DiagID;
        //        ObservableCollection<DiagramItem> diagramItems = HK_General.GetDiagramItems(diagIDs, true);
        //        List<string> ids = new List<string>();
        //        foreach (var child in value.Children)
        //        {
        //            ids = GetDiagIDRecursive(child,ids);
        //        }
        //        diagIDs = string.Join(",", ids.Where(x => !string.IsNullOrEmpty(x)).Distinct());
        //        ObservableCollection<DiagramItem> diagramItemsChild = HK_General.GetDiagramItems(diagIDs, false);

        //        diagramItems.AddRange(diagramItemsChild);
        //        (ucDiag.DataContext as DiagGridViewModel).DiagramItems = diagramItems;
        //    }
        //}
        //private List<string> GetDiagIDRecursive(HkTreeItem item, List<string> ids)
        //{
        //    if (item == null) return ids;
        //    ids.Add(item.DiagID);
        //    foreach (var child in item.Children)
        //    {
        //        GetDiagIDRecursive(child, ids);
        //    }
        //    return ids;
        //}
    }
}
