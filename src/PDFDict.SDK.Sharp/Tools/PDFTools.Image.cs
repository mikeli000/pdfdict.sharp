using PDFDict.SDK.Sharp.Core;
using PDFDict.SDK.Sharp.Core.OCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Tools
{
    public partial class PDFTools
    {
        public static void ExtractImages(string pdfFile, string imgFormat, string outputDir)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            if (string.IsNullOrEmpty(imgFormat))
            {
                imgFormat = "png";
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                int pageCount = pdfDoc.GetPageCount();
                Console.WriteLine($"Page count: {pageCount}");

                string img = Path.Combine(outputDir, "page0.jpg");
                pdfDoc.RenderPage(img, 0, 300f, null, 0);
                var res = TesseractOCR.ExtractText(img);
                Console.WriteLine($"OCR result: {res.Text}");
                Console.WriteLine($"OCR Confidence: {res.Confidence}");



                //for (int i = 0; i < pageCount; i++)
                //{
                //    var pdfPage = pdfDoc.LoadPage(i);


                //    var images = pdfPage.GetImages();
                //    for (int j = 0; j < images.Length; j++)
                //    {
                //        using (images[j])
                //        {
                //            Console.WriteLine($"--- Page {i + 1} image {j + 1}: {images[j].GetWidth()}x{images[j].GetHeight()}");
                //            string imagePath = Path.Combine(outputDir, @$"page{i + 1}_image{j + 1}.{imgFormat}");
                //            images[j].Save(imagePath);

                //            var res = TesseractOCR.ExtractText(imagePath);
                //            Console.WriteLine($"--- Page {i + 1} image {j + 1} OCR result: {res.Text}");
                //            Console.WriteLine($"--- Page {i + 1} image {j + 1} OCR confidence: {res.Confidence}");
                //        }
                //    }
                //    Console.WriteLine($"--- Page {i + 1} image count: {images.Length}");
                //}
            }
        }


    }
}
