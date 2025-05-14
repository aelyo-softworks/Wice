namespace Wice.Samples.Gallery.Pages;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public partial class HomePage : Page
{
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

        var rtb = new RichTextBox
        {
            Padding = D2D_RECT_F.Thickness(0, 0, 20, 0),
            VerticalAlignment = Alignment.Near,
            DisposeOnDetachFromComposition = false,
        };
        rtb.Options |= TextHostOptions.WordWrap;
        SetDockType(rtb, DockType.Top);

        // load from this assembly's resource
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program).Namespace + ".Resources.wice.rtf")!;
        // Document is an ITextDocument(2) and supports IDispatch (usable with C#'s dynamic)
        // https://docs.microsoft.com/en-us/windows/win32/api/tom/nn-tom-itextdocument

        // ITextDocument.Open supports a VARIANT of type IStream, we use ManagedIStream to handle this

        using var mis = new ManagedIStream(stream);
        ComObject.WithComInstanceOfType<IStream>(mis, unk =>
        {
            var v = new Variant(unk);
            rtb.Document!.Object.Open(v.Detach(), 0, (int)DXC_CP.DXC_CP_UTF16);

            sv.Viewer.Child = rtb;
        }, createIfNeeded: true);
    }

    public override string IconText => MDL2GlyphResource.Home;
    public override int SortOrder => 0;
}
