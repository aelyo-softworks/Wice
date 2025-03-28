namespace Wice;

public partial class RadioButton : StateButton, IFocusableParent
{
    public RadioButton()
    {
        AddState(new StateButtonState(false, CreateChild));
        AddState(new StateButtonState(true, CreateChild));
    }

    [Category(CategoryBehavior)]
    public new bool Value { get => (bool)base.Value!; set => base.Value = value; }

    Visual? IFocusableParent.FocusableVisual => null;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    Type IFocusableParent.FocusVisualShapeType => typeof(Ellipse);
    float? IFocusableParent.FocusOffset => null;

    public static Visual CreateDefaultTrueVisual(Compositor compositor)
    {
        ArgumentNullException.ThrowIfNull(compositor);
        var border = new Border();
        var canvas = new Canvas();
        border.Child = canvas;

        var ellipse = new Ellipse
        {
            StrokeBrush = compositor.CreateColorBrush(Application.CurrentTheme.BorderColor.ToColor()),
            StrokeThickness = Application.CurrentTheme.BorderSize / 2,
        };

        canvas.Children.Add(ellipse);

        var disk = new Ellipse
        {
            FillBrush = compositor.CreateColorBrush(Application.CurrentTheme.BorderColor.ToColor()),
            RadiusOffset = new Vector2(-Application.CurrentTheme.BorderSize * 1.2f, -Application.CurrentTheme.BorderSize * 1.2f),
        };

        canvas.Children.Add(disk);
#if DEBUG
        canvas.Name = nameof(CheckBox) + ".true";
#endif
        return border;
    }

    public static Visual CreateDefaultFalseVisual(Compositor compositor)
    {
        ArgumentNullException.ThrowIfNull(compositor);
        var ellipse = new Ellipse
        {
            StrokeBrush = compositor.CreateColorBrush(Application.CurrentTheme.BorderColor.ToColor()),
            StrokeThickness = Application.CurrentTheme.BorderSize / 2,
        };

#if DEBUG
        ellipse.Name = nameof(CheckBox) + ".false";
#endif
        return ellipse;
    }

    protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state)
    {
        if (Compositor == null)
            throw new InvalidOperationException();

        return true.Equals(state.Value) ? CreateDefaultTrueVisual(Compositor) : CreateDefaultFalseVisual(Compositor);
    }
}
