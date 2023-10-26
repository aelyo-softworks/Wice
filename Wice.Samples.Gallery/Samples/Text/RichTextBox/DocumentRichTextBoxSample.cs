using System.Reflection;
using System.Runtime.InteropServices;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Text.RichTextBox
{
    public class DocumentRichTextBoxSample : RichTextBoxSample
    {
        public override string Description => "A rich text box filled from an RTF stream.";
        public override int SortOrder => 1;

        public override void Layout(Visual parent)
        {
            parent.Children.Add(Rtb);
            Dock.SetDockType(Rtb, DockType.Top);

            Rtb.MaxWidth = 500;
            Rtb.MaxHeight = 400;
            Rtb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White.ToColor());
            Rtb.Padding = D2D_RECT_F.Thickness(10);
            Rtb.Margin = D2D_RECT_F.Thickness(10);

            // Document is a COM IDispatch object. Cf https://docs.microsoft.com/en-us/windows/win32/api/tom/nf-tom-itextdocument-open
            dynamic doc = Rtb.Document;

            // load text from this assembly's resources
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.wice.rtf"))
            {
                // we must force wrap it as IUnknown because for some reason, if it's in an outside assembly (DirectN.dll here)
                // ManagedIStream is wrapped as IDispatch and this causes failure in dynamic DLR code
                // "COMException: Cannot marshal 'parameter #1': Invalid managed/unmanaged type combination"
                var unk = new UnknownWrapper(new ManagedIStream(stream));
                doc.Open(unk, 0, 1200); // Flags = 0, CodePage = 1200 (unicode)
            }
        }
    }
}
