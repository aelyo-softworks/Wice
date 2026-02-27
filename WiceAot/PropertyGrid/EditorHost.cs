namespace Wice.PropertyGrid;

/// <summary>
/// Hosts and toggles a <see cref="Dialog"/> anchored to this control's header selection.
/// </summary>
#if  NETFRAMEWORK
public partial class EditorHost : HeaderedContent
#else
public partial class EditorHost<T> : HeaderedContent
#endif
{
    /// <summary>
    /// Raised after a dialog instance is created, configured, and added to the window visual tree.
    /// </summary>
    public event EventHandler<EventArgs>? DialogOpened;

    /// <summary>
    /// Raised just before the dialog is closed and removed from the window visual tree.
    /// </summary>
    public event EventHandler<EventArgs>? DialogClosed;

    /// <summary>
    /// Initializes a new instance of the EditorHost class.
    /// </summary>
    /// <param name="visual">The visual representation of the property value. This parameter cannot be <see langword="null"/>.</param>
#if NETFRAMEWORK
    public EditorHost(PropertyValueVisual visual)
#else
    public EditorHost(PropertyValueVisual<T> visual)
#endif
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));

        Visual = visual;
        IsFocusable = true;
        Header.AccessKeys.Add(new AccessKey(VIRTUAL_KEY.VK_SPACE));
        Header.IsSelectedChanged += (s, e) =>
        {
            var parent = GetParent();
            // not we add ourselves to the window's children, not a modal dialog
            if (parent != null && Compositor != null)
            {
                if (Dialog == null)
                {
                    var dlg = new Dialog();
#if DEBUG
                    dlg.Name = "dialog" + Name.CapitalizeFirst();
#endif
                    parent.MouseButtonDown += OnParentMouseButtonDown;
                    dlg.Closing += (s, e) =>
                    {
                        // prevent closing via other means than header toggle
                        // note this will force close when user presses ESC
                        if (Header.IsSelected)
                        {
                            e.Cancel = true;
                            Header.IsSelected = false;
                        }
                    };

                    if (EditorMode != EditorMode.Modal)
                    {
                        dlg.IsModal = false;
                        dlg.ShowWindowOverlay = false;
                    }

                    dlg.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Transparent.ToColor());
                    dlg.PlacementMode = PlacementMode.Center;
                    dlg.PlacementTarget = this;
                    parent.Children.Add(dlg);
                    Dialog = dlg;
                    OnDialogOpened(this, EventArgs.Empty);
                }
                else
                {
                    OnDialogClosed(this, EventArgs.Empty);
                    parent.MouseButtonDown -= OnParentMouseButtonDown;
                    Dialog.Close();
                    Dialog = null;
                }
            }
        };
    }

#if DEBUG
    /// <inheritdoc/>
    public override string? Name
    {
        get => base.Name;
        set
        {
            base.Name = value;
            Dialog?.Name = "dialog" + Name.CapitalizeFirst();
        }
    }
#endif

    /// <summary>
    /// Gets the visual representation of the property value being edited.
    /// </summary>
#if NETFRAMEWORK
    public PropertyValueVisual Visual { get; }
#else
    public PropertyValueVisual<T> Visual { get; }
#endif

    /// <summary>
    /// Gets a value indicating whether the dialog is currently open.
    /// </summary>
    public bool IsOpened => Dialog != null;

    /// <summary>
    /// Gets or sets the editor mode that influences dialog modality/overlay at open time.
    /// </summary>
    public virtual EditorMode EditorMode { get; set; }

    /// <summary>
    /// Gets the current dialog instance managed by this host, or null when no dialog is open.
    /// </summary>
    public virtual Dialog? Dialog { get; protected set; }

    /// <summary>
    /// Closes the dialog by toggling the header selection off.
    /// </summary>
    public virtual void CloseDialog() => Header.IsSelected = false;

    /// <summary>
    /// Raises <see cref="DialogOpened"/>.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    protected virtual void OnDialogOpened(object? sender, EventArgs e) => DialogOpened?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="DialogClosed"/>.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    protected virtual void OnDialogClosed(object? sender, EventArgs e) => DialogClosed?.Invoke(sender, e);

    /// <summary>
    /// Retrieves the parent visual element to use for the current object.
    /// </summary>
    /// <returns>The parent <see cref="Visual"/> of the current object, or <see langword="null"/> if no parent is available.</returns>
    protected virtual Visual? GetParent() => Window;

    /// <inheritdoc/>
    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        base.OnKeyDown(sender, e);
        if (!e.Handled && e.IsDown && e.Key == VIRTUAL_KEY.VK_SPACE)
        {
            var focused = Window?.FocusedVisual;
            var childFocused = focused != null && IsChild(focused);
            if (childFocused || focused == this)
            {
                Header.IsSelected = true;
            }
        }
    }

    /// <summary>
    /// Handles the mouse button down event from the parent control.
    /// </summary>
    /// <param name="sender">The source of the event, typically the parent control.</param>
    /// <param name="e">The <see cref="MouseButtonEventArgs"/> containing details about the mouse button event.</param>
    protected virtual void OnParentMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        var dlg = Dialog;
        if (dlg == null)
            return;

        // keep this opened
        if (e.Hits(this))
            return;

        // close if outside of dialog
        if (!e.Hits(dlg))
        {
            CloseDialog();
        }
    }
}
