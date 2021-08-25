using DirectN;

namespace Wice.Samples.Gallery.Samples.Windows
{
    public class PopupWindowSampleList : SampleList
    {
        public override string IconText => MDL2GlyphResource.Pictures;
        public override string Description => "PopupWindow visuals are of the Window type (unlike Popup visual). They are the base visual for ToolTips.";
        public override string SubTitle => "A low-level construct visual of type Window, used for tooltips and other windows that appear outside an application's main window.";
    }
}
