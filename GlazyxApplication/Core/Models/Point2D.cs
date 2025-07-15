using System;

namespace GlazyxApplication.Core.Models
{
    /// <summary>
    /// Represents a 2D point in geometric space
    /// </summary>
    public readonly struct Point2D : IEquatable<Point2D>
    {
        public double X { get; }
        public double Y { get; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Point2D Zero => new(0, 0);

        public Point2D Add(Point2D other) => new(X + other.X, Y + other.Y);
        public Point2D Subtract(Point2D other) => new(X - other.X, Y - other.Y);
        public Point2D Scale(double factor) => new(X * factor, Y * factor);
        
        public double DistanceTo(Point2D other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public bool Equals(Point2D other) => 
            Math.Abs(X - other.X) < 1e-10 && Math.Abs(Y - other.Y) < 1e-10;

        public override bool Equals(object? obj) => obj is Point2D other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Point2D left, Point2D right) => left.Equals(right);
        public static bool operator !=(Point2D left, Point2D right) => !left.Equals(right);

        public override string ToString() => $"({X:F2}, {Y:F2})";
    }
}
