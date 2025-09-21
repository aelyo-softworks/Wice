namespace Wice.Utilities;

/// <summary>
/// Provides extension methods related to exception guard clauses.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Throws an <see cref="System.ArgumentNullException"/> if the target object is <c>null</c>.
    /// </summary>
    /// <param name="obj">The object instance to validate.</param>
    /// <param name="paramName">The name of the parameter that represents <paramref name="obj"/>.</param>
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
