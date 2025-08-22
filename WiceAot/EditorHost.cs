namespace Wice;

/// <summary>
/// Hosts and toggles a <see cref="Dialog"/> anchored to this control's header selection.
/// </summary>
/// <remarks>
/// Behavior:
/// - Subscribes to <see cref="Header.IsSelectedChanged"/> and opens/closes a <see cref="Dialog"/> accordingly.
/// - Creates the dialog on-demand (first open) and attaches it to the <see cref="Window"/> as a child.
/// - Placement defaults to <see cref="PlacementMode.OuterBottomRight"/> relative to this host.
/// - When <see cref="EditorMode"/> is not <see cref="EditorMode.Modal"/>, the dialog is non-modal and does not show a window overlay.
/// - Global mouse clicks on the window close the dialog when they occur outside both this host and the dialog.
/// Lifecycle:
/// - Raises <see cref="DialogOpened"/> after creating and attaching the dialog.
/// - Raises <see cref="DialogClosed"/> prior to closing and detaching the dialog.
/// </remarks>
public partial class EditorHost : HeaderedContent
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
    /// Gets or sets the editor mode that influences dialog modality/overlay at open time.
    /// </summary>
    /// <remarks>
    /// Not a base property; the value is only read when opening the dialog to configure
    /// <see cref="Dialog.IsModal"/> and overlay behavior. Changing this after a dialog
    /// is already open has no effect until it is closed and reopened.
    /// </remarks>
    public EditorMode EditorMode { get; set; }

    /// <summary>
    /// Gets the current dialog instance managed by this host, or null when no dialog is open.
    /// </summary>
    public virtual Dialog? Dialog { get; protected set; }

    /// <summary>
    /// Initializes a new instance of <see cref="EditorHost"/> and wires header selection to dialog open/close.
    /// </summary>
    /// <remarks>
    /// On first selection, creates a <see cref="Dialog"/>, configures placement and brushes, wires window-level
    /// mouse handling to detect outside clicks, and adds it to <see cref="Window.Children"/>.
    /// On subsequent selection toggles, closes and disposes the current dialog.
    /// </remarks>
    public EditorHost()
    {
        Header.IsSelectedChanged += (s, e) =>
        {
            if (Window != null && Compositor != null)
            {
                if (Dialog == null)
                {
                    var dlg = new Dialog();
#if DEBUG
                    dlg.Name = "dialog" + Name.CapitalizeFirst();
#endif
                    Window.MouseButtonDown += OnWindowMouseButtonDown;

                    if (EditorMode != EditorMode.Modal)
                    {
                        dlg.IsModal = false;
                        dlg.ShowWindowOverlay = false;
                    }

                    dlg.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.LightPink.ToColor());
                    dlg.PlacementMode = PlacementMode.OuterBottomRight;
                    dlg.PlacementTarget = this;
                    Window.Children.Add(dlg);
                    Dialog = dlg;
                    OnDialogOpened(this, EventArgs.Empty);
                }
                else
                {
                    OnDialogClosed(this, EventArgs.Empty);
                    Window.MouseButtonDown -= OnWindowMouseButtonDown;
                    Dialog.Close();
                    Dialog = null;
                }
            }
        };
    }

#if DEBUG
    /// <inheritdoc/>
    /// <remarks>
    /// When a dialog is open, updates its <see cref="BaseObject.Name"/> to "dialog" + this host's capitalized name
    /// for easier diagnostics.
    /// </remarks>
    public override string? Name
    {
        get => base.Name;
        set
        {
            base.Name = value;
            if (Dialog != null)
            {
                Dialog.Name = "dialog" + Name.CapitalizeFirst();
            }
        }
    }
#endif

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

    private void OnWindowMouseButtonDown(object? sender, MouseButtonEventArgs e)
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
