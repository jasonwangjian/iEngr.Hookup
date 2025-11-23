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
    /// <summary>
    /// UcDxfViewer.xaml 的交互逻辑
    /// </summary>
    public partial class UcDxfPicPdf : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<string> ComosUIDToDiagModGet;

        HkDxfPicPdfViewModel VmDxfPic;

        private FrameworkElement _parentContainer; // 父容器（Border等）
        private double _scale = 1.0;
        private Point _panOffset = new Point(0, 0);
        private Point? _lastDragPoint;
        private bool _isDragging = false;
        private int _framPixelWidth;
        private int _framPixelHeight;
        public UcDxfPicPdf()
        {
            InitializeComponent();
            VmDxfPic = new HkDxfPicPdfViewModel(dxfCanvas);
            DataContext = VmDxfPic;
            //DataContext = this;
            //SetImageSource("pack://application:,,,/iEngr.Hookup;component/Resources/A4LEmpty.png");
            //frameImage.Source = ImageSource;
            //_framPixelWidth = ImageSource.PixelWidth;
            //_framPixelHeight = ImageSource.PixelHeight;
            _parentContainer = FindParentContainer(dxfCanvas);
            MouseEventIni();
            ApplyTransform();
            ViewReset();
            // 监听图像大小变化
            dxfCanvas.SizeChanged += zoomCanvas_SizeChanged;
            contentImage.SizeChanged += ContentImage_SizeChanged;
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
                if (!VmDxfPic.IsDxfFile) return;
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
                if (!VmDxfPic.IsDxfFile) return;
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
                if (!VmDxfPic.IsDxfFile) return;
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

        public void ViewReset()
        {
            _scale = 1.0;
            _panOffset = new Point(0, 0);
            ApplyTransform();
        }
        private void ApplyTransform()
        {
            if (dxfCanvas == null) return;
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_scale, _scale));
            transformGroup.Children.Add(new TranslateTransform(_panOffset.X, _panOffset.Y));
            dxfCanvas.RenderTransform = transformGroup;
        }
        #endregion


        private void zoomCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Canvas大小发生变化时居中
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                VmDxfPic.DxfRenderer.RenderDxfRefresh();
                ViewReset();
            }
        }

        private void FitToView_Click(object sender, RoutedEventArgs e)
        {
            //FitToView(_renderer.EntityObjects);
            ViewReset();
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
            // 定义文件类型常量
            const string dxfFilter = "*.dxf";
            const string imageFilter = "*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif";
            const string pdfFilter = "*.pdf";
            const string allSupportedFilter = "*.dxf;*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif;*.pdf";

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = $"DXF文件 ({dxfFilter})|{dxfFilter}|" +
                         $"图像文件 ({imageFilter})|{imageFilter}|" +
                         $"PDF文件 ({pdfFilter})|{pdfFilter}|" +
                         $"所有支持的文件 ({allSupportedFilter})|{allSupportedFilter}|" +
                         $"所有文件 (*.*)|*.*",
                Title = "打开DXF、图片和PDF文件",
                InitialDirectory = Properties.Settings.Default.LastSelectedFileDirectory,
                FilterIndex = Properties.Settings.Default.LastFileFilter
            };

            if (dialog.ShowDialog() == true)
            {
                Properties.Settings.Default.LastSelectedFileDirectory = System.IO.Path.GetDirectoryName(dialog.FileName);
                Properties.Settings.Default.LastFileFilter = dialog.FilterIndex;
                Properties.Settings.Default.Save();
                VmDxfPic.PicturePath = dialog.FileName;
            }
        }
        #region 图像查看功能（鼠标动作）
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (contentImage.Source == null) return;

            // 缩放时禁用自动适应
            _autoFitToWindow = false;

            Point mouseCanvasPos = e.GetPosition(zoomCanvas);

            double zoomFactor = e.Delta > 0 ? 1.2 : 0.8;
            double newScale = Math.Max(0.1, Math.Min(20.0, _currentScale * zoomFactor));

            double scaleChange = newScale / _currentScale;

            double oldTranslateX = imageTranslate.X;
            double oldTranslateY = imageTranslate.Y;

            _currentScale = newScale;
            imageScale.ScaleX = _currentScale;
            imageScale.ScaleY = _currentScale;

            imageTranslate.X = oldTranslateX + (1 - scaleChange) * (mouseCanvasPos.X - oldTranslateX);
            imageTranslate.Y = oldTranslateY + (1 - scaleChange) * (mouseCanvasPos.Y - oldTranslateY);

            e.Handled = true;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _autoFitToWindow = false;
                _lastDragPoint = e.GetPosition(zoomCanvas);
                _isDragging = true;
                zoomCanvas.CaptureMouse();
                zoomCanvas.Cursor = Cursors.Hand;
                e.Handled = true;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _isDragging)
            {
                _isDragging = false;
                _lastDragPoint = null;
                zoomCanvas.ReleaseMouseCapture();
                zoomCanvas.Cursor = Cursors.Arrow;
                e.Handled = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)// && _lastDragPoint.HasValue) // && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPos = e.GetPosition(zoomCanvas);
                Vector delta = currentPos - _lastDragPoint.Value;

                imageTranslate.X += delta.X;
                imageTranslate.Y += delta.Y;

                _lastDragPoint = currentPos;
                e.Handled = true;
            }
        }
        #endregion

        #region for Picture and pdf
        private double _currentScale = 1.0;
        private bool _autoFitToWindow = true;
        private void ContentImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 图像大小发生变化时居中
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                CenterImageAfterLoad();
            }
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (contentImage.Source == null) return;

            _autoFitToWindow = true;

            if (IsImageLargerThanWindow())
            {
                FitToWindow(true);
            }
            else
            {
                ResetToCenter();
            }
        }
        private void FitToWindow_Click(object sender, RoutedEventArgs e)
        {
            //_autoFitToWindow = false;
            FitToWindow();
        }
        private void FitToWindow(bool isAutoFit = false)
        {
            if (contentImage.Source is BitmapSource bitmap)
            {
                double scaleX = zoomCanvas.ActualWidth / bitmap.PixelWidth;
                double scaleY = zoomCanvas.ActualHeight / bitmap.PixelHeight;
                _currentScale = Math.Min(scaleX, scaleY);

                imageScale.ScaleX = _currentScale;
                imageScale.ScaleY = _currentScale;

                CenterImage();

                if (!isAutoFit)
                {
                    _autoFitToWindow = false;
                }
            }
        }
        private void ResetToCenter()
        {
            _currentScale = 1.0;
            imageScale.ScaleX = 1.0;
            imageScale.ScaleY = 1.0;
            CenterImage();
        }
        private void CenterImage()
        {
            if (contentImage.Source is BitmapSource bitmap)
            {
                double imageWidth = bitmap.PixelWidth * _currentScale;
                double imageHeight = bitmap.PixelHeight * _currentScale;

                imageTranslate.X = Math.Max(0, (zoomCanvas.ActualWidth - imageWidth) / 2);
                imageTranslate.Y = Math.Max(0, (zoomCanvas.ActualHeight - imageHeight) / 2);
            }
        }
        private bool IsImageLargerThanWindow()
        {
            if (!(contentImage.Source is BitmapSource bitmap))
                return false;

            double imageWidth = bitmap.PixelWidth;
            double imageHeight = bitmap.PixelHeight;
            double canvasWidth = zoomCanvas.ActualWidth;
            double canvasHeight = zoomCanvas.ActualHeight;

            return imageWidth > canvasWidth || imageHeight > canvasHeight;
        }
        private void CenterImageAfterLoad()
        {
            // 确保在UI线程执行
            Dispatcher.Invoke(() =>
            {
                if (contentImage.Source is BitmapSource bitmap)
                {
                    // 检查图像是否有效
                    if (bitmap.PixelWidth > 0 && bitmap.PixelHeight > 0)
                    {
                        //_autoFitToWindow = true;

                        if (IsImageLargerThanWindow())
                        {
                            //FitToWindow(true);
                            FitToWindow(_autoFitToWindow);
                        }
                        else
                        {
                            ResetToCenter();
                        }

                        Console.WriteLine($"图像加载完成: {bitmap.PixelWidth}x{bitmap.PixelHeight}, 缩放: {_currentScale}");
                    }
                }
            });
        }

        #endregion
        private void content_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string comosDataString = e.Data.GetData("Text").ToString();
                ComosUIDToDiagModGet?.Invoke(this, comosDataString);
            }
            catch { }
        }
    }
}