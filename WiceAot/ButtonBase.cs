namespace Wice;

public partial class ButtonBase : Border, IAccessKeyParent, IClickable
{
    public event EventHandler<EventArgs>? Click;

    private readonly List<AccessKey> _accessKeys = [];

    public ButtonBase()
    {
        IsFocusable = true;
        HandleOnClick = true;
        HandlePointerEvents = true;
        DoWhenAttachedToComposition(UpdateStyle);
    }

    [Category(CategoryBehavior)]
    public virtual bool HandleOnClick { get; set; }

    [Category(CategoryBehavior)]
    public object? Command { get; set; }

    [Category(CategoryBehavior)]
    public virtual IList<AccessKey> AccessKeys => _accessKeys;

    [Browsable(false)]
    public bool UpdateFromTitleBar { get; set; } = true;

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

    public void DoClick(EventArgs e) => OnClick(this, e);

    void IAccessKeyParent.OnAccessKey(KeyEventArgs e) => OnAccessKey(e);
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

    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
            return;

        OnClick(this, e);
        base.OnMouseButtonDown(sender, e);
    }

    protected virtual void UpdateStyle()
    {
        Opacity = IsEnabled ? 1f : Application.CurrentTheme.DisabledOpacityRatio;
        Cursor = IsEnabled ? Cursor.Hand : null;
    }

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
