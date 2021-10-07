#nullable enable
using JustLogger.Toolkit;
using System;

namespace JustLogger.Interfaces
{
    public interface ILogger
    {
        void Info(string text, string? name = null);

        void Error(string text, string? name = null);

        void Error(string text, Exception _ex, string? name = null);

        void Warn(string text, string? name = null);

        void NewEvent(LogEntryType type, string text);

        long GetEntriesCount(LogEntryType type);

        NamedLogger this[string name] { get; }

        void Flush();

    }

}
