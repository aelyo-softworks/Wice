namespace Wice.Samples.Gallery.Samples.Layout.GridSplitter;

public class SimpleGridSplitterSample : Sample
{
    public override string Description => "A grid separated by splitters.";

    public override void Layout(Visual parent)
    {
        // a new grid already has one column and one row by default
        var grid = new Wice.Grid
        {
            Height = parent.Window!.DipsToPixels(100),
            Width = parent.Window!.DipsToPixels(500)
        };
        parent.Children.Add(grid);
        Wice.Dock.SetDockType(grid, DockType.Top); // remove from display

        // configure first column and row (always there) to auto-size
        grid.Columns[0].Size = float.NaN;
        grid.Rows[0].Size = float.NaN;

        // add splitter column
        grid.Columns.Add(new GridColumn { Size = float.NaN });

        // add second column
        grid.Columns.Add(new GridColumn { Size = float.NaN });

        // add two rows
        grid.Rows.Add(new GridRow { Size = float.NaN });

        // add splitter
        var splitter = new Wice.GridSplitter();
        splitter.RenderBrush = Compositor!.CreateColorBrush(parent.GetWindowTheme().SplitterColor.ToColor());
        grid.Children.Add(splitter);
        Wice.Grid.SetColumn(splitter, 1);

        var cell00 = new TextBox() { Text = "Cell 0,0" };
        grid.Children.Add(cell00);
        cell00.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Purple.ToColor());
        Wice.Grid.SetColumn(cell00, 0);
        Wice.Grid.SetRow(cell00, 0);

        var cell02 = new TextBox() { Text = "Cell 0,2" };
        grid.Children.Add(cell02);
        cell02.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Orange.ToColor());
        Wice.Grid.SetColumn(cell02, 2);
        Wice.Grid.SetRow(cell02, 0);

        var cell20 = new TextBox() { Text = "Cell 2,0" };
        grid.Children.Add(cell20);
        cell20.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());
        Wice.Grid.SetColumn(cell20, 0);
        Wice.Grid.SetRow(cell20, 1);

        var cell22 = new TextBox() { Text = "Cell 2,2" };
        grid.Children.Add(cell22);
        cell22.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LightBlue.ToColor());
        Wice.Grid.SetColumn(cell22, 2);
        Wice.Grid.SetRow(cell22, 1);
    }
}
