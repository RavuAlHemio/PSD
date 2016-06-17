namespace RavuAlHemio.PSD
{
    public class PSDLayerChannelDataPlaceholder
    {
        /// <summary>
        /// The method used to compress the layer data.
        /// </summary>
        public CompressionType Compression { get; set; }

        /// <summary>
        /// The byte position in the PSD file at which the layer data starts.
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// The length of the layer data, as it appears in the PSD file (after compression, if any).
        /// </summary>
        public long DataLength { get; set; }
    }
}
