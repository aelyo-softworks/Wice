namespace Wice;

/// <summary>
/// Describes how to move or extend the caret/selection within a text box.
/// </summary>
/// <remarks>
/// - "Cluster" refers to a Unicode grapheme cluster (a user-perceived character).
/// - "Absolute" positions are typically used for pointing-device interactions and indicate
///   the leading or trailing edge of the grapheme cluster at a specific point.
/// - Consumers can interpret these values either to move the caret or to extend the current selection.
/// </remarks>
public enum TextBoxSetSelection
{
    /// <summary>
    /// Move to the previous grapheme cluster (left).
    /// </summary>
    Left,

    /// <summary>
    /// Move to the next grapheme cluster (right).
    /// </summary>
    Right,

    /// <summary>
    /// Move up by one visual line.
    /// </summary>
    Up,

    /// <summary>
    /// Move down by one visual line.
    /// </summary>
    Down,

    /// <summary>
    /// Move up by one page.
    /// </summary>
    PageUp,

    /// <summary>
    /// Move down by one page.
    /// </summary>
    PageDown,

    /// <summary>
    /// Move left by a single character. Commonly used by backspace operations.
    /// </summary>
    LeftChar,

    /// <summary>
    /// Move right by a single character.
    /// </summary>
    RightChar,

    /// <summary>
    /// Move to the beginning of the previous word.
    /// </summary>
    LeftWord,

    /// <summary>
    /// Move to the end of the next word.
    /// </summary>
    RightWord,

    /// <summary>
    /// Move to the start of the current line.
    /// </summary>
    Home,

    /// <summary>
    /// Move to the end of the current line.
    /// </summary>
    End,

    /// <summary>
    /// Move to the very first position in the document.
    /// </summary>
    First,

    /// <summary>
    /// Move to the very last position in the document.
    /// </summary>
    Last,

    /// <summary>
    /// Move to an explicit position at the leading edge of the cluster (e.g., mouse click).
    /// </summary>
    AbsoluteLeading,

    /// <summary>
    /// Move to an explicit position at the trailing edge of the cluster.
    /// </summary>
    AbsoluteTrailing,

    /// <summary>
    /// Select all text in the document.
    /// </summary>
    All
}
