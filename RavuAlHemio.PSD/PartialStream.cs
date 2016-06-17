using System;
using System.IO;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// A stream which wraps a finite section of another stream.
    /// </summary>
    public class PartialStream : Stream
    {
        /// <summary>
        /// The stream underlying this stream.
        /// </summary>
        public Stream UnderlyingStream { get; }

        /// <summary>
        /// The position of the underlying stream that is the initial position of this partial stream.
        /// </summary>
        public long PartialStart { get; }

        /// <summary>
        /// The length of this partial stream.
        /// </summary>
        public long PartialLength { get; }

        /// <summary>
        /// Initializes a partial stream with an underlying stream, a starting position and a length.
        /// </summary>
        /// <param name="underlying">The underlying stream.</param>
        /// <param name="start">The starting position relative to the start of the underlying stream.</param>
        /// <param name="length">The length of this stream.</param>
        public PartialStream(Stream underlying, long start, long length)
        {
            UnderlyingStream = underlying;
            PartialStart = start;
            PartialLength = length;
        }

        public override void Flush()
        {
            UnderlyingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long remaining = Length - (UnderlyingStream.Position - PartialStart);
            int actuallyRead = (int) Math.Min(remaining, count);
            if (actuallyRead < 0)
            {
                return 0;
            }

            return UnderlyingStream.Read(buffer, offset, actuallyRead);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newLocation = -1;
            if (origin == SeekOrigin.End)
            {
                long actualLocation = PartialStart + PartialLength + offset;
                newLocation = UnderlyingStream.Seek(actualLocation, SeekOrigin.Begin);
            }
            else if (origin == SeekOrigin.Begin)
            {
                long actualLocation = PartialStart + offset;
                newLocation = UnderlyingStream.Seek(actualLocation, SeekOrigin.Begin);
            }
            else if (origin == SeekOrigin.Current)
            {
                newLocation = UnderlyingStream.Seek(offset, SeekOrigin.Current);
            }
            return newLocation - PartialStart;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long remaining = Length - (UnderlyingStream.Position - PartialStart);
            int actuallyWrite = (int)Math.Min(remaining, count);
            if (actuallyWrite < 0)
            {
                return;
            }

            UnderlyingStream.Write(buffer, offset, actuallyWrite);
        }

        public override bool CanRead => UnderlyingStream.CanRead;
        public override bool CanSeek => UnderlyingStream.CanSeek;
        public override bool CanWrite => UnderlyingStream.CanWrite;
        public override long Length => PartialLength;

        public override long Position
        {
            get { return UnderlyingStream.Position - PartialStart; }
            set { Seek(value, SeekOrigin.Begin); }
        }
    }
}
