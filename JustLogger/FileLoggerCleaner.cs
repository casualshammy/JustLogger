#nullable enable
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;

namespace JustLogger;

public class FileLoggerCleaner : IDisposable
{
  private readonly Timer p_rotateTimer;
  private readonly DirectoryInfo p_directory;
  private readonly bool p_recursive;
  private readonly Regex p_rotateFileNamePattern;
  private readonly TimeSpan p_rotateInterval;
  private readonly Action<FileInfo>? p_onFileDeleted;
  private bool p_disposedValue;

  private FileLoggerCleaner(
    DirectoryInfo _directory, 
    bool _recursive, 
    Regex _rotateFileNamePattern, 
    TimeSpan _rotateInterval,
    Action<FileInfo>? _onFileDeleted = null)
  {
    p_rotateTimer = new Timer(10 * 60 * 1000);
    p_rotateTimer.Elapsed += RotateTimer_Elapsed;
    p_rotateTimer.Start();
    p_directory = _directory;
    p_recursive = _recursive;
    p_rotateFileNamePattern = _rotateFileNamePattern;
    p_rotateInterval = _rotateInterval;
    p_onFileDeleted = _onFileDeleted;
  }

  /// <summary>
  /// Creates log watcher. Logs in <paramref name="_directory"/> older than <paramref name="_rotateInterval"/> will be purged every 10 minutes
  /// </summary>
  public static FileLoggerCleaner Create(DirectoryInfo _directory, bool _recursive, Regex _logFileNamePattern, TimeSpan _rotateInterval, Action<FileInfo>? _onFileDeleted = null)
      => new(_directory, _recursive, _logFileNamePattern, _rotateInterval, _onFileDeleted);

  private void RotateTimer_Elapsed(object _sender, ElapsedEventArgs _e)
  {
    var now = DateTime.UtcNow;
    var files = p_recursive ?
        p_directory.GetFiles("*.*", SearchOption.AllDirectories) :
        p_directory.GetFiles("*.*", SearchOption.TopDirectoryOnly);

    foreach (var file in files)
      if ((now - file.LastWriteTimeUtc) > p_rotateInterval && p_rotateFileNamePattern.IsMatch(file.Name))
        try
        {
          file.Delete();
          file.Refresh();
          p_onFileDeleted?.Invoke(file);
        }
        catch { }
  }

  protected virtual void Dispose(bool _disposing)
  {
    if (!p_disposedValue)
    {
      if (_disposing)
      {
        p_rotateTimer.Stop();
        p_rotateTimer.Dispose();
      }
      p_disposedValue = true;
    }
  }

  public void Dispose()
  {
    Dispose(_disposing: true);
    GC.SuppressFinalize(this);
  }

}
