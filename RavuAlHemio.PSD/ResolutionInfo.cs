using System;
using System.IO;
using System.Linq;

namespace RavuAlHemio.PSD
{
    public class ResolutionInfo
    {
        public const short ResolutionInfoResourceID = 1005;
        internal const double FixedPointDivisor = 0x10000;

        public enum ResolutionDisplayUnit : short
        {
            PixelsPerInch = 1,
            PixelsPerCentimeter = 2
        }

        public enum SizeDisplayUnit : short
        {
            Inches = 1,
            Centimeters = 2,
            Points = 3,
            Picas = 4,
            Columns = 5
        }

        public double HorizontalResolutionDPI { get; set; }
        public ResolutionDisplayUnit HorizontalResolutionDisplayUnit { get; set; }
        public SizeDisplayUnit WidthDisplayUnit { get; set; }
        public double VerticalResolutionDPI { get; set; }
        public ResolutionDisplayUnit VerticalResolutionDisplayUnit { get; set; }
        public SizeDisplayUnit HeightDisplayUnit { get; set; }

        public static ResolutionInfo FromPSD(PSDFile psd)
        {
            var resource = psd.ImageResources.FirstOrDefault(ir => ir.ID == ResolutionInfoResourceID);
            if (resource == null)
            {
                return null;
            }

            var resInfo = new ResolutionInfo();
            using (var ms = new MemoryStream(resource.Data, writable: false))
            {
                int horizontalResolutionDPIFixedPoint = ms.ReadBigEndianInt32();
                resInfo.HorizontalResolutionDPI = horizontalResolutionDPIFixedPoint / FixedPointDivisor;
                if (resInfo.HorizontalResolutionDPI <= 0.0)
                {
                    throw new PSDFormatException($"horizontal resolution is {resInfo.HorizontalResolutionDPI}, expected more than 0.0");
                }

                resInfo.HorizontalResolutionDisplayUnit = (ResolutionDisplayUnit)ms.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(ResolutionDisplayUnit), resInfo.HorizontalResolutionDisplayUnit))
                {
                    throw new PSDFormatException(
                        $"horizontal resolution display unit is {resInfo.HorizontalResolutionDisplayUnit}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<ResolutionDisplayUnit, short>())}"
                    );
                }

                resInfo.WidthDisplayUnit = (SizeDisplayUnit)ms.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(SizeDisplayUnit), resInfo.WidthDisplayUnit))
                {
                    throw new PSDFormatException(
                        $"width display unit is {resInfo.WidthDisplayUnit}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<SizeDisplayUnit, short>())}"
                    );
                }

                int verticalResolutionDPIFixedPoint = ms.ReadBigEndianInt32();
                resInfo.VerticalResolutionDPI = verticalResolutionDPIFixedPoint / FixedPointDivisor;
                if (resInfo.VerticalResolutionDPI <= 0.0)
                {
                    throw new PSDFormatException($"vertical resolution is {resInfo.VerticalResolutionDPI}, expected more than 0.0");
                }

                resInfo.VerticalResolutionDisplayUnit = (ResolutionDisplayUnit)ms.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(ResolutionDisplayUnit), resInfo.VerticalResolutionDisplayUnit))
                {
                    throw new PSDFormatException(
                        $"vertical resolution display unit is {resInfo.VerticalResolutionDisplayUnit}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<ResolutionDisplayUnit, short>())}"
                    );
                }

                resInfo.HeightDisplayUnit = (SizeDisplayUnit)ms.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(SizeDisplayUnit), resInfo.HeightDisplayUnit))
                {
                    throw new PSDFormatException(
                        $"height display unit is {resInfo.HeightDisplayUnit}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<SizeDisplayUnit, short>())}"
                    );
                }
            }

            return resInfo;
        }
    }
}
