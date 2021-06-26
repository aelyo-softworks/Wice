using System;

namespace Wice
{
    public interface IFocusableParent
    {
        Visual FocusableVisual { get; }
        Type FocusVisualShapeType { get; }
        float? FocusOffset { get; }
    }
}
