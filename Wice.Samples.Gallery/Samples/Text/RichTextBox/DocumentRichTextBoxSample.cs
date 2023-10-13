using System.Reflection;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Text.RichTextBox
{
    public class DocumentRichTextBoxSample : Sample
    {
        public override string Description => "A rich text box filled from an RTF stream.";
        public override int SortOrder => 1;

        public override void Layout(Visual parent)
        {
            var rtb = new Wice.RichTextBox();
            parent.Children.Add(rtb);
            Dock.SetDockType(rtb, DockType.Top);

            rtb.MaxWidth = 500;
            rtb.MaxHeight = 400;
            rtb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White.ToColor());
            rtb.Padding = D2D_RECT_F.Thickness(10);
            rtb.Margin = D2D_RECT_F.Thickness(10);

            // Document is a COM IDispatch object. Cf https://docs.microsoft.com/en-us/windows/win32/api/tom/nf-tom-itextdocument-open
            dynamic doc = rtb.Document;

            // load text from this assembly's resources
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.wice.rtf"))
            {
                doc.Open(new ManagedIStream(stream), 0, 1200); // Flags = 0, CodePage = 1200 (unicode)
            }
        }
    }
}
