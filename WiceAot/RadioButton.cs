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
#if !NETFRAMEWORK
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type IFocusableParent.FocusVisualShapeType => typeof(Ellipse);
    float? IFocusableParent.FocusOffset => null;

    protected virtual Visual CreateTrueVisual() => new TrueVisual();
    protected virtual Visual CreateFalseVisual() => new FalseVisual();
    protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state) => true.Equals(state.Value) ? CreateTrueVisual() : CreateFalseVisual();

    public partial class TrueVisual : Border
    {
        public TrueVisual()
        {
            var canvas = new Canvas();
#if DEBUG
            canvas.Name = nameof(CheckBox) + ".true";
#endif
            Child = canvas;
            canvas.Children.Add(OuterDisk);
            canvas.Children.Add(InnerDisk);
        }

        public Ellipse OuterDisk { get; } = new();
        public Ellipse InnerDisk { get; } = new();

        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            var theme = GetWindowTheme();
            OuterDisk.StrokeBrush = Compositor!.CreateColorBrush(theme.BorderColor.ToColor());
            InnerDisk.FillBrush = Compositor.CreateColorBrush(theme.BorderColor.ToColor());
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
            OuterDisk.StrokeThickness = theme.BorderSize / 2;
            InnerDisk.RadiusOffset = new Vector2(-theme.BorderSize * 1.2f, -theme.BorderSize * 1.2f);
        }
    }

    public partial class FalseVisual : Ellipse
    {
        public FalseVisual()
        {
#if DEBUG
            Name = nameof(CheckBox) + ".false";
#endif
        }

        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            StrokeBrush = Compositor!.CreateColorBrush(GetWindowTheme().BorderColor.ToColor());
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
            StrokeThickness = theme.BorderSize / 2;
        }
    }
}
