using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout
{
    public class CanvasSampleList : SampleList
    {
        public override string IconText => MDL2GlyphResource.PageMarginPortraitNormal;
        public override string Header => "The Canvas provides absolute positioning of controls or content. Content is positioned relative to the Canvas using the Canvas.Top, Left Right and Bottom properties.";
        public override string Description => "A layout panel that supports absolute positioning of child elements relative to the top left corner of the canvas.";
    }
}
