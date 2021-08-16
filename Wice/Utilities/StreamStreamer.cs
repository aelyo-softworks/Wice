using System;
using System.IO;

namespace Wice.Utilities
{
    public class StreamStreamer : IReadStreamer
    {
        public event EventHandler<ValueEventArgs<Stream>> GetRead;

        protected virtual void OnGetRead(object sender, ValueEventArgs<Stream> e) => GetRead?.Invoke(sender, e);

        public Stream GetReadStream()
        {
            var e = new ValueEventArgs<Stream>(null, false);
            OnGetRead(this, e);
            return e.Value;
        }
    }
}
