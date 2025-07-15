using GlazyxApplication.Core.Models;
using System;
using System.Collections.Generic;

namespace GlazyxApplication.Core.Interfaces
{
    /// <summary>
    /// Interface for managing a drawing canvas and its objects
    /// </summary>
    public interface IDrawingCanvasService
    {
        /// <summary>
        /// Event fired when objects are added to the canvas
        /// </summary>
        event EventHandler<DrawingObjectEventArgs> ObjectAdded;

        /// <summary>
        /// Event fired when objects are removed from the canvas
        /// </summary>
        event EventHandler<DrawingObjectEventArgs> ObjectRemoved;

        /// <summary>
        /// Event fired when objects are selected or deselected
        /// </summary>
        event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        /// <summary>
        /// Event fired when the canvas needs to be redrawn
        /// </summary>
        event EventHandler CanvasInvalidated;

        /// <summary>
        /// All objects on the canvas
        /// </summary>
        IReadOnlyList<IDrawableObject> Objects { get; }

        /// <summary>
        /// Currently selected objects
        /// </summary>
        IReadOnlyList<IDrawableObject> SelectedObjects { get; }

        /// <summary>
        /// Canvas dimensions
        /// </summary>
        Bounds2D CanvasBounds { get; set; }

        /// <summary>
        /// Add an object to the canvas
        /// </summary>
        /// <param name="obj">Object to add</param>
        void AddObject(IDrawableObject obj);

        /// <summary>
        /// Remove an object from the canvas
        /// </summary>
        /// <param name="obj">Object to remove</param>
        /// <returns>True if the object was removed</returns>
        bool RemoveObject(IDrawableObject obj);

        /// <summary>
        /// Remove multiple objects from the canvas
        /// </summary>
        /// <param name="objects">Objects to remove</param>
        /// <returns>Number of objects removed</returns>
        int RemoveObjects(IEnumerable<IDrawableObject> objects);

        /// <summary>
        /// Clear all objects from the canvas
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Select an object
        /// </summary>
        /// <param name="obj">Object to select</param>
        /// <param name="addToSelection">If true, add to current selection; if false, replace selection</param>
        void SelectObject(IDrawableObject obj, bool addToSelection = false);

        /// <summary>
        /// Deselect an object
        /// </summary>
        /// <param name="obj">Object to deselect</param>
        void DeselectObject(IDrawableObject obj);

        /// <summary>
        /// Clear current selection
        /// </summary>
        void ClearSelection();

        /// <summary>
        /// Find object at the specified point
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Object at the point, or null if none found</returns>
        IDrawableObject? GetObjectAt(Point2D point);

        /// <summary>
        /// Find all objects within the specified bounds
        /// </summary>
        /// <param name="bounds">Bounds to test</param>
        /// <returns>Objects within the bounds</returns>
        IEnumerable<IDrawableObject> GetObjectsInBounds(Bounds2D bounds);

        /// <summary>
        /// Move selected objects by the specified offset
        /// </summary>
        /// <param name="offset">Offset to apply</param>
        void MoveSelectedObjects(Point2D offset);

        /// <summary>
        /// Delete selected objects
        /// </summary>
        /// <returns>Number of objects deleted</returns>
        int DeleteSelectedObjects();

        /// <summary>
        /// Invalidate the canvas to trigger a redraw
        /// </summary>
        void InvalidateCanvas();
    }

    /// <summary>
    /// Event arguments for drawing object events
    /// </summary>
    public class DrawingObjectEventArgs : EventArgs
    {
        public IDrawableObject Object { get; }

        public DrawingObjectEventArgs(IDrawableObject obj)
        {
            Object = obj;
        }
    }

    /// <summary>
    /// Event arguments for selection change events
    /// </summary>
    public class SelectionChangedEventArgs : EventArgs
    {
        public IReadOnlyList<IDrawableObject> SelectedObjects { get; }
        public IReadOnlyList<IDrawableObject> DeselectedObjects { get; }

        public SelectionChangedEventArgs(IReadOnlyList<IDrawableObject> selected, IReadOnlyList<IDrawableObject> deselected)
        {
            SelectedObjects = selected;
            DeselectedObjects = deselected;
        }
    }
}
