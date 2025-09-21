namespace Wice.Utilities;

/// <summary>
/// Utilities to load embedded resources as Windows Imaging Component (WIC) bitmap sources.
/// </summary>
public static class ResourcesUtilities
{
    /// <summary>
    /// Loads an embedded manifest resource stream by its exact name and decodes it into a WIC bitmap source.
    /// </summary>
    /// <param name="assembly">
    /// The assembly that contains the embedded resource. If <c>null</c>, the calling assembly is used.
    /// </param>
    /// <param name="name">The fully qualified manifest resource name to load.</param>
    /// <param name="metadataOptions">
    /// Decode options controlling WIC metadata caching behavior. Defaults to <see cref="WICDecodeOptions.WICDecodeMetadataCacheOnDemand"/>.
    /// </param>
    /// <returns>
    /// An <c>IComObject&lt;IWICBitmapSource&gt;</c> if the resource exists and is decoded successfully; otherwise, <c>null</c> if the resource is not found.
    /// </returns>
    public static IComObject<IWICBitmapSource>? GetWicBitmapSource(Assembly assembly, string name, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ExceptionExtensions.ThrowIfNull(name, nameof(name));

        assembly ??= Assembly.GetCallingAssembly();
        var stream = assembly.GetManifestResourceStream(name);
        if (stream == null)
            return null;

        return WicUtilities.LoadBitmapSource(stream, metadataOptions);
    }

    /// <summary>
    /// Searches the assembly's manifest resource names using a predicate and decodes the first match into a WIC bitmap source.
    /// </summary>
    /// <param name="assembly">
    /// The assembly that contains the embedded resources. If <c>null</c>, the calling assembly is used.
    /// </param>
    /// <param name="predicate">
    /// A function used to select the desired resource name from <see cref="Assembly.GetManifestResourceNames()"/>.
    /// The first name for which the predicate returns <c>true</c> will be used.
    /// </param>
    /// <param name="metadataOptions">
    /// Decode options controlling WIC metadata caching behavior. Defaults to <see cref="WICDecodeOptions.WICDecodeMetadataCacheOnDemand"/>.
    /// </param>
    /// <returns>
    /// An <c>IComObject&lt;IWICBitmapSource&gt;</c> if a matching resource exists and is decoded successfully; otherwise, <c>null</c> if no match is found.
    /// </returns>
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
