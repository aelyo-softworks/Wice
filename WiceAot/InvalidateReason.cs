namespace Wice;

/// <summary>
/// Represents a reason that caused an invalidation within the system.
/// </summary>
public class InvalidateReason
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidateReason"/> class.
    /// </summary>
    /// <param name="type">The <see cref="System.Type"/> associated with this invalidation reason. Must not be <c>null</c>.</param>
    /// <param name="innerReason">An optional inner reason to compose a chain of invalidation causes.</param>
    public InvalidateReason(Type type, InvalidateReason? innerReason = null)
    {
        ExceptionExtensions.ThrowIfNull(type, nameof(type));
        Type = type;
        InnerReason = innerReason;
    }

    /// <summary>
    /// Gets the <see cref="System.Type"/> associated with this invalidation reason.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the optional inner reason used to build a chain of invalidation causes.
    /// </summary>
    public InvalidateReason? InnerReason { get; }

    /// <summary>
    /// Builds the base string representation for this reason without the chained inner reasons.
    /// </summary>
    /// <returns>
    /// A string in the form "<c>{ReasonName}({TypeName})</c>", where "<c>ReasonName</c>" is the class name
    /// without the "<c>InvalidateReason</c>" suffix if present, and "<c>TypeName</c>" is the name of <see cref="Type"/>.
    /// </returns>
    protected virtual string GetBaseString()
    {
        var typeName = GetType().Name;
        if (typeName.EndsWith(typeof(InvalidateReason).Name))
        {
#if NETFRAMEWORK
            typeName = typeName.Substring(0, typeName.Length - typeof(InvalidateReason).Name.Length);
#else
            typeName = typeName[..^typeof(InvalidateReason).Name.Length];
#endif
        }
        return typeName + "(" + Type.Name + ")";
    }

    /// <inheritdoc>
    public override string ToString()
    {
        var str = GetBaseString();
        if (InnerReason != null)
        {
            str += " <= " + InnerReason.ToString();
        }
        return str;
    }
}
