namespace Wice;

/// <summary>
/// Invalidation reason raised when a window's DPI changes.
/// </summary>
/// <param name="newDpi">
/// The new DPI for the window/client area after the change, with separate horizontal (X) and vertical (Y) components.
/// </param>
/// <remarks>
/// Use this reason to route DPI-specific invalidation through the visual tree so parents can tailor behavior
/// (e.g., layout and bitmap size clamping). The string representation includes the DPI for diagnostics.
/// </remarks>
public class DpiChangedInvalidateReason(D2D_SIZE_U newDpi)
    : InvalidateReason(typeof(Window))
{
    /// <summary>
    /// Gets the new DPI to apply, represented as separate horizontal and vertical components.
    /// </summary>
    public D2D_SIZE_U NewDpi { get; } = newDpi;

    /// <summary>
    /// Builds the base diagnostic string and appends the new DPI in brackets.
    /// </summary>
    /// <returns>A string suitable for debugging/logging.</returns>
    protected override string GetBaseString() => base.GetBaseString() + "[" + NewDpi + "]";
}
