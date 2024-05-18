namespace Wice;

public class CollectionChangedInvalidateReason : InvalidateReason
{
    public CollectionChangedInvalidateReason(Type type, Type childType, NotifyCollectionChangedAction action, InvalidateReason? innerReason = null)
        : base(type, innerReason)
    {
        ArgumentNullException.ThrowIfNull(childType);
        ChildType = childType;
        Action = action;
    }

    public Type ChildType { get; }
    public NotifyCollectionChangedAction Action { get; }

    protected override string GetBaseString() => base.GetBaseString() + "[" + Action + "](" + ChildType.Name + ")";
}
