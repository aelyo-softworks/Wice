﻿using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Text.TextBox
{
    public class ColorFontTextBoxSample : Sample
    {
        public override string Description => "A non editable text box that demonstrates the color fonts.";
        public override int SortOrder => 2;

        public override void Layout(Visual parent)
        {
            var tb = new Wice.TextBox();
            tb.IsFocusable = true;
            parent.Children.Add(tb);
            Dock.SetDockType(tb, DockType.Top);

            tb.SelectionBrush = new SolidColorBrush(D3DCOLORVALUE.Yellow);
            tb.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Pink.ToColor());
            tb.Padding = D2D_RECT_F.Thickness(10);
            tb.Margin = D2D_RECT_F.Thickness(10);

            // note there can be difference between the source code (created with Windows' emoji editor) and the Wice's rendered text (from DirectWrite)
            tb.Text = "These are colored emoji: 😝👸🎅👨‍👩‍👧‍👦";
        }
    }
}
