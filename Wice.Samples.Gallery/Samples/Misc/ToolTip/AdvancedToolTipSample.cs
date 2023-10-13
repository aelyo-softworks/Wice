using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Misc.ToolTip
{
    public class AdvancedToolTipSample : Sample
    {
        public override string Description => "A button with a custom tooltip.";
        public override int SortOrder => 1;

        public override void Layout(Visual parent)
        {
            var btn = new Button();
            btn.Text.Text = "Hover me!";
            btn.HorizontalAlignment = Alignment.Near; // remove from display
            Dock.SetDockType(btn, DockType.Top); // remove from display
            parent.Children.Add(btn);

            // a tooltip content is created using a function
            btn.ToolTipContentCreator = tt => createToolTipContent(tt, "hello world");

            // here is a custom tooltip creator function
            void createToolTipContent(Wice.ToolTip tt, string text)
            {
                // clear default tooltip shadow
                tt.Content.RenderShadow = null;

                // note we must use the tooltip's compositor, not our window's compositor
                // as the tooltip has its own window
                tt.Content.RenderBrush = tt.Compositor.CreateColorBrush(_D3DCOLORVALUE.Red.ToColor());

                var tb = new TextBox();
                tb.Margin = 4;
                tb.Text = text;
                tb.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.White);
                tb.FontSize = 25;
                tt.Content.Children.Add(tb);
            }
        }
    }
}
