#nullable enable
using JustLogger.Interfaces;

namespace JustLogger
{
    public static class Extensions
    {
        public static NamedLogger GetNamedLog(this ILogger logger, string name)
        {
            return new(logger, name);
        }
    }
}
