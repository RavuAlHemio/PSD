using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RavuAlHemio.PSD
{
    public partial class PSDFile
    {
        /// <summary>
        /// Contains methods decoding the PSD format.
        /// </summary>
        public static class Reading
        {
            /// <summary>
            /// The magic four bytes identifying Photoshop files.
            /// </summary>
            public const string Magic = "8BPS";

            /// <summary>
            /// Magic four bytes identifying additional layer info blocks.
            /// </summary>
            public const string AdditionalLayerInfoSignature = "8BIM";

            /// <summary>
            /// Alternative magic four bytes identifying additional layer info blocks.
            /// </summary>
            public const string AdditionalLayerInfoSignature2 = "8B64";

            /// <summary>
            /// Reads the PSD header from a stream and populates a <see cref="PSDFile"/> with the information.
            /// </summary>
            /// <param name="psd">The PSD file object to populate.</param>
            /// <param name="stream">The stream from which to read the PSD header.</param>
            public static void ReadHeader(PSDFile psd, Stream stream)
            {
                string magic = stream.ReadUsAsciiString(4);
                if (!string.Equals(magic, Magic, StringComparison.Ordinal))
                {
                    throw new PSDFormatException($"unexpected magic value (is \"{magic}\", expected \"{Magic}\")");
                }

                short version = stream.ReadBigEndianInt16();
                if (version < 1 || version > 2)
                {
                    throw new PSDFormatException($"version is {version}, expected 1 or 2");
                }
                psd.Version = version;

                byte[] reserved = stream.ReadBytes(6);
                if (reserved.Any(b => b != 0x00))
                {
                    throw new PSDFormatException("nonzero byte in reserved 6-byte value");
                }

                short numberOfChannels = stream.ReadBigEndianInt16();
                if (numberOfChannels < MinChannels || numberOfChannels > MaxChannels)
                {
                    throw new PSDFormatException(
                        $"number of channels is is {numberOfChannels}, expected at least {MinChannels} and at most {MaxChannels}");
                }
                psd.NumberOfChannels = numberOfChannels;

                int height = stream.ReadBigEndianInt32();
                if (height < 1)
                {
                    throw new PSDFormatException($"height is {height}, expected at least 1");
                }
                if (version == 1 && height > Version1MaxDimension)
                {
                    throw new PSDFormatException(
                        $"height is {height} and this is a version 1 document, expected at most {Version1MaxDimension}");
                }
                if (version == 2 && height > Version2MaxDimension)
                {
                    throw new PSDFormatException(
                        $"height is {height} and this is a version 2 document, expected at most {Version2MaxDimension}");
                }
                psd.Height = height;

                int width = stream.ReadBigEndianInt32();
                if (width < 1)
                {
                    throw new PSDFormatException($"width is {width}, expected at least 1");
                }
                if (version == 1 && width > Version1MaxDimension)
                {
                    throw new PSDFormatException(
                        $"width is {width} and this is a version 1 document, expected at most {Version1MaxDimension}");
                }
                if (version == 2 && width > Version2MaxDimension)
                {
                    throw new PSDFormatException(
                        $"width is {width} and this is a version 2 document, expected at most {Version2MaxDimension}");
                }
                psd.Width = width;

                short depth = stream.ReadBigEndianInt16();
                if (depth != 1 && depth != 8 && depth != 16 && depth != 32)
                {
                    throw new PSDFormatException($"depth is {depth}, expected 1 or 8 or 16 or 32");
                }
                psd.Depth = depth;

                short colorMode = stream.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(ColorMode), colorMode))
                {
                    throw new PSDFormatException(
                        $"color mode is {colorMode}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<ColorMode, short>())}");
                }
                psd.ColorMode = (ColorMode) colorMode;
            }
            
            /// <summary>
            /// Reads PSD color mode data from a stream and populates a <see cref="PSDFile"/> with the information.
            /// </summary>
            /// <param name="psd">The PSD file object to populate.</param>
            /// <param name="stream">The stream from which to read the PSD color mode data.</param>
            public static void ReadColorModeData(PSDFile psd, Stream stream)
            {
                int colorModeDataLength = stream.ReadBigEndianInt32();
                if (colorModeDataLength < 0)
                {
                    throw new PSDFormatException($"color mode data length is {colorModeDataLength}, expected at least 0");
                }

                bool requiresColorModeData = (psd.ColorMode == ColorMode.Indexed || psd.ColorMode == ColorMode.Duotone);
                bool hasColorModeData = (colorModeDataLength > 0);
                if (hasColorModeData != requiresColorModeData)
                {
                    throw new PSDFormatException($"color mode is {psd.ColorMode} but color mode data length is 0");
                }

                psd.ColorModeData = stream.ReadBytes(colorModeDataLength);
            }

            /// <summary>
            /// Reads the PSD image resources from a stream and populates a <see cref="PSDFile"/> with the information.
            /// </summary>
            /// <param name="psd">The PSD file object to populate.</param>
            /// <param name="stream">The stream from which to read the PSD image resources.</param>
            public static void ReadImageResources(PSDFile psd, Stream stream)
            {
                int imageResourceSectionLength = stream.ReadBigEndianInt32();
                if (imageResourceSectionLength < 0)
                {
                    throw new PSDFormatException(
                        $"image resource section length is {imageResourceSectionLength}, expected at least 0");
                }

                psd.ImageResources = new List<PSDImageResource>();

                long endPosition = stream.Position + imageResourceSectionLength;
                while (stream.Position < endPosition)
                {
                    var resource = new PSDImageResource();
                    PSDImageResource.Reading.ReadResourceBlock(resource, stream);
                    psd.ImageResources.Add(resource);
                }
            }

            /// <summary>
            /// Reads PSD layer and mask information from a stream and populates a <see cref="PSDFile"/> with the
            /// information.
            /// </summary>
            /// <param name="psd">The PSD file object to populate.</param>
            /// <param name="stream">The stream from which to read the PSD layer and mask information.</param>
            public static void ReadLayerAndMaskInformation(PSDFile psd, Stream stream)
            {
                long layerMaskInfoLength = (psd.Version == 2)
                    ? stream.ReadBigEndianInt64()
                    : stream.ReadBigEndianInt32();

                long layerStartPosition = stream.Position;

                ReadLayerInformation(psd, stream);
                ReadGlobalMaskInformation(psd, stream);
                
                psd.AdditionalLayerInformation = new List<PSDAdditionalLayerInformation>();
                while (stream.Position - layerStartPosition < layerMaskInfoLength)
                {
                    var additionalLayerInfo = ReadAdditionalLayerInformation(psd, stream);
                    if (additionalLayerInfo != null)
                    {
                        psd.AdditionalLayerInformation.Add(additionalLayerInfo);
                    }
                }
            }

            /// <summary>
            /// Reads the layer information subsection of a PSD layer and mask information section from a stream and
            /// populates a <see cref="PSDFile"/> with the information.
            /// </summary>
            /// <param name="psd">The PSD file object to populate.</param>
            /// <param name="stream">The stream from which to read the layer information subsection.</param>
            /// <param name="roundToFourBytes">
            /// Whether the layer information block is padded to a multiple of 4 bytes.
            /// </param>
            public static void ReadLayerInformation(PSDFile psd, Stream stream, bool roundToFourBytes = false)
            {
                long layerInfoLength = (psd.Version == 2)
                    ? stream.ReadBigEndianInt64()
                    : stream.ReadBigEndianInt32();
                if (layerInfoLength == 0)
                {
                    // no layers at all
                    psd.Layers = new PSDLayer[0];
                    return;
                }

                long layerInfoStart = stream.Position;

                short layerCount = stream.ReadBigEndianInt16();
                if (layerCount < 0)
                {
                    // FIXME: "If it is a negative number, its absolute value is the number of layers and the first alpha channel contains the transparency data for the merged result."
                    layerCount = (short) -layerCount;
                }

                psd.Layers = new PSDLayer[layerCount];
                for (int i = 0; i < layerCount; ++i)
                {
                    psd.Layers[i] = new PSDLayer();
                    PSDLayer.Reading.ReadLayerRecord(psd, psd.Layers[i], stream);
                }

                long totalDataLength = 0;
                for (int l = 0; l < layerCount; ++l)
                {
                    PSDLayer layer = psd.Layers[l];

                    for (int c = 0; c < layer.Channels.Length; ++c)
                    {
                        PSDLayerChannel channel = layer.Channels[c];

                        // get compression type
                        short compressionValue = stream.ReadBigEndianInt16();
                        if (!Enum.IsDefined(typeof(CompressionType), compressionValue))
                        {
                            throw new PSDFormatException($"unknown layer data compression type {compressionValue}");
                        }
                        var compression = (CompressionType) compressionValue;
                        
                        channel.Data = new PSDLayerChannelDataPlaceholder
                        {
                            Compression = compression,
                            Offset = stream.Position,
                            DataLength = channel.DataLength - 2
                        };

                        long dataLengthWithCompressionInfo = channel.DataLength;
                        totalDataLength += dataLengthWithCompressionInfo;

                        // skip
                        stream.Seek(channel.Data.DataLength, SeekOrigin.Current);
                    }
                }

                // skip padding if any
                if (stream.Position < layerInfoStart + layerInfoLength)
                {
                    // padding; skip
                    int skipBytes = (int)((layerInfoStart + layerInfoLength) - stream.Position);
                    stream.ReadBytes(skipBytes);
                }

                // additionally round up to 4 bytes if requested (part of Layr/Lr16/Lr32)
                // WARNING: spec is a liar; says "rounded up to an even byte count"
                if (roundToFourBytes && layerInfoLength % 4 != 0)
                {
                    var skipCount = (int)(4 - (layerInfoLength % 4));
                    stream.ReadBytes(skipCount);
                }
            }

            /// <summary>
            /// Reads the global mask information subsection of a PSD layer and mask information section from a stream
            /// and populates a <see cref="PSDFile"/> with the information.
            /// </summary>
            /// <param name="psd">The PSD file object to populate.</param>
            /// <param name="stream">The stream from which to read the layer information subsection.</param>
            public static void ReadGlobalMaskInformation(PSDFile psd, Stream stream)
            {
                int length = stream.ReadBigEndianInt32();
                if (length == 0)
                {
                    // no global mask info
                    psd.GlobalLayerMask = null;
                    return;
                }

                var mask = new PSDGlobalLayerMask();

                mask.OverlayColorSpace = stream.ReadBigEndianInt16();
                mask.ColorComponent1 = stream.ReadBigEndianInt16();
                mask.ColorComponent2 = stream.ReadBigEndianInt16();
                mask.ColorComponent3 = stream.ReadBigEndianInt16();
                mask.ColorComponent4 = stream.ReadBigEndianInt16();

                short opacity = stream.ReadBigEndianInt16();
                if (opacity < 0 || opacity > 100)
                {
                    throw new PSDFormatException($"global mask opacity must be at least 0 and at most 100, got {opacity}");
                }
                mask.Opacity = opacity;

                byte kind = stream.ReadByteOrThrow();
                if (!Enum.IsDefined(typeof(LayerMaskKind), kind))
                {
                    throw new PSDFormatException($"unknown layer mask kind {kind}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<LayerMaskKind, byte>())}");
                }
                mask.Kind = (LayerMaskKind) kind;

                int paddingByteCount = length - 13;
                stream.ReadBytes(paddingByteCount);

                psd.GlobalLayerMask = mask;
            }

            /// <summary>
            /// Reads an item of additional layer information within the additional layer information subsection of a
            /// PSD layer and mask information section from a stream and either returns a
            /// <see cref="PSDAdditionalLayerInformation"/> object containing the additional information, or (if this
            /// is an "out-of-band" layer information block) populates a <see cref="PSDFile"/> with the layer
            /// information within.
            /// </summary>
            /// <param name="psd">
            /// The PSD file object to populate if this is an "out-of-band" layer information block.
            /// </param>
            /// <param name="stream">The stream from which to read the layer information subsection.</param>
            /// <returns>
            /// The additional layer information, or <c>null</c> if the encountered block is an "out-of-band" layer
            /// information block.
            /// </returns>
            public static PSDAdditionalLayerInformation ReadAdditionalLayerInformation(PSDFile psd, Stream stream)
            {
                var ali = new PSDAdditionalLayerInformation();

                string signature = stream.ReadUsAsciiString(4);
                if (signature != AdditionalLayerInfoSignature && signature != AdditionalLayerInfoSignature2)
                {
                    throw new PSDFormatException($"unknown additional layer information signature \"{signature}\", expected \"{AdditionalLayerInfoSignature}\" or \"{AdditionalLayerInfoSignature2}\"");
                }

                string key = stream.ReadUsAsciiString(4);
                bool length64Bit = false;

                if (key == "Layr" || key == "Lr16" || key == "Lr32")
                {
                    // layer info has been shunted in here instead of where it belongs
                    ReadLayerInformation(psd, stream, roundToFourBytes: true);
                    return null;
                }

                if (psd.Version == 2)
                {
                    // some keys have 64-bit lengths
                    length64Bit =
                        (key == "LMsk") ||
                        (key == "Mt16") ||
                        (key == "Mt32") ||
                        (key == "Mtrn") ||
                        (key == "Alph") ||
                        (key == "FMsk") ||
                        (key == "Ink2") ||
                        (key == "FEid") ||
                        (key == "FXid") ||
                        (key == "PxSD")
                    ;
                }

                long length = length64Bit
                    ? stream.ReadBigEndianInt64()
                    : stream.ReadBigEndianInt32();

                byte[] data = stream.ReadBytes((int) length);

                ali.Key = key;
                ali.Data = data;

                // round up to 4 bytes
                // WARNING: spec is a liar; says "rounded up to an even byte count"
                if (length%4 != 0)
                {
                    var skipCount = (int)(4 - (length%4));
                    stream.ReadBytes(skipCount);
                }

                return ali;
            }

            /// <summary>
            /// Reads metadata about a PSD file's precomposed image data and populates a <see cref="PSDFile"/> with it.
            /// </summary>
            /// <param name="psd">The PSD file object to populate.</param>
            /// <param name="stream">The stream from which to read the precomposed image data metadata.</param>
            public static void CreateImageDataPlaceholder(PSDFile psd, Stream stream)
            {
                var placeholder = new PSDImageDataPlaceholder();
                
                // read compression method
                short compressionValue = stream.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(CompressionType), compressionValue))
                {
                    throw new PSDFormatException($"unexpected compression type {compressionValue}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<CompressionType, short>())}");
                }
                placeholder.Compression = (CompressionType) compressionValue;
                placeholder.Offset = stream.Position;

                psd.PrecomposedImageData = placeholder;
            }
        }
    }
}
