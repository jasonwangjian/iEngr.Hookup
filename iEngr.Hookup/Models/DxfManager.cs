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
using TextAlignment = System.Windows.TextAlignment;
using System.Text.RegularExpressions;
using netDxf.Units;
using ComosQueryInterface;
using System.Windows.Media.Animation;


namespace iEngr.Hookup.Models
{
    public class MTextParser
    {
        public static string GetPlainText(string mtextValue)
        {
            if (string.IsNullOrEmpty(mtextValue))
                return string.Empty;

            //// 首先处理最外层的大括号
            //if (mtextValue.StartsWith("{") && mtextValue.EndsWith("}"))
            //{
            //    mtextValue = mtextValue.Substring(1, mtextValue.Length - 2);
            //}
            // 移除所有大括号
            mtextValue = Regex.Replace(mtextValue, "[{}]", "");

            // 移除格式块
            string result = RemoveFormatBlocks(mtextValue);

            // 处理转义序列
            result = ProcessEscapeSequences(result);

            // 清理多余的空格和换行
            result = CleanText(result);

            return result;
        }

        private static string RemoveFormatBlocks(string text)
        {
            StringBuilder sb = new StringBuilder();
            int braceLevel = 0;

            foreach (char c in text)
            {
                if (c == '{')
                {
                    braceLevel++;
                }
                else if (c == '}')
                {
                    braceLevel--;
                }
                else if (braceLevel == 0)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static string ProcessEscapeSequences(string text)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;

            while (i < text.Length)
            {
                if (text[i] == '\\' && i + 1 < text.Length)
                {
                    switch (text[i + 1])
                    {
                        case 'P':
                            sb.Append("\n");
                            i += 2;
                            break;
                        case '~':
                            sb.Append(" ");
                            i += 2;
                            break;
                        case '\\':
                            sb.Append("\\");
                            i += 2;
                            break;
                        case '{':
                            sb.Append("{");
                            i += 2;
                            break;
                        case '}':
                            sb.Append("}");
                            i += 2;
                            break;
                        default:
                            // 跳过其他转义序列
                            i += 2;
                            // 跳过直到分号
                            while (i < text.Length && text[i] != ';')
                                i++;
                            if (i < text.Length) i++; // 跳过分号
                            break;
                    }
                }
                else
                {
                    sb.Append(text[i]);
                    i++;
                }
            }

            return sb.ToString();
        }

        private static string CleanText(string text)
        {
            // 替换多个连续换行为单个换行
            text = Regex.Replace(text, @"\n+", "\n");
            // 移除行首行尾的空白
            text = Regex.Replace(text, @"^\s+|\s+$", "");
            // 移除每行行首的空白
            text = Regex.Replace(text, @"\n\s+", "\n");

            return text.Trim();
        }
    }
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
        private double defaultMinWeight = 0.02;
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
            //if (entity is Polyline2D polyline2d)
            //{
            //    return polyline2d.Vertexes[0].StartWidth;
            //}
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
        public double ResolveEntityLineWidth(Polyline2D entity)
        {
            return Math.Max(entity.Vertexes[0].StartWidth, defaultMinWeight);
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
        private DxfDocument _dxfDocument;
        private Canvas _canvas;
        private DxfLayerReader _layerReader;
        private DxfStyleManager _styleManager;
        private Insert _parentInsert;
        public AciColor _defaultAciColor = new AciColor(255, 255, 255);
        private double _margin = 0.05;
        private double _fontFactor = 1.0;
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
        public IEnumerable<AttributeDefinition> AttributeDefinitions { get; private set; }

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
                _dxfDocument = DxfDocument.Load(filePath);
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
            entities.AddRange(dxf.Entities.Ellipses);
            entities.AddRange(dxf.Entities.Splines);
            entities.AddRange(dxf.Entities.Solids);
            entities.AddRange(dxf.Entities.Hatches);
            entities.AddRange(dxf.Entities.Dimensions);
            entities.AddRange(dxf.Entities.Inserts);
            EntityObjects = entities.ToArray();
            AttributeDefinitions = dxf.Entities.OfType<AttributeDefinition>().ToArray();
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

                    case Polyline2D polyline2D:
                        return new Rect(new Point(polyline2D.Vertexes.Min(x=>x.Position.X), polyline2D.Vertexes.Min(x => x.Position.Y)),
                                        new Point(polyline2D.Vertexes.Max(x => x.Position.X), polyline2D.Vertexes.Max(x => x.Position.Y)));

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
                case Circle circle:
                    RenderCircle(circle);
                    break;
                case Arc arc:
                    RenderArc(arc);
                    break;
                case Polyline2D polyline:
                    RenderPolyline(polyline);
                    //RenderLwPolyline(polyline);
                    break;
                case Text text:
                    RenderText(text);
                    break;
                case MText mtext:
                    RenderMText(mtext);
                    break;
                case Ellipse ellipse:
                    RenderEllipseAsPath(ellipse);
                    break;
                case Spline spline:
                    RenderSplineAsPolyline(spline);
                    break;
                case Solid solid:
                    RenderSolidAsPolygon(solid);
                    break;
                case Hatch hatch:
                    //过于复杂，放弃
                    //RenderHatch(hatch); 
                    break;
                case Dimension dimension:
                    //过于复杂，仅处理文字
                    RenderDimensionTextOnly(dimension);
                    break;
                case Insert insert:
                    RenderInsert(insert);
                    break;
                    // 可以根据需要添加更多实体类型
            }
        }
        #region Line, Arc, Circle, Ellipse
        private void RenderLine(Line line, Insert insert = null)
        {
            // 解析实体的最终样式
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(line): 
                                        !line.Color.IsByLayer && !line.Color.IsByBlock?line.Color:
                                        _styleManager.ResolveEntityColor(insert);
            var finalLineweight = _styleManager.ResolveEntityLineweight(line);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            System.Windows.Shapes.Line wpfLine = new System.Windows.Shapes.Line
            {
                X1 = ConvertToCanvasPointX(line.StartPoint.X, isInBlock),//(line.StartPoint.X + _offsetX) * _scale + _panX,
                Y1 = ConvertToCanvasPointY(line.StartPoint.Y, isInBlock),//(_dxfHeight - (line.StartPoint.Y + _offsetY)) * _scale + _panY,
                X2 = ConvertToCanvasPointX(line.EndPoint.X, isInBlock),//(line.EndPoint.X + _offsetX) * _scale + _panX,
                Y2 = ConvertToCanvasPointY(line.EndPoint.Y, isInBlock),//(_dxfHeight - (line.EndPoint.Y + _offsetY)) * _scale + _panY,
                Stroke = new SolidColorBrush(wpfColor),
                StrokeThickness = wpfLinewidth * _scale
            };

            SetLineType(wpfLine, line.Linetype);
            _canvas.Children.Add(wpfLine);
        }
        private void RenderArc(Arc arc, Insert insert = null)
        {
            // 解析实体的最终样式
            //var finalColor = _styleManager.ResolveEntityColor(arc);
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(arc) : 
                                             !arc.Color.IsByLayer && !arc.Color.IsByBlock ? arc.Color : 
                                             _styleManager.ResolveEntityColor(insert);
            var finalLineweight = _styleManager.ResolveEntityLineweight(arc);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            // 计算中心点
            //Point center = ConvertToCanvasPoint(arc.Center);
            Point center = ConvertToCanvasPoint(arc.Center, isInBlock);
            //    new Point(
            //    (arc.Center.X + _offsetX) * _scale + _panX,
            //    (_dxfHeight - (arc.Center.Y + _offsetY)) * _scale + _panY
            //);

            double radius = arc.Radius * _scale;

            // 转换角度：DXF逆时针 -> WPF顺时针
            double startAngle = 360 - arc.EndAngle; // 注意：起点和终点要交换
            double endAngle = 360 - arc.StartAngle;

            if (endAngle < startAngle) endAngle += 360;

            // 计算起点和终点
            double startRad = startAngle * Math.PI / 180.0;
            double endRad = endAngle * Math.PI / 180.0;

            Point startPoint = new Point(
                center.X + radius * Math.Cos(startRad),
                center.Y + radius * Math.Sin(startRad)
            );

            Point endPoint = new Point(
                center.X + radius * Math.Cos(endRad),
                center.Y + radius * Math.Sin(endRad)
            );

            // 创建Path
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false
            };

            ArcSegment arcSegment = new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                IsLargeArc = (endAngle - startAngle) > 180,
                SweepDirection = SweepDirection.Clockwise,
                RotationAngle = 0
            };

            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);

            Path wpfPath = new Path
            {
                Data = pathGeometry,
                Stroke = new SolidColorBrush(wpfColor),
                StrokeThickness = wpfLinewidth * _scale,
                Fill = Brushes.Transparent
            };

            SetLineType(wpfPath, arc.Linetype);
            _canvas.Children.Add(wpfPath);
        }
        private void RenderArcAsLineSegment(Arc arc, Insert insert = null)
        {
            // 解析实体的最终样式
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(arc) :
                                             !arc.Color.IsByLayer && !arc.Color.IsByBlock ? arc.Color :
                                             _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(arc);
            var finalLineweight = _styleManager.ResolveEntityLineweight(arc);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            // 计算中心点
            Point center = ConvertToCanvasPoint(arc.Center, isInBlock);

            double radius = arc.Radius * _scale;

            // 创建路径几何
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();

            // 计算圆弧上的点 - 关键修正：角度方向
            double startAngle = arc.StartAngle;
            double endAngle = arc.EndAngle;

            // 确保角度正确
            if (endAngle < startAngle) endAngle += 360;

            int segments = Math.Max(8, (int)((endAngle - startAngle) / 5));
            double angleStep = (endAngle - startAngle) / segments;

            // 起点 - 关键修正：角度计算
            // DXF角度：0度在X轴正方向，逆时针增加
            // WPF需要转换为：0度在X轴正方向，顺时针增加
            double startRad = (360 - startAngle) * Math.PI / 180.0; // 反转角度方向
            Point startPoint = new Point(
                center.X + radius * Math.Cos(startRad),
                center.Y + radius * Math.Sin(startRad)
            );

            pathFigure.StartPoint = startPoint;
            pathFigure.IsClosed = false;

            // 使用多个LineSegment
            for (int i = 1; i <= segments; i++)
            {
                double angle = startAngle + i * angleStep;
                double angleRad = (360 - angle) * Math.PI / 180.0; // 反转角度方向

                Point point = new Point(
                    center.X + radius * Math.Cos(angleRad),
                    center.Y + radius * Math.Sin(angleRad)
                );

                LineSegment lineSegment = new LineSegment(point, true);
                pathFigure.Segments.Add(lineSegment);
            }

            pathGeometry.Figures.Add(pathFigure);

            Path wpfPath = new Path
            {
                Data = pathGeometry,
                Stroke = new SolidColorBrush(wpfColor),
                StrokeThickness = wpfLinewidth * _scale,
                Fill = Brushes.Transparent
            };

            SetLineType(wpfPath, arc.Linetype);
            _canvas.Children.Add(wpfPath);
        }
        private void RenderCircle(Circle circle, Insert insert = null)
        {
            // 解析实体的最终样式
            //var finalColor = _styleManager.ResolveEntityColor(circle);
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(circle) : 
                                         !circle.Color.IsByLayer && !circle.Color.IsByBlock ? circle.Color : 
                                         _styleManager.ResolveEntityColor(insert);
            var finalLineweight = _styleManager.ResolveEntityLineweight(circle);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            // 创建Ellipse来表示Circle
            System.Windows.Shapes.Ellipse wpfEllipse = new System.Windows.Shapes.Ellipse
            {
                Width = circle.Radius * 2 * _scale,
                Height = circle.Radius * 2 * _scale,
                Stroke = new SolidColorBrush(wpfColor),
                StrokeThickness = wpfLinewidth * _scale,
                Fill = Brushes.Transparent // 圆形通常不填充
            };

            // 设置位置（圆心坐标）
            //double centerX = ConvertToCanvasPointX(circle.Center.X);// (circle.Center.X + _offsetX) * _scale + _panX;
            //double centerY = ConvertToCanvasPointY(circle.Center.Y);// (_dxfHeight - (circle.Center.Y + _offsetY)) * _scale + _panY;
            double centerX = ConvertToCanvasPointX(circle.Center.X, isInBlock);
            double centerY = ConvertToCanvasPointY(circle.Center.Y, isInBlock);

            Canvas.SetLeft(wpfEllipse, centerX - wpfEllipse.Width / 2);
            Canvas.SetTop(wpfEllipse, centerY - wpfEllipse.Height / 2);

            SetLineType(wpfEllipse, circle.Linetype);
            _canvas.Children.Add(wpfEllipse);
        }
        private void RenderEllipse(Ellipse ellipse, Insert insert = null)
        {
            // 解析实体的最终样式
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(ellipse) :
            !ellipse.Color.IsByLayer && !ellipse.Color.IsByBlock ? ellipse.Color :
                                             _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(ellipse);
            var finalLineweight = _styleManager.ResolveEntityLineweight(ellipse);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            // 创建WPF Ellipse
            System.Windows.Shapes.Ellipse wpfEllipse = new System.Windows.Shapes.Ellipse
            {
                Stroke = new SolidColorBrush(wpfColor),
                StrokeThickness = wpfLinewidth * _scale,
                Fill = Brushes.Transparent
            };

            // 计算椭圆的边界框
            var boundingBox = ellipse.PolygonalVertexes(6); // 使用6个点来估算边界
            double minX = boundingBox.Min(p => p.X);
            double maxX = boundingBox.Max(p => p.X);
            double minY = boundingBox.Min(p => p.Y);
            double maxY = boundingBox.Max(p => p.Y);

            double width = (maxX - minX) * _scale;
            double height = (maxY - minY) * _scale;

            wpfEllipse.Width = width;
            wpfEllipse.Height = height;

            // 设置位置（中心点）
            //double centerX = (ellipse.Center.X + _offsetX) * _scale + _panX;
            //double centerY = (_dxfHeight - (ellipse.Center.Y + _offsetY)) * _scale + _panY;
            double centerX = ConvertToCanvasPointX(ellipse.Center.X, isInBlock);
            double centerY = ConvertToCanvasPointY(ellipse.Center.Y, isInBlock);

            Canvas.SetLeft(wpfEllipse, centerX - width / 2);
            Canvas.SetTop(wpfEllipse, centerY - height / 2);

            // 处理旋转（如果有）
            if (Math.Abs(ellipse.Rotation) > 0.001)
            {
                RotateTransform rotateTransform = new RotateTransform(ellipse.Rotation, width / 2, height / 2);
                wpfEllipse.RenderTransform = rotateTransform;
            }

            SetLineType(wpfEllipse, ellipse.Linetype);
            _canvas.Children.Add(wpfEllipse);
        }
        private void RenderEllipseAsPath(Ellipse ellipse, Insert insert = null)
        {
            // 解析实体的最终样式
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(ellipse) :
                                         !ellipse.Color.IsByLayer && !ellipse.Color.IsByBlock ? ellipse.Color :
                                         _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(ellipse);
            var finalLineweight = _styleManager.ResolveEntityLineweight(ellipse);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            // 使用NetDXF提供的多边形顶点来渲染椭圆
            try
            {
                // 生成椭圆的多边形近似 - 返回的是局部坐标
                var vertexes = ellipse.PolygonalVertexes(36); // 36个点

                if (vertexes.Count < 2)
                    return;

                // 创建路径几何
                PathGeometry pathGeometry = new PathGeometry();
                PathFigure pathFigure = new PathFigure();

                // 转换第一个点为世界坐标并设置为起点
                Vector2 firstVertex = vertexes[0];
                Point startPoint = new Point(
                    ConvertToCanvasPointX(ellipse.Center.X + firstVertex.X,isInBlock),
                    ConvertToCanvasPointY(ellipse.Center.Y + firstVertex.Y, isInBlock)
                );
                //Point startPoint = new Point(
                //    (ellipse.Center.X + firstVertex.X + _offsetX) * _scale + _panX,
                //    (_dxfHeight - (ellipse.Center.Y + firstVertex.Y + _offsetY)) * _scale + _panY
                //);

                pathFigure.StartPoint = startPoint;
                pathFigure.IsClosed = true;

                // 添加多段线
                PolyLineSegment polySegment = new PolyLineSegment();
                for (int i = 1; i < vertexes.Count; i++)
                {
                    Vector2 vertex = vertexes[i];
                    // 将局部坐标转换为世界坐标：世界坐标 = 中心点 + 局部坐标
                    Point point = new Point(
                        ConvertToCanvasPointX(ellipse.Center.X + vertex.X, isInBlock),
                        ConvertToCanvasPointY(ellipse.Center.Y + vertex.Y, isInBlock)
                    );
                    //Point point = new Point(
                    //    (ellipse.Center.X + vertex.X + _offsetX) * _scale + _panX,
                    //    (_dxfHeight - (ellipse.Center.Y + vertex.Y + _offsetY)) * _scale + _panY
                    //);
                    polySegment.Points.Add(point);
                }

                pathFigure.Segments.Add(polySegment);
                pathGeometry.Figures.Add(pathFigure);

                Path wpfPath = new Path
                {
                    Data = pathGeometry,
                    Stroke = new SolidColorBrush(wpfColor),
                    StrokeThickness = wpfLinewidth * _scale,
                    Fill = Brushes.Transparent
                };

                SetLineType(wpfPath, ellipse.Linetype);
                _canvas.Children.Add(wpfPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ellipse rendering failed: {ex.Message}");
            }
        }
        #endregion

        #region Spline, Solid
        private void RenderSpline(Spline spline, Insert insert = null)
        {
            // 解析实体的最终样式
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(spline) :
                                         !spline.Color.IsByLayer && !spline.Color.IsByBlock ? spline.Color :
                                         _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(spline);
            var finalLineweight = _styleManager.ResolveEntityLineweight(spline);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            // 创建路径几何
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();

            // 生成样条曲线上的点
            List<Point> points = new List<Point>();

            // 方法1：使用控制点（简单但不够平滑）
            if (spline.ControlPoints != null && spline.ControlPoints.Count() > 0)
            {
                foreach (var controlPoint in spline.ControlPoints)
                {
                    Point wpfPoint = ConvertToCanvasPoint(controlPoint,isInBlock);
                    //Point wpfPoint = new Point(
                    //    (controlPoint.X + _offsetX) * _scale + _panX,
                    //    (_dxfHeight - (controlPoint.Y + _offsetY)) * _scale + _panY
                    //);
                    points.Add(wpfPoint);
                }
            }

            // 方法2：使用NetDXF提供的多边形顶点（推荐）
            try
            {
                var vertexes = spline.PolygonalVertexes(50); // 50个点来近似曲线
                points.Clear();

                foreach (var vertex in vertexes)
                {
                    Point wpfPoint = ConvertToCanvasPoint(vertex, isInBlock);
                    //Point wpfPoint = new Point(
                    //    (vertex.X + _offsetX) * _scale + _panX,
                    //    (_dxfHeight - (vertex.Y + _offsetY)) * _scale + _panY
                    //);
                    points.Add(wpfPoint);
                }
            }
            catch (Exception ex)
            {
                // 如果PolygonalVertexes失败，回退到控制点方法
                Console.WriteLine($"Spline polygonal vertexes failed: {ex.Message}");
            }

            if (points.Count < 2)
                return;

            // 设置起点
            pathFigure.StartPoint = points[0];
            pathFigure.IsClosed = spline.IsClosed;

            // 添加多段线
            PolyLineSegment polySegment = new PolyLineSegment();
            for (int i = 1; i < points.Count; i++)
            {
                polySegment.Points.Add(points[i]);
            }

            pathFigure.Segments.Add(polySegment);
            pathGeometry.Figures.Add(pathFigure);

            Path wpfPath = new Path
            {
                Data = pathGeometry,
                Stroke = new SolidColorBrush(wpfColor),
                StrokeThickness = wpfLinewidth * _scale,
                Fill = Brushes.Transparent
            };

            SetLineType(wpfPath, spline.Linetype);
            _canvas.Children.Add(wpfPath);
        }
        private void RenderSplineAsPolyline(Spline spline, Insert insert = null)
        {
            // 解析实体的最终样式
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(spline) :
                                         !spline.Color.IsByLayer && !spline.Color.IsByBlock ? spline.Color :
                                         _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(spline);
            var finalLineweight = _styleManager.ResolveEntityLineweight(spline);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            try
            {
                // 使用NetDXF的Polyline转换功能
                var polyline = spline.ToPolyline2D(50); // 使用50个段来近似曲线

                if (polyline?.Vertexes == null || polyline.Vertexes.Count < 2)
                    return;

                // 创建路径几何
                PathGeometry pathGeometry = new PathGeometry();
                PathFigure pathFigure = new PathFigure();

                // 设置起点
                Point startPoint = new Point(
                    ConvertToCanvasPointX(polyline.Vertexes[0].Position.X,isInBlock),
                    ConvertToCanvasPointY(polyline.Vertexes[0].Position.Y, isInBlock)
                );
                //Point startPoint = new Point(
                //    (polyline.Vertexes[0].Position.X + _offsetX) * _scale + _panX,
                //    (_dxfHeight - (polyline.Vertexes[0].Position.Y + _offsetY)) * _scale + _panY
                //);

                pathFigure.StartPoint = startPoint;
                pathFigure.IsClosed = spline.IsClosed;

                // 添加多段线
                PolyLineSegment polySegment = new PolyLineSegment();
                for (int i = 1; i < polyline.Vertexes.Count; i++)
                {
                    Point point = new Point(
                        ConvertToCanvasPointX(polyline.Vertexes[i].Position.X, isInBlock),
                        ConvertToCanvasPointY(polyline.Vertexes[i].Position.Y, isInBlock)
                    );
                    //Point point = new Point(
                    //    (polyline.Vertexes[i].Position.X + _offsetX) * _scale + _panX,
                    //    (_dxfHeight - (polyline.Vertexes[i].Position.Y + _offsetY)) * _scale + _panY
                    //);
                    polySegment.Points.Add(point);
                }

                pathFigure.Segments.Add(polySegment);
                pathGeometry.Figures.Add(pathFigure);

                Path wpfPath = new Path
                {
                    Data = pathGeometry,
                    Stroke = new SolidColorBrush(wpfColor),
                    StrokeThickness = wpfLinewidth * _scale,
                    Fill = Brushes.Transparent
                };

                SetLineType(wpfPath, spline.Linetype);
                _canvas.Children.Add(wpfPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Spline to Polyline conversion failed: {ex.Message}");
                // 回退到控制点方法
                RenderSpline(spline);
            }
        }
        private void RenderSolid(Solid solid, Insert insert = null)
        {
            // 解析实体的最终样式
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(solid) :
                                         !solid.Color.IsByLayer && !solid.Color.IsByBlock ? solid.Color :
                                         _styleManager.ResolveEntityColor(insert);

            var finalLineweight = _styleManager.ResolveEntityLineweight(solid);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            // 创建路径几何
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();

            try
            {
                // Solid有4个顶点，但可能有些顶点是相同的（三角形情况）
                List<Vector2> vertices = new List<Vector2>
        {
            solid.FirstVertex,
            solid.SecondVertex,
            solid.FourthVertex,
            solid.ThirdVertex
        };

                // 去除重复的顶点（如果Solid是三角形，第四顶点可能与第三顶点相同）
                List<Vector2> distinctVertices = new List<Vector2>();
                foreach (var vertex in vertices)
                {
                    if (!distinctVertices.Any(v =>
                        Math.Abs(v.X - vertex.X) < 0.001 &&
                        Math.Abs(v.Y - vertex.Y) < 0.001))
                    {
                        distinctVertices.Add(vertex);
                    }
                }

                if (distinctVertices.Count < 3)
                    return;

                // 设置起点
                Point startPoint = new Point(
                    ConvertToCanvasPointX(distinctVertices[0].X, isInBlock),
                    ConvertToCanvasPointY(distinctVertices[0].Y, isInBlock)
                );
                //Point startPoint = new Point(
                //    (distinctVertices[0].X + _offsetX) * _scale + _panX,
                //    (_dxfHeight - (distinctVertices[0].Y + _offsetY)) * _scale + _panY
                //);

                pathFigure.StartPoint = startPoint;
                pathFigure.IsClosed = true;

                // 添加多段线
                PolyLineSegment polySegment = new PolyLineSegment();
                for (int i = 1; i < distinctVertices.Count; i++)
                {
                    Point point = new Point(
                        ConvertToCanvasPointX(distinctVertices[i].X, isInBlock),
                        ConvertToCanvasPointY(distinctVertices[i].Y, isInBlock)
                    );
                    //Point point = new Point(
                    //    (distinctVertices[i].X + _offsetX) * _scale + _panX,
                    //    (_dxfHeight - (distinctVertices[i].Y + _offsetY)) * _scale + _panY
                    //);
                    polySegment.Points.Add(point);
                }

                pathFigure.Segments.Add(polySegment);
                pathGeometry.Figures.Add(pathFigure);

                Path wpfPath = new Path
                {
                    Data = pathGeometry,
                    Stroke = new SolidColorBrush(wpfColor),
                    StrokeThickness = wpfLinewidth * _scale,
                    Fill = new SolidColorBrush(wpfColor) { Opacity = 0.3 } // 半透明填充
                };

                SetLineType(wpfPath, solid.Linetype);
                _canvas.Children.Add(wpfPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Solid rendering failed: {ex.Message}");
            }
        }
        private void RenderSolidAsPolygon(Solid solid, Insert insert = null)
        {
            // 解析实体的最终样式
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(solid) :
                                         !solid.Color.IsByLayer && !solid.Color.IsByBlock ? solid.Color :
                                         _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(solid);
            var finalLineweight = _styleManager.ResolveEntityLineweight(solid);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            try
            {
                // 获取Solid的所有顶点
                List<Vector2> vertices = new List<Vector2>
        {
            solid.FirstVertex,
            solid.SecondVertex,
            solid.FourthVertex,
            solid.ThirdVertex
        };

                // 创建点集合
                PointCollection points = new PointCollection();
                foreach (var vertex in vertices)
                {
                    // 检查顶点是否有效（非重复和有效坐标）
                    Point point = new Point(
                        ConvertToCanvasPointX(vertex.X, isInBlock),
                        ConvertToCanvasPointY(vertex.Y, isInBlock)
                    );
                    //Point point = new Point(
                    //    (vertex.X + _offsetX) * _scale + _panX,
                    //    (_dxfHeight - (vertex.Y + _offsetY)) * _scale + _panY
                    //);

                    // 避免重复点
                    if (!points.Any(p => Math.Abs(p.X - point.X) < 0.1 && Math.Abs(p.Y - point.Y) < 0.1))
                    {
                        points.Add(point);
                    }
                }

                // 需要至少3个点才能形成多边形
                if (points.Count < 3)
                    return;

                // 创建Polygon
                System.Windows.Shapes.Polygon polygon = new System.Windows.Shapes.Polygon
                {
                    Points = points,
                    Stroke = new SolidColorBrush(wpfColor),
                    StrokeThickness = wpfLinewidth * _scale,
                    Fill = new SolidColorBrush(wpfColor) { Opacity = 0.5 }, // 半透明填充
                    StrokeLineJoin = PenLineJoin.Miter
                };

                // 设置线型
                SetLineType(polygon, solid.Linetype);
                _canvas.Children.Add(polygon);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Solid polygon rendering failed: {ex.Message}");
            }
        }
        #endregion

        #region DimensionText
        private void RenderDimensionTextOnly(Dimension dimension, Insert insert = null)
        {
            try
            {
                // 解析实体的最终样式
                bool isInBlock = insert != null;
                var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(dimension) :
                                             !dimension.Color.IsByLayer && !dimension.Color.IsByBlock ? dimension.Color :
                                             _styleManager.ResolveEntityColor(insert);
                //var finalColor = _styleManager.ResolveEntityColor(dimension);
                var wpfColor = _styleManager.ConvertToWpfColor(finalColor);

                // 获取格式化的尺寸文本
                string dimensionText = GetFormattedDimensionText(dimension);

                if (string.IsNullOrEmpty(dimensionText))
                    return;

                // 获取文本位置和样式
                Vector2 textPosition = dimension.TextReferencePoint;
                double textHeight = Math.Max(dimension.Style?.TextHeight ?? 2.5, 2.5); // 默认文字高度
                double rotation = dimension.TextRotation;

                // 创建文本
                TextBlock textBlock = new TextBlock
                {
                    Text = dimensionText,
                    Foreground = new SolidColorBrush(wpfColor),
                    FontSize = textHeight * _scale,
                    FontFamily = new FontFamily(GetDimensionFontFamily(dimension)),
                    // Background = Brushes.White, // 确保文字可读
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // 设置位置（文本参考点通常是文字中心）
                //double left = (textPosition.X + _offsetX) * _scale + _panX;
                //double top = (_dxfHeight - (textPosition.Y + _offsetY)) * _scale + _panY;
                double left = ConvertToCanvasPointX(textPosition.X, isInBlock);
                double top = ConvertToCanvasPointY(textPosition.Y, isInBlock);

                Canvas.SetLeft(textBlock, left - textBlock.ActualWidth / 2);
                Canvas.SetTop(textBlock, top - textBlock.ActualHeight / 2);

                // 应用旋转
                if (Math.Abs(rotation) > 0.001)
                {
                    textBlock.RenderTransform = new RotateTransform(rotation);
                    textBlock.RenderTransformOrigin = new Point(0.5, 0.5); // 围绕中心旋转
                }

                _canvas.Children.Add(textBlock);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dimension text rendering failed: {ex.Message}");
            }
        }
        private string GetFormattedDimensionText(Dimension dimension)
        {
            // 优先使用用户覆盖的文本
            if (!string.IsNullOrEmpty(dimension.UserText))
                return ProcessTextOverride(dimension.UserText);

            // 根据尺寸类型格式化测量值
            double measurement = dimension.Measurement;

            switch (dimension)
            {
                case RadialDimension _:
                    return FormatRadialDimension(dimension, measurement);

                case DiametricDimension _:
                    return FormatDiametricDimension(dimension, measurement);

                case Angular2LineDimension _:
                    return FormatAngularDimension(dimension, measurement);

                default:
                    return FormatLinearDimension(dimension, measurement);
            }
        }
        private string FormatLinearDimension(Dimension dimension, double measurement)
        {
            short precision = dimension.Style?.LengthPrecision ?? 2;
            string format = $"F{precision}";

            // 使用NetDXF的单位格式化方法
            if (dimension.Style?.DimLengthUnits != null)
            {
                try
                {
                    // 创建单位样式格式
                    UnitStyleFormat unitFormat = new UnitStyleFormat
                    {
                        LinearDecimalPlaces = precision,
                        DecimalSeparator = "."
                    };

                    // 根据单位类型格式化
                    switch (dimension.Style.DimLengthUnits)
                    {
                        case LinearUnitType.Decimal:
                            return LinearUnitFormat.ToDecimal(measurement, unitFormat);

                        case LinearUnitType.Architectural:
                            return LinearUnitFormat.ToArchitectural(measurement, unitFormat);

                        case LinearUnitType.Engineering:
                            return LinearUnitFormat.ToEngineering(measurement, unitFormat);

                        case LinearUnitType.Fractional:
                            return LinearUnitFormat.ToFractional(measurement, unitFormat);

                        case LinearUnitType.Scientific:
                            return LinearUnitFormat.ToScientific(measurement, unitFormat);

                        case LinearUnitType.WindowsDesktop:
                            return measurement.ToString(format);

                        default:
                            return measurement.ToString(format);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unit formatting failed: {ex.Message}, using default format");
                    return measurement.ToString(format);
                }
            }

            return measurement.ToString(format);
        }
        private string FormatRadialDimension(Dimension dimension, double measurement)
        {
            string value = FormatLinearDimension(dimension, measurement);
            return $"R{value}"; // 半径符号
        }
        private string FormatDiametricDimension(Dimension dimension, double measurement)
        {
            string value = FormatLinearDimension(dimension, measurement);
            return $"Ø{value}"; // 直径符号
        }
        private string FormatAngularDimension(Dimension dimension, double measurement)
        {
            short precision = dimension.Style?.AngularPrecision ?? 1;
            string format = $"F{precision}";

            // 角度单位格式化
            if (dimension.Style?.DimAngularUnits != null)
            {
                try
                {
                    UnitStyleFormat unitFormat = new UnitStyleFormat
                    {
                        AngularDecimalPlaces = precision,
                        DecimalSeparator = "."
                    };

                    switch (dimension.Style.DimAngularUnits)
                    {
                        case AngleUnitType.DecimalDegrees:
                            return AngleUnitFormat.ToDecimal(measurement, unitFormat);

                        case AngleUnitType.DegreesMinutesSeconds:
                            return AngleUnitFormat.ToDegreesMinutesSeconds(measurement, unitFormat);

                        case AngleUnitType.Gradians:
                            return AngleUnitFormat.ToGradians(measurement, unitFormat);

                        case AngleUnitType.Radians:
                            return AngleUnitFormat.ToRadians(measurement, unitFormat);

                        default:
                            return measurement.ToString(format);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Angular formatting failed: {ex.Message}, using default format");
                    return measurement.ToString(format) + "°";
                }
            }

            return measurement.ToString(format) + "°";
        }
        private string ProcessTextOverride(string textOverride)
        {
            if (string.IsNullOrEmpty(textOverride))
                return textOverride;

            // 处理DXF文本中的特殊字符和标记
            string processedText = textOverride;

            // 移除格式标记
            processedText = Regex.Replace(processedText, @"\\[A-Za-z][^;]*;", "");

            // 处理特殊转义序列
            processedText = processedText.Replace("\\P", "\n")        // 换行
                                        .Replace("\\~", " ")         // 非换行空格
                                        .Replace("\\\\", "\\")       // 反斜杠
                                        .Replace("\\{", "{")         // 左花括号
                                        .Replace("\\}", "}");        // 右花括号

            // 处理多行文本标记
            processedText = processedText.Replace("\\A1;", "");

            return processedText.Trim();
        }
        private string GetDimensionFontFamily(Dimension dimension)
        {
            // 从尺寸样式中获取字体
            string fontName = dimension.Style?.TextStyle?.FontFamilyName;

            if (!string.IsNullOrEmpty(fontName))
            {
                return MapDxfFontToWpf(fontName);
            }

            // 如果没有指定字体，使用默认字体
            return "Arial";
        }
        private string MapDxfFontToWpf(string dxfFontName)
        {
            var fontMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "txt", "Arial" },
        { "simplex", "Arial" },
        { "complex", "Times New Roman" },
        { "italic", "Times New Roman" },
        { "romans", "Times New Roman" },
        { "isocp", "Arial" },
        { "isocp2", "Arial" },
        { "isocp3", "Arial" },
        { "Arial", "Arial" },
        { "Times New Roman", "Times New Roman" }
    };

            return fontMappings.TryGetValue(dxfFontName, out string wpfFont) ? wpfFont : "Arial";
        }

        private void RenderDimensionSimple(Dimension dimension, Insert insert = null)
        {
            try
            {
                bool isInBlock = insert != null;
                var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(dimension) :
                                             !dimension.Color.IsByLayer && !dimension.Color.IsByBlock ? dimension.Color :
                                             _styleManager.ResolveEntityColor(insert);
                //var finalColor = _styleManager.ResolveEntityColor(dimension);
                var wpfColor = _styleManager.ConvertToWpfColor(finalColor);

                // 只渲染尺寸文本
                string dimensionText = GetDimensionText(dimension);
                if (!string.IsNullOrEmpty(dimensionText))
                {
                    Vector2 textPosition = dimension.TextReferencePoint;

                    TextBlock textBlock = new TextBlock
                    {
                        Text = dimensionText,
                        Foreground = new SolidColorBrush(wpfColor),
                        FontSize = 3 * _scale, // 固定字体大小
                        FontFamily = new FontFamily(dimension.Style.TextStyle.FontFamilyName ?? "Arial"),// new FontFamily("Arial"),
                        //Background = Brushes.White
                    };

                    Canvas.SetLeft(textBlock, ConvertToCanvasPointX(textPosition.X, isInBlock));
                    Canvas.SetTop(textBlock, ConvertToCanvasPointY(textPosition.Y, isInBlock));
                    //Canvas.SetLeft(textBlock, (textPosition.X + _offsetX) * _scale + _panX);
                    //Canvas.SetTop(textBlock, (_dxfHeight - (textPosition.Y + _offsetY)) * _scale + _panY);

                    _canvas.Children.Add(textBlock);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simple dimension rendering failed: {ex.Message}");
            }
        }
        private string GetDimensionText(Dimension dimension)
        {
            // 优先使用用户覆盖的文本
            if (!string.IsNullOrEmpty(dimension.UserText))
                return dimension.UserText;

            // 使用测量值
            return dimension.Measurement.ToString("F2");
        }
        #endregion

        #region Hatch - Pendding
        private void RenderHatch(Hatch hatch)
        {
            // 解析实体的最终样式
            var finalColor = _styleManager.ResolveEntityColor(hatch);
            var finalLineweight = _styleManager.ResolveEntityLineweight(hatch);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            try
            {
                // 创建组合几何图形
                GeometryGroup geometryGroup = new GeometryGroup();
                geometryGroup.FillRule = FillRule.Nonzero;

                // 处理每个边界路径
                foreach (var boundaryPath in hatch.BoundaryPaths)
                {
                    Geometry pathGeometry = CreateHatchBoundaryGeometry(boundaryPath);
                    if (pathGeometry != null)
                    {
                        geometryGroup.Children.Add(pathGeometry);
                    }
                }

                if (geometryGroup.Children.Count == 0)
                    return;

                // 创建填充路径
                Path fillPath = new Path
                {
                    Data = geometryGroup,
                    Fill = new SolidColorBrush(wpfColor) { Opacity = 0.5 },
                    Stroke = null
                };

                _canvas.Children.Add(fillPath);

                // 创建边框路径
                if (wpfLinewidth > 0)
                {
                    Path strokePath = new Path
                    {
                        Data = geometryGroup,
                        Stroke = new SolidColorBrush(wpfColor),
                        StrokeThickness = wpfLinewidth * _scale,
                        Fill = null
                    };

                    SetLineType(strokePath, hatch.Linetype);
                    _canvas.Children.Add(strokePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hatch rendering failed: {ex.Message}");
            }
        }
        private Geometry CreateHatchBoundaryGeometry(HatchBoundaryPath boundaryPath)
        {
            PathGeometry geometry = new PathGeometry();

            foreach (var edge in boundaryPath.Edges)
            {
                if (edge.Type == HatchBoundaryPath.EdgeType.Polyline)
                {
                    var polyline = (HatchBoundaryPath.Polyline)edge;

                    // 检查是否是圆形
                    if (IsLikelyCircle(polyline))
                    {
                        Geometry circleGeometry = CreateCircleGeometry(polyline);
                        if (circleGeometry != null)
                        {
                            geometry.AddGeometry(circleGeometry);
                            continue;
                        }
                    }

                    // 检查是否是圆弧
                    if (IsLikelyArc(polyline))
                    {
                        Geometry arcGeometry = CreateArcGeometry(polyline);
                        if (arcGeometry != null)
                        {
                            geometry.AddGeometry(arcGeometry);
                            continue;
                        }
                        continue;
                    }

                    // 默认按多段线处理，但强制闭合
                    Geometry polyGeometry = CreateClosedPolylineGeometry(polyline);
                    if (polyGeometry != null)
                    {
                        geometry.AddGeometry(polyGeometry);
                    }
                }
            }

            return geometry.Figures.Count > 0 ? geometry : null;
        }
        private bool IsLikelyCircle(HatchBoundaryPath.Polyline polyline)
        {
            if (polyline.Vertexes.Count() == 2)
                return true;
            if (polyline.Vertexes.Count() < 3)
                return false;

            // 计算中心点
            Vector3 center = CalculatePolygonCenter(polyline.Vertexes.ToList());
            double avgRadius = polyline.Vertexes.Average(v => Vector3.Distance(center, v));

            // 检查半径的一致性
            double maxRadiusDeviation = polyline.Vertexes.Max(v => Math.Abs(Vector3.Distance(center, v) - avgRadius));

            // 检查首尾点是否接近（闭合）
            double startEndDistance = Vector3.Distance(polyline.Vertexes[0], polyline.Vertexes[polyline.Vertexes.Count() - 1]);

            // 半径偏差小且首尾点接近，认为是圆形
            return maxRadiusDeviation < avgRadius * 0.1 && startEndDistance < avgRadius * 0.1;
        }
        private bool IsLikelyArc(HatchBoundaryPath.Polyline polyline)
        {
            if (polyline.Vertexes.Count() != 3)
                return false;

            // 三个点确定圆弧
            Vector3 p1 = polyline.Vertexes[0];
            Vector3 p2 = polyline.Vertexes[1];
            Vector3 p3 = polyline.Vertexes[2];

            // 检查三点是否共线
            double area = Math.Abs(
                (p2.X - p1.X) * (p3.Y - p1.Y) -
                (p3.X - p1.X) * (p2.Y - p1.Y)
            );

            // 面积不为0表示不共线，可能是圆弧
            return area > 0.001;
        }
        private Geometry CreateCircleGeometry(HatchBoundaryPath.Polyline polyline)
        {
            Vector3 center = CalculatePolygonCenter(polyline.Vertexes.ToList());
            double radius = polyline.Vertexes.Average(v => Vector3.Distance(center, v));

            // 创建圆形几何图形
            EllipseGeometry circleGeometry = new EllipseGeometry(
                ConvertToPoint(center),
                radius * _scale,
                radius * _scale
            );

            return circleGeometry;
        }
        private Geometry CreateArcGeometry(HatchBoundaryPath.Polyline polyline)
        {
            if (polyline.Vertexes.Count() != 3)
                return null;

            Vector3 p1 = polyline.Vertexes[0];
            Vector3 p2 = polyline.Vertexes[1];
            Vector3 p3 = polyline.Vertexes[2];

            // 计算圆心和半径
            Vector3 center = CalculateCircleCenter(p1, p2, p3);
            double radius = Vector3.Distance(center, p1);

            // 计算角度
            double startAngle = Math.Atan2(p1.Y - center.Y, p1.X - center.X);
            double endAngle = Math.Atan2(p3.Y - center.Y, p3.X - center.X);

            // 创建圆弧几何图形
            PathGeometry arcGeometry = new PathGeometry();
            PathFigure figure = new PathFigure();

            figure.StartPoint = ConvertToPoint(p1);
            figure.IsClosed = false;

            ArcSegment arcSegment = new ArcSegment
            {
                Point = ConvertToPoint(p3),
                Size = new Size(radius * _scale, radius * _scale),
                IsLargeArc = Math.Abs(endAngle - startAngle) > Math.PI,
                SweepDirection = SweepDirection.Clockwise,
                RotationAngle = 0
            };

            figure.Segments.Add(arcSegment);
            arcGeometry.Figures.Add(figure);

            return arcGeometry;
        }
        private Geometry CreateClosedPolylineGeometry(HatchBoundaryPath.Polyline polyline)
        {
            if (polyline.Vertexes.Count() < 2)
                return null;

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();

            figure.StartPoint = ConvertToPoint(polyline.Vertexes[0]);

            // Hatch边界总是需要闭合，不管IsClosed的值
            figure.IsClosed = true;

            PolyLineSegment segment = new PolyLineSegment();
            for (int i = 1; i < polyline.Vertexes.Count(); i++)
            {
                segment.Points.Add(ConvertToPoint(polyline.Vertexes[i]));
            }

            figure.Segments.Add(segment);
            geometry.Figures.Add(figure);

            return geometry;
        }
        private Point ConvertToPoint(Vector3 vector)
        {
            return new Point(
                (vector.X + _offsetX) * _scale + _panX,
                (_dxfHeight - (vector.Y + _offsetY)) * _scale + _panY
            );
        }
        private Vector3 CalculatePolygonCenter(List<Vector3> vertices)
        {
            // 计算多边形的中心点（简单平均）
            double centerX = vertices.Average(v => v.X);
            double centerY = vertices.Average(v => v.Y);

            return new Vector3(centerX, centerY, 0);
        }
        private Vector3 CalculateCircleCenter(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // 计算三个点确定的圆的圆心
            double x1 = p1.X, y1 = p1.Y;
            double x2 = p2.X, y2 = p2.Y;
            double x3 = p3.X, y3 = p3.Y;

            double A = x2 - x1;
            double B = y2 - y1;
            double C = x3 - x1;
            double D = y3 - y1;

            double E = A * (x1 + x2) + B * (y1 + y2);
            double F = C * (x1 + x3) + D * (y1 + y3);
            double G = 2 * (A * (y3 - y2) - B * (x3 - x2));

            if (Math.Abs(G) < 0.001)
            {
                // 点共线，返回中点
                return new Vector3((x1 + x2 + x3) / 3, (y1 + y2 + y3) / 3, 0);
            }

            double centerX = (D * E - B * F) / G;
            double centerY = (A * F - C * E) / G;

            return new Vector3(centerX, centerY, 0);
        }
        private void RenderHatchDirect(Hatch hatch, Insert insert = null)
        {
            // 解析实体的最终样式
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(hatch) :
                                         !hatch.Color.IsByLayer && !hatch.Color.IsByBlock ? hatch.Color :
                                         _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(hatch);
            var finalLineweight = _styleManager.ResolveEntityLineweight(hatch);

            // 转换为 WPF 格式
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            try
            {
                PathGeometry geometry = new PathGeometry();

                foreach (var boundaryPath in hatch.BoundaryPaths)
                {
                    foreach (var edge in boundaryPath.Edges)
                    {
                        if (edge.Type == HatchBoundaryPath.EdgeType.Polyline)
                        {
                            var polyline = (HatchBoundaryPath.Polyline)edge;

                            PathFigure figure = new PathFigure();
                            //figure.StartPoint = ConvertToPoint(polyline.Vertexes[0]);
                            //figure.IsClosed = true;

                            PolyLineSegment segment = new PolyLineSegment();

                            // 如果顶点太少，手动增加插值点
                            if (polyline.Vertexes.Count() == 2)// && polyline.IsClosed)
                            {
                                // 2个顶点且闭合 - 可能是圆形，增加插值点
                                Vector3 center = new Vector3(
                                    (polyline.Vertexes[0].X + polyline.Vertexes[1].X) / 2,
                                    (polyline.Vertexes[0].Y + polyline.Vertexes[1].Y) / 2,
                                    0
                                );

                                double radius = Vector3.Distance(center, polyline.Vertexes[0]);
                                figure.StartPoint = ConvertToCanvasPoint(new Vector2(
                                        center.X + radius * Math.Cos(0),
                                        center.Y + radius * Math.Sin(0)), isInBlock);
                                figure.IsClosed = true;
                                // 生成36个点的圆形
                                for (int i = 1; i <= 8; i++)
                                {
                                    double angle = 2 * Math.PI * i / 8;
                                    Vector3 point = new Vector3(
                                        center.X + radius * Math.Cos(angle),
                                        center.Y + radius * Math.Sin(angle),
                                        0
                                    );
                                    segment.Points.Add(ConvertToPoint(point));
                                }
                            }
                            else
                            {
                                // 正常处理多段线
                                for (int i = 1; i < polyline.Vertexes.Count(); i++)
                                {
                                    segment.Points.Add(ConvertToPoint(polyline.Vertexes[i]));
                                }
                            }

                            figure.Segments.Add(segment);
                            geometry.Figures.Add(figure);
                        }
                    }
                }

                if (geometry.Figures.Count == 0)
                    return;

                Path hatchPath = new Path
                {
                    Data = geometry,
                    //Fill = new SolidColorBrush(wpfColor) { Opacity = 0.6 },
                    Stroke = new SolidColorBrush(wpfColor),
                    StrokeThickness = wpfLinewidth * _scale
                };

                SetLineType(hatchPath, hatch.Linetype);
                _canvas.Children.Add(hatchPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Direct hatch rendering failed: {ex.Message}");
            }
        }
        #endregion

        #region Text
        private void RenderText(Text text, Insert insert = null)
        {
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(text) :
                                         !text.Color.IsByLayer && !text.Color.IsByBlock ? text.Color :
                                         _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(text);
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);

            TextBlock textBlock = new TextBlock
            {
                Text = text.Value,
                FontFamily = new FontFamily(text.Style.FontFamilyName),
                FontSize = text.Height * _scale * _fontFactor,
                Foreground = new SolidColorBrush(wpfColor),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            ApplyTextAlignment(textBlock, text);

            // 使用增强的位置计算
            var position = CalculateTextPosition(text, isInBlock);
            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);

            if (Math.Abs(text.Rotation) > 0.001)
            {
                textBlock.RenderTransform = new RotateTransform(-text.Rotation);
            }

            _canvas.Children.Add(textBlock);
        }
        private void ApplyTextAlignment(TextBlock textBlock, Text text)
        {
            switch (text.Alignment)
            {
                // 顶部对齐
                case netDxf.Entities.TextAlignment.TopLeft:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case netDxf.Entities.TextAlignment.TopCenter:
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case netDxf.Entities.TextAlignment.TopRight:
                    textBlock.TextAlignment = TextAlignment.Right;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;
                    break;

                // 中间对齐
                case netDxf.Entities.TextAlignment.MiddleLeft:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    break;
                case netDxf.Entities.TextAlignment.MiddleCenter:
                case netDxf.Entities.TextAlignment.Middle:
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    break;
                case netDxf.Entities.TextAlignment.MiddleRight:
                    textBlock.TextAlignment = TextAlignment.Right;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    break;

                // 底部对齐
                case netDxf.Entities.TextAlignment.BottomLeft:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case netDxf.Entities.TextAlignment.BottomCenter:
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case netDxf.Entities.TextAlignment.BottomRight:
                    textBlock.TextAlignment = TextAlignment.Right;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;

                // 基线对齐
                case netDxf.Entities.TextAlignment.BaselineLeft:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case netDxf.Entities.TextAlignment.BaselineCenter:
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case netDxf.Entities.TextAlignment.BaselineRight:
                    textBlock.TextAlignment = TextAlignment.Right;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;

                // 特殊对齐方式
                case netDxf.Entities.TextAlignment.Aligned:
                case netDxf.Entities.TextAlignment.Fit:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;

                // 默认情况
                default:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
            }
        }
        private Point CalculateTextPosition(Text text, bool isInBlock)
        {
            double baseX = ConvertToCanvasPointX(text.Position.X, isInBlock);
            double baseY = ConvertToCanvasPointY(text.Position.Y, isInBlock);

            // 测量文字尺寸
            var textSize = MeasureTextSize(text);
            double textWidth = textSize.Width;
            double textHeight = textSize.Height;

            double finalX = baseX;
            double finalY = baseY;

            // 根据对齐方式调整位置
            switch (text.Alignment)
            {
                // 水平居中
                case netDxf.Entities.TextAlignment.TopCenter:
                case netDxf.Entities.TextAlignment.MiddleCenter:
                case netDxf.Entities.TextAlignment.BottomCenter:
                case netDxf.Entities.TextAlignment.BaselineCenter:
                case netDxf.Entities.TextAlignment.Middle:
                    finalX = baseX - textWidth / 2;
                    break;

                // 水平右对齐
                case netDxf.Entities.TextAlignment.TopRight:
                case netDxf.Entities.TextAlignment.MiddleRight:
                case netDxf.Entities.TextAlignment.BottomRight:
                case netDxf.Entities.TextAlignment.BaselineRight:
                    finalX = baseX - textWidth;
                    break;

                // 水平左对齐（默认）
                default:
                    finalX = baseX;
                    break;
            }

            // 垂直位置调整
            switch (text.Alignment)
            {
                // 顶部对齐
                case netDxf.Entities.TextAlignment.TopLeft:
                case netDxf.Entities.TextAlignment.TopCenter:
                case netDxf.Entities.TextAlignment.TopRight:
                    finalY = baseY;
                    break;

                // 垂直居中
                case netDxf.Entities.TextAlignment.MiddleLeft:
                case netDxf.Entities.TextAlignment.MiddleCenter:
                case netDxf.Entities.TextAlignment.MiddleRight:
                case netDxf.Entities.TextAlignment.Middle:
                    finalY = baseY - textHeight / 2;
                    break;

                // 底部对齐
                case netDxf.Entities.TextAlignment.BottomLeft:
                case netDxf.Entities.TextAlignment.BottomCenter:
                case netDxf.Entities.TextAlignment.BottomRight:
                    finalY = baseY - textHeight;
                    break;

                // 基线对齐
                case netDxf.Entities.TextAlignment.BaselineLeft:
                case netDxf.Entities.TextAlignment.BaselineCenter:
                case netDxf.Entities.TextAlignment.BaselineRight:
                    // 基线对齐，文字基线在指定位置
                    finalY = baseY - textHeight * 0.75; // 调整系数以获得更好的基线效果
                    break;

                // 对齐和拟合模式
                case netDxf.Entities.TextAlignment.Aligned:
                case netDxf.Entities.TextAlignment.Fit:
                    finalY = baseY - textHeight;
                    break;

                default:
                    finalY = baseY - textHeight;
                    break;
            }

            return new Point(finalX, finalY);
        }
        private Size MeasureTextSize(Text text)
        {
            try
            {
                // 使用 FormattedText 精确测量
                var formattedText = new FormattedText(
                    text.Value,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(text.Style.FontFamilyName),
                    text.Height * _scale * _fontFactor,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(_canvas).PixelsPerDip);

                return new Size(formattedText.Width, formattedText.Height);
            }
            catch
            {
                // 回退到简化估算
                return EstimateTextSize(text);
            }
        }
        private Size EstimateTextSize(Text text)
        {
            // 简化估算文字尺寸
            double averageCharWidth = text.Height * _scale * 0.6;
            double textWidth = text.Value.Length * averageCharWidth;
            double textHeight = text.Height * _scale * _fontFactor;

            return new Size(textWidth, textHeight);
        }
        #endregion

        #region MText
        private void RenderMText(MText mtext, Insert insert = null)
        {
            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(mtext) :
                                         !mtext.Color.IsByLayer && !mtext.Color.IsByBlock ? mtext.Color :
                                         _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(mtext);
            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);

            TextBlock textBlock = new TextBlock
            {
                // Text = ProcessMTextContent(mtext.Value),
                Text = MTextParser.GetPlainText(mtext.Value),
                FontFamily = new FontFamily(mtext.Style.FontFamilyName),
                FontSize = mtext.Height * _scale * _fontFactor,
                Foreground = new SolidColorBrush(wpfColor),
                TextWrapping = TextWrapping.Wrap,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // 设置多行文字的宽度（如果有矩形宽度）
            if (mtext.RectangleWidth > 0)
            {
                textBlock.Width = mtext.RectangleWidth * _scale;
            }

            ApplyMTextAlignment(textBlock, mtext);

            // 计算多行文字位置
            var position = CalculateMTextPosition(mtext, isInBlock);
            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);

            if (Math.Abs(mtext.Rotation) > 0.001)
            {
                textBlock.RenderTransform = new RotateTransform(-mtext.Rotation);
            }

            _canvas.Children.Add(textBlock);
        }
        // 应用 MText 对齐方式
        private void ApplyMTextAlignment(TextBlock textBlock, MText mtext)
        {
            switch (mtext.AttachmentPoint)
            {
                case MTextAttachmentPoint.TopLeft:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case MTextAttachmentPoint.TopCenter:
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case MTextAttachmentPoint.TopRight:
                    textBlock.TextAlignment = TextAlignment.Right;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;
                    break;

                case MTextAttachmentPoint.MiddleLeft:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    break;
                case MTextAttachmentPoint.MiddleCenter:
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    break;
                case MTextAttachmentPoint.MiddleRight:
                    textBlock.TextAlignment = TextAlignment.Right;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    break;

                case MTextAttachmentPoint.BottomLeft:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case MTextAttachmentPoint.BottomCenter:
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case MTextAttachmentPoint.BottomRight:
                    textBlock.TextAlignment = TextAlignment.Right;
                    textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    break;

                default:
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;
                    break;
            }
        }
        // 计算 MText 位置
        private Point CalculateMTextPosition(MText mtext, bool isInBlock)
        {
            double baseX = ConvertToCanvasPointX(mtext.Position.X, isInBlock);
            double baseY = ConvertToCanvasPointY(mtext.Position.Y, isInBlock);

            // 测量多行文字尺寸
            var textSize = MeasureMTextSize(mtext);
            double textWidth = textSize.Width;
            double textHeight = textSize.Height;

            double finalX = baseX;
            double finalY = baseY;

            // 水平位置调整
            switch (mtext.AttachmentPoint)
            {
                case MTextAttachmentPoint.TopCenter:
                case MTextAttachmentPoint.MiddleCenter:
                case MTextAttachmentPoint.BottomCenter:
                    finalX = baseX - textWidth / 2;
                    break;

                case MTextAttachmentPoint.TopRight:
                case MTextAttachmentPoint.MiddleRight:
                case MTextAttachmentPoint.BottomRight:
                    finalX = baseX - textWidth;
                    break;

                default: // 左对齐
                    finalX = baseX;
                    break;
            }

            // 垂直位置调整
            switch (mtext.AttachmentPoint)
            {
                case MTextAttachmentPoint.TopLeft:
                case MTextAttachmentPoint.TopCenter:
                case MTextAttachmentPoint.TopRight:
                    finalY = baseY;
                    break;

                case MTextAttachmentPoint.MiddleLeft:
                case MTextAttachmentPoint.MiddleCenter:
                case MTextAttachmentPoint.MiddleRight:
                    finalY = baseY - textHeight / 2;
                    break;

                case MTextAttachmentPoint.BottomLeft:
                case MTextAttachmentPoint.BottomCenter:
                case MTextAttachmentPoint.BottomRight:
                    finalY = baseY - textHeight;
                    break;

                default:
                    finalY = baseY;
                    break;
            }

            return new Point(finalX, finalY);
        }
        // 测量 MText 尺寸
        private Size MeasureMTextSize(MText mtext)
        {
            try
            {
                var formattedText = new FormattedText(
                    MTextParser.GetPlainText(mtext.Value),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(mtext.Style.FontFamilyName),
                    mtext.Height * _scale * _fontFactor,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(_canvas).PixelsPerDip);

                // 如果有矩形宽度限制，使用实际宽度
                double width = mtext.RectangleWidth > 0 ?
                    //Math.Min(mtext.RectangleWidth * _scale, formattedText.Width) :
                    mtext.RectangleWidth * _scale :
                    formattedText.Width;

                return new Size(width, formattedText.Height);
            }
            catch
            {
                // 回退到简化估算
                return EstimateMTextSize(mtext);
            }
        }
        private Size EstimateMTextSize(MText mtext)
        {
            string content = MTextParser.GetPlainText(mtext.Value);

            // 计算行数
            int lineCount = content.Split('\n').Length;

            // 估算最大行宽度
            string[] lines = content.Split('\n');
            double maxLineWidth = 0;

            foreach (string line in lines)
            {
                double lineWidth = line.Length * (mtext.Height * _scale * _fontFactor * 0.6);
                maxLineWidth = Math.Max(maxLineWidth, lineWidth);
            }

            // 应用矩形宽度限制
            if (mtext.RectangleWidth > 0)
            {
                maxLineWidth = Math.Min(maxLineWidth, mtext.RectangleWidth * _scale);
            }

            double totalHeight = lineCount * (mtext.Height * _scale * _fontFactor * 1.2);

            return new Size(maxLineWidth, totalHeight);
        }
        #endregion

        #region Polyline2D
        private void RenderPolyline(Polyline2D lwPolyline, Insert insert = null)
        {
            try
            {
                //var finalColor = _styleManager.ResolveEntityColor(lwPolyline);
                bool isInBlock = insert != null;
                var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(lwPolyline) :
                                             !lwPolyline.Color.IsByLayer && !lwPolyline.Color.IsByBlock ? lwPolyline.Color :
                                             _styleManager.ResolveEntityColor(insert);
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
                    RenderLwPolylineWithWidthAndBulge(lwPolyline, wpfColor, isInBlock);
                }
                else if (hasBulge)
                {
                    // 只有圆弧
                    RenderLwPolylineWithBulge(lwPolyline, wpfColor, isInBlock);
                }
                else if (hasVariableWidth || lwPolyline.Lineweight == Lineweight.ByLayer || lwPolyline.Lineweight == Lineweight.ByBlock)
                {
                    // 只有可变宽度
                    RenderVariableWidthPolyline(lwPolyline, wpfColor, isInBlock);
                }
                else
                {
                    // 简单直线段
                    RenderSimpleLwPolyline(lwPolyline, wpfColor, isInBlock);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"渲染 LwPolyline 时出错: {ex.Message}");
                // 降级到简单渲染
                bool isInBlock = insert != null;
                var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(lwPolyline) :
                                             !lwPolyline.Color.IsByLayer && !lwPolyline.Color.IsByBlock ? lwPolyline.Color :
                                             _styleManager.ResolveEntityColor(insert);
                //var finalColor = _styleManager.ResolveEntityColor(lwPolyline);
                var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
                RenderSimpleLwPolyline(lwPolyline, wpfColor, isInBlock);
            }
        }
        private void RenderSimpleLwPolyline(Polyline2D lwPolyline, Color color, bool isInBlock)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            var brush = new SolidColorBrush(color);
            //var finalLineweight = _styleManager.ResolveEntityLineweight(lwPolyline);
            //var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);
            var wpfLinewidth = _styleManager.ResolveEntityLineWidth(lwPolyline);

            var points = new PointCollection();
            foreach (var vertex in lwPolyline.Vertexes)
            {
                points.Add(ConvertToCanvasPoint(vertex.Position, isInBlock));
            }

            if (lwPolyline.IsClosed && lwPolyline.Vertexes.Count > 2)
            {
                points.Add(ConvertToCanvasPoint(lwPolyline.Vertexes[0].Position, isInBlock));
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
        private void RenderLwPolylineWithBulge(Polyline2D lwPolyline, Color color, bool isInBlock)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            var brush = new SolidColorBrush(color);
            //var finalLineweight = _styleManager.ResolveEntityLineweight(lwPolyline);
            //var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);
            var wpfLinewidth = _styleManager.ResolveEntityLineWidth(lwPolyline);

            var points = new PointCollection();

            // 遍历所有顶点，处理 bulge
            for (int i = 0; i < lwPolyline.Vertexes.Count; i++)
            {
                var currentVertex = lwPolyline.Vertexes[i];
                var nextVertex = lwPolyline.Vertexes[(i + 1) % lwPolyline.Vertexes.Count];

                // 添加当前点
                points.Add(ConvertToCanvasPoint(currentVertex.Position, isInBlock));

                // 处理 bulge（圆弧段）
                if (currentVertex.Bulge != 0 && i < lwPolyline.Vertexes.Count)
                {
                    var bulgePoints = ConvertBulgeToPoints(currentVertex, nextVertex, currentVertex.Bulge, isInBlock);
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
                points.Add(ConvertToCanvasPoint(lwPolyline.Vertexes[0].Position, isInBlock));
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
        private void RenderVariableWidthPolyline(Polyline2D lwPolyline, Color color, bool isInBlock)
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
                    var segmentGeometry = CreateConstantWidthSegment(startVertex, endVertex, startWidth, isInBlock);
                    geometryGroup.Children.Add(segmentGeometry);
                }
                else
                {
                    // 宽度不同，使用梯形
                    var segmentGeometry = CreateVariableWidthSegment(startVertex, endVertex, startWidth, endWidth, isInBlock);
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

                var segmentGeometry = CreateVariableWidthSegment(startVertex, endVertex, startWidth, endWidth, isInBlock);
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
        private Geometry CreateConstantWidthSegment(Polyline2DVertex start, Polyline2DVertex end, double width, bool isInBlock)
        {
            var startPoint = ConvertToCanvasPoint(start.Position, isInBlock);
            var endPoint = ConvertToCanvasPoint(end.Position, isInBlock);

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
        private Geometry CreateVariableWidthSegment(Polyline2DVertex start, Polyline2DVertex end, double startWidth, double endWidth, bool isInBlock)
        {
            var startPoint = ConvertToCanvasPoint(start.Position, isInBlock);
            var endPoint = ConvertToCanvasPoint(end.Position, isInBlock);

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
        private void RenderLwPolylineWithWidthAndBulge(Polyline2D lwPolyline, Color color, bool isInBlock)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            var brush = new SolidColorBrush(color);
            var geometryGroup = new GeometryGroup();

            // 获取所有线段
            var segments = GetPolylineSegments(lwPolyline, isInBlock);

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
        private List<PolylineSegment> GetPolylineSegments(Polyline2D lwPolyline, bool isInBlock)
        {
            var segments = new List<PolylineSegment>();

            for (int i = 0; i < lwPolyline.Vertexes.Count - 1; i++)
            {
                var startVertex = lwPolyline.Vertexes[i];
                var endVertex = lwPolyline.Vertexes[i + 1];

                var segment = new PolylineSegment(
                    ConvertToCanvasPoint(startVertex.Position, isInBlock),
                    ConvertToCanvasPoint(endVertex.Position, isInBlock),
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
                    ConvertToCanvasPoint(startVertex.Position, isInBlock),
                    ConvertToCanvasPoint(endVertex.Position, isInBlock),
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
        private void RenderLwPolylineWithBulge(Polyline2D lwPolyline, Insert insert = null)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(lwPolyline) :
                                         !lwPolyline.Color.IsByLayer && !lwPolyline.Color.IsByBlock ? lwPolyline.Color :
                                         _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(lwPolyline);
            //var finalLineweight = _styleManager.ResolveEntityLineweight(lwPolyline);
            //var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ResolveEntityLineWidth(lwPolyline);

            var points = new PointCollection();

            // 遍历所有顶点，处理 bulge
            for (int i = 0; i < lwPolyline.Vertexes.Count; i++)
            {
                var currentVertex = lwPolyline.Vertexes[i];
                var nextVertex = lwPolyline.Vertexes[(i + 1) % lwPolyline.Vertexes.Count];

                // 添加当前点
                points.Add(ConvertToCanvasPoint(currentVertex.Position, isInBlock));

                // 处理 bulge（圆弧段）
                if (currentVertex.Bulge != 0 && i < lwPolyline.Vertexes.Count - 1)
                {
                    var bulgePoints = ConvertBulgeToPoints(currentVertex, nextVertex, currentVertex.Bulge, isInBlock);
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
                points.Add(ConvertToCanvasPoint(lwPolyline.Vertexes[0].Position, isInBlock));
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
        private List<Point> ConvertBulgeToPoints(Polyline2DVertex start, Polyline2DVertex end, double bulge, bool isInBlock)
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
                points.Add(ConvertToCanvasPoint(new Vector2((float)x, (float)y), isInBlock));
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
        private void RenderSimpleLwPolyline(Polyline2D lwPolyline, Insert insert = null)
        {
            if (lwPolyline.Vertexes.Count < 2) return;

            bool isInBlock = insert != null;
            var finalColor = !isInBlock ? _styleManager.ResolveEntityColor(lwPolyline) :
                                         !lwPolyline.Color.IsByLayer && !lwPolyline.Color.IsByBlock ? lwPolyline.Color :
                                         _styleManager.ResolveEntityColor(insert);
            //var finalColor = _styleManager.ResolveEntityColor(lwPolyline);
            //var finalLineweight = _styleManager.ResolveEntityLineweight(lwPolyline);
            //var wpfLinewidth = _styleManager.ConvertToWpfLinewidth(finalLineweight);

            var wpfColor = _styleManager.ConvertToWpfColor(finalColor);
            var wpfLinewidth = _styleManager.ResolveEntityLineWidth(lwPolyline);

            var points = new PointCollection();


            foreach (var vertex in lwPolyline.Vertexes)
            {
                points.Add(ConvertToCanvasPoint(vertex.Position, isInBlock));
            }

            if (lwPolyline.IsClosed && lwPolyline.Vertexes.Count > 0)
            {
                points.Add(ConvertToCanvasPoint(lwPolyline.Vertexes[0].Position, isInBlock));
            }

            CreatePolylineFromPoints(points, lwPolyline, wpfLinewidth, new SolidColorBrush(wpfColor));
        }
        private void CreatePolylineFromPoints(PointCollection points, Polyline2D LwPolyline,double lineWeight, Brush brush)
        {
            System.Windows.Shapes.Polyline wpfPolyline = new System.Windows.Shapes.Polyline
            {
                Points = points,
                Stroke = brush,
                StrokeThickness = lineWeight * _scale,
                Fill = LwPolyline.IsClosed ? CreateFillBrush(brush) : Brushes.Transparent,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round
            };

            SetLineType(wpfPolyline, LwPolyline.Linetype);
            _canvas.Children.Add(wpfPolyline);
        }
        #endregion

        #region Insert 
        private void RenderInsert(Insert insert, Insert parentInsert = null)
        {
            try
            {
                bool isInBlock = parentInsert != null;
                netDxf.Blocks.Block block = _dxfDocument.Blocks[insert.Block.Name];
                if (block == null || !block.Layer.IsVisible) return;
                _parentInsert = insert;
                TransformGroup transform = new TransformGroup();

                // 检查是否需要镜像
                bool mirrorX = insert.Scale.X < 0;
                bool mirrorY = insert.Scale.Y < 0;

                // 使用绝对值的缩放，通过负值实现镜像
                double scaleX = Math.Abs(insert.Scale.X);
                double scaleY = Math.Abs(insert.Scale.Y);

                // 应用缩放（使用绝对值）
                if (Math.Abs(scaleX - 1.0) > 0.001 || Math.Abs(scaleY - 1.0) > 0.001)
                {
                    ScaleTransform scale = new ScaleTransform(scaleX, scaleY);
                    transform.Children.Add(scale);
                }

                // 应用镜像（如果需要）
                if (mirrorX || mirrorY)
                {
                    // 镜像实际上是通过额外的负缩放实现的
                    ScaleTransform mirror = new ScaleTransform(
                        mirrorX ? -1 : 1,
                        mirrorY ? -1 : 1
                    );
                    transform.Children.Add(mirror);
                }

                // 应用旋转
                if (Math.Abs(insert.Rotation) > 0.001)
                {
                    // 注意：镜像会影响旋转方向，可能需要调整
                    double adjustedRotation = AdjustRotationForMirror(insert.Rotation, mirrorX, mirrorY);
                    RotateTransform rotate = new RotateTransform(adjustedRotation);
                    transform.Children.Add(rotate);
                }

                // 应用平移
                TranslateTransform translate = new TranslateTransform(
                    ConvertToCanvasPointX(insert.Position.X, isInBlock),
                    ConvertToCanvasPointY(insert.Position.Y, isInBlock)
                );
                transform.Children.Add(translate);

                // 渲染块实体
                RenderBlockEntities(block, transform);
                // 渲染属性
                RenderAttributes(insert, transform);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Insert with mirror rendering failed: {ex.Message}");
            }
        }
        private double AdjustRotationForMirror(double originalRotation, bool mirrorX, bool mirrorY)
        {
            double adjustedRotation = originalRotation;

            // 镜像会影响旋转方向
            if (mirrorX && !mirrorY)
            {
                // 仅X轴镜像：旋转方向反转
                adjustedRotation =  originalRotation;
            }
            else if (!mirrorX && mirrorY)
            {
                // 仅Y轴镜像：旋转方向反转
                adjustedRotation = -originalRotation;
            }
            else if (mirrorX && mirrorY)
            {
                // 双轴镜像：相当于180度旋转，保持原方向
                // 不需要调整
            }

            return -adjustedRotation;
        }
        private void RenderBlockEntities(netDxf.Blocks.Block block, TransformGroup transform)
        {
            foreach (var entity in block.Entities)
            {
                // 为每个实体创建容器来应用变换
                Canvas entityContainer = new Canvas();
                entityContainer.RenderTransform = transform;

                // 临时设置当前画布为实体容器
                var originalCanvas = _canvas;
                _canvas = entityContainer;

                try
                {
                    // 渲染实体（使用之前定义的方法）
                    RenderEntityInBlock(entity);
                }
                finally
                {
                    // 恢复原始画布
                    _canvas = originalCanvas;
                }

                // 将实体容器添加到主画布
                _canvas.Children.Add(entityContainer);
            }
        }
        private void RenderEntityInBlock(EntityObject entity)
        {
            // 注意：这里使用之前定义的渲染方法，但坐标是相对于块原点的
            switch (entity)
            {
                case Line line:
                    RenderLine(line, _parentInsert);
                    break;
                case Circle circle:
                    RenderCircle(circle, _parentInsert);
                    break;
                case Arc arc:
                    RenderArc(arc, _parentInsert);
                    break;
                case Polyline2D polyline:
                    RenderPolyline(polyline, _parentInsert);
                    break;
                case Insert insert:
                    RenderInsert(insert, _parentInsert);
                    break;
                default:
                    Console.WriteLine($"Unsupported block entity type: {entity.GetType().Name}");
                    break;
            }
        }
        private void RenderAttributes(Insert insert, TransformGroup transform)
        {
            try
            {
                // 获取块引用中的所有属性
                var attributes = insert.Attributes;
                if (attributes == null || attributes.Count == 0)
                    return;

                // 获取块定义中的属性定义
                var block = _dxfDocument.Blocks[insert.Block.Name];
                var attributeDefinitions = block.Entities.OfType<AttributeDefinition>().ToList();

                foreach (var attribute in attributes)
                {
                    // 找到对应的属性定义
                    var attributeDef = attributeDefinitions.FirstOrDefault(ad =>
                        string.Equals(ad.Tag, attribute.Tag, StringComparison.OrdinalIgnoreCase));

                    if (attributeDef != null)
                    {
                        RenderSingleAttribute(attribute, attributeDef, insert, transform);
                    }
                    else
                    {
                        // 如果没有找到定义，使用默认值渲染
                        RenderSingleAttribute(attribute, null, insert, transform);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attributes rendering failed: {ex.Message}");
            }
        }
        private void RenderSingleAttribute(netDxf.Entities.Attribute attribute,
                                         AttributeDefinition attributeDef,
                                         Insert insert,
                                         TransformGroup transform)
        {
            try
            {
                // 安全地获取属性值
                string attributeValue = GetAttributeValue(attribute, attributeDef);

                if (string.IsNullOrEmpty(attributeValue))
                    return;

                // 安全地获取属性位置
                Vector3 attributePosition = GetAttributePosition(attribute, attributeDef, insert);

                // 安全地获取属性样式
                double textHeight = GetAttributeTextHeight(attributeDef);
                double rotation = GetAttributeRotation(attributeDef);
                var color = GetAttributeColor(attribute, attributeDef);

                // 创建属性文本
                TextBlock textBlock = new TextBlock
                {
                    Text = attributeValue,
                    Foreground = new SolidColorBrush(_styleManager.ConvertToWpfColor(color)),
                    FontSize = textHeight * _scale,
                    FontFamily = new FontFamily("Arial"),
                    Background = Brushes.Transparent
                };

                //// 创建属性容器来应用块变换
                //Canvas attributeContainer = new Canvas();
                //attributeContainer.RenderTransform = transform;

                // 设置属性在容器中的位置（相对坐标）
                //Point localPosition = new Point(
                //    (attributePosition.X + _offsetX) * _scale + _panX,
                //    (_dxfHeight - (attributePosition.Y + _offsetY)) * _scale + _panY // Y轴翻转
                //);
                //double x = (point.X + _offsetX) * _scale + _panX;
                //double y = (_dxfHeight - (point.Y + _offsetY)) * _scale + _panY;
                Point localPosition = ConvertToCanvasPoint(attributePosition, false);

                Canvas.SetLeft(textBlock, localPosition.X);
                Canvas.SetTop(textBlock, localPosition.Y);

                // 应用属性自身的旋转
                if (Math.Abs(rotation) > 0.001)
                {
                    textBlock.RenderTransform = new RotateTransform(rotation);
                }

                //attributeContainer.Children.Add(textBlock);
                //_canvas.Children.Add(attributeContainer);
                _canvas.Children.Add(textBlock);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Single attribute rendering failed (Tag: {attribute.Tag}): {ex.Message}");
            }
        }
        private string GetAttributeValue(netDxf.Entities.Attribute attribute, AttributeDefinition attributeDef)
        {
            // 优先使用属性的值
            if (!string.IsNullOrEmpty(attribute.Value))
                return attribute.Value;

            // 然后使用属性定义的默认值
            if (attributeDef != null && !string.IsNullOrEmpty(attributeDef.Value))
                return attributeDef.Value;

            return string.Empty;
        }
        private double GetAttributeTextHeight(AttributeDefinition attributeDef)
        {
            return attributeDef?.Height ?? 2.5; // 默认文字高度
        }

        private double GetAttributeRotation(AttributeDefinition attributeDef)
        {
            return attributeDef?.Rotation ?? 0.0;
        }

        private AciColor GetAttributeColor(netDxf.Entities.Attribute attribute, AttributeDefinition attributeDef)
        {
            // 方法1: 尝试将AttributeDefinition作为EntityObject处理
            try
            {
                if (attributeDef != null)
                    return GetAttributeDefinitionColor(attributeDef);
                else if (attribute != null)
                    return GetAttributeColor(attribute);
            }
            catch
            {
                // 如果转换失败
            }
            return _defaultAciColor; // 默认颜色
        }
        private AciColor GetAttributeDefinitionColor(AttributeDefinition attributeDef)
        {
            // 专门处理AttributeDefinition的颜色
            try
            {
                // 尝试直接访问颜色属性
                if (attributeDef.Color != null && attributeDef.Color.IsByLayer == false)
                {
                    return attributeDef.Color;
                }

                // 如果颜色是ByLayer，尝试从图层获取
                if (attributeDef.Layer != null)
                {
                    return attributeDef.Layer.Color;
                }
            }
            catch
            {
                // 如果失败，使用默认颜色
            }
            return _defaultAciColor; // 默认颜色
        }
        private AciColor GetAttributeColor(netDxf.Entities.Attribute attribute)
        {
            // 专门处理Attribute的颜色
            try
            {
                if (attribute.Color != null && attribute.Color.IsByLayer == false)
                {
                    return attribute.Color;
                }

                if (attribute.Layer != null)
                {
                    return attribute.Layer.Color;
                }
            }
            catch
            {
                // 如果失败，使用默认颜色
            }
            return _defaultAciColor; // 默认颜色
        }
        private Vector3 GetAttributePosition(netDxf.Entities.Attribute attribute,
                                           AttributeDefinition attributeDef,
                                           Insert insert)
        {
            Vector3 position;

            // 优先使用属性的位置
            if (attribute.Position != Vector3.Zero)
            {
                position = attribute.Position;
            }
            else if (attributeDef != null && attributeDef.Position != Vector3.Zero)
            {
                // 然后使用属性定义的位置
                position = attributeDef.Position;
            }
            else
            {
                // 默认位置（块插入点）
                position = insert.Position;
            }

            return position;
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

        private void SetLineType(System.Windows.Shapes.Shape shape, Linetype linetype)
        {
            if (linetype.Name.Contains("DASHED") || linetype.Name.Contains("HIDDEN"))
            {
                shape.StrokeDashArray = new DoubleCollection(new double[] { 4, 2 });
            }
            else if (linetype.Name.Contains("DOTTED"))
            {
                shape.StrokeDashArray = new DoubleCollection(new double[] { 1, 2 });
            }
        }
        private Point ConvertToCanvasPoint(Vector2 point, bool isInBlock)
        {
            double x = (point.X + _offsetX) * _scale + _panX;
            double y = (_dxfHeight - (point.Y + _offsetY)) * _scale + _panY;
            if (!isInBlock)
                return new Point(x, y);
            x = point.X * _scale;
            y = - point.Y  * _scale;
                return new Point(x, y);
        }
        private Point ConvertToCanvasPoint(Vector3 point, bool isInBlock)
        {
            double x = (point.X + _offsetX) * _scale + _panX;
            double y = (_dxfHeight - (point.Y + _offsetY)) * _scale + _panY;
            if (!isInBlock)
                return new Point(x, y);
            x = point.X * _scale;
            y = -point.Y * _scale;
            return new Point(x, y);
        }
        private double ConvertToCanvasPointX(double positionX, bool isInBlock)
        {
            if (!isInBlock)
                return (positionX + _offsetX) * _scale + _panX;
            return positionX * _scale;
        }
        private double ConvertToCanvasPointY(double positionY, bool isInBlock)
        {
            if (!isInBlock)
                return (_dxfHeight - (positionY + _offsetY)) * _scale + _panY;
            return - positionY * _scale;
        }

    }
}
