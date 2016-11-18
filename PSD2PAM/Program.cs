using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using RavuAlHemio.PSD;

namespace PSD2PAM
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: PSD2PAM.exe FILE.psd FILE.pam");
                return 1;
            }

            // read the PSD file
            var photochop = new PSDFile();
            using (var reading = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                photochop.Read(reading);

                var versionInfo = photochop.ImageResources.FirstOrDefault(ir => ir.ID == 0x0421);
                if (versionInfo?.Data[4] == 0)
                {
                    // does not have valid precomposed image
                    Console.WriteLine("PSD2PAM requires that a valid precomposed image is part of the file.");
                    return 1;
                }

                int width = photochop.Width;
                int height = photochop.Height;
                int channelCount = photochop.NumberOfChannels;
                int depth = photochop.Depth;
                if (depth == 1)
                {
                    Console.WriteLine("PSD2PAM doesn't currently support 1-bit images.");
                    return 1;
                }

                long maxValue;
                switch (depth)
                {
                    case 8:
                        maxValue = 0xFF;
                        break;
                    case 16:
                        maxValue = 0xFFFF;
                        break;
                    case 32:
                        maxValue = 0xFFFFFFFF;
                        break;
                    default:
                        Console.WriteLine("Unsupported depth {0}.", depth);
                        return 1;
                }

                int bytesPerColorComponent = depth/8;

                string tupleTypeString = "";
                switch (photochop.ColorMode)
                {
                    case ColorMode.RGB:
                        tupleTypeString = "TUPLTYPE RGB\n";
                        break;
                    case ColorMode.CMYK:
                        tupleTypeString = "TUPLTYPE CMYK\n";
                        break;
                    case ColorMode.Grayscale:
                        tupleTypeString = "TUPLTYPE GRAYSCALE\n";
                        break;
                }

                string header = string.Format(
                    CultureInfo.InvariantCulture,
                    "P7\nWIDTH {0}\nHEIGHT {1}\nDEPTH {2}\nMAXVAL {3}\n{4}ENDHDR\n",
                    width,
                    height,
                    channelCount,
                    maxValue,
                    tupleTypeString
                );

                reading.Seek(photochop.PrecomposedImageData.Offset, SeekOrigin.Begin);

                using (var tempFiles = new TemporaryFilesDeleteOnDispose(1 + channelCount))
                {
                    // get planar image
                    using (var planarImageStorage = new FileStream(tempFiles.FileNames[0],
                        FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var zippage = new GZipStream(planarImageStorage, CompressionLevel.Fastest))
                    {
                        switch (photochop.PrecomposedImageData.Compression)
                        {
                            case CompressionType.RawData:
                                PixelDataDecoding.DecodeRawData(reading, zippage, null);
                                break;
                            case CompressionType.PackBits:
                                int scanlineCount = height * channelCount;
                                PixelDataDecoding.DecodePackBits(reading, zippage, scanlineCount, photochop.Version == 2);
                                break;
                            case CompressionType.ZipWithoutPrediction:
                                PixelDataDecoding.DecodeZip(reading, zippage, null);
                                break;
                            case CompressionType.ZipWithPrediction:
                                PixelDataDecoding.DecodeZipPredicted(reading, zippage, null, depth, width);
                                break;
                        }
                    }

                    // split planar image into plane files
                    var buf = new byte[width*bytesPerColorComponent];
                    using (var reader = new FileStream(tempFiles.FileNames[0], FileMode.Open, FileAccess.Read,
                        FileShare.Read))
                    using (var unzippage = new GZipStream(reader, CompressionMode.Decompress))
                    {
                        for (int c = 0; c < channelCount; ++c)
                        {
                            using (var writer = new FileStream(tempFiles.FileNames[1 + c], FileMode.Create,
                                FileAccess.Write, FileShare.None))
                            using (var zippage = new GZipStream(writer, CompressionLevel.Fastest))
                            {
                                for (int y = 0; y < height; ++y)
                                {
                                    if (y%32 == 31)
                                    {
                                        Console.WriteLine("splitting channel {0}/{1} row {2}/{3}", c+1, channelCount,
                                            y+1, height);
                                    }

                                    int rd = unzippage.Read(buf, 0, buf.Length);
                                    if (rd < buf.Length)
                                    {
                                        throw new Exception("short read");
                                    }
                                    zippage.Write(buf, 0, buf.Length);
                                }
                            }
                        }
                    }

                    using (var streamDisposal = new DisposableStreamGroup())
                    {
                        // reopen the planes
                        for (int c = 0; c < channelCount; ++c)
                        {
                            var stream = new FileStream(tempFiles.FileNames[1 + c], FileMode.Open,
                                FileAccess.Read, FileShare.Read);
                            var unzipper = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: false);
                            streamDisposal.Streams.Add(unzipper);
                        }

                        // interleave the components
                        var pixel = new byte[bytesPerColorComponent];
                        using (var writer = new FileStream(args[1], FileMode.Create, FileAccess.Write))
                        using (var zipper = new GZipStream(writer, CompressionLevel.Fastest))
                        using (var bufferer = new BufferedStream(zipper, 8 * 1024 * 1024))
                        {
                            // write PAM header
                            var pamHeaderBytes = header.Select(c => (byte)c).ToArray();
                            bufferer.Write(pamHeaderBytes, 0, pamHeaderBytes.Length);

                            // read the pixels
                            for (int y = 0; y < height; ++y)
                            {
                                if (y % 32 == 31)
                                {
                                    Console.WriteLine("copying row {0}/{1}", y + 1, height);
                                }
                                for (int x = 0; x < width; ++x)
                                {
                                    for (int c = 0; c < channelCount; ++c)
                                    {
                                        int r = streamDisposal.Streams[c].Read(pixel, 0, pixel.Length);
                                        if (r != bytesPerColorComponent)
                                        {
                                            throw new Exception("under-read");
                                        }

                                        // invert color (Photoshop -> PAM)
                                        for (int i = 0; i < pixel.Length; ++i)
                                        {
                                            pixel[i] = (byte)(0xFF - pixel[i]);
                                        }

                                        bufferer.Write(pixel, 0, pixel.Length);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return 0;
        }
    }
}
