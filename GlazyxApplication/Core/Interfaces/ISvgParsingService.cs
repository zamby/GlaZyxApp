using GlazyxApplication.Core.Models;
using System.Collections.Generic;

namespace GlazyxApplication.Core.Interfaces
{
    /// <summary>
    /// Interface for SVG file parsing and processing services
    /// </summary>
    public interface ISvgParsingService
    {
        /// <summary>
        /// Parse an SVG file and return basic point data
        /// </summary>
        /// <param name="filePath">Path to the SVG file</param>
        /// <returns>List of points representing the SVG shapes</returns>
        List<Point2D> ParseSvgToPoints(string filePath);

        /// <summary>
        /// Parse an SVG file with full style and element information
        /// </summary>
        /// <param name="filePath">Path to the SVG file</param>
        /// <returns>List of SVG elements with style information</returns>
        List<SvgElementData> ParseSvgWithStyles(string filePath);

        /// <summary>
        /// Parse SVG path data string into points
        /// </summary>
        /// <param name="pathData">SVG path d attribute value</param>
        /// <returns>List of points representing the path</returns>
        List<Point2D> ParseSvgPath(string pathData);

        /// <summary>
        /// Validate if a file is a valid SVG
        /// </summary>
        /// <param name="filePath">Path to the file to validate</param>
        /// <returns>True if the file is a valid SVG</returns>
        bool IsValidSvgFile(string filePath);
    }

    /// <summary>
    /// Represents an SVG element with style and geometric data
    /// </summary>
    public struct SvgElementData
    {
        public List<Point2D> Points { get; set; }
        public SvgStyle Style { get; set; }
        public string ElementType { get; set; }

        public SvgElementData()
        {
            Points = new List<Point2D>();
            Style = new SvgStyle();
            ElementType = "path";
        }
    }

    /// <summary>
    /// Represents SVG style information
    /// </summary>
    public struct SvgStyle
    {
        public ColorInfo FillColor { get; set; }
        public ColorInfo StrokeColor { get; set; }
        public double StrokeWidth { get; set; }
        public bool HasFill { get; set; }
        public bool HasStroke { get; set; }

        public SvgStyle()
        {
            FillColor = ColorInfo.Black;
            StrokeColor = ColorInfo.Black;
            StrokeWidth = 1.0;
            HasFill = true;
            HasStroke = false;
        }
    }

    /// <summary>
    /// Platform-independent color representation
    /// </summary>
    public readonly struct ColorInfo
    {
        public byte A { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public ColorInfo(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public ColorInfo(byte r, byte g, byte b) : this(255, r, g, b) { }

        public static ColorInfo Black => new(0, 0, 0);
        public static ColorInfo White => new(255, 255, 255);
        public static ColorInfo Red => new(255, 0, 0);
        public static ColorInfo Green => new(0, 255, 0);
        public static ColorInfo Blue => new(0, 0, 255);
        public static ColorInfo Transparent => new(0, 0, 0, 0);

        public override string ToString() => $"Color[A={A}, R={R}, G={G}, B={B}]";
    }
}
