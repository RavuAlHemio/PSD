using System;
using System.IO;

namespace RavuAlHemio.PSD
{
    public partial class PSDImageResource
    {
        public static class Reading
        {
            public static void ReadResourceBlock(PSDImageResource resource, Stream stream)
            {
                string magic = stream.ReadUsAsciiString(4);
                if (!string.Equals(magic, Magic, StringComparison.Ordinal))
                {
                    throw new PSDFormatException($"unexpected image resource magic value (is \"{magic}\", expected \"{Magic}\")");
                }

                resource.ID = stream.ReadBigEndianInt16();
                resource.Name = stream.ReadUsAsciiPascalStringPaddedToEven();

                int dataSize = stream.ReadBigEndianInt32();
                if (dataSize < 0)
                {
                    throw new PSDFormatException($"image resource has data size {dataSize}, expected at least 0");
                }

                resource.Data = stream.ReadBytes(dataSize);
                if (dataSize % 2 == 1)
                {
                    // PSD files pad image resource data to even sizes; skip a byte
                    stream.ReadByteOrThrow();
                }
            }
        }
    }
}
