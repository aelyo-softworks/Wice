namespace Wice.Samples.Gallery.Pages;

public partial class SampleListVisual : Titled
{
    public SampleListVisual(SampleList sampleList)
    {
        ArgumentNullException.ThrowIfNull(sampleList);

        Title.Text = sampleList.Title;

        // description
        var tb = new TextBox
        {
            Margin = D2D_RECT_F.Thickness(0, 0, 10, 10),
            FontSize = 18,
            WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD,
            FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_LIGHT,
            Text = sampleList.Description
        };
        SetDockType(tb, DockType.Top);
        Children.Add(tb);

        var sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Children.Add(sv);

        // a dock for all sample visuals
        var dock = new Dock { Margin = D2D_RECT_F.Thickness(10, 0, 0, 0), HorizontalAlignment = Alignment.Near, VerticalAlignment = Alignment.Near };
        sv.Child = dock;

        foreach (var sample in sampleList.Samples)
        {
            var visual = new SampleVisual(sample) { Margin = D2D_RECT_F.Thickness(0, 0, 0, 10) };
            SetDockType(visual, DockType.Top);
            dock.Children.Add(visual);
        }
    }
}
