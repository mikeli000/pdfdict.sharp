using OpenCvSharp;
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
        public static void ExtractImages(string pdfFile, string format, string outputDir)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            if (string.IsNullOrEmpty(format))
            {
                format = "png";
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            using PDFDocument pdfDoc = PDFDocument.Load(pdfFile);
            int pageCount = pdfDoc.GetPageCount();

            for (int i = 0; i < pageCount; i++)
            {
                var pdfPage = pdfDoc.LoadPage(i);
                var images = pdfPage.GetImages();
                for (int j = 0; j < images.Length; j++)
                {
                    using (images[j])
                    {
                        string imagePath = Path.Combine(outputDir, @$"page{i + 1}_image{j + 1}.{format}");
                        images[j].Save(imagePath);
                    }
                }
            }
        }

        public static IList<PageOCRResult> OCRPageImages(string pdfFile, string workDir)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            string format = "jpg";
            using PDFDocument pdfDoc = PDFDocument.Load(pdfFile);
            int pageCount = pdfDoc.GetPageCount();
            var ret = new List<PageOCRResult>();

            for (int i = 0; i < pageCount; i++)
            {

                var pdfPage = pdfDoc.LoadPage(i);
                var images = pdfPage.GetImages();
                if (images.Length == 0)
                {
                    continue;
                }

                var pageRes = new PageOCRResult
                {
                    PageIndex = i,
                    ImageOCRResults = new List<ImageOCRResult>()
                };
                
                for (int j = 0; j < images.Length; j++)
                {
                    using (images[j])
                    {
                        string imagePath = Path.Combine(workDir, @$"page_{i + 1}_image{j + 1}.{format}");
                        images[j].Save(imagePath);

                        var res = TesseractOCR.OCRText(imagePath);
                        pageRes.ImageOCRResults.Add(new ImageOCRResult
                        {
                            ImagePath = imagePath,
                            OCRText = res.Text,
                            OCRConfidence = res.Confidence
                        });
                    }
                }
                ret.Add(pageRes);
            }

            return ret;
        }

        public static IDictionary<int, ImageOCRResult> OCRPages(string pdfFile, string workDir)
        {
            if (!File.Exists(pdfFile))
            {
                throw new FileNotFoundException("PDF file not found", pdfFile);
            }

            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            float dpi = 300f;
            string format = "jpg";
            using (PDFDocument pdfDoc = PDFDocument.Load(pdfFile))
            {
                var ret = new Dictionary<int, ImageOCRResult>();
                int pageCount = pdfDoc.GetPageCount();
                
                for (int i = 0; i < pageCount; i++)
                {
                    string img = Path.Combine(workDir, $"page_{i}.{format}");
                    pdfDoc.RenderPage(img, i, dpi, null, 0);
                    var ocrResult = TesseractOCR.OCRText(img);
                    ret.Add(i, new ImageOCRResult
                    {
                        ImagePath = img,
                        OCRText = ocrResult.Text,
                        OCRConfidence = ocrResult.Confidence
                    });
                }
                return ret;
            }
        }

        public class PageOCRResult
        {
            public int PageIndex { get; set; }
            public List<ImageOCRResult> ImageOCRResults { get; set; }
        }

        public class ImageOCRResult
        {
            public string ImagePath { get; set; }
            public string OCRText { get; set; }
            public float OCRConfidence { get; set; }
        }
    }
}
