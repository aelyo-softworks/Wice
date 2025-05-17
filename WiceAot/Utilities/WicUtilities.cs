namespace Wice.Utilities;

public class WicUtilities
{
    public static IComObject<IWICBitmapSource> LoadBitmapSource(string filePath, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
#if NETFRAMEWORK
        return WICFunctions.LoadBitmapSource(filePath, metadataOptions);
#else
        ArgumentNullException.ThrowIfNull(filePath);
        using var decoder = WicImagingFactory.CreateDecoderFromFilename(filePath, metadataOptions: metadataOptions);
        using var frame = decoder.GetFrame(0);
        var converter = WicImagingFactory.CreateFormatConverter();
        var format = Constants.GUID_WICPixelFormat32bppPBGRA;
        converter.Object.Initialize(frame.Object, format, WICBitmapDitherType.WICBitmapDitherTypeNone, null!, 0, WICBitmapPaletteType.WICBitmapPaletteTypeCustom).ThrowOnError();
        return converter.As<IWICBitmapSource>()!;
#endif
    }

    public static IComObject<IWICBitmapSource> LoadBitmapSource(Stream stream, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
#if NETFRAMEWORK
        return WICFunctions.LoadBitmapSource(stream, metadataOptions);
#else
        ArgumentNullException.ThrowIfNull(stream);
        using var decoder = WicImagingFactory.CreateDecoderFromStream(stream, metadataOptions: metadataOptions);
        using var frame = decoder.GetFrame(0);
        var converter = WicImagingFactory.CreateFormatConverter();
        var format = Constants.GUID_WICPixelFormat32bppPBGRA;
        converter.Object.Initialize(frame.Object, format, WICBitmapDitherType.WICBitmapDitherTypeNone, null!, 0, WICBitmapPaletteType.WICBitmapPaletteTypeCustom).ThrowOnError();
        return converter.As<IWICBitmapSource>()!;
#endif
    }

    public static IComObject<IWICBitmapSource> LoadBitmapSource(nint pointer, long byteLength)
    {
        if (pointer == 0)
            throw new ArgumentException(null, nameof(pointer));

        using var stream = new System.IO.UnmanagedMemoryStream(new IntPtrBuffer(pointer, byteLength), 0, byteLength);
        return LoadBitmapSource(stream, WICDecodeOptions.WICDecodeMetadataCacheOnLoad);
    }

#if NETFRAMEWORK
    public static IComObject<IWICBitmapSource> LoadBitmapSourceFromMemory(
        uint width,
        uint height,
        Guid pixelFormat,
        uint stride,
        uint bufferSize,
        nint pointer)
    {
        var buffer = new byte[bufferSize];
        Marshal.Copy(pointer, buffer, 0, (int)bufferSize);
        return WICImagingFactory.CreateBitmapFromMemory((int)width, (int)height, pixelFormat, (int)stride, buffer);
    }
#else
    public static IComObject<IWICBitmapSource> LoadBitmapSourceFromMemory(
        uint width,
        uint height,
        Guid pixelFormat,
        uint stride,
        uint bufferSize,
        nint pointer) => WicImagingFactory.WithFactory(factory =>
    {
        if (pointer == 0)
            throw new ArgumentException(null, nameof(pointer));

        factory.Object.CreateBitmapFromMemory(width, height, pixelFormat, stride, bufferSize, pointer, out var bmp).ThrowOnError();
        return new ComObject<IWICBitmapSource>(bmp);
    });
#endif
}
