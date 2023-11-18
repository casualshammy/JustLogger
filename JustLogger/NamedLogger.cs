using JustLogger.Interfaces;
using JustLogger.Toolkit;
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

  public void Info(string _text, string? _overrideName = null) => p_logger.Info(_text, _overrideName ?? p_name);

  public void InfoJson<T>(string _text, T _object, string? _name = null) where T : notnull => p_logger.InfoJson(_text, _object, _name ?? p_name);

  public void Warn(string _text, string? _overrideName = null) => p_logger.Warn(_text, _overrideName ?? p_name);

  public void WarnJson<T>(string _text, T _object, string? _name = null) where T : notnull => p_logger.WarnJson(_text, _object, _name ?? p_name);

  public void Error(string _text, string? _overrideName = null) => p_logger.Error(_text, _overrideName ?? p_name);

  public void Error(string _text, Exception _ex, string? _overrideName = null) => p_logger.Error(_text, _ex, _overrideName ?? p_name);

  public void ErrorJson<T>(string _text, T _object, string? _name = null) where T : notnull => p_logger.ErrorJson(_text, _object, _name ?? p_name);

  public long GetEntriesCount(LogEntryType _type)
  {
    return p_logger.GetEntriesCount(_type);
  }

  public ILogger this[string _scope] => new NamedLogger(this, _scope);

  public void Flush() => p_logger.Flush();

}
