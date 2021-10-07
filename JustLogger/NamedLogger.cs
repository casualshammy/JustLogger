#nullable enable
using JustLogger.Interfaces;
using JustLogger.Toolkit;
using System;

namespace JustLogger
{
    public class NamedLogger : ILogger
    {
        private readonly ILogger p_logger;
        private readonly string p_name;

        public NamedLogger(ILogger logger, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            p_logger = logger ?? throw new ArgumentNullException(paramName: nameof(logger));
            p_name = name;
        }

        public void Info(string text, string? overrideName = null)
        {
            p_logger.Info(text, overrideName ?? p_name);
        }

        public void Warn(string text, string? overrideName = null)
        {
            p_logger.Warn(text, overrideName ?? p_name);
        }

        public void Error(string text, string? overrideName = null)
        {
            p_logger.Error(text, overrideName ?? p_name);
        }

        public void Error(string text, Exception _ex, string? overrideName = null)
        {
            p_logger.Error(text, _ex, overrideName ?? p_name);
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
            return p_logger.GetEntriesCount(type);
        }

        public NamedLogger this[string name] => new(this, name);

        public void Flush()
        {
            p_logger.Flush();
        }
    }
}
