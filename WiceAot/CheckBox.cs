namespace Wice;

public partial class CheckBox : StateButton
{
    public CheckBox()
    {
        AddState(new StateButtonState(false, CreateChild) { EqualsFunc = (s, o) => !Conversions.ChangeType<bool>(o) });
        AddState(new StateButtonState(true, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool>(o) });
        Value = false;
    }

    [Category(CategoryBehavior)]
    public new bool Value { get => (bool)base.Value!; set => base.Value = value; }

    protected virtual Visual CreateTrueVisual() => new TrueVisual();
    protected virtual Visual CreateFalseVisual() => new FalseVisual();
    protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state) => true.Equals(state.Value) ? CreateTrueVisual() : CreateFalseVisual();

    public partial class TrueVisual : Border
    {
        public TrueVisual()
        {
#if DEBUG
            Name ??= nameof(CheckBox) + ".true";
#endif
            Children.Add(Path);
        }

        public Path Path { get; } = new();

        protected override void OnArranged(object? sender, EventArgs e)
        {
            base.OnArranged(sender, e);
            var ar = ArrangedRect;
            var geoSource = Application.CurrentResourceManager.GetCheckButtonGeometrySource(ar.Width, ar.Height);
            Path.GeometrySource2D = geoSource;
        }

        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            var theme = GetWindowTheme();
            RenderBrush = Compositor!.CreateColorBrush(theme.SelectedColor.ToColor());
            Path.StrokeBrush = Compositor.CreateColorBrush(theme.UnselectedColor.ToColor());
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
            Path.StrokeThickness = theme.BorderSize / 2;
        }
    }

    public partial class FalseVisual : Rectangle
    {
        public FalseVisual()
        {
#if DEBUG
            Name ??= nameof(CheckBox) + ".false";
#endif
        }

        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            var theme = GetWindowTheme();
            StrokeBrush = Compositor!.CreateColorBrush(theme.BorderColor.ToColor());
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
            StrokeThickness = theme.BorderSize;
        }
    }
}
