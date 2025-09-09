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
using System.Windows.Shapes;

namespace ZoomAndPanExample1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
        //public partial class MainWindow : Window
        //{
        //    private double _currentScale = 1.0;
        //    private Point? _lastDragPoint;
        //    private bool _isDragging = false;

        //    public MainWindow()
        //    {
        //        InitializeComponent();
        //    }

        //    private void Window_Loaded(object sender, RoutedEventArgs e)
        //    {
        //        // 确保可以获得焦点
        //        Focusable = true;
        //        Focus();
        //    }

        //// 使用Preview事件处理
        //private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    // 获取鼠标相对于图像的位置
        //    Point mousePos = e.GetPosition(contentImage);

        //    // 计算缩放因子
        //    double zoomFactor = e.Delta > 0 ? 1.2 : 0.8;
        //    double newScale = _currentScale * zoomFactor;

        //    // 限制缩放范围
        //    newScale = Math.Max(0.1, Math.Min(10.0, newScale));
        //    _currentScale = newScale;

        //    ApplyScale();
        //    e.Handled = true;
        //}

        //private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        _lastDragPoint = e.GetPosition(viewer);
        //        _isDragging = true;
        //        ((Border)sender).CaptureMouse();
        //        ((Border)sender).Cursor = Cursors.Hand;
        //        e.Handled = true;
        //    }
        //}

        //private void Border_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left && _isDragging)
        //    {
        //        _isDragging = false;
        //        _lastDragPoint = null;
        //        ((Border)sender).ReleaseMouseCapture();
        //        ((Border)sender).Cursor = Cursors.Arrow;
        //        e.Handled = true;
        //    }
        //}

        //private void Border_PreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    if (_isDragging && _lastDragPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        Point currentPos = e.GetPosition(viewer);
        //        Vector delta = _lastDragPoint.Value - currentPos;

        //        // 调整滚动位置
        //        viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset + delta.X);
        //        viewer.ScrollToVerticalOffset(viewer.VerticalOffset + delta.Y);

        //        _lastDragPoint = currentPos;
        //        e.Handled = true;
        //    }
        //}
        //#region 缩放控制方法

        //private void ApplyScale()
        //    {
        //        // 使用 ScaleTransform 进行缩放
        //        var scaleTransform = new ScaleTransform(_currentScale, _currentScale);
        //        contentImage.LayoutTransform = scaleTransform;

        //        // 更新UI
        //        ZoomSlider.Value = _currentScale * 100;
        //        ZoomText.Text = $"{_currentScale * 100:F0}%";
        //    }

        //    private void ZoomIn_Click(object sender, RoutedEventArgs e)
        //    {
        //        _currentScale = Math.Min(10.0, _currentScale * 1.2);
        //        ApplyScale();
        //    }

        //    private void ZoomOut_Click(object sender, RoutedEventArgs e)
        //    {
        //        _currentScale = Math.Max(0.1, _currentScale / 1.2);
        //        ApplyScale();
        //    }

        //    private void Reset_Click(object sender, RoutedEventArgs e)
        //    {
        //        _currentScale = 1.0;
        //        ApplyScale();
        //        viewer.ScrollToHorizontalOffset(0);
        //        viewer.ScrollToVerticalOffset(0);
        //    }

        //    private void FitToWindow_Click(object sender, RoutedEventArgs e)
        //    {
        //        if (contentImage.Source is BitmapSource bitmap && bitmap.PixelWidth > 0 && bitmap.PixelHeight > 0)
        //        {
        //            double scaleX = viewer.ViewportWidth / bitmap.PixelWidth;
        //            double scaleY = viewer.ViewportHeight / bitmap.PixelHeight;
        //            _currentScale = Math.Min(scaleX, scaleY);
        //            ApplyScale();

        //            // 居中显示
        //            viewer.ScrollToHorizontalOffset((bitmap.PixelWidth * _currentScale - viewer.ViewportWidth) / 2);
        //            viewer.ScrollToVerticalOffset((bitmap.PixelHeight * _currentScale - viewer.ViewportHeight) / 2);
        //        }
        //    }

        //    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //    {
        //        if (IsLoaded) // 避免初始化时触发
        //        {
        //            _currentScale = ZoomSlider.Value / 100.0;
        //            ApplyScale();
        //        }
        //    }

        //    #endregion

        //    #region 文件操作

        //    private void OpenImage_Click(object sender, RoutedEventArgs e)
        //    {
        //        var dialog = new OpenFileDialog
        //        {
        //            Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.tiff|所有文件|*.*",
        //            Title = "选择要查看的图像"
        //        };

        //        if (dialog.ShowDialog() == true)
        //        {
        //            try
        //            {
        //                // 加载图像
        //                var bitmap = new BitmapImage();
        //                bitmap.BeginInit();
        //                bitmap.UriSource = new Uri(dialog.FileName);
        //                bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //                bitmap.EndInit();

        //                contentImage.Source = bitmap;

        //                // 重置视图
        //                Reset_Click(null, null);

        //                Title = $"图像查看器 - {System.IO.Path.GetFileName(dialog.FileName)}";
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"无法加载图像: {ex.Message}", "错误",
        //                    MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //    }

        //    #endregion
        //}
    }