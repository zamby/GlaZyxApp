using Avalonia;
using Avalonia.Media;
using GlazyxApplication.Core.Models;
using System;
using System.Collections.Generic;

namespace GlazyxApplication
{
    public abstract class DrawObj
    {
        public string Name { get; set; } = string.Empty;

        
        internal Rect Bounds { get; set; }
        internal Color ColorSolid { get; set; } =
            Color.FromArgb(255, 0, 0, 0); // Default black color

        // Object position, used for rendering
        // This is the position relative to canvas or window 
        public Point Position { get; set; }

        public bool isClosed { get; set; } = false; // Indicates if the object is closed (e.g., a polygon)
        
        // Selection state for highlighting
        public bool IsSelected { get; set; } = false;

        #region New Interface-Compatible Methods

        /// <summary>
        /// Get unique identifier for this object
        /// </summary>
        public string GetId() => GetHashCode().ToString();

        /// <summary>
        /// Get the position as Point2D from core models
        /// </summary>
        public Point2D GetPosition() => new(Position.X, Position.Y);

        /// <summary>
        /// Set the position from Point2D
        /// </summary>
        public void SetPosition(Point2D position)
        {
            Position = new Point(position.X, position.Y);
        }

        /// <summary>
        /// Get bounding box as Bounds2D from core models
        /// </summary>
        public Bounds2D GetBounds2D()
        {
            return new Bounds2D(
                new Point2D(Position.X + Bounds.X, Position.Y + Bounds.Y),
                new Point2D(Position.X + Bounds.X + Bounds.Width, Position.Y + Bounds.Y + Bounds.Height)
            );
        }

        /// <summary>
        /// Move the object by the specified offset
        /// </summary>
        public void Translate(Point2D offset)
        {
            var currentPos = GetPosition();
            SetPosition(currentPos.Add(offset));
        }

        /// <summary>
        /// Check if a point (in core Point2D format) is within this object
        /// </summary>
        public bool Contains(Point2D point)
        {
            var avaloniaPoint = new Point(point.X, point.Y);
            return HitTest(avaloniaPoint);
        }

        /// <summary>
        /// Get the geometric points that define this object as Point2D collection
        /// Virtual method that derived classes should override
        /// </summary>
        public virtual IEnumerable<Point2D> GetGeometryPoints()
        {
            // Default implementation returns rectangle corners
            var bounds = GetBounds2D();
            return new[]
            {
                bounds.TopLeft,
                new Point2D(bounds.BottomRight.X, bounds.TopLeft.Y),
                bounds.BottomRight,
                new Point2D(bounds.TopLeft.X, bounds.BottomRight.Y),
                bounds.TopLeft // Close the shape
            };
        }

        #endregion

        /// <summary>
        /// Renders the object in the specified drawing context.
        /// </summary>
        /// <param name="context">The drawing context in which to render the object.</param> 
        public abstract void Render(DrawingContext context);
        
        /// <summary>
        /// Renders the selection highlight around the object if it's selected.
        /// </summary>
        /// <param name="context">The drawing context in which to render the highlight.</param>
        public virtual void RenderSelectionHighlight(DrawingContext context)
        {
            if (IsSelected)
            {
                // Create blue selection rectangle around the object bounds
                var selectionRect = new Rect(
                    Position.X + Bounds.X - 3, 
                    Position.Y + Bounds.Y - 3, 
                    Bounds.Width + 6, 
                    Bounds.Height + 6
                );
                
                // Blue dashed pen for selection highlight
                var selectionPen = new Pen(
                    new SolidColorBrush(Color.FromArgb(180, 0, 120, 255)), // Semi-transparent blue
                    2.0, 
                    new DashStyle(new double[] { 4, 2 }, 0) // Dashed line
                );
                
                // Draw selection rectangle
                context.DrawRectangle(null, selectionPen, selectionRect);
                
                // Add small corner handles for visual feedback
                var handleSize = 6.0;
                var handleBrush = new SolidColorBrush(Color.FromArgb(255, 0, 120, 255));
                
                // Top-left handle
                var topLeft = new Rect(selectionRect.Left - handleSize/2, selectionRect.Top - handleSize/2, handleSize, handleSize);
                context.DrawRectangle(handleBrush, null, topLeft);
                
                // Top-right handle
                var topRight = new Rect(selectionRect.Right - handleSize/2, selectionRect.Top - handleSize/2, handleSize, handleSize);
                context.DrawRectangle(handleBrush, null, topRight);
                
                // Bottom-left handle
                var bottomLeft = new Rect(selectionRect.Left - handleSize/2, selectionRect.Bottom - handleSize/2, handleSize, handleSize);
                context.DrawRectangle(handleBrush, null, bottomLeft);
                
                // Bottom-right handle
                var bottomRight = new Rect(selectionRect.Right - handleSize/2, selectionRect.Bottom - handleSize/2, handleSize, handleSize);
                context.DrawRectangle(handleBrush, null, bottomRight);
            }
        }
        
        /// <summary>
        /// Checks if a specified point is within the object.
        /// </summary>
        /// <param name="pMouse">The point to check.</param>
        /// <returns>True if the point is within the object, otherwise false.</returns>
        public bool HitTest(Point pMouse)
        {
            // Write debug console
            Console.WriteLine("HitTest " + this.Name);
            var hitRect = new Rect(Position.X + Bounds.X, Position.Y + Bounds.Y, Bounds.Width, Bounds.Height);
            return hitRect.Contains(pMouse);
        }
    }
}
