using System;
using System.IO;
using System.Linq;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// Information about the resolution of a PSD file, extractable from the corresponding image resource.
    /// </summary>
    public class ResolutionInfo
    {
        /// <summary>
        /// The ID of the image resource containing resolution information.
        /// </summary>
        public const short ResolutionInfoResourceID = 1005;

        /// <summary>
        /// The divisor used to convert from the fixed-point representation to dots per inch.
        /// </summary>
        internal const double FixedPointDivisor = 0x10000;

        /// <summary>
        /// The available display units for resolution.
        /// </summary>
        public enum ResolutionDisplayUnit : short
        {
            /// <summary>
            /// Pixels per inch (dpi, px/in).
            /// </summary>
            PixelsPerInch = 1,

            /// <summary>
            /// Pixels per centimeter (px/cm).
            /// </summary>
            PixelsPerCentimeter = 2
        }

        /// <summary>
        /// The available display units for width and height.
        /// </summary>
        public enum SizeDisplayUnit : short
        {
            /// <summary>
            /// Inches (in). 1in = 2.54cm
            /// </summary>
            Inches = 1,

            /// <summary>
            /// Centimeters (cm).
            /// </summary>
            Centimeters = 2,

            /// <summary>
            /// Points (pt). 1pt = 1/72in
            /// </summary>
            Points = 3,

            /// <summary>
            /// Picas (pc). 1pc = 12pt = 1/6in
            /// </summary>
            Picas = 4,

            /// <summary>
            /// Columns.
            /// </summary>
            Columns = 5
        }

        /// <summary>
        /// The horizontal resolution of the image, in pixels per inch.
        /// </summary>
        public double HorizontalResolutionDPI { get; set; }

        /// <summary>
        /// The unit in which the horizontal resolution should be displayed to the user.
        /// </summary>
        public ResolutionDisplayUnit HorizontalResolutionDisplayUnit { get; set; }

        /// <summary>
        /// The unit in which the image width should be displayed to the user.
        /// </summary>
        public SizeDisplayUnit WidthDisplayUnit { get; set; }

        /// <summary>
        /// The vertical resolution of the image, in pixels per inch.
        /// </summary>
        public double VerticalResolutionDPI { get; set; }

        /// <summary>
        /// The unit in which the vertical resolution should be displayed to the user.
        /// </summary>
        public ResolutionDisplayUnit VerticalResolutionDisplayUnit { get; set; }

        /// <summary>
        /// The unit in which the image height should be displayed to the user.
        /// </summary>
        public SizeDisplayUnit HeightDisplayUnit { get; set; }

        /// <summary>
        /// Reads resolution information from a PSD file object.
        /// </summary>
        /// <param name="psd">The PSD file object from which to obtain resolution information.</param>
        /// <returns>
        /// The resolution information obtained from the PSD file, or <c>null</c> if the file contains no resolution
        /// information.
        /// </returns>
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
