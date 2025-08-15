namespace Wice;

/// <summary>
/// Specifies options that control whether operations on a visual are executed immediately,
/// deferred, or both.
/// </summary>
/// <remarks>
/// This enumeration supports bitwise combination of its member values.
/// </remarks>
[Flags]
public enum VisualDoOptions
{
    /// <summary>
    /// No restrictions; both immediate and deferred operations are allowed.
    /// </summary>
    /// <remarks>Value: 0x0.</remarks>
    None = 0x0,

    /// <summary>
    /// Restrict processing to immediate operations only.
    /// </summary>
    /// <remarks>Value: 0x1.</remarks>
    ImmediateOnly = 0x1,

    /// <summary>
    /// Restrict processing to deferred operations only.
    /// </summary>
    /// <remarks>Value: 0x2.</remarks>
    DeferredOnly = 0x2,
}
