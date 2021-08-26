﻿using DirectN;

namespace Wice.Samples.Gallery.Samples.Input.CheckBox
{
    public class ThreeStateCheckBoxSample : Sample
    {
        public override int SortOrder => 2;
        public override string Description => "A 3-state check box";

        public override void Layout(Visual parent)
        {
            // stack checkbox and textbox
            var stack = new Stack();
            stack.Orientation = Orientation.Horizontal;
            parent.Children.Add(stack);
            Wice.Dock.SetDockType(stack, DockType.Top); // remove from display

            // add nullable checkbox
            var cb = new Wice.NullableCheckBox();
            stack.Children.Add(cb);

            // add results textbox
            var tb = new Wice.TextBox();
            tb.Margin = D2D_RECT_F.Thickness(10, 0);
            stack.Children.Add(tb);

            cb.Click += (s, e) =>
            {
                if (cb.Value == null)
                {
                    tb.Text = "CheckBox state is undetermined.";
                }
                else
                {
                    tb.Text = "CheckBox is " + (cb.Value.Value ? "Checked" : "Unchecked");
                }
            };
        }
    }
}
