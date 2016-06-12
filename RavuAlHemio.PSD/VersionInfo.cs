using System.IO;
using System.Linq;

namespace RavuAlHemio.PSD
{
    public class VersionInfo
    {
        public const short VersionInfoResourceID = 1057;

        public int Version { get; set; }
        public bool HasValidPrecomposedData { get; set; }
        public string WriterName { get; set; }
        public string ReaderName { get; set; }
        public int FileVersion { get; set; }

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
