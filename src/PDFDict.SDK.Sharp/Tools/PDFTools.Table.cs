using PDFDict.SDK.Sharp.Core;
using System.Drawing;

namespace PDFDict.SDK.Sharp.Tools
{
    public partial class PDFTools
    {
        public static void ExtractTable(string pdfFile)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    var page = pdfDoc.LoadPage(i);

                    var pageThread = page.BuildPageThread();
                    foreach (var textEle in pageThread.GetContentList())
                    {
                        Console.WriteLine(textEle);
                        page.DrawRect(textEle.BBox, new DrawingParams { Stroke = true, Fill = false, StrokeColor = Color.Blue, StrokeWidth = 0.5f });

                        var gs = new DrawingParams { Stroke = true, Fill = false, StrokeColor = Color.Red, StrokeWidth = 0.5f };
                        //page.BeginGraphics(gs);
                        //page.BeginPath(0, 0);
                        //page.MoveTo((float)((TextElement)textEle).GetBaselineX(), (float)((TextElement)textEle).GetBaselineY());
                        //page.LineTo((float)((TextElement)textEle).GetBaselineX() + textEle.BBox.Width, (float)((TextElement)textEle).GetBaselineY());
                        //page.LineTo((float)((TextElement)textEle).GetBaselineX(), (float)((TextElement)textEle).GetBaselineY() + 25);
                        //page.ClosePath();
                        //page.EndGraphics();
                    }

                    // page.RemoveLinessObjects();
                }

                pdfDoc.Save("c:/temp/zzz.pdf", true);
            }
        }

        public static void TestLine(string pdfFile)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();
                var page = pdfDoc.LoadPage(0);
                //page.RemoveLinessObjects();
                page.Flatten();

                pdfDoc.Save("c:/temp/zzz.pdf", true);
            }
        }

        
    }
}
