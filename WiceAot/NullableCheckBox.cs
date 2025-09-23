namespace Wice;

/// <summary>
/// A tri-state checkbox built on <see cref="StateButton"/> that cycles between false, true, and null.
/// </summary>
public partial class NullableCheckBox : StateButton
{
    /// <summary>
    /// Initializes a new <see cref="NullableCheckBox"/> with three logical states:
    /// false, true, and null (default).
    /// </summary>
    public NullableCheckBox()
    {
        AddState(new StateButtonState(false, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool?>(o) == false });
        AddState(new StateButtonState(true, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool?>(o) == true });
        AddState(new StateButtonState(null, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool?>(o) == null });
        // by default, Value is null
    }

    /// <summary>
    /// Gets or sets the current tri-state value.
    /// </summary>
    [Category(CategoryBehavior)]
    public new bool? Value { get => (bool?)base.Value; set => base.Value = value; }

    /// <summary>
    /// Creates the visual for the true state.
    /// </summary>
    /// <returns>A themed visual representing the checked state.</returns>
    protected virtual Visual CreateTrueVisual() => new CheckBox.TrueVisual();

    /// <summary>
    /// Creates the visual for the false state.
    /// </summary>
    /// <returns>A themed visual representing the unchecked state.</returns>
    protected virtual Visual CreateFalseVisual() => new CheckBox.FalseVisual();

    /// <summary>
    /// Creates the visual for the null (indeterminate) state.
    /// </summary>
    /// <returns>A visual representing the indeterminate state.</returns>
    protected virtual Visual CreateNullVisual() => new NullVisual();

    /// <summary>
    /// Factory callback used by <see cref="StateButtonState"/> to create the state-specific child visual.
    /// </summary>
    /// <param name="box">The owning <see cref="StateButton"/>.</param>
    /// <param name="e">The originating event.</param>
    /// <param name="state">The state describing the value and factory context.</param>
    /// <returns>The created visual for the given <paramref name="state"/>.</returns>
    protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state)
    {
        if (true.Equals(state.Value))
            return CreateTrueVisual();

        if (false.Equals(state.Value))
            return CreateFalseVisual();

        return CreateNullVisual();
    }

    /// <summary>
    /// Visual for the indeterminate state: renders a stroked rectangle and an inner border using theme brushes.
    /// </summary>
    public partial class NullVisual : Canvas
    {
        /// <summary>
        /// Creates the null-state visual and adds its children: <see cref="Rectangle"/> then <see cref="Border"/>.
        /// </summary>
        public NullVisual()
        {
#if DEBUG
            Name ??= nameof(NullableCheckBox) + ".null";
#endif
            Children.Add(Rectangle);
            Children.Add(Border);
        }

        /// <summary>
        /// Gets the stroked rectangle element used to indicate the indeterminate box.
        /// </summary>
        public Rectangle Rectangle { get; } = new();

        /// <summary>
        /// Gets the inner border element rendered using the theme border color.
        /// </summary>
        public Border Border { get; } = new();

        /// <inheritdoc/>
        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
            var theme = GetWindowTheme();
            Rectangle.StrokeBrush = Compositor!.CreateColorBrush(theme.SelectedColor.ToColor());
            Border.RenderBrush = Compositor.CreateColorBrush(theme.BorderColor.ToColor());
            Window!.ThemeDpiEvent += OnThemeDpiEvent;
        }

        /// <inheritdoc/>
        protected override void OnDetachingFromComposition(object? sender, EventArgs e)
        {
            base.OnDetachingFromComposition(sender, e);
            Window!.ThemeDpiEvent -= OnThemeDpiEvent;
        }

        /// <summary>
        /// Applies theme/DPI dependent sizes: rectangle stroke thickness and inner border margin.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Theme/DPI data.</param>
        protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
        {
            var theme = GetWindowTheme();
            Rectangle.StrokeThickness = theme.BorderSize;
            Border.Margin = D2D_RECT_F.Thickness(theme.BorderSize);
        }
    }
}
