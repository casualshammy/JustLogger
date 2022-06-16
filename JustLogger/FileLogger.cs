#nullable enable
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
        private readonly Func<string?> p_filename;
        private bool p_disposedValue;
        private readonly ConcurrentDictionary<LogEntryType, long> p_stats = new();
        private readonly Timer p_timer;
        private readonly Action<Exception, IEnumerable<string>>? p_onErrorHandler;
        private readonly Func<LogEntry, string> p_logEntryFormat;
        private readonly HashSet<string> p_filesWrote = new();

        public FileLogger(Func<string?> _filenameFactory, uint _bufferLengthMs, Func<LogEntry, string>? _logEntryFormat = null, Action<Exception, IEnumerable<string>>? _onError = null)
        {
            p_onErrorHandler = _onError;
            p_filename = _filenameFactory ?? throw new ArgumentException($"'{nameof(_filenameFactory)}' cannot be null.", nameof(_filenameFactory));
            if (_logEntryFormat == null)
                p_logEntryFormat = _logEntry =>
                {
                    if (_logEntry.LogName != null)
                        return $"| {_logEntry.GetTypePrefix()} | {_logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} [{_logEntry.LogName}] {_logEntry.Text}";
                    else
                        return $"| {_logEntry.GetTypePrefix()} | {_logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} {_logEntry.Text}";
                };
            else
                p_logEntryFormat = _logEntryFormat;

            p_timer = new Timer(_bufferLengthMs);
            p_timer.Elapsed += Timer_Elapsed;
            p_timer.Start();
        }

        public IReadOnlyCollection<string> LogFilesWrote => p_filesWrote;

        public double BufferLengthMs
        {
            get => p_timer.Interval;
            set => p_timer.Interval = value;
        }

        public FileInfo? CurrentLogFile
        {
            get
            {
                var filePath = p_filename();
                if (filePath == null)
                    return null;

                return new(filePath);
            }
        }

        public void Info(string text, string? name = null)
        {
            if (text is null)
                throw new ArgumentNullException(paramName: nameof(text));

            p_stats.AddOrUpdate(LogEntryType.INFO, 1, (_, prevValue) => ++prevValue);

            p_buffer.Enqueue(new LogEntry(LogEntryType.INFO, text, DateTime.UtcNow, name));
        }

        public void Error(string text, string? name = null)
        {
            if (text is null)
                throw new ArgumentNullException(paramName: nameof(text));

            p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);

            p_buffer.Enqueue(new LogEntry(LogEntryType.ERROR, text, DateTime.UtcNow, name));
        }

        public void Error(string text, Exception _ex, string? name = null)
        {
            if (text is null)
                throw new ArgumentNullException(paramName: nameof(text));
            if (_ex is null)
                throw new ArgumentNullException(paramName: nameof(_ex));

            p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);

            p_buffer.Enqueue(new LogEntry(LogEntryType.ERROR, $"{text}\n({_ex.GetType()}) {_ex.Message}\n{new StackTrace(_ex, 1, true)}", DateTime.UtcNow, name));
        }

        public void Warn(string text, string? name = null)
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
                if (p_buffer.Any())
                {
                    var filepath = p_filename();
                    if (filepath == null)
                        return;

                    var stringBuilder = new StringBuilder();

                    while (p_buffer.TryDequeue(out var logEntry))
                        stringBuilder.AppendLine(p_logEntryFormat(logEntry));

                    p_filesWrote.Add(filepath);
                    File.AppendAllText(filepath, stringBuilder.ToString(), Encoding.UTF8);

                    stringBuilder.Clear();
                }
            }
            catch (Exception ex)
            {
                p_onErrorHandler?.Invoke(ex, p_buffer.Select(l => l.Text));
            }
        }

        private void Timer_Elapsed(object _, ElapsedEventArgs __) => Flush();

    }
}
