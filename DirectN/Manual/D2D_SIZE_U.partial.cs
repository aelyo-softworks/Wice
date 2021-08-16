﻿using System;
using System.Diagnostics;

namespace DirectN
{
    [DebuggerDisplay("{ToString(),nq}")]
    public partial struct D2D_SIZE_U : IEquatable<D2D_SIZE_U>
    {
        public D2D_SIZE_U(uint width, uint height)
        {
#if DEBUG
            if (width < 0)
                throw new ArgumentException(null, nameof(width));

            if (height < 0)
                throw new ArgumentException(null, nameof(height));

#endif
            this.width = width;
            this.height = height;
        }

        public D2D_SIZE_U(float width, float height)
        {
#if DEBUG
            if (width.IsNotSet())
                throw new ArgumentException(null, nameof(width));

            if (height.IsNotSet())
                throw new ArgumentException(null, nameof(height));

            if (width < 0)
                throw new ArgumentException(null, nameof(width));

            if (height < 0)
                throw new ArgumentException(null, nameof(height));

#endif
            this.width = width.FloorU();
            this.height = height.FloorU();
        }

        public override string ToString() => "W: " + width + " H: " + height;

        public bool IsZero => width == 0 && height == 0;
        public bool IsEmpty => width == 0 || height == 0;
        public bool Equals(D2D_SIZE_U other) => width == other.width && height == other.height;
        public override bool Equals(object obj) => obj is D2D_SIZE_U sz && Equals(sz);
        public override int GetHashCode() => width.GetHashCode() ^ height.GetHashCode();
        public static bool operator ==(D2D_SIZE_U left, D2D_SIZE_U right) => left.Equals(right);
        public static bool operator !=(D2D_SIZE_U left, D2D_SIZE_U right) => !(left == right);
        public tagSIZE ToSize() => new tagSIZE(width, height);
        public D2D_SIZE_F ToD2D_SIZE_F() => new D2D_SIZE_F(width, height);
    }
}
