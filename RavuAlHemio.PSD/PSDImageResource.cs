namespace RavuAlHemio.PSD
{
    /// <summary>
    /// An image resource in a PSD file.
    /// </summary>
    public partial class PSDImageResource
    {
        /// <summary>
        /// The magic string used to identify a PSD image resource.
        /// </summary>
        internal const string Magic = "8BIM";

        /// <summary>
        /// The ID of the image resource.
        /// </summary>
        public short ID { get; set; }

        /// <summary>
        /// The name of the image resource. Very often an empty string.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The data of the image resource.
        /// </summary>
        public byte[] Data { get; set; }
    }
}
