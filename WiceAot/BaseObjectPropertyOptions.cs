namespace Wice;

/// <summary>
/// Provides flags that describe special behaviors for base object properties.
/// </summary>
/// <remarks>
/// Values can be combined as a bit field.
/// </remarks>
[Flags]
public enum BaseObjectPropertyOptions
{
    /// <summary>
    /// No special behavior is applied.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Writing to the property must be performed on the main (UI) thread.
    /// </summary>
    /// <remarks>
    /// Use this for thread-affine properties that interact with UI or other main-thread-only resources.
    /// </remarks>
    WriteRequiresMainThread,
}
