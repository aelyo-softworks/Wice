using System.Reflection;
using DirectN;

namespace Wice.Samples.Gallery.Pages
{
    public class HomePage : Page
    {
        public HomePage()
        {
            // home has no title
            Title.IsVisible = false;

//#if DEBUG
//            RichTextBox.Logger = Wice.Utilities.UILogger.Instance;
//#endif

            // add a rich text box in a scroll viewer
            var sv = new ScrollViewer();
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.Viewer.IsWidthUnconstrained = false;
            Children.Add(sv);

            var desc = new RichTextBox();
            desc.VerticalAlignment = Alignment.Near;
            desc.Options |= TextHostOptions.WordWrap;
            SetDockType(desc, DockType.Top);

            // load from this assembly's resource
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program).Namespace + ".Resources.wice.rtf"))
            {
                // Document is an ITextDocument(2) and supports IDispatch (usable with C#'s dynamic)
                // https://docs.microsoft.com/en-us/windows/win32/api/tom/nn-tom-itextdocument

                // ITextDocument.Open supports a VARIANT of type IStream, we use Wice's ManagedIStream to handle this
                desc.Document.Open(new ManagedIStream(stream), 0, 1200);
            }

            sv.Viewer.Child = desc;
        }

        public override string IconText => MDL2GlyphResource.Home;
        public override int SortOrder => 0;
    }
}
