namespace Wice.Interop;

[Flags]
public enum SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS : uint
{
    SPIF_UPDATEINIFILE = 1,
    SPIF_SENDCHANGE = 2,
    SPIF_SENDWININICHANGE = 2,
}
