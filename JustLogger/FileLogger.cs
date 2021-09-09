using JustLogger.Interfaces;
using JustLogger.Toolkit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace JustLogger
{
    public class FileLogger : ILoggerDisposable
    {
        private readonly ConcurrentQueue<LogEntry> p_buffer = new();
        private readonly Func<string> p_filename;
        private bool p_disposedValue;
        private readonly ConcurrentDictionary<LogEntryType, long> p_stats = new();
        private readonly Timer p_timer;
        private readonly Action<Exception, IEnumerable<string>> p_onErrorHandler;
        private readonly HashSet<string> p_filesWrote = new();

        public FileLogger(string filename, uint bufferLengthMs, Action<Exception, IEnumerable<string>> onError)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException($"'{nameof(filename)}' cannot be null or whitespace.", nameof(filename));

            p_onErrorHandler = onError;
            p_filename = () => filename;
            p_timer = new Timer(bufferLengthMs);
            p_timer.Elapsed += Timer_Elapsed;
            p_timer.Start();
        }

        public FileLogger(Func<string> filenameFactory, uint bufferLengthMs, Action<Exception, IEnumerable<string>> onError)
        {
            p_onErrorHandler = onError;
            p_filename = filenameFactory ?? throw new ArgumentException($"'{nameof(filenameFactory)}' cannot be null.", nameof(filenameFactory));
            p_timer = new Timer(bufferLengthMs);
            p_timer.Elapsed += Timer_Elapsed;
            p_timer.Start();
        }

        public IReadOnlyCollection<string> LogFilesWrote => p_filesWrote;

        public double BufferLengthMs
        {
            get => p_timer.Interval;
            set => p_timer.Interval = value;
        }

        public void Info(string text, string name = null)
        {
            if (text is null)
                throw new ArgumentNullException(paramName: nameof(text));

            p_stats.AddOrUpdate(LogEntryType.INFO, 1, (_, prevValue) => ++prevValue);

            p_buffer.Enqueue(new LogEntry(LogEntryType.INFO, text, DateTime.UtcNow, name));
        }

        public void Error(string text, string name = null)
        {
            if (text is null)
                throw new ArgumentNullException(paramName: nameof(text));

            p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);

            p_buffer.Enqueue(new LogEntry(LogEntryType.ERROR, text, DateTime.UtcNow, name));
        }

        public void Error(string text, Exception _ex, string name = null)
        {
            if (text is null)
                throw new ArgumentNullException(paramName: nameof(text));
            if (_ex is null)
                throw new ArgumentNullException(paramName: nameof(_ex));

            p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);

            p_buffer.Enqueue(new LogEntry(LogEntryType.ERROR, $"{text}\n({_ex.GetType()}) {_ex.Message}\n{new StackTrace(_ex, 1, true)}", DateTime.UtcNow, name));
        }

        public void Warn(string text, string name = null)
        {
            if (text is null)
                throw new ArgumentNullException(paramName: nameof(text));

            p_stats.AddOrUpdate(LogEntryType.WARN, 1, (_, prevValue) => ++prevValue);

            p_buffer.Enqueue(new LogEntry(LogEntryType.WARN, text, DateTime.UtcNow, name));
        }

        public void NewEvent(LogEntryType type, string text)
        {
            if (type == LogEntryType.INFO)
                Info(text);
            else if (type == LogEntryType.WARN)
                Warn(text);
            else if (type == LogEntryType.ERROR)
                Error(text);
        }

        public long GetEntriesCount(LogEntryType type)
        {
            p_stats.TryGetValue(type, out long value);
            return value;
        }

        public NamedLogger this[string name] => new(this, name);

        protected virtual void Dispose(bool disposing)
        {
            if (!p_disposedValue)
            {
                if (disposing)
                {
                    p_timer.Dispose();
                    Flush();
                }
                p_disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Flush()
        {
            try
            {
                var stringBuilder = new StringBuilder();
                while (p_buffer.TryDequeue(out var logEntry))
                {
                    if (logEntry.LogName != null)
                        stringBuilder.AppendLine($"{logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} [{logEntry.Type}] [{logEntry.LogName}] {logEntry.Text}");
                    else
                        stringBuilder.AppendLine($"{logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} [{logEntry.Type}] {logEntry.Text}");
                }

                if (stringBuilder.Length > 0)
                {
                    var filepath = p_filename();
                    p_filesWrote.Add(filepath);
                    File.AppendAllText(filepath, stringBuilder.ToString(), Encoding.UTF8);
                }

                stringBuilder.Clear();
            }
            catch (Exception ex)
            {
                p_onErrorHandler?.Invoke(ex, p_buffer.Select(l => l.Text));
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Flush();
        }

    }
}
