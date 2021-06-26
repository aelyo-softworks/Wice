using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Wice.Utilities
{
    public static class Audit
    {
        private static readonly ConcurrentDictionary<string, DateTime> _monitors = new ConcurrentDictionary<string, DateTime>();

        public const string AuditingFileFormat = "{1}_{0:yyyy}_{0:MM}_{0:dd}.log";
        public static int AuditingFlushPeriodInMilliseconds = 1000 * 60; // 1 min
        public const string FileNameToken = "AUDIT_";
        public const string FileNamePrefix = "FCO" + FileNameToken;

        private static Timer _monitoringTimer;
        private static int _inTimerCallback;
        private static readonly List<AuditRecord> _queue = new List<AuditRecord>();
        private static object _syncObject;
        private static object SyncObject
        {
            get
            {
                if (_syncObject == null)
                {
                    var obj = new object();
                    Interlocked.CompareExchange(ref _syncObject, obj, null);
                }
                return _syncObject;
            }
        }

        public static string DirectoryPath { get; private set; }
        public static KeyValuePair<string, DateTime>[] GetMonitors() => _monitors.ToArray();
        public static int Count
        {
            get
            {
                lock (SyncObject)
                {
                    return _queue.Count;
                }
            }
        }

        public static void UpdateMonitor(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            _monitors[name] = DateTime.Now;
        }

        public static AuditRecord AddRecord(AuditRecordType type, Exception error, [CallerMemberName] string methodName = null)
        {
            if (error == null)
                return AddRecord(type, (string)null);

            return AddRecord(type, error.ToString(), methodName);
        }

        public static AuditRecord AddRecord(AuditRecordType type, string message, [CallerMemberName] string methodName = null)
        {
            var record = new AuditRecord(type, message, methodName);
            return AddRecord(record);
        }

        public static AuditRecord AddRecord(AuditRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            if (record.Type == AuditRecordType.AuditStart || record.Type == AuditRecordType.AuditStop)
                throw new ArgumentException(null, nameof(record));

            return NoCheckAddRecord(record);
        }

        private static AuditRecord NoCheckAddRecord(AuditRecord record)
        {
            lock (SyncObject)
            {
                var last = _queue.Count > 0 ? _queue[_queue.Count - 1] : null;
                if (record.Equals(last))
                    return last;

                _queue.Add(record);
                return record;
            }
        }

        public static int FlushRecords() => MonitoringTimerCallback();
        public static void StopAuditing()
        {
            var sw = new Stopwatch();
            sw.Start();

            var record = new AuditRecord(AuditRecordType.AuditStop, "Auditing is stopping");
            NoCheckAddRecord(record);
            MonitoringTimerCallback(); // force flush

            var timer = _monitoringTimer;
            if (timer != null && Interlocked.CompareExchange(ref _monitoringTimer, null, timer) == timer)
            {
                timer.Dispose();
            }

            while (_inTimerCallback != 0)
            {
                Thread.Sleep(100);
            }
        }

        public static void StartAuditing(string directoryPath)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            DirectoryPath = directoryPath;
            AppDomain.CurrentDomain.DomainUnload += OnCurrentDomainUnload;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            var sw = new Stopwatch();
            sw.Start();
            if (_monitoringTimer != null)
            {
                StopAuditing();
            }

            var audit = "Auditing is starting.";
            var record = new AuditRecord(AuditRecordType.AuditStart, audit);
            NoCheckAddRecord(record);
            _monitoringTimer = new Timer(MonitoringTimerCallback, null, 0, AuditingFlushPeriodInMilliseconds);
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception error)
            {
                AddRecord(AuditRecordType.Error, error);
            }
            else if (e.ExceptionObject != null)
            {
                AddRecord(AuditRecordType.Error, string.Format(CultureInfo.InvariantCulture, "{0}", e.ExceptionObject));
            }
        }

        private static void OnCurrentDomainUnload(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.DomainUnload -= OnCurrentDomainUnload;
            AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;
            StopAuditing();
        }

        private static void MonitoringTimerCallback(object state)
        {
            if (Interlocked.Exchange(ref _inTimerCallback, 1) != 0)
                return;

            try
            {
                MonitoringTimerCallback();
                UpdateMonitor("Audit");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Interlocked.Exchange(ref _inTimerCallback, 0);
            }
        }

        private static int MonitoringTimerCallback()
        {
            AuditRecord[] records;
            lock (SyncObject)
            {
                records = _queue.ToArray();
                _queue.Clear();
            }

            if (records.Length > 0)
            {
                var name = FileNamePrefix + string.Format(AuditingFileFormat, DateTime.Now, Environment.MachineName);
                var path = System.IO.Path.Combine(DirectoryPath, name);
                IOUtilities.FileEnsureDirectory(path);
                var i = 0;
                try
                {
                    using (var writer = new StreamWriter(path, true, Encoding.Unicode))
                    {
                        var writerHeader = writer.BaseStream.Position == 0;
                        foreach (var record in records)
                        {
                            if (i == 0 && writerHeader)
                            {
                                record.WriteHeader(writer);
                                writer.WriteLine();
                                i++;
                            }

                            try
                            {
                                record.Write(writer);
                                writer.WriteLine();
                            }
                            catch
                            {
                                // go on
                            }
                        }
                        writer.Flush();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return records.Length;
        }
    }
}
