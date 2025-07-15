using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Animation;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Threading;

namespace GlazyxApplication
{
    public class AreaDraw : Control
    {

        private bool _isPaused;
        private bool _isDragging;
        private Point _lastMousePosition;

        private DrawObj? _lastDrawObjClick;

        private List<DrawObj> _drawObjects = new List<DrawObj>();

        public AreaDraw()
        {

            // Animation timer (approximately 30 FPS)
            /*var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(8) // ~30fps
            };
            timer.Tick += OnTimerTick;
            timer.Start();*/

            // Handle input events
            this.PointerPressed += OnPointerPressed;
            this.PointerReleased += OnPointerReleased;
            this.PointerMoved += OnPointerMoved;
            this.KeyDown += OnKeyDown;
            
            // Make the control focusable to receive keyboard input
            this.Focusable = true;

        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            // Force redraw
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // Draw white background
            var backgroundRect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            context.DrawRectangle(Brushes.White, null, backgroundRect);

            // Draw grid
            var gridPen = new Pen(Brushes.LightGray, 1);
            for (double x = 0; x < Bounds.Width; x += 10)
            {
                context.DrawLine(gridPen, new Point(x, 0), new Point(x, Bounds.Height));
            }
            for (double y = 0; y < Bounds.Height; y += 10)
            {
                context.DrawLine(gridPen, new Point(0, y), new Point(Bounds.Width, y));
            }

            // Render all objects in the list
            foreach (var drawObj in _drawObjects)
            {
                drawObj.Render(context);
            }
            
            // Render selection highlights after all objects
            foreach (var drawObj in _drawObjects)
            {
                drawObj.RenderSelectionHighlight(context);
            }

            // Draw outer border of the control
            var borderPen = new Pen(Brushes.Black, 1);
            var borderRect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            context.DrawRectangle(null, borderPen, borderRect);
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Give focus to this control to receive keyboard input
            this.Focus();
            
            // Check if click occurred on an object
            var mousePoint = e.GetPosition(this);
            var hitObject = _drawObjects.FirstOrDefault(obj => obj.HitTest(mousePoint));
            
            // Clear selection from all objects first
            foreach (var obj in _drawObjects)
            {
                obj.IsSelected = false;
            }
            
            if (hitObject != null)
            {
                // Select the clicked object
                hitObject.IsSelected = true;
                
                _isPaused = true; // Stop animation
                _isDragging = true; // Start dragging
                _lastMousePosition = mousePoint;
                _lastDrawObjClick = hitObject;
                InvalidateVisual(); // Redraw to show selection highlight
            }
            else
            {
                // No object clicked, just clear selection
                InvalidateVisual();
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false; // End dragging
                _isPaused = false; // Resume object movement
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _lastDrawObjClick != null)
            {
                var mousePoint = e.GetPosition(this);
                var offset = mousePoint - _lastMousePosition;

                _lastDrawObjClick.Position = new Point(
                    _lastDrawObjClick.Position.X + offset.X,
                    _lastDrawObjClick.Position.Y + offset.Y
                );

                _lastMousePosition = mousePoint;
                InvalidateVisual();
            }
        }

        // Public methods to manage objects from context menu
        public void AddCircle(Point? position = null)
        {
            // Create a circle with random radius between 15 and 30
            var radius = new Random().Next(15, 30);
            var circle = new Circle(radius);
            
            // Assign random color
            var colors = new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Purple, Colors.Orange, Colors.Pink };
            var color = colors[new Random().Next(colors.Length)];
            circle.ColorSolid = color;
            
            if (position.HasValue)
            {
                circle.Position = position.Value;
            }
            else
            {
                // Random position if not specified (considering radius)
                circle.Position = new Point(
                    new Random().Next(radius, (int)Bounds.Width - radius),
                    new Random().Next(radius, (int)Bounds.Height - radius)
                );
            }
            _drawObjects.Add(circle);
            InvalidateVisual();
        }

        public void AddRectangle(Point? position = null)
        {
            var colors = new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF" };
            var color = colors[new Random().Next(colors.Length)];
            var rectangle = new Rectangle(
                new Random().Next(30, 80), 
                new Random().Next(30, 80), 
                color
            );
            
            if (position.HasValue)
            {
                rectangle.Position = position.Value;
            }
            else
            {
                rectangle.Position = new Point(
                    new Random().Next(50, (int)Bounds.Width - 100),
                    new Random().Next(50, (int)Bounds.Height - 100)
                );
            }
            _drawObjects.Add(rectangle);
            InvalidateVisual();
        }

        public void AddStar(Point? position = null)
        {
            var colors = new[] { "#FFD700", "#FFA500", "#FF6347", "#FF1493", "#9370DB", "#00CED1" };
            var color = colors[new Random().Next(colors.Length)];
            var star = new Star(color);
            
            if (position.HasValue)
            {
                star.Position = position.Value;
            }
            else
            {
                // Random position if not specified
                star.Position = new Point(
                    new Random().Next(50, (int)Bounds.Width - 100),
                    new Random().Next(50, (int)Bounds.Height - 100)
                );
            }
            _drawObjects.Add(star);
            InvalidateVisual();
        }

        public void AddSvg(Point? position = null, string? filePath = null)
        {
            Svg svg;
            
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                // Load from file
                svg = new Svg(filePath, 1); 
            }
            else
            {
                // Create example heart shape SVG if no file is provided
                string svgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""100"" height=""100"" viewBox=""0 0 100 100"">
                    <path d=""M10,30 A20,20 0,0,1 50,30 A20,20 0,0,1 90,30 Q90,60 50,90 Q10,60 10,30 z"" fill=""red""/>
                </svg>";
                svg = new Svg(svgContent, true, 1.0);
            }
            
            /*var colors = new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Purple, Colors.Orange };
            var color = colors[new Random().Next(colors.Length)];
            svg.SetColor(color);*/
            
            if (position.HasValue)
            {
                svg.Position = position.Value;
            }
            else
            {
                svg.Position = new Point(
                    new Random().Next(50, (int)Bounds.Width - 150),
                    new Random().Next(50, (int)Bounds.Height - 150)
                );
            }
            
            _drawObjects.Add(svg);
            InvalidateVisual();
        }

        public async void AddSvgFromFile(Point? position = null)
        {
            try
            {
                // Open dialog to select SVG file
                var fileDialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = "Select SVG file",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new Avalonia.Platform.Storage.FilePickerFileType("SVG Files")
                        {
                            Patterns = new[] { "*.svg" }
                        }
                    }
                };

                var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    var result = await topLevel.StorageProvider.OpenFilePickerAsync(fileDialog);
                    
                    if (result.Count > 0)
                    {
                        var selectedFile = result[0];
                        var filePath = selectedFile.Path.LocalPath;
                        
                        Console.WriteLine($"Selected SVG file: {filePath}");
                        AddSvg(position, filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting file: {ex.Message}");
                // Fallback: create default SVG
                AddSvg(position);
            }
        }

        public void ClearAll()
        {
            _drawObjects.Clear();
            InvalidateVisual();
        }

        public async void ExportToGCode()
        {
            try
            {
                if (_drawObjects.Count == 0)
                {
                    Console.WriteLine("No objects to export to G-Code");
                    return;
                }

                // Default G-Code configuration
                var settings = new GCodeGenerator.GCodeSettings
                {
                    FeedRate = 1000.0,      // mm/min
                    LaserPower = 80.0,      // %
                    ScaleFactor = 0.1,      // pixel to mm
                    LaserMode = true,       // true = laser, false = mill
                    Units = "mm"
                };

                var generator = new GCodeGenerator(settings);
                var gcode = generator.GenerateGCode(_drawObjects);

                // Save G-Code file
                var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    var saveDialog = new Avalonia.Platform.Storage.FilePickerSaveOptions
                    {
                        Title = "Save G-Code",
                        DefaultExtension = "gcode",
                        FileTypeChoices = new[]
                        {
                            new Avalonia.Platform.Storage.FilePickerFileType("G-Code Files")
                            {
                                Patterns = new[] { "*.gcode", "*.nc", "*.cnc" }
                            },
                            new Avalonia.Platform.Storage.FilePickerFileType("All Files")
                            {
                                Patterns = new[] { "*.*" }
                            }
                        }
                    };

                    var result = await topLevel.StorageProvider.SaveFilePickerAsync(saveDialog);
                    
                    if (result != null)
                    {
                        await using var stream = await result.OpenWriteAsync();
                        await using var writer = new System.IO.StreamWriter(stream);
                        await writer.WriteAsync(gcode);
                        
                        Console.WriteLine($"G-Code exported to: {result.Path.LocalPath}");
                        Console.WriteLine($"Objects processed: {_drawObjects.Count}");
                        Console.WriteLine("G-Code preview:");
                        Console.WriteLine(gcode.Substring(0, Math.Min(500, gcode.Length)) + "...");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting G-Code: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            // Handle Delete key to remove selected objects
            if (e.Key == Key.Delete)
            {
                DeleteSelectedObjects();
                e.Handled = true; // Prevent further processing of the key
            }
        }
        
        /// <summary>
        /// Deletes all currently selected objects from the drawing canvas.
        /// </summary>
        private void DeleteSelectedObjects()
        {
            var selectedObjects = _drawObjects.Where(obj => obj.IsSelected).ToList();
            
            if (selectedObjects.Count > 0)
            {
                Console.WriteLine($"Deleting {selectedObjects.Count} selected object(s)");
                
                // Remove selected objects from the list
                foreach (var obj in selectedObjects)
                {
                    _drawObjects.Remove(obj);
                    Console.WriteLine($"Deleted object: {obj.Name}");
                }
                
                // Clear the last clicked object reference if it was deleted
                if (_lastDrawObjClick != null && selectedObjects.Contains(_lastDrawObjClick))
                {
                    _lastDrawObjClick = null;
                }
                
                // Force redraw to update the canvas
                InvalidateVisual();
            }
        }

    }
}
