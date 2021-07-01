using System;
using System.Linq;
using System.Reflection;
using DirectN;

namespace Wice.Resources
{
    public static class ResourcesUtilities
    {
        public static ComObject<IWICBitmapSource> GetWicBitmapSource(Assembly assembly, string name, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            assembly = assembly ?? Assembly.GetCallingAssembly();
            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                return null;

            return WICFunctions.LoadBitmapSource(stream, metadataOptions);
        }

        public static ComObject<IWICBitmapSource> GetWicBitmapSource(Assembly assembly, Func<string, bool> predicate, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            assembly = assembly ?? Assembly.GetCallingAssembly();
            var name = assembly.GetManifestResourceNames().FirstOrDefault(predicate);
            if (name == null)
                return null;

            return GetWicBitmapSource(assembly, name, metadataOptions);
        }
    }
}
