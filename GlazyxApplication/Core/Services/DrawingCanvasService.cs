using GlazyxApplication.Core.Interfaces;
using GlazyxApplication.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlazyxApplication.Core.Services
{
    /// <summary>
    /// Service for managing a drawing canvas and its objects
    /// </summary>
    public class DrawingCanvasService : IDrawingCanvasService
    {
        private readonly List<IDrawableObject> _objects = new();
        private readonly List<IDrawableObject> _selectedObjects = new();
        private Bounds2D _canvasBounds;

        public event EventHandler<DrawingObjectEventArgs>? ObjectAdded;
        public event EventHandler<DrawingObjectEventArgs>? ObjectRemoved;
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler? CanvasInvalidated;

        public IReadOnlyList<IDrawableObject> Objects => _objects.AsReadOnly();
        public IReadOnlyList<IDrawableObject> SelectedObjects => _selectedObjects.AsReadOnly();

        public Bounds2D CanvasBounds 
        { 
            get => _canvasBounds;
            set
            {
                _canvasBounds = value;
                InvalidateCanvas();
            }
        }

        public DrawingCanvasService()
        {
            _canvasBounds = new Bounds2D(0, 0, 800, 600); // Default canvas size
        }

        public void AddObject(IDrawableObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (_objects.Contains(obj))
                return;

            _objects.Add(obj);
            ObjectAdded?.Invoke(this, new DrawingObjectEventArgs(obj));
            InvalidateCanvas();
        }

        public bool RemoveObject(IDrawableObject obj)
        {
            if (obj == null)
                return false;

            bool removed = _objects.Remove(obj);
            if (removed)
            {
                // Remove from selection if it was selected
                if (_selectedObjects.Contains(obj))
                {
                    DeselectObject(obj);
                }

                ObjectRemoved?.Invoke(this, new DrawingObjectEventArgs(obj));
                InvalidateCanvas();
            }

            return removed;
        }

        public int RemoveObjects(IEnumerable<IDrawableObject> objects)
        {
            var objectsToRemove = objects.ToList();
            int removedCount = 0;

            foreach (var obj in objectsToRemove)
            {
                if (RemoveObject(obj))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        public void ClearAll()
        {
            var objectsToRemove = _objects.ToList();
            _objects.Clear();
            
            var selectedToRemove = _selectedObjects.ToList();
            _selectedObjects.Clear();

            // Fire events for all removed objects
            foreach (var obj in objectsToRemove)
            {
                ObjectRemoved?.Invoke(this, new DrawingObjectEventArgs(obj));
            }

            // Fire selection changed event if there were selected objects
            if (selectedToRemove.Any())
            {
                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(
                    new List<IDrawableObject>(), selectedToRemove));
            }

            InvalidateCanvas();
        }

        public void SelectObject(IDrawableObject obj, bool addToSelection = false)
        {
            if (obj == null || !_objects.Contains(obj))
                return;

            var previousSelection = _selectedObjects.ToList();

            if (!addToSelection)
            {
                // Clear existing selection
                foreach (var selected in _selectedObjects)
                {
                    selected.IsSelected = false;
                }
                _selectedObjects.Clear();
            }

            if (!_selectedObjects.Contains(obj))
            {
                _selectedObjects.Add(obj);
                obj.IsSelected = true;

                var deselected = addToSelection ? new List<IDrawableObject>() : previousSelection;
                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(
                    new List<IDrawableObject> { obj }, deselected));

                InvalidateCanvas();
            }
        }

        public void DeselectObject(IDrawableObject obj)
        {
            if (obj == null || !_selectedObjects.Contains(obj))
                return;

            _selectedObjects.Remove(obj);
            obj.IsSelected = false;

            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(
                new List<IDrawableObject>(), new List<IDrawableObject> { obj }));

            InvalidateCanvas();
        }

        public void ClearSelection()
        {
            if (!_selectedObjects.Any())
                return;

            var deselected = _selectedObjects.ToList();

            foreach (var obj in _selectedObjects)
            {
                obj.IsSelected = false;
            }
            _selectedObjects.Clear();

            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(
                new List<IDrawableObject>(), deselected));

            InvalidateCanvas();
        }

        public IDrawableObject? GetObjectAt(Point2D point)
        {
            // Search in reverse order (top to bottom in drawing order)
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                var obj = _objects[i];
                if (obj.IsVisible && obj.Contains(point))
                {
                    return obj;
                }
            }

            return null;
        }

        public IEnumerable<IDrawableObject> GetObjectsInBounds(Bounds2D bounds)
        {
            return _objects.Where(obj => obj.IsVisible && BoundsIntersect(obj.Bounds, bounds));
        }

        public void MoveSelectedObjects(Point2D offset)
        {
            if (!_selectedObjects.Any() || offset == Point2D.Zero)
                return;

            foreach (var obj in _selectedObjects)
            {
                obj.Translate(offset);
            }

            InvalidateCanvas();
        }

        public int DeleteSelectedObjects()
        {
            var objectsToDelete = _selectedObjects.ToList();
            return RemoveObjects(objectsToDelete);
        }

        public void InvalidateCanvas()
        {
            CanvasInvalidated?.Invoke(this, EventArgs.Empty);
        }

        #region Private Helper Methods

        private bool BoundsIntersect(Bounds2D bounds1, Bounds2D bounds2)
        {
            return bounds1.TopLeft.X < bounds2.BottomRight.X &&
                   bounds1.BottomRight.X > bounds2.TopLeft.X &&
                   bounds1.TopLeft.Y < bounds2.BottomRight.Y &&
                   bounds1.BottomRight.Y > bounds2.TopLeft.Y;
        }

        #endregion
    }
}
