using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Wice
{
    [Serializable]
    public class UIException : Exception
    {
        public const string Prefix = "UIX";

        public UIException()
            : base(Prefix + "0001: UI exception.")
        {
        }

        public UIException(string message)
            : base(Prefix + message)
        {
        }

        public UIException(string message, Exception innerException)
            : base(Prefix + message, innerException)
        {
        }

        public UIException(Exception innerException)
            : base(null, innerException)
        {
        }

        protected UIException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static int GetCode(string message)
        {
            if (message == null)
                return -1;

            if (!message.StartsWith(Prefix, StringComparison.Ordinal))
                return -1;

            var pos = message.IndexOf(":", Prefix.Length, StringComparison.Ordinal);
            if (pos < 0)
                return -1;

            if (int.TryParse(message.Substring(Prefix.Length, pos - Prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out int i))
                return i;

            return -1;
        }

        public int Code => GetCode(Message);
    }
}
