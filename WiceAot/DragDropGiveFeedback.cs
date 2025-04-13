namespace Wice;

public class DragDropGiveFeedback(DROPEFFECT effect) : EventArgs
{
    public DROPEFFECT Effect { get; } = effect;
    public HRESULT Result { get; set; } = Constants.DRAGDROP_S_USEDEFAULTCURSORS;
}
