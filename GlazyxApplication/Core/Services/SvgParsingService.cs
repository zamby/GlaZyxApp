using GlazyxApplication.Core.Interfaces;
using GlazyxApplication.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GlazyxApplication.Core.Services
{
    /// <summary>
    /// Service for parsing SVG files and converting them to geometric data
    /// </summary>
    public class SvgParsingService : ISvgParsingService
    {
        public List<Point2D> ParseSvgToPoints(string filePath)
        {
            if (!IsValidSvgFile(filePath))
                throw new ArgumentException($"Invalid SVG file: {filePath}");

            var elements = ParseSvgWithStyles(filePath);
            var points = new List<Point2D>();

            foreach (var element in elements)
            {
                points.AddRange(element.Points);
            }

            return points;
        }

        public List<SvgElementData> ParseSvgWithStyles(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"SVG file not found: {filePath}");

            try
            {
                XDocument svgDoc = XDocument.Load(filePath);
                XElement? root = svgDoc.Root;
                var elements = new List<SvgElementData>();

                if (root != null)
                {
                    foreach (XElement element in root.Elements())
                    {
                        var parsed = ParseElementWithStyle(element);
                        if (parsed != null)
                        {
                            elements.AddRange(parsed);
                        }
                    }
                }

                return elements;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse SVG file: {ex.Message}", ex);
            }
        }

        public List<Point2D> ParseSvgPath(string pathData)
        {
            if (string.IsNullOrWhiteSpace(pathData))
                return new List<Point2D>();

            try
            {
                return ParsePath(pathData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse SVG path data: {ex.Message}", ex);
            }
        }

        public bool IsValidSvgFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                XDocument doc = XDocument.Load(filePath);
                return doc.Root?.Name.LocalName == "svg";
            }
            catch
            {
                return false;
            }
        }

        #region Private Implementation

        private List<SvgElementData> ParseElementWithStyle(XElement element)
        {
            var elements = new List<SvgElementData>();
            var points = new List<Point2D>();

            switch (element.Name.LocalName.ToLowerInvariant())
            {
                case "path":
                    string? pathData = element.Attribute("d")?.Value;
                    if (pathData != null)
                    {
                        points = ParsePath(pathData);
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
                    return ParseGroupWithStyle(element) ?? new List<SvgElementData>();

                default:
                    return elements; // Unknown element type
            }

            if (points.Any())
            {
                var style = ParseElementStyle(element);
                var transform = ParseTransform(element);
                var transformedPoints = ApplyTransform(points, transform);

                elements.Add(new SvgElementData
                {
                    Points = transformedPoints,
                    Style = style,
                    ElementType = element.Name.LocalName
                });
            }

            return elements;
        }

        private List<Point2D> ParsePath(string pathData)
        {
            var points = new List<Point2D>();
            var commands = SplitCommands(pathData);
            var currentPoint = Point2D.Zero;
            var startPoint = Point2D.Zero;

            foreach (var cmd in commands)
            {
                var type = cmd.Command;
                var paramsList = cmd.Parameters;

                switch (type)
                {
                    case 'M': // Move to (absolute)
                        currentPoint = new Point2D(paramsList[0], paramsList[1]);
                        startPoint = currentPoint;
                        points.Add(currentPoint);
                        for (int i = 2; i < paramsList.Count; i += 2)
                        {
                            currentPoint = new Point2D(paramsList[i], paramsList[i + 1]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'm': // Move to (relative)
                        currentPoint = new Point2D(currentPoint.X + paramsList[0], currentPoint.Y + paramsList[1]);
                        startPoint = currentPoint;
                        points.Add(currentPoint);
                        for (int i = 2; i < paramsList.Count; i += 2)
                        {
                            currentPoint = new Point2D(currentPoint.X + paramsList[i], currentPoint.Y + paramsList[i + 1]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'L': // Line to (absolute)
                        for (int i = 0; i < paramsList.Count; i += 2)
                        {
                            currentPoint = new Point2D(paramsList[i], paramsList[i + 1]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'l': // Line to (relative)
                        for (int i = 0; i < paramsList.Count; i += 2)
                        {
                            currentPoint = new Point2D(currentPoint.X + paramsList[i], currentPoint.Y + paramsList[i + 1]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'H': // Horizontal line (absolute)
                        for (int i = 0; i < paramsList.Count; i++)
                        {
                            currentPoint = new Point2D(paramsList[i], currentPoint.Y);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'h': // Horizontal line (relative)
                        for (int i = 0; i < paramsList.Count; i++)
                        {
                            currentPoint = new Point2D(currentPoint.X + paramsList[i], currentPoint.Y);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'V': // Vertical line (absolute)
                        for (int i = 0; i < paramsList.Count; i++)
                        {
                            currentPoint = new Point2D(currentPoint.X, paramsList[i]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'v': // Vertical line (relative)
                        for (int i = 0; i < paramsList.Count; i++)
                        {
                            currentPoint = new Point2D(currentPoint.X, currentPoint.Y + paramsList[i]);
                            points.Add(currentPoint);
                        }
                        break;

                    case 'Z': // Close path
                    case 'z':
                        currentPoint = startPoint;
                        points.Add(currentPoint);
                        break;

                    // Note: Bezier curves and arcs would be implemented here
                    // For now, we'll skip them to keep the core service focused
                }
            }

            return points;
        }

        private List<Point2D> ParseRect(XElement element)
        {
            double x = ParseDoubleAttribute(element, "x", 0);
            double y = ParseDoubleAttribute(element, "y", 0);
            double width = ParseDoubleAttribute(element, "width", 0);
            double height = ParseDoubleAttribute(element, "height", 0);

            return new List<Point2D>
            {
                new Point2D(x, y),              // Top-left
                new Point2D(x + width, y),      // Top-right
                new Point2D(x + width, y + height), // Bottom-right
                new Point2D(x, y + height),     // Bottom-left
                new Point2D(x, y)               // Close path
            };
        }

        private List<Point2D> ParseCircle(XElement element)
        {
            double cx = ParseDoubleAttribute(element, "cx", 0);
            double cy = ParseDoubleAttribute(element, "cy", 0);
            double r = ParseDoubleAttribute(element, "r", 0);

            var points = new List<Point2D>();
            int segments = 50;
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = cx + r * Math.Cos(angle);
                double y = cy + r * Math.Sin(angle);
                points.Add(new Point2D(x, y));
            }

            return points;
        }

        private List<Point2D> ParseEllipse(XElement element)
        {
            double cx = ParseDoubleAttribute(element, "cx", 0);
            double cy = ParseDoubleAttribute(element, "cy", 0);
            double rx = ParseDoubleAttribute(element, "rx", 0);
            double ry = ParseDoubleAttribute(element, "ry", 0);

            var points = new List<Point2D>();
            int segments = 50;
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = cx + rx * Math.Cos(angle);
                double y = cy + ry * Math.Sin(angle);
                points.Add(new Point2D(x, y));
            }

            return points;
        }

        private List<Point2D> ParseLine(XElement element)
        {
            double x1 = ParseDoubleAttribute(element, "x1", 0);
            double y1 = ParseDoubleAttribute(element, "y1", 0);
            double x2 = ParseDoubleAttribute(element, "x2", 0);
            double y2 = ParseDoubleAttribute(element, "y2", 0);

            return new List<Point2D>
            {
                new Point2D(x1, y1),
                new Point2D(x2, y2)
            };
        }

        private List<Point2D> ParsePolygon(XElement element)
        {
            string? pointsData = element.Attribute("points")?.Value;
            if (pointsData == null) return new List<Point2D>();

            var points = ParsePointsList(pointsData);
            // Close the polygon
            if (points.Count > 0)
            {
                points.Add(points[0]);
            }
            return points;
        }

        private List<Point2D> ParsePolyline(XElement element)
        {
            string? pointsData = element.Attribute("points")?.Value;
            return pointsData == null ? new List<Point2D>() : ParsePointsList(pointsData);
        }

        private List<SvgElementData>? ParseGroupWithStyle(XElement element)
        {
            var elements = new List<SvgElementData>();
            var groupTransform = ParseTransform(element);

            foreach (var child in element.Elements())
            {
                var childElements = ParseElementWithStyle(child);
                if (childElements != null)
                {
                    foreach (var childElement in childElements)
                    {
                        var transformedPoints = ApplyTransform(childElement.Points, groupTransform);
                        elements.Add(new SvgElementData
                        {
                            Points = transformedPoints,
                            Style = childElement.Style,
                            ElementType = childElement.ElementType
                        });
                    }
                }
            }

            return elements;
        }

        private SvgStyle ParseElementStyle(XElement element)
        {
            var style = new SvgStyle();

            // Parse fill
            string? fill = element.Attribute("fill")?.Value;
            if (!string.IsNullOrEmpty(fill) && fill.ToLower() != "none")
            {
                style.FillColor = ParseSvgColor(fill, ColorInfo.Black);
                style.HasFill = true;
            }

            // Parse stroke
            string? stroke = element.Attribute("stroke")?.Value;
            if (!string.IsNullOrEmpty(stroke) && stroke.ToLower() != "none")
            {
                style.StrokeColor = ParseSvgColor(stroke, ColorInfo.Black);
                style.HasStroke = true;
            }

            // Parse stroke width
            string? strokeWidth = element.Attribute("stroke-width")?.Value;
            if (!string.IsNullOrEmpty(strokeWidth))
            {
                if (double.TryParse(strokeWidth, CultureInfo.InvariantCulture, out double width))
                {
                    style.StrokeWidth = width;
                }
            }

            return style;
        }

        private ColorInfo ParseSvgColor(string colorValue, ColorInfo defaultColor)
        {
            if (string.IsNullOrEmpty(colorValue) || colorValue.ToLower() == "none")
            {
                return ColorInfo.Transparent;
            }

            try
            {
                if (colorValue.StartsWith("#"))
                {
                    return ParseHexColor(colorValue);
                }

                if (colorValue.StartsWith("rgb"))
                {
                    return ParseRgbColor(colorValue);
                }

                return ParseNamedColor(colorValue, defaultColor);
            }
            catch
            {
                return defaultColor;
            }
        }

        private ColorInfo ParseHexColor(string hexColor)
        {
            hexColor = hexColor.TrimStart('#');

            if (hexColor.Length == 3)
            {
                hexColor = string.Concat(hexColor.Select(c => $"{c}{c}"));
            }

            if (hexColor.Length == 6)
            {
                byte r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                return new ColorInfo(r, g, b);
            }

            if (hexColor.Length == 8)
            {
                byte a = Convert.ToByte(hexColor.Substring(0, 2), 16);
                byte r = Convert.ToByte(hexColor.Substring(2, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(4, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(6, 2), 16);
                return new ColorInfo(a, r, g, b);
            }

            throw new ArgumentException($"Invalid hex color format: {hexColor}");
        }

        private ColorInfo ParseRgbColor(string rgbColor)
        {
            var match = Regex.Match(rgbColor,
                @"rgba?\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+))?\s*\)");

            if (match.Success)
            {
                byte r = byte.Parse(match.Groups[1].Value);
                byte g = byte.Parse(match.Groups[2].Value);
                byte b = byte.Parse(match.Groups[3].Value);

                if (match.Groups[4].Success)
                {
                    double alpha = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    byte a = (byte)(alpha * 255);
                    return new ColorInfo(a, r, g, b);
                }
                else
                {
                    return new ColorInfo(r, g, b);
                }
            }

            throw new ArgumentException($"Invalid RGB color format: {rgbColor}");
        }

        private ColorInfo ParseNamedColor(string colorName, ColorInfo defaultColor)
        {
            var namedColors = new Dictionary<string, ColorInfo>(StringComparer.OrdinalIgnoreCase)
            {
                { "black", ColorInfo.Black },
                { "white", ColorInfo.White },
                { "red", ColorInfo.Red },
                { "green", ColorInfo.Green },
                { "blue", ColorInfo.Blue },
                { "transparent", ColorInfo.Transparent }
            };

            return namedColors.TryGetValue(colorName, out ColorInfo color) ? color : defaultColor;
        }

        private double ParseDoubleAttribute(XElement element, string attributeName, double defaultValue)
        {
            string? value = element.Attribute(attributeName)?.Value;
            return string.IsNullOrEmpty(value) ? defaultValue :
                   double.TryParse(value, CultureInfo.InvariantCulture, out double result) ? result : defaultValue;
        }

        private List<Point2D> ParsePointsList(string pointsData)
        {
            var points = new List<Point2D>();

            var coordinates = pointsData
                .Split(new[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            for (int i = 0; i < coordinates.Length - 1; i += 2)
            {
                if (double.TryParse(coordinates[i], CultureInfo.InvariantCulture, out double x) &&
                    double.TryParse(coordinates[i + 1], CultureInfo.InvariantCulture, out double y))
                {
                    points.Add(new Point2D(x, y));
                }
            }

            return points;
        }

        private List<(char Command, List<double> Parameters)> SplitCommands(string d)
        {
            var result = new List<(char Command, List<double> Parameters)>();
            var matches = Regex.Matches(d, @"([a-zA-Z])([^a-zA-Z]*)");

            foreach (Match match in matches)
            {
                var command = match.Groups[1].Value[0];
                var parameters = match.Groups[2].Value
                    .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => double.Parse(p, CultureInfo.InvariantCulture))
                    .ToList();

                result.Add((command, parameters));
            }

            return result;
        }

        private SvgTransform ParseTransform(XElement element)
        {
            var transform = new SvgTransform();
            string? transformAttr = element.Attribute("transform")?.Value;

            if (string.IsNullOrEmpty(transformAttr))
                return transform;

            // Parse translate
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
            }

            // Parse scale
            var scaleMatch = Regex.Match(transformAttr, @"scale\s*\(\s*([^,\s]+)(?:\s*,\s*([^,\s]+))?\s*\)");
            if (scaleMatch.Success)
            {
                if (double.TryParse(scaleMatch.Groups[1].Value, CultureInfo.InvariantCulture, out double sx))
                {
                    transform.ScaleX = sx;
                    transform.ScaleY = sx;
                }
                if (scaleMatch.Groups[2].Success &&
                    double.TryParse(scaleMatch.Groups[2].Value, CultureInfo.InvariantCulture, out double sy))
                {
                    transform.ScaleY = sy;
                }
            }

            return transform;
        }

        private List<Point2D> ApplyTransform(List<Point2D> points, SvgTransform transform)
        {
            var transformedPoints = new List<Point2D>();

            foreach (var point in points)
            {
                var transformedPoint = ApplyTransformToPoint(point, transform);
                transformedPoints.Add(transformedPoint);
            }

            return transformedPoints;
        }

        private Point2D ApplyTransformToPoint(Point2D point, SvgTransform transform)
        {
            double x = point.X;
            double y = point.Y;

            // Apply scaling
            x *= transform.ScaleX;
            y *= transform.ScaleY;

            // Apply translation
            x += transform.TranslateX;
            y += transform.TranslateY;

            return new Point2D(x, y);
        }

        #endregion

        #region Helper Structures

        private struct SvgTransform
        {
            public double TranslateX { get; set; }
            public double TranslateY { get; set; }
            public double ScaleX { get; set; }
            public double ScaleY { get; set; }

            public SvgTransform()
            {
                TranslateX = 0;
                TranslateY = 0;
                ScaleX = 1;
                ScaleY = 1;
            }
        }

        #endregion
    }
}
