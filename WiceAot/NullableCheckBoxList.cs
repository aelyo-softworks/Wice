namespace Wice;

/// <summary>
/// A <see cref="StateButtonListBox"/> whose per-item state button is a tri-state <see cref="NullableCheckBox"/>.
/// </summary>
public partial class NullableCheckBoxList : StateButtonListBox
{
    /// <inheritdoc/>
    protected override StateButton CreateStateButton(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        var ncb = new NullableCheckBox();
        ncb.Click += (s, e) =>
        {
            if (context.ItemVisual != null)
            {
                context.ItemVisual.IsSelected = ncb.Value == true;
            }
        };
        return ncb;
    }

    /// <inheritdoc/>
    public override bool UpdateItemSelection(ItemVisual visual, bool? select)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        var changed = base.UpdateItemSelection(visual, select);
        var ncb = visual.AllChildren.OfType<NullableCheckBox>().FirstOrDefault();
        if (ncb != null)
        {
            if (visual.IsSelected)
            {
                ncb.Value = true;
            }
        }
        return changed;
    }
}
