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

namespace ZoomAndPanExample
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
        public partial class MainWindow : Window
        {
            private Point? _lastDragPoint;
            private bool _isDragging = false;
            private double _currentScale = 1.0;

            public MainWindow()
            {
                InitializeComponent();
            }

            #region 鼠标事件处理

            private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
            {
                if (contentImage.Source == null) return;

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
                // 以Canvas中心点进行缩放
                ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), 1.2);
            }

            private void ZoomOut_Click(object sender, RoutedEventArgs e)
            {
                // 以Canvas中心点进行缩放
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
                _currentScale = 1.0;
                imageTranslate.X = 0;
                imageTranslate.Y = 0;
                imageScale.ScaleX = 1.0;
                imageScale.ScaleY = 1.0;
                UpdateZoomUI();
            }

            private void FitToWindow_Click(object sender, RoutedEventArgs e)
            {
                if (contentImage.Source is BitmapSource bitmap)
                {
                    double scaleX = zoomCanvas.ActualWidth / bitmap.PixelWidth;
                    double scaleY = zoomCanvas.ActualHeight / bitmap.PixelHeight;
                    _currentScale = Math.Min(scaleX, scaleY);

                    imageScale.ScaleX = _currentScale;
                    imageScale.ScaleY = _currentScale;

                    // 居中显示
                    imageTranslate.X = (zoomCanvas.ActualWidth - bitmap.PixelWidth * _currentScale) / 2;
                    imageTranslate.Y = (zoomCanvas.ActualHeight - bitmap.PixelHeight * _currentScale) / 2;

                    UpdateZoomUI();
                }
            }

            private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                if (IsLoaded && contentImage.Source != null)
                {
                    double newScale = ZoomSlider.Value / 100.0;

                    // 以Canvas中心点进行缩放
                    ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), newScale / _currentScale);
                }
            }

            #endregion

            #region 文件操作

            private void OpenImage_Click(object sender, RoutedEventArgs e)
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.tiff|所有文件|*.*",
                    Title = "选择要查看的图像"
                };

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(dialog.FileName);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        contentImage.Source = bitmap;
                        Reset_Click(null, null);

                        Title = $"图像查看器 - {System.IO.Path.GetFileName(dialog.FileName)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"无法加载图像: {ex.Message}", "错误",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            #endregion
        }
    }