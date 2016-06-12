namespace RavuAlHemio.PSD
{
    public partial class PSDImageResource
    {
        internal const string Magic = "8BIM";

        public short ID { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }
}
