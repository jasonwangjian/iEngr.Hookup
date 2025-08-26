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
using System.Diagnostics;
using System.Linq;
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
    //        VMBomList = ucBL.DataContext as BomListViewModel;
    //    }
    //    public BomListViewModel VMBomList;
    //}
    public partial class UcBomMgr : UserControl//, IComosControl, INotifyPropertyChanged
    {
        public UcBomMgr()
        {
            InitializeComponent();
            var VmBomMgr = new BomMgrViewModel();
            DataContext = VmBomMgr;
            // 获取子控件的 ViewModel
            VmBomMgr.VmMatList = ucMM.ucML.DataContext as MatListViewModel;
            VmBomMgr.VmBomList = ucBL.DataContext as BomListViewModel;
        }
    }
}
