namespace Wice;

/// <summary>
/// Defines a contract for components that can display their textual content in a password-masked form.
/// </summary>
/// <remarks>
/// Implementations are expected to render a masking character in place of the actual content when
/// <see cref="IsPasswordModeEnabled"/> is set to <see langword="true"/>.
/// This interface governs presentation only; it does not imply any storage, encryption, or security semantics.
/// </remarks>
public interface IPasswordCapable
{
    /// <summary>
    /// Gets or sets a value indicating whether password masking is enabled for this component.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to display masked characters instead of the actual content; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// When enabled, the component should render a masking character for each character in the underlying content.
    /// This affects visual representation only and does not secure or encrypt the underlying value.
    /// Implementations may trigger a visual refresh when this value changes.
    /// </remarks>
    bool IsPasswordModeEnabled { get; set; }

    /// <summary>
    /// Sets the character used to mask content when password mode is enabled.
    /// </summary>
    /// <param name="character">
    /// The masking character to display in place of actual content (for example, '•' U+2022 or '*').
    /// </param>
    /// <remarks>
    /// The chosen character should be a printable glyph available in the UI font. If password mode is not enabled,
    /// the setting may be stored and applied once <see cref="IsPasswordModeEnabled"/> is set to <see langword="true"/>.
    /// Implementations may validate the provided character and could throw an exception if it is unsupported.
    /// </remarks>
    void SetPasswordCharacter(char character);
}
