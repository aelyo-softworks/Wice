namespace Wice.Samples.Gallery.Samples.Text.TextBox;

public class UnicodeTextBoxSample : Sample
{
    public override string Description => "A non editable text box that demonstrates the use of chinese characters.";
    public override int SortOrder => 4;

    public override void Layout(Visual parent)
    {
        var sv = new ScrollViewer { Height = parent.Window!.DipsToPixels(200), Width = parent.Window!.DipsToPixels(500) };
        parent.Children.Add(sv);
        Dock.SetDockType(sv, DockType.Top);

        var tb = new Wice.TextBox();
        sv.Viewer.Child = tb;
        tb.SelectionBrush = new SolidColorBrush(D3DCOLORVALUE.Coral);
        tb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.CornflowerBlue.ToColor());
        tb.Padding = parent.Window!.DipsToPixels(10);
        tb.Margin = parent.Window!.DipsToPixels(10);

        tb.Text = getLorem();

        string getLorem()
        {
            // load text from this assembly's resources
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.lorem-cn.txt")!;
            var br = new StreamReader(stream);
            return br.ReadToEnd();
        }
    }
}
