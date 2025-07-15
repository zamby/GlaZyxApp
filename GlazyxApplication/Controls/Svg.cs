using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;

namespace GlazyxApplication
{
    public class Svg : DrawObj
    {
        private List<SvgElement> elements = new List<SvgElement>();
        
        public class SvgElement
        {
            public List<Point> Points { get; set; } = new List<Point>();
            public Color FillColor { get; set; } = Colors.Black;
            public Color StrokeColor { get; set; } = Colors.Transparent;
            public double StrokeWidth { get; set; } = 1.0;
            public string ElementType { get; set; } = "path";
            public bool HasFill { get; set; } = true;
            public bool HasStroke { get; set; } = false;
        }

        public Svg(string svgFilePath, double scale = 1.0)
        {
            Name = "Svg";
            LoadSvgFile(svgFilePath, scale);
        }

        public Svg(string svgContent, bool isContent, double scale = 1.0)
        {
            Name = "Svg";
            if (isContent)
            {
                LoadSvgContent(svgContent, scale);
            }
            else
            {
                LoadSvgFile(svgContent, scale);
            }
        }

        private void LoadSvgFile(string filePath, double scale)
        {
            try
            {
                Console.WriteLine($"[SVG DEBUG] Attempting to load file: {filePath}");
                Console.WriteLine($"[SVG DEBUG] File exists: {File.Exists(filePath)}");
                
                if (File.Exists(filePath))
                {
                    // Load SVG file and extract colors
                    LoadSvgWithBasicColors(filePath, scale);
                }
                else
                {
                    Console.WriteLine($"[SVG DEBUG] File not found: {filePath}");
                    // Don't create default shape, leave empty
                    elements.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SVG DEBUG] Error loading SVG: {ex.Message}");
                Console.WriteLine($"[SVG DEBUG] Stack trace: {ex.StackTrace}");
                // Don't create default shape, leave empty
                elements.Clear();
            }
        }

        private void LoadSvgWithBasicColors(string filePath, double scale)
        {
            try
            {
                Console.WriteLine($"[SVG DEBUG] Attempting to load SVG with colors: {filePath}");
                
                var svgService = new SvgService();
                var svgElements = svgService.ParseSvgWithStyles(filePath);
                
                Console.WriteLine($"[SVG DEBUG] ParseSvgWithStyles returned {svgElements.Count} elements");
                
                if (svgElements.Count > 0)
                {
                    ProcessSvgElementsWithStyle(svgElements, scale);
                    Console.WriteLine($"[SVG DEBUG] Used advanced parser with colors!");
                }
                else
                {
                    Console.WriteLine($"[SVG DEBUG] FALLBACK: No styled elements found, using base parser WITH COLOR ATTEMPT");
                    // Enhanced fallback: try to extract at least basic colors
                    ProcessSvgWithBasicColorExtraction(filePath, scale);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SVG DEBUG] ERROR in parsing with colors: {ex.Message}");
                Console.WriteLine($"[SVG DEBUG] Stack trace: {ex.StackTrace}");
                // Fallback with basic color extraction attempt
                ProcessSvgWithBasicColorExtraction(filePath, scale);
            }
        }

        private void LoadSvgContent(string svgContent, double scale)
        {
            try
            {
                // Salva temporaneamente il contenuto SVG in un file temp
                var tempFile = System.IO.Path.GetTempFileName() + ".svg";
                File.WriteAllText(tempFile, svgContent);
                
                var svgService = new SvgService();
                var points = svgService.ParseSvg(tempFile);
                ProcessSvgPoints(points, scale);
                
                // Pulisci il file temporaneo
                File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing SVG content: {ex.Message}");
                // Non creare una forma di default, lascia vuoto
                elements.Clear();
            }
        }

        private void ProcessSvgPoints(List<SvgService.PointSvgModel> svgPoints, double scale)
        {
            Console.WriteLine($"[SVG DEBUG] ProcessSvgPoints called with {svgPoints.Count} points, scale: {scale}");
            
            if (svgPoints.Count == 0)
            {
                Console.WriteLine($"[SVG DEBUG] No SVG points found, empty SVG");
                // Don't create default shape, leave empty
                elements.Clear();
                return;
            }

            // Convert SVG points to Avalonia points
            var points = svgPoints.Select(p => new Point(p.X * scale, p.Y * scale)).ToList();
            
            Console.WriteLine($"[SVG DEBUG] Converted {points.Count} Avalonia points");
            Console.WriteLine($"[SVG DEBUG] First converted points:");
            foreach (var point in points.Take(3))
            {
                Console.WriteLine($"[SVG DEBUG]   {point}");
            }
            
            // Create SVG element with all points
            var element = new SvgElement
            {
                Points = points,
                FillColor = ColorSolid,
                ElementType = "path"
            };
            elements.Add(element);

            // Calcola i bounds
            if (points.Count > 0)
            {
                double minX = points.Min(p => p.X);
                double minY = points.Min(p => p.Y);
                double maxX = points.Max(p => p.X);
                double maxY = points.Max(p => p.Y);

                Bounds = new Rect(minX, minY, maxX - minX, maxY - minY);
            }
        }

        private void ProcessSvgElementsWithStyle(List<SvgService.SvgElementData> svgElements, double scale)
        {
            Console.WriteLine($"[SVG DEBUG] ProcessSvgElementsWithStyle called with {svgElements.Count} elements, scale: {scale}");
            
            if (svgElements.Count == 0)
            {
                Console.WriteLine($"[SVG DEBUG] No SVG elements found, empty SVG");
                // Don't create default shape, leave empty
                elements.Clear();
                return;
            }

            foreach (var svgElement in svgElements)
            {
                // Convert SVG points to Avalonia points
                var points = svgElement.Points.Select(p => new Point(p.X * scale, p.Y * scale)).ToList();
                
                Console.WriteLine($"[SVG DEBUG] Converted {points.Count} points for element {svgElement.ElementType}");
                
                // Create SVG element with all points and original colors
                var element = new SvgElement
                {
                    Points = points,
                    FillColor = svgElement.Style.FillColor,
                    StrokeColor = svgElement.Style.StrokeColor,
                    StrokeWidth = svgElement.Style.StrokeWidth,
                    HasFill = svgElement.Style.HasFill,
                    HasStroke = svgElement.Style.HasStroke,
                    ElementType = svgElement.ElementType
                };
                elements.Add(element);
                
                Console.WriteLine($"[SVG DEBUG] Element added - Fill: {element.HasFill} ({element.FillColor}), Stroke: {element.HasStroke} ({element.StrokeColor})");
            }

            // Calculate bounds based on all elements
            if (elements.Count > 0)
            {
                var allPoints = elements.SelectMany(e => e.Points).ToList();
                if (allPoints.Count > 0)
                {
                    double minX = allPoints.Min(p => p.X);
                    double minY = allPoints.Min(p => p.Y);
                    double maxX = allPoints.Max(p => p.X);
                    double maxY = allPoints.Max(p => p.Y);

                    Bounds = new Rect(minX, minY, maxX - minX, maxY - minY);
                    Console.WriteLine($"[SVG DEBUG] Calculated bounds: {Bounds}");
                }
            }
        }

        public void SetColor(Color color)
        {
            ColorSolid = color;
            foreach (var element in elements)
            {
                element.FillColor = color;
            }
        }

        public override void Render(DrawingContext context)
        {
            Console.WriteLine($"[SVG RENDER DEBUG] Rendering Svg object: {Name} at position {Position} with {elements.Count} elements");
            
            foreach (var element in elements)
            {
                Console.WriteLine($"[SVG RENDER DEBUG] Rendering element {element.ElementType}:");
                Console.WriteLine($"[SVG RENDER DEBUG]   HasFill: {element.HasFill}, FillColor: {element.FillColor}");
                Console.WriteLine($"[SVG RENDER DEBUG]   HasStroke: {element.HasStroke}, StrokeColor: {element.StrokeColor}, Width: {element.StrokeWidth}");
                Console.WriteLine($"[SVG RENDER DEBUG]   Points count: {element.Points.Count}");
                
                if (element.Points.Count < 2) 
                {
                    Console.WriteLine($"[SVG RENDER DEBUG]   SKIP: Not enough points");
                    continue;
                }

                var geometry = new StreamGeometry();
                using (var geometryContext = geometry.Open())
                {
                    var firstPoint = new Point(
                        element.Points[0].X + Position.X,
                        element.Points[0].Y + Position.Y
                    );

                    geometryContext.BeginFigure(firstPoint, isFilled: element.HasFill);

                    foreach (var point in element.Points.Skip(1))
                    {
                        var transformedPoint = new Point(
                            point.X + Position.X,
                            point.Y + Position.Y
                        );
                        geometryContext.LineTo(transformedPoint);
                    }

                    geometryContext.EndFigure(true);
                }

                // Crea i brush per fill e stroke
                IBrush? fillBrush = null;
                IPen? strokePen = null;

                if (element.HasFill)
                {
                    fillBrush = new SolidColorBrush(element.FillColor);
                    Console.WriteLine($"[SVG RENDER DEBUG]   Created fillBrush with color: {element.FillColor}");
                }

                if (element.HasStroke)
                {
                    strokePen = new Pen(new SolidColorBrush(element.StrokeColor), element.StrokeWidth);
                    Console.WriteLine($"[SVG RENDER DEBUG]   Created strokePen with color: {element.StrokeColor}, width: {element.StrokeWidth}");
                }

                // Disegna la geometria
                Console.WriteLine($"[SVG RENDER DEBUG]   Drawing geometry with fillBrush={fillBrush?.ToString() ?? "null"}, strokePen={strokePen?.ToString() ?? "null"}");
                context.DrawGeometry(fillBrush, strokePen, geometry);
            }
        }

        // Metodi ausiliari per creare punti geometrici (usati dal fallback)
        private List<Point> CreateRectanglePoints(System.Xml.Linq.XElement element, double scale)
        {
            var points = new List<Point>();
            
            double x = ParseDoubleAttribute(element, "x", 0) * scale;
            double y = ParseDoubleAttribute(element, "y", 0) * scale;
            double width = ParseDoubleAttribute(element, "width", 0) * scale;
            double height = ParseDoubleAttribute(element, "height", 0) * scale;
            
            Console.WriteLine($"[SVG DEBUG] CreateRectanglePoints: x={x}, y={y}, width={width}, height={height}");
            
            // Crea i punti del rettangolo in senso orario
            points.Add(new Point(x, y));              // Top-left
            points.Add(new Point(x + width, y));      // Top-right
            points.Add(new Point(x + width, y + height)); // Bottom-right
            points.Add(new Point(x, y + height));     // Bottom-left
            points.Add(new Point(x, y));              // Close path
            
            return points;
        }

        private List<Point> CreateCirclePoints(System.Xml.Linq.XElement element, double scale)
        {
            var points = new List<Point>();
            
            double cx = ParseDoubleAttribute(element, "cx", 0) * scale;
            double cy = ParseDoubleAttribute(element, "cy", 0) * scale;
            double r = ParseDoubleAttribute(element, "r", 0) * scale;
            
            Console.WriteLine($"[SVG DEBUG] CreateCirclePoints: cx={cx}, cy={cy}, r={r}");
            
            // Genera punti lungo la circonferenza
            int segments = 50;
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = cx + r * Math.Cos(angle);
                double y = cy + r * Math.Sin(angle);
                points.Add(new Point(x, y));
            }
            
            return points;
        }

        private List<Point> CreateEllipsePoints(System.Xml.Linq.XElement element, double scale)
        {
            var points = new List<Point>();
            
            double cx = ParseDoubleAttribute(element, "cx", 0) * scale;
            double cy = ParseDoubleAttribute(element, "cy", 0) * scale;
            double rx = ParseDoubleAttribute(element, "rx", 0) * scale;
            double ry = ParseDoubleAttribute(element, "ry", 0) * scale;
            
            Console.WriteLine($"[SVG DEBUG] CreateEllipsePoints: cx={cx}, cy={cy}, rx={rx}, ry={ry}");
            
            // Genera punti lungo l'ellisse
            int segments = 50;
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = cx + rx * Math.Cos(angle);
                double y = cy + ry * Math.Sin(angle);
                points.Add(new Point(x, y));
            }
            
            return points;
        }

        // Metodi per il parsing dei colori SVG
        private Color ParseElementColor(string? colorValue, Color defaultColor)
        {
            Console.WriteLine($"[SVG COLOR DEBUG] ParseElementColor chiamato con: '{colorValue}'");
            
            if (string.IsNullOrEmpty(colorValue) || colorValue.ToLower() == "none")
            {
                Console.WriteLine($"[SVG COLOR DEBUG] Colore vuoto o 'none', ritorno trasparente");
                return Colors.Transparent;
            }

            try
            {
                // Gestione colori esadecimali (#RGB o #RRGGBB)
                if (colorValue.StartsWith("#"))
                {
                    Console.WriteLine($"[SVG COLOR DEBUG] Colore esadecimale rilevato: {colorValue}");
                    var result = ParseHexColor(colorValue);
                    Console.WriteLine($"[SVG COLOR DEBUG] Colore esadecimale parsato: {result}");
                    return result;
                }

                // Gestione rgb() e rgba()
                if (colorValue.StartsWith("rgb"))
                {
                    Console.WriteLine($"[SVG COLOR DEBUG] Colore RGB rilevato: {colorValue}");
                    var result = ParseRgbColor(colorValue);
                    Console.WriteLine($"[SVG COLOR DEBUG] Colore RGB parsato: {result}");
                    return result;
                }

                // Gestione colori con nome
                Console.WriteLine($"[SVG COLOR DEBUG] Colore con nome rilevato: {colorValue}");
                var namedResult = ParseNamedColor(colorValue, defaultColor);
                Console.WriteLine($"[SVG COLOR DEBUG] Colore con nome parsato: {namedResult}");
                return namedResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SVG COLOR DEBUG] ERRORE nel parsing del colore '{colorValue}': {ex.Message}");
                Console.WriteLine($"[SVG COLOR DEBUG] Ritorno colore di default: {defaultColor}");
                return defaultColor;
            }
        }

        private Color ParseHexColor(string hexColor)
        {
            Console.WriteLine($"[SVG HEX DEBUG] ParseHexColor input: '{hexColor}'");
            
            hexColor = hexColor.TrimStart('#');
            Console.WriteLine($"[SVG HEX DEBUG] Dopo rimozione #: '{hexColor}'");
            
            if (hexColor.Length == 3)
            {
                // Formato #RGB -> #RRGGBB
                hexColor = string.Concat(hexColor.Select(c => $"{c}{c}"));
                Console.WriteLine($"[SVG HEX DEBUG] Espanso da 3 a 6 caratteri: '{hexColor}'");
            }
            
            if (hexColor.Length == 6)
            {
                byte r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                var result = Color.FromRgb(r, g, b);
                Console.WriteLine($"[SVG HEX DEBUG] Colore RGB parsato: R={r}, G={g}, B={b} -> {result}");
                return result;
            }
            
            if (hexColor.Length == 8)
            {
                byte a = Convert.ToByte(hexColor.Substring(0, 2), 16);
                byte r = Convert.ToByte(hexColor.Substring(2, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(4, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(6, 2), 16);
                var result = Color.FromArgb(a, r, g, b);
                Console.WriteLine($"[SVG HEX DEBUG] Colore ARGB parsato: A={a}, R={r}, G={g}, B={b} -> {result}");
                return result;
            }

            Console.WriteLine($"[SVG HEX DEBUG] ERRORE: Lunghezza non valida: {hexColor.Length}");
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

        private double ParseStrokeWidth(string? strokeWidthValue)
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

        private double ParseDoubleAttribute(System.Xml.Linq.XElement element, string attributeName, double defaultValue)
        {
            string? value = element.Attribute(attributeName)?.Value;
            if (value != null && double.TryParse(value, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return defaultValue;
        }

        private void ProcessSvgWithBasicColorExtraction(string filePath, double scale)
        {
            try
            {
                Console.WriteLine($"[SVG DEBUG] ProcessSvgWithBasicColorExtraction per: {filePath}");
                
                // Carica il documento SVG direttamente
                var doc = System.Xml.Linq.XDocument.Load(filePath);
                var svgElements = doc.Descendants().Where(e => 
                    e.Name.LocalName == "rect" || 
                    e.Name.LocalName == "circle" || 
                    e.Name.LocalName == "ellipse" ||
                    e.Name.LocalName == "path" ||
                    e.Name.LocalName == "line" ||
                    e.Name.LocalName == "polygon" ||
                    e.Name.LocalName == "polyline").ToList();

                Console.WriteLine($"[SVG DEBUG] Trovati {svgElements.Count} elementi SVG per estrazione colori di base");

                if (svgElements.Count > 0)
                {
                    // Per ogni elemento, estrai geometria e colori separatamente
                    foreach (var element in svgElements)
                    {
                        var svgElement = CreateSvgElementWithBasicColors(element, scale);
                        if (svgElement != null)
                        {
                            elements.Add(svgElement);
                            Console.WriteLine($"[SVG DEBUG] Aggiunto elemento {element.Name.LocalName} con fill={svgElement.FillColor}, stroke={svgElement.StrokeColor}");
                        }
                    }

                    // Calcola i bounds
                    if (elements.Count > 0)
                    {
                        var allPoints = elements.SelectMany(e => e.Points).ToList();
                        if (allPoints.Count > 0)
                        {
                            double minX = allPoints.Min(p => p.X);
                            double minY = allPoints.Min(p => p.Y);
                            double maxX = allPoints.Max(p => p.X);
                            double maxY = allPoints.Max(p => p.Y);

                            Bounds = new Rect(minX, minY, maxX - minX, maxY - minY);
                        }
                    }
                    
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SVG DEBUG] Errore in ProcessSvgWithBasicColorExtraction: {ex.Message}");
            }

            // Ultimo fallback: usa il parser normale ma con colore nero invece di ColorSolid casuale
            Console.WriteLine($"[SVG DEBUG] ULTIMO FALLBACK: Parser normale con colore nero di default");
            var svgService = new SvgService();
            var points = svgService.ParseSvg(filePath);
            ProcessSvgPointsWithDefaultColor(points ?? new List<SvgService.PointSvgModel>(), scale, Colors.Black);
        }

        private SvgElement? CreateSvgElementWithBasicColors(System.Xml.Linq.XElement element, double scale)
        {
            var points = new List<Point>();
            
            // Estrai i punti in base al tipo di elemento usando i metodi helper esistenti
            switch (element.Name.LocalName)
            {
                case "rect":
                    points = CreateRectanglePoints(element, scale);
                    break;
                case "circle":
                    points = CreateCirclePoints(element, scale);
                    break;
                case "ellipse":
                    points = CreateEllipsePoints(element, scale);
                    break;
                case "line":
                    // Le linee richiedono solo stroke, non fill
                    double x1 = ParseDoubleAttribute(element, "x1", 0) * scale;
                    double y1 = ParseDoubleAttribute(element, "y1", 0) * scale;
                    double x2 = ParseDoubleAttribute(element, "x2", 0) * scale;
                    double y2 = ParseDoubleAttribute(element, "y2", 0) * scale;
                    points.Add(new Point(x1, y1));
                    points.Add(new Point(x2, y2));
                    break;
                default:
                    // Per path, polygon, polyline usa il parser normale e poi aggiungi colori
                    Console.WriteLine($"[SVG DEBUG] Elemento {element.Name.LocalName} richiede parser complesso, uso fallback normale");
                    return null;
            }

            if (points.Count == 0) return null;

            // Estrai i colori usando i metodi esistenti
            var fillColor = ParseElementColor(element.Attribute("fill")?.Value, Colors.Black);
            var strokeColor = ParseElementColor(element.Attribute("stroke")?.Value, Colors.Transparent);
            var strokeWidth = ParseStrokeWidth(element.Attribute("stroke-width")?.Value);
            
            // Gestisci fill="none" e stroke="none"
            bool hasFill = true;
            bool hasStroke = false;
            
            string? fillValue = element.Attribute("fill")?.Value;
            if (!string.IsNullOrEmpty(fillValue) && fillValue.ToLower() == "none")
            {
                hasFill = false;
                fillColor = Colors.Transparent;
            }
            
            string? strokeValue = element.Attribute("stroke")?.Value;
            if (!string.IsNullOrEmpty(strokeValue) && strokeValue.ToLower() != "none")
            {
                hasStroke = true;
            }
            
            // Per le linee, forza solo stroke
            if (element.Name.LocalName == "line")
            {
                hasFill = false;
                fillColor = Colors.Transparent;
                if (string.IsNullOrEmpty(strokeValue))
                {
                    hasStroke = true;
                    strokeColor = Colors.Black; // Default per linee
                }
            }

            Console.WriteLine($"[SVG DEBUG] Elemento {element.Name.LocalName} - Fill: {hasFill} ({fillColor}), Stroke: {hasStroke} ({strokeColor}, {strokeWidth})");

            return new SvgElement
            {
                Points = points,
                FillColor = fillColor,
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                HasFill = hasFill,
                HasStroke = hasStroke,
                ElementType = element.Name.LocalName
            };
        }

        private void ProcessSvgPointsWithDefaultColor(List<SvgService.PointSvgModel> svgPoints, double scale, Color defaultColor)
        {
            Console.WriteLine($"[SVG DEBUG] ProcessSvgPointsWithDefaultColor chiamato con {svgPoints.Count} punti, scala: {scale}, colore: {defaultColor}");
            
            if (svgPoints.Count == 0)
            {
                Console.WriteLine($"[SVG DEBUG] Nessun punto SVG trovato, SVG vuoto");
                // Non creare una forma di default, lascia vuoto
                elements.Clear();
                return;
            }

            // Converte i punti SVG in punti Avalonia
            var points = svgPoints.Select(p => new Point(p.X * scale, p.Y * scale)).ToList();
            
            Console.WriteLine($"[SVG DEBUG] Convertiti {points.Count} punti Avalonia con colore di default {defaultColor}");
            
            // Crea un elemento SVG con tutti i punti e il colore specificato
            var element = new SvgElement
            {
                Points = points,
                FillColor = defaultColor,
                ElementType = "path",
                HasFill = true,
                HasStroke = false
            };
            elements.Add(element);

            // Calcola i bounds
            if (points.Count > 0)
            {
                double minX = points.Min(p => p.X);
                double minY = points.Min(p => p.Y);
                double maxX = points.Max(p => p.X);
                double maxY = points.Max(p => p.Y);

                Bounds = new Rect(minX, minY, maxX - minX, maxY - minY);
            }
        }
    }
}
