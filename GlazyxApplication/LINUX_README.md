# Linux Installation and Setup Guide

## Prerequisites for Linux

### 1. Install .NET 8.0 Runtime
```bash
# Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-runtime-8.0

# Fedora/CentOS/RHEL
sudo dnf install dotnet-runtime-8.0

# Arch Linux
sudo pacman -S dotnet-runtime
```

### 2. Install Required System Libraries

```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install -y \
    libx11-6 \
    libxext6 \
    libxrender1 \
    libxtst6 \
    libxrandr2 \
    libasound2 \
    libpangocairo-1.0-0 \
    libatk1.0-0 \
    libcairo-gobject2 \
    libgtk-3-0 \
    libgdk-pixbuf2.0-0

# Fedora/CentOS/RHEL
sudo dnf install -y \
    libX11 \
    libXext \
    libXrender \
    libXtst \
    libXrandr \
    alsa-lib \
    cairo \
    gtk3

# Arch Linux
sudo pacman -S \
    libx11 \
    libxext \
    libxrender \
    libxtst \
    libxrandr \
    alsa-lib \
    cairo \
    gtk3
```

## Building from Source on Linux

### 1. Clone and Build
```bash
git clone <repository-url>
cd GlazyxApplication

# Make build script executable
chmod +x build.sh

# Build the application
./build.sh Release
```

### 2. Run the Application
```bash
# After building
./publish/linux/GlazyxApplication

# Or if running from source
dotnet run
```

## Distribution-Specific Notes

### Ubuntu/Debian
- Works on Ubuntu 20.04+ and Debian 10+
- May need additional packages: `sudo apt-get install libc6-dev`

### Fedora/RHEL/CentOS
- Works on Fedora 35+ and RHEL 8+
- SELinux may require: `sudo setsebool -P use_execmem 1`

### Arch Linux
- Install AUR helper for additional packages if needed
- May need: `sudo pacman -S base-devel`

### OpenSUSE
```bash
sudo zypper install -y \
    libX11-6 \
    libXext6 \
    libXrender1 \
    libXtst6 \
    libXrandr2 \
    alsa \
    cairo \
    gtk3
```

## Troubleshooting Linux Issues

### 1. Display Issues
If the application doesn't display correctly:
```bash
export DISPLAY=:0
./GlazyxApplication
```

### 2. Font Issues
If fonts appear incorrect:
```bash
sudo apt-get install fonts-liberation fonts-dejavu-core
```

### 3. File Dialog Issues
If file dialogs don't work:
```bash
# Install additional desktop integration
sudo apt-get install xdg-utils

# For KDE
sudo apt-get install kde-cli-tools

# For GNOME
sudo apt-get install zenity
```

### 4. Permission Issues
If G-Code export fails:
```bash
# Ensure write permissions
chmod 755 ~/Documents
```

## Performance Optimization for Linux

### 1. Enable Hardware Acceleration
```bash
# Check GPU drivers
lspci | grep VGA

# For NVIDIA
sudo apt-get install nvidia-driver-470

# For AMD
sudo apt-get install mesa-vulkan-drivers
```

### 2. Increase File Limits
Add to `/etc/security/limits.conf`:
```
* soft nofile 65536
* hard nofile 65536
```

## Desktop Integration

### Create Desktop Entry
Create `~/.local/share/applications/glazyxapplication.desktop`:
```ini
[Desktop Entry]
Name=GlazyxApplication
Comment=Drawing and G-Code Generator
Exec=/path/to/GlazyxApplication
Icon=/path/to/icon.png
Type=Application
Categories=Graphics;2DGraphics;Engineering;
```

### File Associations
To open SVG files with GlazyxApplication, add to the desktop entry:
```ini
MimeType=image/svg+xml;
```

## Known Linux Limitations

1. **File Dialogs**: May have different appearance depending on desktop environment
2. **System Tray**: Not available on all Linux desktop environments
3. **Window Decorations**: May vary between window managers

## Testing Checklist for Linux

- [ ] Application starts without errors
- [ ] Canvas renders correctly
- [ ] Pan and zoom work smoothly  
- [ ] Context menu appears on right-click
- [ ] All shape creation works (Circle, Rectangle, Star)
- [ ] SVG file loading works
- [ ] G-Code export creates valid files
- [ ] File dialogs open correctly
- [ ] Application closes properly
