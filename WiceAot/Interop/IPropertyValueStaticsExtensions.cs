namespace Wice.Interop;

public static class IPropertyValueStaticsExtensions
{
    public static HRESULT CreateSingleArray(this IPropertyValueStatics statics, float[] value, out IntPtr ptr)
    {
        ArgumentNullException.ThrowIfNull(statics);
        ArgumentNullException.ThrowIfNull(value);
        return statics.CreateSingleArray(value.Length, value, out ptr);
    }
}
