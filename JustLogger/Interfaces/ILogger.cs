using JustLogger.Toolkit;
using System;

namespace JustLogger.Interfaces;

public interface ILogger
{
  void Info(string _text, string? _scope = null);

  void InfoJson<T>(string _text, T _object, string? _scope = null) where T : notnull;

  void Warn(string _text, string? _scope = null);

  void WarnJson<T>(string _text, T _object, string? _scope = null) where T : notnull;

  void Error(string _text, string? _scope = null);

  void Error(string _text, Exception _ex, string? _scope = null);

  void ErrorJson<T>(string _text, T _object, string? _scope = null) where T : notnull;

  long GetEntriesCount(LogEntryType _type);

  ILogger this[string _scope] { get; }

  void Flush();

}
