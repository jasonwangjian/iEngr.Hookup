using iEngr.Hookup.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using netDxf;
using netDxf.Entities;
using System.Linq;
using netDxf.Tables;
using SkiaSharp;
using Point = System.Windows.Point;
using System.Windows.Threading;

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
        private ZoomManager _zoomManager;

        public UcDxfViewer()
        {
            InitializeComponent();
            DataContext = this;
            SetImageSource("pack://application:,,,/iEngr.Hookup;component/Resources/A4LEmpty.Png");
            _renderer = new DxfRenderer(zoomCanvas);
            _zoomManager = new ZoomManager(zoomCanvas, frameImage);

            // 监听图像大小变化
            zoomCanvas.SizeChanged += ContentImage_SizeChanged;
         }

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
                FileStatus = _renderer.RenderDxf(filePath);
            }
            else
            {
                FileStatus = FileStatus.NotDxfExtension; ;
            }
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
        private System.Windows.Point? _lastDragPoint;
        private bool _isDragging = false;
        private double _scale = 1.0;
        private System.Windows.Point _panOffset = new System.Windows.Point(0, 0);


        //#region 图像查看功能（鼠标动作）
        //private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    //System.Windows.Point mouseCanvasPos = e.GetPosition(zoomCanvas);

        //    //double zoomFactor = e.Delta > 0 ? 1.2 : 0.8;
        //    //double newScale = Math.Max(0.1, Math.Min(20.0, _currentScale * zoomFactor));

        //    //double scaleChange = newScale / _currentScale;

        //    //double oldTranslateX = canvasTranslate.X;
        //    //double oldTranslateY = canvasTranslate.Y;

        //    //_currentScale = newScale;
        //    //canvasScale.ScaleX = _currentScale;
        //    //canvasScale.ScaleY = _currentScale;

        //    //canvasTranslate.X = oldTranslateX + (1 - scaleChange) * (mouseCanvasPos.X - oldTranslateX);
        //    //canvasTranslate.Y = oldTranslateY + (1 - scaleChange) * (mouseCanvasPos.Y - oldTranslateY);
        //    double zoomFactor = e.Delta > 0 ? 1.2 : 1 / 1.2;
        //    Zoom(zoomFactor, e.GetPosition(zoomCanvas));
        //    e.Handled = true;
        //}
        //public void Zoom(double factor, System.Windows.Point zoomCenter)
        //{
        //    double oldScale = _scale;
        //    _scale *= factor;

        //    // 限制缩放范围
        //    _scale = Math.Max(0.6, Math.Min(20.0, _scale));

        //    // 基于缩放中心调整平移
        //    if (oldScale != 0)
        //    {
        //        double scaleRatio = _scale / oldScale;
        //        _panOffset.X = zoomCenter.X / _scale - (zoomCenter.X / oldScale - _panOffset.X) * scaleRatio;
        //        _panOffset.Y = zoomCenter.Y / _scale - (zoomCenter.Y / oldScale - _panOffset.Y) * scaleRatio;
        //    }

        //    ApplyTransform();
        //}
        //private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        _lastDragPoint = e.GetPosition(zoomCanvas);
        //        _isDragging = true;
        //        //zoomCanvas.CaptureMouse();
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
        //        System.Windows.Point currentPos = e.GetPosition(zoomCanvas);
        //        Vector delta = currentPos - _lastDragPoint.Value;

        //        canvasTranslate.X += delta.X;
        //        canvasTranslate.Y += delta.Y;

        //        _lastDragPoint = currentPos;
        //        e.Handled = true;
        //    }
        //}
        //#endregion


        private void Reset_Click(object sender, RoutedEventArgs e)
        {

                FitToWindow(true);
        }

        private void FitToWindow_Click(object sender, RoutedEventArgs e)
        {
            //FitToView(_renderer.EntityObjects);
            _zoomManager.FitToView(_renderer.EntityObjects);
        }
        public void ResetView()
        {
            _scale = 1.0;
            _panOffset = new System.Windows.Point(0, 0);
            ApplyTransform();
        }
        public void FitToView(IEnumerable<EntityObject> entities)
        {
            if (entities == null || !entities.Any())
            {
                ResetView();
                return;
            }
            // 计算所有实体的边界
            Rect bounds = CalculateTotalBounds(entities);

            if (bounds.IsEmpty) return;

            // 计算适合视图的缩放比例
            double scaleX = zoomCanvas.ActualWidth / bounds.Width;
            double scaleY = zoomCanvas.ActualHeight / bounds.Height;
            _scale = Math.Min(scaleX, scaleY) * 0.9; // 留一些边距
            canvasScale.ScaleX = scaleX;
            canvasScale.ScaleY = scaleY;
            // 居中显示
            _panOffset.X = -bounds.Left + (zoomCanvas.ActualWidth / _scale - bounds.Width) / 2;
            _panOffset.Y = -bounds.Top + (zoomCanvas.ActualHeight / _scale - bounds.Height) / 2;
            ApplyTransform();
        }

        private void ApplyTransform()
        {
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_scale, _scale));
            transformGroup.Children.Add(new TranslateTransform(_panOffset.X, _panOffset.Y));

            zoomCanvas.RenderTransform = transformGroup;
        }
        private Rect CalculateTotalBounds(IEnumerable<EntityObject> entities)
        {
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            foreach (var entity in entities)
            {
                var bounds = GetEntityBounds(entity);
                if (bounds.HasValue)
                {
                    minX = Math.Min(minX, bounds.Value.Left);
                    minY = Math.Min(minY, bounds.Value.Top);
                    maxX = Math.Max(maxX, bounds.Value.Right);
                    maxY = Math.Max(maxY, bounds.Value.Bottom);
                }
            }

            if (minX == double.MaxValue) return Rect.Empty;

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        private Rect? GetEntityBounds(EntityObject entity)
        {
            try
            {
                switch (entity)
                {
                    case Line line:
                        return new Rect(
                            System.Math.Min(line.StartPoint.X, line.EndPoint.X),
                            System.Math.Min(line.StartPoint.Y, line.EndPoint.Y),
                            System.Math.Abs(line.EndPoint.X - line.StartPoint.X),
                            System.Math.Abs(line.EndPoint.Y - line.StartPoint.Y));

                    case Circle circle:
                        return new Rect(
                            circle.Center.X - circle.Radius,
                            circle.Center.Y - circle.Radius,
                            circle.Radius * 2,
                            circle.Radius * 2);

                    case Arc arc:
                        // 简化计算圆弧边界
                        return new Rect(
                            arc.Center.X - arc.Radius,
                            arc.Center.Y - arc.Radius,
                            arc.Radius * 2,
                            arc.Radius * 2);

                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }
        private void FitToWindow(bool isAutoFit = false)
        {
                //double scaleX = zoomCanvas.ActualWidth / bitmap.PixelWidth;
                //double scaleY = zoomCanvas.ActualHeight / bitmap.PixelHeight;
                //_currentScale = Math.Min(scaleX, scaleY);

                //imageScale.ScaleX = _currentScale;
                //imageScale.ScaleY = _currentScale;

                //CenterImage();

                //if (!isAutoFit)
                //{
                //    _autoFitToWindow = false;
                //}
        }

        private void ResetToCenter()
        {
            //_currentScale = 1.0;
            //imageScale.ScaleX = 1.0;
            //imageScale.ScaleY = 1.0;
            //CenterImage();
        }

        private void CenterImage()
        {
            //if (contentImage.Source is BitmapSource bitmap)
            //{
            //    double imageWidth = bitmap.PixelWidth * _currentScale;
            //    double imageHeight = bitmap.PixelHeight * _currentScale;

            //    imageTranslate.X = Math.Max(0, (zoomCanvas.ActualWidth - imageWidth) / 2);
            //    imageTranslate.Y = Math.Max(0, (zoomCanvas.ActualHeight - imageHeight) / 2);
            //}
        }



        private void CenterImageAfterLoad()
        {
            // 确保在UI线程执行
            Dispatcher.Invoke(() =>
            {
                 FitToView(_renderer.EntityObjects);
            });
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
    }
    public class ZoomManager
    {
        private Canvas _canvas;
        private System.Windows.Controls.Image _frameImage;
        private double _scale = 1.0;
        private bool _isPanning = false;
        private Point _panOffset = new Point(0, 0);
        private Point _lastMousePosition;

        private int _framPixelWidth = 3508;
        private int _framPixelHeight = 2480;
        public double Scale => _scale;
        public Point PanOffset => _panOffset;

        public ZoomManager(Canvas canvas, System.Windows.Controls.Image frameImage)
        {
            _canvas = canvas;
            _frameImage = frameImage;
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // 鼠标滚轮缩放
            _canvas.MouseWheel += (s, e) =>
            {
                double zoomFactor = e.Delta > 0 ? 1.2 : 1 / 1.2;
                Zoom(zoomFactor, e.GetPosition(_canvas));
                e.Handled = true;
            };

            // 鼠标拖动平移 - 使用Preview事件确保能捕获到
            _canvas.PreviewMouseDown += (s, e) =>
            {
                if (e.MiddleButton == MouseButtonState.Pressed ||
                    (e.LeftButton == MouseButtonState.Pressed && Keyboard.Modifiers == ModifierKeys.Shift))
                {
                    _isPanning = true;
                    _lastMousePosition = e.GetPosition(_canvas);
                    _canvas.Cursor = Cursors.Hand;
                    _canvas.CaptureMouse(); // 重要：捕获鼠标
                    e.Handled = true;
                }
            };

            _canvas.PreviewMouseMove += (s, e) =>
            {
                if (_isPanning && _canvas.IsMouseCaptured)
                {
                    Point currentPosition = e.GetPosition(_canvas);
                    Vector delta = currentPosition - _lastMousePosition;

                    // 应用平移（考虑缩放比例）
                    double newPanX = _panOffset.X + delta.X / _scale;
                    double newPanY = _panOffset.Y + delta.Y / _scale;

                    _panOffset.X = newPanX;
                    _panOffset.Y = newPanY;

                    _lastMousePosition = currentPosition;
                    CanvasApplyTransform();
                }
            };

            _canvas.PreviewMouseUp += (s, e) =>
            {
                if (_isPanning)
                {
                    _isPanning = false;
                    _canvas.Cursor = Cursors.Arrow;
                    _canvas.ReleaseMouseCapture(); // 释放鼠标捕获
                    e.Handled = true;
                }
            };

            // 确保即使鼠标在Canvas外也能释放捕获
            _canvas.LostMouseCapture += (s, e) =>
            {
                if (_isPanning)
                {
                    _isPanning = false;
                    _canvas.Cursor = Cursors.Arrow;
                }
            };
        }

        public void Zoom(double factor, Point zoomCenter)
        {
            double oldScale = _scale;
            _scale *= factor;

            // 限制缩放范围
            _scale = Math.Max(0.1, Math.Min(20.0, _scale));

            // 基于缩放中心调整平移
            if (oldScale != 0)
            {
                double scaleRatio = _scale / oldScale;
                _panOffset.X = zoomCenter.X / _scale - (zoomCenter.X / oldScale - _panOffset.X) * scaleRatio;
                _panOffset.Y = zoomCenter.Y / _scale - (zoomCenter.Y / oldScale - _panOffset.Y) * scaleRatio;
            }

            CanvasApplyTransform();
        }

        public void FitToView()
        {
            if (entities == null || !entities.Any())
            {
                ResetView();
                return;
            }

            // 计算所有实体的边界
            Rect bounds = CalculateTotalBounds(entities);

            if (bounds.IsEmpty) return;

            SetContentBounds(bounds);

            // 计算适合视图的缩放比例
            double scaleX = _canvas.ActualWidth / bounds.Width;
            double scaleY = _canvas.ActualHeight / bounds.Height;
            _scale = Math.Min(scaleX, scaleY) * 0.9; // 留一些边距

            // 居中显示
            _panOffset.X = -bounds.Left + (_canvas.ActualWidth / _scale - bounds.Width) / 2;
            _panOffset.Y = -bounds.Top + (_canvas.ActualHeight / _scale - bounds.Height) / 2;

            // 应用约束
            ApplyPanConstraints();

            CanvasApplyTransform();
        }

        public void ResetView()
        {
            _scale = 1.0;
            _panOffset = new Point(0, 0);
            CanvasApplyTransform();
        }

        public void PanTo(double x, double y)
        {
            _panOffset.X = x;
            _panOffset.Y = y;
            ApplyPanConstraints();
            CanvasApplyTransform();
        }

        public void CenterOn(double x, double y)
        {
            _panOffset.X = -x + _canvas.ActualWidth / (2 * _scale);
            _panOffset.Y = -y + _canvas.ActualHeight / (2 * _scale);
            ApplyPanConstraints();
            CanvasApplyTransform();
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
        private void FitToWindow()
        {
                double scaleX = _canvas.ActualWidth / _framPixelWidth;
                double scaleY = _canvas.ActualHeight / _framPixelHeight;
                _scale = Math.Min(scaleX, scaleY);

                imageScale.ScaleX = _currentScale;
                imageScale.ScaleY = _currentScale;

                CenterImage();
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
            double imageWidth = _framPixelWidth * _scale;
            double imageHeight = _framPixelHeight * _scale;

            Point imageTranslate = new Point
                (
                Math.Max(0, (_canvas.ActualWidth - imageWidth) / 2),
                Math.Max(0, (_canvas.ActualHeight - imageHeight) / 2)
                );
            ImageApplyTransform(_scale, imageTranslate);
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


        private void CanvasApplyTransform()
        {
            if (_canvas == null) return;

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_scale, _scale));
            transformGroup.Children.Add(new TranslateTransform(_panOffset.X, _panOffset.Y));

            _canvas.RenderTransform = transformGroup;
        }
        private void ImageApplyTransform(double _scale, Point _panOffset)
        {
            if (_frameImage == null) return;
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_scale, _scale));
            transformGroup.Children.Add(new TranslateTransform(_panOffset.X, _panOffset.Y));
            _frameImage.RenderTransform = transformGroup;
        }
    }
    public class DxfRenderer
    {
        private Canvas _canvas;
        private double _scale = 1.0;
        private double _offsetX = 0;
        private double _offsetY = 0;
        private Dictionary<string, Brush> _layerBrushes;
        public IEnumerable<EntityObject> EntityObjects { get; private set; }

        public DxfRenderer(Canvas canvas)
        {
            _canvas = canvas;
            InitializeLayerBrushes();
        }

        private void DebugEntityProperties(DxfDocument dxf)
        {
            var properties = dxf.Entities.GetType().GetProperties();
            foreach (var prop in properties)
            {
                Console.WriteLine($"Property: {prop.Name}, Type: {prop.PropertyType}");
            }
        }
        private void InitializeLayerBrushes()
        {
            _layerBrushes = new Dictionary<string, Brush>
        {
            { "0", Brushes.Black },
            { "Defpoints", Brushes.Red },
            { "Dimensions", Brushes.Green },
            { "Text", Brushes.Blue }
        };
        }

        public FileStatus RenderDxf(string filePath, double scale = 1.0)
        {
            _scale = scale;
            _canvas.Children.Clear();

            try
            {
                DxfDocument dxf = DxfDocument.Load(filePath);
                DebugEntityProperties(dxf);
                CalculateViewport(dxf);
                RenderAllEntities(dxf);
                return FileStatus.ValidedDxf;
            }
            catch (System.Exception ex)
            {
                return FileStatus.InValidedDxf;
            }
        }

        private void CalculateViewport(DxfDocument dxf)
        {
            // 修正：使用正确的实体访问方式
            var entities = GetEntitiesList(dxf);
            if (entities.Count > 0)
            {
                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;

                foreach (var entity in entities)
                {
                    var bounds = GetEntityBounds(entity);
                    if (bounds.HasValue)
                    {
                        minX = System.Math.Min(minX, bounds.Value.Left);
                        minY = System.Math.Min(minY, bounds.Value.Top);
                        maxX = System.Math.Max(maxX, bounds.Value.Right);
                        maxY = System.Math.Max(maxY, bounds.Value.Bottom);
                    }
                }

                _offsetX = -minX;
                _offsetY = -minY; // 注意Y轴方向
            }
        }

        // 修正：获取实体列表的正确方法
        private List<EntityObject> GetEntitiesList(DxfDocument dxf)
        {
            var entities = new List<EntityObject>();

            // 使用索引器或特定方法访问实体
            entities.AddRange(dxf.Entities.Lines);
            entities.AddRange(dxf.Entities.Circles);
            entities.AddRange(dxf.Entities.Arcs);
            entities.AddRange(dxf.Entities.Polylines2D);
            entities.AddRange(dxf.Entities.Texts);
            entities.AddRange(dxf.Entities.MTexts);
            entities.AddRange(dxf.Entities.Inserts);
            entities.AddRange(dxf.Entities.Ellipses);
            entities.AddRange(dxf.Entities.Splines);
            entities.AddRange(dxf.Entities.Solids);
            EntityObjects = entities.ToArray();
            return entities;
        }

        private void RenderAllEntities(DxfDocument dxf)
        {
            var entities = GetEntitiesList(dxf);
            foreach (var entity in entities)
            {
                RenderEntity(entity);
            }
        }
        private Rect? GetEntityBounds(EntityObject entity)
        {
            try
            {
                switch (entity)
                {
                    case Line line:
                        return new Rect(
                            System.Math.Min(line.StartPoint.X, line.EndPoint.X),
                            System.Math.Min(line.StartPoint.Y, line.EndPoint.Y),
                            System.Math.Abs(line.EndPoint.X - line.StartPoint.X),
                            System.Math.Abs(line.EndPoint.Y - line.StartPoint.Y));

                    case Circle circle:
                        return new Rect(
                            circle.Center.X - circle.Radius,
                            circle.Center.Y - circle.Radius,
                            circle.Radius * 2,
                            circle.Radius * 2);

                    case Arc arc:
                        // 简化计算圆弧边界
                        return new Rect(
                            arc.Center.X - arc.Radius,
                            arc.Center.Y - arc.Radius,
                            arc.Radius * 2,
                            arc.Radius * 2);

                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }
        private void RenderEntity(EntityObject entity)
        {
            switch (entity)
            {
                case Line line:
                    RenderLine(line);
                    break;
                //case Circle circle:
                //    RenderCircle(circle);
                //    break;
                //case Arc arc:
                //    RenderArc(arc);
                //    break;
                case Polyline2D polyline:
                    RenderPolyline(polyline);
                    break;
                    //case LwPolyline lwPolyline:
                    //    RenderLwPolyline(lwPolyline);
                    //    break;
                    //case Text text:
                    //    RenderText(text);
                    //    break;
                    //case MText mtext:
                    //    RenderMText(mtext);
                    //    break;
                    //case Insert insert:
                    //    RenderInsert(insert);
                    //    break;
                    //case Ellipse ellipse:
                    //    RenderEllipse(ellipse);
                    //    break;
                    //case Spline spline:
                    //    RenderSpline(spline);
                    //    break;
                    // 可以根据需要添加更多实体类型
            }
        }
        private void RenderLine(Line line)
        {
            var brush = GetLayerBrush(line.Layer.Name);

            System.Windows.Shapes.Line wpfLine = new System.Windows.Shapes.Line
            {
                X1 = (line.StartPoint.X + _offsetX) * _scale,
                Y1 = (_canvas.ActualHeight - (line.StartPoint.Y + _offsetY)) * _scale,
                X2 = (line.EndPoint.X + _offsetX) * _scale,
                Y2 = (_canvas.ActualHeight - (line.EndPoint.Y + _offsetY)) * _scale,
                Stroke = brush,
                StrokeThickness = GetLineWeight(line.Lineweight)
            };

            SetLineType(wpfLine, line.Linetype);
            _canvas.Children.Add(wpfLine);
        }

        //private void RenderCircle(Circle circle)
        //{
        //    var brush = GetLayerBrush(circle.Layer.Name);

        //    Ellipse ellipse = new Ellipse
        //    {
        //        Width = circle.Radius * 2 * _scale,
        //        Height = circle.Radius * 2 * _scale,
        //        Stroke = brush,
        //        StrokeThickness = GetLineWeight(circle.Lineweight),
        //        Fill = Brushes.Transparent
        //    };

        //    Canvas.SetLeft(ellipse, ((circle.Center.X - circle.Radius) + _offsetX) * _scale);
        //    Canvas.SetTop(ellipse, (_canvas.ActualHeight - (circle.Center.Y + circle.Radius + _offsetY)) * _scale);
        //    _canvas.Children.Add(ellipse);
        //}

        //private void RenderArc(Arc arc)
        //{
        //    var brush = GetLayerBrush(arc.Layer.Name);

        //    PathGeometry pathGeometry = new PathGeometry();
        //    PathFigure pathFigure = new PathFigure
        //    {
        //        StartPoint = new Point(
        //            (arc.StartPoint.X + _offsetX) * _scale,
        //            (_canvas.ActualHeight - (arc.StartPoint.Y + _offsetY)) * _scale),
        //        IsClosed = false
        //    };

        //    ArcSegment arcSegment = new ArcSegment
        //    {
        //        Point = new Point(
        //            (arc.EndPoint.X + _offsetX) * _scale,
        //            (_canvas.ActualHeight - (arc.EndPoint.Y + _offsetY)) * _scale),
        //        Size = new Size(arc.Radius * _scale, arc.Radius * _scale),
        //        SweepDirection = arc.StartAngle < arc.EndAngle ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
        //        IsLargeArc = System.Math.Abs(arc.EndAngle - arc.StartAngle) > 180
        //    };

        //    pathFigure.Segments.Add(arcSegment);
        //    pathGeometry.Figures.Add(pathFigure);

        //    Path path = new Path
        //    {
        //        Data = pathGeometry,
        //        Stroke = brush,
        //        StrokeThickness = GetLineWeight(arc.Lineweight)
        //    };

        //    _canvas.Children.Add(path);
        //}

        private void RenderPolyline(Polyline2D polyline)
        {
            if (polyline.Vertexes.Count < 2) return;

            var brush = GetLayerBrush(polyline.Layer.Name);
            System.Windows.Shapes.Polyline wpfPolyline = new System.Windows.Shapes.Polyline
            {
                Stroke = brush,
                StrokeThickness = GetLineWeight(polyline.Lineweight),
                Fill = polyline.IsClosed ? brush : Brushes.Transparent
            };

            foreach (var vertex in polyline.Vertexes)
            {
                wpfPolyline.Points.Add(new System.Windows.Point(
                    (vertex.Position.X + _offsetX) * _scale,
                    (_canvas.ActualHeight - (vertex.Position.Y + _offsetY)) * _scale));
            }

            _canvas.Children.Add(wpfPolyline);
        }
        private Brush GetLayerBrush(string layerName)
        {
            return _layerBrushes.ContainsKey(layerName) ? _layerBrushes[layerName] : Brushes.Black;
        }

        private double GetLineWeight(Lineweight lineweight)
        {
            //return lineweight.Value > 0 ? lineweight.Value * _scale * 0.1 : 1;
            if (lineweight != Lineweight.ByLayer)
            {

            }
            return 1;
        }

        private void SetLineType(System.Windows.Shapes.Shape shape, Linetype linetype)
        {
            if (linetype.Name.Contains("DASHED"))
            {
                shape.StrokeDashArray = new DoubleCollection(new double[] { 4, 2 });
            }
            else if (linetype.Name.Contains("DOTTED"))
            {
                shape.StrokeDashArray = new DoubleCollection(new double[] { 1, 2 });
            }
        }
    }
}