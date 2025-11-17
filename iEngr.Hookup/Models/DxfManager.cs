using iEngr.Hookup.Views;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Windows.Point;
using Path = System.Windows.Shapes.Path;

namespace iEngr.Hookup.Models
{
    public class PolylineSegment
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public double StartWidth { get; set; }
        public double EndWidth { get; set; }
        public double Bulge { get; set; }
        public bool IsArc => Math.Abs(Bulge) > 0.001;

        public PolylineSegment(Point start, Point end, double startWidth, double endWidth, double bulge)
        {
            StartPoint = start;
            EndPoint = end;
            StartWidth = Math.Max(0.1, startWidth);
            EndWidth = Math.Max(0.1, endWidth);
            Bulge = bulge;
        }
    }
    public class DxfLayerReader
    {
        private DxfDocument _dxfDocument;
        private Dictionary<string, Layer> _layers;

        public DxfLayerReader(string filePath)
        {
            _dxfDocument = DxfDocument.Load(filePath);
            _layers = new Dictionary<string, Layer>();
            LoadLayers();
        }

        private void LoadLayers()
        {
            // 读取所有图层信息
            foreach (Layer layer in _dxfDocument.Layers)
            {
                _layers[layer.Name] = layer;
                Console.WriteLine($"图层: {layer.Name}, 颜色: {layer.Color}, 线宽: {layer.Lineweight}");
            }
        }

        // 获取特定图层的颜色（ByLayer）
        public AciColor GetLayerColor(string layerName)
        {
            if (_layers.TryGetValue(layerName, out Layer layer))
            {
                return layer.Color;
            }
            return AciColor.ByLayer; // 默认返回 ByLayer
        }

        // 获取特定图层的线宽（ByLayer）
        public Lineweight GetLayerLineweight(string layerName)
        {
            if (_layers.TryGetValue(layerName, out Layer layer))
            {
                return layer.Lineweight;
            }
            return Lineweight.ByLayer; // 默认返回 ByLayer
        }

        // 获取所有图层信息
        public Dictionary<string, Layer> GetAllLayers()
        {
            return new Dictionary<string, Layer>(_layers);
        }

        // 检查图层是否存在
        public bool LayerExists(string layerName)
        {
            return _layers.ContainsKey(layerName);
        }
    }
    public class DxfStyleManager
    {
        private double defaultLineWeight = 0.1;
        private DxfDocument _dxfDocument;
        public Dictionary<string, Layer> _layers;

        public DxfStyleManager(DxfDocument dxfDocument)
        {
            _dxfDocument = dxfDocument;
            _layers = new Dictionary<string, Layer>();
            InitializeLayers();
        }

        private void InitializeLayers()
        {
            foreach (Layer layer in _dxfDocument.Layers)
            {
                _layers[layer.Name] = layer;
            }
        }

        // 解析实体的最终颜色（处理 ByLayer 和 ByBlock）
        public AciColor ResolveEntityColor(EntityObject entity)
        {
            if (entity.Color.IsByLayer)
            {
                // 如果是 ByLayer，返回图层颜色
                return GetLayerColor(entity.Layer.Name);
            }
            else if (entity.Color.IsByBlock)
            {
                // 如果是 ByBlock，可能需要特殊处理
                return AciColor.Default;
            }
            else
            {
                // 返回实体自身的颜色
                return entity.Color;
            }
        }

        // 解析实体的最终线宽（处理 ByLayer 和 ByBlock）
        public Lineweight ResolveEntityLineweight(EntityObject entity)
        {
            if (entity.Lineweight == Lineweight.ByLayer)
            {
                // 如果是 ByLayer，返回图层线宽
                return GetLayerLineweight(entity.Layer.Name);
            }
            else if (entity.Lineweight == Lineweight.ByBlock)
            {
                // 如果是 ByBlock，返回默认线宽
                return Lineweight.Default;
            }
            else
            {
                // 返回实体自身的线宽
                return entity.Lineweight;
            }
        }

        // 将 AciColor 转换为 WPF 的 Color
        public Color ConvertToWpfColor(AciColor dxfColor)
        {
            if (dxfColor == null) return Colors.Black;

            // AciColor 的 R, G, B 值范围是 0-255
            return Color.FromRgb(dxfColor.R, dxfColor.G, dxfColor.B);
        }

        // 将 Lineweight 转换为 WPF 的线宽（像素）
        public double ConvertToWpfLinewidth(Lineweight lineweight)
        {
            if (lineweight == Lineweight.ByLayer || lineweight == Lineweight.ByBlock || double.TryParse(lineweight.ToString().Substring(1), out double value) &&  value <= 0)
            {
                return defaultLineWeight; // 默认线宽
            }

            // Lineweight 值是以毫米为单位的，转换为像素（假设 96 DPI）
            return value>0? value * 3.78:defaultLineWeight; // 1 mm ≈ 3.78 pixels at 96 DPI
        }

        private AciColor GetLayerColor(string layerName)
        {
            if (_layers.TryGetValue(layerName, out Layer layer))
            {
                return layer.Color;
            }
            return AciColor.Default;
        }

        private Lineweight GetLayerLineweight(string layerName)
        {
            if (_layers.TryGetValue(layerName, out Layer layer))
            {
                return layer.Lineweight;
            }
            return Lineweight.Default;
        }
    }
    public class DxfRenderer
    {
        private Canvas _canvas;
        private DxfLayerReader _layerReader;
        private DxfStyleManager _styleManager;
        private double _margin = 0.05;
        private double _dxfWidth;
        private double _dxfHeight;
        private double _zoomFacter;
        private double _panX = 0;
        private double _panY = 0;
        private double _scale = 1.0;
        private double _offsetX = 0;
        private double _offsetY = 0;
        private Dictionary<string, Brush> _layerBrushes;
        public IEnumerable<EntityObject> EntityObjects { get; private set; }

        public DxfRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public FileStatus RenderDxf(string filePath, double scale = 1.0)
        {
            //_scale = _zoomFacter;
            _canvas.Children.Clear();

            try
            {
                DxfDocument _dxfDocument = DxfDocument.Load(filePath);
                _styleManager = new DxfStyleManager(_dxfDocument);
                DebugEntityProperties(_dxfDocument);
                DisplayLayerInfo();
                CalculateViewport(_dxfDocument);
                RenderAllEntities(_dxfDocument);
                return FileStatus.ValidedDxf;
            }
            catch (System.Exception ex)
            {
                return FileStatus.InValidedDxf;
            }
        }
        private void DebugEntityProperties(DxfDocument dxf)
        {
            var properties = dxf.Entities.GetType().GetProperties();
            foreach (var prop in properties)
            {
                Console.WriteLine($"Property: {prop.Name}, Type: {prop.PropertyType}");
            }
        }
        private void DisplayLayerInfo()
        {
            Console.WriteLine("=== DXF 图层信息 ===");
            foreach (var layer in _styleManager._layers)
            {
                var color = _styleManager.ConvertToWpfColor(layer.Value.Color);
                var lineweight = _styleManager.ConvertToWpfLinewidth(layer.Value.Lineweight);
                Console.WriteLine($"图层: {layer.Key}, 颜色: {color}, 线宽: {lineweight:F2}px");
            }
            Console.WriteLine("====================");
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
                        minX = Math.Min(minX, bounds.Value.Left);
                        minY = Math.Min(minY, bounds.Value.Top);
                        maxX = Math.Max(maxX, bounds.Value.Right);
                        maxY = Math.Max(maxY, bounds.Value.Bottom);
                    }
                }
                _dxfWidth = maxX - minX;
                _dxfHeight = maxY - minY;
                double marginX = _dxfWidth * _margin;
                double marginY = _dxfHeight * _margin;
                minX = minX - marginX;
                minY = minY - marginY;
                maxX = maxX + marginX;
                maxY = maxY + marginY;
                _dxfWidth = maxX - minX;
                _dxfHeight = maxY - minY;
                _zoomFacter = Math.Min(_canvas.ActualWidth / _dxfWidth, _canvas.ActualHeight / _dxfHeight);
                _panX = (_canvas.ActualWidth - _dxfWidth * _zoomFacter) / 2;
                _panY = (_canvas.ActualHeight - _dxfHeight * _zoomFacter) / 2;
                _scale = _zoomFacter;
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
                            Math.Min(line.StartPoint.X, line.EndPoint.X),
                            Math.Min(line.StartPoint.Y, line.EndPoint.Y),
                            Math.Abs(line.EndPoint.X - line.StartPoint.X),
                            Math.Abs(line.EndPoint.Y - line.StartPoint.Y));

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
                    //RenderLwPolyline(polyline);
                    break;
                    //case LwPolyline lwPolyline:
                    //    RenderLwPolyline(lwPolyline);
                    //    break;
                    case Text text:
                        RenderText(text);
                        break;
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
            // 解析实体的最终样式
            var finalColor = _styleManager.ResolveEntityColor(line);
            var finalLineweight = _styleManager.ResolveEntityLineweight(line);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            System.Windows.Shapes.Line wpfLine = new System.Windows.Shapes.Line
            {
                //X1 = (line.StartPoint.X + _offsetX) * _scale,
                //Y1 = (_canvas.ActualHeight - (line.StartPoint.Y + _offsetY) - _canvas.ActualHeight + _dxfHeight) * _scale + _panY,
                //X2 = (line.EndPoint.X + _offsetX) * _scale,
                //Y2 = (_canvas.ActualHeight - (line.EndPoint.Y + _offsetY) - _canvas.ActualHeight + _dxfHeight) * _scale + _panY,
                X1 = (line.StartPoint.X + _offsetX) * _scale + _panX,
                Y1 = (_dxfHeight - (line.StartPoint.Y + _offsetY)) * _scale + _panY,
                X2 = (line.EndPoint.X + _offsetX) * _scale + _panX,
                Y2 = (_dxfHeight - (line.EndPoint.Y + _offsetY)) * _scale + _panY,
                Stroke = new SolidColorBrush(wpfColor),
                StrokeThickness = wpfLinewidth * _scale
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
        private void RenderText(Text text)
        {
            // 解析实体的最终样式
            var finalColor = _styleManager.ResolveEntityColor(text);
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);

            TextBlock textBlock = new TextBlock
            {
                Text = text.Value,
                FontFamily = new FontFamily(text.Style.FontFamilyName),
                FontSize = text.Height * _scale * 0.8, // 调整字体大小比例
                Foreground = new SolidColorBrush(wpfColor),
                RenderTransform = new RotateTransform(-text.Rotation) // DXF旋转角度与WPF相反
            };

            Canvas.SetLeft(textBlock, ConvertToCanvasPointX(text.Position.X));
            Canvas.SetTop(textBlock, ConvertToCanvasPointY(text.Position.Y));

            _canvas.Children.Add(textBlock);
        }

        #region Polyline2D
        private void RenderPolyline(Polyline2D lwPolyline)
        {
            try
            {
                var finalColor = _styleManager.ResolveEntityColor(lwPolyline);
                var wpfColor = _styleManager.ConvertToWpfColor(finalColor);

                // 检查特性
                //bool hasBulge = lwPolyline.Vertexes.Any(v => Math.Abs(v.Bulge) > 0.001);
                bool hasBulge = lwPolyline.Vertexes.Any(v => v.Bulge != 0);
                bool hasVariableWidth = lwPolyline.Vertexes.Any(v =>
                    (Math.Abs(v.StartWidth) > 0.001 || Math.Abs(v.EndWidth) > 0.001) &&
                    v.StartWidth != v.EndWidth);

                if (hasVariableWidth && hasBulge)
                {
                    // 最复杂的情况：可变宽度 + 圆弧
                    RenderLwPolylineWithWidthAndBulge(lwPolyline, wpfColor);
                }
                else if (hasVariableWidth)
                {
                    // 只有可变宽度
                    RenderVariableWidthPolyline(lwPolyline, wpfColor);
                }
                else if (hasBulge)
                {
                    // 只有圆弧
                    RenderLwPolylineWithBulge(lwPolyline, wpfColor);
                }
                else
                {
                    // 简单直线段
                    RenderSimpleLwPolyline(lwPolyline, wpfColor);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"渲染 LwPolyline 时出错: {ex.Message}");
                // 降级到简单渲染
                var finalColor = _styleManager.ResolveEntityColor(lwPolyline);
                var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
                RenderSimpleLwPolyline(lwPolyline, wpfColor);
            }
        }
        private void RenderSimpleLwPolyline(Polyline2D lwPolyline, Color color)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            var brush = new SolidColorBrush(color);
            var finalLineweight = _styleManager.ResolveEntityLineweight(lwPolyline);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            var points = new PointCollection();
            foreach (var vertex in lwPolyline.Vertexes)
            {
                points.Add(ConvertToCanvasPoint(vertex.Position));
            }

            if (lwPolyline.IsClosed && lwPolyline.Vertexes.Count > 2)
            {
                points.Add(ConvertToCanvasPoint(lwPolyline.Vertexes[0].Position));
            }

            System.Windows.Shapes.Polyline wpfPolyline = new System.Windows.Shapes.Polyline
            {
                Points = points,
                Stroke = brush,
                StrokeThickness = wpfLinewidth * _scale,
                Fill = lwPolyline.IsClosed ?
                    new SolidColorBrush(Color.FromArgb(50, color.R, color.G, color.B)) :
                    Brushes.Transparent,
                StrokeLineJoin = PenLineJoin.Round
            };

            _canvas.Children.Add(wpfPolyline);
        }
        private void RenderLwPolylineWithBulge(Polyline2D lwPolyline, Color color)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            var brush = new SolidColorBrush(color);
            var finalLineweight = _styleManager.ResolveEntityLineweight(lwPolyline);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            var points = new PointCollection();

            // 遍历所有顶点，处理 bulge
            for (int i = 0; i < lwPolyline.Vertexes.Count; i++)
            {
                var currentVertex = lwPolyline.Vertexes[i];
                var nextVertex = lwPolyline.Vertexes[(i + 1) % lwPolyline.Vertexes.Count];

                // 添加当前点
                points.Add(ConvertToCanvasPoint(currentVertex.Position));

                // 处理 bulge（圆弧段）
                if (currentVertex.Bulge != 0 && i < lwPolyline.Vertexes.Count - 1)
                {
                    var bulgePoints = ConvertBulgeToPoints(currentVertex, nextVertex, currentVertex.Bulge);
                    // 修正：使用循环添加点
                    foreach (var point in bulgePoints)
                    {
                        points.Add(point);
                    }
                }
            }

            // 如果是闭合的，添加第一个点
            if (lwPolyline.IsClosed)
            {
                points.Add(ConvertToCanvasPoint(lwPolyline.Vertexes[0].Position));
            }

            System.Windows.Shapes.Polyline wpfPolyline = new System.Windows.Shapes.Polyline
            {
                Points = points,
                Stroke = brush,
                StrokeThickness = wpfLinewidth * _scale,
                Fill = lwPolyline.IsClosed ? CreateFillBrush(new SolidColorBrush(Color.FromArgb(50, color.R, color.G, color.B))) : Brushes.Transparent,
                StrokeLineJoin = PenLineJoin.Round
            };

            SetLineType(wpfPolyline, lwPolyline.Linetype);
            _canvas.Children.Add(wpfPolyline);
        }
        private void RenderVariableWidthPolyline(Polyline2D lwPolyline, Color color)
        {
            var brush = new SolidColorBrush(color);
            var geometryGroup = new GeometryGroup();

            // 遍历每一段，为每段创建独立的几何图形
            for (int i = 0; i < lwPolyline.Vertexes.Count - 1; i++)
            {
                var startVertex = lwPolyline.Vertexes[i];
                var endVertex = lwPolyline.Vertexes[i + 1];

                // 获取段的宽度
                double startWidth = Math.Max(0.1, startVertex.StartWidth); // 使用起始顶点的结束宽度
                double endWidth = Math.Max(0.1, endVertex.EndWidth);   // 使用结束顶点的起始宽度

                if (Math.Abs(startWidth - endWidth) < 0.001)
                {
                    // 宽度相同，使用简单的矩形
                    var segmentGeometry = CreateConstantWidthSegment(startVertex, endVertex, startWidth);
                    geometryGroup.Children.Add(segmentGeometry);
                }
                else
                {
                    // 宽度不同，使用梯形
                    var segmentGeometry = CreateVariableWidthSegment(startVertex, endVertex, startWidth, endWidth);
                    geometryGroup.Children.Add(segmentGeometry);
                }
            }

            // 如果是闭合多段线，添加最后一段到第一段的连接
            if (lwPolyline.IsClosed && lwPolyline.Vertexes.Count > 2)
            {
                var startVertex = lwPolyline.Vertexes[lwPolyline.Vertexes.Count - 1]; // 最后一个顶点
                var endVertex = lwPolyline.Vertexes[0];    // 第一个顶点

                double startWidth = Math.Max(0.1, startVertex.EndWidth);
                double endWidth = Math.Max(0.1, endVertex.StartWidth);

                var segmentGeometry = CreateVariableWidthSegment(startVertex, endVertex, startWidth, endWidth);
                geometryGroup.Children.Add(segmentGeometry);
            }

            Path path = new Path
            {
                Data = geometryGroup,
                Fill = brush,
                Stroke = Brushes.Transparent, // 使用填充而不是描边
                StrokeThickness = 0
            };

            _canvas.Children.Add(path);
        }
        private Geometry CreateConstantWidthSegment(Polyline2DVertex start, Polyline2DVertex end, double width)
        {
            var startPoint = ConvertToCanvasPoint(start.Position);
            var endPoint = ConvertToCanvasPoint(end.Position);

            // 计算垂直方向
            Vector direction = endPoint - startPoint;
            if (direction.Length == 0) return null;

            direction.Normalize();
            Vector perpendicular = new Vector(-direction.Y, direction.X);

            // 计算矩形的四个角点
            Point p1 = startPoint + perpendicular * (width * _scale / 2);
            Point p2 = startPoint - perpendicular * (width * _scale / 2);
            Point p3 = endPoint - perpendicular * (width * _scale / 2);
            Point p4 = endPoint + perpendicular * (width * _scale / 2);

            // 创建路径几何
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = true,
                IsFilled = true
            };

            figure.Segments.Add(new LineSegment(p2, true));
            figure.Segments.Add(new LineSegment(p3, true));
            figure.Segments.Add(new LineSegment(p4, true));

            geometry.Figures.Add(figure);
            return geometry;
        }
        private Geometry CreateVariableWidthSegment(Polyline2DVertex start, Polyline2DVertex end, double startWidth, double endWidth)
        {
            var startPoint = ConvertToCanvasPoint(start.Position);
            var endPoint = ConvertToCanvasPoint(end.Position);

            // 计算垂直方向
            Vector direction = endPoint - startPoint;
            if (direction.Length == 0) return null;

            direction.Normalize();
            Vector perpendicular = new Vector(-direction.Y, direction.X);

            // 计算梯形的四个角点
            Point p1 = startPoint + perpendicular * (startWidth * _scale / 2);  // 起始点左侧
            Point p2 = startPoint - perpendicular * (startWidth * _scale / 2);  // 起始点右侧
            Point p3 = endPoint - perpendicular * (endWidth * _scale / 2);      // 结束点右侧
            Point p4 = endPoint + perpendicular * (endWidth * _scale / 2);      // 结束点左侧

            // 创建路径几何
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = true,
                IsFilled = true
            };

            figure.Segments.Add(new LineSegment(p2, true));
            figure.Segments.Add(new LineSegment(p3, true));
            figure.Segments.Add(new LineSegment(p4, true));

            geometry.Figures.Add(figure);
            return geometry;
        }
        private void RenderLwPolylineWithWidthAndBulge(Polyline2D lwPolyline, Color color)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            var brush = new SolidColorBrush(color);
            var geometryGroup = new GeometryGroup();

            // 获取所有线段
            var segments = GetPolylineSegments(lwPolyline);

            foreach (var segment in segments)
            {
                Geometry segmentGeometry;

                if (segment.IsArc)
                {
                    // 处理带宽度和圆弧的段
                    segmentGeometry = CreateVariableWidthArcSegment(segment);
                }
                else
                {
                    // 处理带宽度和直线的段
                    segmentGeometry = CreateVariableWidthLineSegment(segment);
                }

                if (segmentGeometry != null)
                {
                    geometryGroup.Children.Add(segmentGeometry);
                }
            }

            Path path = new Path
            {
                Data = geometryGroup,
                Fill = brush,
                Stroke = Brushes.Transparent,
                StrokeThickness = 0
            };

            _canvas.Children.Add(path);
        }
        private List<PolylineSegment> GetPolylineSegments(Polyline2D lwPolyline)
        {
            var segments = new List<PolylineSegment>();

            for (int i = 0; i < lwPolyline.Vertexes.Count - 1; i++)
            {
                var startVertex = lwPolyline.Vertexes[i];
                var endVertex = lwPolyline.Vertexes[i + 1];

                var segment = new PolylineSegment(
                    ConvertToCanvasPoint(startVertex.Position),
                    ConvertToCanvasPoint(endVertex.Position),
                    startVertex.EndWidth,  // 使用起始顶点的结束宽度
                    endVertex.StartWidth,  // 使用结束顶点的起始宽度
                    startVertex.Bulge
                );

                segments.Add(segment);
            }

            // 处理闭合多段线
            if (lwPolyline.IsClosed && lwPolyline.Vertexes.Count > 2)
            {
                var startVertex = lwPolyline.Vertexes[lwPolyline.Vertexes.Count - 1];
                var endVertex = lwPolyline.Vertexes[0];

                var segment = new PolylineSegment(
                    ConvertToCanvasPoint(startVertex.Position),
                    ConvertToCanvasPoint(endVertex.Position),
                    startVertex.EndWidth,
                    endVertex.StartWidth,
                    startVertex.Bulge
                );

                segments.Add(segment);
            }

            return segments;
        }
        private Geometry CreateVariableWidthLineSegment(PolylineSegment segment)
        {
            Vector direction = segment.EndPoint - segment.StartPoint;
            if (direction.Length == 0) return null;

            direction.Normalize();
            Vector perpendicular = new Vector(-direction.Y, direction.X);

            // 计算梯形的四个角点
            double startHalfWidth = segment.StartWidth * _scale / 2;
            double endHalfWidth = segment.EndWidth * _scale / 2;

            Point p1 = segment.StartPoint + perpendicular * startHalfWidth;  // 起始左侧
            Point p2 = segment.StartPoint - perpendicular * startHalfWidth;  // 起始右侧
            Point p3 = segment.EndPoint - perpendicular * endHalfWidth;      // 结束右侧
            Point p4 = segment.EndPoint + perpendicular * endHalfWidth;      // 结束左侧

            return CreateQuadGeometry(p1, p2, p3, p4);
        }
        private Geometry CreateVariableWidthArcSegment(PolylineSegment segment)
        {
            // 计算圆弧参数
            var arcParams = CalculateArcParameters(segment.StartPoint, segment.EndPoint, segment.Bulge);
            if (arcParams == null) return null;

            var (center, radius, startAngle, endAngle, isCounterClockwise) = arcParams.Value;

            // 将圆弧分割为多个小段来近似
            int segments = CalculateArcSegments(radius, Math.Abs(endAngle - startAngle));
            var outerPoints = new List<Point>();
            var innerPoints = new List<Point>();

            for (int i = 0; i <= segments; i++)
            {
                double t = (double)i / segments;
                double angle = startAngle + (endAngle - startAngle) * t;

                // 计算圆弧上的点
                double x = center.X + radius * Math.Cos(angle);
                double y = center.Y + radius * Math.Sin(angle);
                Point arcPoint = new Point(x, y);

                // 计算该点的宽度（线性插值）
                double currentWidth = segment.StartWidth + (segment.EndWidth - segment.StartWidth) * t;
                double halfWidth = currentWidth * _scale / 2;

                // 计算该点的法线方向（指向圆心的方向）
                Vector toCenter = center - arcPoint;
                if (toCenter.Length > 0)
                {
                    toCenter.Normalize();
                }
                else
                {
                    toCenter = new Vector(0, 1); // 特殊情况处理
                }

                // 根据 bulge 方向调整内外侧
                if (isCounterClockwise)
                {
                    outerPoints.Add(arcPoint + toCenter * halfWidth);
                    innerPoints.Add(arcPoint - toCenter * halfWidth);
                }
                else
                {
                    outerPoints.Add(arcPoint - toCenter * halfWidth);
                    innerPoints.Add(arcPoint + toCenter * halfWidth);
                }
            }

            // 创建路径几何
            return CreateVariableWidthArcGeometry(outerPoints, innerPoints, isCounterClockwise);
        }
        private (Point center, double radius, double startAngle, double endAngle, bool isCounterClockwise)?
            CalculateArcParameters(Point start, Point end, double bulge)
        {
            if (Math.Abs(bulge) < 0.001) return null;

            double chordLength = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
            if (chordLength == 0) return null;

            // bulge = tan(θ/4)，其中 θ 是圆弧的包含角
            double theta = 4 * Math.Atan(Math.Abs(bulge));
            double radius = chordLength / (2 * Math.Sin(theta / 2));

            // 计算弦的中点
            double midX = (start.X + end.X) / 2;
            double midY = (start.Y + end.Y) / 2;

            // 计算垂直方向
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double perpX = -dy;
            double perpY = dx;
            double perpLength = Math.Sqrt(perpX * perpX + perpY * perpY);

            if (perpLength == 0) return null;

            perpX /= perpLength;
            perpY /= perpLength;

            // 计算 sagitta（弦高）
            double sagitta = radius * Math.Sqrt(1 - Math.Pow(chordLength / (2 * radius), 2));
            if (double.IsNaN(sagitta)) sagitta = radius;

            // 根据 bulge 符号确定圆心方向
            bool isCounterClockwise = bulge > 0;
            //if (!isCounterClockwise)
            if (!isCounterClockwise)
            {
                perpX = -perpX;
                perpY = -perpY;
            }

            // 计算圆心
            double centerX = midX + perpX * sagitta;
            double centerY = midY + perpY * sagitta;
            Point center = new Point(centerX, centerY);

            // 计算起始角和终止角
            double startAngle = Math.Atan2(start.Y - center.Y, start.X - center.X);
            double endAngle = Math.Atan2(end.Y - center.Y, end.X - center.X);

            // 调整角度方向
            if (isCounterClockwise)
            {
                if (endAngle < startAngle) endAngle += 2 * Math.PI;
            }
            else
            {
                if (endAngle > startAngle) endAngle -= 2 * Math.PI;
            }

            return (center, radius, startAngle, endAngle, isCounterClockwise);
        }
         private int CalculateArcSegments(double radius, double angle)
        {
            // 根据圆弧长度和半径动态计算分段数
            double arcLength = radius * Math.Abs(angle);
            int segments = Math.Max(8, (int)(arcLength / 10)); // 每10像素一个分段
            return Math.Min(segments, 50); // 最多50段
        }
        private Geometry CreateVariableWidthArcGeometry(List<Point> outerPoints, List<Point> innerPoints, bool isCounterClockwise)
        {
            if (outerPoints.Count < 2 || innerPoints.Count < 2) return null;

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                IsClosed = true,
                IsFilled = true
            };

            // 根据方向确定绘制顺序
            if (isCounterClockwise)
            {
                figure.StartPoint = outerPoints[0];

                // 添加外侧边（使用多段线近似）
                for (int i = 1; i < outerPoints.Count; i++)
                {
                    figure.Segments.Add(new LineSegment(outerPoints[i], true));
                }

                // 添加内侧边（反向）
                for (int i = innerPoints.Count - 1; i >= 0; i--)
                {
                    figure.Segments.Add(new LineSegment(innerPoints[i], true));
                }
            }
            else
            {
                figure.StartPoint = innerPoints[0];

                // 添加内侧边
                for (int i = 1; i < innerPoints.Count; i++)
                {
                    figure.Segments.Add(new LineSegment(innerPoints[i], true));
                }

                // 添加外侧边（反向）
                for (int i = outerPoints.Count - 1; i >= 0; i--)
                {
                    figure.Segments.Add(new LineSegment(outerPoints[i], true));
                }
            }

            geometry.Figures.Add(figure);
            return geometry;
        }
        #endregion

        #region 等宽曲线
        private void RenderLwPolyline(Polyline2D lwPolyline)
        {
            try
            {
                // 检查是否有 bulge
                bool hasBulge = lwPolyline.Vertexes.Any(v => v.Bulge != 0);

                if (hasBulge)
                {
                    // 使用支持 bulge 的渲染方法
                    RenderLwPolylineWithBulge(lwPolyline);
                }
                else
                {
                    // 使用简单的直线段渲染
                    RenderSimpleLwPolyline(lwPolyline);
                }
            }
            catch (Exception ex)
            {
                // 降级到简单渲染
                Console.WriteLine($"渲染 LwPolyline 时出错: {ex.Message}");
                RenderSimpleLwPolyline(lwPolyline);
            }
        }
        private void RenderLwPolylineWithBulge(Polyline2D lwPolyline)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            var finalColor = _styleManager.ResolveEntityColor(lwPolyline);
            var finalLineweight = _styleManager.ResolveEntityLineweight(lwPolyline);

            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            var points = new PointCollection();

            // 遍历所有顶点，处理 bulge
            for (int i = 0; i < lwPolyline.Vertexes.Count; i++)
            {
                var currentVertex = lwPolyline.Vertexes[i];
                var nextVertex = lwPolyline.Vertexes[(i + 1) % lwPolyline.Vertexes.Count];

                // 添加当前点
                points.Add(ConvertToCanvasPoint(currentVertex.Position));

                // 处理 bulge（圆弧段）
                if (currentVertex.Bulge != 0 && i < lwPolyline.Vertexes.Count - 1)
                {
                    var bulgePoints = ConvertBulgeToPoints(currentVertex, nextVertex, currentVertex.Bulge);
                    // 修正：使用循环添加点
                    foreach (var point in bulgePoints)
                    {
                        points.Add(point);
                    }
                }
            }

            // 如果是闭合的，添加第一个点
            if (lwPolyline.IsClosed)
            {
                points.Add(ConvertToCanvasPoint(lwPolyline.Vertexes[0].Position));
            }

            System.Windows.Shapes.Polyline wpfPolyline = new System.Windows.Shapes.Polyline
            {
                Points = points,
                Stroke = new SolidColorBrush(wpfColor),
                StrokeThickness = wpfLinewidth * _scale,
                Fill = lwPolyline.IsClosed ? CreateFillBrush(new SolidColorBrush(Color.FromArgb(50, wpfColor.R, wpfColor.G, wpfColor.B))) : Brushes.Transparent,
                StrokeLineJoin = PenLineJoin.Round
            };

            SetLineType(wpfPolyline, lwPolyline.Linetype);
            _canvas.Children.Add(wpfPolyline);
        }
        //private List<Point> ConvertBulgeToPoints(Polyline2DVertex start, Polyline2DVertex end, double bulge)
        //{
        //    var points = new List<Point>();

        //    // bulge = tan(θ/4)，其中 θ 是圆弧的包含角
        //    double theta = 4 * Math.Atan(Math.Abs(bulge));
        //    double radius = CalculateArcRadius(start.Position, end.Position, theta);

        //    // 计算圆心
        //    var center = CalculateArcCenter(start.Position, end.Position, radius, bulge > 0);

        //    // 计算起始角和终止角
        //    double startAngle = Math.Atan2(start.Position.Y - center.Y, start.Position.X - center.X);
        //    double endAngle = Math.Atan2(end.Position.Y - center.Y, end.Position.X - center.X);

        //    // 调整角度方向
        //    if (bulge < 0)
        //    {
        //        if (endAngle > startAngle) endAngle -= 2 * Math.PI;
        //    }
        //    else
        //    {
        //        if (endAngle < startAngle) endAngle += 2 * Math.PI;
        //    }

        //    // 将圆弧分割为多个线段来近似
        //    int segments = Math.Max(8, (int)(Math.Abs(endAngle - startAngle) * 16 / Math.PI));
        //    for (int i = 1; i < segments; i++)
        //    {
        //        double angle = startAngle + (endAngle - startAngle) * i / segments;
        //        double x = center.X + radius * Math.Cos(angle);
        //        double y = center.Y + radius * Math.Sin(angle);
        //        points.Add(ConvertToCanvasPoint(new Vector2((float)x, (float)y)));
        //    }

        //    return points;
        //}
        private List<Point> ConvertBulgeToPoints(Polyline2DVertex start, Polyline2DVertex end, double bulge)
        {
            var points = new List<Point>();

            if (Math.Abs(bulge) < 0.001)
            {
                // 没有 bulge，直接返回直线
                return points;
            }

            // 计算圆弧参数
            var arcParams = CalculateArcParameters(
                new Point(start.Position.X, start.Position.Y),
                new Point(end.Position.X, end.Position.Y),
                bulge);

            if (arcParams == null) return points;

            var (center, radius, startAngle, endAngle, isCounterClockwise) = arcParams.Value;

            // 确定正确的圆弧方向
            int segments = Math.Max(8, (int)(Math.Abs(endAngle - startAngle) * 16 / Math.PI));

            for (int i = 1; i < segments; i++)
            {
                double t = (double)i / segments;
                double angle = startAngle + (endAngle - startAngle) * t;

                double x = center.X + radius * Math.Cos(angle);
                double y = center.Y + radius * Math.Sin(angle);
                points.Add(ConvertToCanvasPoint(new Vector2((float)x, (float)y)));
            }

            return points;
        }
        private double CalculateArcRadius(Vector2 start, Vector2 end, double theta)
        {
            double chordLength = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
            return chordLength / (2 * Math.Sin(theta / 2));
        }
        private Vector2 CalculateArcCenter(Vector2 start, Vector2 end, double radius, bool isCounterClockwise)
        {
            double chordLength = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
            double sagitta = radius * Math.Sqrt(1 - Math.Pow(chordLength / (2 * radius), 2));

            // 计算弦的中点
            double midX = (start.X + end.X) / 2;
            double midY = (start.Y + end.Y) / 2;

            // 计算垂直方向
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;

            // 垂直向量（旋转90度）
            double perpX = -dy;
            double perpY = dx;
            double length = Math.Sqrt(perpX * perpX + perpY * perpY);

            // 单位化
            perpX /= length;
            perpY /= length;

            // 根据 bulge 符号确定方向
            if (!isCounterClockwise)
            {
                perpX = -perpX;
                perpY = -perpY;
            }

            // 计算圆心
            double centerX = midX + perpX * sagitta;
            double centerY = midY + perpY * sagitta;

            return new Vector2((float)centerX, (float)centerY);
        }
        private void RenderSimpleLwPolyline(Polyline2D lwPolyline)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            var finalColor = _styleManager.ResolveEntityColor(lwPolyline);
            var finalLineweight = _styleManager.ResolveEntityLineweight(lwPolyline);

            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            var points = new PointCollection();


            foreach (var vertex in lwPolyline.Vertexes)
            {
                points.Add(ConvertToCanvasPoint(vertex.Position));
            }

            if (lwPolyline.IsClosed && lwPolyline.Vertexes.Count > 0)
            {
                points.Add(ConvertToCanvasPoint(lwPolyline.Vertexes[0].Position));
            }

            CreatePolylineFromPoints(points, lwPolyline, new SolidColorBrush(wpfColor));
        }
        private void CreatePolylineFromPoints(PointCollection points, Polyline2D LwPolyline, Brush brush)
        {
            System.Windows.Shapes.Polyline wpfPolyline = new System.Windows.Shapes.Polyline
            {
                Points = points,
                Stroke = brush,
                StrokeThickness = GetLineWeight(LwPolyline.Lineweight),//wpfLinewidth * _scale,
                Fill = LwPolyline.IsClosed ? CreateFillBrush(brush) : Brushes.Transparent,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round
            };

            SetLineType(wpfPolyline, LwPolyline.Linetype);
            _canvas.Children.Add(wpfPolyline);
        }
        #endregion
        private Brush CreateFillBrush(Brush strokeBrush)
        {
            // 创建填充画刷（使用半透明颜色）
            if (strokeBrush is SolidColorBrush solidBrush)
            {
                Color fillColor = solidBrush.Color;
                fillColor.A = 50; // 半透明
                return new SolidColorBrush(fillColor);
            }

            return new SolidColorBrush(Color.FromArgb(50, 200, 200, 200));
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
        private Geometry CreateQuadGeometry(Point p1, Point p2, Point p3, Point p4)
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = true,
                IsFilled = true
            };

            figure.Segments.Add(new LineSegment(p2, true));
            figure.Segments.Add(new LineSegment(p3, true));
            figure.Segments.Add(new LineSegment(p4, true));

            geometry.Figures.Add(figure);
            return geometry;
        }
        private Point ConvertToCanvasPoint(Vector2 point)
        {
            double x = (point.X + _offsetX) * _scale + _panX;
            double y = (_dxfHeight - (point.Y + _offsetY)) * _scale + _panY;
            return new Point(x, y);
        }
        private double ConvertToCanvasPointX(double positionX)
        {
            return (positionX + _offsetX) * _scale + _panX;
        }
        private double ConvertToCanvasPointY(double positionY)
        {
            return (_dxfHeight - (positionY + _offsetY)) * _scale + _panY;
        }

    }
}
