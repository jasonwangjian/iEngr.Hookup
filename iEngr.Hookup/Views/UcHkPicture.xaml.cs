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

            // PDF相关字段
            private int _currentPdfPage = 0;
            private int _totalPdfPages = 0;
            private bool _isPdfFile = false;
            private string _currentFilePath;


            private void Window_Loaded(object sender, RoutedEventArgs e)
            {
                zoomCanvas.Focusable = true;
                zoomCanvas.Focus();
            }

            #region 文件打开处理

            private void OpenFile_Click(object sender, RoutedEventArgs e)
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "所有支持的文件|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif;*.pdf|图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif|PDF文件|*.pdf|所有文件|*.*",
                    Title = "选择要查看的文件"
                };

                if (dialog.ShowDialog() == true)
                {
                    OpenFile(dialog.FileName);
                }
            }

            private async void OpenFile(string filePath)
            {
                _currentFilePath = filePath;
                string fileExtension = Path.GetExtension(filePath).ToLower();

                if (fileExtension == ".pdf")
                {
                    await OpenPdfFileAsync(filePath);
                }
                else
                {
                    OpenImageFile(filePath);
                }
            }

        #endregion

        #region PDF处理功能

        private async Task OpenPdfFileAsync(string filePath)
        {
            ShowLoading(true);

            try
            {
                // 使用 PDFWrapper 获取 PDF 页数
                _totalPdfPages = PDFWrapper.GetPageCount(filePath);
                _isPdfFile = true;
                _currentPdfPage = 0;

                // 显示PDF导航控件
                prevPageButton.Visibility = Visibility.Visible;
                nextPageButton.Visibility = Visibility.Visible;
                pageInfoText.Visibility = Visibility.Visible;
                zoomSeparator.Visibility = Visibility.Visible;

                // 显示第一页
                await ShowPdfPageAsync(_currentPdfPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法加载PDF文件: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task ShowPdfPageAsync(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= _totalPdfPages)
                return;

            ShowLoading(true);

            try
            {
                BitmapSource bitmapSource = await Task.Run(() =>
                {
                    // 使用 PDFWrapper 渲染指定页面为 System.Drawing.Bitmap
                    var bitmap = PDFWrapper.GetImage(_currentFilePath, pageIndex, 300); // 300 DPI
                    return ConvertBitmapToBitmapSource(bitmap);
                });

                // 在UI线程更新图像
                Dispatcher.Invoke(() =>
                {
                    contentImage.Source = bitmapSource;
                    _currentPdfPage = pageIndex;
                    pageInfoText.Text = $"第 {pageIndex + 1} 页 / 共 {_totalPdfPages} 页";

                    // 自动适应窗口
                    _autoFitToWindow = true;
                    if (IsImageLargerThanWindow())
                    {
                        FitToWindow(true);
                    }
                    else
                    {
                        ResetToCenter();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"渲染PDF页面时出错: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // 转换 System.Drawing.Bitmap 到 BitmapSource
        private BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                // 将 Bitmap 保存到内存流
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 可选，提高性能

                return bitmapImage;
            }
        }


        private async void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPdfPage > 0)
            {
                await ShowPdfPageAsync(_currentPdfPage - 1);
            }
        }

        private async void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPdfPage < _totalPdfPages - 1)
            {
                await ShowPdfPageAsync(_currentPdfPage + 1);
            }
        }

        #endregion

        #region 图像处理功能（原有功能保留）

        private void OpenImageFile(string filePath)
            {
                _isPdfFile = false;

                // 隐藏PDF导航控件
                prevPageButton.Visibility = Visibility.Collapsed;
                nextPageButton.Visibility = Visibility.Collapsed;
                pageInfoText.Visibility = Visibility.Collapsed;
                zoomSeparator.Visibility = Visibility.Collapsed;

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    contentImage.Source = bitmap;
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
                catch (Exception ex)
                {
                    MessageBox.Show($"无法加载图像: {ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            #endregion

            #region 加载状态管理

            private void ShowLoading(bool isLoading)
            {
                loadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

                // 禁用交互
                prevPageButton.IsEnabled = !isLoading;
                nextPageButton.IsEnabled = !isLoading;
                zoomCanvas.IsEnabled = !isLoading;
            }

            #endregion

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

                //UpdateZoomUI();
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

            //private void UpdateZoomUI()
            //{
            //    ZoomSlider.Value = _currentScale * 100;
            //    ZoomText.Text = $"{_currentScale * 100:F0}%";
            //}

            //private void ZoomIn_Click(object sender, RoutedEventArgs e)
            //{
            //    if (contentImage.Source == null) return;
            //    _autoFitToWindow = false;
            //    ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), 1.2);
            //}

            //private void ZoomOut_Click(object sender, RoutedEventArgs e)
            //{
            //    if (contentImage.Source == null) return;
            //    _autoFitToWindow = false;
            //    ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), 0.8);
            //}

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

                //UpdateZoomUI();
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

            //private void FitToWindow_Click(object sender, RoutedEventArgs e)
            //{
            //    _autoFitToWindow = false;
            //    FitToWindow();
            //}

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
                    //UpdateZoomUI();

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
                ///*UpdateZoomUI*/();
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

            //private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            //{
            //    if (IsLoaded && contentImage.Source != null)
            //    {
            //        _autoFitToWindow = false;
            //        double newScale = ZoomSlider.Value / 100.0;
            //        ZoomAroundPoint(new Point(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2), newScale / _currentScale);
            //    }
            //}

        #endregion

        //private void ContentImage_Loaded(object sender, RoutedEventArgs e)
        //{
        //    // 确保图像已经加载完成
        //    if (contentImage.Source is BitmapSource bitmap && bitmap.IsDownloading)
        //    {
        //        // 如果图像还在下载，等待下载完成
        //        bitmap.DownloadCompleted += Bitmap_DownloadCompleted;
        //        bitmap.DownloadFailed += Bitmap_DownloadFailed;
        //    }
        //    else
        //    {
        //        // 图像已经加载完成，直接居中
        //        CenterImageAfterLoad();
        //    }
        //}
        //private void Bitmap_DownloadCompleted(object sender, EventArgs e)
        //{
        //    // 下载完成后居中
        //    Dispatcher.Invoke(() =>
        //    {
        //        CenterImageAfterLoad();
        //    });
        //}
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
                        _autoFitToWindow = true;

                        if (IsImageLargerThanWindow())
                        {
                            FitToWindow(true);
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
        //private void Bitmap_DownloadFailed(object sender, ExceptionEventArgs e)
        //{
        //    // 处理下载失败
        //    Console.WriteLine($"图像加载失败: {e.ErrorException.Message}");
        //}
    }

    public class PDFWrapper
    {
        // 获取 PDF 页数
        public static int GetPageCount(string filePath)
        {
            using (var document = PdfDocument.Load(filePath))
            {
                return document.PageCount;
            }
        }

        // 获取指定页面的图像
        public static Bitmap GetImage(string filePath, int pageIndex, int dpi)
        {
            using (var document = PdfDocument.Load(filePath))
            {
                // Render the PDF page as a System.Drawing.Image
                var image = document.Render(pageIndex, dpi, dpi, true);

                // Convert the System.Drawing.Image to a System.Drawing.Bitmap
                var bitmap = new Bitmap(image);
                return bitmap;
            }
        }

    }
}