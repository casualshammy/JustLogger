#nullable enable
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;

namespace JustLogger
{
    public class FileLoggerCleaner : IDisposable
    {
        private readonly Timer p_rotateTimer;
        private readonly DirectoryInfo p_directory;
        private readonly bool p_recursive;
        private readonly Regex p_rotateFileNamePattern;
        private readonly TimeSpan p_rotateInterval;
        private bool p_disposedValue;

        public FileLoggerCleaner(DirectoryInfo _directory, bool _recursive, Regex _rotateFileNamePattern, TimeSpan _rotateInterval)
        {
            p_rotateTimer = new Timer(10 * 60 * 1000);
            p_rotateTimer.Elapsed += RotateTimer_Elapsed;
            p_rotateTimer.Start();
            p_directory = _directory;
            p_recursive = _recursive;
            p_rotateFileNamePattern = _rotateFileNamePattern;
            p_rotateInterval = _rotateInterval;
        }

        private void RotateTimer_Elapsed(object sender, ElapsedEventArgs e)
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
                    }
                    catch { }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!p_disposedValue)
            {
                if (disposing)
                {
                    p_rotateTimer.Stop();
                    p_rotateTimer.Dispose();
                }
                p_disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
