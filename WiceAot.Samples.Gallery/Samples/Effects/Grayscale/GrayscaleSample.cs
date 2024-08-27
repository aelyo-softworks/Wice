namespace Wice.Samples.Gallery.Samples.Effects.Grayscale;

public class GrayscaleSample : Sample
{
    public override void Layout(Visual parent)
    {
        var visual = new Visual();
        Dock.SetDockType(visual, DockType.Top); // remove from display
        parent.Children.Add(visual);

        // use the sepia effect
        var fx = new GrayscaleEffect { Source = new CompositionEffectSourceParameter("Input") };

        // build an effect graph
        var fac = Compositor!.CreateEffectFactory(fx);
        var effect = fac.CreateBrush();

        // use image as the source
        var img = getEmbeddedImageAsBrush("rainier.jpg");
        effect.SetSourceParameter("Input", img);

        // set the visual's effect
        visual.RenderBrush = effect;

        // get an image as composition brush
        CompositionSurfaceBrush getEmbeddedImageAsBrush(string name)
        {
            using var im = ResourcesUtilities.GetWicBitmapSource(Assembly.GetExecutingAssembly(), n => n.EndsWith(name))!;

            // use image size to determine visual size
            var size = im.GetSizeF();
            visual.Width = 300;
            visual.Height = visual.Width * size.height / size.width;

            // create a drawing (D2D1) surface
            var surface = Window!.CompositionDevice.CreateDrawingSurface(Wice.Utilities.Extensions.ToSize(size), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            using var dc = surface.BeginDraw();
            // we don't need to clear the surface as we redraw it completely
            using (var bmp = dc.CreateBitmapFromWicBitmap(im))
            {
                dc.DrawBitmap(bmp);
            }
            surface.EndDraw();

            // create a composition brush from the D2D1 surface
            return Compositor.CreateSurfaceBrush(surface);
        }
    }
}
