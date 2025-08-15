namespace Wice;

/// <summary>
/// Defines how content is resized to fit within a given layout slot.
/// </summary>
/// <remarks>
/// - None: No scaling; content retains its natural size.
/// - Fill: Scales to fill the slot without preserving aspect ratio (may distort).
/// - Uniform: Scales uniformly to fit entirely within the slot; aspect ratio preserved (letterboxing/pillarboxing possible).
/// - UniformToFill: Scales uniformly to fill the entire slot; aspect ratio preserved (content may be clipped).
/// </remarks>
public enum Stretch
{
    /// <summary>
    /// No scaling is applied. The content is arranged at its natural size.
    /// </summary>
    None,

    /// <summary>
    /// The content is resized to exactly fill the available space.
    /// The aspect ratio is not preserved and distortion may occur.
    /// </summary>
    Fill,

    /// <summary>
    /// The content is uniformly scaled to fit within the available space while preserving its aspect ratio.
    /// The entire content is visible; empty margins may appear.
    /// </summary>
    Uniform,

    /// <summary>
    /// The content is uniformly scaled to completely fill the available space while preserving its aspect ratio.
    /// Parts of the content may be clipped.
    /// </summary>
    UniformToFill
}
