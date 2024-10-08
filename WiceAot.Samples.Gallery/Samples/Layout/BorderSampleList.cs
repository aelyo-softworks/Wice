﻿using Wice.Samples.Gallery.Samples.Layout.Border;

namespace Wice.Samples.Gallery.Samples.Layout;

public class BorderSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.SwitchApps;
    public override string Description => "Use a Border control to draw a boundary line, background, or both, around another object. A Border can contain only one child object.";
    public override string SubTitle => "A container control that draws a boundary line, background or both around another object.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new BordersSample();
            yield return new RoundBorderSample();
            yield return new SimpleBorderSample();
        }
    }
}
