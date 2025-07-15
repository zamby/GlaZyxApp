using GlazyxApplication.Tests;
using System;

namespace GlazyxApplication.Testing
{
    /// <summary>
    /// Simple test console application for verifying core services
    /// </summary>
    class TestRunner
    {
        /// <summary>
        /// Test entry point - can be called from Program.cs for debugging
        /// </summary>
        public static void RunTests()
        {
            try
            {
                Console.WriteLine("Starting GlazyxApplication Core Services Tests...\n");
                ServiceTests.RunBasicTests();
                Console.WriteLine("\nTests completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test execution failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
