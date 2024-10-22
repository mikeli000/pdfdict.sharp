using System.Drawing;
using PDFDict.SDK.Sharp.Core;
using QRCoder;

namespace PDFDict.SDK.Sharp.Tools
{
    public partial class PDFTools
    {
        public static void AddQRCode(string srcPdf, string text, Rectangle imageBox, string outputPdf, string iconImagePath = null, IEnumerable<int> pages = null)
        {
            using (var doc = PDFDocument.Load(srcPdf))
            {
                Bitmap? iconImage = null;
                if (!string.IsNullOrEmpty(iconImagePath) && File.Exists(iconImagePath))
                {
                    using var img = new Bitmap(Image.FromFile(iconImagePath));
                    iconImage = new Bitmap(img);
                }
                
                var barcode = GenerateBarcode(text, iconImage);
                var pdfImage = PDFImage.Create(barcode);

                int pageCount = doc.GetPageCount();
                if (pages == null || !pages.Any())
                {
                    pages = new List<int>() { pageCount - 1};
                }
                else
                {
                    pages = pages.Where(val => val < pageCount).ToArray();
                }

                for (int i = 0; i < pages.Count(); i++)
                {
                    doc.LoadPage(pages.ElementAt(i)).AddImage(pdfImage, imageBox, ImageFitting.AutoFit);
                }

                doc.Save(outputPdf, true);
            }
        }

        private static Bitmap GenerateBarcode(string text, Bitmap? iconImage = null)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(10, Color.DarkGoldenrod, Color.White, iconImage);

            return qrCodeImage;
        }
    }
}
