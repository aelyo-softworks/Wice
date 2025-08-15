namespace Wice;

/// <summary>
/// Base class for clickable visuals backed by a <see cref="Border"/>.
/// Provides a unified click surface with keyboard activation, access key support,
/// and simple enabled/disabled styling.
/// </summary>
/// <remarks>
/// Interaction:
/// - Mouse: any button down invokes <see cref="OnClick(object?, EventArgs)"/>.
/// - Keyboard: SPACE invokes <see cref="OnClick(object?, EventArgs)"/> when focused.
/// - Access keys: when focused, matching <see cref="AccessKeys"/> invoke <see cref="DoClick(EventArgs)"/>.
/// Styling:
/// - When <see cref="IsEnabled"/> changes, focusability and visual style are updated to reflect the state.
/// </remarks>
public partial class ButtonBase : Border, IAccessKeyParent, IClickable
{
    /// <summary>
    /// Occurs when the visual is clicked via mouse, keyboard, or an access key.
    /// </summary>
    /// <remarks>
    /// The <paramref name="sender"/> of the event is the origin that triggered the click:
    /// - For keyboard events, it is the original sender from the key event pipeline.
    /// - For mouse events, it is this instance.
    /// </remarks>
    public event EventHandler<EventArgs>? Click;

    private readonly List<AccessKey> _accessKeys = [];

    /// <summary>
    /// Initializes a new instance of <see cref="ButtonBase"/>.
    /// </summary>
    /// <remarks>
    /// Defaults:
    /// - <see cref="Visual.IsFocusable"/> = true
    /// - <see cref="HandleOnClick"/> = true
    /// - <c>HandlePointerEvents</c> = true
    /// Hooks:
    /// - Schedules <see cref="UpdateStyle"/> once attached to composition so initial styling reflects state.
    /// </remarks>
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
    /// <remarks>
    /// When <see langword="true"/>, if the event args implement <see cref="HandledEventArgs"/>,
    /// <c>Handled</c> will be set to <see langword="true"/> after <see cref="Click"/> is raised.
    /// </remarks>
    [Category(CategoryBehavior)]
    public virtual bool HandleOnClick { get; set; }

    /// <summary>
    /// Gets or sets an optional command object associated with this button.
    /// </summary>
    /// <remarks>
    /// This class does not execute or route the command; consumers may subscribe to <see cref="Click"/>
    /// and use this property to bind execution logic.
    /// </remarks>
    [Category(CategoryBehavior)]
    public object? Command { get; set; }

    /// <summary>
    /// Gets the collection of access keys that can activate this button when focused.
    /// </summary>
    /// <remarks>
    /// Access keys are matched exactly (key + modifiers) via <see cref="AccessKey.Matches(KeyEventArgs)"/>.
    /// </remarks>
    [Category(CategoryBehavior)]
    public virtual IList<AccessKey> AccessKeys => _accessKeys;

    /// <summary>
    /// Gets or sets whether this element should update aspects of its appearance when hosted in a title bar.
    /// </summary>
    /// <remarks>
    /// This flag is not used by <see cref="ButtonBase"/> directly; it is provided for container-specific behavior.
    /// </remarks>
    [Browsable(false)]
    public bool UpdateFromTitleBar { get; set; } = true;

    /// <inheritdoc/>
    /// <remarks>
    /// Special handling:
    /// - When <paramref name="property"/> is <c>IsEnabledProperty</c>, synchronizes <see cref="Visual.IsFocusable"/>
    ///   with the new value and calls <see cref="UpdateStyle"/> to reflect the enabled state.
    /// </remarks>
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
    /// <remarks>
    /// Preconditions:
    /// - <see cref="AccessKeys"/> must contain at least one entry.
    /// - <see cref="Visual.IsEnabled"/> and <see cref="IsFocused"/> must be <see langword="true"/>.
    /// Behavior:
    /// - Invokes <see cref="DoClick(EventArgs)"/> on the first matching key and sets <see cref="HandledEventArgs.Handled"/>
    ///   to <see langword="true"/>.
    /// </remarks>
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

    /// <summary>
    /// Handles key presses while this visual has focus.
    /// </summary>
    /// <param name="sender">Originating sender.</param>
    /// <param name="e">Key event data.</param>
    /// <remarks>
    /// SPACE activates the button. The event is marked handled when activation occurs.
    /// </remarks>
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

    /// <summary>
    /// Handles mouse button presses.
    /// </summary>
    /// <param name="sender">Originating sender.</param>
    /// <param name="e">Mouse button event data.</param>
    /// <remarks>
    /// Any mouse button down invokes <see cref="OnClick(object?, EventArgs)"/> when enabled.
    /// </remarks>
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
    /// <remarks>
    /// - Sets <see cref="Visual.Opacity"/> to 1.0 when enabled, or to <c>GetWindowTheme().DisabledOpacityRatio</c> when disabled.
    /// - Sets <see cref="Visual.Cursor"/> to <see cref="Cursor.Hand"/> when enabled; clears it when disabled.
    /// </remarks>
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
    /// <remarks>
    /// When <see cref="HandleOnClick"/> is <see langword="true"/> and <paramref name="e"/> implements
    /// <see cref="HandledEventArgs"/>, the event is marked handled after <see cref="Click"/> is invoked.
    /// </remarks>
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
