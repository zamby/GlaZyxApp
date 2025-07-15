using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GlazyxApplication
{

    public class SvgService
    {

        // Punto geometricamente rappresentato
        public struct PointSvgModel
        {
            public double X { get; set; }
            public double Y { get; set; }
            public PointSvgModel(double x, double y)
            {
                X = x;
                Y = y;
            }
            public override string ToString() => $"({X}, {Y})";

        }

        // Struttura per rappresentare una trasformazione SVG
        public struct Transform
        {
            public double TranslateX { get; set; }
            public double TranslateY { get; set; }
            public double ScaleX { get; set; }
            public double ScaleY { get; set; }
            public double Rotation { get; set; } // in gradi
            public double RotationCenterX { get; set; }
            public double RotationCenterY { get; set; }

            public Transform()
            {
                TranslateX = 0;
                TranslateY = 0;
                ScaleX = 1;
                ScaleY = 1;
                Rotation = 0;
                RotationCenterX = 0;
                RotationCenterY = 0;
            }
        }

        // Struttura per rappresentare le informazioni di stile SVG
        public struct SvgStyle
        {
            public Color FillColor { get; set; }
            public Color StrokeColor { get; set; }
            public double StrokeWidth { get; set; }
            public bool HasFill { get; set; }
            public bool HasStroke { get; set; }

            public SvgStyle()
            {
                FillColor = Colors.Black;
                StrokeColor = Colors.Black;
                StrokeWidth = 1.0;
                HasFill = true;
                HasStroke = false;
            }
        }

        // Struttura per rappresentare un elemento SVG con stile
        public struct SvgElementData
        {
            public List<PointSvgModel> Points { get; set; }
            public SvgStyle Style { get; set; }
            public string ElementType { get; set; }

            public SvgElementData()
            {
                Points = new List<PointSvgModel>();
                Style = new SvgStyle();
                ElementType = "path";
            }
        }

        public List<SvgElementData> ParseSvgWithStyles(string filePath)
        {
            Console.WriteLine($"[SVG DEBUG] ParseSvgWithStyles chiamato per: {filePath}");

            XDocument svgDoc = XDocument.Load(filePath);
            XNamespace ns = svgDoc.Root?.GetDefaultNamespace() ?? XNamespace.None;
            XElement? root = svgDoc.Root;
            List<SvgElementData> elements = new List<SvgElementData>();

            if (root != null)
            {
                Console.WriteLine($"[SVG DEBUG] Root element trovato: {root.Name}");
                Console.WriteLine($"[SVG DEBUG] Numero di elementi figli: {root.Elements().Count()}");
                
                foreach (XElement element in root.Elements())
                {
                    var parsed = ParseElementWithStyle(element);
                    if (parsed != null)
                    {
                        elements.AddRange(parsed);
                    }
                }
            }
            else
            {
                Console.WriteLine($"[SVG DEBUG] Root element null!");
            }

            Console.WriteLine($"[SVG DEBUG] ParseSvgWithStyles restituisce {elements.Count} elementi totali");
            return elements;
        }

        public List<PointSvgModel> ParseSvg(string filePath)
        {
            Console.WriteLine($"[SVG DEBUG] ParseSvg chiamato per: {filePath}");

            XDocument svgDoc = XDocument.Load(filePath);
            XNamespace ns = svgDoc.Root?.GetDefaultNamespace() ?? XNamespace.None;
            XElement? root = svgDoc.Root;
            List<PointSvgModel> elements = new List<PointSvgModel>();

            if (root != null)
            {
                Console.WriteLine($"[SVG DEBUG] Root element trovato: {root.Name}");
                Console.WriteLine($"[SVG DEBUG] Numero di elementi figli: {root.Elements().Count()}");
                
                foreach (XElement element in root.Elements())
                {
                    var parsed = ParseElement(element);
                    if (parsed != null)
                    {
                        elements.AddRange(parsed);
                    }
                }
            }
            else
            {
                Console.WriteLine($"[SVG DEBUG] Root element null!");
            }

            Console.WriteLine($"[SVG DEBUG] ParseSvg restituisce {elements.Count} punti totali");
            return elements;
        }

        private List<PointSvgModel> ParseElement(XElement element)
        {
            Console.WriteLine($"[SVG DEBUG] ParseElement chiamato per: {element.Name.LocalName}");

            switch (element.Name.LocalName)
            {
                case "path":
                    string? pathData = element.Attribute("d")?.Value;
                    Console.WriteLine($"[SVG DEBUG] Path data trovato: {pathData}");
                    if (pathData != null)
                    {
                        var result = ParsePath(pathData);
                        Console.WriteLine($"[SVG DEBUG] ParsePath ha restituito {result?.Count ?? 0} punti");
                        return result ?? new List<PointSvgModel>();
                    }
                    break;

                case "rect":
                    return ParseRect(element);

                case "circle":
                    return ParseCircle(element);

                case "ellipse":
                    return ParseEllipse(element);

                case "line":
                    return ParseLine(element);

                case "polygon":
                    return ParsePolygon(element);

                case "polyline":
                    return ParsePolyline(element);

                case "g":
                    // Group element - parse children recursively
                    return ParseGroup(element);
            }

            Console.WriteLine($"[SVG DEBUG] Elemento non gestito o senza dati: {element.Name.LocalName}");
            return new List<PointSvgModel>();
        }



        public static List<PointSvgModel> ParsePath(string d)
        {
            var points = new List<PointSvgModel>();
            var commands = SplitCommands(d);
            PointSvgModel currentPoint = new PointSvgModel(0, 0); // Punto corrente
            PointSvgModel startPoint = new PointSvgModel(0, 0);   // Punto di partenza per `Z` o `z`

            foreach (var cmd in commands)
            {
                var type = cmd.Command;
                var paramsList = cmd.Parameters;

                switch (type)
                {
                    case 'M': // Move to (assoluto)
                        currentPoint = new PointSvgModel(paramsList[0], paramsList[1]);
                        startPoint = currentPoint;
                        points.Add(currentPoint);
                        for (int i = 2; i < paramsList.Count; i += 2)
                        {
                            currentPoint = new PointSvgModel(paramsList[i], paramsList[i + 1]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'm': // Move to (relativo)
                        currentPoint = new PointSvgModel(currentPoint.X + paramsList[0], currentPoint.Y + paramsList[1]);
                        startPoint = currentPoint;
                        points.Add(currentPoint);
                        for (int i = 2; i < paramsList.Count; i += 2)
                        {
                            currentPoint = new PointSvgModel(currentPoint.X + paramsList[i], currentPoint.Y + paramsList[i + 1]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'L': // Line to (assoluto)
                        for (int i = 0; i < paramsList.Count; i += 2)
                        {
                            currentPoint = new PointSvgModel(paramsList[i], paramsList[i + 1]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'l': // Line to (relativo)
                        for (int i = 0; i < paramsList.Count; i += 2)
                        {
                            currentPoint = new PointSvgModel(currentPoint.X + paramsList[i], currentPoint.Y + paramsList[i + 1]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'H': // Horizontal line (assoluto)
                        for (int i = 0; i < paramsList.Count; i++)
                        {
                            currentPoint = new PointSvgModel(paramsList[i], currentPoint.Y);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'h': // Horizontal line (relativo)
                        for (int i = 0; i < paramsList.Count; i++)
                        {
                            currentPoint = new PointSvgModel(currentPoint.X + paramsList[i], currentPoint.Y);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'V': // Vertical line (assoluto)
                        for (int i = 0; i < paramsList.Count; i++)
                        {
                            currentPoint = new PointSvgModel(currentPoint.X, paramsList[i]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'v': // Vertical line (relativo)
                        for (int i = 0; i < paramsList.Count; i++)
                        {
                            currentPoint = new PointSvgModel(currentPoint.X, currentPoint.Y + paramsList[i]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'Z': // Close path
                    case 'z':
                        currentPoint = startPoint; // Torna al punto iniziale
                        points.Add(currentPoint);
                        break;

                    case 'A': // Arco ellittico assoluto
                        {
                            double rx = paramsList[0];
                            double ry = paramsList[1];
                            double xAxisRotation = paramsList[2];
                            int largeArcFlag = (int)paramsList[3];
                            int sweepFlag = (int)paramsList[4];
                            var endPoint = new PointSvgModel(paramsList[5], paramsList[6]);

                            points.AddRange(CalculateArcPoints(currentPoint, endPoint, rx, ry, xAxisRotation, largeArcFlag, sweepFlag));
                            currentPoint = endPoint; // Aggiorna il punto corrente
                        }
                        break;

                    case 'a': // Arco ellittico relativo
                        {
                            double rx = paramsList[0];
                            double ry = paramsList[1];
                            double xAxisRotation = paramsList[2];
                            int largeArcFlag = (int)paramsList[3];
                            int sweepFlag = (int)paramsList[4];
                            var endPoint = new PointSvgModel(currentPoint.X + paramsList[5], currentPoint.Y + paramsList[6]);

                            points.AddRange(CalculateArcPoints(currentPoint, endPoint, rx, ry, xAxisRotation, largeArcFlag, sweepFlag));
                            currentPoint = endPoint; // Aggiorna il punto corrente
                        }
                        break;

                    case 'Q': // Bézier quadratica assoluta
                        {
                            for (int i = 0; i < paramsList.Count; i += 4)
                            {
                                var controlPoint = new PointSvgModel(paramsList[i], paramsList[i + 1]); // Punto di controllo assoluto
                                var endPoint = new PointSvgModel(paramsList[i + 2], paramsList[i + 3]); // Punto finale assoluto

                                points.AddRange(CalculateQuadraticBezierPoints(currentPoint, controlPoint, endPoint));
                                currentPoint = endPoint; // Aggiorna il punto corrente
                            }
                        }
                        break;

                    case 'q': // Bézier quadratica relativa
                        {
                            for (int i = 0; i < paramsList.Count; i += 4)
                            {
                                var controlPoint = new PointSvgModel(currentPoint.X + paramsList[i], currentPoint.Y + paramsList[i + 1]); // Relativo
                                var endPoint = new PointSvgModel(currentPoint.X + paramsList[i + 2], currentPoint.Y + paramsList[i + 3]); // Relativo

                                points.AddRange(CalculateQuadraticBezierPoints(currentPoint, controlPoint, endPoint));
                                currentPoint = endPoint; // Aggiorna il punto corrente
                            }
                        }
                        break;

                    case 'C': // Bézier cubica assoluta
                        {
                            for (int i = 0; i < paramsList.Count; i += 6)
                            {
                                var controlPoint1 = new PointSvgModel(paramsList[i], paramsList[i + 1]);
                                var controlPoint2 = new PointSvgModel(paramsList[i + 2], paramsList[i + 3]);
                                var endPoint = new PointSvgModel(paramsList[i + 4], paramsList[i + 5]);

                                points.AddRange(CalculateCubicBezierPoints(currentPoint, controlPoint1, controlPoint2, endPoint));
                                currentPoint = endPoint; // Aggiorna il punto corrente
                            }
                        }
                        break;

                    case 'c': // Bézier cubica relativa
                        {
                            for (int i = 0; i < paramsList.Count; i += 6)
                            {
                                var controlPoint1 = new PointSvgModel(currentPoint.X + paramsList[i], currentPoint.Y + paramsList[i + 1]);
                                var controlPoint2 = new PointSvgModel(currentPoint.X + paramsList[i + 2], currentPoint.Y + paramsList[i + 3]);
                                var endPoint = new PointSvgModel(currentPoint.X + paramsList[i + 4], currentPoint.Y + paramsList[i + 5]);

                                points.AddRange(CalculateCubicBezierPoints(currentPoint, controlPoint1, controlPoint2, endPoint));
                                currentPoint = endPoint; // Aggiorna il punto corrente
                            }
                        }
                        break;

                    default:
                        Console.WriteLine($"Comando non gestito: {type}");
                        break;
                }
            }

            return points;
        }
        private static List<PointSvgModel> CalculateQuadraticBezierPoints(
        PointSvgModel p0, PointSvgModel p1, PointSvgModel p2, int segments = 100)
        {
            var points = new List<PointSvgModel>();

            for (int i = 0; i <= segments; i++)
            {
                double t = (double)i / segments;

                // Formula della curva Bézier quadratica
                double x = Math.Pow(1 - t, 2) * p0.X +
                           2 * (1 - t) * t * p1.X +
                           Math.Pow(t, 2) * p2.X;

                double y = Math.Pow(1 - t, 2) * p0.Y +
                           2 * (1 - t) * t * p1.Y +
                           Math.Pow(t, 2) * p2.Y;

                points.Add(new PointSvgModel(x, y));
            }

            return points;
        }

        private static List<PointSvgModel> CalculateCubicBezierPoints(PointSvgModel p0, PointSvgModel p1, PointSvgModel p2, PointSvgModel p3, int segments = 100)
        {
            var points = new List<PointSvgModel>();
            for (int i = 0; i <= segments; i++)
            {
                double t = (double)i / segments;
                double x = Math.Pow(1 - t, 3) * p0.X +
                           3 * Math.Pow(1 - t, 2) * t * p1.X +
                           3 * (1 - t) * Math.Pow(t, 2) * p2.X +
                           Math.Pow(t, 3) * p3.X;
                double y = Math.Pow(1 - t, 3) * p0.Y +
                           3 * Math.Pow(1 - t, 2) * t * p1.Y +
                           3 * (1 - t) * Math.Pow(t, 2) * p2.Y +
                           Math.Pow(t, 3) * p3.Y;
                points.Add(new PointSvgModel(x, y));
            }
            return points;
        }


        private static List<(char Command, List<double> Parameters)> SplitCommands(string d)
        {
            // Divide i comandi e i relativi parametri
            var result = new List<(char Command, List<double> Parameters)>();
            var matches = System.Text.RegularExpressions.Regex.Matches(d, @"([a-zA-Z])([^a-zA-Z]*)");

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var command = match.Groups[1].Value[0]; // Il comando (M, L, etc.)
                var parameters = match.Groups[2].Value
                    .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries) // Divide i parametri
                    .Select(p => double.Parse(p, CultureInfo.InvariantCulture)) // Converte in double
                    .ToList();

                result.Add((command, parameters));
            }

            return result;
        }


        private static List<PointSvgModel> CalculateArcPoints(
        PointSvgModel start, PointSvgModel end, double rx, double ry,
        double xAxisRotation, int largeArcFlag, int sweepFlag)
        {
            var points = new List<PointSvgModel>();

            // Step 1: Conversione delle coordinate dell'arco secondo la specifica SVG
            double x1 = start.X;
            double y1 = start.Y;
            double x2 = end.X;
            double y2 = end.Y;

            // Rotazione dell'ellisse: Calcola le coordinate del punto iniziale
            double rad = Math.PI * xAxisRotation / 180.0; // Rotazione in radianti
            double dx = (x1 - x2) / 2.0;
            double dy = (y1 - y2) / 2.0;
            double x1Prime = Math.Cos(rad) * dx + Math.Sin(rad) * dy;
            double y1Prime = -Math.Sin(rad) * dx + Math.Cos(rad) * dy;

            // Assicurati che i raggi siano abbastanza grandi
            double rxSq = rx * rx;
            double rySq = ry * ry;
            double x1PrimeSq = x1Prime * x1Prime;
            double y1PrimeSq = y1Prime * y1Prime;
            double radiiCheck = (x1PrimeSq / rxSq) + (y1PrimeSq / rySq);
            if (radiiCheck > 1)
            {
                double scaleFactor = Math.Sqrt(radiiCheck);
                rx *= scaleFactor;
                ry *= scaleFactor;
                rxSq = rx * rx;
                rySq = ry * ry;
            }

            // Step 2: Calcolo del centro del cerchio
            double sign = (largeArcFlag != sweepFlag) ? 1 : -1;
            double sq = ((rxSq * rySq) - (rxSq * y1PrimeSq) - (rySq * x1PrimeSq)) /
                        ((rxSq * y1PrimeSq) + (rySq * x1PrimeSq));
            double coef = Math.Sqrt(Math.Max(sq, 0)) * sign;
            double cxPrime = coef * ((rx * y1Prime) / ry);
            double cyPrime = coef * -((ry * x1Prime) / rx);

            // Centro dell'ellisse
            double cx = Math.Cos(rad) * cxPrime - Math.Sin(rad) * cyPrime + (x1 + x2) / 2.0;
            double cy = Math.Sin(rad) * cxPrime + Math.Cos(rad) * cyPrime + (y1 + y2) / 2.0;

            // Step 3: Calcolo dell'angolo iniziale e dell'angolo del delta
            double theta1 = Math.Atan2((y1Prime - cyPrime) / ry, (x1Prime - cxPrime) / rx);
            double deltaTheta = Math.Atan2((-y1Prime - cyPrime) / ry, (-x1Prime - cxPrime) / rx) - theta1;
            if (sweepFlag == 0 && deltaTheta > 0) deltaTheta -= 2 * Math.PI;
            else if (sweepFlag == 1 && deltaTheta < 0) deltaTheta += 2 * Math.PI;

            // Step 4: Genera punti lungo l'arco
            int segments = 100; // Numero di punti lungo l'arco
            for (int i = 0; i <= segments; i++)
            {
                double t = (double)i / segments;
                double angle = theta1 + t * deltaTheta;
                double x = Math.Cos(rad) * rx * Math.Cos(angle) - Math.Sin(rad) * ry * Math.Sin(angle) + cx;
                double y = Math.Sin(rad) * rx * Math.Cos(angle) + Math.Cos(rad) * ry * Math.Sin(angle) + cy;
                points.Add(new PointSvgModel(x, y));
            }

            return points;
        }

        // Metodi per parsare diversi tipi di elementi SVG

        private List<PointSvgModel> ParseRect(XElement element)
        {
            var points = new List<PointSvgModel>();
            
            double x = ParseDoubleAttribute(element, "x", 0);
            double y = ParseDoubleAttribute(element, "y", 0);
            double width = ParseDoubleAttribute(element, "width", 0);
            double height = ParseDoubleAttribute(element, "height", 0);
            
            Console.WriteLine($"[SVG DEBUG] Rect: x={x}, y={y}, width={width}, height={height}");
            
            // Parse style
            var style = ParseElementStyle(element);
            Console.WriteLine($"[SVG DEBUG] Rect style: Fill={style.HasFill}, Stroke={style.HasStroke}");
            
            // Crea i punti del rettangolo in senso orario
            points.Add(new PointSvgModel(x, y));              // Top-left
            points.Add(new PointSvgModel(x + width, y));      // Top-right
            points.Add(new PointSvgModel(x + width, y + height)); // Bottom-right
            points.Add(new PointSvgModel(x, y + height));     // Bottom-left
            points.Add(new PointSvgModel(x, y));              // Close path
            
            return points;
        }

        private List<PointSvgModel> ParseCircle(XElement element)
        {
            var points = new List<PointSvgModel>();
            
            double cx = ParseDoubleAttribute(element, "cx", 0);
            double cy = ParseDoubleAttribute(element, "cy", 0);
            double r = ParseDoubleAttribute(element, "r", 0);
            
            Console.WriteLine($"[SVG DEBUG] Circle: cx={cx}, cy={cy}, r={r}");
            
            // Genera punti lungo la circonferenza
            int segments = 50;
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = cx + r * Math.Cos(angle);
                double y = cy + r * Math.Sin(angle);
                points.Add(new PointSvgModel(x, y));
            }
            
            return points;
        }

        private List<PointSvgModel> ParseEllipse(XElement element)
        {
            var points = new List<PointSvgModel>();
            
            double cx = ParseDoubleAttribute(element, "cx", 0);
            double cy = ParseDoubleAttribute(element, "cy", 0);
            double rx = ParseDoubleAttribute(element, "rx", 0);
            double ry = ParseDoubleAttribute(element, "ry", 0);
            
            Console.WriteLine($"[SVG DEBUG] Ellipse: cx={cx}, cy={cy}, rx={rx}, ry={ry}");
            
            // Genera punti lungo l'ellisse
            int segments = 50;
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = cx + rx * Math.Cos(angle);
                double y = cy + ry * Math.Sin(angle);
                points.Add(new PointSvgModel(x, y));
            }
            
            return points;
        }

        private List<PointSvgModel> ParseLine(XElement element)
        {
            var points = new List<PointSvgModel>();
            
            double x1 = ParseDoubleAttribute(element, "x1", 0);
            double y1 = ParseDoubleAttribute(element, "y1", 0);
            double x2 = ParseDoubleAttribute(element, "x2", 0);
            double y2 = ParseDoubleAttribute(element, "y2", 0);
            
            Console.WriteLine($"[SVG DEBUG] Line: x1={x1}, y1={y1}, x2={x2}, y2={y2}");
            
            points.Add(new PointSvgModel(x1, y1));
            points.Add(new PointSvgModel(x2, y2));
            
            return points;
        }

        private List<PointSvgModel> ParsePolygon(XElement element)
        {
            var points = new List<PointSvgModel>();
            
            string? pointsData = element.Attribute("points")?.Value;
            Console.WriteLine($"[SVG DEBUG] Polygon points: {pointsData}");
            
            if (pointsData != null)
            {
                points = ParsePointsList(pointsData);
                // Chiudi il poligono tornando al primo punto
                if (points.Count > 0)
                {
                    points.Add(points[0]);
                }
            }
            
            return points;
        }

        private List<PointSvgModel> ParsePolyline(XElement element)
        {
            var points = new List<PointSvgModel>();
            
            string? pointsData = element.Attribute("points")?.Value;
            Console.WriteLine($"[SVG DEBUG] Polyline points: {pointsData}");
            
            if (pointsData != null)
            {
                points = ParsePointsList(pointsData);
            }
            
            return points;
        }

        private List<PointSvgModel> ParseGroup(XElement element)
        {
            var points = new List<PointSvgModel>();
            
            Console.WriteLine($"[SVG DEBUG] Group con {element.Elements().Count()} elementi figli");
            
            // Parse la trasformazione del gruppo se presente
            Transform groupTransform = ParseTransform(element);
            Console.WriteLine($"[SVG DEBUG] Group transform: translate({groupTransform.TranslateX},{groupTransform.TranslateY}) scale({groupTransform.ScaleX},{groupTransform.ScaleY}) rotate({groupTransform.Rotation})");
            
            // Parse ricorsivo di tutti gli elementi del gruppo
            foreach (var child in element.Elements())
            {
                var childPoints = ParseElement(child);
                
                // Applica la trasformazione del gruppo ai punti figli
                var transformedPoints = ApplyTransform(childPoints, groupTransform);
                points.AddRange(transformedPoints);
            }
            
            return points;
        }

        private List<SvgElementData> ParseElementWithStyle(XElement element)
        {
            Console.WriteLine($"[SVG DEBUG] ParseElementWithStyle chiamato per: {element.Name.LocalName}");

            var elements = new List<SvgElementData>();
            var points = new List<PointSvgModel>();

            switch (element.Name.LocalName)
            {
                case "path":
                    string? pathData = element.Attribute("d")?.Value;
                    Console.WriteLine($"[SVG DEBUG] Path data trovato: {pathData}");
                    if (pathData != null)
                    {
                        points = ParsePath(pathData) ?? new List<PointSvgModel>();
                    }
                    break;

                case "rect":
                    points = ParseRect(element);
                    break;

                case "circle":
                    points = ParseCircle(element);
                    break;

                case "ellipse":
                    points = ParseEllipse(element);
                    break;

                case "line":
                    points = ParseLine(element);
                    break;

                case "polygon":
                    points = ParsePolygon(element);
                    break;

                case "polyline":
                    points = ParsePolyline(element);
                    break;

                case "g":
                    // Group element - parse children recursively
                    return ParseGroupWithStyle(element);

                default:
                    Console.WriteLine($"[SVG DEBUG] Elemento non gestito: {element.Name.LocalName}");
                    return null;
            }

            if (points.Count > 0)
            {
                var style = ParseElementStyle(element);
                var elementData = new SvgElementData
                {
                    Points = points,
                    Style = style,
                    ElementType = element.Name.LocalName
                };
                elements.Add(elementData);
            }

            return elements;
        }

        private List<SvgElementData>? ParseGroupWithStyle(XElement element)
        {
            var elements = new List<SvgElementData>();
            
            Console.WriteLine($"[SVG DEBUG] Group con {element.Elements().Count()} elementi figli");
            
            // Parse la trasformazione del gruppo se presente
            Transform groupTransform = ParseTransform(element);
            Console.WriteLine($"[SVG DEBUG] Group transform: translate({groupTransform.TranslateX},{groupTransform.TranslateY}) scale({groupTransform.ScaleX},{groupTransform.ScaleY}) rotate({groupTransform.Rotation})");
            
            // Parse ricorsivo di tutti gli elementi del gruppo
            foreach (var child in element.Elements())
            {
                var childElements = ParseElementWithStyle(child);
                if (childElements != null)
                {
                    foreach (var childElement in childElements)
                    {
                        // Applica la trasformazione del gruppo ai punti figli
                        var transformedPoints = ApplyTransform(childElement.Points, groupTransform);
                        
                        var transformedElement = new SvgElementData
                        {
                            Points = transformedPoints,
                            Style = childElement.Style,
                            ElementType = childElement.ElementType
                        };
                        
                        elements.Add(transformedElement);
                    }
                }
            }
            
            return elements;
        }

        private SvgStyle ParseElementStyle(XElement element)
        {
            var style = new SvgStyle();

            // Parse fill
            string? fillValue = element.Attribute("fill")?.Value;
            if (!string.IsNullOrEmpty(fillValue))
            {
                if (fillValue.ToLower() == "none")
                {
                    style.HasFill = false;
                    style.FillColor = Colors.Transparent;
                }
                else
                {
                    style.HasFill = true;
                    style.FillColor = ParseSvgColor(fillValue, Colors.Black);
                }
            }
            else
            {
                // Default SVG behavior: fill with black if not specified
                style.HasFill = true;
                style.FillColor = Colors.Black;
            }

            // Parse stroke
            string? strokeValue = element.Attribute("stroke")?.Value;
            if (!string.IsNullOrEmpty(strokeValue) && strokeValue.ToLower() != "none")
            {
                style.HasStroke = true;
                style.StrokeColor = ParseSvgColor(strokeValue, Colors.Black);
            }
            else
            {
                style.HasStroke = false;
                style.StrokeColor = Colors.Transparent;
            }

            // Parse stroke-width
            string? strokeWidthValue = element.Attribute("stroke-width")?.Value;
            style.StrokeWidth = ParseStrokeWidthValue(strokeWidthValue);

            Console.WriteLine($"[SVG DEBUG] Stile elemento - Fill: {style.HasFill} ({style.FillColor}), Stroke: {style.HasStroke} ({style.StrokeColor}, {style.StrokeWidth})");

            return style;
        }

        private Color ParseSvgColor(string colorValue, Color defaultColor)
        {
            if (string.IsNullOrEmpty(colorValue) || colorValue.ToLower() == "none")
            {
                return Colors.Transparent;
            }

            try
            {
                // Gestione colori esadecimali (#RGB o #RRGGBB)
                if (colorValue.StartsWith("#"))
                {
                    return ParseHexColor(colorValue);
                }

                // Gestione rgb() e rgba()
                if (colorValue.StartsWith("rgb"))
                {
                    return ParseRgbColor(colorValue);
                }

                // Gestione colori con nome
                return ParseNamedColor(colorValue, defaultColor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SVG DEBUG] Errore nel parsing del colore '{colorValue}': {ex.Message}");
                return defaultColor;
            }
        }

        private Color ParseHexColor(string hexColor)
        {
            hexColor = hexColor.TrimStart('#');
            
            if (hexColor.Length == 3)
            {
                // Formato #RGB -> #RRGGBB
                hexColor = string.Concat(hexColor.Select(c => $"{c}{c}"));
            }
            
            if (hexColor.Length == 6)
            {
                byte r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                return Color.FromRgb(r, g, b);
            }
            
            if (hexColor.Length == 8)
            {
                byte a = Convert.ToByte(hexColor.Substring(0, 2), 16);
                byte r = Convert.ToByte(hexColor.Substring(2, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(4, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(6, 2), 16);
                return Color.FromArgb(a, r, g, b);
            }

            throw new ArgumentException($"Formato colore esadecimale non valido: #{hexColor}");
        }

        private Color ParseRgbColor(string rgbColor)
        {
            var match = System.Text.RegularExpressions.Regex.Match(rgbColor, 
                @"rgba?\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+))?\s*\)");
            
            if (match.Success)
            {
                byte r = byte.Parse(match.Groups[1].Value);
                byte g = byte.Parse(match.Groups[2].Value);
                byte b = byte.Parse(match.Groups[3].Value);
                
                if (match.Groups[4].Success)
                {
                    // rgba con alpha
                    double alpha = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    byte a = (byte)(alpha * 255);
                    return Color.FromArgb(a, r, g, b);
                }
                else
                {
                    // rgb senza alpha
                    return Color.FromRgb(r, g, b);
                }
            }

            throw new ArgumentException($"Formato colore RGB non valido: {rgbColor}");
        }

        private Color ParseNamedColor(string colorName, Color defaultColor)
        {
            var namedColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
            {
                { "black", Colors.Black },
                { "white", Colors.White },
                { "red", Colors.Red },
                { "green", Colors.Green },
                { "blue", Colors.Blue },
                { "yellow", Colors.Yellow },
                { "cyan", Colors.Cyan },
                { "magenta", Colors.Magenta },
                { "orange", Colors.Orange },
                { "purple", Colors.Purple },
                { "pink", Colors.Pink },
                { "brown", Colors.Brown },
                { "gray", Colors.Gray },
                { "grey", Colors.Gray },
                { "darkred", Colors.DarkRed },
                { "darkgreen", Colors.DarkGreen },
                { "darkblue", Colors.DarkBlue },
                { "lightgray", Colors.LightGray },
                { "lightgrey", Colors.LightGray },
                { "transparent", Colors.Transparent }
            };

            if (namedColors.TryGetValue(colorName, out Color color))
            {
                return color;
            }

            Console.WriteLine($"[SVG DEBUG] Colore con nome sconosciuto: {colorName}, uso colore di default");
            return defaultColor;
        }

        private double ParseStrokeWidthValue(string? strokeWidthValue)
        {
            if (string.IsNullOrEmpty(strokeWidthValue))
            {
                return 1.0;
            }

            // Rimuovi unità di misura se presenti (px, pt, etc.)
            string numericValue = System.Text.RegularExpressions.Regex.Replace(strokeWidthValue, @"[a-zA-Z%]", "");
            
            if (double.TryParse(numericValue, CultureInfo.InvariantCulture, out double width))
            {
                return Math.Max(width, 0); // Assicura che non sia negativo
            }

            return 1.0; // Default
        }

        // Helper methods
        private double ParseDoubleAttribute(XElement element, string attributeName, double defaultValue)
        {
            string? value = element.Attribute(attributeName)?.Value;
            if (value != null && double.TryParse(value, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return defaultValue;
        }

        private List<PointSvgModel> ParsePointsList(string pointsData)
        {
            var points = new List<PointSvgModel>();
            
            // Split by commas and spaces, remove empty entries
            var coordinates = pointsData
                .Split(new[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            
            // Parse coordinates in pairs (x, y)
            for (int i = 0; i < coordinates.Length - 1; i += 2)
            {
                if (double.TryParse(coordinates[i], CultureInfo.InvariantCulture, out double x) &&
                    double.TryParse(coordinates[i + 1], CultureInfo.InvariantCulture, out double y))
                {
                    points.Add(new PointSvgModel(x, y));
                }
            }
            
            return points;
        }

        // Metodi per gestire le trasformazioni SVG
        
        private Transform ParseTransform(XElement element)
        {
            var transform = new Transform();
            string? transformAttr = element.Attribute("transform")?.Value;
            
            if (string.IsNullOrEmpty(transformAttr))
                return transform;
                
            Console.WriteLine($"[SVG DEBUG] Parsing transform: {transformAttr}");
            
            // Parse delle varie trasformazioni usando regex
            
            // translate(x,y) o translate(x)
            var translateMatch = Regex.Match(transformAttr, @"translate\s*\(\s*([^,\s]+)(?:\s*,\s*([^,\s]+))?\s*\)");
            if (translateMatch.Success)
            {
                if (double.TryParse(translateMatch.Groups[1].Value, CultureInfo.InvariantCulture, out double tx))
                {
                    transform.TranslateX = tx;
                }
                if (translateMatch.Groups[2].Success && 
                    double.TryParse(translateMatch.Groups[2].Value, CultureInfo.InvariantCulture, out double ty))
                {
                    transform.TranslateY = ty;
                }
                Console.WriteLine($"[SVG DEBUG] Found translate: {transform.TranslateX}, {transform.TranslateY}");
            }
            
            // scale(x,y) o scale(x)
            var scaleMatch = Regex.Match(transformAttr, @"scale\s*\(\s*([^,\s]+)(?:\s*,\s*([^,\s]+))?\s*\)");
            if (scaleMatch.Success)
            {
                if (double.TryParse(scaleMatch.Groups[1].Value, CultureInfo.InvariantCulture, out double sx))
                {
                    transform.ScaleX = sx;
                    transform.ScaleY = sx; // Se non specificato Y, usa lo stesso valore di X
                }
                if (scaleMatch.Groups[2].Success && 
                    double.TryParse(scaleMatch.Groups[2].Value, CultureInfo.InvariantCulture, out double sy))
                {
                    transform.ScaleY = sy;
                }
                Console.WriteLine($"[SVG DEBUG] Found scale: {transform.ScaleX}, {transform.ScaleY}");
            }
            
            // rotate(angle) o rotate(angle,cx,cy)
            var rotateMatch = Regex.Match(transformAttr, @"rotate\s*\(\s*([^,\s]+)(?:\s*,\s*([^,\s]+)\s*,\s*([^,\s]+))?\s*\)");
            if (rotateMatch.Success)
            {
                if (double.TryParse(rotateMatch.Groups[1].Value, CultureInfo.InvariantCulture, out double angle))
                {
                    transform.Rotation = angle;
                }
                if (rotateMatch.Groups[2].Success && rotateMatch.Groups[3].Success)
                {
                    if (double.TryParse(rotateMatch.Groups[2].Value, CultureInfo.InvariantCulture, out double cx) &&
                        double.TryParse(rotateMatch.Groups[3].Value, CultureInfo.InvariantCulture, out double cy))
                    {
                        transform.RotationCenterX = cx;
                        transform.RotationCenterY = cy;
                    }
                }
                Console.WriteLine($"[SVG DEBUG] Found rotate: {transform.Rotation}° around ({transform.RotationCenterX}, {transform.RotationCenterY})");
            }
            
            return transform;
        }
        
        private List<PointSvgModel> ApplyTransform(List<PointSvgModel> points, Transform transform)
        {
            var transformedPoints = new List<PointSvgModel>();
            
            foreach (var point in points)
            {
                var transformedPoint = ApplyTransformToPoint(point, transform);
                transformedPoints.Add(transformedPoint);
            }
            
            return transformedPoints;
        }
        
        private PointSvgModel ApplyTransformToPoint(PointSvgModel point, Transform transform)
        {
            double x = point.X;
            double y = point.Y;
            
            // 1. Applica scaling
            x *= transform.ScaleX;
            y *= transform.ScaleY;
            
            // 2. Applica rotazione se necessaria
            if (Math.Abs(transform.Rotation) > 0.001) // Evita calcoli inutili per rotazioni nulle
            {
                double radians = transform.Rotation * Math.PI / 180.0;
                double cos = Math.Cos(radians);
                double sin = Math.Sin(radians);
                
                // Sposta al centro di rotazione
                double tempX = x - transform.RotationCenterX;
                double tempY = y - transform.RotationCenterY;
                
                // Applica rotazione
                double rotatedX = tempX * cos - tempY * sin;
                double rotatedY = tempX * sin + tempY * cos;
                
                // Riposiziona
                x = rotatedX + transform.RotationCenterX;
                y = rotatedY + transform.RotationCenterY;
            }
            
            // 3. Applica traslazione
            x += transform.TranslateX;
            y += transform.TranslateY;
            
            return new PointSvgModel(x, y);
        }

    }
}
