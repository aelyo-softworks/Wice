using Wice.Samples.Gallery.Samples.Input;

namespace Wice.Samples.Gallery.Pages;

public partial class InputPage : SampleListPage
{
    public InputPage()
    {
    }

    public override string IconText => MDL2GlyphResource.Input;
    public override int SortOrder => 1;

    protected override IEnumerable<SampleList> SampleLists
    {
        get
        {
            yield return new ButtonSampleList();
            yield return new CheckBoxSampleList();
            yield return new RadioButtonSampleList();
            yield return new StateButtonSampleList();
            yield return new ToggleSwitchSampleList();
#if !NETFRAMEWORK
            yield return new SliderSampleList();
#endif
        }
    }
}
