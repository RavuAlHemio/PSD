using System.Collections.Generic;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// A layer in a PSD image.
    /// </summary>
    public partial class PSDLayer
    {
        /// <summary>
        /// The difference in pixels between the top edge of the layer and the top edge of the image.
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// The difference in pixels between the left edge of the layer and the left edge of the image.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// The difference in pixels between the bottom edge of the layer and the bottom edge of the image.
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// The difference in pixels between the right edge of the layer and the right edge of the image.
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// The constituent channels of this layer.
        /// </summary>
        public PSDLayerChannel[] Channels { get; set; }

        /// <summary>
        /// Specifies how this layer is to be combined with the layer below it.
        /// </summary>
        public BlendMode BlendMode { get; set; }

        /// <summary>
        /// The opacity of this layer, a value between 0 (fully transparent) and 255 (fully opaque).
        /// </summary>
        public byte Opacity { get; set; }

        /// <summary>
        /// Whether this layer has non-base clipping.
        /// </summary>
        public bool NonBaseClipping { get; set; }

        /// <summary>
        /// Whether the transparency in this layer is protected.
        /// </summary>
        public bool TransparencyProtected { get; set; }

        /// <summary>
        /// Whether the layer is currently visible.
        /// </summary>
        public bool Visible { get; set; }
        
        public bool Obsolete { get; set; }

        /// <summary>
        /// Whether the pixel data stored in this layer is irrelevant to the appearance of the document.
        /// </summary>
        public bool PixelDataIrrelevantToDocumentAppearance { get; set; }

        /// <summary>
        /// Information about the mask applied to this layer.
        /// </summary>
        public PSDLayerMask LayerMask { get; set; }

        /// <summary>
        /// The blending ranges of this layer.
        /// </summary>
        public PSDLayerBlendingRange[] BlendingRanges { get; set; }

        /// <summary>
        /// The name of this layer.
        /// </summary>
        /// <remarks>
        /// It is undefined which encoding is used to store the layer name; the Unicode layer name should be extracted
        /// from the additional layer information instead.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Additional information about the layer.
        /// </summary>
        public List<PSDAdditionalLayerInformation> AdditionalInformation { get; set; }
    }
}
