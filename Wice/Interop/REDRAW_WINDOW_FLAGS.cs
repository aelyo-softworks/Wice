namespace Wice.Interop;

[Flags]
public enum REDRAW_WINDOW_FLAGS : uint
{
    RDW_INVALIDATE = 1,
    RDW_INTERNALPAINT = 2,
    RDW_ERASE = 4,
    RDW_VALIDATE = 8,
    RDW_NOINTERNALPAINT = 16,
    RDW_NOERASE = 32,
    RDW_NOCHILDREN = 64,
    RDW_ALLCHILDREN = 128,
    RDW_UPDATENOW = 256,
    RDW_ERASENOW = 512,
    RDW_FRAME = 1024,
    RDW_NOFRAME = 2048,
}
