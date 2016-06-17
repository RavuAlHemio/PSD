using System.IO;
using System.Linq;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// Information about the resolution of a PSD file, extractable from the corresponding image resource.
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// The ID of the image resource containing version information.
        /// </summary>
        public const short VersionInfoResourceID = 1057;

        /// <summary>
        /// The version.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Whether the precomposed data at the end of the PSD file is valid.
        /// </summary>
        /// The precomposed data is valid if the image only consists of a single background layer, or if the user has
        /// chosen to maximize compatibility while saving the PSD file.
        public bool HasValidPrecomposedData { get; set; }

        /// <summary>
        /// The name of the application writing the file.
        /// </summary>
        public string WriterName { get; set; }

        /// <summary>
        /// The name of the application reading the file.
        /// </summary>
        public string ReaderName { get; set; }

        /// <summary>
        /// The version of the file.
        /// </summary>
        public int FileVersion { get; set; }

        /// <summary>
        /// Reads version information from a PSD file object.
        /// </summary>
        /// <param name="psd">The PSD file object from which to obtain version information.</param>
        /// <returns>
        /// The version information obtained from the PSD file, or <c>null</c> if the file contains no version
        /// information.
        /// </returns>
        public static VersionInfo FromPSD(PSDFile psd)
        {
            var resource = psd.ImageResources.FirstOrDefault(ir => ir.ID == VersionInfoResourceID);
            if (resource == null)
            {
                return null;
            }

            var verInfo = new VersionInfo();
            using (var ms = new MemoryStream(resource.Data, writable: false))
            {
                verInfo.Version = ms.ReadBigEndianInt32();

                byte hasRealMergedData = ms.ReadByteOrThrow();
                if (hasRealMergedData > 1)
                {
                    throw new PSDFormatException($"got hasRealMergedData value {hasRealMergedData}, expected 0 or 1");
                }
                verInfo.HasValidPrecomposedData = (hasRealMergedData == 1);

                verInfo.WriterName = ms.ReadUnicodeString();
                verInfo.ReaderName = ms.ReadUnicodeString();
                verInfo.FileVersion = ms.ReadBigEndianInt32();
            }

            return verInfo;
        }
    }
}
