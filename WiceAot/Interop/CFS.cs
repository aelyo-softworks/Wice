namespace Wice.Interop;

[Flags]
public enum CFS : uint
{
    CFS_DEFAULT = 0x0000,
    CFS_RECT = 0x0001,
    CFS_POINT = 0x0002,
    CFS_FORCE_POSITION = 0x0020,
    CFS_CANDIDATEPOS = 0x0040,
    CFS_EXCLUDE = 0x0080,
}
