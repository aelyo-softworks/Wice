namespace Wice.Samples.Gallery.Samples.Layout.Dock;

public class SimpleDockSample : Sample
{
    public override string Description => "A dock with borders at corners. Note Dock is used in many other samples.";

    public override void Layout(Visual parent)
    {
        var dock = new Wice.Dock();
        dock.Width = 120;
        dock.Height = 120;
        dock.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Gray.ToColor());
        parent.Children.Add(dock);
        Wice.Dock.SetDockType(dock, DockType.Top); // remove from display

        var b0 = new Wice.Border();
        Wice.Dock.SetDockType(b0, DockType.Top);
        b0.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());
        b0.Width = 40;
        b0.Height = 40;
        dock.Children.Add(b0);

        var b1 = new Wice.Border();
        Wice.Dock.SetDockType(b1, DockType.Left);
        b1.VerticalAlignment = Alignment.Near;
        b1.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor());
        b1.Width = 40;
        b1.Height = 40;
        dock.Children.Add(b1);

        var b2 = new Wice.Border();
        Wice.Dock.SetDockType(b2, DockType.Right);
        b2.VerticalAlignment = Alignment.Near;
        b2.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Green.ToColor());
        b2.Width = 40;
        b2.Height = 40;
        dock.Children.Add(b2);

        var b3 = new Wice.Border();
        Wice.Dock.SetDockType(b3, DockType.Bottom);
        b3.VerticalAlignment = Alignment.Far;
        b3.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor());
        b3.Width = 40;
        b3.Height = 40;
        dock.Children.Add(b3);
    }
}
