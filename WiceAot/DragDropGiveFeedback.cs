namespace Wice;

public class DragDropGiveFeedback(DROPEFFECT effect) : EventArgs
{
    // TODO: remove once in DirectN
#if NETFRAMEWORK
    public const int DRAGDROP_S_DROP = 0x00040100;
    public const int DRAGDROP_S_CANCEL = 0x00040101;
    public const int DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;
#endif

    public DROPEFFECT Effect { get; } = effect;
    public HRESULT Result { get; set; } =
#if NETFRAMEWORK
        DRAGDROP_S_USEDEFAULTCURSORS;
#else
        Constants.DRAGDROP_S_USEDEFAULTCURSORS;
#endif
}
