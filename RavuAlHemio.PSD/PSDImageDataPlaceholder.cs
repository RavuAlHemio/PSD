namespace RavuAlHemio.PSD
{
    /// <summary>
    /// A placeholder storing the starting position and compression for the precomposed image data stored in a PSD
    /// file.
    /// </summary>
    public class PSDImageDataPlaceholder
    {
        /// <summary>
        /// The method used to compress the precomposed image data.
        /// </summary>
        public CompressionType Compression { get; set; }

        /// <summary>
        /// The byte position in the PSD file at which the precomposed image data starts.
        /// </summary>
        public long Offset { get; set; }
    }
}
