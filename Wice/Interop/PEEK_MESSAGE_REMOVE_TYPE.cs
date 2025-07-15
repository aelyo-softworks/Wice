namespace Wice.Interop;

[Flags]
public enum PEEK_MESSAGE_REMOVE_TYPE : uint
{
    PM_NOREMOVE = 0,
    PM_REMOVE = 1,
    PM_NOYIELD = 2,
    PM_QS_INPUT = 67567616,
    PM_QS_POSTMESSAGE = 9961472,
    PM_QS_PAINT = 2097152,
    PM_QS_SENDMESSAGE = 4194304,
}
