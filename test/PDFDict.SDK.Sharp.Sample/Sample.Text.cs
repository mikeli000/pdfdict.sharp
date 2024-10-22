using System.Diagnostics;
using PDFDict.SDK.Sharp.Core;

namespace PDFDict.SDK.Sharp.Sample
{
    internal partial class Sample
    {
        public static void ExtractText()
        {
            string pdfPath = Path.Combine(Environment.CurrentDirectory, @"files\pdfua\便携式文档格式.pdf");

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfPath))
            {
                int pageCount = pdfDoc.GetPageCount();
                Debug.WriteLine($"Page count: {pageCount}");

                for (int i = 0; i < pageCount; i++)
                {
                    var pdfPage = pdfDoc.LoadPage(i);
                    Debug.WriteLine($"--- Page {i + 1} text: \n {pdfPage.GetText()}");
                }
            }
        }
    }
}
