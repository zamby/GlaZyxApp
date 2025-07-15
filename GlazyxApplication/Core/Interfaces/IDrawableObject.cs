using GlazyxApplication.Core.Models;
using System.Collections.Generic;

namespace GlazyxApplication.Core.Interfaces
{
    /// <summary>
    /// Interface for objects that can be drawn and rendered
    /// </summary>
    public interface IDrawableObject
    {
        /// <summary>
        /// Unique identifier for the object
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display name of the object
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Position of the object
        /// </summary>
        Point2D Position { get; set; }

        /// <summary>
        /// Whether the object is currently selected
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Whether the object is visible
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Bounding box of the object
        /// </summary>
        Bounds2D Bounds { get; }

        /// <summary>
        /// Get the geometric points that define this object
        /// </summary>
        /// <returns>Collection of points representing the object's geometry</returns>
        IEnumerable<Point2D> GetGeometryPoints();

        /// <summary>
        /// Check if a point is within this object
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>True if the point is inside the object</returns>
        bool Contains(Point2D point);

        /// <summary>
        /// Move the object by the specified offset
        /// </summary>
        /// <param name="offset">Offset to apply</param>
        void Translate(Point2D offset);

        /// <summary>
        /// Create a deep copy of this object
        /// </summary>
        /// <returns>New instance that is a copy of this object</returns>
        IDrawableObject Clone();
    }

    /// <summary>
    /// Interface for objects that can be rendered to a drawing context
    /// </summary>
    public interface IRenderableObject : IDrawableObject
    {
        /// <summary>
        /// Render the object to a drawing context
        /// </summary>
        /// <param name="context">Platform-specific drawing context</param>
        void Render(object context);

        /// <summary>
        /// Render selection indicators (handles, highlight) for the object
        /// </summary>
        /// <param name="context">Platform-specific drawing context</param>
        void RenderSelectionHighlight(object context);
    }

    /// <summary>
    /// Interface for shapes with configurable stroke and fill
    /// </summary>
    public interface IStylableObject : IDrawableObject
    {
        /// <summary>
        /// Fill color of the object
        /// </summary>
        ColorInfo FillColor { get; set; }

        /// <summary>
        /// Stroke color of the object
        /// </summary>
        ColorInfo StrokeColor { get; set; }

        /// <summary>
        /// Stroke width of the object
        /// </summary>
        double StrokeWidth { get; set; }

        /// <summary>
        /// Whether the object has a fill
        /// </summary>
        bool HasFill { get; set; }

        /// <summary>
        /// Whether the object has a stroke
        /// </summary>
        bool HasStroke { get; set; }
    }
}
