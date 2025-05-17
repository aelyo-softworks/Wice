namespace Wice;

public partial class CheckBoxList : StateButtonListBox
{
    protected override StateButton CreateStateButton(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        var cb = new CheckBox();
        cb.Click += (s, e) =>
        {
            if (context.ItemVisual != null)
            {
                context.ItemVisual.IsSelected = cb.Value;
            }
        };
        return cb;
    }

    public override bool UpdateItemSelection(ItemVisual visual, bool? select)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        var changed = base.UpdateItemSelection(visual, select);
        var cb = visual.AllChildren.OfType<CheckBox>().FirstOrDefault();
        if (cb != null)
        {
            cb.Value = visual.IsSelected;
        }
        return changed;
    }
}
