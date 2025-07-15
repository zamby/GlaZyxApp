using Avalonia;
using Avalonia.Media;

namespace GlazyxApplication
{
    public class Rectangle : DrawObj
    {
        public Rectangle(double w, double h, string htmlColor)
        {
            Name = "Rectangle";
            Bounds = new Rect(0, 0, w, h);
            ColorSolid = Color.Parse(htmlColor);
        }

        public override void Render(DrawingContext context)
        {
            var rect = new Rect(Position.X, Position.Y, Bounds.Width, Bounds.Height);
            context.DrawGeometry(new SolidColorBrush(ColorSolid), null, new RectangleGeometry(rect));
        }
    }
}
