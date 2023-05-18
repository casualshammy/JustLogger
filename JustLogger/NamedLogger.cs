using JustLogger.Interfaces;
using JustLogger.Toolkit;
using Newtonsoft.Json.Linq;
using System;

namespace JustLogger;

public class NamedLogger : ILogger
{
  private readonly ILogger p_logger;
  private readonly string p_name;

  public NamedLogger(ILogger _logger, string _name)
  {
    if (string.IsNullOrEmpty(_name)) throw new ArgumentException($"'{nameof(_name)}' cannot be null or empty.", nameof(_name));
    p_logger = _logger ?? throw new ArgumentNullException(paramName: nameof(_logger));

    if (_logger is not NamedLogger namedLogger)
      p_name = _name;
    else
      p_name = $"{namedLogger.p_name} | {_name}";
  }

  public void Info(string _text, string? _overrideName = null)
  {
    p_logger.Info(_text, _overrideName ?? p_name);
  }

  public void InfoJson(string _text, JToken _object, string? _name = null)
  {
    p_logger.InfoJson(_text, _object, _name ?? p_name);
  }

  public void Warn(string _text, string? _overrideName = null)
  {
    p_logger.Warn(_text, _overrideName ?? p_name);
  }

  public void Error(string _text, string? _overrideName = null)
  {
    p_logger.Error(_text, _overrideName ?? p_name);
  }

  public void Error(string _text, Exception _ex, string? _overrideName = null)
  {
    p_logger.Error(_text, _ex, _overrideName ?? p_name);
  }

  public void NewEvent(LogEntryType _type, string _text)
  {
    if (_type == LogEntryType.INFO)
      Info(_text);
    else if (_type == LogEntryType.WARN)
      Warn(_text);
    else if (_type == LogEntryType.ERROR)
      Error(_text);
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
