using GlazyxApplication.Core.Interfaces;
using GlazyxApplication.Core.Services;

namespace GlazyxApplication.Infrastructure
{
    /// <summary>
    /// Factory for creating and managing core services
    /// </summary>
    public static class ServiceFactory
    {
        private static ISvgParsingService? _svgParsingService;
        private static IGCodeGenerationService? _gCodeGenerationService;
        private static IDrawingCanvasService? _drawingCanvasService;

        /// <summary>
        /// Get the SVG parsing service instance
        /// </summary>
        public static ISvgParsingService SvgParsingService =>
            _svgParsingService ??= new SvgParsingService();

        /// <summary>
        /// Get the G-Code generation service instance
        /// </summary>
        public static IGCodeGenerationService GCodeGenerationService =>
            _gCodeGenerationService ??= new GCodeGenerationService();

        /// <summary>
        /// Get the drawing canvas service instance
        /// </summary>
        public static IDrawingCanvasService DrawingCanvasService =>
            _drawingCanvasService ??= new DrawingCanvasService();

        /// <summary>
        /// Reset all service instances (useful for testing)
        /// </summary>
        public static void Reset()
        {
            _svgParsingService = null;
            _gCodeGenerationService = null;
            _drawingCanvasService = null;
        }
    }
}
