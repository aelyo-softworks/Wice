namespace Wice;

public class EditorHost : HeaderedContent
{
    public event EventHandler<EventArgs>? DialogOpened;
    public event EventHandler<EventArgs>? DialogClosed;

    //  not a baseprop, only considered at open time
    public EditorMode EditorMode { get; set; }

    public virtual Dialog? Dialog { get; protected set; }

    public EditorHost()
    {
        Header.IsSelectedChanged += (s, e) =>
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
        };
    }

#if DEBUG
    public override string Name
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

    public virtual void CloseDialog() => Header.IsSelected = false;

    protected virtual void OnDialogOpened(object? sender, EventArgs e) => DialogOpened?.Invoke(sender, e);
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
