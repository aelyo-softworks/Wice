namespace Wice.Samples.Gallery.Pages;

public partial class SampleListVisual : Titled
{
    private readonly TextBox _description = new();
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
        var dock = new Dock { Margin = D2D_RECT_F.Thickness(10, 0, 0, 0), HorizontalAlignment = Alignment.Near, VerticalAlignment = Alignment.Near };
        sv.Child = dock;

        foreach (var sample in sampleList.Samples)
        {
            var visual = new SampleVisual(sample);
            SetDockType(visual, DockType.Top);
            dock.Children.Add(visual);
            _sampleVisuals.Add(visual);
        }
    }

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        var theme = (GalleryTheme)GetWindowTheme();

        update();
        void update()
        {
            _description.FontSize = theme.SampleListVisualFontSize;
            var margin = theme.SampleListVisualMargin;
            _description.Margin = margin;
            margin.right = 0;
            foreach (var visual in _sampleVisuals)
            {
                visual.Margin = margin;
            }
        }

        Window!.ThemeDpiChanged += (s, e) => update();
    }
}
