using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Windows.Window
{
    public class WindowSample : Sample
    {
        public override string Description => "A simple secondary window. All windows in the same UI thread belong to the same Wice application.";

        public override void Layout(Visual parent)
        {
            var btn = new Button();
            btn.Text.Text = "Open a new Window...";
            btn.Click += (s, e) =>
            {
                var window = new Wice.Window { Title = "Hello World" };
                window.RenderBrush = window.Compositor.CreateColorBrush(_D3DCOLORVALUE.Green.ToColor());
                window.ResizeClient(400, 400);
                window.Center();
                window.Show();

                var close = new Button();
                close.Text.Text = "Close me";
                close.Click += (_, __) =>
                {
                    window.Destroy();
                };

                window.Children.Add(close);
            };

            btn.HorizontalAlignment = Alignment.Near; // remove from display
            Dock.SetDockType(btn, DockType.Top); // remove from display
            parent.Children.Add(btn);
        }
    }
}
