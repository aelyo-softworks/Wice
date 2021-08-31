using System.IO;
using System.Reflection;
using DirectN;

namespace Wice.Samples.Gallery.Samples.Text.TextBox
{
    public class UnicodeTextBoxSample : Sample
    {
        public override string Description => "A non editable text box that demonstrates the use of chinese characters.";
        public override int SortOrder => 3;

        public override void Layout(Visual parent)
        {
            var sv = new ScrollViewer();
            sv.Height = 200;
            sv.Width = 500;
            parent.Children.Add(sv);
            Dock.SetDockType(sv, DockType.Top);

            var tb = new Wice.TextBox();
            sv.Viewer.Child = tb;
            tb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.CornflowerBlue);
            tb.Padding = D2D_RECT_F.Thickness(10);
            tb.Margin = D2D_RECT_F.Thickness(10);

            tb.Text = getLorem();

            string getLorem()
            {
                // load text from this assembly's resources
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.lorem-cn.txt"))
                {
                    var br = new StreamReader(stream);
                    return br.ReadToEnd();
                }
            }
        }
    }
}
