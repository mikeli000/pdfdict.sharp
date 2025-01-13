using PDFDict.SDK.Sharp.Core;

namespace PDFDict.SDK.Sharp.Sample
{
    internal partial class Sample
    {
        public static void TestCV(string pdfFile)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            // https://medium.com/@rajashekarganiger2002/detect-and-extract-table-data-using-opencv-3039df2b80b0

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();
                var page = pdfDoc.LoadPage(0);

                string tempFile = "test1.png";
                pdfDoc.RenderPage(tempFile, 0, 72f, null, RenderFlag.FPDF_GRAYSCALE);

            }
        }
    }
}
