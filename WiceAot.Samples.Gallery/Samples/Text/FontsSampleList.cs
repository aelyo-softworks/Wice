using Wice.Samples.Gallery.Samples.Text.Fonts;

namespace Wice.Samples.Gallery.Samples.Text;

public class FontsSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Font;
    public override string Description => "Fonts are used to display text in your app. You can use any font installed on the system or include custom fonts in your app package, including color fonts.";
    public override string SubTitle => "Any font installed on the system can be used in your app, including color fonts. You can also include custom fonts in your app package and use them.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new CustomFontSample();
        }
    }
}
