using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout.Stack
{
    public class BoxesStackSample : Sample
    {
        public override string Description => "A fixed-size stack containing two boxes.";
        public override int SortOrder => 2;

        public override void Layout(Visual parent)
        {
            var stack = new Wice.Stack();
            parent.Children.Add(stack);
            stack.Width = 150;
            stack.Width = 150;
            Wice.Dock.SetDockType(stack, DockType.Top); // remove from display
            stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green);

            var b0 = new Wice.Border();
            stack.Children.Add(b0);
            b0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            b0.Width = 100;
            b0.Height = 100;

            var b1 = new Wice.Border();
            stack.Children.Add(b1);
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            b1.Width = 50;
            b1.Height = 50;
        }
    }
}
