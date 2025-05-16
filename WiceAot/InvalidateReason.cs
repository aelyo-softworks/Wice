namespace Wice;

public class InvalidateReason
{
    public InvalidateReason(Type type, InvalidateReason? innerReason = null)
    {
        ExceptionExtensions.ThrowIfNull(type, nameof(type));
        Type = type;
        InnerReason = innerReason;
    }

    public Type Type { get; }
    public InvalidateReason? InnerReason { get; }

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
