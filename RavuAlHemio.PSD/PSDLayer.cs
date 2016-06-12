using System.Collections.Generic;
using System.IO;

namespace RavuAlHemio.PSD
{
    public partial class PSDLayer
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Bottom { get; set; }
        public int Right { get; set; }
        public PSDLayerChannel[] Channels { get; set; }
        public BlendMode BlendMode { get; set; }
        public byte Opacity { get; set; }
        public bool NonBaseClipping { get; set; }
        public bool TransparencyProtected { get; set; }
        public bool Visible { get; set; }
        public bool Obsolete { get; set; }
        public bool PixelDataIrrelevantToDocumentAppearance { get; set; }
        public PSDLayerMask LayerMask { get; set; }
        public PSDLayerBlendingRange[] BlendingRanges { get; set; }
        public string Name { get; set; }
        public List<PSDAdditionalLayerInformation> AdditionalInformation { get; set; }
    }
}
