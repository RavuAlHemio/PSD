namespace RavuAlHemio.PSD
{
    /// <summary>
    /// The compression type of image data.
    /// </summary>
    public enum CompressionType : short
    {
        /// <summary>
        /// Raw, uncompressed data.
        /// </summary>
        RawData = 0,

        /// <summary>
        /// Data compressed using the lossless PackBits run-length encoding scheme.
        /// </summary>
        PackBits = 1,

        /// <summary>
        /// Data compressed using the Deflate algorithm.
        /// </summary>
        ZipWithoutPrediction = 2,

        /// <summary>
        /// Data compressed using the Deflate algorithm applied to a per-row delta-encoded version of the data.
        /// </summary>
        ZipWithPrediction = 3
    }
}
