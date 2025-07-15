using Avalonia;
using Avalonia.Media;
using System;

namespace GlazyxApplication
{
    public class Circle : DrawObj
    {
        public double Radius { get; set; }
        public Point Center { get; set; }

        public Circle(double radius)
        {
            Name = "Circle";
            Radius = radius;
            Center = new Point(radius, radius); // Center relative to bounds
            Bounds = new Rect(0, 0, radius * 2, radius * 2);
        }

        public Circle(double radius, Point center) : this(radius)
        {
            Center = center;
        }

        public override void Render(DrawingContext context)
        {
            Console.WriteLine($"Rendering Circle: radius={Radius} at position {Position}");
            
            // Calculate circle center position
            var centerX = Position.X + Center.X;
            var centerY = Position.Y + Center.Y;
            
            // Create a perfect circle using EllipseGeometry
            var circleGeometry = new EllipseGeometry(new Rect(
                centerX - Radius, 
                centerY - Radius, 
                Radius * 2, 
                Radius * 2
            ));
            
            // Draw the circle with the set color
            var brush = new SolidColorBrush(ColorSolid);
            context.DrawGeometry(brush, null, circleGeometry);
        }

        // Method to check if a point is inside the circle
        public bool ContainsPoint(Point point)
        {
            var centerX = Position.X + Center.X;
            var centerY = Position.Y + Center.Y;
            
            var dx = point.X - centerX;
            var dy = point.Y - centerY;
            var distanceSquared = dx * dx + dy * dy;
            
            return distanceSquared <= Radius * Radius;
        }

        // Method to get circle perimeter points (SVG compatible)
        public Point[] GetCirclePoints(int segments = 50)
        {
            var points = new Point[segments + 1];
            var centerX = Position.X + Center.X;
            var centerY = Position.Y + Center.Y;
            
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = centerX + Radius * Math.Cos(angle);
                double y = centerY + Radius * Math.Sin(angle);
                points[i] = new Point(x, y);
            }
            
            return points;
        }
    }
}
