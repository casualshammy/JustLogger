#nullable enable
using System;

namespace JustLogger.Interfaces
{
    public interface ILoggerDisposable : ILogger, IDisposable { }

}
