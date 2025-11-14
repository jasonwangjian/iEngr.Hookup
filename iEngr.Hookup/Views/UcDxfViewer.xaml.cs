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
            SetImageSource("pack://application:,,,/iEngr.Hookup;component/Resources/A4LEmpty.png");
            frameImage.Source = ImageSource;
            _framPixelWidth = ImageSource.PixelWidth;
            _framPixelHeight = ImageSource.PixelHeight;
            MouseEventIni();
            _renderer = new DxfRenderer(zoomCanvas);
            ApplyTransform();
            Reset();
            // 监听图像大小变化
            zoomCanvas.SizeChanged += zoomCanvas_SizeChanged;
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

                    _panOffset.X += delta.X;
                    _panOffset.Y += delta.Y;

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
        public void Reset()
        {
            _scale = 1.0;
            _panOffset = new Point(0, 0);
            ApplyTransform();
            if (IsImageLargerThanWindow())
            {
                double scaleX = zoomCanvas.ActualWidth / _framPixelWidth;
                double scaleY = zoomCanvas.ActualHeight / _framPixelHeight;
                double scaleImage = Math.Min(scaleX, scaleY) * 0.95;

                double imageWidth = _framPixelWidth * scaleImage;
                double imageHeight = _framPixelHeight * scaleImage;
                Point offsetImage = new Point(Math.Max(0, (zoomCanvas.ActualWidth - imageWidth) / 2),
                                              Math.Max(0, (zoomCanvas.ActualHeight - imageHeight) / 2));
                ApplyTransform(scaleImage, offsetImage);
            }
            else
            {
                ApplyTransform(1.0, new Point(0, 0));
            }
        }
        private bool IsImageLargerThanWindow()
        {
            if (!(frameImage.Source is BitmapSource bitmap))
                return false;

            double imageWidth = bitmap.PixelWidth;
            double imageHeight = bitmap.PixelHeight;
            double canvasWidth = zoomCanvas.ActualWidth;
            double canvasHeight = zoomCanvas.ActualHeight;

            return imageWidth > canvasWidth * 0.95 || imageHeight > canvasHeight * 0.95;
        }
        private void ApplyTransform()
        {
            if (zoomCanvas == null) return;
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_scale, _scale));
            transformGroup.Children.Add(new TranslateTransform(_panOffset.X, _panOffset.Y));
            zoomCanvas.RenderTransform = transformGroup;
        }
        private void ApplyTransform(double scale, Point offset)
        {
            if (frameImage == null) return;
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(scale, scale));
            transformGroup.Children.Add(new TranslateTransform(offset.X, offset.Y));
            frameImage.RenderTransform = transformGroup;
        }
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
                Reset();
            }
        }

        private void FitToWindow_Click(object sender, RoutedEventArgs e)
        {
            //FitToView(_renderer.EntityObjects);
            Reset();
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



        //private void CenterImageAfterLoad()
        //{
        //    // 确保在UI线程执行
        //    Dispatcher.Invoke(() =>
        //    {
        //         _zoomManager.FitToWindow();
        //    });
        //}



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

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point windowPosition = e.GetPosition(this);
            PositionX = windowPosition.X;
            PositionY = windowPosition.Y;
        }
    }
    public class ZoomManager
    {
        private Canvas _canvas;
        private System.Windows.Controls.Image _frameImage;
        private BitmapImage _image;
        private double _scale = 1.0;
        private bool _isDragging = false;
        private Point _panOffset = new Point(0, 0);
        private Point? _lastDragPoint;

        private int _framPixelWidth;
        private int _framPixelHeight;
        public double Scale => _scale;
        public Point PanOffset => _panOffset;

        public ZoomManager(Canvas canvas, System.Windows.Controls.Image frameImage)
        {
            _canvas = canvas;
            _frameImage = frameImage;
            _image = frameImage.Source as BitmapImage;
            _framPixelWidth = _image.PixelWidth;
            _framPixelHeight = _image.PixelHeight;
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            //鼠标滚轮缩放
            //_canvas.MouseWheel += (s, e) =>
            //{

            //    Point mouseCanvasPos = e.GetPosition(_canvas);
            //    double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            //    double newScale = Math.Max(0.1, Math.Min(20.0, _scale * zoomFactor));

            //    double scaleChange = newScale / _scale;

            //    //((_frameImage.RenderTransform as TransformGroup).Children[1] as TranslateTransform).X
            //    double oldTranslateX = _panOffset.X;
            //    double oldTranslateY = _panOffset.Y;

            //    _panOffset.X = mouseCanvasPos.X / newScale - (mouseCanvasPos.X / _scale - oldTranslateX) * scaleChange;
            //    _panOffset.Y = mouseCanvasPos.Y / newScale - (mouseCanvasPos.Y / _scale - oldTranslateY) * scaleChange;
            //    //_panOffset.X = oldTranslateX + (1 - scaleChange) * (mouseCanvasPos.X - oldTranslateX);
            //    //_panOffset.Y = oldTranslateY + (1 - scaleChange) * (mouseCanvasPos.Y - oldTranslateY);

            //    _scale = newScale;

            //    CanvasApplyTransform();
            //    e.Handled = true;
            //};

        }



        //public void FitToView(IEnumerable<EntityObject> entities)
        //{
        //    if (entities == null || !entities.Any())
        //    {
        //        ResetView();
        //        return;
        //    }

        //    // 计算所有实体的边界
        //    Rect bounds = CalculateTotalBounds(entities);

        //    if (bounds.IsEmpty) return;

        //    SetContentBounds(bounds);

        //    // 计算适合视图的缩放比例
        //    double scaleX = _canvas.ActualWidth / bounds.Width;
        //    double scaleY = _canvas.ActualHeight / bounds.Height;
        //    _scale = Math.Min(scaleX, scaleY) * 0.9; // 留一些边距

        //    // 居中显示
        //    _panOffset.X = -bounds.Left + (_canvas.ActualWidth / _scale - bounds.Width) / 2;
        //    _panOffset.Y = -bounds.Top + (_canvas.ActualHeight / _scale - bounds.Height) / 2;

        //    CanvasApplyTransform();
        //}

        public void ResetView()
        {
            _scale = 1.0;
            _panOffset = new Point(0, 0);
            CanvasApplyTransform();
        }








        private void CanvasApplyTransform()
        {
            if (_canvas == null) return;
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_scale, _scale));
            transformGroup.Children.Add(new TranslateTransform(_panOffset.X, _panOffset.Y));
            _canvas.RenderTransform = transformGroup;
        }
        private void ImageApplyTransform()
        {
            if (_frameImage == null) return;
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_scale, _scale));
            transformGroup.Children.Add(new TranslateTransform(_panOffset.X, _panOffset.Y));
            _frameImage.RenderTransform = transformGroup;
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
            //_canvas.Children.Clear();

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