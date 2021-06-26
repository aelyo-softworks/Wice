using System;

namespace Wice
{
    [Flags]
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute
    public enum CompositionUpdateParts
#pragma warning restore CA2217 // Do not mark enums with FlagsAttribute
    {
        None = 0x0,
        RotationAngle = 0x1,
        RotationAngleInDegrees = RotationAngle,
        RotationAxis = 0x2,
        Scale = 0x4,
        Offset = 0x8,
        Size = 0x10,
        ZIndex = 0x20,
        TransformMatrix = 0x40,
        Opacity = 0x80,
        CompositeMode = 0x100,
        Shadow = 0x200,
        Brushes = 0x400,
        IsVisible = 0x800,
        Clip = 0x1000,
        Effect = 0x2000,
        D2DSurface = 0x4000,

        All = 0x7FFFFFFF,
    }
}
