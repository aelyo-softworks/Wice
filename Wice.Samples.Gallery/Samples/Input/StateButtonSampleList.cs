using DirectN;

namespace Wice.Samples.Gallery.Samples.Input
{
    public class StateButtonSampleList : SampleList
    {
        public override string IconText => MDL2GlyphResource.ButtonX;
        public override string SubTitle => "The StateButton visual represents a button that changes state at each user input.";
        public override string Description => SubTitle + " It's also the base class for RadioButton, CheckBox and NullableCheckBox.";
    }
}
