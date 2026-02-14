namespace Wice;

/// <summary>
/// Represents a keyboard access key (hotkey) made of a <see cref="VIRTUAL_KEY"/> and optional modifier flags.
/// </summary>
public class AccessKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccessKey"/> class with no key and no modifiers.
    /// </summary>
    public AccessKey()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessKey"/> class with the specified virtual key and no modifiers.
    /// </summary>
    /// <param name="key">The <see cref="VIRTUAL_KEY"/> to associate with this access key.</param>
    public AccessKey(VIRTUAL_KEY key)
        : this()
    {
        Key = key;
    }

    /// <summary>
    /// Gets a reusable <see cref="AccessKey"/> configured for the Enter/Return key (no modifiers).
    /// </summary>
    public static AccessKey Enter { get; } = new AccessKey(VIRTUAL_KEY.VK_RETURN);

    /// <summary>
    /// Gets a reusable <see cref="AccessKey"/> configured for the Escape key (no modifiers).
    /// </summary>
    public static AccessKey Escape { get; } = new AccessKey(VIRTUAL_KEY.VK_ESCAPE);

    /// <summary>
    /// Gets or sets the primary <see cref="VIRTUAL_KEY"/> for this access key.
    /// </summary>
    public virtual VIRTUAL_KEY Key { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Shift modifier must be pressed.
    /// </summary>
    public virtual bool WithShift { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the visual (Ctrl) modifier must be pressed.
    /// </summary>
    public virtual bool WithControl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Menu (Alt) modifier must be pressed.
    /// </summary>
    public virtual bool WithMenu { get; set; }

    /// <summary>
    /// Determines whether the specified <see cref="KeyEventArgs"/> matches this access key exactly,
    /// including the primary key and all modifier states.
    /// </summary>
    /// <param name="e">The key event to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="e"/> is non-null and its key and modifier flags
    /// equal this instance; otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool Matches(KeyEventArgs e)
    {
        if (e == null)
            return false;

        if (Key != e.Key)
            return false;

        if (WithShift != e.WithShift)
            return false;

        if (WithControl != e.WithControl)
            return false;

        if (WithMenu != e.WithMenu)
            return false;

        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var str = Key.ToString();
        if (WithShift)
        {
            str = "SHIFT + " + str;
        }

        if (WithMenu)
        {
            str = "ALT + " + str;
        }

        if (WithControl)
        {
            str = "CTL + " + str;
        }

        return str;
    }
}
