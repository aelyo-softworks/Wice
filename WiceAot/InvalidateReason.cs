namespace Wice;

public class InvalidateReason
{
    public InvalidateReason(Type type, InvalidateReason? innerReason = null)
    {
        ArgumentNullException.ThrowIfNull(type);
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
            typeName = typeName[..^typeof(InvalidateReason).Name.Length];
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
