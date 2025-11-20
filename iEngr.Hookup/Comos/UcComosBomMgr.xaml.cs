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
using Comos.Controls;
using Comos.CtxMenu;
using Comos.UIF;
using ComosPPGeneral;
using ComosQueryInterface;
using ComosQueryXObj;
using iEngr.Hookup;
using iEngr.Hookup.Models;
using iEngr.Hookup.ViewModels;
using iEngr.Hookup.Views;
using Plt;
using IContainer = Comos.Controls.IContainer;

namespace iEngr.Hookup.Comos
{
    /// <summary>
    /// UcComosBomMgr.xaml 的交互逻辑
    /// </summary>
    public partial class UcComosBomMgr : UserControl, IComosControl, INotifyPropertyChanged
    {
        private PopupDefinitions m_ppDef;
        private CtxMenu m_popup;
        private UserComos userCurrent;
        public UcComosBomMgr()
        {
            InitializeComponent();
            VmBomMgr = ucBM.DataContext as BomMgrViewModel;
            VmBomMgr.DiagramNameCnChanged += OnDiagramNameCnChanged;
            VmBomMgr.DiagramNameEnChanged += OnDiagramNameEnChanged;
            VmBomMgr.DiagramRemarksCnChanged += OnDiagramRemarksCnChanged;
            VmBomMgr.DiagramRemarksEnChanged += OnDiagramRemarksEnChanged;
            VmBomList = ucBM.ucBL.DataContext as BomItemsViewModel;
            VmMatList = (ucBM.ucMM.DataContext as MatMainViewModel).VmMatList;
            VmMatList.IsAdminBtnShow = false;
            VmPic = ucBM.ucPic.DataContext as HkPictureViewModel;
            VmApplied = ucBM.ucComosDevs.DataContext as AppliedComosViewModel;
            VmApplied.IsRemoveShow = false;
            VmApplied.ComosDiagAppDelCmd += OnComosDiagAppDelCmdClick;
            VmApplied.ComosItemContextMenu += PopContextMenu;
        }
        public BomMgrViewModel VmBomMgr;
        public BomItemsViewModel VmBomList;
        public MatListViewModel VmMatList;
        public HkPictureViewModel VmPic;
        public AppliedComosViewModel VmApplied;

        private IComosDGeneralCollection _objects;
        private string _parameters;
        private IContainer _controlContainer;

        private IComosDWorkset _workset;
        public IComosDWorkset Workset
        {
            get => _workset;
            set
            {
                _workset = value;
                Project = Workset.GetCurrentProject();
                ProjectInitial();
                CurrentUserInitial();
                VmBomList.Project = Project;
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
                    SetBomListVmData(CurrentObject);
                    SetBomListDataSource(CurrentObject);
                    SetDiagramData(CurrentObject);
                }
            }
        }
        public string Parameters //在Menu的Main.xml定义的
        {
            get => _parameters;
            set => SetField(ref _parameters, value);
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
                        ((IUIForm)p).Header.Description = $"安装图材料清单 {GetType().FullName}";
                    }
                }
            }
        }

        private void ProjectInitial()
        {
            HK_General.ProjLanguage = int.TryParse(Project.CurrentLanguage.FullName(), out int projLanguange) ? projLanguange : 4;
            HK_General.UserName = string.IsNullOrEmpty(Workset.Globals()?.IniRealName) ? Workset.GetCurrentUser().FullName() : Workset.Globals()?.IniRealName;
        }
        private void CurrentUserInitial()
        {
            userCurrent = new UserComos(Workset.GetCurrentUser());
            string strUserCurrID = userCurrent.ComosDUser.GetRemark(5);
            if (!string.IsNullOrEmpty(strUserCurrID))
            {
                userCurrent.ObjUser = (IComosBaseObject)Workset.LoadObjectByType(8, strUserCurrID);
                userCurrent.RealName = userCurrent.ObjUser?.spec("Y00T00026.Y00A01154").DisplayValue();
                userCurrent.HaisumId = userCurrent.ObjUser?.spec("Y00T00026.Y00A02688").DisplayValue();
                IComosDCDevice cDevUser = Project.GetCDeviceBySystemFullname("@20|B30|A10|Z10", 1);
                var col = Project.GetObjectByPathFullName("08U@ProjectManagement").ScanDevicesWithCObject("", cDevUser);
                for (int i = 0; i < col.Count(); i++)
                {
                    IComosBaseObject user = col.Item(i + 1);
                    if (user.spec("Z00A09998").LinkObject?.SystemUID == strUserCurrID)
                    {
                        if (user.spec("Z00A00032").StandardValue() != null &&
                            user.spec("Z00A00032").value == "I" &&
                            user.spec("Z00A00033").StandardValue() != null)
                        {
                            switch (user.spec("Z00A00033").value)
                            {
                                case "AP":
                                    userCurrent.Roles += HK_General.RoleAP;
                                    break;
                                case "MG":
                                    userCurrent.Roles += HK_General.RoleMG;
                                    break;
                                case "VF":
                                    userCurrent.Roles += HK_General.RoleVF;
                                    break;
                                case "DL":
                                    userCurrent.Roles += HK_General.RoleDL;
                                    break;
                                case "CK":
                                    userCurrent.Roles += HK_General.RoleCK;
                                    break;
                                case "RE":
                                    userCurrent.Roles += HK_General.RoleRE;
                                    break;
                                case "AE":
                                    userCurrent.Roles += HK_General.RoleAE;
                                    break;
                            }
                        }
                    }
                }
            }
            if (userCurrent.ComosDUser.AdminRights == 1) // administrator
                userCurrent.Roles += HK_General.RoleAdmin;
            if (userCurrent.ComosDUser.ExtendedRights == 1) // Project Manager
                userCurrent.Roles += HK_General.RoleMG;
            if ((userCurrent.Roles & HK_General.RoleAdmin) > 0)
                VmMatList.IsAdminBtnShow = true;
            if (userCurrent.Roles == 0)
                IsEnabled = false;
        }
        public void OnCanExecute(CanExecuteRoutedEventArgs e) { }
        public void OnPreviewExecuted(ExecutedRoutedEventArgs e) { }
        public void OnExecuted(ExecutedRoutedEventArgs e) { }
        public IComosDProject Project { set; get; }
        public IComosBaseObject _currentObject;
        public IComosBaseObject CurrentObject
        {
            get => _currentObject;
            set => SetField(ref _currentObject, value);
        }
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

        private void SetBomListVmData(IComosBaseObject currentObject)
        {
            try
            {
                if (currentObject is null) return;
                VmBomList.CurrentObject = currentObject;
                VmBomList.DiagramNameCn = currentObject.GetInternationalDescription(4);
                VmBomList.DiagramNameEn = currentObject.GetInternationalDescription(2);
                //// 读取标准表数据
                //IComosDStandardTable sdt = Project.CDeviceSystem.Project().GetObjectByPathFullName("22-@40\\~22-Z20\\~22-Z20N00003\\~22-Z20N00003A07");
                //IComosDOwnCollection sdtValues = sdt.StandardValues();
                //for (int i = 1; i <= sdtValues.Count(); i++)
                //{
                //    IComosDStandardValue stv = sdtValues.Item(i) as IComosDStandardValue;
                //    VmBomList.MatMats.Add(new GeneralItem
                //    {

                //        Code = stv.value,
                //        NameCn = stv.GetInternationalDescription(4),
                //        NameEn = stv.GetInternationalDescription(2),
                //        SpecCn = stv.GetXValue(94),
                //        SpecEn = stv.GetXValue(92)

                //    });
                //    string name = sdtValues.Item(i).Name;

                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcBomMgr.SetBomListVMData, Error occurred: {ex.Message}");
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
                    List<BomItem> bomItems = new List<BomItem>();
                    for (int i = 1; i <= qry.RowCount; i++)
                    {
                        IComosBaseObject item = qry.RowObject[i];
                        BomItem bomItem = new BomItem() { ObjComosBomItem = item };
                        //bomItem.SetDataFromComosObject();
                        bomItems.Add(bomItem);
                    }
                    VmBomList.DataSource = new ObservableCollection<BomItem>(bomItems.OrderBy(x => x.No));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcBomMgr.SetBomListDataSource Error occurred: {ex.Message}");
            }
        }
        private void SetDiagramData(IComosBaseObject currentObject)
        {
            try
            {
                if (currentObject is null) return;
                VmBomMgr.DiagramNameCn = currentObject.GetInternationalDescription(4);
                VmBomMgr.DiagramNameEn = currentObject.GetInternationalDescription(2);
                VmBomMgr.DiagramRemarksCn = currentObject.spec("Y00T00103.Remarks").GetInternationalValue(4);
                VmBomMgr.DiagramRemarksEn = currentObject.spec("Y00T00103.Remarks").GetInternationalValue(2);
                VmPic.PicturePath = currentObject.spec("Y00T00103.PicturePath").value;
                //刷新VmApplied.AppliedItems
                VmApplied.AppliedItems.Clear();
                var appliedDevs = (currentObject as dynamic).BackPointerDevicesWithImplementation;
                for (int i = 1; i <= appliedDevs.Count(); i++)
                {
                    IComosBaseObject dev = appliedDevs.Item(i);
                    VmApplied.AppliedItems.Add(GetAppliedComosItem(dev));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcBomMgr.SetDiagramData(IComosBaseObject currentObject), Error occurred: {ex.Message}");
            }
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
                Debug.WriteLine($" ___UcComosBomMgr.Comos_ContextMenuOpening(IComosBaseObject obj) Error occurred: {ex.Message}");
                //System.Diagnostics.Trace.WriteLine(ex);
            }
        }
        private void OnComosDiagAppDelCmdClick(object sender, AppliedComosItem value)
        {
            if (value == null || string.IsNullOrEmpty(value.ComosUID)) return;
            IComosBaseObject obj = Project.Workset().LoadObjectByType(8, value.ComosUID);
            if (obj == null) return;
            obj.DeleteAll();
            VmApplied.AppliedItems.Remove(value);
        }
        private void OnDiagramNameCnChanged(object sender, string value)
        {
            if (value == null) return;
            CurrentObject.SetInternationalDescription(4, value);
        }
        private void OnDiagramNameEnChanged(object sender, string value)
        {
            if (value == null) return;
            CurrentObject.SetInternationalDescription(2, value);
        }
        private void OnDiagramRemarksCnChanged(object sender, string value)
        {
            if (value == null) return;
            CurrentObject.spec("Y00T00103.Remarks").SetInternationalValue(4, value);
        }
        private void OnDiagramRemarksEnChanged(object sender, string value)
        {
            if (value == null) return;
            CurrentObject.spec("Y00T00103.Remarks").SetInternationalValue(2, value);
        }
    }
}
