namespace Wice;

/// <summary>
/// Specifies options that control whether operations on a visual are executed immediately,
/// deferred, or both.
/// </summary>
[Flags]
public enum VisualDoOptions
{
    /// <summary>
    /// No restrictions; both immediate and deferred operations are allowed.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Restrict processing to immediate operations only.
    /// </summary>
    ImmediateOnly = 0x1,

    /// <summary>
    /// Restrict processing to deferred operations only.
    /// </summary>
    DeferredOnly = 0x2,
}
