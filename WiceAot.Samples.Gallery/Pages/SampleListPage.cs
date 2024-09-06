namespace Wice.Samples.Gallery.Pages;

public abstract class SampleListPage : Page
{
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
