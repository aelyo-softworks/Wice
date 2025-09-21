namespace Wice;

/// <summary>
/// Contract for visuals that can control the position of the legacy IMM/IME composition window
/// so it follows their caret or text input region.
/// </summary>
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
    bool SetImmCompositionWindowPosition(Window window);
}
