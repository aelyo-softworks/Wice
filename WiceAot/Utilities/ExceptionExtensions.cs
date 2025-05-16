namespace Wice.Utilities;

internal static class ExceptionExtensions
{
    public static void ThrowIfNull(this object? obj, string paramName)
    {
#if NETFRAMEWORK
        if (paramName == null)
            throw new ArgumentNullException(nameof(paramName));
#else
        ArgumentNullException.ThrowIfNull(paramName);
#endif

        if (obj == null)
            throw new ArgumentNullException(paramName);
    }
}
