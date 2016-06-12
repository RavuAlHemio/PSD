namespace RavuAlHemio.PSD
{
    public class PSDLayerChannel
    {
        public short ID { get; set; }
        internal long DataLength { get; set; }
        public PSDLayerChannelDataPlaceholder Data { get; set; }
    }
}
