namespace Wice.Samples.Gallery.Samples.Effects.Contrast;

public class ContrastSample : Sample
{
    public override void Layout(Visual parent)
    {
        var visual = new Visual();
        Dock.SetDockType(visual, DockType.Top); // remove from display
        parent.Children.Add(visual);

        var cb = new CheckBox { Margin = D2D_RECT_F.Thickness(5) };
        Dock.SetDockType(cb, DockType.Top); // remove from display
        parent.Children.Add(cb);

        var tb = new TextBox { HorizontalAlignment = Alignment.Center, Text = "Toggle Effect" };
        Dock.SetDockType(tb, DockType.Top); // remove from display
        parent.Children.Add(tb);

        // use the sepia effect
        var fx = new ContrastEffect { ClampInput = true, Contrast = 1, Source = new CompositionEffectSourceParameter("Input") };

        // build an effect graph
        var fac = Compositor!.CreateEffectFactory(fx);
        var effect = fac.CreateBrush();

        // use image as the source
        var img = getEmbeddedImageAsBrush("rainier.jpg");
        effect.SetSourceParameter("Input", img);

        // set the visual's effect
        visual.RenderBrush = effect;

        cb.Click += (s, e) =>
        {
            if (visual.RenderBrush == effect)
            {
                visual.RenderBrush = img;
            }
            else
            {
                visual.RenderBrush = effect;
            }
        };

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
