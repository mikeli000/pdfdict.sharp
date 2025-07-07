using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core.Contents
{
    public class ImageElement : PageElement
    {
        public PDFImage PDFImage { get; private set; }

        public ImageElement(PDFImage pdfImage, RectangleF bbox): base(ElementType.Image, bbox)
        {
            PDFImage = pdfImage;
        }

        public override string ToString()
        {
            return $"Image: {PDFImage}, BBox: {BBox}";
        }

        public override bool TryBuildHTMLPiece(out string html)
        {
            html = null;
            return false;
        }
    }
}
