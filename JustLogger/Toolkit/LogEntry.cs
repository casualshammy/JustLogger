#nullable enable
using System;

namespace JustLogger.Toolkit
{
    internal class LogEntry
    {
        public LogEntryType Type;
        public string Text;
        public DateTime Time;
        public string? LogName;

        public LogEntry(LogEntryType type, string text, DateTime time, string? logName)
        {
            Type = type;
            Text = text;
            Time = time;
            LogName = logName;
        }
    }
}
