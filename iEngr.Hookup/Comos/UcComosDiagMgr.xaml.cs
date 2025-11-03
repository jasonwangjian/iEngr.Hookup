using Comos.Controls;
using Comos.CtxMenu;
using Comos.UIF;
using ComosPPGeneral;
using ComosQueryInterface;
using ComosQueryXObj;
using iEngr.Hookup.Models;
using iEngr.Hookup.ViewModels;
using iEngr.Hookup.Views;
using Plt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IContainer = Comos.Controls.IContainer;

namespace iEngr.Hookup.Comos
{
    /// <summary>
    /// UcComosDiagMgr.xaml 的交互逻辑
    /// </summary>
    public partial class UcComosDiagMgr : UserControl, IComosControl, INotifyPropertyChanged
    {
        private PopupDefinitions m_ppDef;
        private CtxMenu m_popup;
        public UcComosDiagMgr()
        {
            InitializeComponent();
            VmTree = ucPDM.ucTree.DataContext as HkTreeViewModel;
            VmTree.TreeItemApplied += OnTreeItemApplied;
            
            VmPicture = ucPDM.ucPic.DataContext as HkPictureViewModel;

            VmDiagComos = ucPDM.ucDiagComos.DataContext as DiagItemsViewModel;
            VmDiagComos.ComosDiagChanged += OnComosDiagramChanged;
            VmDiagComos.DiagLabelItemsChanged += OnDiagLabelItemsChanged;
            VmDiagComos.DiagLabelModified += OnDiagLabelModified;
            VmDiagComos.ComosPicturePathSet += OnComosPicturePathSet;
            VmDiagComos.ComosDiagObjDelCmd += OnComosDiagObjDelCmdClick;
            VmDiagComos.ComosDiagModClsCmd += OnComosDiagModClsCmdClick;
            VmDiagComos.ComosDiagModDelCmd += OnComosDiagModDelCmdClick;
            VmDiagComos.ComosDiagModRSCmd += OnComosDiagModRSCmdClick;
            VmDiagComos.ComosItemContextMenu += PopContextMenu;

            VmBomComos = ucPDM.ucBomComos.DataContext as BomItemsViewModel;
            VmBomComos.SelectedBomIDChanged += OnSelectedBomIDChanged;
            VmBomComos.ComosItemContextMenu += PopContextMenu;

            VmDiagLib = ucPDM.ucDiagLib.DataContext as DiagItemsViewModel;
            VmDiagLib.ComosDiagModAddCmd += OnComosDiagAppCmdClick;

            VmBomLib = ucPDM.ucBomLib.DataContext as BomItemsViewModel;
            VmBomLib.SelectedBomIDChanged += OnSelectedBomIDChanged;

            VmAppliedComos = ucPDM.ucComosDevs.DataContext as AppliedComosViewModel;
            VmAppliedComos.ComosItemSelected += OnComosItemSelected;
            VmAppliedComos.ComosDiagAppDelCmd += OnComosDiagAppDelCmdClick;
            VmAppliedComos.ComosItemContextMenu += PopContextMenu;

            VmLabel = ucPDM.ucProp.DataContext as PropLabelViewModel;

            ucPDM.ucPic.ComosUIDToDiagModGet += OnComosUIDToDiagModGet;
            //ucPDM.ucComosDevs.ComosItemContext += OnComosItemRightClick;
            ucPDM.ucComosDevs.ComosItemDoubleClick += OnComosItemDoubleClick;
        }
        public HkTreeViewModel VmTree;
        public HkPictureViewModel VmPicture;
        public DiagItemsViewModel VmDiagComos;
        public DiagItemsViewModel VmDiagLib;
        public BomItemsViewModel VmBomComos;
        public BomItemsViewModel VmBomLib;
        public AppliedComosViewModel VmAppliedComos;
        public PropLabelViewModel VmLabel;

        private IComosDGeneralCollection _objects;
        private string _parameters;
        private IContainer _controlContainer;
        public IComosDProject Project { set; get; }
        public IComosBaseObject _currentObject;
        public IComosBaseObject CurrentObject
        {
            get => _currentObject;
            set => SetField(ref _currentObject, value);
        }

        private IComosDWorkset _workset;
        public IComosDWorkset Workset
        {
            get => _workset;
            set
            {
                _workset = value;
                Project = Workset.GetCurrentProject();
                ProjectInitial();
                VmBomComos.Project = Project;
            }
        }
        public IComosDGeneralCollection Objects
        {
            get => _objects;
            set
            {
                _objects = value;
                if (_objects != null)
                {
                    CurrentObject = _objects.get_Item(1);
                    SetDiagComosAvailable(CurrentObject);
                    SetDiagComosAssigned(CurrentObject);
                }
            }
        }
        public IContainer ControlContainer
        {
            get => _controlContainer;
            set
            {
                _controlContainer = value;
                if (_controlContainer != null)
                {
                    object p = _controlContainer.UIFParent;
                    if (p != null)
                    {
                        ((IUIForm)p).Header.Description = $"项目安装图模板管理 {GetType().FullName}";
                    }
                }
            }
        }
        public string Parameters //在Menu的Main.xml定义的
        {
            get => _parameters;
            set => SetField(ref _parameters, value);
        }
        public void OnCanExecute(CanExecuteRoutedEventArgs e) { }
        public void OnPreviewExecuted(ExecutedRoutedEventArgs e) { }
        public void OnExecuted(ExecutedRoutedEventArgs e) { }
        public bool IsComosProjectIniOK { get; set; }
        public IComosBaseObject ProjectCfgCI { get; set; }
        public IComosBaseObject DiagModFolder { get; set; }
        public IComosDCDevice CDevDiagModFolder { get; set; }
        public IComosDCDevice CDevDiagObjFolder { get; set; }
        public IComosDCDevice CDevDiagMod { get; set; }
        public IComosDCDevice CDevDiagObj { get; set; }
        public IComosDCDevice CDevDiagBom { get; set; }
        private void ProjectInitial()
        {
            IsComosProjectIniOK = false;
            HK_General.ProjLanguage = int.TryParse(Project.CurrentLanguage.FullName(), out int projLanguange) ? projLanguange : 4;
            //VmBomList.LangInChinese = (HK_General.ProjLanguage == 4);
            //VmBomList.LangInEnglish = (HK_General.ProjLanguage == 2);
            //VmMatList.LangInChinese = (HK_General.ProjLanguage == 4);
            //VmMatList.LangInEnglish = (HK_General.ProjLanguage == 2);
            HK_General.UserName = string.IsNullOrEmpty(Workset.Globals()?.IniRealName) ? Workset.GetCurrentUser().FullName() : Workset.Globals()?.IniRealName;
            ProjectCfgCI = Project.GetObjectByPathFullName("08U@ProjectManagement\\~08UC&I");
            if (ProjectCfgCI == null) return;
            CDevDiagModFolder = Project.GetCDeviceBySystemFullname("@20|A10|A20|M41|A60", 1);
            if (CDevDiagModFolder == null) return;
            var col = ProjectCfgCI.ScanDevicesWithCObject("", CDevDiagModFolder);
            if (col == null || col.Count() == 0)
            {
                DiagModFolder = Project.Workset().CreateDeviceObject(ProjectCfgCI, CDevDiagModFolder);
            }
            else
            {
                DiagModFolder = col.Item(1);
            }
            CDevDiagObjFolder = Project.GetCDeviceBySystemFullname("@20|A10|A10|M41|Z10", 1);
            if (CDevDiagObjFolder == null) { return; }
            CDevDiagMod = Project.GetCDeviceBySystemFullname("@20|B70|M41|A20Z", 1);
            if (CDevDiagMod == null) { return; }
            CDevDiagObj = Project.GetCDeviceBySystemFullname("@20|B70|M41|A10Z", 1);
            if (CDevDiagObj == null) { return; }

            CDevDiagBom = Project.GetCDeviceBySystemFullname("@30|M41|A50|A10Z|A10|A10|A60|A30|Z10", 1);
            if (CDevDiagBom == null) { return; }

            IsComosProjectIniOK = true;

            //Project.Workset().Globals().
        }
        private void SetDiagComosAvailable(IComosBaseObject objQueryStart = null)
        {
            try
            {
                //if (objQueryStart is null) return;
                if (objQueryStart is null || (objQueryStart as dynamic).CDevice()?.IsInheritSuccessorFrom(CDevDiagModFolder) != true)
                {
                    objQueryStart = DiagModFolder;
                }
                IComosDCDevice qrydev = Project.GetCDeviceBySystemFullname("@20|A70|Z10|A20|QHkDiagMod", 1);
                if (qrydev != null)
                {
                    VmDiagComos.AvailableDiagramItems.Clear();
                    ITopQuery tqry = ((QueryXObj)qrydev.XObj).TopQuery;
                    tqry.MainObject = objQueryStart;
                    tqry.Execute();
                    IQuery qry = tqry.Query;
                    for (int i = 1; i <= qry.RowCount; i++)
                    {
                        IComosBaseObject item = qry.RowObject[i];
                        DiagramItem diagItem = new DiagramItem() { ObjComosDiagMod = item };
                        VmDiagComos.AvailableDiagramItems.Add(diagItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcComosDiagMgr.SetDiagComosAvailable(IComosBaseObject objQueryStart) Error occurred: {ex.Message}");
            }
        }
        private void SetDiagComosAssigned(IComosBaseObject objQueryStart)
        {
            try
            {
                //VmDiagComos.AssignedDiagramItems = GetDiagComosItems(objQueryStart);
                VmDiagComos.AssignedDiagramItems.Clear();
                ObservableCollection<DiagramItem> diagramItems = GetDiagComosItems(objQueryStart);
                var objComosDiags = diagramItems.Select(x => x.ObjComosDiagMod);
                foreach (DiagramItem item in VmDiagComos.AvailableDiagramItems)
                {
                    if (objComosDiags.Contains(item.ObjComosDiagMod))
                    {
                        VmDiagComos.AssignedDiagramItems.Add(item);
                    }
                }
                var nullDiagramItems = diagramItems.Where(x => x.ObjComosDiagMod == null);
                foreach (DiagramItem item in nullDiagramItems)
                {
                    VmDiagComos.AssignedDiagramItems.Add(item);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcComosDiagMgr.SetDiagComosAssigned(IComosBaseObject objQueryStart) Error occurred: {ex.Message}");
            }
        }
        private ObservableCollection<DiagramItem> GetDiagComosItems(IComosBaseObject objQueryStart)
        {
            ObservableCollection<DiagramItem> diagrams = new ObservableCollection<DiagramItem>();
            try
            {
                IComosDCDevice qrydev = Project.GetCDeviceBySystemFullname("@20|A70|Z10|A20|QHkDiagObj", 1);
                if (qrydev == null) return diagrams;
                if ((objQueryStart as dynamic).CDevice()?.IsInheritSuccessorFrom(CDevDiagObjFolder) != true) return diagrams;
                VmDiagComos.IsAssignedDiagramItemsShown = true;
                ITopQuery tqry = ((QueryXObj)qrydev.XObj).TopQuery;
                tqry.MainObject = objQueryStart;
                tqry.Execute();
                IQuery qry = tqry.Query;
                for (int i = 1; i <= qry.RowCount; i++)
                {
                    IComosBaseObject item = qry.RowObject[i];
                    IComosBaseObject itemLinkObj = item.spec("Y00T00103.Y00A00641")?.LinkObject;
                    DiagramItem diagItem = new DiagramItem() { ObjComosDiagMod = itemLinkObj, ObjComosDiagObj = item, BomQty="?", IsOwned = true };
                    //if (itenLinkObj == null)
                    //{
                    //    diagItem.RefID = item.Name;
                    //    diagItem.NameCn = item.GetInternationalDescription(4);
                    //    diagItem.NameEn = item.GetInternationalDescription(2);
                    //}
                    diagrams.Add(diagItem);
                }
                return diagrams;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcComosDiagMgr.GetDiagComosItems(IComosBaseObject objQueryStart) Error occurred: {ex.Message}");
                return diagrams;
            }
        }
        private void OnComosDiagramChanged(object sender, DiagramItem value)
        {
            VmBomComos.DataSource.Clear();
            VmAppliedComos.AppliedItems.Clear();
            if (value != null && value.ObjComosDiagMod != null)
            {
                SetBomListDataSource(value.ObjComosDiagMod);
                ucPDM.ucBomComos.dgBOM.UpdateLayout();
                ucPDM.ExecComparision(ucPDM.IsComparisonEnabled);
                value.BomQty = VmBomComos.DataSource.Count().ToString();
                VmBomComos.SelectedDiagramItem = VmDiagComos.SelectedItem;
                //刷新IsSelectedGroup
                string groupID = value.GroupID;
                if (string.IsNullOrEmpty(groupID))
                {
                    foreach (var diagItem in VmDiagComos.AvailableDiagramItems)
                    {
                        diagItem.IsSelectedGroup = false;
                    }
                    value.IsSelectedGroup = true;
                }
                else
                {
                    foreach (var diagItem in VmDiagComos.AvailableDiagramItems)
                    {
                        if (diagItem.GroupID == groupID)
                            diagItem.IsSelectedGroup = true;
                        else
                            diagItem.IsSelectedGroup=false ;
                    }
                }
                //刷新VmAppliedComos.AppliedItems
                var appliedDevs = (value.ObjComosDiagMod as dynamic).BackPointerDevicesWithImplementation;
                for (int i = 1; i <= appliedDevs.Count(); i++)
                {
                    IComosBaseObject dev = appliedDevs.Item(i);
                    VmAppliedComos.AppliedItems.Add(GetAppliedComosItem(dev));
                }
                
                if (VmAppliedComos.AppliedItems.Count()>0)
                {
                    VmDiagComos.CanComosDiagModCls = true;
                    VmDiagComos.CanComosDiagModDel = false;
                }
                else
                {
                    VmDiagComos.CanComosDiagModCls = false;
                    VmDiagComos.CanComosDiagModDel = true;
                }
                //刷行标签比较控件
                OnDiagLabelItemsChanged(sender, value);
            }
        }
        private void OnDiagLabelItemsChanged(object sender, DiagramItem value)
        {
            var diagLabels = HK_General.GetPropLabelItems(value).ToDictionary(x => x.Key, x => x);
            VmLabel.Clear("diagram");
            var validLabels = VmLabel.LabelItems.Where(x => x.DisplayValue1 != null || x.DisplayValue2 != null);
            foreach (var label in validLabels)
            {
                if (diagLabels.ContainsKey(label.Key))
                {
                    label.DisplayValue2 = diagLabels[label.Key].DisplayValue2;
                    label.IsComosLabel = diagLabels[label.Key].IsComosLabel;
                    diagLabels.Remove(label.Key);
                }
            }
            VmLabel.LabelItems = new ObservableCollection<LabelDisplay>(
                validLabels.Union(new ObservableCollection<LabelDisplay>(diagLabels.Select(x => x.Value))));
        }
        private void OnDiagLabelModified(object sender, DiagramItem value)
        {
            string groupID = value?.GroupID;
            if (value == null || string.IsNullOrEmpty(groupID)) return;
            foreach (var diagMod in VmDiagComos.AvailableDiagramItems)
            {
                if (diagMod.GroupID == groupID && diagMod != value)
                {
                    diagMod.IdLabels = value.IdLabels;
                }
            }
        }


        private void SetBomListDataSource(IComosBaseObject objQueryStart)
        {
            try
            {
                if (objQueryStart is null) return;
                IComosDCDevice qrydev = Project.GetCDeviceBySystemFullname("@20|A70|Z10|A20|QHkBom", 1);
                if (null != qrydev)
                {
                    ITopQuery tqry = ((QueryXObj)qrydev.XObj).TopQuery;
                    tqry.MainObject = objQueryStart;
                    tqry.Execute();
                    IQuery qry = tqry.Query;
                    for (int i = 1; i <= qry.RowCount; i++)
                    {
                        IComosBaseObject item = qry.RowObject[i];
                        BomItem bomItem = new BomItem() { ObjComosBomItem = item };
                        //bomItem.SetDataFromComosObject();
                        VmBomComos.DataSource.Add(bomItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcComosDiagMgr.SetBomListDataSource(IComosBaseObject objQueryStart) Error occurred: {ex.Message}");
            }
        }
        private void OnComosPicturePathSet(object sender, DiagramItem value)
        {
            if (value != null && value.ObjComosDiagMod != null)
            {
                value.ObjComosDiagMod.spec("Y00T00103.PicturePath").value = value.PicturePath;
            }
        }
        private void OnComosItemSelected(object sender, AppliedComosItem value)
        {
            if (value == null || !ucPDM.UcDM.IsAutoNavigate) return;
            //IComosBaseObject obj = Project.Workset().LoadObjectByType(8, value.ComosUID);
            IComosBaseObject obj = value.ComosObj;
            Project.Workset().Globals().Navigator.GetCurrentTree.DefaultAction(obj);
        }
        private void OnComosItemDoubleClick(object sender, AppliedComosItem value)
        {
            if (value == null || string.IsNullOrEmpty(value.ComosUID)) return;
            IComosBaseObject obj = Project.Workset().LoadObjectByType(8, value.ComosUID);
            if (obj == null) return;
            Project.Workset().Globals().Navigator.GetCurrentTree.DefaultAction(obj);

            //Set mntc = createobject("Chemserv.ComosClass")
            //mntc.ShowComosObject(c) '弹出浮窗
            //Set Mntc = Nothing
            //新弹窗
            //Chemserv.ComosClass mntc = new Chemserv.ComosClass();
            //mntc.ShowComosObject(obj);
        }
        private void PopContextMenu(object sender, IComosBaseObject comosObj)
        {
            if (comosObj == null) return;
            Comos_ContextMenuOpening(comosObj);
        }

        private void Comos_ContextMenuOpening(IComosBaseObject obj)
        {
            try
            {
                m_ppDef?.ShutDown();
                ((_DPopup)m_popup)?.Clear();

                m_ppDef = m_ppDef ?? new PopupDefinitions();
                m_popup = m_popup ?? new CtxMenu();

                _DPopup popup = m_popup;
                popup.Clear();

                m_ppDef.CreatePopup(m_popup, obj);

                for (int i = popup.ItemCount() - 1; i >= 0; i--)
                {
                    string id = popup.GetItemID(i);
                    if (id != "PROPERTIES" && id != "Navigate")
                    {
                        popup.Delete(id);
                    }
                }
                popup.Popup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcComosDiagMgr.Comos_ContextMenuOpening(IComosBaseObject obj) Error occurred: {ex.Message}");
                //System.Diagnostics.Trace.WriteLine(ex);
            }
        }

        private void OnComosUIDToDiagModGet(object sender, string value)
        {
            if (value.Contains("WITHOUT_UID") || value.Contains("CDevice"))
            {
                //无效对象
                return;
            }
            string comosUID = value.Split(',')[1];
            IComosBaseObject ciObj = Project.Workset().LoadObjectByType(8, comosUID);
            //ciObj 被锁定检查，锁定提示，退出 TBA
            string groupId = VmDiagComos.SelectedItem?.GroupID;
            if (string.IsNullOrEmpty(groupId))
            {
                var diagObj = ComosDiagObjCreate(ciObj, VmDiagComos.SelectedItem?.ObjComosDiagMod);
                if (diagObj != null)
                {
                    VmAppliedComos.AppliedItems.Add(GetAppliedComosItem(diagObj));
                }
            }
            else //有分组
            {
                var diagItems = VmDiagComos.AvailableDiagramItems.Where(x => x.GroupID == groupId);
                foreach (var diagItem in diagItems)
                {
                    var diagObj = ComosDiagObjCreate(ciObj, diagItem?.ObjComosDiagMod);
                    if (VmDiagComos.SelectedItem == diagItem && diagObj != null)
                    {
                        VmAppliedComos.AppliedItems.Add(GetAppliedComosItem(diagObj));
                    }
                }
            }
            SetDiagComosAssigned(CurrentObject);
        }
        private ObservableCollection<IComosBaseObject> GetAppliedCIDevice(IComosBaseObject diagMod)
        {
            ObservableCollection<IComosBaseObject> appliedCIDevice = new ObservableCollection<IComosBaseObject>();
            if (diagMod == null) return appliedCIDevice;
            var appliedDevs = (diagMod as dynamic).BackPointerDevicesWithImplementation;
            for (int i = 1; i <= appliedDevs.Count(); i++)
            {
                IComosBaseObject dev = appliedDevs.Item(i);
                IComosBaseObject ciDev = dev.owner().owner();
                appliedCIDevice.Add(ciDev);
            }
            return appliedCIDevice;
        }
        private AppliedComosItem GetAppliedComosItem(IComosBaseObject diagObj)
        {
            IComosBaseObject ciDev = diagObj.owner().owner();
            return new AppliedComosItem()
            {
                ComosUID = diagObj.SystemUID(),
                AssignMode = diagObj.spec("Y00T00103.AssignMode").value,
                //IsLocked = ciDev.spec("XXXXXXXXX.IsLocked").value == "1" ? true : false,
                ComosObj = ciDev,
                DisplayName = ciDev.spec("Z00T00402.Y00A07320AA01").DisplayValue()
            };
        }
        private IComosBaseObject ComosDiagObjCreate(IComosBaseObject ciDevice, IComosBaseObject diagMod)
        {
            try
            {
                var hookups = (ciDevice as dynamic).Elements.Item("Hookups");
                if (hookups == null)
                {
                    //无效对象
                    return null;
                }
                if (GetAppliedCIDevice(diagMod).Any(x => x == ciDevice))
                {
                    //重复对象
                    return null;
                }
                IComosBaseObject diagObj = Project.Workset().CreateDeviceObject(hookups, CDevDiagObj);
                if (diagObj != null)
                {
                    diagObj.spec("Y00T00103.Y00A00641").LinkObject = diagMod;
                    diagObj.spec("Y00T00103.AssignMode").value = "M";
                    //成功添加
                }
                return diagObj;
            }
            catch
            {
                //创建失败
                return null;
            }
        }


        #region Command for Event
        //创建Diagram模板
        private  void OnTreeItemApplied(object sender, HkTreeItem value)
        {
            string groupID = value.ID;
            if (VmDiagComos.AvailableDiagramItems.Any(x=>x.GroupID == groupID))
            {
                //有重名GroupID
            }
            foreach (var item in VmDiagLib.AssignedDiagramItems)
            {
                DiagramItem newAddedDiag = ComosDiagModCreate(DiagModFolder,
                                                              item,
                                                              value.GetPropertiesStrings(),
                                                              groupID);
                if (newAddedDiag != null)
                {
                    VmDiagComos.AvailableDiagramsSelectedItem = newAddedDiag;
                }
            }
        }
        private void OnComosDiagAppCmdClick(object sender, DiagramItem value)
        {
            DiagramItem newAddedDiag = ComosDiagModCreate(DiagModFolder,
                                                          value,
                                                          VmTree.SelectedItem?.GetPropertiesStrings(),
                                                          null);
            if (newAddedDiag != null)
            {
                VmDiagComos.AvailableDiagramsSelectedItem = newAddedDiag;
            }
        }
        private DiagramItem ComosDiagModCreate(IComosBaseObject ModOwner,DiagramItem item, string lables, string groupId = null)
        {
            if (item == null || ModOwner == null) return null;
            IComosBaseObject diagMod = Project.Workset().CreateDeviceObject(ModOwner, CDevDiagMod);
            if (diagMod == null) return null;
            diagMod.spec("Y00T00103.GroupID").value = groupId;
            diagMod.spec("Y00T00103.PicturePath").value = item.PicturePath;
            diagMod.spec("Y00T00103.RefIdInLib").value = item.DisplayID;
            diagMod.SetInternationalDescription(4, item.NameCn);
            diagMod.SetInternationalDescription(2, item.NameEn);
            diagMod.spec("Y00T00103.Remarks").SetInternationalValue(4, item.RemarksCn);
            diagMod.spec("Y00T00103.Remarks").SetInternationalValue(2, item.RemarksEn);
            //diagMod.spec("Y00T00103.IdLabels").value = lables;
            diagMod.XMLSet("Comos/System/Hookup/IdLabels", lables);
            DiagramItem newAddedDiag = new DiagramItem() { ObjComosDiagMod = diagMod };
            VmDiagComos.AvailableDiagramItems.Add(newAddedDiag);
            //添加BOM
            var bomItens = HK_General.GetDiagBomItems(item.ID.ToString());
            foreach (var bomItem in bomItens)
            {
                IComosBaseObject bomItemObj = Project.Workset().CreateDeviceObject(diagMod, CDevDiagBom);
                if (bomItemObj != null)
                {
                    bomItemObj.Label = bomItem.No;
                    bomItemObj.spec("Z00T00003.ID").value = bomItem.ID;
                    bomItemObj.spec("Z00T00003.Qty").value = bomItem.Qty;
                    bomItemObj.spec("Z00T00003.Unit").value = bomItem.Unit;
                    bomItemObj.spec("Z00T00003.SD").value = bomItem.SupplyDiscipline;
                    bomItemObj.spec("Z00T00003.SR").value = bomItem.SupplyResponsible;
                    bomItemObj.spec("Z00T00003.ED").value = bomItem.ErectionDiscipline;
                    bomItemObj.spec("Z00T00003.ER").value = bomItem.ErectionResponsible;
                    bomItemObj.spec("Z00T00003.Mat").value = bomItem.MatMatCode;
                    bomItemObj.SetInternationalDescription(4, bomItem.NameCn);
                    bomItemObj.SetInternationalDescription(2, bomItem.NameEn);
                    bomItemObj.spec("Z00T00003.Name").SetInternationalValue(4, bomItem.NameCn);
                    bomItemObj.spec("Z00T00003.Name").SetInternationalValue(2, bomItem.NameEn);
                    bomItemObj.spec("Z00T00003.SpecAll").SetInternationalValue(4, bomItem.SpecAllCn);
                    bomItemObj.spec("Z00T00003.SpecAll").SetInternationalValue(2, bomItem.SpecAllEn);
                    bomItemObj.spec("Z00T00003.Remarks").SetInternationalValue(4, bomItem.RemarksCn);
                    bomItemObj.spec("Z00T00003.Remarks").SetInternationalValue(2, bomItem.RemarksEn);
                }
            }
            return newAddedDiag;
        }
        //删除模板后重建
        //private void ComosDiagModReset(DiagramItem item, string groupId = null)
        //{
        //    if (item == null || item.ObjComosDiagMod == null) return;
        //    IComosBaseObject diagMod = Project.Workset().CreateDeviceObject(item.ObjComosDiagMod.owner(), CDevDiagMod);
        //    if (diagMod == null) return;
        //    diagMod.spec("Y00T00103.GroupID").value = item.ObjComosDiagMod.spec("Y00T00103.GroupID").value;
        //    diagMod.spec("Y00T00103.PicturePath").value = item.PicturePath;
        //    diagMod.spec("Y00T00103.RefIdInLib").value = item.DisplayID;
        //    diagMod.SetInternationalDescription(4, item.NameCn);
        //    diagMod.SetInternationalDescription(2, item.NameEn);
        //    diagMod.spec("Y00T00103.Remarks").SetInternationalValue(4, item.RemarksCn);
        //    diagMod.spec("Y00T00103.Remarks").SetInternationalValue(2, item.RemarksEn);
        //    diagMod.XMLSet("Comos/System/Hookup/IdLabels", diagMod.XMLGet("Comos/System/Hookup/IdLabels"));
        //    item.ObjComosDiagMod.DeleteAll();
        //    item.ObjComosDiagMod = diagMod;
        //    //添加BOM
        //    var bomItens = HK_General.GetDiagBomItems(item.RefID);
        //    foreach (var bomItem in bomItens)
        //    {
        //        IComosBaseObject bomItemObj = Project.Workset().CreateDeviceObject(diagMod, CDevDiagBom);
        //        if (bomItemObj != null)
        //        {
        //            bomItemObj.Label = bomItem.No;
        //            bomItemObj.spec("Z00T00003.ID").value = bomItem.ID;
        //            bomItemObj.spec("Z00T00003.Qty").value = bomItem.Qty;
        //            bomItemObj.spec("Z00T00003.Unit").value = bomItem.Unit;
        //            bomItemObj.spec("Z00T00003.SD").value = bomItem.SupplyDiscipline;
        //            bomItemObj.spec("Z00T00003.SR").value = bomItem.SupplyResponsible;
        //            bomItemObj.spec("Z00T00003.ED").value = bomItem.ErectionDiscipline;
        //            bomItemObj.spec("Z00T00003.ER").value = bomItem.ErectionResponsible;
        //            bomItemObj.spec("Z00T00003.Mat").value = bomItem.MatMatCode;
        //            bomItemObj.SetInternationalDescription(4, bomItem.NameCn);
        //            bomItemObj.SetInternationalDescription(2, bomItem.NameEn);
        //            bomItemObj.spec("Z00T00003.Name").SetInternationalValue(4, bomItem.NameCn);
        //            bomItemObj.spec("Z00T00003.Name").SetInternationalValue(2, bomItem.NameEn);
        //            bomItemObj.spec("Z00T00003.SpecAll").SetInternationalValue(4, bomItem.SpecAllCn);
        //            bomItemObj.spec("Z00T00003.SpecAll").SetInternationalValue(2, bomItem.SpecAllEn);
        //            bomItemObj.spec("Z00T00003.Remarks").SetInternationalValue(4, bomItem.RemarksCn);
        //            bomItemObj.spec("Z00T00003.Remarks").SetInternationalValue(2, bomItem.RemarksEn);
        //        }
        //    }
        //}
        //删除BOM后重建
        private void ComosDiagModReset(DiagramItem item, string groupId = null)
        {
            if (item == null || item.ObjComosDiagMod == null) return;
            //删除已有BOM
            var existingBomItems = item.ObjComosDiagMod.ScanDevicesWithCObject("", CDevDiagBom);
            for (int i = 1;i<=existingBomItems.Count(); i++)
            {
                existingBomItems.Item(i).DeleteAll();
            }
            //添加BOM
            var bomItens = HK_General.GetDiagBomItems(item.RefID);
            foreach (var bomItem in bomItens)
            {
                IComosBaseObject bomItemObj = Project.Workset().CreateDeviceObject(item.ObjComosDiagMod, CDevDiagBom);
                if (bomItemObj != null)
                {
                    bomItemObj.Label = bomItem.No;
                    bomItemObj.spec("Z00T00003.ID").value = bomItem.ID;
                    bomItemObj.spec("Z00T00003.Qty").value = bomItem.Qty;
                    bomItemObj.spec("Z00T00003.Unit").value = bomItem.Unit;
                    bomItemObj.spec("Z00T00003.SD").value = bomItem.SupplyDiscipline;
                    bomItemObj.spec("Z00T00003.SR").value = bomItem.SupplyResponsible;
                    bomItemObj.spec("Z00T00003.ED").value = bomItem.ErectionDiscipline;
                    bomItemObj.spec("Z00T00003.ER").value = bomItem.ErectionResponsible;
                    bomItemObj.spec("Z00T00003.Mat").value = bomItem.MatMatCode;
                    bomItemObj.SetInternationalDescription(4, bomItem.NameCn);
                    bomItemObj.SetInternationalDescription(2, bomItem.NameEn);
                    bomItemObj.spec("Z00T00003.Name").SetInternationalValue(4, bomItem.NameCn);
                    bomItemObj.spec("Z00T00003.Name").SetInternationalValue(2, bomItem.NameEn);
                    bomItemObj.spec("Z00T00003.SpecAll").SetInternationalValue(4, bomItem.SpecAllCn);
                    bomItemObj.spec("Z00T00003.SpecAll").SetInternationalValue(2, bomItem.SpecAllEn);
                    bomItemObj.spec("Z00T00003.Remarks").SetInternationalValue(4, bomItem.RemarksCn);
                    bomItemObj.spec("Z00T00003.Remarks").SetInternationalValue(2, bomItem.RemarksEn);
                }
            }
        }
        private void OnComosDiagObjDelCmdClick(object sender, DiagramItem value)
        {
            if (value == null || value.ObjComosDiagObj == null) return;
            if (string.IsNullOrEmpty(value.GroupID))
            {
                value.ObjComosDiagObj.DeleteAll();
            }
            else
            {
                foreach (var diagItem in VmDiagComos.AssignedDiagramItems)
                {
                    if (diagItem.GroupID == value.GroupID)
                    {
                        diagItem.ObjComosDiagObj.DeleteAll();
                    }
                }
            }
            SetDiagComosAssigned(CurrentObject);
        }
        private void OnComosDiagModClsCmdClick(object sender, DiagramItem value)
        {
            if (value == null || value.ObjComosDiagMod == null) return;
            if (string.IsNullOrEmpty(value.GroupID))
            {
                onComosDiagModClsCmdClick(value.ObjComosDiagMod);
            }
            else
            {
                foreach (var diagItem in VmDiagComos.AvailableDiagramItems)
                {
                    if (diagItem.GroupID == value.GroupID)
                    {
                        onComosDiagModClsCmdClick(diagItem.ObjComosDiagMod);
                    }
                }
            }
            VmAppliedComos.AppliedItems.Clear();
        }
        private void onComosDiagModClsCmdClick(IComosBaseObject diagMod)
        {
            if (diagMod == null) return;
            var appliedDevs = (diagMod as dynamic).BackPointerDevicesWithImplementation;
            for (int i = 1; i <= appliedDevs.Count(); i++)
            {
                IComosBaseObject dev = appliedDevs.Item(i);
                dev.DeleteAll();
            }
        }
        private void OnComosDiagModDelCmdClick(object sender, DiagramItem value)
        {
            if (value == null || value.ObjComosDiagMod == null) return;
            if (string.IsNullOrEmpty(value.GroupID))
            {
                value.ObjComosDiagMod.DeleteAll();
                VmDiagComos.AvailableDiagramItems.Remove(value);
            }
            else
            {
                foreach (var diagItem in VmDiagComos.AvailableDiagramItems.ToList())
                {
                    if (diagItem.GroupID == value.GroupID)
                    {
                        diagItem.ObjComosDiagMod.DeleteAll();
                        VmDiagComos.AvailableDiagramItems.Remove(diagItem);
                    }
                }
            }
           SetDiagComosAssigned(CurrentObject);
        }
        private void OnComosDiagModRSCmdClick(object sender, DiagramItem value)
        {
            if (value == null || value.ObjComosDiagMod == null) return;
            //var appliedCIDevices = GetAppliedCIDevice(value.ObjComosDiagMod);
            //// 被锁定检查，锁定提示、退出； TBA
            //ComosDiagModReset(value);
            //string groupId = value.GroupID;
            //foreach(var ciDev in appliedCIDevices)
            //{
            //    ComosDiagObjCreate(ciDev, value.ObjComosDiagMod);
            //}
            ComosDiagModReset(value);
            if(value == VmDiagComos.SelectedItem)
            {
                VmBomComos.DataSource.Clear();
                SetBomListDataSource(value.ObjComosDiagMod);
                ucPDM.ucBomComos.dgBOM.UpdateLayout();
                ucPDM.ExecComparision(ucPDM.IsComparisonEnabled);
                value.BomQty = VmBomComos.DataSource.Count().ToString();
            }
        }

        private void OnComosDiagAppDelCmdClick(object sender, AppliedComosItem value)
        {
            if (value == null || string.IsNullOrEmpty(value.ComosUID)) return;
            IComosBaseObject obj = Project.Workset().LoadObjectByType(8, value.ComosUID);
            if (obj == null) return;    
            obj.DeleteAll();
            VmAppliedComos.AppliedItems.Remove(value);
            if (ucPDM.UcDM.ActiveArea == ActiveArea.Assigned) 
                SetDiagComosAssigned(CurrentObject);
        }
        private void OnSelectedBomIDChanged(object sender, string value)
        {
            foreach (var item in VmBomComos.DataSource)
            {
                item.IsSameID = (item.ID == value);
            }
            foreach (var item in VmBomLib.DataSource)
            {
                item.IsSameID = (item.ID == value);
            }
        }

        #endregion
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        //INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
