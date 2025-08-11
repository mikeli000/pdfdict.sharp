using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using PDFDict.SDK.Sharp.Core;
using PDFiumCore;

namespace PDFDict.SDK.Sharp.Tools
{
    public partial class PDFTools
    {
        public static void DrawTextBox(string pdfFile, string outputFolder, float resolution = 300f, Color? backgroundColor = null, int renderFlag = 0)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    string pageImagePath = Path.Combine(outputFolder, $"page-{i + 1}.png");
                    pdfDoc.RenderPage(pageImagePath, i, resolution, backgroundColor, renderFlag);

                    using (var pdfPage = pdfDoc.LoadPage(i))
                    {
                        DrawBox(pdfPage, pageImagePath);
                    }
                }
            }
        }

        static void DrawBox(PDFPage pdfPage, string img)
        {
            var thread = pdfPage.BuildPageThread();
            var eles = thread.GetTextThread().GetTextElements();

            float ratio = 300 / 72f;
            double ph = pdfPage.GetPageHeight() * ratio;
            using Mat src = Cv2.ImRead(img);
            foreach (var e in eles)
            {
                if (e.BBox == null || e.BBox.IsEmpty)
                {
                    continue;
                }

                int top = (int)Math.Round(ph - e.BBox.Top * ratio - e.BBox.Height * ratio);
                int bottom = (int)Math.Round(top + e.BBox.Height * ratio);
                int left = (int)Math.Round(e.BBox.Left * ratio);
                int right = (int)Math.Round(e.BBox.Right * ratio);

                Rect rect = Rect.FromLTRB(left, top, right, bottom);
                Cv2.Rectangle(src, rect, Scalar.Magenta, 1);
            }

            Cv2.ImWrite(img, src);
        }

        public static void Render(string pdfFile, string outputFolder, float resolution = 96f, Color? backgroundColor = null, int renderFlag = 0)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    string pageImagePath = Path.Combine(outputFolder, $"page-{i + 1}.png");
                    pdfDoc.RenderPage(pageImagePath, i, resolution, backgroundColor, renderFlag);
                }
            }
        }

        public static void RenderAsGray(string pdfFile, string outputFolder, float resolution = 96f)
        {
            Render(pdfFile, outputFolder, resolution, null, RenderFlag.FPDF_GRAYSCALE);
        }

        public static void SplitPDF(string pdfFile, IEnumerable<int[]> pageIndexList, string outputFolder)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            FileInfo fileInfo = new FileInfo(pdfFile);
            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();

                for (int i = 0; i < pageIndexList.Count(); i++)
                {
                    var pageIndeics = pageIndexList.ElementAt(i);
                    var importedPages = pageIndeics.Where(val => val < pageCount).ToArray();
                    if (importedPages.Length == 0)
                    {
                        continue;
                    }
                    using (var newDoc = PDFDocument.Create())
                    {
                        newDoc.ImportPages(pdfDoc, importedPages, 0);
                        string outputPath = Path.Combine(outputFolder, @$"{i}_{fileInfo.Name}");
                        newDoc.Save(outputPath, true);
                    }
                }
            }
        }

        public static void MergePDF(IDictionary<string, int[]> pdfFiles, string outputFile)
        {
            using (var newDoc = PDFDocument.Create())
            {
                int pageIndex = 0;
                foreach (var pdfFile in pdfFiles)
                {
                    var pdfPath = pdfFile.Key;
                    if (!File.Exists(pdfPath))
                    {
                        throw new FileNotFoundException("PDF file not found", pdfPath);
                    }

                    using (PDFDocument pdfDoc = PDFDocument.Load(pdfPath))
                    {
                        int pageCount = pdfDoc.GetPageCount();
                        var mergedPages = pdfFile.Value;
                        if (mergedPages == null)
                        {
                            mergedPages = Enumerable.Range(0, pageCount).ToArray();
                        }
                        else
                        {
                            mergedPages = mergedPages.Where(val => val < pageCount).ToArray();

                        }

                        newDoc.ImportPages(pdfDoc, mergedPages, pageIndex);
                        pageIndex += mergedPages.Length;
                    }
                }

                newDoc.Save(outputFile, true);
            }
        }

        public static IDictionary<int, string> ExtractText(string pdfFile)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            Dictionary<int, string> pageTexts = new Dictionary<int, string>();
            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    var pdfPage = pdfDoc.LoadPage(i);
                    pageTexts[i] = pdfPage.GetText();
                }

                pdfDoc.Save("C:/temp/xxx.pdf", true);
            }

            return pageTexts;
        }
    }
}
