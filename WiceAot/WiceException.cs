namespace Wice;

/// <summary>
/// Represents a domain-specific exception for Wice UI operations.
/// </summary>
/// <remarks>
/// Messages produced or passed to this exception are expected to begin with a prefix and numeric code
/// in the form "WICE####: <message>". The numeric portion can be extracted using <see cref="GetCode(string)"/>
/// or via the <see cref="Code"/> property.
/// </remarks>
/// <example>
/// throw new WiceException("0002: Operation failed.");
/// </example>
[Serializable]
public class WiceException : Exception
{
    /// <summary>
    /// The textual prefix applied to all Wice exception messages (e.g., "WICE0001: ...").
    /// </summary>
    /// <remarks>
    /// Combined with a four-digit code and a colon, for example: <c>WICE0001: UI exception.</c>
    /// </remarks>
    public const string Prefix = "WICE";

    /// <summary>
    /// Initializes a new instance of the <see cref="WiceException"/> class
    /// with the default message <c>WICE0001: UI exception.</c>.
    /// </summary>
    public WiceException()
        : base(Prefix + "0001: UI exception.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WiceException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">
    /// The error message text to append after the prefix. Typically includes the numeric code and colon,
    /// e.g., <c>0002: Details about the error.</c>
    /// </param>
    public WiceException(string message)
        : base(Prefix + message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WiceException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">
    /// The error message text to append after the prefix. Typically includes the numeric code and colon,
    /// e.g., <c>0003: Additional details.</c>
    /// </param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public WiceException(string message, Exception innerException)
        : base(Prefix + message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WiceException"/> class that wraps an inner exception.
    /// </summary>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <remarks>
    /// The base <see cref="Exception.Message"/> may be a framework-provided default when a null message is supplied.
    /// </remarks>
    public WiceException(Exception innerException)
        : base(null, innerException)
    {
    }

    /// <summary>
    /// Attempts to extract the numeric Wice error code from a message string that begins with the WICE prefix.
    /// </summary>
    /// <param name="message">A message in the format <c>WICE####: ...</c>. If null or malformed, parsing fails.</param>
    /// <returns>
    /// The parsed integer code (e.g., <c>1</c> for <c>WICE0001</c>), or <c>-1</c> if the message is null,
    /// does not start with the expected prefix, lacks a colon delimiter, or the numeric portion cannot be parsed.
    /// </returns>
    public static int GetCode(string message)
    {
        if (message == null)
            return -1;

        if (!message.StartsWith(Prefix, StringComparison.Ordinal))
            return -1;

        var pos = message.IndexOf(':', Prefix.Length);
        if (pos < 0)
            return -1;

#if NETFRAMEWORK
        if (int.TryParse(message.Substring(Prefix.Length, pos - Prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var i))
#else
        if (int.TryParse(message.AsSpan(Prefix.Length, pos - Prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var i))
#endif
            return i;

        return -1;
    }

    /// <summary>
    /// Gets the numeric Wice error code parsed from the <see cref="Exception.Message"/> of this instance.
    /// </summary>
    /// <value>
    /// The integer code extracted from the message, or <c>-1</c> if the message does not conform to the expected format.
    /// </value>
    public int Code => GetCode(Message);
}
