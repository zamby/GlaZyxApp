using GlazyxApplication.Core.Interfaces;
using GlazyxApplication.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GlazyxApplication.Core.Services
{
    /// <summary>
    /// Service for generating G-Code from drawable objects and geometric data
    /// </summary>
    public class GCodeGenerationService : IGCodeGenerationService
    {
        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

        public string GenerateGCode(IEnumerable<IDrawableObject> objects, GCodeSettings? settings = null)
        {
            settings ??= new GCodeSettings();
            var gcode = new StringBuilder();

            WriteHeader(gcode, settings);

            foreach (var obj in objects.Where(o => o.IsVisible))
            {
                WriteObjectComment(gcode, obj, settings);
                ProcessDrawableObject(gcode, obj, settings);
                gcode.AppendLine();
            }

            WriteFooter(gcode, settings);

            return gcode.ToString();
        }

        public string GenerateGCodeFromPoints(IEnumerable<Point2D> points, GCodeSettings? settings = null)
        {
            settings ??= new GCodeSettings();
            var gcode = new StringBuilder();

            WriteHeader(gcode, settings);

            if (settings.IncludeComments)
            {
                gcode.AppendLine("; Processing point sequence");
            }

            ProcessPointSequence(gcode, points, settings);

            WriteFooter(gcode, settings);

            return gcode.ToString();
        }

        public bool ValidateGCode(string gcode)
        {
            if (string.IsNullOrWhiteSpace(gcode))
                return false;

            try
            {
                var lines = gcode.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    
                    // Skip comments
                    if (trimmed.StartsWith(";") || trimmed.StartsWith("("))
                        continue;
                    
                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;

                    // Basic G-Code validation - should start with G, M, or coordinate
                    if (!Regex.IsMatch(trimmed, @"^[GMXYZFSgmxyzfs]", RegexOptions.IgnoreCase))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public double EstimateExecutionTime(string gcode, GCodeSettings settings)
        {
            if (string.IsNullOrWhiteSpace(gcode))
                return 0;

            double totalTime = 0;
            var lines = gcode.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var currentPosition = Point2D.Zero;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                // Skip comments and empty lines
                if (trimmed.StartsWith(";") || trimmed.StartsWith("(") || string.IsNullOrWhiteSpace(trimmed))
                    continue;

                // Parse movement commands
                if (trimmed.StartsWith("G0") || trimmed.StartsWith("G1") || 
                    trimmed.StartsWith("g0") || trimmed.StartsWith("g1"))
                {
                    var newPosition = ParsePosition(trimmed, currentPosition);
                    double distance = currentPosition.DistanceTo(newPosition);
                    
                    double feedRate = trimmed.ToLower().StartsWith("g0") ? 
                        settings.RapidFeedRate : settings.CutFeedRate;
                    
                    totalTime += (distance / feedRate) * 60; // Convert from mm/min to seconds
                    currentPosition = newPosition;
                }
            }

            return totalTime;
        }

        #region Private Implementation

        private void WriteHeader(StringBuilder gcode, GCodeSettings settings)
        {
            if (settings.IncludeComments)
            {
                gcode.AppendLine("; G-Code generated by GlazyxApplication");
                gcode.AppendLine($"; Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                gcode.AppendLine("; Settings:");
                gcode.AppendLine($";   Cut Feed Rate: {settings.CutFeedRate} mm/min");
                gcode.AppendLine($";   Rapid Feed Rate: {settings.RapidFeedRate} mm/min");
                gcode.AppendLine($";   Laser Power: {settings.LaserPower}");
                gcode.AppendLine($";   Scale Factor: {settings.ScaleFactor}");
                gcode.AppendLine();
            }

            // Initialize machine
            gcode.AppendLine("G21 ; Set units to millimeters");
            gcode.AppendLine("G90 ; Absolute positioning");
            gcode.AppendLine("G94 ; Units per minute feed rate mode");
            
            if (settings.UseLaserMode)
            {
                gcode.AppendLine("M3 S0 ; Enable laser mode with power off");
            }

            // Move to safe height
            gcode.AppendLine($"G0 Z{settings.SafeHeight.ToString(_culture)}");
            
            // Home position
            gcode.AppendLine("G0 X0 Y0");
            gcode.AppendLine();
        }

        private void WriteFooter(StringBuilder gcode, GCodeSettings settings)
        {
            if (settings.IncludeComments)
            {
                gcode.AppendLine("; End of program");
            }

            // Turn off laser/spindle
            if (settings.UseLaserMode)
            {
                gcode.AppendLine("M5 ; Turn off laser");
            }
            else
            {
                gcode.AppendLine("M5 ; Turn off spindle");
            }

            // Move to safe height and home
            gcode.AppendLine($"G0 Z{settings.SafeHeight.ToString(_culture)}");
            gcode.AppendLine("G0 X0 Y0");
            gcode.AppendLine("M30 ; Program end");
        }

        private void WriteObjectComment(StringBuilder gcode, IDrawableObject obj, GCodeSettings settings)
        {
            if (settings.IncludeComments)
            {
                gcode.AppendLine($"; Processing object: {obj.Name} (Type: {obj.GetType().Name})");
                gcode.AppendLine($"; Position: X{obj.Position.X.ToString(_culture)} Y{obj.Position.Y.ToString(_culture)}");
            }
        }

        private void ProcessDrawableObject(StringBuilder gcode, IDrawableObject obj, GCodeSettings settings)
        {
            var points = obj.GetGeometryPoints().ToList();
            if (!points.Any()) return;

            ProcessPointSequence(gcode, points, settings);
        }

        private void ProcessPointSequence(StringBuilder gcode, IEnumerable<Point2D> points, GCodeSettings settings)
        {
            var pointList = points.ToList();
            if (!pointList.Any()) return;

            bool isFirstPoint = true;

            foreach (var point in pointList)
            {
                var scaledPoint = new Point2D(
                    (point.X * settings.ScaleFactor) + settings.XOffset,
                    (point.Y * settings.ScaleFactor) + settings.YOffset
                );

                string x = scaledPoint.X.ToString($"F{settings.DecimalPlaces}", _culture);
                string y = scaledPoint.Y.ToString($"F{settings.DecimalPlaces}", _culture);

                if (isFirstPoint)
                {
                    // Rapid move to start position
                    gcode.AppendLine($"G0 X{x} Y{y}");
                    
                    // Lower to work height
                    gcode.AppendLine($"G0 Z{settings.WorkHeight.ToString(_culture)}");
                    
                    // Start cutting
                    if (settings.UseLaserMode)
                    {
                        gcode.AppendLine($"M3 S{settings.LaserPower}");
                    }
                    else
                    {
                        gcode.AppendLine($"M3 S{settings.LaserPower}");
                    }
                    
                    isFirstPoint = false;
                }
                else
                {
                    // Cutting move
                    gcode.AppendLine($"G1 X{x} Y{y} F{settings.CutFeedRate.ToString(_culture)}");
                }
            }

            // Stop cutting
            if (settings.UseLaserMode)
            {
                gcode.AppendLine("M5 ; Turn off laser");
            }
            else
            {
                gcode.AppendLine("M5 ; Turn off spindle");
            }

            // Lift to safe height
            gcode.AppendLine($"G0 Z{settings.SafeHeight.ToString(_culture)}");
        }

        private Point2D ParsePosition(string gcodeLine, Point2D currentPosition)
        {
            double x = currentPosition.X;
            double y = currentPosition.Y;

            // Parse X coordinate
            var xMatch = Regex.Match(gcodeLine, @"X([-+]?\d*\.?\d+)", RegexOptions.IgnoreCase);
            if (xMatch.Success)
            {
                double.TryParse(xMatch.Groups[1].Value, CultureInfo.InvariantCulture, out x);
            }

            // Parse Y coordinate
            var yMatch = Regex.Match(gcodeLine, @"Y([-+]?\d*\.?\d+)", RegexOptions.IgnoreCase);
            if (yMatch.Success)
            {
                double.TryParse(yMatch.Groups[1].Value, CultureInfo.InvariantCulture, out y);
            }

            return new Point2D(x, y);
        }

        #endregion
    }
}
