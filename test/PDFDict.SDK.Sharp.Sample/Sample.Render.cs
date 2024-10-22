using System.Drawing;
using PDFDict.SDK.Sharp.Core;

namespace PDFDict.SDK.Sharp.Sample
{
    internal partial class Sample
    {
        public static void RenderPDF()
        {
            string pdfPath = Path.Combine(Environment.CurrentDirectory, @"files\pdfua\AcroForm_ToggleButton_Sample.pdf");
            string outputFolder = Path.Combine(Environment.CurrentDirectory, @"files\pdfua");

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfPath))
            {
                int pageCount = pdfDoc.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    string pageImagePath = Path.Combine(outputFolder, @$"page{i + 1}.png");
                    pdfDoc.RenderPage(pageImagePath, i, backgroundColor: Color.White);
                }
            }
        }

        public static void RenderPDF_Gray()
        {
            string pdfPath = Path.Combine(Environment.CurrentDirectory, @"files\pdfua\AcroForm_ToggleButton_Sample.pdf");
            string outputFolder = Path.Combine(Environment.CurrentDirectory, @"files\pdfua");

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfPath))
            {
                int pageCount = pdfDoc.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    string pageImagePath = Path.Combine(outputFolder, @$"page{i + 1}.png");
                    pdfDoc.RenderPage(pageImagePath, i, renderFlag: RenderFlag.FPDF_GRAYSCALE, resolution: 600);
                }
            }
        }
    }
}
