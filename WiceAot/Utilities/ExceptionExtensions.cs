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
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description><paramref name="paramName"/> is <c>null</c>. On .NET Framework, this is validated explicitly; on modern targets it uses <see cref="System.ArgumentNullException.ThrowIfNull(object?)"/>.</description></item>
    /// <item><description><paramref name="obj"/> is <c>null</c>, in which case the exception references <paramref name="paramName"/>.</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// On .NET Framework targets, <paramref name="paramName"/> is checked using a traditional null check.
    /// On .NET (5+) targets, <see cref="System.ArgumentNullException.ThrowIfNull(object?)"/> is used for the <paramref name="paramName"/> validation.
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// object? value = null;
    /// value.ThrowIfNull(nameof(value)); // throws ArgumentNullException("value")
    /// </code>
    /// </example>
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
