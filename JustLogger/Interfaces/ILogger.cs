using JustLogger.Toolkit;
using Newtonsoft.Json.Linq;
using System;

namespace JustLogger.Interfaces;

public interface ILogger
{
  void Info(string _text, string? _name = null);

  void InfoJson(string _text, JToken _object, string? _name = null);

  void Error(string _text, string? name = null);

  void Error(string _text, Exception _ex, string? _name = null);

  void Warn(string _text, string? _name = null);

  void NewEvent(LogEntryType type, string _text);

  long GetEntriesCount(LogEntryType type);

  NamedLogger this[string name] { get; }

  void Flush();

}
