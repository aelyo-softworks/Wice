using System;
using System.Collections.Generic;
using System.ComponentModel;
using DirectN;

namespace Wice
{
    public class MouseEventArgs : HandledEventArgs
    {
        internal readonly List<Visual> _visualsStack = new List<Visual>();

        internal MouseEventArgs()
        {
        }

        internal MouseEventArgs(int x, int y, MouseVirtualKeys vk)
            : this()
        {
            X = x;
            Y = y;
            Keys = vk;
        }

        public MouseVirtualKeys Keys { get; }
        public int X { get; }
        public int Y { get; }
        public IReadOnlyList<Visual> VisualsStack => _visualsStack;

        public D2D_POINT_2F GetPosition(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            return visual.GetRelativePosition(X, Y);
        }

        public bool Hits(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            var size = visual.RenderSize;
            if (size.IsInvalid)
                return false;

            return new D2D_RECT_F(size).Contains(GetPosition(visual));
        }

        public override string ToString() => "X=" + X + ",Y=" + Y + ",VK=" + Keys;
    }
}
