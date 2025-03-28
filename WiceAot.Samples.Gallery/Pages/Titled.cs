namespace Wice.Samples.Gallery.Pages;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public abstract class Titled : Dock
{
    protected Titled()
    {
        SetDockType(this, DockType.Top);
        Title = CreateTitle();
        if (Title == null)
            throw new InvalidOperationException();

        SetDockType(Title, DockType.Top);
        Title.Margin = D2D_RECT_F.Thickness(0, 0, 0, 20);
        Children.Add(Title);
    }

    [Category(CategoryBehavior)]
    public TextBox Title { get; }

    protected virtual TextBox CreateTitle()
    {
        var tb = new TextBox
        {
            FontSize = 26,
            FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_LIGHT,
            TextRenderingParameters = new TextRenderingParameters { Mode = DWRITE_RENDERING_MODE1.DWRITE_RENDERING_MODE1_NATURAL_SYMMETRIC }
        };
        return tb;
    }
}
