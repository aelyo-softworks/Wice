namespace Wice.Interop;

[ComImport, Guid("A1BEA8BA-D726-4663-8129-6B5E7927FFA6"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface ICompositionTarget
{

    Windows.UI.Composition.Visual Root { get; set; }
}
