using JustLogger.Interfaces;
using JustLogger.Toolkit;
using System;
using System.Collections.Concurrent;

namespace JustLogger
{
    public class CompositeLogger : ILoggerDisposable
    {
        private readonly ILoggerDisposable[] p_loggers;
        private bool p_disposedValue;
        private readonly ConcurrentDictionary<LogEntryType, long> p_stats = new();

        public CompositeLogger(params ILoggerDisposable[] loggers)
        {
            p_loggers = loggers;
        }

        public void Error(string text, string name = null)
        {
            p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
            foreach (ILogger logger in p_loggers)
                logger.Error(text, name);
        }

        public void Error(string text, Exception _ex, string name = null)
        {
            p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
            foreach (ILogger logger in p_loggers)
                logger.Error(text, _ex, name);
        }

        public void Info(string text, string name = null)
        {
            p_stats.AddOrUpdate(LogEntryType.INFO, 1, (_, prevValue) => ++prevValue);
            foreach (ILogger logger in p_loggers)
                logger.Info(text, name);
        }

        public void Warn(string text, string name = null)
        {
            p_stats.AddOrUpdate(LogEntryType.WARN, 1, (_, prevValue) => ++prevValue);
            foreach (ILogger logger in p_loggers)
                logger.Warn(text, name);
        }

        public void NewEvent(LogEntryType type, string text)
        {
            foreach (ILogger logger in p_loggers)
                logger.NewEvent(type, text);
        }

        public long GetEntriesCount(LogEntryType type)
        {
            p_stats.TryGetValue(type, out long value);
            return value;
        }

        public NamedLogger this[string name] => new(this, name);

        public void Flush()
        {
            foreach (ILogger logger in p_loggers)
                logger.Flush();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!p_disposedValue)
            {
                if (disposing)
                    foreach (var logger in p_loggers)
                        logger.Dispose();
                p_disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
