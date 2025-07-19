namespace Wice.Samples.Gallery.Pages;

public partial class SampleListVisual : Titled
{
    private readonly TextBox _description = new();
    private readonly Dock _dock = new();
    private readonly List<SampleVisual> _sampleVisuals = [];

    public SampleListVisual(SampleList sampleList)
    {
        ExceptionExtensions.ThrowIfNull(sampleList, nameof(sampleList));

        Title.Text = sampleList.Title;

        _description.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
        _description.FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_LIGHT;
        _description.Text = sampleList.Description;
        SetDockType(_description, DockType.Top);
        Children.Add(_description);

        var sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Children.Add(sv);

        // a dock for all sample visuals
        _dock.HorizontalAlignment = Alignment.Near;
        _dock.VerticalAlignment = Alignment.Near;
        sv.Child = _dock;

        foreach (var sample in sampleList.Samples)
        {
            var visual = new SampleVisual(sample);
            SetDockType(visual, DockType.Top);
            _dock.Children.Add(visual);
            _sampleVisuals.Add(visual);
        }
    }

    protected override void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        base.OnThemeDpiEvent(sender, e);
        var theme = (GalleryTheme)GetWindowTheme();

        _dock.Margin = theme.SampleListVisualDockMargin;
        _description.FontSize = theme.SampleListVisualFontSize;
        var margin = theme.SampleListVisualMargin;
        _description.Margin = margin;
        margin.right = 0;
        foreach (var visual in _sampleVisuals)
        {
            visual.Margin = margin;
        }
    }
}
