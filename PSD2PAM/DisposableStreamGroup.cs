using System;
using System.Collections.Generic;
using System.IO;

namespace PSD2PAM
{
    public sealed class DisposableStreamGroup : IDisposable
    {
        public List<Stream> Streams { get; }

        public DisposableStreamGroup()
        {
            Streams = new List<Stream>();
        }
        
        public void Dispose()
        {
            foreach (Stream stream in Streams)
            {
                stream.Dispose();
            }
        }
    }
}
