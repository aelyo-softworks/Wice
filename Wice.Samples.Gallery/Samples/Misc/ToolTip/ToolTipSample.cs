namespace Wice.Samples.Gallery.Samples.Misc.ToolTip
{
    public class ToolTipSample : Sample
    {
        public override string Description => "A button with a simple tooltip.";

        public override void Layout(Visual parent)
        {
            var btn = new Button();
            btn.Text.Text = "Hover me!";
            btn.HorizontalAlignment = Alignment.Near; // remove from display
            Dock.SetDockType(btn, DockType.Top); // remove from display
            parent.Children.Add(btn);

            // a tooltip content is created using a function
            // Wice provides a default function for default tooltips
            btn.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, "hello world");
        }
    }
}
