using System;
using System.IO;
using RavuAlHemio.PSD;

namespace PSDRead
{
    class Program
    {
        static void Main(string[] args)
        {
            var psd = new PSDFile();
            using (var readStream = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                psd.Read(readStream, loadData: false);
            }

            foreach (var res in psd.ImageResources)
            {
                Console.WriteLine("Image resource ID: {0}", res.ID);
                Console.WriteLine("  Name: {0}", res.Name);

                if (res.ID == ResolutionInfo.ResolutionInfoResourceID)
                {
                    Console.WriteLine("  Binary data:");

                    for (int rowOffset = 0; rowOffset < res.Data.Length; rowOffset += 8)
                    {
                        Console.Write("    ");
                        for (int i = 0; i < 8 && rowOffset + i < res.Data.Length; ++i)
                        {
                            Console.Write("{0:X2} ", res.Data[rowOffset + i]);
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine("  Resolution info:");
                    var resInfo = new ResolutionInfo(res);
                    Console.WriteLine("    Horizontal resolution: {0} dpi", resInfo.HorizontalResolutionDPI);
                    Console.WriteLine("      Display unit:        {0}", resInfo.HorizontalResolutionDisplayUnit);
                    Console.WriteLine("    Width display unit:    {0}", resInfo.WidthDisplayUnit);
                    Console.WriteLine("    Vertical resolution:   {0} dpi", resInfo.VerticalResolutionDPI);
                    Console.WriteLine("      Display unit:        {0}", resInfo.VerticalResolutionDisplayUnit);
                    Console.WriteLine("    Height display unit:   {0}", resInfo.HeightDisplayUnit);
                }
            }
        }
    }
}
