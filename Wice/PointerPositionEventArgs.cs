using System;
using System.Collections.Generic;
using DirectN;

namespace Wice
{
    public abstract class PointerPositionEventArgs : PointerEventArgs
    {
        internal readonly List<Visual> _visualsStack = new List<Visual>();

        protected PointerPositionEventArgs(int pointerId, int x, int y)
            : base(pointerId)
        {
            X = x;
            Y = y;
        }

        // window relative
        public int X { get; }
        public int Y { get; }
        public IReadOnlyList<Visual> VisualsStack => _visualsStack;

        public tagPOINT GetPosition(Visual visual)
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

        public override string ToString() => base.ToString() + ",X=" + X + ",Y=" + Y;
    }
}
