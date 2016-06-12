namespace RavuAlHemio.PSD
{
    public struct PSDLayerBlendingRange
    {
        public byte SourceLowFirst { get; set; }
        public byte SourceLowSecond { get; set; }
        public byte SourceHighFirst { get; set; }
        public byte SourceHighSecond { get; set; }
        public byte DestinationLowFirst { get; set; }
        public byte DestinationLowSecond { get; set; }
        public byte DestinationHighFirst { get; set; }
        public byte DestinationHighSecond { get; set; }
    }
}
