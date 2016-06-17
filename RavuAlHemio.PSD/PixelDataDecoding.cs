using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace RavuAlHemio.PSD
{
    public static class PixelDataDecoding
    {
        public const int BufferSize = 8*1024*1024;

        /// <summary>
        /// Copies raw data from a source to a target stream until a specific number of bytes has been copied or the
        /// source stream ends.
        /// </summary>
        /// <param name="source">The stream from which to copy the data.</param>
        /// <param name="dest">The stream to which to copy the data.</param>
        /// <param name="length">
        /// The number of bytes to copy from the source stream, or <c>null</c> to copy all data until the source stream
        /// ends.
        /// </param>
        /// <param name="cancelToken">
        /// A cancellation token which allows copying to be cancelled before it completes.
        /// </param>
        public static void DecodeRawData(Stream source, Stream dest, long? length,
            CancellationToken cancelToken = default(CancellationToken))
        {
            var buf = new byte[BufferSize];
            long totalCopied = 0;

            while (!length.HasValue || totalCopied < length.Value)
            {
                int maxToCopy = length.HasValue
                    ? (int)Math.Min(length.Value - totalCopied, buf.Length)
                    : buf.Length;

                int read = source.Read(buf, 0, maxToCopy);
                if (read == 0)
                {
                    if (length.HasValue)
                    {
                        throw new EndOfStreamException();
                    }
                    else
                    {
                        return;
                    }
                }
                dest.Write(buf, 0, read);
                totalCopied += read;
                
                cancelToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Decodes rows of PackBits-encoded data from a source stream and writes the decoded data to a target stream.
        /// </summary>
        /// <remarks>
        /// The expected data consists of a <paramref name="scanlineCount"/> 16-bit or 32-bit (depending on
        /// <paramref name="fourByteLengths"/>) big-endian integers specifying the encoded lengths of each
        /// PackBits-encoded row of data, then <paramref name="scanlineCount"/> PackBits-encoded rows themselves.
        /// <paramref name="source"/> must already be at the correct position.
        /// </remarks>
        /// <param name="source">The stream from which to copy the data.</param>
        /// <param name="dest">The stream to which to copy the data.</param>
        /// <param name="scanlineCount">The number of rows to decode.</param>
        /// <param name="fourByteLengths">
        /// Whether the row lengths are encoded as 32-bit instead of 16-bit integers.
        /// </param>
        /// <param name="cancelToken">
        /// A cancellation token which allows copying to be cancelled before it completes.
        /// </param>
        public static void DecodePackBits(Stream source, Stream dest, int scanlineCount,
            bool fourByteLengths, CancellationToken cancelToken = default(CancellationToken))
        {
            var scanlineDataLengths = new int[scanlineCount];
            for (int i = 0; i < scanlineCount; ++i)
            {
                scanlineDataLengths[i] = fourByteLengths
                    ? source.ReadBigEndianInt32()
                    : source.ReadBigEndianInt16();
            }
            
            var inMule = new byte[scanlineDataLengths.Max()];
            var outMule = new List<byte>();
            for (int i = 0; i < scanlineCount; ++i)
            {
                if (i%64 == 0)
                {
                    cancelToken.ThrowIfCancellationRequested();
                }

                int thisScanlineLength = scanlineDataLengths[i];
                source.ReadBytes(inMule, 0, thisScanlineLength);
                outMule.AddRange(BitPacking.UnpackBits(inMule.Take(thisScanlineLength)));

                var buf = outMule.ToArray();
                dest.Write(buf, 0, buf.Length);
                outMule.Clear();
            }
        }

        /// <summary>
        /// Obtains the precomposed image data of a PSD image by decoding rows of PackBits-encoded data from a source
        /// stream and writing the decoded data to a target stream.
        /// </summary>
        /// <remarks><paramref name="source"/> must already be at the correct position.</remarks>
        /// <param name="source">The stream from which to copy the data.</param>
        /// <param name="dest">The stream to which to copy the data.</param>
        /// <param name="psd">The PSD file from which to obtain the number of PackBits-encoded rows.</param>
        /// <param name="cancelToken">
        /// A cancellation token which allows copying to be cancelled before it completes.
        /// </param>
        public static void DecodePackBits(Stream source, Stream dest, PSDFile psd,
            CancellationToken cancelToken = default(CancellationToken))
        {
            int scanlineCount = psd.Height * psd.NumberOfChannels;
            DecodePackBits(source, dest, scanlineCount, psd.Version == 2, cancelToken);
        }

        /// <summary>
        /// Decodes Deflate-compressed data from a source stream and writes the decoded data to a target stream.
        /// </summary>
        /// <remarks><paramref name="source"/> must already be at the correct position.</remarks>
        /// <param name="source">The stream from which to copy the data.</param>
        /// <param name="dest">The stream to which to copy the data.</param>
        /// <param name="length">
        /// The number of bytes to copy from the source stream, or <c>null</c> to copy all data until the source stream
        /// ends.
        /// </param>
        /// <param name="cancelToken">
        /// A cancellation token which allows copying to be cancelled before it completes.
        /// </param>
        public static void DecodeZip(Stream source, Stream dest, long? length,
            CancellationToken cancelToken = default(CancellationToken))
        {
            Stream actualSource = length.HasValue
                ? new PartialStream(source, source.Position, length.Value)
                : source;

            // skip zlib header
            source.ReadBytes(2);

            using (var inflater = new DeflateStream(actualSource, CompressionMode.Decompress, leaveOpen: true))
            {
                inflater.CopyToAsync(dest, BufferSize, cancelToken).Wait(cancelToken);
            }
        }

        /// <summary>
        /// Decodes per-row delta-encoded and then Deflate-compressed data from a source stream and writes the decoded
        /// data to a target stream.
        /// </summary>
        /// <remarks><paramref name="source"/> must already be at the correct position.</remarks>
        /// <param name="source">The stream from which to copy the data.</param>
        /// <param name="dest">The stream to which to copy the data.</param>
        /// <param name="length">
        /// The number of bytes to copy from the source stream, or <c>null</c> to copy all data until the source stream
        /// ends.
        /// </param>
        /// <param name="depth">The depth of the image in bits per pixel color component.</param>
        /// <param name="imageWidth">The width of the image in pixels.</param>
        /// <param name="cancelToken">
        /// A cancellation token which allows copying to be cancelled before it completes.
        /// </param>
        public static void DecodeZipPredicted(Stream source, Stream dest, long? length, int depth, int imageWidth,
            CancellationToken cancelToken = default(CancellationToken))
        {
            if (depth == 1 || depth == 8)
            {
                // no prediction here
                DecodeZip(source, dest, length, cancelToken);
                return;
            }

            Stream actualSource = length.HasValue
                ? new PartialStream(source, source.Position, length.Value)
                : source;

            // skip zlib header
            source.ReadBytes(2);

            var buf = new byte[imageWidth * (depth / 8)];
            using (var inflater = new DeflateStream(actualSource, CompressionMode.Decompress, leaveOpen: true))
            {
                for (;;)
                {
                    try
                    {
                        inflater.ReadBytes(buf);
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }

                    if (depth == 16)
                    {
                        DeltaEncoding.DeltaDecodeBigEndian16(buf);
                        dest.Write(buf, 0, buf.Length);
                    }
                    else if (depth == 32)
                    {
                        DeltaEncoding.DeltaDecodeBigEndian32(buf);
                        dest.Write(buf, 0, buf.Length);
                    }

                    cancelToken.ThrowIfCancellationRequested();
                }
            }
        }

        /// <summary>
        /// Obtains the precomposed image data of a PSD image by decoding per-row delta-encoded and then
        /// Deflate-compressed data from a source stream and writing the decoded data to a target stream.
        /// </summary>
        /// <remarks><paramref name="source"/> must already be at the correct position.</remarks>
        /// <param name="source">The stream from which to copy the data.</param>
        /// <param name="dest">The stream to which to copy the data.</param>
        /// <param name="psd">The PSD file from which to obtain the image depth and width.</param>
        /// <param name="cancelToken">
        /// A cancellation token which allows copying to be cancelled before it completes.
        /// </param>
        public static void DecodeZipPredicted(Stream source, Stream dest, PSDFile psd,
            CancellationToken cancelToken = default(CancellationToken))
        {
            DecodeZipPredicted(source, dest, null, psd.Depth, psd.Width, cancelToken);
        }
    }
}
