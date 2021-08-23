using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wice.Samples.Gallery.Samples.Layout.Border
{
    public class SimpleBorderSample : Sample
    {
        public override int SortOrder => 0;
        public override string Description => "A border around a TextBox.";

        // this method's content will be displayed by the CodeBox visual
        public override void Layout(Visual parent)
        {
            var border = new Wice.Border();
            parent.Children.Add(border);

            var textBox = new TextBox();
            border.Children.Add(textBox);
            textBox.Text = "Text inside a border";
            textBox.FontSize = 18;
        }
    }
}
