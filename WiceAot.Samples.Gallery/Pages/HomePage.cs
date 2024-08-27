namespace Wice.Samples.Gallery.Pages;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public partial class HomePage : Page, IDisposable
{
    private readonly RichTextBox _rtb = new();

    public HomePage()
    {
        // home has no title
        Title.IsVisible = false;

        // add a rich text box in a scroll viewer
        var sv = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        sv.Viewer.IsWidthUnconstrained = false;
        Children.Add(sv);

        _rtb.VerticalAlignment = Alignment.Near;
        _rtb.Options |= TextHostOptions.WordWrap;
        SetDockType(_rtb, DockType.Top);

        // load from this assembly's resource
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program).Namespace + ".Resources.wice.rtf")!)
        {
            // Document is an ITextDocument(2) and supports IDispatch (usable with C#'s dynamic)
            // https://docs.microsoft.com/en-us/windows/win32/api/tom/nn-tom-itextdocument

            // ITextDocument.Open supports a VARIANT of type IStream, we use ManagedIStream to handle this
            //const int CP_UNICODE = 1200;

            //using var mis = new ManagedIStream(stream);
            //var unk = ComObject.GetOrCreateComInstance<IStream>(mis);
            //using var v = new Variant(unk);
            //_rtb.Document!.Object.Open(v.Detached, 0, CP_UNICODE);
        }

        sv.Viewer.Child = _rtb;
    }

    public override string IconText => MDL2GlyphResource.Home;
    public override int SortOrder => 0;

    public void Dispose() => _rtb?.Dispose();
}
