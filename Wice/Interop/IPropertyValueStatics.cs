using System;
using System.Runtime.InteropServices;
using DirectN;
using Windows.Foundation;

namespace Wice.Interop
{
#if NET
    [ComImport, Guid("629BDBC8-D932-4FF4-96B9-8D96C5C1E858"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyValueStatics : IInspectable
    {
        // IInspectable
        [PreserveSig]
        new HRESULT GetIids(out int iidCount, out IntPtr iids);

        [PreserveSig]
        new HRESULT GetRuntimeClassName([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] out string className);

        [PreserveSig]
        new HRESULT GetTrustLevel(out TrustLevel trustLevel);

#else
    // note: we can't use IInspectable or IPropertyValue for return values here, as the .NET Framework plays too many tricks with these
    [ComImport, Guid("629BDBC8-D932-4FF4-96B9-8D96C5C1E858"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface IPropertyValueStatics
    {
#endif
        [PreserveSig]
        HRESULT CreateEmpty(out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateUInt8(byte value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateInt16(short value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateUInt16(ushort value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateInt32(int value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateUInt32(uint value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateInt64(long value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateUInt64(ulong value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateSingle(float value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateDouble(double value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateChar16(char value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateBoolean(bool value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateString([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] string value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateInspectable([MarshalAs(UnmanagedType.IUnknown)] object value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateGuid(Guid value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateDateTime(DateTime value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateTimeSpan(TimeSpan value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreatePoint(Point value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateSize(Size value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateRect(Rect value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateUInt8Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateInt16Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] short[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateUInt16Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ushort[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateInt32Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateUInt32Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateInt64Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] long[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateUInt64Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ulong[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateSingleArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] float[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateDoubleArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] double[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateChar16Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateBooleanArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] bool[] value, out IntPtr propertyValue);

        [PreserveSig]
        // .NET Framework is ok with this, but .NET 5+ isn't
        // but we currently don't need it so we just let value as an IntPtr
        HRESULT CreateStringArray(int valueSize, IntPtr value, /* [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.HString, SizeParamIndex = 0)] string[] value,*/ out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateInspectableArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 0)] object[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateGuidArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Guid[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateDateTimeArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] DateTime[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateTimeSpanArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] TimeSpan[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreatePointArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Point[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateSizeArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Size[] value, out IntPtr propertyValue);

        [PreserveSig]
        HRESULT CreateRectArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Rect[] value, out IntPtr propertyValue);
    }
}
