namespace Wice.Utilities;

public static class ResourcesUtilities
{
    public static IComObject<IWICBitmapSource>? GetWicBitmapSource(Assembly assembly, string name, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ExceptionExtensions.ThrowIfNull(name, nameof(name));

        assembly ??= Assembly.GetCallingAssembly();
        var stream = assembly.GetManifestResourceStream(name);
        if (stream == null)
            return null;

        return WicUtilities.LoadBitmapSource(stream, metadataOptions);
    }

    public static IComObject<IWICBitmapSource>? GetWicBitmapSource(Assembly assembly, Func<string, bool> predicate, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ExceptionExtensions.ThrowIfNull(predicate, nameof(predicate));
        assembly ??= Assembly.GetCallingAssembly();
        var name = assembly.GetManifestResourceNames().FirstOrDefault(predicate);
        if (name == null)
            return null;

        return GetWicBitmapSource(assembly, name, metadataOptions);
    }
}
