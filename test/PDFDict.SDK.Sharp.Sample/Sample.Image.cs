using PDFDict.SDK.Sharp.Core;

namespace PDFDict.SDK.Sharp.Sample
{
    internal partial class Sample
    {
        public static void ExtractImages()
        {
            string pdfPath = Path.Combine(Environment.CurrentDirectory, @"files\pdfua\FlyerPDFUA-en2015.pdf");

            using (PDFDocument pdfDoc = PDFDocument.Load(pdfPath))
            {
                int pageCount = pdfDoc.GetPageCount();
                Console.WriteLine($"Page count: {pageCount}");

                for (int i = 0; i < pageCount; i++)
                {
                    var pdfPage = pdfDoc.LoadPage(i);

                    var images = pdfPage.GetImages();
                    for (int j = 0; j < images.Length; j++)
                    {
                        using (images[j])
                        {
                            Console.WriteLine($"--- Page {i + 1} image {j + 1}: {images[j].GetWidth()}x{images[j].GetHeight()}");
                            string imagePath = Path.Combine(Environment.CurrentDirectory, @$"files\img\page{i + 1}_image{j + 1}.png");
                            images[j].Save(imagePath);
                        }
                    }
                    Console.WriteLine($"--- Page {i + 1} image count: {images.Length}");
                }
            }
        }
    }
}
