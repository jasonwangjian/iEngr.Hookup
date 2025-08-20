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
    public partial class UcBomMgr : UserControl, IComosControl, INotifyPropertyChanged
    {
        public UcBomMgr()
        {
            InitializeComponent();
            VMBomList = ucBL.DataContext as BomListViewModels;
        }
        public BomListViewModels VMBomList;
        private IComosDGeneralCollection _objects;
        private IComosDWorkset _workset;
        private string _parameters;
        private IContainer _controlContainer;

        public IComosBaseObject CurrentObject;
        public IComosDWorkset Workset
        {
            get => _workset;
            set => _workset = value;
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
                    GetQueryResult();
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

        private void GetQueryResult()
        {
            try
            {
                if (CurrentObject is null) return;
                IComosDProject prj = Workset.GetCurrentProject();
                IComosBaseObject cdev = prj.GetCDeviceBySystemFullname("@30|M41|A50|A10Z|A10|A10|A60|A30|Z10", 1);

                IComosBaseObject newMat = prj.Workset().CreateDeviceObject(CurrentObject, cdev);
                IComosDCDevice qrydev = prj.GetCDeviceBySystemFullname("@20|A70|Z10|A20|QHkBom", 1);
                if (null != qrydev)
                {
                    ITopQuery tqry = ((QueryXObj)qrydev.XObj).TopQuery;
                    tqry.MainObject = CurrentObject;
                    tqry.Execute();
                    IQuery qry = tqry.Query;
                    for (int i = 1; i <= qry.RowCount; i++)
                    {
                        IComosBaseObject item = qry.RowObject[i];
                        string no = item.Label;
                        HKBOM hKBOM = new HKBOM();
                        hKBOM.No = no;
                        hKBOM.ObjMat = item;
                        hKBOM.Qty = qry.Cell(i, "Qty").Text;
                        hKBOM.SupplyDiscipline = item.spec("Z00T00003.SD").value;
                        //hKBOM.SupplyResponsible = qry.Cell(i, "SR").Value;
                        //hKBOM.ErectionDiscipline = qry.Cell(i, "ED").Text;
                        //hKBOM.ErectionResponsible = qry.Cell(i, "ER").Text;
                        VMBomList.DataSource.Add(hKBOM);
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine($" ---------------- UcBomMgr.GetQueryResult() Error occurred: {ex.Message}");
            }
        }
    }
}
