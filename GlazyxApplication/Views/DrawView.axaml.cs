using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Diagnostics;


namespace GlazyxApplication.Views;

public partial class DrawView : UserControl
{
    private readonly ZoomBorder? _zoomBorder;
    private AreaDraw? _areaDraw;
    private Point _lastRightClickPosition;

    public DrawView()
    {
        InitializeComponent();
     
        _zoomBorder = this.Find<ZoomBorder>("ZoomBorder");
        if (_zoomBorder != null)
        {
            _zoomBorder.KeyDown += ZoomBorder_KeyDown;
            _zoomBorder.ZoomChanged += ZoomBorder_ZoomChanged;
        }

        _areaDraw = this.Find<AreaDraw>("AreaDraw");
        if (_areaDraw != null)
        {
            _areaDraw.PointerPressed += AreaDraw_PointerPressed;
        }

        // Handle context menu events
        var menuAddCircle = this.Find<MenuItem>("MenuAddCircle");
        if (menuAddCircle != null)
            menuAddCircle.Click += MenuAddCircle_Click;

        var menuAddRectangle = this.Find<MenuItem>("MenuAddRectangle");
        if (menuAddRectangle != null)
            menuAddRectangle.Click += MenuAddRectangle_Click;

        var menuAddStar = this.Find<MenuItem>("MenuAddStar");
        if (menuAddStar != null)
            menuAddStar.Click += MenuAddStar_Click;

        var menuAddSvg = this.Find<MenuItem>("MenuAddSvg");
        if (menuAddSvg != null)
            menuAddSvg.Click += MenuAddSvg_Click;

        var menuLoadSvgFile = this.Find<MenuItem>("MenuLoadSvgFile");
        if (menuLoadSvgFile != null)
            menuLoadSvgFile.Click += MenuLoadSvgFile_Click;

        var menuExportGCode = this.Find<MenuItem>("MenuExportGCode");
        if (menuExportGCode != null)
            menuExportGCode.Click += MenuExportGCode_Click;

        var menuClearAll = this.Find<MenuItem>("MenuClearAll");
        if (menuClearAll != null)
            menuClearAll.Click += MenuClearAll_Click;
    }

    private void ZoomBorder_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F:
                _zoomBorder?.Fill();
                break;
            case Key.U:
                _zoomBorder?.Uniform();
                break;
            case Key.R:
                _zoomBorder?.ResetMatrix();
                break;
            case Key.T:
                _zoomBorder?.ToggleStretchMode();
                _zoomBorder?.AutoFit();
                break;
        }
    }

    private void ZoomBorder_ZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        Debug.WriteLine($"[ZoomChanged] {e.ZoomX} {e.ZoomY} {e.OffsetX} {e.OffsetY}");
    }

    // Handle right click to store position
    private void AreaDraw_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(sender as Control).Properties.IsRightButtonPressed)
        {
            _lastRightClickPosition = e.GetPosition(sender as Control);
        }
    }

    // Context menu events
    private void MenuAddCircle_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _areaDraw?.AddCircle(_lastRightClickPosition);
    }

    private void MenuAddRectangle_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _areaDraw?.AddRectangle(_lastRightClickPosition);
    }

    private void MenuAddStar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _areaDraw?.AddStar(_lastRightClickPosition);
    }

    private void MenuAddSvg_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Adds a predefined heart shape SVG to the canvas
        _areaDraw?.AddSvg(_lastRightClickPosition);
    }

    private void MenuLoadSvgFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _areaDraw?.AddSvgFromFile(_lastRightClickPosition);
    }

    private void MenuExportGCode_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _areaDraw?.ExportToGCode();
    }

    private void MenuClearAll_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _areaDraw?.ClearAll();
    }

}
