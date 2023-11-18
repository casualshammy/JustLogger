using JustLogger.Interfaces;
using JustLogger.Toolkit;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace JustLogger;

public class ConsoleLogger : ILoggerDisposable
{
  private readonly ConcurrentDictionary<LogEntryType, long> p_stats = new();
  private readonly static object p_consoleLock = new();
  private readonly Func<LogEntry, string> p_logEntryFormat;
  private readonly JsonSerializerOptions p_jsonSerializerOptions;
  private bool p_disposedValue;

  public ConsoleLogger(Func<LogEntry, string>? _logEntryFormat = null)
  {
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

    p_jsonSerializerOptions = new JsonSerializerOptions
    {
      WriteIndented = true,
    };
  }

  private static void ConsoleWriteColourText(string _text, ConsoleColor _colour)
  {
    lock (p_consoleLock)
    {
      ConsoleColor oldColour = Console.ForegroundColor;
      Console.ForegroundColor = _colour;
      Console.Write(_text);
      Console.ForegroundColor = oldColour;
    }
  }

  public void Info(string _text, string? _scope = null)
  {
    if (_text is null)
      throw new ArgumentNullException(paramName: nameof(_text));

    p_stats.AddOrUpdate(LogEntryType.INFO, 1, (_, prevValue) => ++prevValue);
    WriteInConsole(new LogEntry(LogEntryType.INFO, _text, DateTime.UtcNow, _scope));
  }

  public void InfoJson<T>(string _text, T _object, string? _scope = null) where T : notnull
  {
    p_stats.AddOrUpdate(LogEntryType.INFO, 1, (_, _prevValue) => ++_prevValue);

    var entry = new LogEntry(
      LogEntryType.INFO,
      $"{_text}{Environment.NewLine}{JsonSerializer.Serialize(_object, p_jsonSerializerOptions)}",
      DateTimeOffset.UtcNow,
      _scope);

    WriteInConsole(entry);
  }

  public void Warn(string _text, string? _scope = null)
  {
    if (_text is null)
      throw new ArgumentNullException(paramName: nameof(_text));

    p_stats.AddOrUpdate(LogEntryType.WARN, 1, (_, _prevValue) => ++_prevValue);
    WriteInConsole(new LogEntry(LogEntryType.WARN, _text, DateTime.UtcNow, _scope));
  }

  public void WarnJson<T>(string _text, T _object, string? _scope = null) where T : notnull
  {
    p_stats.AddOrUpdate(LogEntryType.WARN, 1, (_, _prevValue) => ++_prevValue);

    var entry = new LogEntry(
      LogEntryType.WARN,
      $"{_text}{Environment.NewLine}{JsonSerializer.Serialize(_object, p_jsonSerializerOptions)}",
      DateTimeOffset.UtcNow,
      _scope);

    WriteInConsole(entry);
  }

  public void Error(string _text, string? _scope = null)
  {
    if (_text is null)
      throw new ArgumentNullException(paramName: nameof(_text));

    p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
    WriteInConsole(new LogEntry(LogEntryType.ERROR, _text, DateTime.UtcNow, _scope));
  }

  public void Error(string _text, Exception _ex, string? _scope = null)
  {
    if (_text is null)
      throw new ArgumentNullException(paramName: nameof(_text));
    if (_ex is null)
      throw new ArgumentNullException(paramName: nameof(_ex));

    p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
    WriteInConsole(new LogEntry(LogEntryType.ERROR, $"{_text}\n({_ex.GetType()}) {_ex.Message}\n{new StackTrace(_ex, 1, true)}", DateTime.UtcNow, _scope));
  }

  public void ErrorJson<T>(string _text, T _object, string? _scope = null) where T : notnull
  {
    p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, _prevValue) => ++_prevValue);

    var entry = new LogEntry(
      LogEntryType.ERROR,
      $"{_text}{Environment.NewLine}{JsonSerializer.Serialize(_object, p_jsonSerializerOptions)}",
      DateTimeOffset.UtcNow,
      _scope);

    WriteInConsole(entry);
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

  public ILogger this[string _scope] => new NamedLogger(this, _scope);

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
    string text = p_logEntryFormat(_entry) + "\n";

    if (_entry.Type == LogEntryType.INFO)
      ConsoleWriteColourText(text, ConsoleColor.White);
    else if (_entry.Type == LogEntryType.WARN)
      ConsoleWriteColourText(text, ConsoleColor.Yellow);
    else if (_entry.Type == LogEntryType.ERROR)
      ConsoleWriteColourText(text, ConsoleColor.Red);
  }

}
