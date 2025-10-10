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
            (ucTree.DataContext as HkTreeViewModel).DiagramIDsChanged += OnDiagramIDsChanged;
            (ucTree.DataContext as HkTreeViewModel).DiagramIDAdded += OnDiagramIDAdded;
            (ucTree.DataContext as HkTreeViewModel).TreeItemChanged += OnTreeItemChanged;
            (ucDiag.DataContext as DiagGrid2ViewModel).PicturePathChanged += OnPicturePathChanged;
            (ucDiag.DataContext as DiagGrid2ViewModel).DiagramIDChanged += OnDiagramIDChanged;
            (ucNode.DataContext as NodeAppliedViewModel).NodeIDHighlighted += OnNodeIDHighlighted;
            (ucMatLib.DataContext as MatMainViewModel).VmMatList.MatListItemChanged += OmMatListItemChanged;
            (ucDiag.DataContext as DiagGrid2ViewModel).LibDiagramItems = HK_General.GetDiagramItems();
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
        //更新UcDiagGrid2
        private void OnDiagramIDsChanged(object sender, HkTreeItem value)
        {
            //刷新NodeDiagramItems
            ObservableCollection<DiagramItem> diagramItems = HK_General.GetDiagramItems(value.DiagID, true,false);
            if (value.DiagID == null) diagramItems = HK_General.GetDiagramItems(value.InheritDiagID, true, true);
            (ucDiag.DataContext as DiagGrid2ViewModel).NodeDiagramItems = diagramItems;
            if (diagramItems.Count > 0) { (ucDiag.DataContext as DiagGrid2ViewModel).NodeSelectedItem = diagramItems.FirstOrDefault(); }
            //修正LibDiagramItems.IsOwned
             List<int> ids = diagramItems.Select(x=> x.ID).ToList();
            foreach (var item in (ucDiag.DataContext as DiagGrid2ViewModel).LibDiagramItems)
            {
                item.IsOwned = ids.Contains(item.ID);
            }
        }
        private void OnDiagramIDAdded(object sender, HkTreeItem value)
        {
            //刷新NodeDiagramItems
            ObservableCollection<DiagramItem> diagramItems = HK_General.GetDiagramItems(value.DiagID, true, false);
            (ucDiag.DataContext as DiagGrid2ViewModel).NodeDiagramItems = diagramItems;
            //刷新LibDiagramItems
            (ucDiag.DataContext as DiagGrid2ViewModel).LibDiagramItems = HK_General.GetDiagramItems();
            List<int> ids = diagramItems.Select(x => x.ID).ToList();
            foreach (var item in (ucDiag.DataContext as DiagGrid2ViewModel).LibDiagramItems)
            {
                item.IsOwned = ids.Contains(item.ID);
            }
        }
        //更新UcNodeApplied
        private void OnDiagramIDChanged(object sender, string value)
        {
            ObservableCollection<NodeItem> nodeItems = new ObservableCollection<NodeItem>();
            if (!(string.IsNullOrEmpty(value)))
            {
                foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
                {
                    GetNoteItemsRecursive(item,value,nodeItems);
                }
            }
            (ucNode.DataContext as NodeAppliedViewModel).AppliedNodeItems = nodeItems;
            //(ucBomLib.DataContext as BomListViewModel).SelectedDiagramItem = HK_General.GetDiagramItem(value);
            (ucBomLib.DataContext as BomListViewModel).SelectedDiagramItem = (ucDiag.DataContext as DiagGrid2ViewModel).SelectedItem;
            (ucBomLib.DataContext as BomListViewModel).DataSource = HK_General.GetDiagBomItems(value);
        }
        private ObservableCollection<NodeItem> GetNoteItemsRecursive(HkTreeItem item, string diagID, ObservableCollection<NodeItem> nodeItems)
        {
            if (item == null) return nodeItems;
            if (!string.IsNullOrEmpty(item.DiagID))
            {
                if (item.DiagID.Split(',').Contains(diagID))
                {
                    nodeItems.Add(new NodeItem
                    {
                        NodeID = item.ID,
                        DisPlayName = item.DisPlayName,
                        IsInherit = false
                    });
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(item.InheritDiagID) && item.InheritDiagID.Split(',').Contains(diagID))
                {
                    nodeItems.Add(new NodeItem
                    {
                        NodeID = item.ID,
                        DisPlayName = item.DisPlayName,
                        IsInherit = true
                    });
                }
            }
            foreach (var child in item.Children)
            {
                GetNoteItemsRecursive(child, diagID, nodeItems);
            }
            return nodeItems;
        }
        private void OnNodeIDHighlighted(object sender, NodeItem value)
        {
            
            if (!string.IsNullOrEmpty(value?.NodeID))
            {
                foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
                {
                    HighlightNodeRecursive(item, value?.NodeID);
                }
            }
            else
            {
                foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
                {
                    HighlightNodeClearRecursive(item);
                }
            }
        }
        private void HighlightNodeRecursive(HkTreeItem item, string nodeId)
        {
            if (item == null || string.IsNullOrEmpty(nodeId)) return;
            if (item.ID == nodeId)
            {
                item.IsHighlighted = true;
                if (item.Parent != null)
                    item.Parent.IsExpanded = true;
            }
            else
                item.IsHighlighted = false;

           foreach (var child in item.Children)
            {
                HighlightNodeRecursive(child, nodeId);
            }
        }
        private void HighlightNodeClearRecursive(HkTreeItem item)
        {
            if (item == null) return;
           item.IsHighlighted = false;
            foreach (var child in item.Children)
            {
                HighlightNodeClearRecursive(child);
            }
        }

        private void OnTreeItemChanged(object sender, HkTreeItem value)
        {
            (ucDiag.DataContext as DiagGrid2ViewModel).FocusedNode = value;
            OnPicturePathChanged(sender, value?.PicturePath);
            OnPropLabelItemsChanged(sender, value);
            OnDiagramIDsChanged(sender, value);
        }
        //更新UcBomList
        private void OmMatListItemChanged(object sender, MatListItem value)
        {
            (ucBomLib.DataContext as BomListViewModel).SelectedMatListItem = value;
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
