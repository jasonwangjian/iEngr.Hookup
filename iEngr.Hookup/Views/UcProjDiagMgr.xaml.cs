using ComosQueryInterface;
using ComosQueryXObj;
using iEngr.Hookup.Models;
using iEngr.Hookup.ViewModels;
using Plt;
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
    public enum ActiveArea
    {
        Assigned,
        Available
    }
    /// <summary>
    /// UcDiagMgr.xaml 的交互逻辑
    /// </summary>
    public partial class UcProjDiagMgr :UserControl, INotifyPropertyChanged
    {
        public event EventHandler<bool> RefreshComosData;
        public ActiveArea ActiveArea { get; set; }  
        HkTreeViewModel VmTree;
        HkPictureViewModel VmPicture;
        DiagItemsViewModel VmDiagLib;
        DiagItemsViewModel VmDiagComos;
        AppliedNodeViewModel VmAppliedLib;
        BomItemsViewModel VmBomComos;
        BomItemsViewModel VmBomLib;
        PropLabelViewModel VmLabel;

        public UcProjDiagMgr()
        {
            InitializeComponent();
            VmTree= ucTree.DataContext as HkTreeViewModel;
            VmTree.TreeItemChanged += OnTreeItemChanged;
            VmTree.DiagramIDsChanged += OnDiagramIDsChanged;
            VmTree.DiagramIDAdded += OnDiagramIDAdded;
            VmTree.PropLabelItemsChanged += OnNodeLabelItemsChanged;
            VmPicture = ucPic.DataContext as HkPictureViewModel;
            VmDiagLib = ucDiagLib.DataContext as DiagItemsViewModel;
            VmDiagLib.LibDiagramChanged += OnLibDiagramChanged;
            VmDiagLib.ClearLibDiagBom += OnClearLibDiagBom;
            VmDiagLib.PicturePathChanged += OnPicturePathChanged;
            VmDiagLib.AvailableDiagramItems = HK_General.GetDiagramItems();
            VmDiagLib.AssignedDiagramItems = new ObservableCollection<DiagramItem>();

            VmDiagComos = ucDiagComos.DataContext as DiagItemsViewModel;
            VmDiagComos.PicturePathChanged += OnPicturePathChanged;
            VmDiagComos.DiagramGroupChanged += OnNodeIDHighlighted;
            VmAppliedLib = ucNodes.DataContext as AppliedNodeViewModel;
            VmAppliedLib.NodeIDHighlighted += OnNodeIDHighlighted;
            VmBomComos = ucBomComos.DataContext as BomItemsViewModel;
            VmBomLib = ucBomLib.DataContext as BomItemsViewModel;
            VmLabel = ucProp.DataContext as PropLabelViewModel;

            ucDiagLib.ActiveAreaChange += OnActiveAreaChange;
            ucDiagComos.ActiveAreaChange += OnActiveAreaChange;

            VmDiagLib.IsLangCtrlShown = false;
            VmDiagComos.IsLangCtrlShown = false;
            VmBomComos.IsLangCtrlShown = false;
            VmBomLib.IsLangCtrlShown=false;
            VmBomComos.IsButtonShown = false;
            VmBomLib.IsButtonShown = false;
            IsComparisonById = true;
            IsComparisonEnabled = false;

            VmLabel.IsCompared = true;

            VmDiagComos.IsAssignedDiagramItemsShown = false;
            VmDiagLib.IsAssignedDiagramItemsShown = true;

            VmDiagComos.AvailableDiagramItems = HK_General.GetDiagramItems();
            VmDiagComos.AssignedDiagramItems= new ObservableCollection<DiagramItem>();
            //ProjDiagMgrViewModel VmProjDiagMgr = new ProjDiagMgrViewModel();
            //DataContext = VmProjDiagMgr;
            //VmProjDiagMgr.LangInChineseChanged += OnLangInChineseChanged;
            //VmProjDiagMgr.LangInEnglishChanged += OnLangInEnglishChanged;
            DataContext = this;

            RefreshCommand = new RelayCommand<object>(Refresh, CanRefresh);
            LibDiagMgrCommand = new RelayCommand<object>(LibDiagMgr, CanLibDiagMgr);
            BomCompareCommand = new RelayCommand<object>(BomCompare, CanBomCompare);

            LangInChinese = true;

        }

        private void OnActiveAreaChange(object sender, ActiveArea value)
        {
            ActiveArea = value;
        }
        //更新UcHkPicture
        private void OnPicturePathChanged(object sender, DiagramItem value)
        {
            VmPicture.PicturePath = value?.PicturePath;
            if (value == null) return;
            if (value.IsLibItem)
            {
                if (ActiveArea == ActiveArea.Assigned)
                    VmPicture.AttachTo = AttachTo.LibAssigned;
                else if (ActiveArea == ActiveArea.Available)
                    VmPicture.AttachTo = AttachTo.LibAvailable;
            }
            else if (value.IsComosItem)
            {
                if (ActiveArea == ActiveArea.Assigned)
                    VmPicture.AttachTo = AttachTo.ComosAssigned;
                else if (ActiveArea == ActiveArea.Available)
                    VmPicture.AttachTo = AttachTo.ComosAvailable;
            }
        }
        //更新UcPropLabel
        private void OnNodeLabelItemsChanged(object sender, HkTreeItem value)
        {
            if (VmLabel.LabelItems == null)
            {
                VmLabel.LabelItems = HK_General.GetPropLabelItems(value);
                return;
            }
            var nodeLabels = HK_General.GetPropLabelItems(value).ToDictionary(x => x.Key, x => x);
            VmLabel.Clear("node");
            var validLabels = VmLabel.LabelItems.Where(x => x.DisplayValue1 != null || x.DisplayValue2 != null);
            foreach (var label in validLabels)
            {
                if (nodeLabels.ContainsKey(label.Key))
                {
                    label.DisplayValue1 = nodeLabels[label.Key].DisplayValue1;
                    label.IsNodeLabel = nodeLabels[label.Key].IsNodeLabel;
                    label.IsInherit = nodeLabels[label.Key].IsInherit;
                    nodeLabels.Remove(label.Key);
                }
            }
            VmLabel.LabelItems = new ObservableCollection<LabelDisplay>(
                validLabels.Union(new ObservableCollection<LabelDisplay>(nodeLabels.Select(x => x.Value))));
        }

        private void OnTreeItemChanged(object sender, HkTreeItem value)
        {
            VmDiagLib.FocusedNode = value;
            //OnPicturePathChanged(sender, value);
            OnNodeLabelItemsChanged(sender, value);
            OnDiagramIDsChanged(sender, value);
        }
        private void OnLabelsChanged(object sender, Dictionary<string,object> value)
        {
            if(sender is HkTreeItem treeItem)
            {
                
            }
        }
        private void OnDiagramIDAdded(object sender, HkTreeItem value)
        {
            DiagramItem item = HK_General.GetDiagramItem(value.NewAddedID.ToString());
            item.IsOwned = true; item.IsInherit = false;
            VmDiagLib.AvailableDiagramItems.Add(item);
            VmDiagLib.AssignedDiagramItems.Add(item);
            VmDiagLib.FilterText = VmDiagLib.FilterText;// 刷新FilteredItems
        }
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
        private void OnClearLibDiagBom(object sender, bool value)
        {
            if (value)
            {
                VmBomLib.DataSource.Clear();
            }
        }
        //更新UcNodeApplied
        private void OnLibDiagramChanged(object sender, DiagramItem value)
        {
            string id = value.ID.ToString();
            VmDiagComos.AvailableDiagramsSelectedItem = null;
            VmDiagComos.AssignedDiagramsSelectedItem = null;
            ObservableCollection<AppliedNodeItem> nodeItems = new ObservableCollection<AppliedNodeItem>();
            if (!(string.IsNullOrEmpty(id)))
            {
                foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
                {
                    GetNoteItemsRecursive(item, id, nodeItems);
                }
            }
            VmAppliedLib.AppliedItems = nodeItems;
            //(ucBomLib.DataContext as BomListViewModel).SelectedDiagramItem = HK_General.GetDiagramItem(value);
            if (VmBomLib.SelectedDiagramItem != VmDiagLib.SelectedItem) VmBomLib.SelectedDiagramItem = VmDiagLib.SelectedItem;
            VmBomLib.DataSource = HK_General.GetDiagBomItems(id);
            ucBomLib.dgBOM.UpdateLayout();
            ExecComparision(IsComparisonEnabled);
            //刷行IsSelectedID
            if (string.IsNullOrEmpty(id))
            {
                foreach (var diagItem in VmDiagComos.AvailableDiagramItems)
                {
                    diagItem.IsSelectedID = false;
                }
                foreach (var diagItem in VmDiagLib.AvailableDiagramItems)
                {
                    diagItem.IsSelectedID = false;
                }
            }
            else
            {
                foreach (var diagItem in VmDiagComos.AvailableDiagramItems)
                {
                    if (diagItem.RefID == id)
                        diagItem.IsSelectedID = true;
                    else
                        diagItem.IsSelectedID = false;
                }
                foreach (var diagItem in VmDiagLib.AvailableDiagramItems)
                {
                    if (diagItem.ID.ToString() == id)
                        diagItem.IsSelectedID = true;
                    else
                        diagItem.IsSelectedID = false;
                }
            }
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
        //在Tree上标记
        private void OnNodeIDHighlighted(object sender, AppliedNodeItem value)
        {
            HighlightNode(value?.NodeID);
            //HkTreeItem highlightedNode = null;
            //if (!string.IsNullOrEmpty(value?.NodeID))
            //{
            //    foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
            //    {
            //        highlightedNode = HighlightNodeRecursive(item, value?.NodeID);
            //    }
            //}
            //else
            //{
            //    foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
            //    {
            //        HighlightNodeClearRecursive(item);
            //    }
            //}
            //if (highlightedNode != null) { ucTree.tvHk.BringItemIntoView(highlightedNode); }
        }
        private void OnNodeIDHighlighted(object sender, DiagramItem value)
        {
            HighlightNode(value?.GroupID);
        }
        private void HighlightNode(string nodeId)
        {
            HkTreeItem highlightedNode = null;
            if (!string.IsNullOrEmpty(nodeId) && int.TryParse(nodeId, out _))
            {
                foreach (var item in (ucTree.DataContext as HkTreeViewModel).TreeItems)
                {
                    highlightedNode = HighlightNodeRecursive(item, nodeId);
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
        private bool _langInChinese;
        public bool LangInChinese
        {
            get => _langInChinese;
            set
            {
                SetField(ref _langInChinese, value);
                VmDiagComos.LangInChinese = value;
                VmDiagLib.LangInChinese = value;
                VmBomComos.LangInChinese = value;
                VmBomLib.LangInChinese = value;
            }
        }
        private bool _langInEnglish;
        public bool LangInEnglish
        {
            get => _langInEnglish;
            set
            {
                SetField(ref _langInEnglish, value);
                VmDiagComos.LangInEnglish = value;
                VmDiagLib.LangInEnglish = value;
                VmBomComos.LangInEnglish = value;
                VmBomLib.LangInEnglish = value;
            }
        }
        private bool _isComparisonEnabled;
        public bool IsComparisonEnabled
        {
            get => _isComparisonEnabled;
            set
            {
                SetField(ref _isComparisonEnabled, value);
                //VmBomComos.IsComparisonEnabled =value;
                //VmBomLib.IsComparisonEnabled=value;
                ExecComparision(value);
            }
        }
        public void ExecComparision(bool IsCompare)
        {
            if (IsCompare == true)
            {
                DataGridComparisonHelper.ExecComparisonMan(ucBomComos.dgBOM, ucBomLib.dgBOM);
            }
            else
            {
                DataGridComparisonHelper.ClearHighlightsMan(ucBomComos.dgBOM);
                DataGridComparisonHelper.ClearHighlightsMan(ucBomLib.dgBOM);
            }
        }
        private bool _isComparisonById;
        public bool IsComparisonById
        {
            get => _isComparisonById;
            set
            {
                SetField(ref _isComparisonById, value);
                VmBomComos.IsComparisonById = value;
                VmBomLib.IsComparisonById = value;
                ExecComparision(IsComparisonEnabled);
            }
        }
        private bool _isAutoNavigate;
        public bool IsAutoNavigate
        {
            get => _isAutoNavigate;
            set
            {
                SetField(ref _isAutoNavigate, value);
            }
        }
        public bool IsAdmin
        {
            get
            {
                return (HK_General.UserComos.Roles & HK_General.RoleAdmin) > 0;
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

        #region Command
        public RelayCommand<object> RefreshCommand { get; set; }
        private bool CanRefresh(object parameter)
        {
            return true;
        }
        private void Refresh(object parameter)
        {
            RefreshComosData?.Invoke(this, true);
        }
        public RelayCommand<object> LibDiagMgrCommand { get; set; }
        private bool CanLibDiagMgr(object parameter)
        {
            return true;
        }
        // 在用户控件中获取父窗口
        private void LibDiagMgr(object parameter)
        {
            var dialog = new GenLibDiagMgrDialog();
            dialog.WindowState = WindowState.Maximized;
            dialog.Show();
        }
        public RelayCommand<object> BomCompareCommand { get; set; }
        private bool CanBomCompare(object parameter)
        {
            return true;
        }
        private void BomCompare(object parameter)
        {
            DataGridComparisonHelper.ExecComparisonMan(ucBomComos.dgBOM, ucBomLib.dgBOM);
        }
        #endregion
    }
}
