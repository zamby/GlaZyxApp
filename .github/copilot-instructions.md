# Copilot Instructions for GlazyxApplication

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This is a desktop drawing application built with Avalonia UI framework for Windows and Linux platforms. The application provides:

- Drawing canvas with pan and zoom capabilities
- Basic shapes creation (circles, rectangles, stars)
- SVG file import and rendering
- G-Code export functionality for CNC/laser cutting
- Modern English UI

## Key Components
- **CampoDraw**: Main drawing canvas control
- **DrawObj**: Base class for drawable objects
- **SVG Parser**: For importing and rendering SVG files
- **GCodeGenerator**: For exporting drawings to G-Code format
- **Drawing Objects**: Circle, Rectangle, Star, SVG shapes

## Code Style Guidelines
- Use English for all UI text, comments, and variable names
- Follow C# naming conventions
- Use modern C# features (nullable reference types, pattern matching)
- Maintain separation between UI logic and drawing logic
- Use proper error handling for file operations and SVG parsing

## Architecture Notes
- MVVM pattern with Avalonia UI
- Canvas-based drawing system
- Plugin-style object rendering
- Reflection-based SVG element processing
- Culture-invariant G-Code generation


## Development Environment
- Use Git for version control   
- Environment Windows Visual studio code editor
- Use of Avalonia's built-in controls and custom controls for drawing
- Use Dependency Injection for services like G-Code generation and SVG parsing
- Use Solid principles for class design

