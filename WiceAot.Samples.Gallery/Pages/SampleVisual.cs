namespace Wice.Samples.Gallery.Pages;

public partial class SampleVisual : Dock
{
    private readonly CodeBox _codeBox = new();
    private readonly TextBox _description = new();
    private readonly Border _border = new();

    public SampleVisual(Sample sample)
    {
        ExceptionExtensions.ThrowIfNull(sample, nameof(sample));

        var desc = sample.Description.Nullify();
        if (desc != null)
        {
            _description.Text = desc;
            SetDockType(_description, DockType.Top);
            Children.Add(_description);
        }

        _border.BorderBrush = new SolidColorBrush(D3DCOLORVALUE.LightGray);
        SetDockType(_border, DockType.Top);
        Children.Add(_border);

        var dock = new Dock();
        _border.Children.Add(dock);

        DoWhenAttachedToComposition(() =>
        {
            sample.Compositor = Compositor;
            sample.Window = Window;
            sample.Layout(dock);

            var text = sample.GetSampleText();
            if (text != null)
            {
                _codeBox.Options |= TextHostOptions.WordWrap;
                _codeBox.CodeLanguage = WiceLanguage.Default.Id; // init & put in repo
                SetDockType(_codeBox, DockType.Top);
                _codeBox.CodeText = text;
                dock.Children.Add(_codeBox);
            }
        });
    }

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        var theme = (GalleryTheme)GetWindowTheme();

        _codeBox.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor());

        update();
        void update()
        {
            _description.Margin = theme.SampleVisualDescriptionMargin;
            _description.FontSize = theme.SampleVisualFontSize;
            _border.Padding = theme.SampleVisualBorderPadding;
            _border.BorderThickness = theme.SampleVisualBorderThickness;
            _codeBox.Margin = theme.SampleVisualCodeBoxMargin;
            _codeBox.Padding = theme.SampleVisualCodeBoxPadding;
        }

        Window!.ThemeDpiChanged += (s, e) => update();
    }
}
