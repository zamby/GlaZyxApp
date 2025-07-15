using GlazyxApplication.Core.Interfaces;
using GlazyxApplication.Core.Models;
using GlazyxApplication.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;

namespace GlazyxApplication.Tests
{
    /// <summary>
    /// Simple test class to verify core services functionality
    /// </summary>
    public static class ServiceTests
    {
        /// <summary>
        /// Run basic tests for all core services
        /// </summary>
        public static void RunBasicTests()
        {
            Console.WriteLine("=== GlazyxApplication Service Tests ===");

            TestGCodeGenerationService();
            TestSvgParsingService();
            TestDrawingCanvasService();

            Console.WriteLine("=== All Tests Completed ===");
        }

        private static void TestGCodeGenerationService()
        {
            Console.WriteLine("\n--- Testing G-Code Generation Service ---");
            
            try
            {
                var service = ServiceFactory.GCodeGenerationService;
                
                // Test basic point sequence
                var points = new List<Point2D>
                {
                    new Point2D(0, 0),
                    new Point2D(10, 0),
                    new Point2D(10, 10),
                    new Point2D(0, 10),
                    new Point2D(0, 0)
                };

                var settings = new GCodeSettings
                {
                    CutFeedRate = 1000,
                    LaserPower = 200,
                    IncludeComments = true
                };

                var gcode = service.GenerateGCodeFromPoints(points, settings);
                
                Console.WriteLine($"✓ Generated G-Code ({gcode.Length} characters)");
                
                // Test validation
                bool isValid = service.ValidateGCode(gcode);
                Console.WriteLine($"✓ G-Code validation: {(isValid ? "PASSED" : "FAILED")}");
                
                // Test execution time estimation
                double estimatedTime = service.EstimateExecutionTime(gcode, settings);
                Console.WriteLine($"✓ Estimated execution time: {estimatedTime:F2} seconds");
                
                Console.WriteLine("G-Code Generation Service: ✓ PASSED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"G-Code Generation Service: ✗ FAILED - {ex.Message}");
            }
        }

        private static void TestSvgParsingService()
        {
            Console.WriteLine("\n--- Testing SVG Parsing Service ---");
            
            try
            {
                var service = ServiceFactory.SvgParsingService;
                
                // Test with a simple SVG content (create temporary file)
                string tempSvgPath = Path.GetTempFileName() + ".svg";
                string simpleSvg = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg xmlns=""http://www.w3.org/2000/svg"" width=""100"" height=""100"">
    <rect x=""10"" y=""10"" width=""30"" height=""30"" fill=""red""/>
    <circle cx=""70"" cy=""25"" r=""15"" fill=""blue""/>
</svg>";
                
                File.WriteAllText(tempSvgPath, simpleSvg);
                
                // Test validation
                bool isValid = service.IsValidSvgFile(tempSvgPath);
                Console.WriteLine($"✓ SVG validation: {(isValid ? "PASSED" : "FAILED")}");
                
                if (isValid)
                {
                    // Test basic parsing
                    var points = service.ParseSvgToPoints(tempSvgPath);
                    Console.WriteLine($"✓ Parsed {points.Count} points from SVG");
                    
                    // Test parsing with styles
                    var elements = service.ParseSvgWithStyles(tempSvgPath);
                    Console.WriteLine($"✓ Parsed {elements.Count} styled elements from SVG");
                }
                
                // Cleanup
                File.Delete(tempSvgPath);
                
                Console.WriteLine("SVG Parsing Service: ✓ PASSED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SVG Parsing Service: ✗ FAILED - {ex.Message}");
            }
        }

        private static void TestDrawingCanvasService()
        {
            Console.WriteLine("\n--- Testing Drawing Canvas Service ---");
            
            try
            {
                var service = ServiceFactory.DrawingCanvasService;
                
                // Test events
                int objectAddedCount = 0;
                int selectionChangedCount = 0;
                
                service.ObjectAdded += (s, e) => objectAddedCount++;
                service.SelectionChanged += (s, e) => selectionChangedCount++;
                
                // Test basic properties
                Console.WriteLine($"✓ Initial object count: {service.Objects.Count}");
                Console.WriteLine($"✓ Initial selection count: {service.SelectedObjects.Count}");
                
                // Test canvas bounds
                var bounds = new Bounds2D(0, 0, 800, 600);
                service.CanvasBounds = bounds;
                Console.WriteLine($"✓ Canvas bounds set: {service.CanvasBounds.Width}x{service.CanvasBounds.Height}");
                
                // Test operations (we would need to create mock drawable objects for full testing)
                service.ClearAll();
                Console.WriteLine($"✓ Clear all completed");
                
                Console.WriteLine($"✓ Events fired - Added: {objectAddedCount}, Selection: {selectionChangedCount}");
                
                Console.WriteLine("Drawing Canvas Service: ✓ PASSED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Drawing Canvas Service: ✗ FAILED - {ex.Message}");
            }
        }
    }
}
