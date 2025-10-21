using Comos.Controls;
using Comos.UIF;
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
using IContainer = Comos.Controls.IContainer;

namespace iEngr.Hookup.Comos
{
    /// <summary>
    /// UcComosDiagMgr.xaml 的交互逻辑
    /// </summary>
    public partial class UcComosDiagMgr : UserControl, IComosControl, INotifyPropertyChanged
    {
        public UcComosDiagMgr()
        {
            InitializeComponent();
            VmDiagComos = ucPDM.ucDiagComos.DataContext as DiagItemsViewModel;
            VmBomComos = ucPDM.ucBomComos.DataContext as BomListViewModel;
            VmDiagComos.ComosDiagChanged += OnComosDiagramIDChanged;

        }
        public DiagItemsViewModel VmDiagComos;
        public BomListViewModel VmBomComos;

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
        public IComosBaseObject DefaultPrjDiagHolder { get; set; }
        public IComosBaseObject ProjectCfgCI { get; set; }
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
            IComosDCDevice prjDiagHolder = Project.GetCDeviceBySystemFullname("@20|A10|A20|M41|A60", 1);
            if (prjDiagHolder == null) return;
            var col = ProjectCfgCI.ScanDevicesWithCObject("", prjDiagHolder);
            if (col == null || col.Count()==0)
            {
                DefaultPrjDiagHolder = Project.Workset().CreateDeviceObject(ProjectCfgCI, prjDiagHolder);
            }
            else
            {
                DefaultPrjDiagHolder = col.Item(1);
            }
            IsComosProjectIniOK = true;
        }
        private void SetDiagComosAvailable(IComosBaseObject objQueryStart)
        {
            try
            {
                //if (objQueryStart is null) return;
                IComosDCDevice devHkFolder = Project.GetCDeviceBySystemFullname("@20|A10|A10|M41|Z10", 1);
                if ((objQueryStart as dynamic).CDevice()?.IsInheritSuccessorFrom(devHkFolder) == true)
                {
                    objQueryStart = DefaultPrjDiagHolder;
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
                        DiagramItem diagItem = new DiagramItem() { ObjComosDiag = item };
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
                VmDiagComos.AssignedDiagramItems.Clear();
                ObservableCollection<DiagramItem> diagramItems = GetDiagComosItems(objQueryStart);
                var objComosDiags = diagramItems.Select(x => x.ObjComosDiag);
                foreach(DiagramItem item in VmDiagComos.AvailableDiagramItems)
                {
                    if (objComosDiags.Contains(item.ObjComosDiag))
                    {
                        VmDiagComos.AssignedDiagramItems.Add(item);
                    }
                }
                var nullDiagramItems = diagramItems.Where(x => x.ObjComosDiag == null);
                foreach( DiagramItem item in nullDiagramItems ) 
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
                IComosDCDevice devHkFolder = Project.GetCDeviceBySystemFullname("@20|A10|A10|M41|Z10", 1);
                if ((objQueryStart as dynamic).CDevice()?.IsInheritSuccessorFrom(devHkFolder) != true) return diagrams;
                ITopQuery tqry = ((QueryXObj)qrydev.XObj).TopQuery;
                tqry.MainObject = objQueryStart;
                tqry.Execute();
                IQuery qry = tqry.Query;
                for (int i = 1; i <= qry.RowCount; i++)
                {
                    IComosBaseObject item = qry.RowObject[i];
                    IComosBaseObject itenLinkObj = item.spec("Y00T00103.Y00A00641")?.LinkObject;
                    DiagramItem diagItem = new DiagramItem() { ObjComosDiag = itenLinkObj, IsOwned=true};
                    if (itenLinkObj == null)
                    {
                        diagItem.RefID = item.Name;
                        diagItem.NameCn = item.GetInternationalDescription(4);
                        diagItem.NameEn = item.GetInternationalDescription(2);
                    }
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
        private void OnComosDiagramIDChanged(object sender, IComosBaseObject value)
        {
            VmBomComos.DataSource.Clear();
            if (value != null)
                SetBomListDataSource(value);
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
                        BomListItem bomItem = new BomListItem() { ObjMatBomItem = item };
                        bomItem.SetDataFromComosObject();
                        VmBomComos.DataSource.Add(bomItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcComosDiagMgr.SetBomListDataSource(IComosBaseObject objQueryStart) Error occurred: {ex.Message}");
            }
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
    }
}
