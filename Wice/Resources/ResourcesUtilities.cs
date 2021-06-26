using System;
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
            return WICFunctions.LoadBitmapSource(stream, metadataOptions);
        }
    }
}
