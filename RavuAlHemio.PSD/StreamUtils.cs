using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RavuAlHemio.PSD
{
    internal static class StreamUtils
    {
        public static byte[] ReadBytes(this Stream stream, int byteCount)
        {
            var ret = new byte[byteCount];
            stream.ReadBytes(ret);
            return ret;
        }

        public static void ReadBytes(this Stream stream, byte[] array, int offset = 0, int length = -1)
        {
            if (length == -1)
            {
                length = array.Length;
            }

            int totalRead = 0;
            while (totalRead < length)
            {
                int readNow = stream.Read(array, offset + totalRead, length - totalRead);
                if (readNow == 0)
                {
                    throw new EndOfStreamException();
                }
                totalRead += readNow;
            }
        }

        public static byte ReadByteOrThrow(this Stream stream)
        {
            int b = stream.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte) b;
        }

        public static ushort ReadBigEndianUInt16(this Stream stream)
        {
            var bytes = stream.ReadBytes(2);
            return (ushort)(
                ((uint)bytes[0] << 8) |
                ((uint)bytes[1] << 0)
            );
        }

        public static ushort ReadLittleEndianUInt16(this Stream stream)
        {
            var bytes = stream.ReadBytes(2);
            return (ushort)(
                ((uint)bytes[0] << 0) |
                ((uint)bytes[1] << 8)
            );
        }

        public static short ReadBigEndianInt16(this Stream stream)
        {
            ushort unsigned = stream.ReadBigEndianUInt16();
            return unchecked((short) unsigned);
        }

        public static short ReadLittleEndianInt16(this Stream stream)
        {
            ushort unsigned = stream.ReadLittleEndianUInt16();
            return unchecked((short)unsigned);
        }

        public static uint ReadBigEndianUInt32(this Stream stream)
        {
            var bytes = stream.ReadBytes(4);
            return (
                ((uint)bytes[0] << 24) |
                ((uint)bytes[1] << 16) |
                ((uint)bytes[2] <<  8) |
                ((uint)bytes[3] <<  0)
            );
        }

        public static uint ReadLittleEndianUInt32(this Stream stream)
        {
            var bytes = stream.ReadBytes(4);
            return (
                ((uint)bytes[0] <<  0) |
                ((uint)bytes[1] <<  8) |
                ((uint)bytes[2] << 16) |
                ((uint)bytes[3] << 24)
            );
        }

        public static int ReadBigEndianInt32(this Stream stream)
        {
            uint unsigned = stream.ReadBigEndianUInt32();
            return unchecked((int) unsigned);
        }

        public static int ReadLittleEndianInt32(this Stream stream)
        {
            uint unsigned = stream.ReadLittleEndianUInt32();
            return unchecked((int)unsigned);
        }

        public static ulong ReadBigEndianUInt64(this Stream stream)
        {
            var bytes = stream.ReadBytes(8);
            return (
                ((ulong)bytes[0] << 56) |
                ((ulong)bytes[1] << 48) |
                ((ulong)bytes[2] << 40) |
                ((ulong)bytes[3] << 32) |
                ((ulong)bytes[4] << 24) |
                ((ulong)bytes[5] << 16) |
                ((ulong)bytes[6] <<  8) |
                ((ulong)bytes[7] <<  0)
            );
        }

        public static ulong ReadLittleEndianUInt64(this Stream stream)
        {
            var bytes = stream.ReadBytes(8);
            return (
                ((ulong)bytes[0] <<  0) |
                ((ulong)bytes[1] <<  8) |
                ((ulong)bytes[2] << 16) |
                ((ulong)bytes[3] << 24) |
                ((ulong)bytes[4] << 32) |
                ((ulong)bytes[5] << 40) |
                ((ulong)bytes[6] << 48) |
                ((ulong)bytes[7] << 56)
            );
        }

        public static long ReadBigEndianInt64(this Stream stream)
        {
            ulong unsigned = stream.ReadBigEndianUInt64();
            return unchecked((long) unsigned);
        }

        public static long ReadLittleEndianInt64(this Stream stream)
        {
            ulong unsigned = stream.ReadLittleEndianUInt64();
            return unchecked((long)unsigned);
        }

        public static string ReadUsAsciiString(this Stream stream, int byteCount)
        {
            var bytes = stream.ReadBytes(byteCount);

            if (bytes.Any(b => b > 0x7F))
            {
                byte firstInvalidByte = bytes.First(b => b > 0x7F);
                throw new DecoderFallbackException($"invalid US-ASCII byte value {(int)firstInvalidByte}");
            }

            var chars = bytes
                .Select(c => (char)c)
                .ToArray();

            return new string(chars);
        }

        public static string ReadUsAsciiPascalStringPaddedToEven(this Stream stream)
        {
            // get length (1 byte)
            byte length = stream.ReadByteOrThrow();

            // read that many bytes
            string str = stream.ReadUsAsciiString(length);

            // if the length is even, then length + string is an odd number of bytes, so skip one more byte
            if (length%2 == 0)
            {
                int paddingByte = stream.ReadByte();
                if (paddingByte == -1)
                {
                    throw new EndOfStreamException();
                }
            }

            return str;
        }

        public static float ReadBigEndianSingle(this Stream stream)
        {
            uint bytesAsInteger = stream.ReadBigEndianUInt32();
            byte[] bytesAsArray = BitConverter.GetBytes(bytesAsInteger);
            return BitConverter.ToSingle(bytesAsArray, 0);
        }

        public static float ReadLittleEndianSingle(this Stream stream)
        {
            uint bytesAsInteger = stream.ReadLittleEndianUInt32();
            byte[] bytesAsArray = BitConverter.GetBytes(bytesAsInteger);
            return BitConverter.ToSingle(bytesAsArray, 0);
        }

        public static double ReadBigEndianDouble(this Stream stream)
        {
            ulong bytesAsInteger = stream.ReadBigEndianUInt64();
            byte[] bytesAsArray = BitConverter.GetBytes(bytesAsInteger);
            return BitConverter.ToDouble(bytesAsArray, 0);
        }

        public static double ReadLittleEndianDouble(this Stream stream)
        {
            ulong bytesAsInteger = stream.ReadLittleEndianUInt64();
            byte[] bytesAsArray = BitConverter.GetBytes(bytesAsInteger);
            return BitConverter.ToDouble(bytesAsArray, 0);
        }
    }
}
