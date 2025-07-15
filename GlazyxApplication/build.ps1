#!/usr/bin/env pwsh

# Build script for GlazyxApplication
# Supports Windows and Linux builds

param(
    [Parameter()]
    [ValidateSet("windows", "linux", "both")]
    [string]$Platform = "windows",
    
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

Write-Host "Building GlazyxApplication..." -ForegroundColor Green

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean

if ($Platform -eq "windows" -or $Platform -eq "both") {
    Write-Host "Building for Windows (win-x64)..." -ForegroundColor Cyan
    dotnet publish -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "./publish/windows"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Windows build completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Windows build failed!" -ForegroundColor Red
        exit 1
    }
}

if ($Platform -eq "linux" -or $Platform -eq "both") {
    Write-Host "Building for Linux (linux-x64)..." -ForegroundColor Cyan
    dotnet publish -c $Configuration -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "./publish/linux"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Linux build completed successfully!" -ForegroundColor Green
        
        # Make the Linux executable file executable
        if (Test-Path "./publish/linux/GlazyxApplication") {
            Write-Host "Setting execute permissions for Linux binary..." -ForegroundColor Yellow
            if ($IsLinux -or $IsMacOS) {
                chmod +x "./publish/linux/GlazyxApplication"
            } else {
                Write-Host "Note: Run 'chmod +x GlazyxApplication' on Linux to make the file executable" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "❌ Linux build failed!" -ForegroundColor Red
        exit 1
    }
}

Write-Host "`n🎉 Build process completed!" -ForegroundColor Green
Write-Host "Check the './publish/' directory for the built applications." -ForegroundColor Cyan
