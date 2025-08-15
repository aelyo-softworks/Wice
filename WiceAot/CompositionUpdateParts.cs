namespace Wice;

/// <summary>
/// Flags that indicate which parts of a composition visual require updating.
/// Combine values with bitwise operations to represent multiple changes.
/// </summary>
/// <remarks>
/// This enum is marked with <see cref="System.FlagsAttribute"/>.
/// </remarks>
[Flags]
public enum CompositionUpdateParts
{
    /// <summary>
    /// No properties are updated.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// The rotation angle (in radians) has changed.
    /// </summary>
    RotationAngle = 0x1,

    /// <summary>
    /// Alias for <see cref="RotationAngle"/> when APIs expect degrees.
    /// </summary>
    RotationAngleInDegrees = RotationAngle,

    /// <summary>
    /// The 3D rotation axis has changed.
    /// </summary>
    RotationAxis = 0x2,

    /// <summary>
    /// The scale factors have changed.
    /// </summary>
    Scale = 0x4,

    /// <summary>
    /// The position (offset) has changed.
    /// </summary>
    Offset = 0x8,

    /// <summary>
    /// The size has changed.
    /// </summary>
    Size = 0x10,

    /// <summary>
    /// The Z-order index has changed.
    /// </summary>
    ZIndex = 0x20,

    /// <summary>
    /// The full transform matrix has changed.
    /// </summary>
    TransformMatrix = 0x40,

    /// <summary>
    /// The opacity has changed.
    /// </summary>
    Opacity = 0x80,

    /// <summary>
    /// The composite/blend mode has changed.
    /// </summary>
    CompositeMode = 0x100,

    /// <summary>
    /// The shadow configuration has changed.
    /// </summary>
    Shadow = 0x200,

    /// <summary>
    /// One or more brushes/materials have changed.
    /// </summary>
    Brushes = 0x400,

    /// <summary>
    /// The visibility state has changed.
    /// </summary>
    IsVisible = 0x800,

    /// <summary>
    /// The clipping geometry has changed.
    /// </summary>
    Clip = 0x1000,

    /// <summary>
    /// The effect chain has changed.
    /// </summary>
    Effect = 0x2000,

    /// <summary>
    /// The Direct2D surface content has changed.
    /// </summary>
    D2DSurface = 0x4000,

    /// <summary>
    /// All parts should be considered changed.
    /// </summary>
    All = 0x7FFFFFFF,
}
