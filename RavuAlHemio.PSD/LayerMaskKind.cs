namespace RavuAlHemio.PSD
{
    /// <summary>
    /// The kind of layer mask. Almost universally <see cref="UsePerLayerValue"/>.
    /// </summary>
    public enum LayerMaskKind : byte
    {
        /// <summary>
        /// Color selected, i.e. inverted.
        /// </summary>
        ColorSelected = 0,

        /// <summary>
        /// Color protected.
        /// </summary>
        ColorProtected = 1,

        /// <summary>
        /// Use the values stored with each layer.
        /// </summary>
        UsePerLayerValue = 128
    }
}
