using iEngr.Hookup.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UcHkPicture.xaml 的交互逻辑
    /// </summary>
    public partial class UcHkPicture : UserControl
    {
        private HkPictureViewModel _viewModel;
        public UcHkPicture()
        {
            InitializeComponent();
            _viewModel = new HkPictureViewModel();
            DataContext = _viewModel;
        }
        private Point? _lastDragPoint;
        private bool _isDragging = false;
        private double _currentScale = 1.0;
        private bool _autoFitToWindow = true; // 新增：自动适应窗口标志


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            zoomCanvas.Focusable = true;
            zoomCanvas.Focus();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (contentImage.Source != null && _autoFitToWindow)
            {
                // 窗口大小改变时，如果启用自动适应，则重新适应窗口
                FitToWindow(true);
            }
            else if (contentImage.Source != null)
            {
                // 否则只重新居中
                CenterImage();
            }
        }
        #region 自动适应窗口功能

        // 检查图片是否超过窗口大小
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

        // 适应窗口功能
        private void FitToWindow(bool isAutoFit = false)
        {
            if (contentImage.Source is BitmapSource bitmap)
            {
                double scaleX = zoomCanvas.ActualWidth / bitmap.PixelWidth;
                double scaleY = zoomCanvas.ActualHeight / bitmap.PixelHeight;

                // 选择较小的缩放比例，确保图片完全可见
                _currentScale = Math.Min(scaleX, scaleY);

                imageScale.ScaleX = _currentScale;
                imageScale.ScaleY = _currentScale;

                // 居中显示
                CenterImage();

                UpdateZoomUI();

                if (!isAutoFit)
                {
                    // 如果是手动点击"适应窗口"按钮，则暂时禁用自动适应
                    _autoFitToWindow = false;
                }
            }
        }

        // 居中显示图片
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

        #endregion

        #region 图片打开和初始化

        private void OpenImageAndCenter()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif|所有文件|*.*",
                Title = "选择要查看的图像"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // 加载图像
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(dialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    contentImage.Source = bitmap;

                    // 重置状态
                    _autoFitToWindow = true;

                    // 检查图片是否超过窗口大小
                    if (IsImageLargerThanWindow())
                    {
                        // 如果图片超过窗口，自动适应窗口
                        FitToWindow(true);
                        Console.WriteLine("图片超过窗口，自动适应窗口大小");
                    }
                    else
                    {
                        // 否则以原始大小居中显示
                        _currentScale = 1.0;
                        imageScale.ScaleX = 1.0;
                        imageScale.ScaleY = 1.0;
                        CenterImage();
                        UpdateZoomUI();
                        Console.WriteLine("图片较小，以原始大小居中显示");
                    }


                    Console.WriteLine($"图片尺寸: {bitmap.PixelWidth}x{bitmap.PixelHeight}");
                    Console.WriteLine($"窗口尺寸: {zoomCanvas.ActualWidth}x{zoomCanvas.ActualHeight}");
                    Console.WriteLine($"当前缩放: {_currentScale:F2}");

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法加载图像: {ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region 鼠标事件处理

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

            // 智能缩放：以鼠标位置为中心
            imageTranslate.X = oldTranslateX + (1 - scaleChange) * (mouseCanvasPos.X - oldTranslateX);
            imageTranslate.Y = oldTranslateY + (1 - scaleChange) * (mouseCanvasPos.Y - oldTranslateY);

            UpdateZoomUI();
            e.Handled = true;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // 拖拽时禁用自动适应
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
            if (_isDragging && _lastDragPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
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

        #region 缩放控制方法

        private void UpdateZoomUI()
        {
            ZoomSlider.Value = _currentScale * 100;
            ZoomText.Text = $"{_currentScale * 100:F0}%";
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (contentImage.Source == null) return;

            // 缩放时禁用自动适应
            _autoFitToWindow = false;

            ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), 1.2);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (contentImage.Source == null) return;

            // 缩放时禁用自动适应
            _autoFitToWindow = false;

            ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), 0.8);
        }

        private void ZoomAroundPoint(Point zoomCenter, double zoomFactor)
        {
            if (contentImage.Source == null) return;

            double newScale = Math.Max(0.1, Math.Min(20.0, _currentScale * zoomFactor));
            double scaleChange = newScale / _currentScale;

            double oldTranslateX = imageTranslate.X;
            double oldTranslateY = imageTranslate.Y;

            _currentScale = newScale;
            imageScale.ScaleX = _currentScale;
            imageScale.ScaleY = _currentScale;

            imageTranslate.X = oldTranslateX + (1 - scaleChange) * (zoomCenter.X - oldTranslateX);
            imageTranslate.Y = oldTranslateY + (1 - scaleChange) * (zoomCenter.Y - oldTranslateY);

            UpdateZoomUI();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (contentImage.Source == null) return;

            // 重置时重新启用自动适应
            _autoFitToWindow = true;

            if (IsImageLargerThanWindow())
            {
                // 如果图片仍然超过窗口，适应窗口
                FitToWindow(true);
            }
            else
            {
                // 否则恢复原始大小
                _currentScale = 1.0;
                imageScale.ScaleX = 1.0;
                imageScale.ScaleY = 1.0;
                CenterImage();
                UpdateZoomUI();
            }
        }

        private void FitToWindow_Click(object sender, RoutedEventArgs e)
        {
            // 手动适应窗口，暂时禁用自动适应
            _autoFitToWindow = false;
            FitToWindow();
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && contentImage.Source != null)
            {
                // 滑块调整时禁用自动适应
                _autoFitToWindow = false;

                double newScale = ZoomSlider.Value / 100.0;
                ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), newScale / _currentScale);
            }
        }

        #endregion

        #region 文件操作

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenImageAndCenter();
        }

        #endregion
        //#region 鼠标事件处理

        //private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    if (contentImage.Source == null) return;

        //    Point mouseCanvasPos = e.GetPosition(zoomCanvas);

        //    double zoomFactor = e.Delta > 0 ? 1.2 : 0.8;
        //    double newScale = Math.Max(0.1, Math.Min(20.0, _currentScale * zoomFactor));

        //    double scaleChange = newScale / _currentScale;

        //    double oldTranslateX = imageTranslate.X;
        //    double oldTranslateY = imageTranslate.Y;

        //    _currentScale = newScale;
        //    imageScale.ScaleX = _currentScale;
        //    imageScale.ScaleY = _currentScale;

        //    // 智能缩放：以鼠标位置为中心
        //    imageTranslate.X = oldTranslateX + (1 - scaleChange) * (mouseCanvasPos.X - oldTranslateX);
        //    imageTranslate.Y = oldTranslateY + (1 - scaleChange) * (mouseCanvasPos.Y - oldTranslateY);

        //    UpdateZoomUI();
        //    e.Handled = true;
        //}

        //private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        _lastDragPoint = e.GetPosition(zoomCanvas);
        //        _isDragging = true;
        //        zoomCanvas.CaptureMouse();
        //        zoomCanvas.Cursor = Cursors.Hand;
        //        e.Handled = true;
        //    }
        //}

        //private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left && _isDragging)
        //    {
        //        _isDragging = false;
        //        _lastDragPoint = null;
        //        zoomCanvas.ReleaseMouseCapture();
        //        zoomCanvas.Cursor = Cursors.Arrow;
        //        e.Handled = true;
        //    }
        //}

        //private void Canvas_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (_isDragging && _lastDragPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        Point currentPos = e.GetPosition(zoomCanvas);
        //        Vector delta = currentPos - _lastDragPoint.Value;

        //        imageTranslate.X += delta.X;
        //        imageTranslate.Y += delta.Y;

        //        _lastDragPoint = currentPos;
        //        e.Handled = true;
        //    }
        //}

        //#endregion

        //#region 缩放控制方法

        //private void UpdateZoomUI()
        //{
        //    ZoomSlider.Value = _currentScale * 100;
        //    ZoomText.Text = $"{_currentScale * 100:F0}%";
        //}

        //private void ZoomIn_Click(object sender, RoutedEventArgs e)
        //{
        //    // 以Canvas中心点进行缩放
        //    ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), 1.2);
        //}

        //private void ZoomOut_Click(object sender, RoutedEventArgs e)
        //{
        //    // 以Canvas中心点进行缩放
        //    ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), 0.8);
        //}

        //private void ZoomAroundPoint(Point zoomCenter, double zoomFactor)
        //{
        //    if (contentImage.Source == null) return;

        //    double newScale = Math.Max(0.1, Math.Min(20.0, _currentScale * zoomFactor));
        //    double scaleChange = newScale / _currentScale;

        //    double oldTranslateX = imageTranslate.X;
        //    double oldTranslateY = imageTranslate.Y;

        //    _currentScale = newScale;
        //    imageScale.ScaleX = _currentScale;
        //    imageScale.ScaleY = _currentScale;

        //    imageTranslate.X = oldTranslateX + (1 - scaleChange) * (zoomCenter.X - oldTranslateX);
        //    imageTranslate.Y = oldTranslateY + (1 - scaleChange) * (zoomCenter.Y - oldTranslateY);

        //    UpdateZoomUI();
        //}

        //private void Reset_Click(object sender, RoutedEventArgs e)
        //{
        //    _currentScale = 1.0;
        //    imageTranslate.X = 0;
        //    imageTranslate.Y = 0;
        //    imageScale.ScaleX = 1.0;
        //    imageScale.ScaleY = 1.0;
        //    UpdateZoomUI();
        //}

        //private void FitToWindow_Click(object sender, RoutedEventArgs e)
        //{
        //    if (contentImage.Source is BitmapSource bitmap)
        //    {
        //        double scaleX = zoomCanvas.ActualWidth / bitmap.PixelWidth;
        //        double scaleY = zoomCanvas.ActualHeight / bitmap.PixelHeight;
        //        _currentScale = Math.Min(scaleX, scaleY);

        //        imageScale.ScaleX = _currentScale;
        //        imageScale.ScaleY = _currentScale;

        //        // 居中显示
        //        imageTranslate.X = (zoomCanvas.ActualWidth - bitmap.PixelWidth * _currentScale) / 2;
        //        imageTranslate.Y = (zoomCanvas.ActualHeight - bitmap.PixelHeight * _currentScale) / 2;

        //        UpdateZoomUI();
        //    }
        //}

        //private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (IsLoaded && contentImage.Source != null)
        //    {
        //        double newScale = ZoomSlider.Value / 100.0;

        //        // 以Canvas中心点进行缩放
        //        ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), newScale / _currentScale);
        //    }
        //}

        //#endregion

        //#region 文件操作

        //private void OpenImage_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new OpenFileDialog
        //    {
        //        Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.tiff|所有文件|*.*",
        //        Title = "选择要查看的图像"
        //    };

        //    if (dialog.ShowDialog() == true)
        //    {
        //        try
        //        {
        //            var bitmap = new BitmapImage();
        //            bitmap.BeginInit();
        //            bitmap.UriSource = new Uri(dialog.FileName);
        //            bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //            bitmap.EndInit();

        //            contentImage.Source = bitmap;
        //            Reset_Click(null, null);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"无法加载图像: {ex.Message}", "错误",
        //                MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}

        //#endregion
    }
}

