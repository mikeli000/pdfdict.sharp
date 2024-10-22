using PDFDict.SDK.Sharp.Tools;
using System.Drawing;

namespace TestNugtPackage
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string src = Path.Combine(Environment.CurrentDirectory, @"C:\dev\testfiles\pdfs\0-rendition.pdf");
            string dest = Path.Combine(Environment.CurrentDirectory, @"C:\dev\testfiles\pdfs\0-rendition-qr.pdf");
            string icon = Path.Combine(Environment.CurrentDirectory, @"C:\dev\pdfs\logo\powerpoint_icon.png");
            PDFWatermark.AddQRCode(src, "pdfdict add barcode sample", new Rectangle(100, 100, 100, 100), dest, icon);
            Console.WriteLine("Done");
        }
    }
}