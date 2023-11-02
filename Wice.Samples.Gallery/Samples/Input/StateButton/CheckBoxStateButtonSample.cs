using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Input.StateButton
{
    public class CheckBoxStateButtonSample : Sample
    {
        public override int SortOrder => 1;
        public override string Description => "A checkbox recreated using a state button";

        public override void Layout(Visual parent)
        {
            // stack state button and textbox
            var stack = new Stack();
            stack.Orientation = Orientation.Horizontal;
            parent.Children.Add(stack);
            Dock.SetDockType(stack, DockType.Top); // remove from display

            // add state button
            var btn = new Wice.StateButton();
            btn.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue.ToColor());

            // add "true" / checked state
            btn.AddState(new StateButtonState(true, (button, args, state) => new TextBox
            {
                ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.White),
                Text = "X",
                VerticalAlignment = Alignment.Center,
                HorizontalAlignment = Alignment.Center
            })); ;

            // add "false" / unchecked set
            btn.AddState(new StateButtonState(false, (button, args, state) => new TextBox()));
            btn.Width = 20;
            btn.Height = 20;

            // add results textbox
            var results = new TextBox();
            results.Margin = D2D_RECT_F.Thickness(10, 0);
            results.HorizontalAlignment = Alignment.Center;
            results.VerticalAlignment = Alignment.Center;
            stack.Children.Add(results);

            stack.Children.Add(btn);
            btn.Click += (s, e) =>
            {
                results.Text = "\"CheckBox\" StateButton is " + (((bool)btn.Value) ? "Checked" : "Unchecked");
            };
        }
    }
}
