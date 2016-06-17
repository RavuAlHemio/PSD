using System;
using System.IO;
using RavuAlHemio.PSD;

namespace PSDRead
{
    class Program
    {
        static void Main(string[] args)
        {
            var psd = new PSDFile();
            using (var readStream = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                psd.Read(readStream);
            }

            Console.WriteLine("Version: {0}", psd.Version);
            Console.WriteLine("Number of channels: {0}", psd.NumberOfChannels);
            Console.WriteLine("Width: {0}", psd.Width);
            Console.WriteLine("Height: {0}", psd.Height);
            Console.WriteLine("Depth: {0}", psd.Depth);
            Console.WriteLine("Color mode: {0}", psd.ColorMode);

            Console.WriteLine("Image resources:");
            foreach (var res in psd.ImageResources)
            {
                Console.WriteLine("  Image resource ID: {0}", res.ID);
                //Console.WriteLine("    Name: {0}", res.Name);

                if (res.ID == ResolutionInfo.ResolutionInfoResourceID)
                {
                    Console.WriteLine("    Binary data:");

                    for (int rowOffset = 0; rowOffset < res.Data.Length; rowOffset += 8)
                    {
                        Console.Write("      ");
                        for (int i = 0; i < 8 && rowOffset + i < res.Data.Length; ++i)
                        {
                            Console.Write("{0:X2} ", res.Data[rowOffset + i]);
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine("    Resolution info:");
                    var resInfo = ResolutionInfo.FromPSD(psd);
                    Console.WriteLine("      Horizontal resolution: {0} dpi", resInfo.HorizontalResolutionDPI);
                    Console.WriteLine("        Display unit:        {0}", resInfo.HorizontalResolutionDisplayUnit);
                    Console.WriteLine("      Width display unit:    {0}", resInfo.WidthDisplayUnit);
                    Console.WriteLine("      Vertical resolution:   {0} dpi", resInfo.VerticalResolutionDPI);
                    Console.WriteLine("        Display unit:        {0}", resInfo.VerticalResolutionDisplayUnit);
                    Console.WriteLine("      Height display unit:   {0}", resInfo.HeightDisplayUnit);
                }
                else if (res.ID == VersionInfo.VersionInfoResourceID)
                {
                    var verInfo = VersionInfo.FromPSD(psd);
                    Console.WriteLine("    Version info:");
                    Console.WriteLine("      Version: {0}", verInfo.Version);
                    Console.WriteLine("      {0} valid precomposed data", verInfo.HasValidPrecomposedData ? "Has" : "Does not have");
                    Console.WriteLine("      Writer: {0}", verInfo.WriterName);
                    Console.WriteLine("      Reader: {0}", verInfo.ReaderName);
                    Console.WriteLine("      File version: {0}", verInfo.FileVersion);
                }
            }
            
            Console.WriteLine("Layers: {0}", psd.Layers.Length);
            foreach (PSDLayer layer in psd.Layers)
            {
                Console.WriteLine("    Layer: {0}", layer.Name);
                Console.WriteLine("      Channels: {0}", layer.Channels.Length);
                foreach (PSDLayerChannel chan in layer.Channels)
                {
                    Console.WriteLine("        {0} ({1} bytes {2} from 0x{3:x})", chan.ID, chan.Data.DataLength, chan.Data.Compression, chan.Data.Offset);
                }
                Console.WriteLine("      Additional layer information: {0} entries", layer.AdditionalInformation.Count);
                foreach (PSDAdditionalLayerInformation pali in layer.AdditionalInformation)
                {
                    Console.WriteLine("        Key: {0}", pali.Key);
                    Console.WriteLine("          Data length: {0}", pali.Data.Length);
                }
            }

            if (psd.GlobalLayerMask != null)
            {
                var glm = psd.GlobalLayerMask;
                Console.WriteLine("Global layer mask:");
                Console.WriteLine("  Overlay color space: {0}", glm.OverlayColorSpace);
                Console.WriteLine("  Color components: {0}, {1}, {2}, {3}", glm.ColorComponent1, glm.ColorComponent2, glm.ColorComponent3, glm.ColorComponent4);
                Console.WriteLine("  Opacity: {0}", glm.Opacity);
                Console.WriteLine("  Kind: {0}", glm.Kind);
            }
            else
            {
                Console.WriteLine("No global layer mask");
            }

            Console.WriteLine("Additional layer information: {0} entries", psd.AdditionalLayerInformation.Count);
            foreach (PSDAdditionalLayerInformation ali in psd.AdditionalLayerInformation)
            {
                Console.WriteLine("  Key: {0}", ali.Key);
                Console.WriteLine("    Data length: {0}", ali.Data.Length);
            }

            if (psd.PrecomposedImageData == null)
            {
                Console.WriteLine("No precomposed image data");
            }
            else
            {
                Console.WriteLine("Precomposed image data: {0} from 0x{1:x}", psd.PrecomposedImageData.Compression, psd.PrecomposedImageData.Offset);
            }

            if (args.Length > 1 && args[1] == "-d")
            {
                // decode
                for (int l = 0; l < psd.Layers.Length; ++l)
                {
                    PSDLayer layer = psd.Layers[l];
                    foreach (PSDLayerChannel chan in layer.Channels)
                    {
                        Console.WriteLine("extracting layer {0} channel {1} ({2})", l, chan.ID, chan.Data.Compression);
                        string name = $"{args[0]}.l{l}c{chan.ID}.bin";

                        using (var reader = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var writer = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            reader.Seek(chan.Data.Offset, SeekOrigin.Begin);
                            switch (chan.Data.Compression)
                            {
                                case CompressionType.RawData:
                                    PixelDataDecoding.DecodeRawData(
                                        reader,
                                        writer,
                                        chan.Data.DataLength
                                    );
                                    break;
                                case CompressionType.PackBits:
                                    PixelDataDecoding.DecodePackBits(
                                        reader,
                                        writer,
                                        scanlineCount: layer.Bottom-layer.Top,
                                        fourByteLengths: psd.Version == 2
                                    );
                                    break;
                                case CompressionType.ZipWithoutPrediction:
                                    PixelDataDecoding.DecodeZip(
                                        reader,
                                        writer,
                                        chan.Data.DataLength
                                    );
                                    break;
                                case CompressionType.ZipWithPrediction:
                                    PixelDataDecoding.DecodeZipPredicted(
                                        reader,
                                        writer,
                                        chan.Data.DataLength,
                                        psd.Depth,
                                        psd.Width
                                    );
                                    break;
                            }
                        }
                    }
                }

                if (psd.PrecomposedImageData != null)
                {
                    string compositeName = $"{args[0]}.composite.bin";
                    using (var reader = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var writer = new FileStream(compositeName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        reader.Seek(psd.PrecomposedImageData.Offset, SeekOrigin.Begin);
                        switch (psd.PrecomposedImageData.Compression)
                        {
                            case CompressionType.RawData:
                                PixelDataDecoding.DecodeRawData(
                                    reader,
                                    writer,
                                    length: null
                                );
                                break;
                            case CompressionType.PackBits:
                                PixelDataDecoding.DecodePackBits(
                                    reader,
                                    writer,
                                    scanlineCount: psd.Height * psd.NumberOfChannels,
                                    fourByteLengths: psd.Version == 2
                                );
                                break;
                            case CompressionType.ZipWithoutPrediction:
                                PixelDataDecoding.DecodeZip(
                                    reader,
                                    writer,
                                    length: null
                                );
                                break;
                            case CompressionType.ZipWithPrediction:
                                PixelDataDecoding.DecodeZipPredicted(
                                    reader,
                                    writer,
                                    length: null,
                                    depth: psd.Depth,
                                    imageWidth: psd.Width
                                );
                                break;
                        }
                    }
                }
            }
        }
    }
}
