﻿using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Layout.Stack
{
    public class SimpleVerticalStackSample : Sample
    {
        public override string Description => "A simple vertical stack containing two boxes.";
        public override int SortOrder => 1;

        public override void Layout(Visual parent)
        {
            var stack = new Wice.Stack();
            parent.Children.Add(stack);
            Wice.Dock.SetDockType(stack, DockType.Top); // remove from display
            stack.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green.ToColor());

            var b0 = new Wice.Border();
            stack.Children.Add(b0);
            b0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue.ToColor());
            b0.Width = 100;
            b0.Height = 100;

            var b1 = new Wice.Border();
            stack.Children.Add(b1);
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red.ToColor());
            b1.Width = 50;
            b1.Height = 50;
        }
    }
}
