using Wice.Samples.Gallery.Samples.Windows.MessageBox;

namespace Wice.Samples.Gallery.Samples.Windows;

public class MessageBoxSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Favicon2;
    public override string Description => "MessageBox visuals can contain any kind of message users need or want to see as they use your app. They can present errors, warning, confirmations, or notifications in your app.";
    public override string SubTitle => "A message box is a modal dialog box that asks if the user wants to proceed with an action.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new MessageBoxSample();
        }
    }
}
