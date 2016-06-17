namespace RavuAlHemio.PSD
{
    /// <summary>
    /// A constituent channel of a layer in a PSD file.
    /// </summary>
    public class PSDLayerChannel
    {
        /// <summary>
        /// The ID of this channel.
        /// </summary>
        public short ID { get; set; }

        /// <summary>
        /// The length of the data of the channel. This property is only a mule; the value is eventually stored in
        /// <see cref="PSDLayerChannelDataPlaceholder.DataLength"/> in <see cref="Data"/>.
        /// </summary>
        internal long DataLength { get; set; }

        /// <summary>
        /// A placeholder for the data of this channel.
        /// </summary>
        public PSDLayerChannelDataPlaceholder Data { get; set; }
    }
}
