using System.Runtime.InteropServices.Marshalling;

namespace Wice.Interop;

[GeneratedComInterface, Guid("629BDBC8-D932-4FF4-96B9-8D96C5C1E858")]
public partial interface IPropertyValueStatics : IInspectable
{
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateEmpty(out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateUInt8(byte value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateInt16(short value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateUInt16(ushort value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateInt32(int value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateUInt32(uint value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateInt64(long value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateUInt64(ulong value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateSingle(float value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateDouble(double value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateChar16(char value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateBoolean([MarshalAs(UnmanagedType.Bool)] bool value, out nint propertyValue);

    [PreserveSig]
    HRESULT CreateString(HSTRING value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateInspectable([MarshalUsing(typeof(UniqueComInterfaceMarshaller<object>))] object value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateGuid(Guid value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateDateTime(DateTime value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateTimeSpan(TimeSpan value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreatePoint(Point value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateSize(Size value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateRect(Rect value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateUInt8Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateInt16Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] short[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateUInt16Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ushort[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateInt32Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateUInt32Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateInt64Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] long[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateUInt64Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ulong[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateSingleArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] float[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateDoubleArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] double[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateChar16Array(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateBooleanArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, ArraySubType = UnmanagedType.Bool)] bool[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateStringArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] nint[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateInspectableArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] nint[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateGuidArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Guid[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateDateTimeArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] DateTime[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateTimeSpanArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] TimeSpan[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreatePointArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Point[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateSizeArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Size[] value, out nint propertyValue);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT CreateRectArray(int valueSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Rect[] value, out nint propertyValue);
}
