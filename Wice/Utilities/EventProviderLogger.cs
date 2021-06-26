using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using DirectN;

namespace Wice.Utilities
{
    public class EventProviderLogger : ILogger, IDisposable
    {
        private bool _disposedValue = false;
        private readonly EventProvider _provider;

        public EventProviderLogger(Guid providerId)
        {
            if (providerId == Guid.Empty)
                throw new ArgumentException(null, nameof(providerId));

            _provider = new EventProvider(providerId);
            Log(TraceLevel.Info, "Hello from logger id '" + providerId + "' type '" + GetType().FullName + "'.");
        }

        public void Log(TraceLevel level, string message = null, [CallerMemberName] string methodName = null)
        {
            if (!string.IsNullOrEmpty(methodName))
            {
                methodName += "|";
            }

            var name = Thread.CurrentThread.Name.Nullify() ?? Thread.CurrentThread.ManagedThreadId.ToString();
            _provider.WriteMessageEvent(name + "|" + methodName + message);
            AuditRecordType type;
            switch (level)
            {
                case TraceLevel.Error:
                    type = AuditRecordType.Error;
                    break;

                case TraceLevel.Info:
                    type = AuditRecordType.Info;
                    break;

                case TraceLevel.Verbose:
                    type = AuditRecordType.Verbose;
                    break;

                case TraceLevel.Warning:
                    type = AuditRecordType.Warning;
                    break;

                default:
                    type = AuditRecordType.Unspecified;
                    break;
            }

            if (level == TraceLevel.Error || level == TraceLevel.Warning)
            {
                Audit.AddRecord(type, message, methodName);
                Audit.FlushRecords();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    _provider?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        ~EventProviderLogger() { Dispose(false); }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
    }
}
