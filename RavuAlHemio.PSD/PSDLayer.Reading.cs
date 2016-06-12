using System;
using System.Collections.Generic;
using System.IO;

namespace RavuAlHemio.PSD
{
    public partial class PSDLayer
    {
        public static class Reading
        {
            internal const string BlendModeSignature = "8BIM";

            public static void ReadLayerRecord(PSDFile psd, PSDLayer layer, Stream stream)
            {
                layer.Top = stream.ReadBigEndianInt32();
                layer.Left = stream.ReadBigEndianInt32();
                layer.Bottom = stream.ReadBigEndianInt32();
                layer.Right = stream.ReadBigEndianInt32();

                short numberChannels = stream.ReadBigEndianInt16();
                layer.Channels = new PSDLayerChannel[numberChannels];
                for (int i = 0; i < numberChannels; ++i)
                {
                    var channel = new PSDLayerChannel();
                    channel.ID = stream.ReadBigEndianInt16();
                    channel.DataLength = (psd.Version == 2)
                        ? stream.ReadBigEndianInt64()
                        : stream.ReadBigEndianInt32();
                    layer.Channels[i] = channel;
                }

                string blendModeSignature = stream.ReadUsAsciiString(4);
                if (blendModeSignature != BlendModeSignature)
                {
                    throw new PSDFormatException($"expected blend mode signature \"{BlendModeSignature}\", got \"{blendModeSignature}\"");
                }

                int blendModeValue = stream.ReadBigEndianInt32();
                if (!Enum.IsDefined(typeof(BlendMode), blendModeValue))
                {
                    char[] blendModeChars =
                    {
                        (char)((blendModeValue >> 24) & 0xFF),
                        (char)((blendModeValue >> 16) & 0xFF),
                        (char)((blendModeValue >>  8) & 0xFF),
                        (char)((blendModeValue >>  0) & 0xFF)
                    };
                    string blendModeString = new string(blendModeChars);
                    throw new PSDFormatException($"invalid blend mode {blendModeValue} (\"{blendModeString}\")");
                }
                layer.BlendMode = (BlendMode) blendModeValue;

                byte opacity = stream.ReadByteOrThrow();
                layer.Opacity = opacity;

                byte clipping = stream.ReadByteOrThrow();
                if (clipping > 1)
                {
                    throw new PSDFormatException($"invalid clipping value {clipping}; expected 0 (base) or 1 (non-base)");
                }
                layer.NonBaseClipping = (clipping == 1);

                byte flags = stream.ReadByteOrThrow();
                if ((flags & 0x01) != 0)
                {
                    layer.TransparencyProtected = true;
                }
                if ((flags & 0x02) != 0)
                {
                    layer.Visible = true;
                }
                if ((flags & 0x04) != 0)
                {
                    layer.Obsolete = true;
                }
                if ((flags & 0x18) == 0x18)
                {
                    // (both bits 3 and 4 must be set)
                    layer.PixelDataIrrelevantToDocumentAppearance = true;
                }

                // filler
                stream.ReadByteOrThrow();

                int extraDataLength = stream.ReadBigEndianInt32();
                long extraDataStart = stream.Position;
                ReadLayerMask(layer, stream);
                ReadLayerBlendingRanges(layer, stream);

                byte layerNameLength = stream.ReadByteOrThrow();
                layer.Name = stream.ReadWindows1252String(layerNameLength);
                int padTo4Bytes = 4 - ((layerNameLength + 1)%4);
                if (padTo4Bytes != 4)  // there is zero padding if == 4
                {
                    stream.ReadBytes(padTo4Bytes);
                }

                layer.AdditionalInformation = new List<PSDAdditionalLayerInformation>();
                while (stream.Position < extraDataStart + extraDataLength)
                {
                    PSDAdditionalLayerInformation pali = PSDFile.Reading.ReadAdditionalLayerInformation(psd, stream);
                    layer.AdditionalInformation.Add(pali);
                }
            }

            public static void ReadLayerMask(PSDLayer layer, Stream stream)
            {
                int maskSize = stream.ReadBigEndianInt32();
                if (maskSize == 0)
                {
                    layer.LayerMask = null;
                    return;
                }

                var mask = new PSDLayerMask();

                mask.Top = stream.ReadBigEndianInt32();
                mask.Left = stream.ReadBigEndianInt32();
                mask.Bottom = stream.ReadBigEndianInt32();
                mask.Right = stream.ReadBigEndianInt32();

                byte defaultColor = stream.ReadByteOrThrow();
                if (defaultColor != 0 && defaultColor != 255)
                {
                    throw new PSDFormatException($"expected layer mask default color to be 0 or 255, got {defaultColor}");
                }
                mask.DefaultColor = defaultColor;

                byte flags = stream.ReadByteOrThrow();
                if ((flags & 0x01) == 0x01)
                {
                    mask.PositionIsRelativeToLayerData = true;
                }
                if ((flags & 0x02) == 0x02)
                {
                    mask.Disabled = true;
                }
                if ((flags & 0x04) == 0x04)
                {
                    mask.InvertMaskWhenBlending = true;
                }
                if ((flags & 0x08) == 0x08)
                {
                    mask.OriginatesFromRenderingOtherData = true;
                }
                bool hasParameters = ((flags & 0x10) == 0x10);

                if (maskSize == 20 && !hasParameters)
                {
                    // skip two bytes of padding and we're done
                    stream.ReadBytes(2);
                    return;
                }

                // rectangle + default color + flags + real flags + real BG + real rectangle
                int actualMaskSize = 4*4 + 1 + 1 + 1 + 1 + 4*4;
                if (hasParameters)
                {
                    actualMaskSize += 1;

                    // read parameters
                    byte availableParameters = stream.ReadByteOrThrow();
                    if ((availableParameters & 0x01) == 0x01)
                    {
                        actualMaskSize += 1;
                        mask.UserMaskDensity = stream.ReadByteOrThrow();
                    }
                    if ((availableParameters & 0x02) == 0x02)
                    {
                        actualMaskSize += 8;
                        mask.UserMaskFeather = stream.ReadBigEndianDouble();
                    }
                    if ((availableParameters & 0x04) == 0x04)
                    {
                        actualMaskSize += 1;
                        mask.VectorMaskDensity = stream.ReadByteOrThrow();
                    }
                    if ((availableParameters & 0x08) == 0x08)
                    {
                        actualMaskSize += 8;
                        mask.VectorMaskFeather = stream.ReadBigEndianDouble();
                    }
                }

                if (actualMaskSize != maskSize)
                {
                    throw new PSDFormatException(
                        $"actual calculated layer mask size ({actualMaskSize}) does not match mask size in header ({maskSize})"
                    );
                }

                // yay, duplicated data...
                // FIXME: let's just skip it
                stream.ReadBytes(18);
            }

            public static void ReadLayerBlendingRanges(PSDLayer layer, Stream stream)
            {
                int length = stream.ReadBigEndianInt32();
                if (length%8 != 0)
                {
                    throw new PSDFormatException($"expected a layer blending ranges block length divisible by 8, got {length} ({length % 8} remainder)");
                }

                int count = length/8;
                var blendingRanges = new PSDLayerBlendingRange[count];
                for (int i = 0; i < count; ++i)
                {
                    blendingRanges[i].SourceLowFirst = stream.ReadByteOrThrow();
                    blendingRanges[i].SourceLowSecond = stream.ReadByteOrThrow();
                    blendingRanges[i].SourceHighFirst = stream.ReadByteOrThrow();
                    blendingRanges[i].SourceHighSecond = stream.ReadByteOrThrow();
                    blendingRanges[i].DestinationLowFirst = stream.ReadByteOrThrow();
                    blendingRanges[i].DestinationLowSecond = stream.ReadByteOrThrow();
                    blendingRanges[i].DestinationHighFirst = stream.ReadByteOrThrow();
                    blendingRanges[i].DestinationHighSecond = stream.ReadByteOrThrow();
                }
                layer.BlendingRanges = blendingRanges;
            }
        }
    }
}
