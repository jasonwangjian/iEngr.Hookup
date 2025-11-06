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
        HkTreeViewModel VmTree;
        HkPictureViewModel VmPicture;
        DiagItemsViewModel VmDiagLib;
        AppliedNodeViewModel VmAppliedLib;
        BomItemsViewModel VmBomLib;
        PropLabelViewModel VmLabel;
        MatMainViewModel VmMatLib;

        public UcDiagLibMgr()
        {
            InitializeComponent();

            VmTree = ucTree.DataContext as HkTreeViewModel;
            VmTree.TreeItemChanged += OnTreeItemChanged;
            VmTree.DiagramIDsChanged += OnDiagramIDsChanged;
            VmTree.PropLabelItemsChanged += OnNodeLabelItemsChanged;

            //(ucTree.DataContext as HkTreeViewModel).DiagramIDAdded += OnDiagramIDAdded;
            VmPicture = ucPic.DataContext as HkPictureViewModel;
            VmDiagLib = ucDiag.DataContext as DiagItemsViewModel;
            VmDiagLib.LibDiagramChanged += OnLibDiagramChanged;
            VmDiagLib.PicturePathChanged += OnPicturePathChanged;
            VmDiagLib.AvailableDiagramItems = HK_General.GetDiagramItems();
            VmDiagLib.AssignedDiagramItems = new ObservableCollection<DiagramItem>();

            VmAppliedLib = ucNode.DataContext as AppliedNodeViewModel;
            VmAppliedLib.NodeIDHighlighted += OnNodeIDHighlighted;

            VmMatLib = ucMatLib.DataContext as MatMainViewModel;
            VmMatLib.VmMatList.MatListItemChanged += OmMatListItemChanged;

            VmBomLib = ucBomLib.DataContext as BomItemsViewModel;

            VmLabel = ucProp.DataContext as PropLabelViewModel;
            VmLabel.IsCompared = false;
        }
        //更新UcHkPicture
        private void OnPicturePathChanged(object sender, DiagramItem value)
        {
            VmPicture.PicturePath = value?.PicturePath;
        }
        //更新UcPropLabel
        private void OnNodeLabelItemsChanged(object sender, HkTreeItem value)
        {
            (ucProp.DataContext as PropLabelViewModel).LabelItems = HK_General.GetPropLabelItems(value);
        }
        //更新UcDiagGrid2
        private void OnDiagramIDsChanged(object sender, HkTreeItem value)
        {
            VmDiagLib.AssignedDiagramItems.Clear();
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
            foreach (var item in VmDiagLib.AvailableDiagramItems)
            {
                if (ids != null && ids.Contains(item.ID))
                {
                    item.IsOwned = true;
                    item.IsInherit = isInherit;
                    VmDiagLib.AssignedDiagramItems.Add(item);
                }
                else
                {
                    item.IsOwned = false;
                }
            }
            VmDiagLib.FilterText = VmDiagLib.FilterText;
            if (VmDiagLib.AssignedDiagramItems.Count > 0)
            { VmDiagLib.AssignedDiagramsSelectedItem = VmDiagLib.FilteredAssignedDiagramItems.FirstOrDefault(); }

        }
        private void OnDiagramIDAdded(object sender, HkTreeItem value)
        {
            //刷新NodeDiagramItems
            //ObservableCollection<DiagramItem> diagramItems = HK_General.GetDiagramItems(value.DiagID, true, false);
            //(ucDiag.DataContext as DiagGrid2ViewModel).NodeDiagramItems = diagramItems;
            (ucDiag.DataContext as DiagGrid2ViewModel).NodeDiagramItems.Clear();
            List<int> ids = value.DiagID?.Split(',')
                       .Select(s => s.Trim())  // 去除空格
                       .Where(s => int.TryParse(s, out _))
                       .Select(int.Parse)
                       .ToList();

            //刷新LibDiagramItems
            (ucDiag.DataContext as DiagGrid2ViewModel).LibDiagramItems = HK_General.GetDiagramItems();
            //List<int> ids = diagramItems.Select(x => x.ID).ToList();
            foreach (var item in (ucDiag.DataContext as DiagGrid2ViewModel).LibDiagramItems)
            {
                if (ids != null && ids.Contains(item.ID))
                {
                    item.IsOwned = true;
                    item.IsInherit = false;
                    (ucDiag.DataContext as DiagGrid2ViewModel).NodeDiagramItems.Add(item);
                }
                else
                {
                    item.IsOwned = false;
                }
            }
        }
        private void OnLibDiagramChanged(object sender, DiagramItem value)
        {
            string id = value.ID.ToString();
            ObservableCollection<AppliedNodeItem> nodeItems = new ObservableCollection<AppliedNodeItem>();
            if (!(string.IsNullOrEmpty(id)))
            {
                foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
                {
                    GetNoteItemsRecursive(item, id, nodeItems);
                }
            }
            VmAppliedLib.AppliedItems = nodeItems;
            if (VmBomLib.SelectedDiagramItem != VmDiagLib.SelectedItem) VmBomLib.SelectedDiagramItem = VmDiagLib.SelectedItem;
            VmBomLib.DataSource = HK_General.GetDiagBomItems(id);
        }
        private ObservableCollection<AppliedNodeItem> GetNoteItemsRecursive(HkTreeItem item, string diagID, ObservableCollection<AppliedNodeItem> nodeItems)
        {
            if (item == null) return nodeItems;
            if (!string.IsNullOrEmpty(item.DiagID))
            {
                if (item.DiagID.Split(',').Contains(diagID))
                {
                    nodeItems.Add(new AppliedNodeItem
                    {
                        NodeID = item.ID,
                        DisplayName = item.DisPlayName,
                        IsInherit = false
                    });
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(item.InheritDiagID) && item.InheritDiagID.Split(',').Contains(diagID))
                {
                    nodeItems.Add(new AppliedNodeItem
                    {
                        NodeID = item.ID,
                        DisplayName = item.DisPlayName,
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
        private void OnNodeIDHighlighted(object sender, AppliedNodeItem value)
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
            VmDiagLib.FocusedNode = value;
            OnNodeLabelItemsChanged(sender, value);
            OnDiagramIDsChanged(sender, value);
        }
        //更新UcBomList
        private void OmMatListItemChanged(object sender, MatListItem value)
        {
            VmBomLib.SelectedMatListItem = value;
        }

    }
}
