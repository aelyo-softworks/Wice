namespace Wice.Samples.Gallery.Samples.Text.RichTextBox;

public partial class DocumentRichTextBoxSample : Sample
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
        rtb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor());
        rtb.Padding = D2D_RECT_F.Thickness(10);
        rtb.Margin = D2D_RECT_F.Thickness(10);

        // Document is a COM IDispatch object. Cf https://docs.microsoft.com/en-us/windows/win32/api/tom/nf-tom-itextdocument-open

#if NETFRAMEWORK
        dynamic doc = rtb.Document;

        // load text from this assembly's resources
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.wice.rtf"))
        {
            // we must force wrap it as IUnknown because for some reason, if it's in an outside assembly (DirectN.dll here)
            // ManagedIStream is wrapped as IDispatch and this causes failure in dynamic DLR code
            // "COMException: Cannot marshal 'parameter #1': Invalid managed/unmanaged type combination"
            var unk = new UnknownWrapper(new ManagedIStream(stream));
            doc.Open(unk, 0, 1200); // Flags = 0, CodePage = 1200 (unicode)
        }
#else
        // ITextDocument.Open supports a VARIANT of type IStream, we use ManagedIStream to handle this
        const int CP_UNICODE = 1200;

        // load text from this assembly's resources
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.wice.rtf")!;
        using var mis = new ManagedIStream(stream);
        var unk = ComObject.ComWrappers.GetOrCreateComInterfaceForObject(mis, CreateComInterfaceFlags.None);
        using var v = new Variant(unk);
        rtb.Document!.Object.Open(v.Detached, 0, CP_UNICODE);
#endif
    }
}
