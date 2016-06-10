using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RavuAlHemio.PSD
{
    public class PSDFile
    {
        internal const string Magic = "8BPS";
        internal const short MinChannels = 1;
        internal const short MaxChannels = 56;
        internal const int Version1MaxDimension = 30000;
        internal const int Version2MaxDimension = 300000;

        public short Version { get; set; }
        public short NumberOfChannels { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public short Depth { get; set; }
        public ColorMode ColorMode { get; set; }
        public byte[] ColorModeData { get; set; }
        public List<PSDImageResource> ImageResources { get; set; }

        public void Read(Stream stream, bool loadData)
        {
            try
            {
                ReadHeader(stream);
                ReadColorModeData(stream);
                ReadImageResources(stream);
                ReadLayerAndMaskInformation(stream, loadData);

                // TODO
                /*
                if (loadData)
                {
                    ReadImageData(stream);
                }
                else
                {
                    CreateImageDataPlaceholder(stream);
                }
                */
            }
            catch (PSDFormatException)
            {
                throw;
            }
            catch (Exception exc)
            {
                throw new PSDFormatException("decoding failed", exc);
            }
        }

        protected internal void ReadHeader(Stream stream)
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
            Version = version;

            byte[] reserved = stream.ReadBytes(6);
            if (reserved.Any(b => b != 0x00))
            {
                throw new PSDFormatException("nonzero byte in reserved 6-byte value");
            }

            short numberOfChannels = stream.ReadBigEndianInt16();
            if (numberOfChannels < MinChannels || numberOfChannels > MaxChannels)
            {
                throw new PSDFormatException($"number of channels is is {numberOfChannels}, expected at least {MinChannels} and at most {MaxChannels}");
            }
            NumberOfChannels = numberOfChannels;

            int height = stream.ReadBigEndianInt32();
            if (height < 1)
            {
                throw new PSDFormatException($"height is {height}, expected at least 1");
            }
            if (version == 1 && height > Version1MaxDimension)
            {
                throw new PSDFormatException($"height is {height} and this is a version 1 document, expected at most {Version1MaxDimension}");
            }
            if (version == 2 && height > Version2MaxDimension)
            {
                throw new PSDFormatException($"height is {height} and this is a version 2 document, expected at most {Version2MaxDimension}");
            }
            Height = height;

            int width = stream.ReadBigEndianInt32();
            if (width < 1)
            {
                throw new PSDFormatException($"width is {width}, expected at least 1");
            }
            if (version == 1 && width > Version1MaxDimension)
            {
                throw new PSDFormatException($"width is {width} and this is a version 1 document, expected at most {Version1MaxDimension}");
            }
            if (version == 2 && width > Version2MaxDimension)
            {
                throw new PSDFormatException($"width is {width} and this is a version 2 document, expected at most {Version2MaxDimension}");
            }
            Width = width;

            short depth = stream.ReadBigEndianInt16();
            if (depth != 1 && depth != 8 && depth != 16 && depth != 32)
            {
                throw new PSDFormatException($"depth is {depth}, expected 1 or 8 or 16 or 32");
            }
            Depth = depth;

            short colorMode = stream.ReadBigEndianInt16();
            if (!Enum.IsDefined(typeof(ColorMode), colorMode))
            {
                throw new PSDFormatException($"color mode is {colorMode}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<ColorMode, short>())}");
            }
            ColorMode = (ColorMode)colorMode;
        }

        protected internal void ReadColorModeData(Stream stream)
        {
            int colorModeDataLength = stream.ReadBigEndianInt32();
            if (colorModeDataLength < 0)
            {
                throw new PSDFormatException($"color mode data length is {colorModeDataLength}, expected at least 0");
            }

            bool requiresColorModeData = (ColorMode == ColorMode.Indexed || ColorMode == ColorMode.Duotone);
            bool hasColorModeData = (colorModeDataLength > 0);
            if (hasColorModeData != requiresColorModeData)
            {
                throw new PSDFormatException($"color mode is {ColorMode} but color mode data length is 0");
            }

            ColorModeData = stream.ReadBytes(colorModeDataLength);
        }

        protected internal void ReadImageResources(Stream stream)
        {
            int imageResourceSectionLength = stream.ReadBigEndianInt32();
            if (imageResourceSectionLength < 0)
            {
                throw new PSDFormatException($"image resource section length is {imageResourceSectionLength}, expected at least 0");
            }

            ImageResources = new List<PSDImageResource>();

            long endPosition = stream.Position + imageResourceSectionLength;
            while (stream.Position < endPosition)
            {
                var resource = new PSDImageResource();
                resource.ReadResourceBlock(stream);
                ImageResources.Add(resource);
            }
        }

        protected internal void ReadLayerAndMaskInformation(Stream stream, bool loadData)
        {
            long layerMaskInfoLength = (Version == 2)
                ? stream.ReadBigEndianInt64()
                : stream.ReadBigEndianInt32();

            long position = stream.Position;

            // TODO
            /*
            ReadLayerInformation(stream);
            ReadGlobalMaskInformation(stream);
            if (stream.Position < layerMaskInfoLength)
            {
                ReadAdditionalLayerInformation(stream);
            }
            */
        }
    }
}
