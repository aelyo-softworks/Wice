﻿using DirectN;

namespace Wice.Samples.Gallery.Samples.Input.RadioButton
{
    public class SimpleRadioButtonSample : Sample
    {
        public override int SortOrder => 0;
        public override string Description => "A simple radio button";

        public override void Layout(Visual parent)
        {
            // stack radio button and textbox
            var stack = new Stack();
            stack.Orientation = Orientation.Horizontal;
            parent.Children.Add(stack);
            Wice.Dock.SetDockType(stack, DockType.Top); // remove from display

            // add radio button
            var rb = new Wice.RadioButton();
            stack.Children.Add(rb);

            // add results textbox
            var tb = new Wice.TextBox();
            tb.Margin = D2D_RECT_F.Thickness(10, 0);
            stack.Children.Add(tb);

            rb.Click += (s, e) =>
            {
                tb.Text = "RadioButton is " + (rb.Value ? "Checked" : "Unchecked");
            };
        }
    }
}
