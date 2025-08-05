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

    protected virtual Visual CreateTrueVisual() => new CheckBox.TrueVisual();
    protected virtual Visual CreateFalseVisual() => new CheckBox.FalseVisual();
    protected virtual Visual CreateNullVisual() => new NullVisual();
    protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state)
    {
        if (true.Equals(state.Value))
            return CreateTrueVisual();

        if (false.Equals(state.Value))
            return CreateFalseVisual();

        return CreateNullVisual();
    }

    public partial class NullVisual : Canvas
    {
        public NullVisual()
        {
#if DEBUG
            Name ??= nameof(NullableCheckBox) + ".null";
#endif
            Children.Add(Rectangle);
            Children.Add(Border);
        }

        public Rectangle Rectangle { get; } = new();
        public Border Border { get; } = new();

        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            var theme = GetWindowTheme();
            Rectangle.StrokeBrush = Compositor!.CreateColorBrush(theme.SelectedColor.ToColor());
            Border.RenderBrush = Compositor.CreateColorBrush(theme.BorderColor.ToColor());
            Window!.ThemeDpiEvent += OnThemeDpiEvent;
        }

        protected override void OnDetachingFromComposition(object? sender, EventArgs e)
        {
            base.OnDetachingFromComposition(sender, e);
            Window!.ThemeDpiEvent -= OnThemeDpiEvent;
        }

        protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
        {
            var theme = GetWindowTheme();
            Rectangle.StrokeThickness = theme.BorderSize;
            Border.Margin = D2D_RECT_F.Thickness(theme.BorderSize);
        }
    }
}
