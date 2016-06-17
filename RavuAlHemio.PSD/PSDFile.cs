using System;
using System.Collections.Generic;
using System.IO;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// A PSD file, the native file format of Adobe Photoshop.
    /// </summary>
    public partial class PSDFile
    {
        /// <summary>
        /// The minimum number of channels a PSD file must contain.
        /// </summary>
        public const short MinChannels = 1;

        /// <summary>
        /// The maximum number of channels a PSD file can contain.
        /// </summary>
        public const short MaxChannels = 56;

        /// <summary>
        /// The maximum value supported by PSD version 1 for image width and height.
        /// </summary>
        public const int Version1MaxDimension = 30000;

        /// <summary>
        /// The maximum value supported by PSD version 2 for image width and height.
        /// </summary>
        public const int Version2MaxDimension = 300000;

        /// <summary>
        /// The version of this PSD file. Version 2 doubles the size (and thereby the range) of some fields.
        /// </summary>
        public short Version { get; set; }

        /// <summary>
        /// The number of channels stored within this PSD file. A value between <see cref="MinChannels"/> and
        /// <see cref="MaxChannels"/>.
        /// </summary>
        public short NumberOfChannels { get; set; }

        /// <summary>
        /// The width of the image. A value between 1 and either <see cref="Version1MaxDimension"/> or
        /// <see cref="Version2MaxDimension"/>, depending on <see cref="Version"/>.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the image. A value between 1 and either <see cref="Version1MaxDimension"/> or
        /// <see cref="Version2MaxDimension"/>, depending on <see cref="Version"/>.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The depth of the image, the number of bits used to represent the value of one channel of one pixel.
        /// </summary>
        public short Depth { get; set; }

        /// <summary>
        /// The color mode of this image.
        /// </summary>
        public ColorMode ColorMode { get; set; }

        /// <summary>
        /// Additional data pertaining to the color mode (e.g. the palette for
        /// <see cref="RavuAlHemio.PSD.ColorMode.Indexed"/> images).
        /// </summary>
        public byte[] ColorModeData { get; set; }

        /// <summary>
        /// Image resources of the PSD file, primarily metadata.
        /// </summary>
        public List<PSDImageResource> ImageResources { get; set; }

        /// <summary>
        /// The layers of the image. May be <c>null</c> or an empty array if the image only consists of a background
        /// layer; in this case, the image data is stored in <see cref="PrecomposedImageData"/>.
        /// </summary>
        public PSDLayer[] Layers { get; set; }

        /// <summary>
        /// Information about the global layer mask, or <c>null</c> if the image has no global layer mask.
        /// </summary>
        public PSDGlobalLayerMask GlobalLayerMask { get; set; }

        /// <summary>
        /// Additional layer information, pertaining the precomposed image layer.
        /// </summary>
        public List<PSDAdditionalLayerInformation> AdditionalLayerInformation { get; set; }

        /// <summary>
        /// The precomposed image data. If <see cref="Layers"/> is empty or <c>null</c>, the image consists of only one
        /// background layer and <see cref="PrecomposedImageData"/> contains its data. If <see cref="Layers"/> is not
        /// empty, <see cref="PrecomposedImageData"/> either contains a precomposed version of the image (all layers
        /// blended together with all adjustments combined) or an empty image; whether this precomposed image is valid
        /// can be obtained from <see cref="VersionInfo.HasValidPrecomposedData"/>.
        /// </summary>
        public PSDImageDataPlaceholder PrecomposedImageData { get; set; }
        
        /// <summary>
        /// Reads a PSD image from a stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the PSD image. The stream must support seeking.</param>
        public void Read(Stream stream)
        {
            try
            {
                Reading.ReadHeader(this, stream);
                Reading.ReadColorModeData(this, stream);
                Reading.ReadImageResources(this, stream);
                Reading.ReadLayerAndMaskInformation(this, stream);
                Reading.CreateImageDataPlaceholder(this, stream);
            }
            catch (PSDFormatException)
            {
                throw;
            }
            catch (Exception exc)
            {
                throw new PSDFormatException("decoding failed", exc);
            }
        }
    }
}
