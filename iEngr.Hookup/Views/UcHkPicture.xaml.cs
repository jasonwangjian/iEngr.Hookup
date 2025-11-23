using iEngr.Hookup.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Point = System.Windows.Point;
using PdfiumViewer;
using Plt;

namespace iEngr.Hookup.Views
{
    /// <summary>
    /// UcHkPicture.xaml 的交互逻辑
    /// </summary>
    public partial class UcHkPicture : UserControl
    {
        public event EventHandler<string> ComosUIDToDiagModGet;

        private HkPictureViewModel _viewModel;
        public UcHkPicture()
        {
            InitializeComponent();
            _viewModel = new HkPictureViewModel();
            DataContext = _viewModel;

            // 监听图像大小变化
            contentImage.SizeChanged += ContentImage_SizeChanged;
         }


        private void ContentImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 图像大小发生变化时居中
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                CenterImageAfterLoad();
            }
        }

        // 原有图像查看字段
        private Point? _lastDragPoint;
        private bool _isDragging = false;
        private double _currentScale = 1.0;
        private bool _autoFitToWindow = true;


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



        private void contentImage_Drop(object sender, DragEventArgs e)
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