using DirectN;

namespace Wice.Samples.Gallery.Samples.Misc
{
    public class DragDropSampleList : SampleList
    {
        public override string IconText => MDL2GlyphResource.Move;
        public override string Description => "You can enable user drag-and-drop operations within by handling events. You can also implement user cut/copy/paste support and user data transfer to the Clipboard within your Windows-based applications by using simple method calls.";
        public override string SubTitle => "Support for Windows Drag & Drop";
    }
}