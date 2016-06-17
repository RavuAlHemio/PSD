using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// Utilities pertaining to stream manupulation.
    /// </summary>
    internal static class StreamUtils
    {
        /// <summary>
        /// Reads a specific number of bytes from the stream and returns a newly allocated array containing them.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <param name="byteCount">The number of bytes to read.</param>
        /// <returns>A newly allocated array containing the bytes read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before <paramref name="byteCount"/> could be read.
        /// </exception>
        public static byte[] ReadBytes(this Stream stream, int byteCount)
        {
            var ret = new byte[byteCount];
            stream.ReadBytes(ret);
            return ret;
        }

        /// <summary>
        /// Reads a specific number of bytes from the stream and fills the given array slice with them.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <param name="array">The array whose slice to fill with bytes read from the stream.</param>
        /// <param name="offset">
        /// The first index of the array slice to fill with bytes read from the stream. <c>0</c> starts at the
        /// beginning of the array.
        /// </param>
        /// <param name="length">
        /// The length of the array slice to fill with bytes read from the stream, or <c>-1</c> to fill the rest of the
        /// array (starting at <paramref name="offset"/>).
        /// </param>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the requested array slice could be filled.
        /// </exception>
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

        /// <summary>
        /// Reads a single byte from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read a byte.</param>
        /// <returns>The byte read from the stream.</returns>
        /// <exception cref="EndOfStreamException">Thrown if the stream ended before a byte could be read.</exception>
        public static byte ReadByteOrThrow(this Stream stream)
        {
            int b = stream.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte) b;
        }

        /// <summary>
        /// Reads a big-endian unsigned 16-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
        public static ushort ReadBigEndianUInt16(this Stream stream)
        {
            var bytes = stream.ReadBytes(2);
            return (ushort)(
                ((uint)bytes[0] << 8) |
                ((uint)bytes[1] << 0)
            );
        }

        /// <summary>
        /// Reads a little-endian unsigned 16-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
        public static ushort ReadLittleEndianUInt16(this Stream stream)
        {
            var bytes = stream.ReadBytes(2);
            return (ushort)(
                ((uint)bytes[0] << 0) |
                ((uint)bytes[1] << 8)
            );
        }

        /// <summary>
        /// Reads a big-endian signed 16-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
        public static short ReadBigEndianInt16(this Stream stream)
        {
            ushort unsigned = stream.ReadBigEndianUInt16();
            return unchecked((short) unsigned);
        }

        /// <summary>
        /// Reads a little-endian signed 16-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
        public static short ReadLittleEndianInt16(this Stream stream)
        {
            ushort unsigned = stream.ReadLittleEndianUInt16();
            return unchecked((short)unsigned);
        }

        /// <summary>
        /// Reads a big-endian unsigned 32-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
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

        /// <summary>
        /// Reads a little-endian unsigned 32-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
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

        /// <summary>
        /// Reads a big-endian signed 32-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
        public static int ReadBigEndianInt32(this Stream stream)
        {
            uint unsigned = stream.ReadBigEndianUInt32();
            return unchecked((int) unsigned);
        }

        /// <summary>
        /// Reads a little-endian signed 32-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
        public static int ReadLittleEndianInt32(this Stream stream)
        {
            uint unsigned = stream.ReadLittleEndianUInt32();
            return unchecked((int)unsigned);
        }

        /// <summary>
        /// Reads a big-endian unsigned 64-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
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

        /// <summary>
        /// Reads a little-endian unsigned 64-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
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

        /// <summary>
        /// Reads a big-endian signed 64-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
        public static long ReadBigEndianInt64(this Stream stream)
        {
            ulong unsigned = stream.ReadBigEndianUInt64();
            return unchecked((long) unsigned);
        }

        /// <summary>
        /// Reads a little-endian signed 64-bit integer from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the integer.</param>
        /// <returns>The integer read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the integer could be read.
        /// </exception>
        public static long ReadLittleEndianInt64(this Stream stream)
        {
            ulong unsigned = stream.ReadLittleEndianUInt64();
            return unchecked((long)unsigned);
        }

        /// <summary>
        /// Reads a US-ASCII string of a specific length from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the string.</param>
        /// <param name="byteCount">The number of characters (bytes) of the US-ASCII string.</param>
        /// <returns>The string read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the string could be read.
        /// </exception>
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

        /// <summary>
        /// Reads a Windows-1252 string of a specific length from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the string.</param>
        /// <param name="byteCount">The number of characters (bytes) of the Windows-1252 string.</param>
        /// <returns>The string read from the stream.</returns>
        /// <exception cref="EndOfStreamException">
        /// Thrown if the stream ended before the string could be read.
        /// </exception>
        public static string ReadWindows1252String(this Stream stream, int byteCount)
        {
            var bytes = stream.ReadBytes(byteCount);
            var coding = Encoding.GetEncoding("windows-1252");
            return coding.GetString(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Reads a US-ASCII Pascal string of a specific length from the stream and ensures that an even number of
        /// bytes have been read.
        /// </summary>
        /// <remarks>
        /// A Pascal string is a sequence of characters prefixed with a byte signifying the length of the string. If
        /// the length of the string is even,  i.e. the length of the data consisting of length and string is odd, an
        /// additional byte is read and discarded.
        /// </remarks>
        /// <param name="stream">The stream from which to read the string.</param>
        /// <returns>The US-ASCII string read from the stream.</returns>
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

        /// <summary>
        /// Reads a Unicode string, as specified by the Photoshop file format documentation, from the stream.
        /// </summary>
        /// <remarks>
        /// A Unicode string, as specified by the Photoshop file format documentation, is a four-byte big-endian
        /// integer specifying the length of the string in UTF-16 encoding units, followed by that number of big-endian
        /// 16-bit integers representing the UTF-16-encoded string.
        /// </remarks>
        /// <param name="stream">The stream from which to read the string.</param>
        /// <returns>The string read from the stream.</returns>
        public static string ReadUnicodeString(this Stream stream)
        {
            int length = stream.ReadBigEndianInt32();
            char[] chars = new char[length];

            for (int i = 0; i < length; ++i)
            {
                chars[i] = (char)stream.ReadBigEndianUInt16();
            }

            return new string(chars);
        }

        /// <summary>
        /// Read a big-endian IEEE 754 binary32 floating-point number from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the floating-point number.</param>
        /// <returns>The floating-point number read from the stream.</returns>
        public static float ReadBigEndianSingle(this Stream stream)
        {
            uint bytesAsInteger = stream.ReadBigEndianUInt32();
            byte[] bytesAsArray = BitConverter.GetBytes(bytesAsInteger);
            return BitConverter.ToSingle(bytesAsArray, 0);
        }

        /// <summary>
        /// Read a little-endian IEEE 754 binary32 floating-point number from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the floating-point number.</param>
        /// <returns>The floating-point number read from the stream.</returns>
        public static float ReadLittleEndianSingle(this Stream stream)
        {
            uint bytesAsInteger = stream.ReadLittleEndianUInt32();
            byte[] bytesAsArray = BitConverter.GetBytes(bytesAsInteger);
            return BitConverter.ToSingle(bytesAsArray, 0);
        }

        /// <summary>
        /// Read a big-endian IEEE 754 binary64 floating-point number from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the floating-point number.</param>
        /// <returns>The floating-point number read from the stream.</returns>
        public static double ReadBigEndianDouble(this Stream stream)
        {
            ulong bytesAsInteger = stream.ReadBigEndianUInt64();
            byte[] bytesAsArray = BitConverter.GetBytes(bytesAsInteger);
            return BitConverter.ToDouble(bytesAsArray, 0);
        }

        /// <summary>
        /// Read a little-endian IEEE 754 binary64 floating-point number from the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the floating-point number.</param>
        /// <returns>The floating-point number read from the stream.</returns>
        public static double ReadLittleEndianDouble(this Stream stream)
        {
            ulong bytesAsInteger = stream.ReadLittleEndianUInt64();
            byte[] bytesAsArray = BitConverter.GetBytes(bytesAsInteger);
            return BitConverter.ToDouble(bytesAsArray, 0);
        }
    }
}
