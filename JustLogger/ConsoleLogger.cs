#nullable enable
using JustLogger.Interfaces;
using JustLogger.Toolkit;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace JustLogger
{
    public class ConsoleLogger : ILoggerDisposable
    {
        private bool p_disposedValue;
        private readonly ConcurrentDictionary<LogEntryType, long> p_stats = new();
        private readonly static object p_consoleLock = new();

        public ConsoleLogger() { }

        private static void ConsoleWriteColourText(string text, ConsoleColor colour)
        {
            lock (p_consoleLock)
            {
                ConsoleColor oldColour = Console.ForegroundColor;
                Console.ForegroundColor = colour;
                Console.Write(text);
                Console.ForegroundColor = oldColour;
            }
        }

        public void Info(string text, string? name = null)
        {
            if (text is null) throw new ArgumentNullException(paramName: nameof(text));
            p_stats.AddOrUpdate(LogEntryType.INFO, 1, (_, prevValue) => ++prevValue);
            WriteInConsole(new LogEntry(LogEntryType.INFO, text, DateTime.UtcNow, name));
        }

        public void Error(string text, string? name = null)
        {
            if (text is null) throw new ArgumentNullException(paramName: nameof(text));
            p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
            WriteInConsole(new LogEntry(LogEntryType.ERROR, text, DateTime.UtcNow, name));
        }

        public void Error(string text, Exception _ex, string? name = null)
        {
            if (text is null) throw new ArgumentNullException(paramName: nameof(text));
            if (_ex is null)
                throw new ArgumentNullException(paramName: nameof(_ex));

            p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
            WriteInConsole(new LogEntry(LogEntryType.ERROR, $"{text}\n({_ex.GetType()}) {_ex.Message}\n{new StackTrace(_ex, 1, true)}", DateTime.UtcNow, name));
        }

        public void Warn(string text, string? name = null)
        {
            if (text is null) throw new ArgumentNullException(paramName: nameof(text));
            p_stats.AddOrUpdate(LogEntryType.WARN, 1, (_, prevValue) => ++prevValue);
            WriteInConsole(new LogEntry(LogEntryType.WARN, text, DateTime.UtcNow, name));
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
            // do nothing
        }

        private void WriteInConsole(LogEntry _entry)
        {
            string text;
            if (_entry.LogName != null)
                text = $"{_entry.Time:dd.MM.yyyy HH:mm:ss.fff} [{_entry.Type}] [{_entry.LogName}] {_entry.Text}\n";
            else
                text = $"{_entry.Time:dd.MM.yyyy HH:mm:ss.fff} [{_entry.Type}] {_entry.Text}\n";
            if (_entry.Type == LogEntryType.INFO)
                ConsoleWriteColourText(text, ConsoleColor.White);
            else if (_entry.Type == LogEntryType.WARN)
                ConsoleWriteColourText(text, ConsoleColor.Yellow);
            else if (_entry.Type == LogEntryType.ERROR)
                ConsoleWriteColourText(text, ConsoleColor.Red);
        }

    }
}
