namespace Wice.Utilities;

public class WicUtilities
{
    public static IComObject<IWICBitmapSource> LoadBitmapSource(string filePath, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using var decoder = WicImagingFactory.CreateDecoderFromFilename(filePath, metadataOptions: metadataOptions);
        using var frame = decoder.GetFrame(0);
        using var converter = WicImagingFactory.CreateFormatConverter();
        var format = Constants.GUID_WICPixelFormat32bppPBGRA;
        converter.Object.Initialize(frame.Object, format, WICBitmapDitherType.WICBitmapDitherTypeNone, null!, 0, WICBitmapPaletteType.WICBitmapPaletteTypeCustom).ThrowOnError();
        return new ComObject<IWICBitmapSource>(converter);
    }

    public static IComObject<IWICBitmapSource> LoadBitmapSource(Stream stream, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var decoder = WicImagingFactory.CreateDecoderFromStream(stream, metadataOptions: metadataOptions);
        using var frame = decoder.GetFrame(0);
        using var converter = WicImagingFactory.CreateFormatConverter();
        var format = Constants.GUID_WICPixelFormat32bppPBGRA;
        converter.Object.Initialize(frame.Object, format, WICBitmapDitherType.WICBitmapDitherTypeNone, null!, 0, WICBitmapPaletteType.WICBitmapPaletteTypeCustom).ThrowOnError();
        return new ComObject<IWICBitmapSource>(converter);
    }
}
