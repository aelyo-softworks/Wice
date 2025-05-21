namespace Wice;

public partial class NullableCheckBox : StateButton
{
    public NullableCheckBox()
    {
        AddState(new StateButtonState(false, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool?>(o) == false });
        AddState(new StateButtonState(true, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool?>(o) == true });
        AddState(new StateButtonState(null, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool?>(o) == null });
        // by default, Value is null
    }

    [Category(CategoryBehavior)]
    public new bool? Value { get => (bool?)base.Value; set => base.Value = value; }

    protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state)
    {
        if (true.Equals(state.Value))
            return CheckBox.CreateDefaultTrueVisual();

        if (false.Equals(state.Value))
            return CheckBox.CreateDefaultFalseVisual();

        return CreateDefaultNullVisual();
    }

    public static Visual CreateDefaultNullVisual()
    {
        var canvas = new Canvas();

        var rect = (Rectangle)CheckBox.CreateDefaultFalseVisual();
        canvas.Children.Add(rect);

        var b = new Border
        {
            Margin = D2D_RECT_F.Thickness(Application.CurrentTheme.BorderSize)
        };
        canvas.Children.Add(b);
#if DEBUG
        canvas.Name ??= nameof(NullableCheckBox) + ".null";
#endif

        canvas.AttachedToComposition += (s, e) =>
        {
            rect.StrokeBrush = canvas.Compositor!.CreateColorBrush(Application.CurrentTheme.SelectedColor.ToColor());
            b.RenderBrush = canvas.Compositor.CreateColorBrush(Application.CurrentTheme.BorderColor.ToColor());
        };
        return canvas;
    }
}
