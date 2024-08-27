namespace Wice.Samples.Gallery.Samples.Input.Button;

public class SimpleButtonSample : Sample
{
    public override string Description => "A simple button";

    public override void Layout(Visual parent)
    {
        var btn = new Wice.Button();
        btn.Click += (s, e) => MessageBox.Show(parent.Window!, "You clicked me!");
        btn.Text.Text = "Click Me!";
        btn.HorizontalAlignment = Alignment.Near; // remove from display
        Dock.SetDockType(btn, DockType.Top); // remove from display
        parent.Children.Add(btn);
    }
}
