using JustLogger.Interfaces;

namespace JustLogger;

public static class Extensions
{
  public static NamedLogger GetNamedLog(this ILogger _logger, string _name) => new(_logger, _name);
}
