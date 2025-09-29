namespace Wice.Samples.Gallery.Samples.Layout.GridSplitter;

public class SimpleGridSplitterSample : Sample
{
    public override string Description => "A grid separated by splitters.";

    public override void Layout(Visual parent)
    {
        // a new grid already has one column and one row by default
        var grid = new Wice.Grid();
        parent.Children.Add(grid);
        Wice.Dock.SetDockType(grid, DockType.Top); // remove from display

        // add 4 columns
        grid.Columns.Add(new GridColumn());
        grid.Columns.Add(new GridColumn()); // splitter column is here
        grid.Columns.Add(new GridColumn());
        grid.Columns.Add(new GridColumn());

        var splitterColumn = grid.Columns.Count / 2;

        // add second row
        grid.Rows.Add(new GridRow());

        // add splitter column
        var splitter = new Wice.GridSplitter { RenderBrush = Compositor!.CreateColorBrush(parent.GetWindowTheme().SplitterColor.ToColor()) };
        grid.Children.Add(splitter);
        Wice.Grid.SetColumn(splitter, splitterColumn);

        var colors = new D3DCOLORVALUE[] {
            D3DCOLORVALUE.Purple, D3DCOLORVALUE.Orange, D3DCOLORVALUE.Yellow, D3DCOLORVALUE.LightBlue,
            D3DCOLORVALUE.Pink, D3DCOLORVALUE.LightGreen, D3DCOLORVALUE.LightCoral, D3DCOLORVALUE.LightCyan};

        var colIndex = 0;
        for (var row = 0; row < grid.Rows.Count; row++)
        {
            grid.Rows[row].Size = float.NaN; // auto-size rows
            for (var col = 0; col < grid.Columns.Count; col++)
            {
                if (col == splitterColumn) // skip splitter column
                    continue;

                grid.Columns[col].Size = float.NaN; // auto-size columns
                var cell = new TextBox
                {
                    Text = $"Cell {row},{col}",
                    Padding = 5,
                    Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,
                    ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,
                    BackgroundColor = colors[colIndex++]
                };
                grid.Children.Add(cell);
                Wice.Grid.SetColumn(cell, col);
                Wice.Grid.SetRow(cell, row);
            }
        }
    }
}
