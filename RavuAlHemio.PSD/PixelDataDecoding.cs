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

        public static void DecodePackBits(Stream source, Stream dest, long? length, int scanlineCount,
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

        /// <remarks>
        /// <paramref name="source"/> must already be at the correct position.
        /// </remarks>
        public static void DecodePackBits(Stream source, Stream dest, PSDFile psd,
            CancellationToken cancelToken = default(CancellationToken))
        {
            int scanlineCount = psd.Height * psd.NumberOfChannels;
            DecodePackBits(source, dest, null, scanlineCount, psd.Version == 2, cancelToken);
        }

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

        /// <remarks>
        /// <paramref name="source"/> must already be at the correct position.
        /// </remarks>
        public static void DecodeZipPredicted(Stream source, Stream dest, PSDFile psd,
            CancellationToken cancelToken = default(CancellationToken))
        {
            DecodeZipPredicted(source, dest, null, psd.Depth, psd.Width, cancelToken);
        }
    }
}
