namespace Wice;

/// <summary>
/// A ListBox specialization that prepends a <see cref="StateButton"/> to each item
/// and shows the item's display text next to it. Selection is always multi-select.
/// </summary>
public abstract partial class StateButtonListBox : ListBox
{
    /// <summary>
    /// Initializes a new instance and enforces multi-selection.
    /// </summary>
    protected StateButtonListBox()
    {
        SelectionMode = SelectionMode.Multiple;
    }

    /// <summary>
    /// Creates the per-item <see cref="StateButton"/> instance.
    /// </summary>
    /// <param name="context">The data-bind context for the current item.</param>
    /// <returns>A non-null <see cref="StateButton"/> configured for the item.</returns>
    protected abstract StateButton CreateStateButton(DataBindContext context);

    /// <inheritdoc/>
    protected override void CreateDataItemVisual(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        var panel = new Dock
        {
            IsFocusable = true
        };

        var button = CreateStateButton(context);
        if (button == null)
            throw new InvalidOperationException();

        panel.Children.Add(button);

        var text = new TextBox2
        {
            Margin = D2D_RECT_F.Thickness(GetWindowTheme().StateButtonListPadding, 0, 0, 0),
            IsFocusable = true,
            IsEditable = false
        };
        panel.Children.Add(text);

        if (context.DataVisual != null)
        {
            panel.Children.Add(context.DataVisual);
        }

        context.DataVisual = panel;
    }

    /// <inheritdoc/>
    protected override void BindDataItemVisual(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        if (context.DataVisual is not Dock dock)
            return;

        var tb = dock.Children.OfType<TextBox>().FirstOrDefault();
        tb?.Text = context.GetDisplayName();
    }

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        // prevent single mode
        if (property == SelectionModeProperty)
        {
            var mode = (SelectionMode)value!;
            if (mode == SelectionMode.Single)
                return false;
        }
        return base.SetPropertyValue(property, value, options);
    }

    private sealed partial class TextBox2 : TextBox
    {
    }

    /// <inheritdoc/>
    protected override void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        base.OnThemeDpiEvent(sender, e);
        foreach (var tb in AllChildren.OfType<TextBox2>())
        {
            tb.Margin = D2D_RECT_F.Thickness(GetWindowTheme().StateButtonListPadding, 0, 0, 0);
        }
    }
}
