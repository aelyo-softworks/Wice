namespace Wice;

/// <summary>
/// Defines a contract for components that can display their textual content in a password-masked form.
/// </summary>
public interface IPasswordCapable
{
    /// <summary>
    /// Gets or sets a value indicating whether password masking is enabled for this component.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to display masked characters instead of the actual content; otherwise, <see langword="false"/>.
    /// </value>
    bool IsPasswordModeEnabled { get; set; }

    /// <summary>
    /// Sets the character used to mask content when password mode is enabled.
    /// </summary>
    /// <param name="character">
    /// The masking character to display in place of actual content (for example, '•' U+2022 or '*').
    /// </param>
    void SetPasswordCharacter(char character);
}
