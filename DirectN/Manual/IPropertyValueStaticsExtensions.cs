using System;

namespace DirectN
{
    public static class IPropertyValueStaticsExtensions
    {
        public static HRESULT CreateSingleArray(this IPropertyValueStatics statics, float[] value, out IntPtr ptr)
        {
            if (statics == null)
                throw new ArgumentNullException(nameof(statics));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return statics.CreateSingleArray(value.Length, value, out ptr);
        }
    }
}
