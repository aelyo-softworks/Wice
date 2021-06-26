using System;
using System.Runtime.InteropServices;
using Windows.UI.Composition;

namespace DirectN
{
    [Guid("A1BEA8BA-D726-4663-8129-6B5E7927FFA6"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface ICompositionTarget
    {
        Visual Root { get; set; }
    }
}
