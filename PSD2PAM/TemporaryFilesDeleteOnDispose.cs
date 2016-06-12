using System;
using System.Collections.Generic;
using System.IO;

namespace PSD2PAM
{
    public sealed class TemporaryFilesDeleteOnDispose : IDisposable
    {
        public IReadOnlyList<string> FileNames { get; }
        private bool _disposed = false;

        public TemporaryFilesDeleteOnDispose(int count)
        {
            var fileNames = new List<string>(count);
            for (int i = 0; i < count; ++i)
            {
                fileNames.Add(Path.GetTempFileName());
            }
            FileNames = fileNames;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (var file in FileNames)
            {
                File.Delete(file);
            }

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        ~TemporaryFilesDeleteOnDispose()
        {
            Dispose(false);
        }
    }
}
