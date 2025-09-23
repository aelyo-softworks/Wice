namespace Wice;

/// <summary>
/// A two-state toggle button that exposes a strongly-typed boolean <see cref="Value"/>.
/// </summary>
public partial class CheckBox : StateButton
{
    /// <summary>
    /// Initializes the checkbox with two states (<c>false</c> and <c>true</c>) and sets the default <see cref="Value"/> to <c>false</c>.
    /// </summary>
    public CheckBox()
    {
        AddState(new StateButtonState(false, CreateChild) { EqualsFunc = (s, o) => !Conversions.ChangeType<bool>(o) });
        AddState(new StateButtonState(true, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool>(o) });
        Value = false;
    }

    /// <summary>
    /// Gets or sets the current boolean state of the checkbox.
    /// </summary>
    [Category(CategoryBehavior)]
    public new bool Value { get => (bool)base.Value!; set => base.Value = value; }

    /// <summary>
    /// Creates the visual used for the <c>true</c> state (checked).
    /// </summary>
    /// <returns>A non-null <see cref="Visual"/> representing the checked state.</returns>
    protected virtual Visual CreateTrueVisual() => new TrueVisual();

    /// <summary>
    /// Creates the visual used for the <c>false</c> state (unchecked).
    /// </summary>
    /// <returns>A non-null <see cref="Visual"/> representing the unchecked state.</returns>
    protected virtual Visual CreateFalseVisual() => new FalseVisual();

    /// <summary>
    /// Factory used by state definitions to build per-state visual content.
    /// </summary>
    /// <param name="box">The owning <see cref="StateButton"/>.</param>
    /// <param name="e">The event that triggered the creation.</param>
    /// <param name="state">The target <see cref="StateButtonState"/>.</param>
    /// <returns>
    /// <see cref="CreateTrueVisual"/> when <paramref name="state"/> equals <c>true</c>;
    /// otherwise <see cref="CreateFalseVisual"/>.
    /// </returns>
    protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state) => true.Equals(state.Value) ? CreateTrueVisual() : CreateFalseVisual();

    /// <summary>
    /// Visual for the <c>true</c> state: renders a themed filled background and a stroked check glyph.
    /// </summary>
    public partial class TrueVisual : Border
    {
        /// <summary>
        /// Initializes the visual tree and adds the check <see cref="Path"/> as a child.
        /// </summary>
        public TrueVisual()
        {
#if DEBUG
            Name ??= nameof(CheckBox) + ".true";
#endif
            Children.Add(Path);
        }

        /// <summary>
        /// The vector path that renders the check glyph.
        /// </summary>
        public Path Path { get; } = new();

        /// <inheritdoc/>
        protected override void OnArranged(object? sender, EventArgs e)
        {
            base.OnArranged(sender, e);
            var ar = ArrangedRect;
            var geoSource = Application.CurrentResourceManager.GetCheckButtonGeometrySource(ar.Width, ar.Height);
            Path.GeometrySource2D = geoSource;
        }

        /// <inheritdoc/>
        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            var theme = GetWindowTheme();
            RenderBrush = Compositor!.CreateColorBrush(theme.SelectedColor.ToColor());
            Path.StrokeBrush = Compositor.CreateColorBrush(theme.UnselectedColor.ToColor());
            Window!.ThemeDpiEvent += OnThemeDpiEvent;
        }

        /// <inheritdoc/>
        protected override void OnDetachingFromComposition(object? sender, EventArgs e)
        {
            base.OnDetachingFromComposition(sender, e);
            Window!.ThemeDpiEvent -= OnThemeDpiEvent;
        }

        /// <summary>
        /// Applies theme-dependent stroke thickness to the check glyph.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Theme/DPI data.</param>
        protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
        {
            var theme = GetWindowTheme();
            Path.StrokeThickness = theme.BorderSize / 2;
        }
    }

    /// <summary>
    /// Visual for the <c>false</c> state: renders a themed stroked rectangle.
    /// </summary>
    public partial class FalseVisual : Rectangle
    {
        /// <summary>
        /// Initializes the visual, naming it in DEBUG builds for diagnostics.
        /// </summary>
        public FalseVisual()
        {
#if DEBUG
            Name ??= nameof(CheckBox) + ".false";
#endif
        }

        /// <inheritdoc/>
        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            var theme = GetWindowTheme();
            StrokeBrush = Compositor!.CreateColorBrush(theme.BorderColor.ToColor());
            Window!.ThemeDpiEvent += OnThemeDpiEvent;
        }

        /// <inheritdoc/>
        protected override void OnDetachingFromComposition(object? sender, EventArgs e)
        {
            base.OnDetachingFromComposition(sender, e);
            Window!.ThemeDpiEvent -= OnThemeDpiEvent;
        }

        /// <summary>
        /// Applies theme-dependent stroke thickness.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Theme/DPI data.</param>
        protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
        {
            var theme = GetWindowTheme();
            StrokeThickness = theme.BorderSize;
        }
    }
}
