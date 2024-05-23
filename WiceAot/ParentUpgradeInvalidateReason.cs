namespace Wice;

public class ParentUpgradeInvalidateReason : InvalidateReason
{
    public ParentUpgradeInvalidateReason(Type type, Type childType, InvalidateMode initialModes, VisualPropertyInvalidateModes finalModes, InvalidateReason? innerReason = null)
        : base(type, innerReason)
    {
        ArgumentNullException.ThrowIfNull(childType);
        ChildType = childType;
        InitialModes = initialModes;
        FinalModes = finalModes;
    }

    public Type ChildType { get; }
    public InvalidateMode InitialModes { get; }
    public VisualPropertyInvalidateModes FinalModes { get; }

    protected override string GetBaseString() => base.GetBaseString() + "(" + ChildType.Name + ")[" + InitialModes + " -> " + FinalModes + "]";
}
