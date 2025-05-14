using System.Reflection;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Samples.Gallery.Pages
{
    public class HomePage : Page
    {
        public HomePage()
        {
            // home has no title
            Title.IsVisible = false;

            // add a rich text box in a scroll viewer
            var sv = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
            sv.Viewer.IsWidthUnconstrained = false;
            Children.Add(sv);

            var rtb = new RichTextBox
            {
                Padding = D2D_RECT_F.Thickness(0, 0, 20, 0),
                VerticalAlignment = Alignment.Near,
                DisposeOnDetachFromComposition = false,
            };
            rtb.Options |= TextHostOptions.WordWrap;
            SetDockType(rtb, DockType.Top);

            // load from this assembly's resource
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program).Namespace + ".Resources.wice.rtf"))
            {
                // Document is an ITextDocument(2) and supports IDispatch (usable with C#'s dynamic)
                // https://docs.microsoft.com/en-us/windows/win32/api/tom/nn-tom-itextdocument

                // ITextDocument.Open supports a VARIANT of type IStream, we use ManagedIStream to handle this
                const int CP_UNICODE = 1200;

                // we must force wrap it as IUnknown because for some reason, if it's in an outside assembly (DirectN.dll here)
                // ManagedIStream is wrapped as IDispatch and this causes failure in dynamic DLR code
                // "COMException: Cannot marshal 'parameter #1': Invalid managed/unmanaged type combination"
                var unk = new UnknownWrapper(new ManagedIStream(stream));
                rtb.Document.Open(unk, 0, CP_UNICODE);
            }

            sv.Viewer.Child = rtb;
        }

        public override string IconText => MDL2GlyphResource.Home;
        public override int SortOrder => 0;
    }
}
