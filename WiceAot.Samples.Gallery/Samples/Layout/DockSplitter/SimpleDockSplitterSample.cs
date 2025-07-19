﻿namespace Wice.Samples.Gallery.Samples.Layout.DockSplitter;

public class SimpleDockSplitterSample : Sample
{
    public override string Description => "A dock with yellow dock splitters you can move.";

    public override void Layout(Visual parent)
    {
        var dock = new Wice.Dock();
        parent.Children.Add(dock);
        dock.Height = parent.Window!.DipsToPixels(200);
        Wice.Dock.SetDockType(dock, DockType.Top); // remove from display

        var b1 = new Wice.Border();
        dock.Children.Add(b1);
        Wice.Dock.SetDockType(b1, DockType.Top);
        b1.MinHeight = parent.Window!.DipsToPixels(20);
        b1.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());

        var splitter1 = new Wice.DockSplitter();
        splitter1.Name = nameof(splitter1);
        dock.Children.Add(splitter1);
        splitter1.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());

        var b2 = new Wice.Border();
        dock.Children.Add(b2);
        b2.MinHeight = parent.Window!.DipsToPixels(20);
        Wice.Dock.SetDockType(b2, DockType.Bottom);
        b2.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Green.ToColor());

        var splitter2 = new Wice.DockSplitter();
        splitter2.Name = nameof(splitter2);
        dock.Children.Add(splitter2);
        splitter2.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());

        var b3 = new Wice.Border();
        dock.Children.Add(b3);
        b3.MinWidth = parent.Window!.DipsToPixels(20);
        Wice.Dock.SetDockType(b3, DockType.Left);
        b3.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor());

        var splitter3 = new Wice.DockSplitter();
        splitter3.Name = nameof(splitter3);
        dock.Children.Add(splitter3);
        splitter3.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());

        var b4 = new Wice.Border();
        dock.Children.Add(b4);
        b4.MinWidth = parent.Window!.DipsToPixels(20);
        Wice.Dock.SetDockType(b4, DockType.Right);
        b4.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Orange.ToColor());

        var splitter4 = new Wice.DockSplitter();
        splitter4.Name = nameof(splitter4);
        dock.Children.Add(splitter4);
        splitter4.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());

        var b5 = new Wice.Border();
        dock.Children.Add(b5);
        Wice.Dock.SetDockType(b5, DockType.Left);
        b5.MinWidth = parent.Window!.DipsToPixels(10);
        b5.MinHeight = parent.Window!.DipsToPixels(10);
        b5.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Violet.ToColor());
    }
}
