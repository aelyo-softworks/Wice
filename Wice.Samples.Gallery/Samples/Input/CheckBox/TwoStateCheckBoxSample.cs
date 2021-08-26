using DirectN;

namespace Wice.Samples.Gallery.Samples.Input.CheckBox
{
    public class TwoStateCheckBoxSample : Sample
    {
        public override int SortOrder => 0;
        public override string Description => "A 2-state check box";

        public override void Layout(Visual parent)
        {
            // stack checkbox and textbox
            var stack = new Stack();
            stack.Orientation = Orientation.Horizontal;
            parent.Children.Add(stack);
            Wice.Dock.SetDockType(stack, DockType.Top); // remove from display

            // add checkbox
            var cb = new Wice.CheckBox();
            stack.Children.Add(cb);

            // add results textbox
            var tb = new Wice.TextBox();
            tb.Margin = D2D_RECT_F.Thickness(10, 0);
            stack.Children.Add(tb);

            cb.Click += (s, e) =>
            {
                tb.Text = "CheckBox is " + (cb.Value ? "Checked" : "Unchecked");
            };
        }
    }
}
