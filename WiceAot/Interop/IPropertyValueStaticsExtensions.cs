namespace Wice.Interop;

/// <summary>
/// Provides extension methods for the <see cref="IPropertyValueStatics"/> interface.
/// </summary>
public static class IPropertyValueStaticsExtensions
{
    /// <summary>
    /// Creates a property value that represents an array of single-precision floating-point numbers.
    /// </summary>
    /// <param name="statics">The <see cref="IPropertyValueStatics"/> instance used to create the property value.</param>
    /// <param name="value">The array of single-precision floating-point numbers to be represented as a property value. Cannot be <see
    /// langword="null"/>.</param>
    /// <param name="ptr">When this method returns, contains a pointer to the created property value. This parameter is passed
    /// uninitialized.</param>
    /// <returns>An <see cref="HRESULT"/> value indicating the success or failure of the operation.</returns>
    public static HRESULT CreateSingleArray(this IPropertyValueStatics statics, float[] value, out IntPtr ptr)
    {
        ArgumentNullException.ThrowIfNull(statics);
        ArgumentNullException.ThrowIfNull(value);
        return statics.CreateSingleArray(value.Length, value, out ptr);
    }
}
