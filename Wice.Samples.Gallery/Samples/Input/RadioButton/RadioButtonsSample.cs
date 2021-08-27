using System;
using System.Linq;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Input.RadioButton
{
    public class RadioButtonsSample : Sample
    {
        public override int SortOrder => 1;
        public override string Description => "A group of radio buttons";

        public override void Layout(Visual parent)
        {
            // dock radio button and textbox
            var dock = new Dock();
            parent.Children.Add(dock);
            Wice.Dock.SetDockType(dock, DockType.Top); // remove from display

            // add radio buttons with texts
            var rb0 = new Wice.RadioButton();
            rb0.Name = "button 0";
            rb0.Margin = D2D_RECT_F.Thickness(10, 0);
            dock.Children.Add(rb0);

            var rb1 = new Wice.RadioButton();
            rb1.Name = "button 1";
            rb1.Margin = D2D_RECT_F.Thickness(10, 0);
            dock.Children.Add(rb1);

            var rb2 = new Wice.RadioButton();
            rb2.Name = "button 2";
            rb2.Margin = D2D_RECT_F.Thickness(10, 0);
            dock.Children.Add(rb2);

            // add results textbox
            var results = new Wice.TextBox();
            results.Margin = D2D_RECT_F.Thickness(10, 0);
            dock.Children.Add(results);

            rb0.Click += onRadioButtonClick;
            rb1.Click += onRadioButtonClick;
            rb2.Click += onRadioButtonClick;

            void onRadioButtonClick(object sender, EventArgs e)
            {
                var rb = (Wice.RadioButton)sender;
                results.Text = "Selected RadioButton is " + rb.Name;

                // use a method extension to deselect all other buttons
                new[] { rb0, rb1, rb2 }.Select(rb);
            }
        }
    }
}
