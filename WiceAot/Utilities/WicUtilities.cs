namespace Wice.Utilities;

public class WicUtilities
{
    public static IComObject<IWICBitmapSource> LoadBitmapSource(string filePath, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using var decoder = WicImagingFactory.CreateDecoderFromFilename(filePath, metadataOptions: metadataOptions);
        using var frame = decoder.GetFrame(0);
        var converter = WicImagingFactory.CreateFormatConverter();
        var format = Constants.GUID_WICPixelFormat32bppPBGRA;
        converter.Object.Initialize(frame.Object, format, WICBitmapDitherType.WICBitmapDitherTypeNone, null!, 0, WICBitmapPaletteType.WICBitmapPaletteTypeCustom).ThrowOnError();
        return converter.As<IWICBitmapSource>()!;
    }

    public static IComObject<IWICBitmapSource> LoadBitmapSource(Stream stream, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var decoder = WicImagingFactory.CreateDecoderFromStream(stream, metadataOptions: metadataOptions);
        using var frame = decoder.GetFrame(0);
        var converter = WicImagingFactory.CreateFormatConverter();
        var format = Constants.GUID_WICPixelFormat32bppPBGRA;
        converter.Object.Initialize(frame.Object, format, WICBitmapDitherType.WICBitmapDitherTypeNone, null!, 0, WICBitmapPaletteType.WICBitmapPaletteTypeCustom).ThrowOnError();
        return converter.As<IWICBitmapSource>()!;
    }

    public static IComObject<IWICBitmapSource> LoadBitmapSource(nint pointer, long byteLength)
    {
        ArgumentNullException.ThrowIfNull(pointer);
        using var stream = new System.IO.UnmanagedMemoryStream(new IntPtrBuffer(pointer, byteLength), 0, byteLength);
        return LoadBitmapSource(stream, WICDecodeOptions.WICDecodeMetadataCacheOnLoad);
    }

    public static IComObject<IWICBitmapSource> LoadBitmapSourceFromMemory(
        uint width,
        uint height,
        Guid pixelFormat,
        uint stride,
        uint bufferSize,
        nint pointer) => WicImagingFactory.WithFactory(factory =>
    {
        ArgumentNullException.ThrowIfNull(pointer);
        factory.Object.CreateBitmapFromMemory(width, height, pixelFormat, stride, bufferSize, pointer, out var bmp).ThrowOnError();
        return new ComObject<IWICBitmapSource>(bmp);
    });
}
