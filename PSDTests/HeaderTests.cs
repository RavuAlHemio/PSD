using System.IO;
using RavuAlHemio.PSD;
using Xunit;

namespace PSDTests
{
    public class HeaderTests
    {
        protected static byte[] GetValidHeader()
        {
            return new byte[]
            {
                // Magic
                0x38, 0x42, 0x50, 0x53,
                // Version 1
                0x00, 0x01,
                // 6 reserved bytes
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                // 4 channels
                0x00, 0x04,
                // height: 32 pixels
                0x00, 0x00, 0x00, 0x20,
                // width: 16 pixels
                0x00, 0x00, 0x00, 0x10,
                // 8-bit depth
                0x00, 0x08,
                // CMYK
                0x00, 0x04
            };
        }

        [Fact]
        public void TestValidHeader()
        {
            var image = new PSDFile();

            using (var validHeaderStream = new MemoryStream(GetValidHeader(), writable: false))
            {
                PSDFile.Reading.ReadHeader(image, validHeaderStream);
                Assert.Equal(1, image.Version);
                Assert.Equal(4, image.NumberOfChannels);
                Assert.Equal(32, image.Height);
                Assert.Equal(16, image.Width);
                Assert.Equal(8, image.Depth);
                Assert.Equal(ColorMode.CMYK, image.ColorMode);
            }
        }

        [Fact]
        public void TestInvalidMagic()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[0] = 0x46;
            header[1] = 0x55;
            header[2] = 0x43;
            header[3] = 0x4B;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }
        }

        [Fact]
        public void TestInvalidVersion()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[4] = 0x00;
            header[5] = 0x45;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }
        }

        [Fact]
        public void TestInvalidReservedValue()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[11] = 0x01;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }
        }

        [Fact]
        public void TestZeroChannels()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[13] = 0x00;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }
        }

        [Fact]
        public void TestTooManyChannels()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[13] = 0x39;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }
        }

        [Fact]
        public void TestTooHighForVersion1()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[14] = 0x00;
            header[15] = 0x00;
            header[16] = 0x75;
            header[17] = 0x31;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }

            var v2Image = new PSDFile();
            var v2Header = GetValidHeader();
            v2Header[5] = 0x02;
            v2Header[14] = 0x00;
            v2Header[15] = 0x00;
            v2Header[16] = 0x75;
            v2Header[17] = 0x31;
            using (var v2HeaderStream = new MemoryStream(v2Header, writable: false))
            {
                PSDFile.Reading.ReadHeader(v2Image, v2HeaderStream);
                Assert.Equal(2, v2Image.Version);
                Assert.Equal(4, v2Image.NumberOfChannels);
                Assert.Equal(30001, v2Image.Height);
                Assert.Equal(16, v2Image.Width);
                Assert.Equal(8, v2Image.Depth);
                Assert.Equal(ColorMode.CMYK, v2Image.ColorMode);
            }
        }

        [Fact]
        public void TestTooWideForVersion1()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[18] = 0x00;
            header[19] = 0x00;
            header[20] = 0x75;
            header[21] = 0x31;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }

            var v2Image = new PSDFile();
            var v2Header = GetValidHeader();
            v2Header[5] = 0x02;
            v2Header[18] = 0x00;
            v2Header[19] = 0x00;
            v2Header[20] = 0x75;
            v2Header[21] = 0x31;
            using (var v2HeaderStream = new MemoryStream(v2Header, writable: false))
            {
                PSDFile.Reading.ReadHeader(v2Image, v2HeaderStream);
                Assert.Equal(2, v2Image.Version);
                Assert.Equal(4, v2Image.NumberOfChannels);
                Assert.Equal(32, v2Image.Height);
                Assert.Equal(30001, v2Image.Width);
                Assert.Equal(8, v2Image.Depth);
                Assert.Equal(ColorMode.CMYK, v2Image.ColorMode);
            }
        }

        [Fact]
        public void TestTooHighForVersion2()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[5] = 0x02;
            header[14] = 0x00;
            header[15] = 0x04;
            header[16] = 0x93;
            header[17] = 0xE1;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }
        }

        [Fact]
        public void TestTooWideForVersion2()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[5] = 0x02;
            header[18] = 0x00;
            header[19] = 0x04;
            header[20] = 0x93;
            header[21] = 0xE1;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }
        }

        [Fact]
        public void TestWrongDepth()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[23] = 0x02;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }
        }

        [Fact]
        public void TestWrongColorMode()
        {
            var image = new PSDFile();
            var header = GetValidHeader();
            header[25] = 0x05;
            using (var headerStream = new MemoryStream(header, writable: false))
            {
                Assert.Throws<PSDFormatException>(() => PSDFile.Reading.ReadHeader(image, headerStream));
            }
        }
    }
}
