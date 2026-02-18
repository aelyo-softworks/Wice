namespace Wice;

/// <summary>
/// Base class for clickable visuals backed by a <see cref="Border"/>.
/// Provides a unified click surface with keyboard activation, access key support,
/// and simple enabled/disabled styling.
/// </summary>
public partial class ButtonBase : Border, IAccessKeyParent, IClickable
{
    /// <summary>
    /// Occurs when the visual is clicked via mouse, keyboard, or an access key.
    /// </summary>
    public event EventHandler<EventArgs>? Click;

    private readonly List<AccessKey> _accessKeys = [];

    /// <summary>
    /// Initializes a new instance of <see cref="ButtonBase"/>.
    /// </summary>
    public ButtonBase()
    {
        IsFocusable = true;
        HandleOnClick = true;
        HandlePointerEvents = true;
        DoWhenAttachedToComposition(UpdateStyle);
    }

    /// <summary>
    /// Gets or sets whether <see cref="OnClick(object?, EventArgs)"/> should mark the incoming
    /// event as handled when possible.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool HandleOnClick { get; set; }

    /// <summary>
    /// Gets or sets an optional command object associated with this button.
    /// </summary>
    [Category(CategoryBehavior)]
    public object? Command { get; set; }

    /// <summary>
    /// Gets the collection of access keys that can activate this button when focused.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual IList<AccessKey> AccessKeys => _accessKeys;

    /// <summary>
    /// Gets or sets whether this element should update aspects of its appearance when hosted in a title bar.
    /// </summary>
    [Browsable(false)]
    public virtual bool UpdateFromTitleBar { get; set; } = true;

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == IsEnabledProperty)
        {
            IsFocusable = (bool)value!;
            UpdateStyle();
            return true;
        }

        return true;
    }

    /// <summary>
    /// Programmatically triggers a click on this instance, routing through <see cref="OnClick(object?, EventArgs)"/>.
    /// </summary>
    /// <param name="e">Event arguments to forward to click handlers.</param>
    public void DoClick(EventArgs e) => OnClick(this, e);

    /// <summary>
    /// Handles access key invocation from the access key routing pipeline.
    /// </summary>
    /// <param name="e">Key event carrying the pressed key and modifiers.</param>
    void IAccessKeyParent.OnAccessKey(KeyEventArgs e) => OnAccessKey(e);

    /// <summary>
    /// Called when an access key is pressed while this visual is focused.
    /// </summary>
    /// <param name="e">Key event data.</param>
    protected virtual void OnAccessKey(KeyEventArgs e)
    {
        if (AccessKeys == null || !IsEnabled || !IsFocused)
            return;

        foreach (var ak in AccessKeys)
        {
            if (ak.Matches(e))
            {
                DoClick(e);
                e.Handled = true;
                return;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsEnabled || !IsFocused)
            return;

        if (e.Key == VIRTUAL_KEY.VK_SPACE)
        {
            OnClick(sender, e);
            e.Handled = true;
        }
        base.OnKeyDown(sender, e);
    }

    /// <inheritdoc/>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
            return;

        OnClick(this, e);
        base.OnMouseButtonDown(sender, e);
    }

    /// <summary>
    /// Updates visual styling to reflect the current enabled state.
    /// </summary>
    protected virtual void UpdateStyle()
    {
        Opacity = IsEnabled ? 1f : GetWindowTheme().DisabledOpacityRatio;
        Cursor = IsEnabled ? Cursor.Hand : null;
    }

    /// <summary>
    /// Raises the <see cref="Click"/> event if enabled and optionally marks the event as handled.
    /// </summary>
    /// <param name="sender">Event origin.</param>
    /// <param name="e">Event data passed to subscribers.</param>
    protected virtual void OnClick(object? sender, EventArgs e)
    {
        if (!IsEnabled)
            return;

        Click?.Invoke(sender, e);

        if (HandleOnClick)
        {
            if (e is HandledEventArgs he)
            {
                he.Handled = true;
            }
        }
    }
}
