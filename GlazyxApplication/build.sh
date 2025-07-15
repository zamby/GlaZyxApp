#!/bin/bash

# Build script for GlazyxApplication on Linux
# Usage: ./build.sh [Release|Debug]

CONFIGURATION=${1:-Release}

echo "🚀 Building GlazyxApplication for Linux..."
echo "Configuration: $CONFIGURATION"

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET is not installed. Please install .NET 8.0 or later."
    echo "Visit: https://dotnet.microsoft.com/download"
    exit 1
fi

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Failed to restore dependencies!"
    exit 1
fi

# Build for Linux
echo "🔨 Building for Linux (linux-x64)..."
dotnet publish -c $CONFIGURATION -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "./publish/linux"

if [ $? -eq 0 ]; then
    echo "✅ Linux build completed successfully!"
    
    # Make the executable file executable
    if [ -f "./publish/linux/GlazyxApplication" ]; then
        chmod +x "./publish/linux/GlazyxApplication"
        echo "🔧 Execute permissions set for GlazyxApplication"
    fi
    
    echo ""
    echo "🎉 Build completed!"
    echo "📁 Executable location: ./publish/linux/GlazyxApplication"
    echo "🏃 To run: ./publish/linux/GlazyxApplication"
else
    echo "❌ Build failed!"
    exit 1
fi
