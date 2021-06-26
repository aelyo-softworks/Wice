using System;

namespace Wice
{
    [Flags]
    public enum GetRectOptions
    {
        None = 0x0,
        StretchWidthToParent = 0x1,
        StretchHeightToParent = 0x2,
        KeepProportions = 0x4,

        Default = StretchWidthToParent | StretchHeightToParent,
    }
}
