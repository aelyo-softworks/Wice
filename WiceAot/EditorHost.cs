namespace Wice;

/// <summary>
/// Hosts and toggles a <see cref="Dialog"/> anchored to this control's header selection.
/// </summary>
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
    public EditorMode EditorMode { get; set; }

    /// <summary>
    /// Gets the current dialog instance managed by this host, or null when no dialog is open.
    /// </summary>
    public virtual Dialog? Dialog { get; protected set; }

    /// <summary>
    /// Initializes a new instance of <see cref="EditorHost"/> and wires header selection to dialog open/close.
    /// </summary>
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
