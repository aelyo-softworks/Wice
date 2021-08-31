using DirectN;

namespace Wice.Samples.Gallery.Samples.Media.Brush
{
    public class Direct2DBrushSample : Sample
    {
        public override string Description => "A Direct2D solid color brush.";
        public override int SortOrder => 1;

        public override void Layout(Visual parent)
        {
            var tb = new Wice.TextBox();
            parent.Children.Add(tb);
            Dock.SetDockType(tb, DockType.Top); // remove from display
            tb.HorizontalAlignment = Alignment.Center;
            tb.Padding = 5;
            tb.Text = "This text is white";

            // _D3DCOLORVALUE is the base DirectX color using single/float (0 => 1) ARGB.
            // redefined by Wice with .NET

            // to create a composition brush, the visual must be attached to composition
            // to be able to use the Compositor instance corresponding to its parent Window.
            tb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.MediumAquamarine);

            // in contrast, a Direct2D brush can be created off ground.
            // we need Direct2D colors when we use primitives that Windows composition doesn't support
            // such as DirectWrite, Images, etc.
            tb.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.White);
        }
    }
}
