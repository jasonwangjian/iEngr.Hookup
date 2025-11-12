using netDxf.Entities;
using netDxf;
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
using static netDxf.Entities.HatchBoundaryPath;
using Line = netDxf.Entities.Line;
using Arc = netDxf.Entities.Arc;
using netDxf.Tables;
using Point = System.Windows.Point;

namespace DxfViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DxfRenderer _renderer;
        private ZoomManager _zoomManager;
        private List<EntityObject> _currentEntities;

        public MainWindow()
        {
            InitializeComponent();
            _renderer = new DxfRenderer(mainCanvas);
            _zoomManager = new ZoomManager(mainCanvas);

            _zoomManager.OnViewChanged += (s, e) => UpdateStatus();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "DXF文件 (*.dxf)|*.dxf|所有文件 (*.*)|*.*",
                Title = "打开DXF文件"
            };

            if (dialog.ShowDialog() == true)
            {
                _renderer.RenderDxf(dialog.FileName);
                //_currentEntities = _renderer.GetEntities(); // 假设有这个方法获取实体

                //_zoomManager.FitToView(_renderer.EntityObjects);
                statusText.Text = $"已加载: {System.IO.Path.GetFileName(dialog.FileName)}";
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _zoomManager.ZoomIn();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _zoomManager.ZoomOut();
        }

        private void FitToView_Click(object sender, RoutedEventArgs e)
        {
            _zoomManager.FitToView(_renderer.EntityObjects);
        }

        private void ResetView_Click(object sender, RoutedEventArgs e)
        {
            _zoomManager.ResetView();
        }

        private void UpdateStatus()
        {
            statusText.Text = $"缩放: {_zoomManager.Scale:P0} | 平移: ({_zoomManager.PanOffset.X:F1}, {_zoomManager.PanOffset.Y:F1})";
        }

        // 支持数据类
        private class LineData
        {
            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }
            public double StrokeThickness { get; set; }
        }

        private class EllipseData
        {
            public double Width { get; set; }
            public double Height { get; set; }
            public double Left { get; set; }
            public double Top { get; set; }
            public double StrokeThickness { get; set; }
        }

        private class TextData
        {
            public double FontSize { get; set; }
            public double Left { get; set; }
            public double Top { get; set; }
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

        public void RenderDxf(string filePath, double scale = 1.0)
        {
            _scale = scale;
            _canvas.Children.Clear();

            try
            {
                DxfDocument dxf = DxfDocument.Load(filePath);
                DebugEntityProperties(dxf);
                CalculateViewport(dxf);
                RenderAllEntities(dxf);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"加载DXF文件失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                _offsetY = -maxY; // 注意Y轴方向
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
    public class ZoomManager
    {
        private Canvas _canvas;
        private double _scale = 1.0;
        private Point _panOffset = new Point(0, 0);
        private Point _lastMousePosition;
        private bool _isPanning = false;

        public double Scale => _scale;
        public Point PanOffset => _panOffset;

        public ZoomManager(Canvas canvas)
        {
            _canvas = canvas;
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

            // 鼠标拖动平移
            _canvas.MouseDown += (s, e) =>
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    _isPanning = true;
                    _lastMousePosition = e.GetPosition(_canvas);
                    _canvas.Cursor = Cursors.Hand;
                }
            };

            _canvas.MouseMove += (s, e) =>
            {
                if (_isPanning)
                {
                    Point currentPosition = e.GetPosition(_canvas);
                    Vector delta = currentPosition - _lastMousePosition;

                    _panOffset.X += delta.X / _scale;
                    _panOffset.Y += delta.Y / _scale;

                    _lastMousePosition = currentPosition;
                    ApplyTransform();
                }
            };

            _canvas.MouseUp += (s, e) =>
            {
                _isPanning = false;
                _canvas.Cursor = Cursors.Arrow;
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

            ApplyTransform();
        }

        public void ZoomIn()
        {
            Zoom(1.2, new Point(_canvas.ActualWidth / 2, _canvas.ActualHeight / 2));
        }

        public void ZoomOut()
        {
            Zoom(1 / 1.2, new Point(_canvas.ActualWidth / 2, _canvas.ActualHeight / 2));
        }

        public void FitToView(IEnumerable<EntityObject> entities)
        {
            if (entities == null || !entities.Any()) return;

            // 计算所有实体的边界
            Rect bounds = CalculateTotalBounds(entities);

            if (bounds.IsEmpty) return;

            // 计算适合视图的缩放比例
            double scaleX = _canvas.ActualWidth / bounds.Width;
            double scaleY = _canvas.ActualHeight / bounds.Height;
            _scale = Math.Min(scaleX, scaleY) * 0.9; // 留一些边距

            // 居中显示
            _panOffset.X = -bounds.Left + (_canvas.ActualWidth / _scale - bounds.Width) / 2;
            _panOffset.Y = -bounds.Top + (_canvas.ActualHeight / _scale - bounds.Height) / 2;

            ApplyTransform();
        }

        public void ResetView()
        {
            _scale = 1.0;
            _panOffset = new Point(0, 0);
            ApplyTransform();
        }

        private void ApplyTransform()
        {
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(_scale, _scale));
            transformGroup.Children.Add(new TranslateTransform(_panOffset.X, _panOffset.Y));

            _canvas.RenderTransform = transformGroup;

            OnViewChanged?.Invoke(this, EventArgs.Empty);
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
        public event EventHandler OnViewChanged;
    }
}
