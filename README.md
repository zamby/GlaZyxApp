# GlazyxApplication

A desktop drawing application built with Avalonia UI framework for Windows and Linux platforms. Designed as a specialized tool for processing SVG files and creating vector graphics optimized for laser cutting and CNC machining operations.

> **⚠️ Project Status: Refactored Architecture**  
> This project has undergone significant architectural improvements while maintaining backward compatibility. The new modular architecture provides better maintainability, testability, and extensibility. See [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md) for details.

## Features

### Core Functionality
- **Drawing Canvas**: Interactive canvas with pan and zoom capabilities for precise design work
- **Shape Creation**: Create circles, rectangles, and stars with various colors optimized for cutting
- **SVG Processing**: Import, render, and optimize SVG files for laser/CNC manufacturing
- **G-Code Export**: Convert drawings to G-Code format compatible with laser cutters and CNC machines
- **Manufacturing Ready**: Tools designed specifically for preparation of files for digital fabrication

## Getting Started

### Prerequisites

- .NET 8.0 or later
- Windows or Linux operating system

#### For Linux Users
Additional system libraries may be required. See [LINUX_README.md](LINUX_README.md) for detailed Linux installation instructions.

### Building the Application

#### Windows
```bash
# Clone the repository
git clone <repository-url>
cd GlazyxApplication

# Build and run
dotnet build
dotnet run

# Or use the build script for Windows only
.\build.ps1 -Platform windows

# Or build for both platforms
.\build.ps1 -Platform both
```

#### Linux
```bash
# Clone the repository
git clone <repository-url>
cd GlazyxApplication

# Make build script executable and build
chmod +x build.sh
./build.sh Release

# Or use dotnet directly
dotnet build
dotnet run
```

#### Cross-Platform Build from Windows
```powershell
# Build for Linux from Windows
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o "./publish/linux"

# Build for Windows
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "./publish/windows"
```

## Dependencies

- **Avalonia**: Cross-platform .NET UI framework
- **Avalonia.Controls.PanAndZoom**: Canvas pan and zoom functionality

## Usage

1. **Launch the application**
2. **Create or Import designs**: 
   - Right-click on the canvas to create circles, rectangles, or stars
   - Import existing SVG files for processing
3. **Optimize for manufacturing**: Review and adjust shapes for laser/CNC compatibility
4. **Navigate the design**: Use mouse wheel to zoom, drag to pan around the canvas
5. **Export for production**: Save your drawings as G-Code for laser cutters or CNC machines

## Contributing

Contributions are welcome! This project has a solid architectural foundation and many opportunities for enhancement.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Third-Party Licenses
- **Avalonia UI**: MIT License
- **CommunityToolkit.Mvvm**: MIT License
- **Avalonia.Controls.PanAndZoom**: MIT License

## Roadmap

### Current Version (v0.1.0 - First Draft)
- [x] Basic drawing canvas with pan/zoom
- [x] Shape creation (circles, rectangles, stars)
- [x] SVG import and rendering
- [x] G-Code export functionality
- [x] Context menu interactions

### Planned Features
- [ ] Layer support
- [ ] More shape types (polygons, bezier curves)
- [ ] Advanced SVG features support
- [ ] Machine setup configuration
- [ ] G-Code simulation and preview
- [ ] Undo/Redo functionality
- [ ] Object selection and transformation tools
- [ ] Property panels for editing objects
- [ ] Custom tool creation
- [ ] Project save/load functionality

### Future Enhancements
- [ ] Plugin system for custom tools
- [ ] Advanced CAM features
- [ ] Material library
- [ ] Cutting optimization algorithms
- [ ] 3D preview capabilities

