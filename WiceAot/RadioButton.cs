namespace Wice;

/// <summary>
/// A radio-style state button that toggles between two logical states (false/true)
/// and builds its visual content based on the current <see cref="Value"/>.
/// </summary>
public partial class RadioButton : StateButton, IFocusableParent
{
    /// <summary>
    /// Initializes a new <see cref="RadioButton"/> and registers the default false/true states.
    /// </summary>
    public RadioButton()
    {
        AddState(new StateButtonState(false, CreateChild));
        AddState(new StateButtonState(true, CreateChild));
    }

    /// <summary>
    /// Gets or sets the boolean state of the radio button.
    /// </summary>
    [Category(CategoryBehavior)]
    public new bool Value { get => (bool)base.Value!; set => base.Value = value; }

    /// <inheritdoc/>
    Visual? IFocusableParent.FocusableVisual => null;
#if !NETFRAMEWORK
    /// <inheritdoc/>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type IFocusableParent.FocusVisualShapeType => typeof(Ellipse);

    /// <inheritdoc/>
    float? IFocusableParent.FocusOffset => null;

    /// <summary>
    /// Creates the visual for the <c>true</c> state. Override to customize the inner/outer disks.
    /// </summary>
    protected virtual Visual CreateTrueVisual() => new TrueVisual();

    /// <summary>
    /// Creates the visual for the <c>false</c> state. Override to customize the empty circle.
    /// </summary>
    protected virtual Visual CreateFalseVisual() => new FalseVisual();

    /// <summary>
    /// Factory used by <see cref="StateButtonState"/> to build the appropriate visual for the given <paramref name="state"/>.
    /// </summary>
    /// <param name="box">The owning <see cref="StateButton"/>.</param>
    /// <param name="e">The originating event.</param>
    /// <param name="state">The state descriptor for which to create the child visual.</param>
    /// <returns>The created visual, never null.</returns>
    protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state) => true.Equals(state.Value) ? CreateTrueVisual() : CreateFalseVisual();

    /// <summary>
    /// Visual for the <c>true</c> state: renders an outer stroked disk and an inner filled disk.
    /// </summary>
    public partial class TrueVisual : Border
    {
        /// <summary>
        /// Initializes the visual tree (Canvas containing the outer and inner ellipses).
        /// </summary>
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

        /// <summary>
        /// Outer stroked circle.
        /// </summary>
        public Ellipse OuterDisk { get; } = new();

        /// <summary>
        /// Inner filled circle.
        /// </summary>
        public Ellipse InnerDisk { get; } = new();

        /// <inheritdoc/>
        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            var theme = GetWindowTheme();
            OuterDisk.StrokeBrush = Compositor!.CreateColorBrush(theme.BorderColor.ToColor());
            InnerDisk.FillBrush = Compositor.CreateColorBrush(theme.BorderColor.ToColor());
            Window!.ThemeDpiEvent += OnThemeDpiEvent;
        }

        /// <inheritdoc/>
        protected override void OnDetachingFromComposition(object? sender, EventArgs e)
        {
            base.OnDetachingFromComposition(sender, e);
            Window!.ThemeDpiEvent -= OnThemeDpiEvent;
        }

        /// <summary>
        /// Applies theme/DPI dependent sizes (stroke thickness and inner disk radius).
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Theme/DPI data.</param>
        protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
        {
            var theme = GetWindowTheme();
            OuterDisk.StrokeThickness = theme.BorderSize / 2;
            InnerDisk.RadiusOffset = new Vector2(-theme.BorderSize * 1.2f, -theme.BorderSize * 1.2f);
        }
    }

    /// <summary>
    /// Visual for the <c>false</c> state: a simple stroked circle.
    /// </summary>
    public partial class FalseVisual : Ellipse
    {
        /// <summary>
        /// Initializes the visual, naming it in DEBUG builds for diagnostics.
        /// </summary>
        public FalseVisual()
        {
#if DEBUG
            Name = nameof(CheckBox) + ".false";
#endif
        }

        /// <inheritdoc/>
        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            StrokeBrush = Compositor!.CreateColorBrush(GetWindowTheme().BorderColor.ToColor());
            Window!.ThemeDpiEvent += OnThemeDpiEvent;
        }

        /// <inheritdoc/>
        protected override void OnDetachingFromComposition(object? sender, EventArgs e)
        {
            base.OnDetachingFromComposition(sender, e);
            Window!.ThemeDpiEvent -= OnThemeDpiEvent;
        }

        /// <summary>
        /// Applies theme/DPI dependent stroke thickness.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Theme/DPI data.</param>
        protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
        {
            var theme = GetWindowTheme();
            StrokeThickness = theme.BorderSize / 2;
        }
    }
}
