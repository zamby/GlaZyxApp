using GlazyxApplication.Infrastructure;
using GlazyxApplication.Infrastructure.Extensions;
using GlazyxApplication.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlazyxApplication.Examples
{
    /// <summary>
    /// Examples showing how to integrate the new services with existing code
    /// </summary>
    public static class ServiceIntegrationExamples
    {
        /// <summary>
        /// Example: Generate G-Code from existing DrawObj collection using new service
        /// </summary>
        public static void ExampleGenerateGCodeFromDrawObjects()
        {
            Console.WriteLine("=== G-Code Generation Example ===");

            // Simulate having some existing DrawObj instances
            var drawObjects = new List<DrawObj>();
            // In real usage, these would be Circle, Rectangle, Star, etc.
            // drawObjects.Add(new Circle { Name = "Test Circle", Position = new Point(50, 50) });

            // Use extension method to generate G-Code with new service
            var settings = new GCodeSettings
            {
                CutFeedRate = 1500,
                RapidFeedRate = 3000,
                LaserPower = 200,
                IncludeComments = true,
                ScaleFactor = 1.0
            };

            try
            {
                var gcode = drawObjects.GenerateGCodeUsingService(settings);
                Console.WriteLine($"Generated G-Code: {gcode.Length} characters");
                
                // Validate the generated G-Code
                bool isValid = DrawObjExtensions.ValidateGCodeUsingService(gcode);
                Console.WriteLine($"G-Code is valid: {isValid}");

                // Estimate execution time
                double estimatedTime = DrawObjExtensions.EstimateGCodeExecutionTime(gcode, settings);
                Console.WriteLine($"Estimated execution time: {estimatedTime:F2} seconds");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Example: Parse SVG file using new service
        /// </summary>
        public static void ExampleParseSvgFile(string svgFilePath)
        {
            Console.WriteLine($"\n=== SVG Parsing Example: {svgFilePath} ===");

            try
            {
                // Check if file is valid SVG first
                bool isValid = DrawObjExtensions.IsValidSvgUsingService(svgFilePath);
                if (!isValid)
                {
                    Console.WriteLine("File is not a valid SVG");
                    return;
                }

                // Parse basic points
                var points = DrawObjExtensions.ParseSvgUsingService(svgFilePath);
                Console.WriteLine($"Parsed {points.Count} points from SVG");

                // Parse with style information
                var elements = DrawObjExtensions.ParseSvgWithStylesUsingService(svgFilePath);
                Console.WriteLine($"Parsed {elements.Count} styled elements:");

                foreach (var element in elements.Take(5)) // Show first 5 elements
                {
                    Console.WriteLine($"  - {element.ElementType}: {element.Points.Count} points, " +
                                      $"Fill: {element.Style.HasFill}, Stroke: {element.Style.HasStroke}");
                }

                if (elements.Count > 5)
                {
                    Console.WriteLine($"  ... and {elements.Count - 5} more elements");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing SVG: {ex.Message}");
            }
        }

        /// <summary>
        /// Example: Using the canvas service for object management
        /// </summary>
        public static void ExampleCanvasServiceUsage()
        {
            Console.WriteLine("\n=== Canvas Service Example ===");

            try
            {
                var canvasService = ServiceFactory.DrawingCanvasService;
                
                // Subscribe to events
                canvasService.ObjectAdded += (sender, e) => 
                    Console.WriteLine($"Object added: {e.Object.Name}");
                
                canvasService.SelectionChanged += (sender, e) => 
                    Console.WriteLine($"Selection changed: {e.SelectedObjects.Count} selected, {e.DeselectedObjects.Count} deselected");

                // Set canvas properties
                canvasService.CanvasBounds = new Core.Models.Bounds2D(0, 0, 800, 600);
                Console.WriteLine($"Canvas size: {canvasService.CanvasBounds.Width}x{canvasService.CanvasBounds.Height}");

                // In a real implementation, you would add actual drawable objects
                // For now, just demonstrate the service capabilities
                Console.WriteLine($"Current objects: {canvasService.Objects.Count}");
                Console.WriteLine($"Selected objects: {canvasService.SelectedObjects.Count}");

                // Clear all (demonstrating the service works)
                canvasService.ClearAll();
                Console.WriteLine("Canvas cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with canvas service: {ex.Message}");
            }
        }

        /// <summary>
        /// Example: Complete workflow from SVG to G-Code
        /// </summary>
        public static void ExampleCompleteWorkflow(string svgFilePath)
        {
            Console.WriteLine($"\n=== Complete Workflow Example: SVG to G-Code ===");

            try
            {
                // Step 1: Parse SVG
                if (!DrawObjExtensions.IsValidSvgUsingService(svgFilePath))
                {
                    Console.WriteLine("Invalid SVG file");
                    return;
                }

                var svgPoints = DrawObjExtensions.ParseSvgUsingService(svgFilePath);
                Console.WriteLine($"Step 1: Parsed {svgPoints.Count} points from SVG");

                // Step 2: Generate G-Code directly from points
                var gCodeService = ServiceFactory.GCodeGenerationService;
                var settings = new GCodeSettings
                {
                    CutFeedRate = 1200,
                    LaserPower = 180,
                    ScaleFactor = 0.5, // Scale down by half
                    IncludeComments = true
                };

                var gcode = gCodeService.GenerateGCodeFromPoints(svgPoints, settings);
                Console.WriteLine($"Step 2: Generated G-Code ({gcode.Length} characters)");

                // Step 3: Validate and analyze
                bool isValid = gCodeService.ValidateGCode(gcode);
                double execTime = gCodeService.EstimateExecutionTime(gcode, settings);
                
                Console.WriteLine($"Step 3: Validation passed: {isValid}");
                Console.WriteLine($"Step 3: Estimated execution time: {execTime:F2} seconds");

                // In a real application, you would save the G-Code to a file here
                Console.WriteLine("Workflow completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Workflow failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Run all examples with sample data
        /// </summary>
        public static void RunAllExamples()
        {
            Console.WriteLine("GlazyxApplication Service Integration Examples");
            Console.WriteLine("==============================================");

            ExampleGenerateGCodeFromDrawObjects();
            
            // Try to find a sample SVG file
            var sampleSvgPaths = new[]
            {
                @"svg-samples\simple_circle.svg",
                @"svg-samples\heart.svg",
                @"..\svg-samples\simple_circle.svg"
            };

            string? foundSvg = null;
            foreach (var path in sampleSvgPaths)
            {
                if (System.IO.File.Exists(path))
                {
                    foundSvg = path;
                    break;
                }
            }

            if (foundSvg != null)
            {
                ExampleParseSvgFile(foundSvg);
                ExampleCompleteWorkflow(foundSvg);
            }
            else
            {
                Console.WriteLine("\nNo sample SVG files found for examples");
            }

            ExampleCanvasServiceUsage();

            Console.WriteLine("\n==============================================");
            Console.WriteLine("All examples completed!");
        }
    }
}
