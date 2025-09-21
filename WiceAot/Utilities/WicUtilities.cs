namespace Wice.Utilities;

/// <summary>
/// Utility helpers for working with Windows Imaging Component (WIC) and Direct2D.
/// Provides methods to load <see cref="IWICBitmapSource"/> from files, streams, unmanaged memory,
/// as well as mapping WIC pixel formats to <see cref="D2D1_PIXEL_FORMAT"/>.
/// </summary>
public class WicUtilities
{
    /// <summary>
    /// Loads an image from a file path and returns a WIC bitmap source converted to 32bpp premultiplied BGRA (PBGRA).
    /// </summary>
    /// <param name="filePath">Path to an encoded image file (e.g., PNG, JPEG, BMP, etc.).</param>
    /// <param name="metadataOptions">Decode metadata caching behavior. Defaults to <see cref="WICDecodeOptions.WICDecodeMetadataCacheOnDemand"/>.</param>
    /// <returns>An <see cref="IComObject{T}"/> wrapping an <see cref="IWICBitmapSource"/> in 32bpp PBGRA format.</returns>
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

    /// <summary>
    /// Loads an image from a stream and returns a WIC bitmap source converted to 32bpp premultiplied BGRA (PBGRA).
    /// </summary>
    /// <param name="stream">A readable stream containing an encoded image (e.g., PNG, JPEG, BMP, etc.).</param>
    /// <param name="metadataOptions">Decode metadata caching behavior. Defaults to <see cref="WICDecodeOptions.WICDecodeMetadataCacheOnDemand"/>.</param>
    /// <returns>An <see cref="IComObject{T}"/> wrapping an <see cref="IWICBitmapSource"/> in 32bpp PBGRA format.</returns>
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

    // https://learn.microsoft.com/en-us/windows/win32/Direct2D/supported-pixel-formats-and-alpha-modes#supported-formats-for-wic-bitmap-render-target
#if NETFRAMEWORK
    /// <summary>
    /// Maps a WIC pixel format GUID to a <see cref="D2D1_PIXEL_FORMAT"/> suitable for a WIC bitmap render target.
    /// </summary>
    /// <param name="wicPixelFormat">The WIC pixel format GUID.</param>
    /// <returns>
    /// A <see cref="D2D1_PIXEL_FORMAT"/> containing the corresponding <see cref="DXGI_FORMAT"/> and <see cref="D2D1_ALPHA_MODE"/>.
    /// </returns>
    public static D2D1_PIXEL_FORMAT GetDxgiFormat(Guid wicPixelFormat)
    {
        if (wicPixelFormat == WICConstants.GUID_WICPixelFormat32bppPBGRA)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED };

        if (wicPixelFormat == WICConstants.GUID_WICPixelFormat32bppBGRA)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_STRAIGHT };

        if (wicPixelFormat == WICConstants.GUID_WICPixelFormat32bppBGR)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_IGNORE };

        if (wicPixelFormat == WICConstants.GUID_WICPixelFormat32bppPRGBA)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED };

        if (wicPixelFormat == WICConstants.GUID_WICPixelFormat32bppRGBA)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_STRAIGHT };

        if (wicPixelFormat == WICConstants.GUID_WICPixelFormat32bppRGB)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_IGNORE };

        if (wicPixelFormat == WICConstants.GUID_WICPixelFormat64bppPRGBAHalf)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED };

        if (wicPixelFormat == WICConstants.GUID_WICPixelFormat64bppRGBAHalf)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_STRAIGHT };

        if (wicPixelFormat == WICConstants.GUID_WICPixelFormat64bppRGBHalf)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_IGNORE };

        throw new NotSupportedException($"Unsupported WIC pixel format: {wicPixelFormat}.");
    }
#else
    /// <summary>
    /// Maps a WIC pixel format GUID to a <see cref="D2D1_PIXEL_FORMAT"/> suitable for a WIC bitmap render target.
    /// </summary>
    /// <param name="wicPixelFormat">The WIC pixel format GUID.</param>
    /// <returns>
    /// A <see cref="D2D1_PIXEL_FORMAT"/> containing the corresponding <see cref="DXGI_FORMAT"/> and <see cref="D2D1_ALPHA_MODE"/>.
    /// </returns>
    public static D2D1_PIXEL_FORMAT GetDxgiFormat(Guid wicPixelFormat)
    {
        if (wicPixelFormat == Constants.GUID_WICPixelFormat32bppPBGRA)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED };

        if (wicPixelFormat == Constants.GUID_WICPixelFormat32bppBGRA)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_STRAIGHT };

        if (wicPixelFormat == Constants.GUID_WICPixelFormat32bppBGR)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_IGNORE };

        if (wicPixelFormat == Constants.GUID_WICPixelFormat32bppPRGBA)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED };

        if (wicPixelFormat == Constants.GUID_WICPixelFormat32bppRGBA)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_STRAIGHT };

        if (wicPixelFormat == Constants.GUID_WICPixelFormat32bppRGB)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_IGNORE };

        if (wicPixelFormat == Constants.GUID_WICPixelFormat64bppPRGBAHalf)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED };

        if (wicPixelFormat == Constants.GUID_WICPixelFormat64bppRGBAHalf)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_STRAIGHT };

        if (wicPixelFormat == Constants.GUID_WICPixelFormat64bppRGBHalf)
            return new D2D1_PIXEL_FORMAT { format = DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT, alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_IGNORE };

        throw new NotSupportedException($"Unsupported WIC pixel format: {wicPixelFormat}.");
    }
#endif

    /// <summary>
    /// Loads an image from an unmanaged memory buffer containing encoded image bytes.
    /// </summary>
    /// <param name="pointer">Pointer to the start of the encoded image buffer.</param>
    /// <param name="byteLength">Length, in bytes, of the encoded image buffer.</param>
    /// <returns>An <see cref="IComObject{T}"/> wrapping an <see cref="IWICBitmapSource"/>.</returns>
    public static IComObject<IWICBitmapSource> LoadBitmapSource(nint pointer, long byteLength)
    {
        if (pointer == 0)
            throw new ArgumentException(null, nameof(pointer));

        using var stream = new System.IO.UnmanagedMemoryStream(new IntPtrBuffer(pointer, byteLength), 0, byteLength);
        return LoadBitmapSource(stream, WICDecodeOptions.WICDecodeMetadataCacheOnLoad);
    }

#if NETFRAMEWORK
    /// <summary>
    /// Creates a WIC bitmap from raw pixel memory by copying into a managed buffer (Framework-specific path).
    /// </summary>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="pixelFormat">The WIC pixel format GUID of the buffer (e.g., GUID_WICPixelFormat32bppPBGRA).</param>
    /// <param name="stride">The number of bytes per scanline of the source buffer.</param>
    /// <param name="bufferSize">Total size, in bytes, of the source buffer.</param>
    /// <param name="pointer">Pointer to the start of the raw pixel buffer.</param>
    /// <returns>An <see cref="IComObject{T}"/> wrapping an <see cref="IWICBitmapSource"/> backed by the copied buffer.</returns>
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
    /// <summary>
    /// Creates a WIC bitmap from raw pixel memory without copying (passes the unmanaged pointer to WIC).
    /// </summary>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="pixelFormat">The WIC pixel format GUID of the buffer (e.g., <c>GUID_WICPixelFormat32bppPBGRA</c>).</param>
    /// <param name="stride">The number of bytes per scanline of the source buffer.</param>
    /// <param name="bufferSize">Total size, in bytes, of the source buffer.</param>
    /// <param name="pointer">Pointer to the start of the raw pixel buffer.</param>
    /// <returns>An <see cref="IComObject{T}"/> wrapping an <see cref="IWICBitmapSource"/> that references the provided memory.</returns>
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
