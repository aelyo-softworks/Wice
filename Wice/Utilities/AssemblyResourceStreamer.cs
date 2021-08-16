using System;
using System.IO;
using System.Reflection;

namespace Wice.Utilities
{
    public class AssemblyResourceStreamer : IReadStreamer
    {
        public AssemblyResourceStreamer(Assembly assembly, string streamName)
        {
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            if (streamName == null)
                throw new ArgumentNullException(nameof(streamName));

            StreamName = streamName;
        }

        public Assembly Assembly { get; }
        public string StreamName { get; }

        public Stream GetReadStream() => Assembly.GetManifestResourceStream(StreamName);
    }
}
