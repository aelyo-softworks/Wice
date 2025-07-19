namespace Wice.Samples.Gallery.Samples.Layout.UniformGrid;

public class ShapesUniformGridSample : Sample
{
    public override string Description => "A uniform grid that display colored shapes.";
    public override int SortOrder => 1;

    public override void Layout(Visual parent)
    {
        var grid = new Wice.UniformGrid { BackgroundColor = D3DCOLORVALUE.Transparent, Rows = 10 };
        grid.Columns = grid.Rows;
        parent.Children.Add(grid);
        grid.Width = parent.Window!.DipsToPixels(300);
        grid.Height = parent.Window!.DipsToPixels(300);
        Wice.Dock.SetDockType(grid, DockType.Top); // remove from display

        for (var i = 0; i < grid.Rows; i++)
        {
            for (var j = 0; j < grid.Columns; j++)
            {
                var shape = new Ellipse();
                grid.Children.Add(shape);
                var color = new D3DCOLORVALUE(0, i / (float)grid.Rows, j / (float)grid.Columns);
                shape.RenderBrush = Compositor!.CreateColorBrush(color.ToColor());
                shape.Shape!.StrokeBrush = Compositor.CreateColorBrush(color.ToColor());
                shape.Shape.StrokeThickness = parent.Window!.DipsToPixels(0.5f);
            }
        }
    }
}
