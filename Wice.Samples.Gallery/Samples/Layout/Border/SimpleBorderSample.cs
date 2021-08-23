using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectN;

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
            border.BorderThickness = 2;
            //border.VerticalAlignment = Alignment.Center;
            //border.HorizontalAlignment = Alignment.Center;
            border.BorderBrush = new SolidColorBrush(new _D3DCOLORVALUE(0xFFFFD700));
            parent.Children.Add(border);

            var textBox = new TextBox();
            border.Children.Add(textBox);
            textBox.Text = "Text inside a border";
            textBox.FontSize = 18;
        }
    }
}
