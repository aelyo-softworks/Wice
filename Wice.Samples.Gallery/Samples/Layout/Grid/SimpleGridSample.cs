using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Layout.Grid
{
    public class SimpleGridSample : Sample
    {
        public override bool IsEnabled => false; // buggy
        public override string Description => "A simple grid.";

        public override void Layout(Visual parent)
        {
            var grid = new Wice.Grid();
            grid.Height = 100;
            grid.Width = 100;
            parent.Children.Add(grid);
            //Wice.Dock.SetDockType(grid, DockType.Top); // remove from display

            // add 2 columns and 2 rows
            grid.Columns.Add(new GridColumn());
            grid.Columns.Add(new GridColumn());
            grid.Rows.Add(new GridRow());
            grid.Rows.Add(new GridRow());

            var cell0 = new Wice.Border();
            grid.Children.Add(cell0);
            cell0.Margin = 10;
            cell0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Purple.ToColor());

            var cell1 = new Wice.Border();
            grid.Children.Add(cell1);
            cell1.Margin = 10;
            cell1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink.ToColor());

            Wice.Grid.SetColumn(cell1, 2);
            Wice.Grid.SetRow(cell1, 2);
        }
    }
}
