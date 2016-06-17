namespace RavuAlHemio.PSD
{
    public class PSDLayerMask
    {
        /// <summary>
         /// The top offset in pixels, as a difference between the top edge of the layer and the top edge of the image.
         /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// The left offset in pixels, as a difference between the left edge of the layer and the left edge of the
        /// image.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// The bottom offset in pixels, as a difference between the bottom edge of the layer and the bottom edge of
        /// the image.
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// The right offset in pixels, as a difference between the right edge of the layer and the right edge of the
        /// image.
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// The default color of the layer mask, either 0 or 255.
        /// </summary>
        public byte DefaultColor { get; set; }

        /// <summary>
        /// Whether the position of the mask is relative to the position of the layer.
        /// </summary>
        public bool PositionIsRelativeToLayerData { get; set; }

        /// <summary>
        /// Whether the layer mask is currently disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Whether to invert the mask when blending. Should be <c>false</c> in most newer PSD files.
        /// </summary>
        public bool InvertMaskWhenBlending { get; set; }

        /// <summary>
        /// Whether the user mask actually originates from rendering other data.
        /// </summary>
        public bool OriginatesFromRenderingOtherData { get; set; }

        /// <summary>
        /// The density of the user mask, or <c>null</c> if not specified.
        /// </summary>
        public byte? UserMaskDensity { get; set; }

        /// <summary>
        /// The feather of the user mask, or <c>null</c> if not specified.
        /// </summary>
        public double? UserMaskFeather { get; set; }

        /// <summary>
        /// The density of the vector mask, or <c>null</c> if not specified.
        /// </summary>
        public byte? VectorMaskDensity { get; set; }

        /// <summary>
        /// The feather of the vector mask, or <c>null</c> if not specified.
        /// </summary>
        public double? VectorMaskFeather { get; set; }
    }
}
