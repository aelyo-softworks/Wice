namespace Wice.Interop;

public static class InitializeWithWindow
{
    public static void Initialize(object obj, HWND hwnd)
    {
        ExceptionExtensions.ThrowIfNull(obj, nameof(obj));
        var initialize = obj.AsComObject<IInitializeWithWindow>()!;
        initialize.Object.Initialize(hwnd);
    }
}
