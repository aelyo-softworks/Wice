namespace Wice;

public class DpiChangedInvalidateReason(D2D_SIZE_U newDpi) : InvalidateReason(typeof(Window))
{
    public D2D_SIZE_U NewDpi { get; }

    protected override string GetBaseString() => base.GetBaseString() + "[" + NewDpi + "]";
}
