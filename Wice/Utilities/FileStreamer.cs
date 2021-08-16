using System;
using System.IO;

namespace Wice.Utilities
{
    public class FileStreamer : IReadStreamer
    {
        public FileStreamer(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            FilePath = filePath;
        }

        public string FilePath { get; }

        public override string ToString() => FilePath;
        public virtual Stream GetReadStream() => new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }
}
