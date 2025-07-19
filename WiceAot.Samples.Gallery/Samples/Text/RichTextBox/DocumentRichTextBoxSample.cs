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

        rtb.MaxWidth = parent.Window!.DipsToPixels(500);
        rtb.MaxHeight = parent.Window!.DipsToPixels(400);
        rtb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor());
        rtb.Padding = parent.Window!.DipsToPixels(10);
        rtb.Margin = parent.Window!.DipsToPixels(10);

        // Document is a COM IDispatch object. Cf https://docs.microsoft.com/en-us/windows/win32/api/tom/nf-tom-itextdocument-open

#if NETFRAMEWORK  // remove from display
        dynamic doc = rtb.Document;  // remove from display
         // remove from display
        // load text from this assembly's resources  // remove from display
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.wice.rtf"))  // remove from display
        { // remove from display
            // we must force wrap it as IUnknown because for some reason, if it's in an outside assembly (DirectN.dll here) // remove from display
            // ManagedIStream is wrapped as IDispatch and this causes failure in dynamic DLR code // remove from display
            // "COMException: Cannot marshal 'parameter #1': Invalid managed/unmanaged type combination" // remove from display
            var unk = new UnknownWrapper(new ManagedIStream(stream)); // remove from display
            doc.Open(unk, 0, 1200); // Flags = 0, CodePage = 1200 (unicode) // remove from display
        } // remove from display
#else // remove from display
        // ITextDocument.Open supports a VARIANT of type IStream, we use ManagedIStream to handle this
        const int CP_UNICODE = 1200;

        // load text from this assembly's resources
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wice.Samples.Gallery.Resources.wice.rtf")!;
        using var mis = new ManagedIStream(stream);
        var unk = ComObject.ComWrappers.GetOrCreateComInterfaceForObject(mis, CreateComInterfaceFlags.None);
        using var v = new Variant(unk);
        rtb.Document!.Object.Open(v.Detached, 0, CP_UNICODE);
#endif // remove from display
    }
}
