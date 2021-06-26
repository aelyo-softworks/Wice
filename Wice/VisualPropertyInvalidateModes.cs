using System;

namespace Wice
{
    [Flags]
    public enum VisualPropertyInvalidateModes
    {
        None = 0x0,
        Render = 0x1,
        Arrange = 0x2,
        Measure = 0x4,

        ParentRender = 0x8,
        ParentArrange = 0x10,
        ParentMeasure = 0x20,
    }
}
