namespace Wice.Samples.Gallery.Samples.Misc.Focus;

public class FocusSample : Sample
{
    public override string Description => "Four button. Press TAB key to change focus.";

    public override void Layout(Visual parent)
    {
        for (var i = 0; i < 4; i++)
        {
            var btn = new Button { Margin = parent.Window!.DipsToPixels(5) };
            btn.FocusedChanged += (s, e) => { btn.Text.Text = e.Value ? "Focused" : "Unfocused"; };
            btn.Text.Text = "Unfocused";
            Dock.SetDockType(btn, DockType.Top); // remove from display
            parent.Children.Add(btn);

            // focus the first one
            if (i == 0)
            {
                btn.Focus();
            }
        }
    }
}
