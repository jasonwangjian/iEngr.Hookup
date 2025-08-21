using Comos.Controls;
using Comos.UIF;
using ComosQueryInterface;
using ComosQueryXObj;
using iEngr.Hookup.Models;
using iEngr.Hookup.ViewModels;
using Plt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using IContainer = Comos.Controls.IContainer;

namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UcBomMgr.xaml 的交互逻辑
    /// </summary>
    //public partial class UcBomMgr : UserControl
    //{
    //    public UcBomMgr()
    //    {
    //        InitializeComponent();
    //        VMBomList = ucBL.DataContext as BomListViewModels;
    //    }
    //    public BomListViewModels VMBomList;
    //}
    public partial class UcBomMgr : UserControl, IComosControl, INotifyPropertyChanged
    {
        public UcBomMgr()
        {
            InitializeComponent();
            VMBomList = ucBL.DataContext as BomListViewModels;

        }
        public BomListViewModels VMBomList;

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
                    SetBomListVMData(CurrentObject);
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
        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SetBomListVMData(IComosBaseObject currentObject)
        {
            try
            {
                if (currentObject is null) return;
                VMBomList.DiagramNameCn = currentObject.GetInternationalDescription(4);
                VMBomList.DiagramNameEn = currentObject.GetInternationalDescription(2);
                IComosDStandardTable sdt =  Project.CDeviceSystem.Project().GetObjectByPathFullName("22-@40\\~22-Z20\\~22-Z20N00003\\~22-Z20N00003A07");
                IComosDOwnCollection sdtValues = sdt.StandardValues();
                for (int i = 1; i<= sdtValues.Count();i++)
                {
                    IComosDStandardValue stv = sdtValues.Item(i) as IComosDStandardValue;
                    VMBomList.MatMats.Add(new GeneralItem
                    {

                        Code = stv.value,
                        NameCn = stv.GetInternationalDescription(4),
                        NameEn = stv.GetInternationalDescription(2),
                        SpecCn= stv.GetXValue(94),
                        SpecEn= stv.GetXValue(92)
                        
                    });
                    string name = sdtValues.Item(i).Name;

                }


           }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine($" ---------------- UcBomMgr.SetBomListVMData Error occurred: {ex.Message}");
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
                        HKBOM hKBOM = new HKBOM();
                        hKBOM.No = item.Label; ;
                        hKBOM.ObjMat = item;
                        hKBOM.Qty = item.spec("Z00T00003.Qty").value;//qry.Cell(i, "Qty").Text;
                        hKBOM.Unit = item.spec("Z00T00003.Unit").value;
                        hKBOM.SupplyDiscipline = item.spec("Z00T00003.SD").value;
                        hKBOM.SupplyResponsible = item.spec("Z00T00003.SR").value;
                        hKBOM.ErectionDiscipline = item.spec("Z00T00003.ED").value;
                        hKBOM.ErectionResponsible = item.spec("Z00T00003.ER").value;
                        hKBOM.RemarksCn = item.spec("Z00T00003.Remarks").GetInternationalDisplayValue(4);
                        hKBOM.RemarksEn = item.spec("Z00T00003.Remarks").GetInternationalDisplayValue(2);
                        hKBOM.NameCn = item.spec("Z00T00003.Name").GetInternationalDisplayValue(4);
                        hKBOM.NameEn = item.spec("Z00T00003.Name").GetInternationalDisplayValue(2);
                        VMBomList.DataSource.Add(hKBOM);
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine($" ---------------- UcBomMgr.SetBomListDataSource Error occurred: {ex.Message}");
            }
        }

        private void btnNewAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //IComosDProject prj = Workset.GetCurrentProject();
            //IComosBaseObject cdev = prj.GetCDeviceBySystemFullname("@30|M41|A50|A10Z|A10|A10|A60|A30|Z10", 1);

            //IComosBaseObject newMat = prj.Workset().CreateDeviceObject(objQueryStart, cdev);
        }
    }
}
