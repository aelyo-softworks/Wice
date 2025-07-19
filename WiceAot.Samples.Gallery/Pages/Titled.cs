namespace Wice.Samples.Gallery.Pages;

#if !NETFRAMEWORK
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
public abstract class Titled : Dock
{
    protected Titled()
    {
        SetDockType(this, DockType.Top);
        Title = CreateTitle();
        if (Title == null)
            throw new InvalidOperationException();

        SetDockType(Title, DockType.Top);
        Children.Add(Title);
    }

    [Category(CategoryBehavior)]
    public TextBox Title { get; }

    protected virtual TextBox CreateTitle()
    {
        var tb = new TextBox
        {
            FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_LIGHT,
            TextRenderingParameters = new TextRenderingParameters { Mode = DWRITE_RENDERING_MODE1.DWRITE_RENDERING_MODE1_NATURAL_SYMMETRIC }
        };
        return tb;
    }

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        var theme = (GalleryTheme)GetWindowTheme();
        Title.Padding = theme.TitledMargin;
        Title.FontSize = theme.TitledFontSize;
    }
}
