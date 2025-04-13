namespace Wice;

public class DragDropEventArgs(DragDropEventType type) : HandledEventArgs
{
    public DragDropEventType Type { get; internal set; } = type;
    public IDataObject? DataObject { get; set; }
    public MODIFIERKEYS_FLAGS KeyFlags { get; set; }
    public POINT Point { get; set; }
    public DROPEFFECT Effect { get; set; }
}
