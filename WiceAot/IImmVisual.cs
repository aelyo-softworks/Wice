namespace Wice;

/// <summary>
/// Contract for visuals that can control the position of the legacy IMM/IME composition window
/// so it follows their caret or text input region.
/// </summary>
/// <remarks>
/// Implementations typically compute the caret or composition bounds and request the host window
/// to move the IME composition window accordingly. This is useful when the caret moves, focus changes,
/// layout updates, or DPI changes occur.
/// </remarks>
/// <seealso cref="Window.SetImmCompositionWindowPosition(Visual)"/>
public interface IImmVisual
{
    /// <summary>
    /// Requests the specified <paramref name="window"/> to position the IME/IMM composition window
    /// so it visually tracks the caret or input region of the implementing visual.
    /// </summary>
    /// <param name="window">The window that owns this visual and hosts the IME.</param>
    /// <returns>
    /// true if the composition window position was successfully updated; otherwise, false.
    /// </returns>
    /// <remarks>
    /// An implementation should compute the desired screen-relative location for the IME composition UI
    /// and use <see cref="Window.SetImmCompositionWindowPosition(Visual)"/> (or equivalent native calls)
    /// to reposition it.
    /// </remarks>
    bool SetImmCompositionWindowPosition(Window window);
}
