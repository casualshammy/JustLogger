using JustLogger.Interfaces;
using JustLogger.Toolkit;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;

namespace JustLogger;

public class CompositeLogger : ILoggerDisposable
{
  private readonly ILogger[] p_loggers;
  private bool p_disposedValue;
  private readonly ConcurrentDictionary<LogEntryType, long> p_stats = new();

  public CompositeLogger(params ILogger[] _loggers)
  {
    p_loggers = _loggers;
  }

  public void Error(string _text, string? _name = null)
  {
    p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
    foreach (ILogger logger in p_loggers)
      logger.Error(_text, _name);
  }

  public void Error(string _text, Exception _ex, string? _name = null)
  {
    p_stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
    foreach (ILogger logger in p_loggers)
      logger.Error(_text, _ex, _name);
  }

  public void Info(string _text, string? _name = null)
  {
    p_stats.AddOrUpdate(LogEntryType.INFO, 1, (_, _prevValue) => ++_prevValue);
    foreach (ILogger logger in p_loggers)
      logger.Info(_text, _name);
  }

  public void InfoJson(string _text, JToken _object, string? _name = null)
  {
    p_stats.AddOrUpdate(LogEntryType.INFO, 1, (_, _prevValue) => ++_prevValue);
    foreach (var logger in p_loggers)
      logger.InfoJson(_text, _object, _name);
  }

  public void Warn(string _text, string? _name = null)
  {
    p_stats.AddOrUpdate(LogEntryType.WARN, 1, (_, _prevValue) => ++_prevValue);
    foreach (ILogger logger in p_loggers)
      logger.Warn(_text, _name);
  }

  public void NewEvent(LogEntryType _type, string _text)
  {
    foreach (ILogger logger in p_loggers)
      logger.NewEvent(_type, _text);
  }

  public long GetEntriesCount(LogEntryType _type)
  {
    p_stats.TryGetValue(_type, out long value);
    return value;
  }

  public NamedLogger this[string _name] => new(this, _name);

  public void Flush()
  {
    foreach (ILogger logger in p_loggers)
      logger.Flush();
  }

  protected virtual void Dispose(bool _disposing)
  {
    if (!p_disposedValue)
    {
      if (_disposing)
        foreach (var logger in p_loggers)
          if (logger is IDisposable disposable)
            disposable.Dispose();

      p_disposedValue = true;
    }
  }

  public void Dispose()
  {
    Dispose(_disposing: true);
    GC.SuppressFinalize(this);
  }

}
