using iEngr.Hookup.Models;
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
using System.Xml.Linq;

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
            (ucDiagLib.DataContext as DiagGrid2ViewModel).DiagramIDChanged += OnLibDiagramIDChanged;
            (ucDiagLib.DataContext as DiagGrid2ViewModel).PicturePathChanged += OnPicturePathChanged;
            (ucDiagLib.DataContext as DiagGrid2ViewModel).LibDiagramItems = HK_General.GetDiagramItems();
            (ucDiagLib.DataContext as DiagGrid2ViewModel).NodeDiagramItems = new ObservableCollection<DiagramItem>();
            (ucDiagComos.DataContext as DiagGridViewModel).DiagramIDChanged += OnComosDiagramIDChanged;
            (ucDiagComos.DataContext as DiagGridViewModel).PicturePathChanged += OnPicturePathChanged;
            (ucDiagComos.DataContext as DiagGridViewModel).DiagramItems = HK_General.GetDiagramItems();
            (ucNodes.DataContext as NodeAppliedViewModel).NodeIDHighlighted += OnNodeIDHighlighted;
            (ucDiagLib.DataContext as DiagGrid2ViewModel).IsLangCtrlShown = false;
            (ucBomLib.DataContext as BomListViewModel).IsLangCtrlShown = false;
            (ucBomLib.DataContext as BomListViewModel).IsButtonShown = false;
            (ucDiagComos.DataContext as DiagGridViewModel).IsLangCtrlShown = false;
            (ucBomComos.DataContext as BomListViewModel).IsLangCtrlShown = false;
            (ucBomComos.DataContext as BomListViewModel).IsButtonShown = false;
            ProjDiagMgrViewModel vmProjDiagMgr = new ProjDiagMgrViewModel();
            DataContext = vmProjDiagMgr;
            vmProjDiagMgr.LangInChineseChanged += OnLangInChineseChanged;
            vmProjDiagMgr.LangInEnglishChanged += OnLangInEnglishChanged;
        }
        private void OnLangInChineseChanged(object sender, bool value)
        {
            (ucDiagComos.DataContext as DiagGridViewModel).LangInChinese = value;
            (ucDiagLib.DataContext as DiagGrid2ViewModel).LangInChinese = value;
            (ucBomComos.DataContext as BomListViewModel).LangInChinese = value;
            (ucBomLib.DataContext as BomListViewModel).LangInChinese = value;
        }
        private void OnLangInEnglishChanged(object sender, bool value)
        {
            (ucDiagComos.DataContext as DiagGridViewModel).LangInEnglish = value;
            (ucDiagLib.DataContext as DiagGrid2ViewModel).LangInEnglish = value;
            (ucBomComos.DataContext as BomListViewModel).LangInEnglish = value;
            (ucBomLib.DataContext as BomListViewModel).LangInEnglish = value;
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
        private void OnComosDiagramIDChanged(object sender, string value)
        {
            (ucBomComos.DataContext as BomListViewModel).DataSource = HK_General.GetDiagBomItems(value);
        }
        //更新UcNodeApplied
        private void OnLibDiagramIDChanged(object sender, string value)
        {
            ObservableCollection<NodeItem> nodeItems = new ObservableCollection<NodeItem>();
            if (!(string.IsNullOrEmpty(value)))
            {
                foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
                {
                    GetNoteItemsRecursive(item, value, nodeItems);
                }
            }
            (ucNodes.DataContext as NodeAppliedViewModel).AppliedNodeItems = nodeItems;
            //(ucBomLib.DataContext as BomListViewModel).SelectedDiagramItem = HK_General.GetDiagramItem(value);
            (ucBomLib.DataContext as BomListViewModel).SelectedDiagramItem = (ucDiagLib.DataContext as DiagGrid2ViewModel).SelectedItem;
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
        //在Tree上标记
        private void OnNodeIDHighlighted(object sender, NodeItem value)
        {
            HkTreeItem highlightedNode = null;
            if (!string.IsNullOrEmpty(value?.NodeID))
            {
                foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
                {
                    highlightedNode = HighlightNodeRecursive(item, value?.NodeID);
                }
            }
            else
            {
                foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
                {
                    HighlightNodeClearRecursive(item);
                }
            }
            if (highlightedNode != null) { ucTree.tvHk.BringItemIntoView(highlightedNode); }
        }
        private HkTreeItem HighlightNodeRecursive(HkTreeItem item, string nodeId)
        {
            HkTreeItem highlightedNode = null;
            if (item == null || string.IsNullOrEmpty(nodeId)) return null;
            if (item.ID == nodeId)
            {
                item.IsHighlighted = true;
                highlightedNode = item;
                if (item.Parent != null)
                    item.Parent.IsExpanded = true;
            }
            else
                item.IsHighlighted = false;

            foreach (var child in item.Children)
            {
                if (highlightedNode == null)
                    highlightedNode = HighlightNodeRecursive(child, nodeId);
                HighlightNodeRecursive(child, nodeId);
            }
            return highlightedNode;
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
    }
}
