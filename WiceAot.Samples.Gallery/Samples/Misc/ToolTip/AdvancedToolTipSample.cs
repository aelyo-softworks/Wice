namespace Wice.Samples.Gallery.Samples.Misc.ToolTip;

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
        static void createToolTipContent(Wice.ToolTip tt, string text)
        {
            // clear default tooltip shadow
            tt.Content.RenderShadow = null;

            // note we must use the tooltip's compositor, not our window's compositor
            // as the tooltip has its own window
            tt.Content.RenderBrush = tt.Compositor!.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());

            var tb = new TextBox
            {
                Text = text,
                ForegroundBrush = new SolidColorBrush(D3DCOLORVALUE.White),
            };
            tt.Content.Children.Add(tb);
            updateStyle();

            void updateStyle()
            {
                tb.Margin = tt.Window!.DipsToPixels(4);
                tb.FontSize = tt.Window!.DipsToPixels(25);
            }

            tt.ThemeDpiEvent += (s, e) => updateStyle();
        }
    }
}
