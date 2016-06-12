namespace RavuAlHemio.PSD
{
    public class PSDGlobalLayerMask
    {
        public short OverlayColorSpace { get; set; }
        public short ColorComponent1 { get; set; }
        public short ColorComponent2 { get; set; }
        public short ColorComponent3 { get; set; }
        public short ColorComponent4 { get; set; }
        public short Opacity { get; set; }
        public LayerMaskKind Kind { get; set; }
    }
}
