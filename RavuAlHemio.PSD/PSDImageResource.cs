using System;
using System.IO;

namespace RavuAlHemio.PSD
{
    public class PSDImageResource
    {
        internal const string Magic = "8BIM";

        public short ID { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }

        protected internal void ReadResourceBlock(Stream stream)
        {
            string magic = stream.ReadUsAsciiString(4);
            if (!string.Equals(magic, Magic, StringComparison.Ordinal))
            {
                throw new PSDFormatException($"unexpected image resource magic value (is \"{magic}\", expected \"{Magic}\")");
            }

            ID = stream.ReadBigEndianInt16();
            Name = stream.ReadUsAsciiPascalStringPaddedToEven();

            int dataSize = stream.ReadBigEndianInt32();
            if (dataSize < 0)
            {
                throw new PSDFormatException($"image resource has data size {dataSize}, expected at least 0");
            }

            Data = stream.ReadBytes(dataSize);
            if (dataSize%2 == 1)
            {
                // PSD files pad image resource data to even sizes; skip a byte
                int paddingByte = stream.ReadByte();
                if (paddingByte == -1)
                {
                    throw new PSDFormatException();
                }
            }
        }
    }
}
