using PDFDict.SDK.Sharp.Core;

namespace PDFDict.SDK.Sharp.Tools
{
    /// <summary>
    /// Filling out PDF forms or extracting data from PDF forms.
    /// </summary>
    public partial class PDFTools
    {
        public static IDictionary<int, IList<PDFAnnotation>> ExtractAnnots(string pdfFile)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                var pageAnnots = new Dictionary<int, IList<PDFAnnotation>>();
                int pageCount = pdfDoc.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    var page = pdfDoc.LoadPage(i);
                    var annots = page.LoadAnnots();
                    pageAnnots.Add(i, annots);
                }

                return pageAnnots;
            }
        }

        public static void FillForm(string pdfFile, int pageIndex, string fieldName, string fieldValue, string dstFile)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                var pageAnnots = new Dictionary<int, IList<PDFAnnotation>>();
                int pageCount = pdfDoc.GetPageCount();

                var page = pdfDoc.LoadPage(pageIndex);
                if (page == null)
                {
                    throw new ArgumentOutOfRangeException("pageIndex", "Page index out of range");
                }

                var annots = page.LoadAnnots();
                for (int i = 0; i < annots.Count; i++)
                {
                    if (annots[i] is PDFWidget widget)
                    {
                        if (string.Equals(widget.FieldName, fieldName))
                        {
                            widget.SetFieldValue(fieldValue);
                        }
                    }
                }

                pdfDoc.Save(dstFile, true);
            }
        }
    }
}
