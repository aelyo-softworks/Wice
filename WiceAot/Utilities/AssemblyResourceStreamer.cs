namespace Wice.Utilities;

/// <summary>
/// Provides an <see cref="IReadStreamer"/> implementation that reads an embedded
/// manifest resource stream from a specified <see cref="System.Reflection.Assembly"/>.
/// </summary>
public class AssemblyResourceStreamer : IReadStreamer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyResourceStreamer"/> class.
    /// </summary>
    /// <param name="assembly">
    /// The assembly that contains the embedded resource. If <paramref name="assembly"/> is <see langword="null"/>,
    /// the calling assembly (<c>Assembly.GetCallingAssembly()</c>) is used.
    /// </param>
    /// <param name="streamName">
    /// The fully qualified manifest resource name to open. Cannot be <see langword="null"/>.
    /// </param>
    public AssemblyResourceStreamer(Assembly assembly, string streamName)
    {
        Assembly = assembly ?? Assembly.GetCallingAssembly();
        ExceptionExtensions.ThrowIfNull(streamName, nameof(streamName));

        StreamName = streamName;
    }

    /// <summary>
    /// Gets the assembly that contains the embedded manifest resource.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// Gets the fully qualified manifest resource name to be opened from <see cref="Assembly"/>.
    /// </summary>
    public string StreamName { get; }

    /// <summary>
    /// Gets a readable stream for the configured manifest resource.
    /// </summary>
    /// <returns>
    /// A readable <see cref="System.IO.Stream"/> for the resource, or <see langword="null"/> if the resource is not found.
    /// The caller owns the returned stream and should dispose it when no longer needed.
    /// </returns>
    public Stream GetReadStream() => Assembly.GetManifestResourceStream(StreamName)!;
}
