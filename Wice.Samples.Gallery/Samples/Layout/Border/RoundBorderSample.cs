﻿using System.Numerics;
using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout.Border
{
    public class RoundBorderSample : Sample
    {
        public override int SortOrder => 1;
        public override string Description => "A round long border around a TextBox.";

        public override void Layout(Visual parent)
        {
            var border = new Wice.Border();
            border.BorderThickness = 2.5f;
            border.CornerRadius = new Vector2(5);
            border.BorderBrush = new SolidColorBrush(_D3DCOLORVALUE.YellowGreen); // well-known colors
            parent.Children.Add(border);
            Wice.Dock.SetDockType(border, DockType.Top); // remove from display

            var textBox = new TextBox();
            textBox.Padding = 10;
            border.Children.Add(textBox);
            textBox.Text = "Text inside a round border";
            textBox.FontSize = 18;
        }
    }
}
