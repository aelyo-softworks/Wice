using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout.Stack
{
    public class SimpleStackSample : Sample
    {
        public override string Description => "A simple stack.";

        public override void Layout(Visual parent)
        {
            var stack = new Wice.Stack();
            parent.Children.Add(stack);
            Wice.Dock.SetDockType(stack, DockType.Top); // remove from display
        }
    }
}
