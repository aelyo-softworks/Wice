namespace Wice;

[Serializable]
public class WiceException : Exception
{
    public const string Prefix = "WICE";

    public WiceException()
        : base(Prefix + "0001: UI exception.")
    {
    }

    public WiceException(string message)
        : base(Prefix + message)
    {
    }

    public WiceException(string message, Exception innerException)
        : base(Prefix + message, innerException)
    {
    }

    public WiceException(Exception innerException)
        : base(null, innerException)
    {
    }

    public static int GetCode(string message)
    {
        if (message == null)
            return -1;

        if (!message.StartsWith(Prefix, StringComparison.Ordinal))
            return -1;

        var pos = message.IndexOf(":", Prefix.Length, StringComparison.Ordinal);
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

    public int Code => GetCode(Message);
}
