﻿using Wice.Samples.Gallery.Samples.Text.RichTextBox;

namespace Wice.Samples.Gallery.Samples.Text;

public class RichTextBoxSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.GiftboxOpen;
    public override string Description => "You can use a RichEditBox visual to display rich text documents (RTF) that contain formatted text, hyperlinks, and images. Currently only supports read-only mode.";
    public override string SubTitle => "A rich text editing visual that supports formatted text, hyperlinks, and other rich content. Currently only supports read-only mode.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new DocumentRichTextBoxSample();
            yield return new RtfRichTextBoxSample();
            yield return new SimpleRichTextBoxSample();
        }
    }
}
