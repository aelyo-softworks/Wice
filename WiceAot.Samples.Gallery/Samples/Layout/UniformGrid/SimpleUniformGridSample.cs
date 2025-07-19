﻿namespace Wice.Samples.Gallery.Samples.Layout.UniformGrid;

public class SimpleUniformGridSample : Sample
{
    public override string Description => "A simple uniform grid.";

    public override void Layout(Visual parent)
    {
        var grid = new Wice.UniformGrid();
        parent.Children.Add(grid);
        grid.Columns = 2;
        grid.Rows = 2;
        grid.Width = parent.Window!.DipsToPixels(300);
        grid.Height = parent.Window!.DipsToPixels(300);
        grid.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Pink.ToColor());
        Wice.Dock.SetDockType(grid, DockType.Top); // remove from display

        var b0 = new Wice.Border();
        grid.Children.Add(b0);
        b0.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor());
        b0.Width = parent.Window!.DipsToPixels(100);
        b0.Height = parent.Window!.DipsToPixels(100);

        var b1 = new Wice.Border();
        grid.Children.Add(b1);
        b1.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());
        b1.Width = parent.Window!.DipsToPixels(50);
        b1.Height = parent.Window!.DipsToPixels(50);

        var b2 = new Wice.Border();
        grid.Children.Add(b2);
        b2.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());
        b2.Width = parent.Window!.DipsToPixels(50);
        b2.Height = parent.Window!.DipsToPixels(50);

        var b3 = new Wice.Border();
        grid.Children.Add(b3);
        b3.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor());
        b3.Width = parent.Window!.DipsToPixels(100);
        b3.Height = parent.Window!.DipsToPixels(100);
    }
}
