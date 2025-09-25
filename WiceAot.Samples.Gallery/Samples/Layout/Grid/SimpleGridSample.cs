namespace Wice.Samples.Gallery.Samples.Layout.Grid;

public class SimpleGridSample : Sample
{
    public override string Description => "A simple grid.";

    public override void Layout(Visual parent)
    {
        var grid = new Wice.Grid
        {
            Height = parent.Window!.DipsToPixels(100),
            Width = parent.Window!.DipsToPixels(1000)
        };
        parent.Children.Add(grid);
        Wice.Dock.SetDockType(grid, DockType.Top); // remove from display

        // add 2 columns and 2 rows
        grid.Columns.Add(new GridColumn());
        grid.Columns.Add(new GridColumn());
        grid.Rows.Add(new GridRow());
        grid.Rows.Add(new GridRow());

        var cell00 = new TextBox() { Text = "Cell 0,0" };
        grid.Children.Add(cell00);
        cell00.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Purple.ToColor());

        var cell22 = new TextBox()
        {
            Text = "Cell 2,2, spanning 2 rows, 2 columns",
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_FAR,
            Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_TRAILING
        };
        grid.Children.Add(cell22);
        cell22.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.LightBlue.ToColor());
        Wice.Grid.SetColumn(cell22, 1);
        Wice.Grid.SetRow(cell22, 1);
        Wice.Grid.SetColumnSpan(cell22, 2);
        Wice.Grid.SetRowSpan(cell22, 2);

        var cell11 = new TextBox() { Text = "Cell 1,1, overwriting cell 2,2", Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER };
        grid.Children.Add(cell11);
        cell11.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Pink.ToColor());

        Wice.Grid.SetColumn(cell11, 1);
        Wice.Grid.SetRow(cell11, 1);

        var cellSpanRows = new TextBox() { Text = "Cell 0,1 spanning 2 rows", ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER };
        grid.Children.Add(cellSpanRows);
        cellSpanRows.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Orange.ToColor());
        Wice.Grid.SetColumn(cellSpanRows, 0);
        Wice.Grid.SetRow(cellSpanRows, 1);
        Wice.Grid.SetRowSpan(cellSpanRows, 2);

        var cellSpanCols = new TextBox() { Text = "Cell 1,0 spanning 2 columns", ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER };
        grid.Children.Add(cellSpanCols);
        cellSpanCols.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());
        Wice.Grid.SetColumn(cellSpanCols, 1);
        Wice.Grid.SetRow(cellSpanCols, 0);
        Wice.Grid.SetColumnSpan(cellSpanCols, 2);
    }
}
