using System.Diagnostics;
using System.Drawing;
using PDFDict.SDK.Sharp.Core;

namespace PDFDict.SDK.Sharp.Sample
{
    internal partial class Sample
    {
        public static void CreatePDF()
        {
            using (var doc = PDFDocument.Create())
            {
                var page = doc.AppendPage(595, 842);

                string fontDir = Path.Combine(Environment.CurrentDirectory, @"files\fonts");
                string imgPath = Path.Combine(Environment.CurrentDirectory, @"files\img\Coders at work.jpg");
                var pdfImg = PDFImage.Create(imgPath);

                var imageBox = new Rectangle(100, 50, 400, 300);
                page.AddImage(pdfImg, imageBox, ImageFitting.AutoFit);

                string fontPath = Path.Combine(fontDir, "simkai.ttf");
                doc.AddFont(fontPath, PDFFont.FPDF_FONT_TRUETYPE);
                page.AddText("Hello 汉字\0 world", 100, 500, "KaiTi", 18, Color.DarkCyan);

                DrawingParams gState = new DrawingParams
                {
                    StrokeWidth = 2,
                    StrokeColor = Color.DodgerBlue,
                    FillColor = Color.FromArgb(100, Color.LightGoldenrodYellow)
                };
                page.DrawRect(new Rectangle(100, 500, 200, 33), gState);

                string pdfPath = Path.Combine(Environment.CurrentDirectory, @"files\img\test.pdf");
                doc.Save(Path.Combine(Environment.CurrentDirectory, pdfPath), true);
            }
        }

        public static void CreatePDF_Graphics()
        {
            using (var doc = PDFDocument.Create())
            {
                var page = doc.AppendPage(595, 842);

                string fontDir = Path.Combine(Environment.CurrentDirectory, @"files\fonts");
                string imgPath = Path.Combine(Environment.CurrentDirectory, @"files\img\Coders at work.jpg");
                var pdfImg = PDFImage.Create(imgPath);

                var imageBox = new Rectangle(100, 50, 400, 300);
                page.AddImage(pdfImg, imageBox, ImageFitting.AutoFit);

                string fontPath = Path.Combine(fontDir, "simkai.ttf");
                doc.AddFont(fontPath, PDFFont.FPDF_FONT_TRUETYPE);
                page.AddText("Hello 汉字\0 world", 100, 500, "KaiTi", 18, Color.DarkCyan);

                DrawingParams gState = new DrawingParams 
                {
                    StrokeWidth = 2,
                    StrokeColor = Color.DodgerBlue,
                    FillColor = Color.FromArgb(100, Color.LightGoldenrodYellow)
                };
                page.DrawRect(new Rectangle(100, 500, 200, 33), gState);

                gState.StrokeColor = Color.Brown;
                page.DrawRect(new Rectangle(200, 600, 200, 33), gState);

                page.BeginGraphics(gState);
                page.BeginPath(0, 0);
                page.MoveTo(300, 800);
                page.LineTo(400, 800);
                page.BezierTo(300, 700, 400, 700, 300, 800);
                page.ClosePath();
                page.EndGraphics();

                string pdfPath = Path.Combine(Environment.CurrentDirectory, @"files\img\test.pdf");
                doc.Save(Path.Combine(Environment.CurrentDirectory, pdfPath), true);
            }
        }

        public static void ReadDocumentProperties()
        {
            string pdfPath = Path.Combine(Environment.CurrentDirectory, @"files/pdfua/便携式文档格式.pdf");
            using (var doc = PDFDocument.Load(pdfPath))
            {
                var metadata = doc.GetDocumentProperties();

                Debug.WriteLine(metadata.Title);
                Debug.WriteLine("\n");
                Debug.WriteLine(metadata.Author);
                Debug.WriteLine("\n");
                Debug.WriteLine(metadata.Subject);
                Debug.WriteLine("\n");
                Debug.WriteLine(metadata.Keywords);
                Debug.WriteLine("\n");
                Debug.WriteLine(metadata.Creator);
                Debug.WriteLine("\n");
                Debug.WriteLine(metadata.Producer);
                Debug.WriteLine("\n");
                Debug.WriteLine(metadata.CreationDate);
                Debug.WriteLine("\n");
                Debug.WriteLine(metadata.ModDate);
                Debug.WriteLine("\n");

                Console.WriteLine(metadata.Title);
                Console.WriteLine("\n");
                Console.WriteLine(metadata.Author);
                Console.WriteLine("\n");
                Console.WriteLine(metadata.Subject);
                Console.WriteLine("\n");
                Console.WriteLine(metadata.Keywords);
                Console.WriteLine("\n");
                Console.WriteLine(metadata.Creator);
                Console.WriteLine("\n");
                Console.WriteLine(metadata.Producer);
                Console.WriteLine("\n");
                Console.WriteLine(metadata.CreationDate);
                Console.WriteLine("\n");
                Console.WriteLine(metadata.ModDate);
                Console.WriteLine("\n");
            }
        }
    }
}
