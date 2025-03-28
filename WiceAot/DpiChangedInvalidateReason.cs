namespace Wice;

public class DpiChangedInvalidateReason : InvalidateReason
{
    public DpiChangedInvalidateReason(D2D_SIZE_U newDpi)
        : base(typeof(Window))
    {
    }

    public D2D_SIZE_U NewDpi { get; }

    protected override string GetBaseString() => base.GetBaseString() + "[" + NewDpi + "]";
}
