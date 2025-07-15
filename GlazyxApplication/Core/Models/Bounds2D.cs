using System;

namespace GlazyxApplication.Core.Models
{
    /// <summary>
    /// Represents a 2D rectangular boundary
    /// </summary>
    public readonly struct Bounds2D : IEquatable<Bounds2D>
    {
        public Point2D TopLeft { get; }
        public Point2D BottomRight { get; }

        public double Width => BottomRight.X - TopLeft.X;
        public double Height => BottomRight.Y - TopLeft.Y;
        public Point2D Center => new((TopLeft.X + BottomRight.X) / 2, (TopLeft.Y + BottomRight.Y) / 2);

        public Bounds2D(Point2D topLeft, Point2D bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public Bounds2D(double x, double y, double width, double height)
            : this(new Point2D(x, y), new Point2D(x + width, y + height))
        {
        }

        public static Bounds2D Empty => new(Point2D.Zero, Point2D.Zero);

        public bool Contains(Point2D point) =>
            point.X >= TopLeft.X && point.X <= BottomRight.X &&
            point.Y >= TopLeft.Y && point.Y <= BottomRight.Y;

        public Bounds2D Expand(double margin) =>
            new(TopLeft.Subtract(new Point2D(margin, margin)),
                BottomRight.Add(new Point2D(margin, margin)));

        public bool Equals(Bounds2D other) => TopLeft.Equals(other.TopLeft) && BottomRight.Equals(other.BottomRight);
        public override bool Equals(object? obj) => obj is Bounds2D other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(TopLeft, BottomRight);

        public static bool operator ==(Bounds2D left, Bounds2D right) => left.Equals(right);
        public static bool operator !=(Bounds2D left, Bounds2D right) => !left.Equals(right);

        public override string ToString() => $"Bounds[{TopLeft} - {BottomRight}]";
    }
}
