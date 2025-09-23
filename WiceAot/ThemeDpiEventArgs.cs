namespace Wice;

/// <summary>
/// Event data describing a DPI change for a theme/window context.
/// </summary>
/// <param name="oldDpi">
/// The DPI in effect before the change. Expressed as dots per inch (DPI), where 96 represents 100% scaling.
/// </param>
/// <param name="newDpi">
/// The DPI in effect after the change. Expressed as dots per inch (DPI), where 96 represents 100% scaling.
/// </param>
public class ThemeDpiEventArgs(uint oldDpi, uint newDpi)
{
    /// <summary>
    /// Gets the DPI that was in effect prior to the change.
    /// </summary>
    public uint OldDpi { get; } = oldDpi;

    /// <summary>
    /// Gets the DPI that is in effect after the change.
    /// </summary>
    public uint NewDpi { get; } = newDpi;

    /// <inheritdoc/>
    public override string ToString() => $"{OldDpi} => {NewDpi}";

    /// <summary>
    /// Creates an instance whose <see cref="OldDpi"/> and <see cref="NewDpi"/> are both set to the
    /// specified window's current DPI (or the system default when <paramref name="window"/> is null).
    /// </summary>
    /// <param name="window">The window from which to read the current DPI; may be null.</param>
    /// <returns>
    /// A <see cref="ThemeDpiEventArgs"/> initialized with the same DPI for both old and new values.
    /// Useful when raising a DPI-aware event without a prior change.
    /// </returns>
    public static ThemeDpiEventArgs FromWindow(Window? window)
    {
        var dpi = window?.Dpi ?? WiceCommons.USER_DEFAULT_SCREEN_DPI;
        return new ThemeDpiEventArgs(dpi, dpi);
    }
}
