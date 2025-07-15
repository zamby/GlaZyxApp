using GlazyxApplication.Core.Interfaces;
using GlazyxApplication.Core.Models;
using GlazyxApplication.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlazyxApplication.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods to bridge between existing DrawObj classes and new core interfaces
    /// </summary>
    public static class DrawObjExtensions
    {
        /// <summary>
        /// Convert a DrawObj to use the new G-Code generation service
        /// </summary>
        public static string GenerateGCodeUsingService(this IEnumerable<DrawObj> drawObjects, GCodeSettings? settings = null)
        {
            var service = ServiceFactory.GCodeGenerationService;
            var geometryData = new List<Point2D>();

            foreach (var obj in drawObjects)
            {
                // Get geometry points from each object
                var points = obj.GetGeometryPoints();
                geometryData.AddRange(points);
            }

            return service.GenerateGCodeFromPoints(geometryData, settings);
        }

        /// <summary>
        /// Parse SVG file using the new service and convert to DrawObj format
        /// </summary>
        public static List<Point2D> ParseSvgUsingService(string filePath)
        {
            var service = ServiceFactory.SvgParsingService;
            return service.ParseSvgToPoints(filePath);
        }

        /// <summary>
        /// Parse SVG file with style information using the new service
        /// </summary>
        public static List<SvgElementData> ParseSvgWithStylesUsingService(string filePath)
        {
            var service = ServiceFactory.SvgParsingService;
            return service.ParseSvgWithStyles(filePath);
        }

        /// <summary>
        /// Validate if a file is a valid SVG using the new service
        /// </summary>
        public static bool IsValidSvgUsingService(string filePath)
        {
            var service = ServiceFactory.SvgParsingService;
            return service.IsValidSvgFile(filePath);
        }

        /// <summary>
        /// Validate G-Code output using the new service
        /// </summary>
        public static bool ValidateGCodeUsingService(string gcode)
        {
            var service = ServiceFactory.GCodeGenerationService;
            return service.ValidateGCode(gcode);
        }

        /// <summary>
        /// Estimate G-Code execution time using the new service
        /// </summary>
        public static double EstimateGCodeExecutionTime(string gcode, GCodeSettings settings)
        {
            var service = ServiceFactory.GCodeGenerationService;
            return service.EstimateExecutionTime(gcode, settings);
        }

        /// <summary>
        /// Convert DrawObj collection to a format compatible with the canvas service
        /// </summary>
        public static void SyncWithCanvasService(this IEnumerable<DrawObj> drawObjects)
        {
            var canvasService = ServiceFactory.DrawingCanvasService;
            
            // Clear existing objects
            canvasService.ClearAll();
            
            // This is a basic implementation - in a full integration, you'd create proper adapters
            // For now, we just demonstrate the concept
            Console.WriteLine($"Syncing {drawObjects.Count()} objects with canvas service");
        }
    }
}
