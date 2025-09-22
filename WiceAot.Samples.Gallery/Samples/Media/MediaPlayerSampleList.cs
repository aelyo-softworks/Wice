using Wice.Samples.Gallery.Samples.Media.MediaPlayer;

namespace Wice.Samples.Gallery.Samples.Media;

public class MediaPlayerSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Video;
    public override string Description => "You can use a MediaPlayer visual to playback video or audio content.";
    public override string SubTitle => "A visual that supports displaying audio or video content.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new MediaPlayerSample();
        }
    }
}
