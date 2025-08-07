namespace Wice.Interop;

[Flags]
public enum IME_CPS : uint
{
    CPS_COMPLETE = 0x0001,
    CPS_CONVERT = 0x0002,
    CPS_REVERT = 0x0003,
    CPS_CANCEL = 0x0004,
}
