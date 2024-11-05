using PDFDict.SDK.Sharp.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

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

                    var pageTextChunks = page.GetTextChunks();
                    Console.WriteLine($"Page {i + 1} text chunks:");
                    foreach (var textChunk in pageTextChunks.TextChunks)
                    {
                        Console.WriteLine(textChunk);
                        page.DrawRect(textChunk.BBox, new GraphicsState { Stroke = true, Fill = false, StrokeColor = Color.Blue, StrokeWidth = 0.5f });
                    }
                }

                pdfDoc.Save("c:/temp/xxx.pdf", true);
            }
        }
    }
}
