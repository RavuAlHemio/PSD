namespace RavuAlHemio.PSD
{
    public partial class PSDLayerMask
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Bottom { get; set; }
        public int Right { get; set; }
        public byte DefaultColor { get; set; }
        public bool PositionIsRelativeToLayerData { get; set; }
        public bool Disabled { get; set; }
        public bool InvertMaskWhenBlending { get; set; }
        public bool OriginatesFromRenderingOtherData { get; set; }
        public byte? UserMaskDensity { get; set; }
        public double? UserMaskFeather { get; set; }
        public byte? VectorMaskDensity { get; set; }
        public double? VectorMaskFeather { get; set; }
    }
}
