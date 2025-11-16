using iEngr.Hookup.Models;
using iEngr.Hookup.ViewModels;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace iEngr.Hookup.Views
{
    public enum FileStatus
    {
        NotAssigned,
        NotDxfExtension,
        ValidedDxf,
        InValidedDxf
    }
    /// <summary>
    /// UcDxfViewer.xaml 的交互逻辑
    /// </summary>
    public partial class UcDxfViewer : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<string> ComosUIDToDiagModGet;

        private DxfRenderer _renderer;
        //private ZoomManager _zoomManager;

        private FrameworkElement _parentContainer; // 父容器（Border等）
        private double _scale = 1.0;
        private Point _panOffset = new Point(0, 0);
        private Point? _lastDragPoint;
        private bool _isDragging = false;
        private int _framPixelWidth;
        private int _framPixelHeight;
        public UcDxfViewer()
        {
            InitializeComponent();
            DataContext = this;
            //SetImageSource("pack://application:,,,/iEngr.Hookup;component/Resources/A4LEmpty.png");
            //frameImage.Source = ImageSource;
            //_framPixelWidth = ImageSource.PixelWidth;
            //_framPixelHeight = ImageSource.PixelHeight;
            _parentContainer = FindParentContainer(zoomCanvas);
            MouseEventIni();
            _renderer = new DxfRenderer(zoomCanvas);
            ApplyTransform();
            Reset();
            // 监听图像大小变化
            zoomCanvas.SizeChanged += zoomCanvas_SizeChanged;
         }
        private FrameworkElement FindParentContainer(Canvas canvas)
        {
            // 查找第一个非Canvas的父容器
            DependencyObject parent = VisualTreeHelper.GetParent(canvas);
            while (parent != null)
            {
                if (parent is FrameworkElement frameworkElement &&
                    !(parent is Canvas) &&
                    !(parent is Window))
                {
                    return frameworkElement;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }        
        #region Zoom Manager
        private void MouseEventIni()
        {
            MouseWheel += (s, e) =>
            {
                Point mouseCanvasPos = e.GetPosition(this);
                double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
                double newScale = Math.Max(0.1, Math.Min(20.0, _scale * zoomFactor));

                double scaleChange = newScale / _scale;

                double oldTranslateX = _panOffset.X;
                double oldTranslateY = _panOffset.Y;

                _panOffset.X = oldTranslateX + (1 - scaleChange) * (mouseCanvasPos.X - oldTranslateX);
                _panOffset.Y = oldTranslateY + (1 - scaleChange) * (mouseCanvasPos.Y - oldTranslateY);

                _scale = newScale;

                ApplyTransform();
                e.Handled = true;
            };
            // 鼠标拖动平移 - 使用Preview事件确保能捕获到
            PreviewMouseDown += (s, e) =>
            {
                if (e.MiddleButton == MouseButtonState.Pressed ||
                    (e.LeftButton == MouseButtonState.Pressed)) // && Keyboard.Modifiers == ModifierKeys.Shift))
                {
                    _isDragging = true;
                    _lastDragPoint = e.GetPosition(this);
                    Cursor = Cursors.Hand;
                    CaptureMouse(); // 重要：捕获鼠标
                    e.Handled = true;
                }
            };

            PreviewMouseMove += (s, e) =>
            {
                if (_isDragging && _lastDragPoint.HasValue) // && e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentPos = e.GetPosition(this);
                    Vector delta = currentPos - _lastDragPoint.Value;

                    double newPanX = _panOffset.X + delta.X;
                    double newPanY = _panOffset.Y + delta.Y;

                    if (newPanX > 200)
                    {

                    }
                    ApplyPanConstraints(ref newPanX, ref newPanY);

                    // 应用约束

                    _panOffset.X = newPanX;
                    _panOffset.Y = newPanY;

                    //_panOffset.X += delta.X;
                    //_panOffset.Y += delta.Y;

                    _lastDragPoint = currentPos;
                    ApplyTransform();
                    e.Handled = true;
                }
            };

            PreviewMouseUp += (s, e) =>
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    Cursor = Cursors.Arrow;
                    ReleaseMouseCapture(); // 释放鼠标捕获
                    e.Handled = true;
                }
            };

            // 确保即使鼠标在Canvas外也能释放捕获
            LostMouseCapture += (s, e) =>
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    Cursor = Cursors.Arrow;
                }
            };

        }
        private void ApplyPanConstraints(ref double panX, ref double panY)
        {
            if (_parentContainer == null) return;
            // 计算父容器的可见区域（世界坐标）
            double visibleWidth = _parentContainer.ActualWidth * 0.8 * _scale;
            double visibleHeight = _parentContainer.ActualHeight * 0.8 * _scale; 
            panX = panX > 0 ? Math.Min(visibleWidth, panX) : Math.Max(-visibleWidth, panX);
            panY = panY > 0 ? Math.Min(visibleHeight, panY) : Math.Max(-visibleHeight, panY);
        }

        public void Reset()
        {
            _scale = 1.0;
            _panOffset = new Point(0, 0);
            ApplyTransform();
            //if (IsImageLargerThanWindow())
            //{
            //    double scaleX = zoomCanvas.ActualWidth / _framPixelWidth;
            //    double scaleY = zoomCanvas.ActualHeight / _framPixelHeight;
            //    double scaleImage = Math.Min(scaleX, scaleY) * 0.95;

            //    double imageWidth = _framPixelWidth * scaleImage;
            //    double imageHeight = _framPixelHeight * scaleImage;
            //    Point offsetImage = new Point(Math.Max(0, (zoomCanvas.ActualWidth - imageWidth) / 2),
            //                                  Math.Max(0, (zoomCanvas.ActualHeight - imageHeight) / 2));
            //    ApplyTransform(scaleImage, offsetImage);
            //}
            //else
            //{
            //    ApplyTransform(1.0, new Point(0, 0));
            //}
        }
        //private bool IsImageLargerThanWindow()
        //{
        //    if (!(frameImage.Source is BitmapSource bitmap))
        //        return false;

        //    double imageWidth = bitmap.PixelWidth;
        //    double imageHeight = bitmap.PixelHeight;
        //    double canvasWidth = zoomCanvas.ActualWidth;
        //    double canvasHeight = zoomCanvas.ActualHeight;

        //    return imageWidth > canvasWidth * 0.95 || imageHeight > canvasHeight * 0.95;
        //}
        private void ApplyTransform()
        {
            if (zoomCanvas == null) return;
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_scale, _scale));
            transformGroup.Children.Add(new TranslateTransform(_panOffset.X, _panOffset.Y));
            zoomCanvas.RenderTransform = transformGroup;
        }
        //private void ApplyTransform(double scale, Point offset)
        //{
        //    if (frameImage == null) return;
        //    TransformGroup transformGroup = new TransformGroup();
        //    transformGroup.Children.Add(new ScaleTransform(scale, scale));
        //    transformGroup.Children.Add(new TranslateTransform(offset.X, offset.Y));
        //    frameImage.RenderTransform = transformGroup;
        //}
        #endregion
        #region 属性
        private FileStatus _fileStatus;
        public FileStatus FileStatus
        {
            get => _fileStatus;
            set
            {
                if (SetField(ref _fileStatus, value))
                    OnPropertyChanged(nameof(IsValidFile));
            }
        }
        private AttachTo _attachTo;
        public AttachTo AttachTo
        {
            get => _attachTo;
            set
            {
                if (SetField(ref _attachTo, value))
                    OnPropertyChanged(nameof(AllowDrop));
            }
        }
        public new bool AllowDrop
        {
            get
            {
                if (AttachTo == AttachTo.ComosAssigned || AttachTo == AttachTo.ComosAvailable)
                {
                    return true;
                }
                return false;
            }
        }
        private BitmapImage _imageSource;
        public BitmapImage ImageSource
        {
            get => _imageSource;
            set => SetField(ref _imageSource, value);
        }
        private bool IsValidFile {
            get
            {
                return FileStatus == FileStatus.ValidedDxf;
            }
        }
        private string _dxfPath;
        public string DxfPath
        {
            get => _dxfPath;
            set
            {
                if (SetField(ref _dxfPath, value))
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        FileStatus = FileStatus.NotAssigned;
                    }
                    else
                        OpenFile(value);
                }
            }
        }
        private double _positionX;
        public double PositionX
        {
            get => _positionX;
            set => SetField(ref _positionX, value);
        }
        private double _positionY;
        public double PositionY
        {
            get => _positionY;
            set => SetField(ref _positionY, value);
        }
        #endregion
        public void SetImageSource(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            if (filePath == null) return;
            bitmap.UriSource = new Uri(filePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // 确保立即加载
            bitmap.EndInit();
            ImageSource = bitmap;
        }
        private void OpenFile(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".dxf")
            {
                FileStatus = _renderer.RenderDxf(filePath,_scale);
            }
            else
            {
                FileStatus = FileStatus.NotDxfExtension; ;
            }
        }
        private void zoomCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Canvas大小发生变化时居中
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                FileStatus = _renderer.RenderDxf(DxfPath, _scale);
                Reset();
            }
        }

        private void FitToWindow_Click(object sender, RoutedEventArgs e)
        {
            //FitToView(_renderer.EntityObjects);
            Reset();
        }

        private void frameImage_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string comosDataString = e.Data.GetData("Text").ToString();
                ComosUIDToDiagModGet?.Invoke(this, comosDataString);
            }
            catch { }   
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void Test_Click(object sender, RoutedEventArgs e)
        {
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "DXF文件 (*.dxf)|*.dxf|所有文件 (*.*)|*.*",
                Title = "打开DXF文件"
            };

            if (dialog.ShowDialog() == true)
            {
               DxfPath =  dialog.FileName;
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point windowPosition = e.GetPosition(this);
            PositionX = windowPosition.X;
            PositionY = windowPosition.Y;
        }
    }
}