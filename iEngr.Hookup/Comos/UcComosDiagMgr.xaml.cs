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
            VmPicture = ucPDM.ucPic.DataContext as HkPictureViewModel;

            VmDiagComos = ucPDM.ucDiagComos.DataContext as DiagItemsViewModel;
            VmBomComos = ucPDM.ucBomComos.DataContext as BomItemsViewModel;
            VmDiagComos.ComosDiagChanged += OnComosDiagramChanged;
            VmDiagComos.ComosPicturePathSet += OnComosPicturePathSet;
            VmDiagComos.ComosDiagObjDelCmd += OnComosDiagObjDelCmdClick;
            VmDiagComos.ComosDiagModClsCmd += OnComosDiagModClsCmdClick;
            VmDiagComos.ComosDiagModDelCmd += OnComosDiagModDelCmdClick;

            VmDiagLib = ucPDM.ucDiagLib.DataContext as DiagItemsViewModel;
            VmDiagLib.ComosDiagModAddCmd += OnComosDiagAppCmdClick;
            VmBomLib = ucPDM.ucBomLib.DataContext as BomItemsViewModel;

            VmAppliedComos = ucPDM.ucComosDevs.DataContext as AppliedComosViewModel;
            VmAppliedComos.ComosItemSelected += OnComosItemSelected;
            VmAppliedComos.ComosDiagAppDelCmd += OnComosDiagAppDelCmdClick;
            VmAppliedComos.ComosItemContextMenu += PopContextMenu;

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
                    IComosBaseObject itenLinkObj = item.spec("Y00T00103.Y00A00641")?.LinkObject;
                    DiagramItem diagItem = new DiagramItem() { ObjComosDiagMod = itenLinkObj, ObjComosDiagObj = item, BomQty="?", IsOwned = true };
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
            //Project.Workset().Globals().Navigator.GetCurrentTree.DefaultAction(obj);
            //Set mntc = createobject("Chemserv.ComosClass")
            //mntc.ShowComosObject(c) '弹出浮窗
            //Set Mntc = Nothing
            Chemserv.ComosClass mntc = new Chemserv.ComosClass();
            mntc.ShowComosObject(obj);
        }
        private void PopContextMenu(object sender, AppliedComosItem value)
        {
            if (value == null || value.ComosObj == null) return;
            //IComosBaseObject obj = Project.Workset().LoadObjectByType(8, value.ComosUID);
            Comos_ContextMenuOpening(value.ComosObj);
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
            IComosBaseObject obj = Project.Workset().LoadObjectByType(8, comosUID);
            var diagObj = ComosDiagObjCreate(obj, VmDiagComos.SelectedItem?.ObjComosDiagMod);
            if (diagObj != null)
            {
                VmAppliedComos.AppliedItems.Add(GetAppliedComosItem(diagObj));
            }
        }
        private AppliedComosItem GetAppliedComosItem(IComosBaseObject diagObj)
        {
            IComosBaseObject ciDev = diagObj.owner().owner();
            return new AppliedComosItem()
            {
                ComosUID = diagObj.SystemUID(),
                AssignMode = diagObj.spec("Y00T00103.AssignMode").value,
                IsLocked = diagObj.spec("Y00T00103.IsLocked").value,
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
                if (VmAppliedComos.AppliedItems.Any(x => x.ComosObj == ciDevice))
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
        private void OnComosDiagAppCmdClick(object sender, DiagramItem value)
        {
            if (value == null || DiagModFolder == null) return;
            IComosBaseObject diagMod = Project.Workset().CreateDeviceObject(DiagModFolder, CDevDiagMod);
            if (diagMod != null)
            {
                diagMod.spec("Y00T00103.PicturePath").value = value.PicturePath;
                diagMod.spec("Y00T00103.RefIdInLib").value = value.DisplayID;
                diagMod.SetInternationalDescription(4, value.NameCn);
                diagMod.SetInternationalDescription(2, value.NameEn);
                diagMod.spec("Y00T00103.Remarks").SetInternationalValue(4, value.RemarksCn);
                diagMod.spec("Y00T00103.Remarks").SetInternationalValue(2, value.RemarksEn);
                //IdLabels = value.spec("Y00T00103.IdLabels").value;
                if (VmTree.SelectedItem != null)
                {
                    diagMod.spec("Y00T00103.IdLabels").value = VmTree.SelectedItem.GetPropertiesStrings();
                }
                DiagramItem newAddedDiag = new DiagramItem() { ObjComosDiagMod = diagMod };
                VmDiagComos.AvailableDiagramItems.Add(newAddedDiag);
                //添加BOM
                foreach (var bomItem in VmBomLib.DataSource)
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
                VmDiagComos.AvailableDiagramsSelectedItem = newAddedDiag;
            };
        }
        private void OnComosDiagObjDelCmdClick(object sender, DiagramItem value)
        {
            if (value == null || value.ObjComosDiagObj == null) return;
            value.ObjComosDiagObj.DeleteAll();
            SetDiagComosAssigned(CurrentObject);
        }
        private void OnComosDiagModClsCmdClick(object sender, DiagramItem value)
        {
            if (value == null || value.ObjComosDiagMod == null) return;
            var appliedDevs = (value.ObjComosDiagMod as dynamic).BackPointerDevicesWithImplementation;
            for (int i = 1; i <= appliedDevs.Count(); i++)
            {
                IComosBaseObject dev = appliedDevs.Item(i);
                dev.DeleteAll();
            }
            VmAppliedComos.AppliedItems.Clear();
        }
        private void OnComosDiagModDelCmdClick(object sender, DiagramItem value)
        {
            if (value == null || value.ObjComosDiagMod == null) return;
            value.ObjComosDiagMod.DeleteAll();
            VmDiagComos.AvailableDiagramItems.Remove(value);
            SetDiagComosAssigned(CurrentObject);
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
