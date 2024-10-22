using PDFDict.SDK.Sharp.Core;
using PDFDict.SDK.Sharp.Tools;

namespace PDFDict.SDK.Sharp.Sample
{
    internal partial class Sample
    {
        public static void SplitPDF()
        {
            string pdfPath = Path.Combine(Environment.CurrentDirectory, @"files\pdf\BrightCarbon-Interactive-PDF.pdf");
            string outputFolder = Path.Combine(Environment.CurrentDirectory, @"files\pdf\split");
            var pageIndexList = new List<int[]> { new int[] { 0, 1 }, new int[] { 2, 3 } };
            PDFTools.SplitPDF(pdfPath, pageIndexList, outputFolder);
        }

        public static void MergePDF()
        {
            string pdf1 = Path.Combine(Environment.CurrentDirectory, @"files\pdfua\FlyerPDFUA-en2015.pdf");
            string pdf2 = Path.Combine(Environment.CurrentDirectory, @"files\pdfua\BrightCarbon-Interactive-PDF.pdf");

            using (var newDoc = PDFDocument.Create())
            {
                int pageIndex = 0;
                using (PDFDocument pdfDoc = PDFDocument.Load(pdf1))
                {
                    int pageCount = pdfDoc.GetPageCount();

                    for (int i = 0; i < pageCount; i++)
                    {
                        var srcPage = pdfDoc.LoadPage(i);
                        newDoc.ImportPages(pdfDoc, new int[] { i }, pageIndex++);
                    }
                }

                using (PDFDocument pdfDoc = PDFDocument.Load(pdf2))
                {
                    int pageCount = pdfDoc.GetPageCount();
                    int[] importedPages = new int[pageCount];
                    for (int i = 0; i < pageCount; i++)
                    {
                        importedPages[i] = i;
                    }
                    newDoc.ImportPages(pdfDoc, importedPages, 0);
                }

                string outputPath = Path.Combine(Environment.CurrentDirectory, @"files\pdfua\merged.pdf");
                newDoc.Save(outputPath, true);
            }
        }
    }
}
