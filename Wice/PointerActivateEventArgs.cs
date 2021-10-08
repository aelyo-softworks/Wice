﻿using System;
using DirectN;

namespace Wice
{
    public class PointerActivateEventArgs : PointerEventArgs
    {
        internal PointerActivateEventArgs(int pointerId, IntPtr windowBeingActivated, HT hitTest)
            : base(pointerId)
        {
            WindowBeingActivated = windowBeingActivated;
            HitTest = hitTest;
        }

        public IntPtr WindowBeingActivated { get; }
        public HT HitTest { get; }

        public override string ToString() => base.ToString() + ",W=" + WindowBeingActivated + ",HT=" + HitTest;
    }
}
