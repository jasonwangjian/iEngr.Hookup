using System;
using System.Collections.Generic;
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
using Comos.UIF;
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
        public UcComosBomMgr()
        {
            InitializeComponent();
            VmBomList = ucBM.ucBL.DataContext as BomListViewModel;
            VmMatList = (ucBM.ucMM.DataContext as MatMainViewModel).VmMatList;
        }
        public BomListViewModel VmBomList;
        public MatListViewModel VmMatList;

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
            //VmBomList.LangInChinese = (HK_General.ProjLanguage == 4);
            //VmBomList.LangInEnglish = (HK_General.ProjLanguage == 2);
            //VmMatList.LangInChinese = (HK_General.ProjLanguage == 4);
            //VmMatList.LangInEnglish = (HK_General.ProjLanguage == 2);
            HK_General.UserName = string.IsNullOrEmpty(Workset.Globals()?.IniRealName) ? Workset.GetCurrentUser().FullName() : Workset.Globals()?.IniRealName;
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
                    for (int i = 1; i <= qry.RowCount; i++)
                    {
                        IComosBaseObject item = qry.RowObject[i];
                        BomListItem bomItem = new BomListItem() { ObjMatBomItem = item };
                        bomItem.SetDataFromComosObject();
                        VmBomList.DataSource.Add(bomItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ___UcBomMgr.SetBomListDataSource Error occurred: {ex.Message}");
            }
        }
    }
}
