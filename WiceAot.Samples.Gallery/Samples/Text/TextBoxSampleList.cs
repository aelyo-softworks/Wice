using Wice.Samples.Gallery.Samples.Text.TextBox;

namespace Wice.Samples.Gallery.Samples.Text;

public class TextBoxSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Rename;
    public override string Description => "Use a TextBox to let a user display or enter text input in your app. You can customize it in many ways.";
    public override string SubTitle => "A single-line or multi-line, read-only or read-write, text field. ";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new ColorFontTextBoxSample();
            yield return new EditableTextBoxSample();
            yield return new SimpleTextBoxSample();
            yield return new UnicodeTextBoxSample();
        }
    }
}
