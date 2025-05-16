namespace Wice.Utilities;

public class AssemblyResourceStreamer : IReadStreamer
{
    public AssemblyResourceStreamer(Assembly assembly, string streamName)
    {
        Assembly = assembly ?? Assembly.GetCallingAssembly();
        ExceptionExtensions.ThrowIfNull(streamName, nameof(streamName));

        StreamName = streamName;
    }

    public Assembly Assembly { get; }
    public string StreamName { get; }

    public Stream GetReadStream() => Assembly.GetManifestResourceStream(StreamName)!;
}
