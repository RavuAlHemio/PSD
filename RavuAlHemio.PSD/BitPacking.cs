using System.Collections.Generic;
using System.IO;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// Utility functions pertaining to the PackBits run-length encoding scheme.
    /// </summary>
    public static class BitPacking
    {
        /// <summary>
        /// Reads PackBits-encoded data and returns it in its unpacked form.
        /// </summary>
        /// <param name="bitsToUnpack">An <see cref="IEnumerable{T}"/> of PackBits-encoded bytes.</param>
        /// <returns>The unpacked bytes.</returns>
        public static IEnumerable<byte> UnpackBits(IEnumerable<byte> bitsToUnpack)
        {
            var numera = bitsToUnpack.GetEnumerator();

            for (;;)
            {
                if (!numera.MoveNext())
                {
                    yield break;
                }

                byte countByte = numera.Current;
                sbyte count = unchecked((sbyte) countByte);

                if (count == -128)
                {
                    // skip
                    continue;
                }
                else if (count >= 0)
                {
                    // literal bytes
                    int byteCount = 1 + count;
                    for (int i = 0; i < byteCount; ++i)
                    {
                        if (!numera.MoveNext())
                        {
                            throw new InvalidDataException($"end of enumerable when reading literal string of {byteCount} bytes");
                        }
                        yield return numera.Current;
                    }
                }
                else // if (count < 0)
                {
                    int repeatCount = 1 - count;
                    if (!numera.MoveNext())
                    {
                        throw new InvalidDataException($"end of enumerable when reading byte that should be repeated {repeatCount} times");
                    }
                    byte byteToRepeat = numera.Current;
                    for (int i = 0; i < repeatCount; ++i)
                    {
                        yield return byteToRepeat;
                    }
                }
            }
        }
    }
}
