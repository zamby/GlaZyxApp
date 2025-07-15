using GlazyxApplication.Core.Models;
using System.Collections.Generic;

namespace GlazyxApplication.Core.Interfaces
{
    /// <summary>
    /// Interface for G-Code generation services
    /// </summary>
    public interface IGCodeGenerationService
    {
        /// <summary>
        /// Generate G-Code from a collection of drawable objects
        /// </summary>
        /// <param name="objects">Collection of objects to convert to G-Code</param>
        /// <param name="settings">G-Code generation settings</param>
        /// <returns>Generated G-Code as string</returns>
        string GenerateGCode(IEnumerable<IDrawableObject> objects, GCodeSettings? settings = null);

        /// <summary>
        /// Generate G-Code from a collection of points
        /// </summary>
        /// <param name="points">Points to convert to G-Code</param>
        /// <param name="settings">G-Code generation settings</param>
        /// <returns>Generated G-Code as string</returns>
        string GenerateGCodeFromPoints(IEnumerable<Point2D> points, GCodeSettings? settings = null);

        /// <summary>
        /// Validate G-Code output
        /// </summary>
        /// <param name="gcode">G-Code string to validate</param>
        /// <returns>True if G-Code is valid</returns>
        bool ValidateGCode(string gcode);

        /// <summary>
        /// Estimate execution time for G-Code
        /// </summary>
        /// <param name="gcode">G-Code to analyze</param>
        /// <param name="settings">Settings used for time calculation</param>
        /// <returns>Estimated execution time in seconds</returns>
        double EstimateExecutionTime(string gcode, GCodeSettings settings);
    }

    /// <summary>
    /// Configuration settings for G-Code generation
    /// </summary>
    public class GCodeSettings
    {
        /// <summary>
        /// Feed rate for cutting movements (mm/min)
        /// </summary>
        public double CutFeedRate { get; set; } = 1000;

        /// <summary>
        /// Feed rate for rapid movements (mm/min)
        /// </summary>
        public double RapidFeedRate { get; set; } = 3000;

        /// <summary>
        /// Laser power level (0-255 or percentage)
        /// </summary>
        public int LaserPower { get; set; } = 255;

        /// <summary>
        /// Height to lift tool during rapid movements
        /// </summary>
        public double SafeHeight { get; set; } = 5.0;

        /// <summary>
        /// Working height for cutting
        /// </summary>
        public double WorkHeight { get; set; } = 0.0;

        /// <summary>
        /// Use laser mode (M3/M5) instead of spindle mode
        /// </summary>
        public bool UseLaserMode { get; set; } = true;

        /// <summary>
        /// Add comments to generated G-Code
        /// </summary>
        public bool IncludeComments { get; set; } = true;

        /// <summary>
        /// Scale factor to apply to all coordinates
        /// </summary>
        public double ScaleFactor { get; set; } = 1.0;

        /// <summary>
        /// Offset to apply to all X coordinates
        /// </summary>
        public double XOffset { get; set; } = 0.0;

        /// <summary>
        /// Offset to apply to all Y coordinates
        /// </summary>
        public double YOffset { get; set; } = 0.0;

        /// <summary>
        /// Number of decimal places for coordinates
        /// </summary>
        public int DecimalPlaces { get; set; } = 3;
    }
}
