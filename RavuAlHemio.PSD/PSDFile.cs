using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RavuAlHemio.PSD
{
    public partial class PSDFile
    {
        internal const short MinChannels = 1;
        internal const short MaxChannels = 56;
        internal const int Version1MaxDimension = 30000;
        internal const int Version2MaxDimension = 300000;

        public short Version { get; set; }
        public short NumberOfChannels { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public short Depth { get; set; }
        public ColorMode ColorMode { get; set; }
        public byte[] ColorModeData { get; set; }
        public List<PSDImageResource> ImageResources { get; set; }
        public PSDLayer[] Layers { get; set; }
        public PSDGlobalLayerMask GlobalLayerMask { get; set; }
        public List<PSDAdditionalLayerInformation> AdditionalLayerInformation { get; set; }
        public PSDImageDataPlaceholder PrecomposedImageData { get; set; }
        
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
