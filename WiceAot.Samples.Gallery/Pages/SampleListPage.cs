namespace Wice.Samples.Gallery.Pages;

public abstract class SampleListPage : Page
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    protected SampleListPage()
    {
        // add a wrap that holds all sample lists
        var wrap = new Wrap
        {
            Orientation = Orientation.Horizontal
        };
        foreach (var list in SampleLists.Where(s => s.IsEnabled).OrderBy(s => s.TypeName))
        {
            // use a custom button for a sample
            var btn = new SampleButton(list);
            btn.Click += (s, e) =>
            {
                var sampleListVisual = new SampleListVisual(list);
                var page = sampleListVisual;
                ((GalleryWindow)Window!).ShowPage(page);
            };
            wrap.Children.Add(btn);
        }

        Children.Add(wrap);
    }

    protected abstract IEnumerable<SampleList> SampleLists { get; }
}
