namespace Wice;

/// <summary>
/// Provides flags that describe special behaviors for base object properties.
/// </summary>
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
    WriteRequiresMainThread,
}
