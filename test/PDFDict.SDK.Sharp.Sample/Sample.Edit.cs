using System.Drawing;
using PDFDict.SDK.Sharp.Core;

namespace PDFDict.SDK.Sharp.Sample
{
    internal partial class Sample
    {
        public static void DrawText()
        {
            string pdfPath = Path.Combine(Environment.CurrentDirectory, @"files\pdf\zh_cn.pdf");
            using (var doc = PDFDocument.Load(pdfPath))
            {
                var cover = doc.LoadPage(0);
                cover.AddText("CONFIDENTIAL", 100, 500, "Arial", 32, Color.Red);

                var page = doc.AppendPage(595, 842);
                string imgPath = Path.Combine(Environment.CurrentDirectory, @"files\img\我是老虎.jpg");
                var pdfImg = PDFImage.Create(imgPath);
                var imageBox = new Rectangle(100, 50, 500, 300);
                page.AddImage(pdfImg, imageBox, ImageFitting.FitWidth);

                string fontPath = @"C:\dev\pdfbox\3293\simkai.ttf";
                doc.AddFont(fontPath, PDFFont.FPDF_FONT_TRUETYPE);
                page.AddText("CONFIDENTIAL 草稿", 100, 500, "KaiTi", 18, Color.Red);

                string output = Path.Combine(Environment.CurrentDirectory, @"files\pdf\test.pdf");
                doc.Save(Path.Combine(Environment.CurrentDirectory, output), true);
                Console.WriteLine($"PDF file saved to {output}");
            }
        }
    }
}
