namespace Wice.Samples.Gallery.Samples.Layout.Stack;

public class BoxesStackSample : Sample
{
    public override string Description => "A fixed-size stack containing two boxes.";
    public override int SortOrder => 2;

    public override void Layout(Visual parent)
    {
        var stack = new Wice.Stack();
        parent.Children.Add(stack);
        stack.Width = parent.Window!.DipsToPixels(150);
        stack.Width = parent.Window!.DipsToPixels(150);
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
