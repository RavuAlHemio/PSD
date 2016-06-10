namespace RavuAlHemio.PSD
{
    /// <summary>
    /// The color mode of the image.
    /// </summary>
    public enum ColorMode : short
    {
        /// <summary>
        /// A bitmap image (0 or 1 for each pixel).
        /// </summary>
        Bitmap = 0,

        /// <summary>
        /// A grayscale image.
        /// </summary>
        Grayscale = 1,

        /// <summary>
        /// An index image (color palette and indices).
        /// </summary>
        Indexed = 2,

        /// <summary>
        /// An RGB image (red, green and blue components).
        /// </summary>
        RGB = 3,

        /// <summary>
        /// A CMYK image (cyan, magenta, yellow and black components).
        /// </summary>
        CMYK = 4,

        /// <summary>
        /// An image with a custom number of channels.
        /// </summary>
        Multichannel = 7,

        /// <summary>
        /// An image with two components (often black and a spot color).
        /// </summary>
        Duotone = 8,

        /// <summary>
        /// An image in the CIE Lab color space.
        /// </summary>
        Lab = 9
    }
}
