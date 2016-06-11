using System;
using System.IO;

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

        public ResolutionInfo()
        {
        }

        public ResolutionInfo(PSDImageResource resolutionResource)
        {
            if (resolutionResource.ID != ResolutionInfoResourceID)
            {
                throw new ArgumentException(
                    $"{nameof(resolutionResource.ID)} must be {ResolutionInfoResourceID}, not {resolutionResource.ID}",
                    nameof(resolutionResource)
                );
            }

            using (var ms = new MemoryStream(resolutionResource.Data, writable: false))
            {
                int horizontalResolutionDPIFixedPoint = ms.ReadBigEndianInt32();
                HorizontalResolutionDPI = horizontalResolutionDPIFixedPoint/FixedPointDivisor;
                if (HorizontalResolutionDPI <= 0.0)
                {
                    throw new PSDFormatException($"horizontal resolution is {HorizontalResolutionDPI}, expected more than 0.0");
                }

                HorizontalResolutionDisplayUnit = (ResolutionDisplayUnit) ms.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(ResolutionDisplayUnit), HorizontalResolutionDisplayUnit))
                {
                    throw new PSDFormatException(
                        $"horizontal resolution display unit is {HorizontalResolutionDisplayUnit}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<ResolutionDisplayUnit, short>())}"
                    );
                }

                WidthDisplayUnit = (SizeDisplayUnit) ms.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(SizeDisplayUnit), WidthDisplayUnit))
                {
                    throw new PSDFormatException(
                        $"width display unit is {WidthDisplayUnit}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<SizeDisplayUnit, short>())}"
                    );
                }

                int verticalResolutionDPIFixedPoint = ms.ReadBigEndianInt32();
                VerticalResolutionDPI = verticalResolutionDPIFixedPoint / FixedPointDivisor;
                if (VerticalResolutionDPI <= 0.0)
                {
                    throw new PSDFormatException($"vertical resolution is {VerticalResolutionDPI}, expected more than 0.0");
                }

                VerticalResolutionDisplayUnit = (ResolutionDisplayUnit)ms.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(ResolutionDisplayUnit), VerticalResolutionDisplayUnit))
                {
                    throw new PSDFormatException(
                        $"vertical resolution display unit is {VerticalResolutionDisplayUnit}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<ResolutionDisplayUnit, short>())}"
                    );
                }

                HeightDisplayUnit = (SizeDisplayUnit)ms.ReadBigEndianInt16();
                if (!Enum.IsDefined(typeof(SizeDisplayUnit), HeightDisplayUnit))
                {
                    throw new PSDFormatException(
                        $"height display unit is {HeightDisplayUnit}, expected one of {string.Join(", ", EnumUtils.GetUnderlyingValues<SizeDisplayUnit, short>())}"
                    );
                }
            }
        }
    }
}
