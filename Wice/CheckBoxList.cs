using System;
using System.Linq;

namespace Wice
{
    public class CheckBoxList : StateButtonListBox
    {
        protected override StateButton CreateStateButton(DataBindContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var cb = new CheckBox();
            cb.Click += (s, e) =>
            {
                context.ItemVisual.IsSelected = cb.Value;
            };
            return cb;
        }

        public override bool UpdateItemSelection(ItemVisual visual, bool? select)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            var changed = base.UpdateItemSelection(visual, select);
            var cb = visual.AllChildren.OfType<CheckBox>().FirstOrDefault();
            if (cb != null)
            {
                cb.Value = visual.IsSelected;
            }
            return changed;
        }
    }
}
