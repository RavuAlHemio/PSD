using System;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// Utility functions pertaining to delta-encoding.
    /// </summary>
    public static class DeltaEncoding
    {
        /// <summary>
        /// Reverses delta-encoding in place on the big-endian 16-bit integer values stored as a byte array.
        /// </summary>
        /// <param name="bytes">
        /// The byte array containing big-endian 16-bit integer values whose delta-encoding to reverse.
        /// </param>
        public static void DeltaDecodeBigEndian16(byte[] bytes)
        {
            if (bytes.Length%2 != 0)
            {
                throw new ArgumentException($"the length of {nameof(bytes)} must be divisible by 2", nameof(bytes));
            }

            if (bytes.Length == 0)
            {
                return;
            }

            ushort previous = (ushort)(
                (bytes[0] << 8) |
                (bytes[1] << 0)
            );

            for (int i = 2; i < bytes.Length; i += 2)
            {
                ushort current = (ushort)(
                    (bytes[i+0] << 8) |
                    (bytes[i+1] << 0)
                );
                current = unchecked((ushort)(current + previous));
                bytes[i + 0] = (byte)((current >> 8) & 0xFF);
                bytes[i + 1] = (byte)((current >> 0) & 0xFF);
                previous = current;
            }
        }

        /// <summary>
        /// Reverses delta-encoding in place on the big-endian 32-bit integer values stored as a byte array.
        /// </summary>
        /// <param name="bytes">
        /// The byte array containing big-endian 32-bit integer values whose delta-encoding to reverse.
        /// </param>
        public static void DeltaDecodeBigEndian32(byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
            {
                throw new ArgumentException($"the length of {nameof(bytes)} must be divisible by 4", nameof(bytes));
            }

            if (bytes.Length == 0)
            {
                return;
            }

            uint previous = (uint)(
                (bytes[0] << 24) |
                (bytes[1] << 16) |
                (bytes[2] <<  8) |
                (bytes[3] <<  0)
            );

            for (int i = 4; i < bytes.Length; i += 4)
            {
                uint current = (uint)(
                    (bytes[i + 0] << 24) |
                    (bytes[i + 1] << 16) |
                    (bytes[i + 2] <<  8) |
                    (bytes[i + 3] <<  0)
                );
                current = unchecked(current + previous);
                bytes[i + 0] = (byte)((current >> 24) & 0xFF);
                bytes[i + 1] = (byte)((current >> 16) & 0xFF);
                bytes[i + 2] = (byte)((current >>  8) & 0xFF);
                bytes[i + 3] = (byte)((current >>  0) & 0xFF);
                previous = current;
            }
        }
    }
}
