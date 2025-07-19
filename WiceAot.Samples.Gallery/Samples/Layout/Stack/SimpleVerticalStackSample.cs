namespace Wice.Samples.Gallery.Samples.Layout.Stack;

public class SimpleVerticalStackSample : Sample
{
    public override string Description => "A simple vertical stack containing two boxes with spacing.";
    public override int SortOrder => 1;

    public override void Layout(Visual parent)
    {
        var stack = new Wice.Stack { Spacing = new D2D_SIZE_F(parent.Window!.DipsToPixels(10), parent.Window!.DipsToPixels(10)) };
        parent.Children.Add(stack);
        Wice.Dock.SetDockType(stack, DockType.Top); // remove from display
        stack.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Green.ToColor());

        var b0 = new Wice.Border();
        stack.Children.Add(b0);
        b0.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor());
        b0.Width = parent.Window!.DipsToPixels(100);
        b0.Height = parent.Window!.DipsToPixels(100);

        var b1 = new Wice.Border();
        stack.Children.Add(b1);
        b1.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());
        b1.Width = parent.Window!.DipsToPixels(50);
        b1.Height = parent.Window!.DipsToPixels(50);
    }
}
