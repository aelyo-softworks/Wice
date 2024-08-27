namespace Wice.Samples.Gallery.Samples.Text.RichTextBox;

public partial class DocumentRichTextBoxSample : RichTextBoxSample
{
    public override string Description => "A rich text box filled from an RTF stream.";
    public override int SortOrder => 1;

    public override void Layout(Visual parent)
    {
        parent.Children.Add(Rtb);
        Dock.SetDockType(Rtb, DockType.Top);

        Rtb.MaxWidth = 500;
        Rtb.MaxHeight = 400;
        Rtb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor());
        Rtb.Padding = D2D_RECT_F.Thickness(10);
        Rtb.Margin = D2D_RECT_F.Thickness(10);

        // Document is a COM IDispatch object. Cf https://docs.microsoft.com/en-us/windows/win32/api/tom/nf-tom-itextdocument-open

        // ITextDocument.Open supports a VARIANT of type IStream, we use ManagedIStream to handle this
        const int CP_UNICODE = 1200;

        // load text from this assembly's resources
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.wice.rtf")!;
        using var mis = new ManagedIStream(stream);
        var unk = ComObject.ComWrappers.GetOrCreateComInterfaceForObject(mis, CreateComInterfaceFlags.None);
        using var v = new Variant(unk);
        Rtb.Document!.Object.Open(v.Detached, 0, CP_UNICODE);
    }
}
