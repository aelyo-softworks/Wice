namespace Wice.Interop;

[Flags]
public enum TRACKMOUSEEVENT_FLAGS : uint
{
    TME_CANCEL = 2147483648,
    TME_HOVER = 1,
    TME_LEAVE = 2,
    TME_NONCLIENT = 16,
    TME_QUERY = 1073741824,
}
