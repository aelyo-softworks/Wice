namespace Wice.Utilities
{
    public static class ResourcesUtilities
    {
        public static IComObject<IWICBitmapSource>? GetWicBitmapSource(Assembly assembly, string name, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            ArgumentNullException.ThrowIfNull(name);

            assembly = assembly ?? Assembly.GetCallingAssembly();
            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                return null;

            return WicUtilities.LoadBitmapSource(stream, metadataOptions);
        }

        public static IComObject<IWICBitmapSource>? GetWicBitmapSource(Assembly assembly, Func<string, bool> predicate, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            assembly = assembly ?? Assembly.GetCallingAssembly();
            var name = assembly.GetManifestResourceNames().FirstOrDefault(predicate);
            if (name == null)
                return null;

            return GetWicBitmapSource(assembly, name, metadataOptions);
        }
    }
}
