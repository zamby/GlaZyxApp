using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlazyxApplication
{
    public class Star : DrawObj
    {
        public Star(double outerRadius = 50, double innerRadius = 25, int points = 5)
        {
            Name = "Star";
            var starPoints = CreateStarShape(outerRadius, innerRadius, points);
            Points = starPoints;
            
            // Calculate bounds
            if (starPoints.Count > 0)
            {
                double minX = starPoints.Min(p => p.X);
                double minY = starPoints.Min(p => p.Y);
                double maxX = starPoints.Max(p => p.X);
                double maxY = starPoints.Max(p => p.Y);

                Bounds = new Rect(minX, minY, maxX - minX, maxY - minY);
            }
            else
            {
                Bounds = new Rect(0, 0, outerRadius * 2, outerRadius * 2);
            }
            
            // Default gold color
            ColorSolid = Color.FromArgb(255, 255, 215, 0);
        }

        public Star(string colorHex, double outerRadius = 50, double innerRadius = 25, int points = 5)
        {
            Name = "Star";
            var starPoints = CreateStarShape(outerRadius, innerRadius, points);
            Points = starPoints;
            
            // Calculate bounds
            if (starPoints.Count > 0)
            {
                double minX = starPoints.Min(p => p.X);
                double minY = starPoints.Min(p => p.Y);
                double maxX = starPoints.Max(p => p.X);
                double maxY = starPoints.Max(p => p.Y);

                Bounds = new Rect(minX, minY, maxX - minX, maxY - minY);
            }
            else
            {
                Bounds = new Rect(0, 0, outerRadius * 2, outerRadius * 2);
            }
            
            // Convert hex color
            ColorSolid = ParseHexColor(colorHex);
        }

        public List<Point> Points { get; private set; } = new List<Point>();

        private List<Point> CreateStarShape(double outerRadius, double innerRadius, int points)
        {
            var starPoints = new List<Point>();
            double angleStep = Math.PI / points;
            
            for (int i = 0; i < points * 2; i++)
            {
                double angle = i * angleStep;
                double radius = (i % 2 == 0) ? outerRadius : innerRadius;
                double x = Math.Cos(angle) * radius + outerRadius;
                double y = Math.Sin(angle) * radius + outerRadius;
                starPoints.Add(new Point(x, y));
            }
            
            return starPoints;
        }

        public override void Render(DrawingContext context)
        {
            Console.WriteLine($"Rendering Star object: {Name} at position {Position}");
            
            if (Points.Count < 3) return; // Need at least a triangle
            
            var geometry = new StreamGeometry();
            using (var geometryContext = geometry.Open())
            {
                var firstPoint = new Point(
                    Points[0].X + Position.X,
                    Points[0].Y + Position.Y
                );

                geometryContext.BeginFigure(firstPoint, isFilled: true);

                foreach (var point in Points.Skip(1))
                {
                    var transformedPoint = new Point(
                        point.X + Position.X,
                        point.Y + Position.Y
                    );
                    geometryContext.LineTo(transformedPoint);
                }

                geometryContext.EndFigure(true); // Automatically closes the path
            }

            var brush = new SolidColorBrush(ColorSolid);
            context.DrawGeometry(brush, null, geometry);
        }

        private Color ParseHexColor(string hexColor)
        {
            try
            {
                hexColor = hexColor.TrimStart('#');
                
                if (hexColor.Length == 3)
                {
                    // Format #RGB -> #RRGGBB
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing color '{hexColor}': {ex.Message}");
            }

            // Fallback to default gold color
            return Color.FromArgb(255, 255, 215, 0);
        }
    }
}
