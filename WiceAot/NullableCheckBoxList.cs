namespace Wice;

public class NullableCheckBoxList : StateButtonListBox
{
    protected override StateButton CreateStateButton(DataBindContext context)
    {
        var ncb = new NullableCheckBox();
        ncb.Click += (s, e) =>
        {
            context.ItemVisual.IsSelected = ncb.Value == true;
        };
        return ncb;
    }

    public override bool UpdateItemSelection(ItemVisual visual, bool? select)
    {
        ArgumentNullException.ThrowIfNull(visual);
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
