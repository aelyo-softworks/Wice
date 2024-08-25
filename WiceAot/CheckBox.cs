namespace Wice;

public class CheckBox : StateButton
{
    public CheckBox()
    {
        AddState(new StateButtonState(false, CreateChild) { EqualsFunc = (s, o) => !Conversions.ChangeType<bool>(o) });
        AddState(new StateButtonState(true, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool>(o) });
        Value = false;
    }

    [Category(CategoryBehavior)]
    public new bool Value { get => (bool)base.Value!; set => base.Value = value; }

    public static Visual CreateDefaultTrueVisual()
    {
        var border = new Border();

        var path = new Path
        {
            StrokeThickness = Application.CurrentTheme.BorderSize / 2,
        };

        border.Arranged += (s, e) =>
        {
            var geoSource = Application.Current.ResourceManager.GetCheckButtonGeometrySource(border.ArrangedRect.Width, border.ArrangedRect.Height);
            path.GeometrySource2D = geoSource;
        };

        border.AttachedToComposition += (s, e) =>
        {
            border.RenderBrush = border.Compositor.CreateColorBrush(Application.CurrentTheme.SelectedColor.ToColor());
            path.StrokeBrush = border.Compositor.CreateColorBrush(Application.CurrentTheme.UnselectedColor.ToColor());
        };

        border.Children.Add(path);
#if DEBUG
        border.Name = nameof(CheckBox) + ".true";
#endif
        return border;
    }

    public static Visual CreateDefaultFalseVisual()
    {
        var rect = new Rectangle
        {
            StrokeThickness = Application.CurrentTheme.BorderSize,
        };

        rect.AttachedToComposition += (s, e) =>
        {
            rect.StrokeBrush = rect.Compositor.CreateColorBrush(Application.CurrentTheme.BorderColor.ToColor());
        };
#if DEBUG
        rect.Name = nameof(CheckBox) + ".false";
#endif

        return rect;
    }

    protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state) => true.Equals(state.Value) ? CreateDefaultTrueVisual() : CreateDefaultFalseVisual();
}
