#nullable enable
using System;

namespace JustLogger.Toolkit;

public class LogEntry
{
    public LogEntryType Type;
    public string Text;
    public DateTimeOffset Time;
    public string? LogName;

    public LogEntry(LogEntryType type, string text, DateTimeOffset time, string? logName)
    {
        Type = type;
        Text = text;
        Time = time;
        LogName = logName;
    }

    public char GetTypePrefix() => Type switch
    {
        LogEntryType.WARN => 'W',
        LogEntryType.ERROR => 'E',
        _ => ' ',
    };

}
