using System.ComponentModel;
using System.Runtime.InteropServices.ComTypes;
using DirectN;
using Wice.Interop;

namespace Wice
{
    public class DragDropEventArgs : HandledEventArgs
    {
        public DragDropEventArgs(DragDropEventType type)
        {
            Type = type;
        }

        public DragDropEventType Type { get; internal set; }
        public IDataObject DataObject { get; set; }
        public MODIFIERKEYS_FLAGS KeyFlags { get; set; }
        public tagPOINT Point { get; set; }
        public DROPEFFECT Effect { get; set; }
    }
}