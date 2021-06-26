using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Wice.Utilities
{
    public class AuditRecord : IEquatable<AuditRecord>
    {
        private const char _csvSep = '\t';

        public AuditRecord(AuditRecordType type, string message, [CallerMemberName] string methodName = null)
        {
            FirstOccurrenceUtc = DateTime.UtcNow;
            Type = type;
            Message = message;
            IncrementCount();
            Method = methodName;
        }

        public AuditRecordType Type { get; private set; }
        public string Message { get; private set; }
        public string Method { get; private set; }
        public virtual DateTime FirstOccurrenceUtc { get; private set; }
        public virtual DateTime LastOccurrenceUtc { get; private set; }
        public virtual int Count { get; private set; }

        public virtual void WriteHeader(TextWriter writer)
        {
            if (writer == null)
                return;

            WriterCsvEscape(writer, "FirstOccurrenctUtc", true);
            WriterCsvEscape(writer, "LastOccurrenceUtc", true);
            WriterCsvEscape(writer, "Count", true);
            WriterCsvEscape(writer, "ThreadId", true);
            WriterCsvEscape(writer, "Type", true);
            WriterCsvEscape(writer, "Method", true);
            WriterCsvEscape(writer, "Message", false);
        }

        public virtual void Write(TextWriter writer)
        {
            if (writer == null)
                return;

            WriterCsvEscape(writer, string.Format("{0:o}", FirstOccurrenceUtc), true);
            if (Count > 1)
            {
                WriterCsvEscape(writer, string.Format("{0:o}", LastOccurrenceUtc), true);
                WriterCsvEscape(writer, Count.ToString(), true);
            }
            else
            {
                WriterCsvEscape(writer, null, true);
                WriterCsvEscape(writer, null, true);
            }

            WriterCsvEscape(writer, Thread.CurrentThread.ManagedThreadId.ToString(), true);
            WriterCsvEscape(writer, Type.ToString(), true);
            WriterCsvEscape(writer, Method, true);
            WriterCsvEscape(writer, Message, false);
        }

        // http://stackoverflow.com/questions/165042/stop-excel-from-automatically-converting-certain-text-values-to-dates
        private static string CsvEqualQuote(string text) => "=\"" + text + "\"";
        private static void WriterCsvEscape(TextWriter writer, string text, bool addSep) => WriterCsvEscape(writer, text, addSep, false);
        private static void WriterCsvEscape(TextWriter writer, string text, bool addSep, bool surroundWithQuotes)
        {
            if (surroundWithQuotes)
            {
                text = CsvEqualQuote(text);
            }

            if (text != null && text.IndexOfAny(new[] { _csvSep, '\r', '\n' }) >= 0)
            {
                writer.Write('"');
                writer.Write(text.Replace("\"", "\"\""));
                writer.Write('"');
            }
            else
            {
                writer.Write(text);
            }

            if (addSep)
            {
                writer.Write(_csvSep);
            }
        }

        public override int GetHashCode()
        {
            if (Message == null)
                return Type.GetHashCode();

            return Type.GetHashCode() ^ Message.GetHashCode();
        }

        public override bool Equals(object obj) => Equals(obj as AuditRecord);
        public bool Equals(AuditRecord other)
        {
            if (other == null)
                return false;

            return Type == other.Type && Message == other.Message;
        }

        public virtual void IncrementCount()
        {
            Count++;
            LastOccurrenceUtc = DateTime.UtcNow;
        }
    }
}
